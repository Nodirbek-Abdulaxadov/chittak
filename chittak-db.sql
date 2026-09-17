-- =====================================================================
--  Chittak — SERVER database (relay + key directory).  PostgreSQL 13+.
--
--  CORE PRINCIPLE (the whole point of "Secure"):
--  the server stores ONLY:
--    - PUBLIC keys (identity, signed prekey, one-time prekeys)
--    - OPAQUE ciphertext queued for delivery (deleted on client ACK / TTL)
--    - minimal routing + auth metadata
--  The server NEVER stores: plaintext, private keys, Double-Ratchet /
--  session state (client-only), delivered message history, or the
--  user's contact list (lives on the client).
--
--  Model is PER-DEVICE (like Signal): one user may have several devices;
--  each device has its own identity key + prekeys + sessions.
-- =====================================================================

-- =====================================================================
--  ACCOUNTS  (phone-based)
-- =====================================================================
CREATE TABLE users (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    phone        text UNIQUE NOT NULL
                 CHECK (phone ~ '^\+[1-9][0-9]{6,14}$'),   -- E.164 only, normalized before insert
    phone_hash   text UNIQUE NOT NULL,   -- SHA-256(phone), used by contact discovery
    discoverable boolean NOT NULL DEFAULT true,  -- opt-out of being found by phone
    display_name text,                   -- optional, user-set (public-ish)
    created_at   timestamptz NOT NULL DEFAULT now()
);
-- Contact discovery: client sends SHA-256 hashes of its address book
-- (E.164 normalized); server answers which hashes exist AND are discoverable.
-- The server never stores the submitted list. NOTE: a phone-number hash is
-- brute-forceable (small keyspace) — this only stops casual logging, it is
-- NOT private contact discovery (see roadmap: SGX/OPRF-based discovery).
-- Rate-limit this endpoint per device (e.g. 1 request / min, <= 5000 hashes).

-- =====================================================================
--  DEVICES  (Signal is per-device)
-- =====================================================================
CREATE TABLE devices (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    registration_id int  NOT NULL,        -- Signal registration id (client-generated)
    name            text,                 -- "Nodirbek's Pixel"
    platform        text CHECK (platform IN ('android', 'ios')),
    push_token      text,                 -- for wake-up push (opaque to msg content)
    created_at      timestamptz NOT NULL DEFAULT now(),
    last_seen_at    timestamptz,
    UNIQUE (user_id, registration_id)
);
CREATE INDEX idx_devices_user ON devices (user_id);
-- Multi-device: a sender must encrypt separately for EVERY device of the
-- recipient AND for its own other devices (sync copies). `GET /users/{id}/devices`
-- lists device ids; bundles are fetched per device.

-- Public identity key per device (long-term)
CREATE TABLE device_identity_keys (
    device_id    uuid PRIMARY KEY REFERENCES devices(id) ON DELETE CASCADE,
    -- 64 bytes = Ed25519 public (32, for signatures) || X25519 public (32, for DH).
    -- Decision (ADR-0004): two keys instead of Signal's XEdDSA single key —
    -- NSec/BouncyCastle have no XEdDSA; two standard keys are simpler and safe.
    identity_key bytea NOT NULL CHECK (octet_length(identity_key) = 64),
    created_at   timestamptz NOT NULL DEFAULT now()
);

-- =====================================================================
--  PREKEYS  (X3DH bundle material — all PUBLIC)
-- =====================================================================

-- Signed prekey (rotated periodically). Keep current + previous: prekey
-- messages already in flight may still reference the previous key_id.
CREATE TABLE signed_prekeys (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    device_id  uuid NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
    key_id     int  NOT NULL,             -- client-assigned
    public_key bytea NOT NULL CHECK (octet_length(public_key) = 32),  -- X25519
    signature  bytea NOT NULL CHECK (octet_length(signature) = 64),   -- Ed25519 sig over public_key, by identity Ed25519 key
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (device_id, key_id)
);
CREATE INDEX idx_signed_prekeys_device ON signed_prekeys (device_id, created_at DESC);

-- One-time prekeys pool. CONSUMED (row deleted) when handed out for a handshake.
CREATE TABLE one_time_prekeys (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    device_id  uuid NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
    key_id     int  NOT NULL,
    public_key bytea NOT NULL CHECK (octet_length(public_key) = 32),  -- X25519
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (device_id, key_id)
);
CREATE INDEX idx_otk_device ON one_time_prekeys (device_id);
-- Handing out an OTK MUST be atomic — two concurrent bundle requests must
-- never receive the same key. Use exactly this shape (single statement):
--
--   DELETE FROM one_time_prekeys
--    WHERE id = (SELECT id FROM one_time_prekeys
--                 WHERE device_id = $1
--                 ORDER BY id
--                 FOR UPDATE SKIP LOCKED
--                 LIMIT 1)
--   RETURNING key_id, public_key;
--
-- Zero rows => bundle is returned WITHOUT an OTK (X3DH allows this).
-- `GET /keys/count` tells the client to top up when the pool runs low (< 20).

