[← S10](S10-multi-device-push.md) · [Reja va qoidalar](README.md) · [S12 →](S12-webrtc-media.md)

# Sprint 11 — Qo'ng'iroq 1: coturn, TURN credential, signaling

**Maqsad:** infratuzilma va signaling: coturn ishlaydi, klient credential oladi, offer/answer/ICE E2EE envelope ichida server orqali o'tadi. Media hali yo'q.

**Nega hozir:** WebRTC'ni ikkiga bo'lish — signaling (tanish narsa: envelope + WS) va media (notanish). Avval tanishini.

**O'rganiladi:** ICE/STUN/TURN nima, NAT turlari, TURN REST credential (HMAC), SDP tuzilishi, signaling state machine.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S11-01 | `docker-compose`ga coturn: `turnserver.conf` — `use-auth-secret`, `static-auth-secret`, `realm`, `listening-port 3478`, TLS 5349 (keyin) | `turnutils_uclient` bilan test o'tadi |
| S11-02 | `GET /calls/turn-credentials` → `{ urls: [stun:, turn:udp, turn:tcp], username: "expiry:deviceId", credential: base64(HMAC-SHA1(secret, username)), ttl }` | Test: credential coturn'da ishlaydi (integratsiya, ixtiyoriy) |
| S11-03 | `.proto`: `CallSignal { callId, kind: OFFER/ANSWER/ICE/HANGUP/BUSY/RINGING, sdp?, iceCandidate? }` — **ratchet ichida** (envelope_type=2, ichki tur) — server SDP'ni ko'rmaydi | Round-trip test |
| S11-04 | Signaling — WS orqali **navbatsiz** yo'l: `SendCallSignal(recipientDeviceId, envelope)` hub metodi — qabul qiluvchi offline bo'lsa `BUSY/UNREACHABLE` qaytadi (qo'ng'iroq navbatda turmaydi, lekin "o'tkazib yuborilgan qo'ng'iroq" xabari oddiy xabar sifatida navbatga tushadi) | Integratsiya test |
| S11-05 | Klient: `CallSession` state machine: `Idle → Calling → Ringing → Connected → Ended` + timeouts (30s javob yo'q → Ended) | Unit test: barcha o'tishlar |
| S11-06 | UI: chaqirish ekrani, kiruvchi qo'ng'iroq ekrani (qabul/rad), hozircha media o'rniga "Ulandi (signaling OK)" | Ikki qurilma orasida offer/answer/ICE almashinadi (media yo'q) |
| S11-07 | `docs/calls/signaling.md` — sequence diagram | Yozilgan |

## Demo
Ikki qurilma: chaqirish → jiringlash → qabul → "signaling OK", loglarda ICE candidate'lar; server SDP'ni ko'rmaydi (faqat ciphertext).

## Resurslar
- **WebRTC for the Curious (bepul kitob, majburiy 1–4 boblar):** https://webrtcforthecurious.com/
- coturn: https://github.com/coturn/coturn · konfiguratsiya: https://github.com/coturn/coturn/wiki/turnserver
- TURN REST API credential sxemasi: https://datatracker.ietf.org/doc/html/draft-uberti-behave-turn-rest-00
- ICE (RFC 8445, fon): https://www.rfc-editor.org/rfc/rfc8445
- Trickle ICE test sahifasi (STUN/TURN serveringni tekshirish): https://webrtc.github.io/samples/src/content/peerconnection/trickle-ice/

## Tuzoqlar
- coturn ommaviy IP'siz (faqat lokal tarmoq) haqiqiy NAT testini bermaydi — arzon VPS (1 ta) S11'da ol.
- Credential TTL 1 soat; qo'ng'iroq boshida yangisini ol, keshlama.
- Signaling'ni navbatga qo'yish — 2 soat oldingi "offer" ma'nosiz. Faqat online.

---

---
[← S10](S10-multi-device-push.md) · [Reja va qoidalar](README.md) · [S12 →](S12-webrtc-media.md)
