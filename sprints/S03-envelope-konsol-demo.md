[← S02](S02-double-ratchet.md) · [Reja va qoidalar](README.md) · [S04 →](S04-server-auth.md)

# Sprint 3 — Envelope formati, sessiya menejeri, serversiz demo

**Maqsad:** `Chittak.Protocol` "tashqariga" tayyor: envelope bayt formati, sessiya storage interfeysi, va **serversiz konsol demo** — ikki in-process "qurilma" bir-biriga yozadi.

**Nega hozir:** server yozishdan oldin protokol API'si "qulay"ligini bilish kerak. Konsol demo — birinchi ko'rinadigan natija (motivatsiya!).

**O'rganiladi:** binary format dizayni (versiya bayti, length-prefix), Protobuf yoki qo'lda `BinaryPrimitives`, interfeys orqali storage abstraksiyasi, "prekey message" vs "normal message" farqi.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S3-01 | ADR `0005-envelope-format.md`: Protobuf (`Google.Protobuf`) vs qo'lda binary. **Tavsiya: Protobuf** — o'rganish uchun ham foydali, forward-compatible | ADR |
| S3-02 | `Envelope` sxemasi: `version(1) ‖ type(1: prekey/normal/receipt) ‖ payload`. PreKey payload: `senderIdentityPub, ephemeralPub, signedPreKeyId, otkId?, ratchetHeader, ciphertext`. Normal: `ratchetHeader, ciphertext`. Receipt: `refClientMessageId, kind(delivered/read)` — receipt ham ratchet ichida shifrlanadi | `.proto` fayl + generatsiya + round-trip testlar |
| S3-03 | `ISessionStore` (load/save `RatchetState` by `(remoteDeviceId)`), `IIdentityStore`, `IPreKeyStore` (o'z signed/one-time prekey'larim maxfiy qismi) — **interfeyslar Protocol'da, implementatsiya klientda** | In-memory implementatsiya testlar uchun |
| S3-04 | `SessionCipher` (fasad): `EncryptAsync(remoteDeviceId, plaintext)` → sessiya yo'q bo'lsa bundle so'raydi (`IPreKeyBundleSource`), X3DH + PreKey envelope; bor bo'lsa Normal envelope | Test: birinchi xabar prekey, ikkinchisi normal |
| S3-05 | `SessionCipher.DecryptAsync(remoteDeviceId, envelope)` → PreKey bo'lsa X3DH respond + OTK maxfiy qismini **o'chirish**; Normal bo'lsa ratchet | Test: ishlatilgan OTK ikkinchi marta rad etiladi |
| S3-06 | Ikkala tomon bir vaqtda PreKey xabar yuborsa (poyga) — Signal usuli: ikkalasi ham qabul qiladi, sessiya "yangisi g'olib" | Test + `docs/crypto/session-race.md` |
| S3-07 | `samples/Chittak.ConsoleDemo`: ikkita in-memory qurilma, in-memory "server" (bundle + navbat), REPL: `alice> salom` → `bob: [decrypt] salom` | Ishlaydi, ekranda ciphertext hex ham ko'rsatiladi |
| S3-08 | Xatoliklar taksonomiyasi: `ChittakProtocolException` → `InvalidSignature`, `NoSession`, `DuplicateMessage`, `UntrustedIdentity`... | Barcha throw'lar shu turlardan |
| S3-09 | Public API'ni `Chittak.Protocol/README.md`da hujjatla — server va klient shu API'ni ishlatadi | Yozilgan |

## Demo
Konsolda ikki "telefon" gaplashadi; "server" qismidagi baytlar shifrlangan; Bob'ning OTK'si iste'mol bo'lgani ko'rinadi.

## Resurslar
- Protobuf C#: https://protobuf.dev/getting-started/csharptutorial/ · `Grpc.Tools` orqali generatsiya
- Signal'ning `.proto` fayllari (namuna): libsignal `rust/protocol/src/proto/`
- `System.Buffers.Binary.BinaryPrimitives` (qo'lda variant): https://learn.microsoft.com/dotnet/api/system.buffers.binary.binaryprimitives
- Sesame spec (sessiya poygasi va multi-device fikrlash uchun, S10'gacha 1 marta o'qib qo'y): https://signal.org/docs/specifications/sesame/

## Tuzoqlar
- Envelope'ga plaintext metadata (jo'natuvchi ismi, vaqt) qo'shma — hammasi ciphertext ichida.
- `version` baytini birinchi kundan qo'y — keyin migratsiya bo'lmaydi.

---

---
[← S02](S02-double-ratchet.md) · [Reja va qoidalar](README.md) · [S04 →](S04-server-auth.md)
