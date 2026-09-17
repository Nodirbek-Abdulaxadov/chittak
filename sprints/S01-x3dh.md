[← S00](S00-skelet.md) · [Reja va qoidalar](README.md) · [S02 →](S02-double-ratchet.md)

# Sprint 1 — Kripto primitivlar va X3DH

**Maqsad:** `Chittak.Protocol` ichida X3DH: ikki tomon serversiz, faqat public bundle almashib, **bir xil shared secret** chiqaradi. Faqat testlar — UI/server yo'q.

**Nega hozir:** bu loyihaning yuragi. Bu ishlamasa — qolgani ma'nosiz. Va bu eng ko'p yangi bilim beradigan sprint.

**O'rganiladi:** X25519 (DH), Ed25519 (imzo), HKDF, "key agreement" tushunchasi, nima uchun 3–4 ta DH kerak (autentifikatsiya + forward secrecy), test-first ish uslubi.

## Qaror (qabul qilingan) — ADR `0004-identity-key-format.md`
Identity = **ikki kalit**: Ed25519 (imzo uchun) + X25519 (DH uchun). Public qismi 64 bayt (Ed25519 ‖ X25519) `device_identity_keys.identity_key`ga yoziladi (DB'da `CHECK octet_length = 64`). Sabab: Signal XEdDSA bilan bitta X25519 kalitni imzolaydi, NSec/BouncyCastle'da XEdDSA yo'q; ikkita standart kalit soddaroq va xavfsiz. S1-01 birinchi kuni shu ADR'ni yozib qo'y.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S1-01 | `Primitives/`: `X25519KeyPair`, `Ed25519KeyPair`, `Hkdf.Derive(ikm, salt, info, len)`, `Aead.Encrypt/Decrypt` (ChaCha20-Poly1305 yoki AES-256-GCM — ADR) — hammasi kutubxona ustidan **yupqa** wrapper | Har biri uchun test: RFC test vektorlari bilan (X25519: RFC 7748 §6.1, Ed25519: RFC 8032 §7.1, HKDF: RFC 5869 A.1) |
| S1-02 | `Keys/`: `IdentityKeyPair`, `SignedPreKey` (key_id, X25519 pair, Ed25519 imzo), `OneTimePreKey` (key_id, pair), `PreKeyBundle` (public DTO — server beradigan narsa) | Bundle serialize/deserialize round-trip testi |
| S1-03 | `SignedPreKey.Verify(bundle)` — imzo tekshiruvi | Buzilgan imzo → `false`; boshqa identity bilan imzolangan → `false` |
| S1-04 | `X3dh.Initiate(aliceIdentity, bobBundle)` → `{ sharedSecret, ephemeralPublic, usedOtkId }` — spec bo'yicha DH1..DH4, `KDF(F ‖ DH1 ‖ DH2 ‖ DH3 ‖ DH4)`, F = 32 ta 0xFF bayt, info = `"Chittak_X3DH_v1"` | Test: qadamlar spec'dagi tartibda (kod review o'zingga) |
| S1-05 | `X3dh.Respond(bobIdentity, bobSignedPreKey, bobOtk?, aliceIdentityPublic, aliceEphemeralPublic)` → `sharedSecret` | **Asosiy test:** `Initiate` va `Respond` bir xil 32 bayt chiqaradi |
| S1-06 | OTK'siz variant (bundle'da one-time prekey yo'q — DH4 tashlab ketiladi) | Test: ikkala tomon ham OTK'siz bir xil secret |
| S1-07 | Salbiy testlar: noto'g'ri identity → secret farq qiladi; ishlatilgan OTK qayta ishlatilsa → farq qiladi; bundle imzosi buzilgan → `Initiate` exception | 3 ta test |
| S1-08 | Associated data: `AD = aliceIdentityPub ‖ bobIdentityPub` — keyingi sprintda AEAD'ga beriladi | AD hisoblash funksiyasi + test |
| S1-09 | Maxfiy materialni xotirada tozalash: `CryptographicOperations.ZeroMemory`, `IDisposable` kalit sinflar | Kod review checklist |
| S1-10 | `docs/crypto/x3dh.md` — o'z so'zing bilan 1 sahifa: 4 ta DH nimani beradi | Yozilgan (o'rganishning eng kuchli usuli) |

## Demo
`dotnet test --filter X3dh` → yashil; `x3dh.md` o'qib bo'lgan odam nima uchun DH3 kerakligini tushunadi.

## Resurslar
- **X3DH spec (majburiy, 2 marta o'qi):** https://signal.org/docs/specifications/x3dh/
- XEdDSA (nega Signal bitta kalit ishlatadi): https://signal.org/docs/specifications/xeddsa/
- RFC 7748 (X25519): https://www.rfc-editor.org/rfc/rfc7748
- RFC 8032 (Ed25519): https://www.rfc-editor.org/rfc/rfc8032
- RFC 5869 (HKDF): https://www.rfc-editor.org/rfc/rfc5869
- RFC 8439 (ChaCha20-Poly1305): https://www.rfc-editor.org/rfc/rfc8439
- .NET HKDF: https://learn.microsoft.com/dotnet/api/system.security.cryptography.hkdf
- .NET AesGcm / ChaCha20Poly1305: https://learn.microsoft.com/dotnet/api/system.security.cryptography.aesgcm
- libsignal'da X3DH (Rust, solishtirish uchun): `rust/protocol/src/` — https://github.com/signalapp/libsignal
- Mashq (ixtiyoriy, kuchli): Cryptopals Set 1–2: https://cryptopals.com/

## Tuzoqlar
- **Hech qachon o'z primitivingni yozma** — faqat kompozitsiya. X25519'ni "tushunish uchun" yozish — o'rganish uchun ok, lekin `Protocol`ga kirmaydi.
- Bayt tartibi: `DH1 ‖ DH2 ‖ DH3 ‖ DH4` — tartib spec'dagidek, aks holda kelajakda libsignal bilan mos kelmaydi.
- `info` string'ini keyin o'zgartirib bo'lmaydi (eski sessiyalar sinadi) — versiya qo'shib qo'y: `_v1`.

---

---
[← S00](S00-skelet.md) · [Reja va qoidalar](README.md) · [S02 →](S02-double-ratchet.md)