-- =====================================================================
--  MESSAGE QUEUE  (delivery of OPAQUE ciphertext)
--  The server CANNOT read `envelope`.
--  Delivery contract:
--    1. sender POSTs envelope with a client-generated `client_message_id`
--       (retry-safe: duplicate insert is rejected by the UNIQUE below,
--       server answers 200 as if accepted).
--    2. server pushes over SignalR (WebSocket, MessagePack) if recipient
--       is online, otherwise waits. (ADR-0006: SignalR, not raw WS.)
--    3. row is deleted ONLY after the recipient device ACKs the queue `id`
--       over WS — NOT when it is pushed. A dropped socket => re-delivered
--       on reconnect (client dedups by `client_message_id`).
--    4. unACKed rows past `expires_at` are purged by a background job.
-- =====================================================================
CREATE TABLE message_queue (
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    recipient_device_id uuid  NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
    sender_device_id    uuid,                -- routing only (nullable if sealed-sender later)
    client_message_id   uuid  NOT NULL,      -- idempotency key, generated by sender
    envelope            bytea NOT NULL,       -- OPAQUE E2EE ciphertext
    envelope_type       smallint NOT NULL
                        CHECK (envelope_type IN (1, 2, 3)),  -- 1=prekey(initial) 2=normal 3=receipt
    created_at          timestamptz NOT NULL DEFAULT now(),
    expires_at          timestamptz NOT NULL, -- TTL (e.g. now() + 30 days); purge job deletes expired
    UNIQUE (recipient_device_id, client_message_id)
);
CREATE INDEX idx_queue_recipient ON message_queue (recipient_device_id, id);
CREATE INDEX idx_queue_expiry    ON message_queue (expires_at);

-- =====================================================================
--  AUTH  (phone OTP + device sessions)
--  Rate limits (enforced in the API layer, counted via idx_phone_verif):
--    - max 3 OTP sends per phone per 10 min, max 10 per day
--    - per-IP limit on /auth/otp/send
--    - max 5 verify attempts per code (`attempts`), then the row is dead
-- =====================================================================
CREATE TABLE phone_verifications (
    id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    phone       text NOT NULL,
    code_hash   text NOT NULL,            -- HASHED OTP (never plaintext)
    expires_at  timestamptz NOT NULL,     -- short: 5 min
    attempts    int NOT NULL DEFAULT 0 CHECK (attempts <= 5),
    consumed_at timestamptz,              -- set on successful verify; a consumed code is never accepted again
    created_at  timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX idx_phone_verif ON phone_verifications (phone, created_at DESC);
-- Verify accepts a row only if: consumed_at IS NULL AND expires_at > now()
-- AND attempts < 5. Old rows are purged after 24h.

CREATE TABLE auth_sessions (
    id                 uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    device_id          uuid NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
    refresh_token_hash text NOT NULL,    -- HASHED; access tokens are short-lived JWTs (stateless)
    created_at         timestamptz NOT NULL DEFAULT now(),
    expires_at         timestamptz NOT NULL,
    revoked_at         timestamptz
);
CREATE INDEX idx_auth_device ON auth_sessions (device_id);

-- =====================================================================
--  CALLS
--  Signaling (offer/answer/ICE) is REAL-TIME over WebSocket and relayed,
--  NOT persisted. Media is P2P (WebRTC) + E2EE (DTLS-SRTP; the SDP
--  fingerprint is carried inside the E2EE signaling envelope so the
--  server cannot MITM the media key exchange).
--
--  NAT traversal: run coturn (STUN + TURN). Clients get short-lived TURN
--  credentials from `GET /calls/turn-credentials` (TURN REST API: HMAC
--  of expiry:deviceId with the shared secret, valid ~1 h). Nothing to
--  store in the DB. TURN relays only ciphertext (SRTP) — it sees no media.
--
--  Optional minimal call log below — consider OMITTING for maximum
--  metadata privacy.
-- =====================================================================
-- CREATE TABLE call_log (
--     id               bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
--     caller_device_id uuid, callee_device_id uuid,
--     started_at timestamptz, ended_at timestamptz, status text
-- );

-- =====================================================================
--  NOTE: NO `sessions` / `ratchet_state` table on purpose.
--  Double-Ratchet + session state live ONLY on the client device.
--  NOTE: NO `contacts` table on purpose — the address book stays on the
--  client; the server only answers discovery queries and forgets them.
--  NOTE: NO `groups` tables — v1 is 1:1 only (see roadmap in the
--  architecture doc: Sender Keys for groups).
-- =====================================================================
