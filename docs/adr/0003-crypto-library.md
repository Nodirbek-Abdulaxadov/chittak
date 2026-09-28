# 0003. Kripto kutubxona: NSec (libsodium)

- **Holat:** Qabul qilindi (2026-09-28) — iOS qismi ochiq
- **Sprint:** S0-08 (spike)

## Kontekst

Protokol (X3DH, Double Ratchet) uchun X25519 (DH), Ed25519 (imzo), HKDF va AEAD kerak. Primitivlar o'zimiz yozilmaydi — faqat tayyor kutubxona ustida kompozitsiya. .NET BCL'da X25519 va Ed25519 yo'q. Nomzodlar: **NSec** (libsodium ustida, native) va **BouncyCastle** (to'liq C#). Asosiy xavf — NSec'ning native `libsodium`i mobil platformada yuklanmasligi.

## Spike (S0-08)

`Chittak.Protocol/Spike/NsecSpike.cs` — X25519 DH (ikki tomon bir xil sir), Ed25519 sign/verify, ChaCha20-Poly1305 round-trip. Haqiqiy bog'liqlik yo'li orqali chaqirildi: ilova → `Chittak.Client` → `Chittak.Protocol` → NSec.

| Platforma | Qanday | Natija |
|---|---|---|
| Linux x64 (Ubuntu 26.04) | konsol, `Chittak.Protocol` orqali | ✅ DH=True, Ed25519=True, ChaCha20-Poly1305=True |
| Android arm64, API 36 (haqiqiy telefon) | `Chittak.Mobile` (MAUI), logcat | ✅ DH=True, Ed25519=True, ChaCha20-Poly1305=True |
| iOS | — | ⚪ Tekshirilmagan: iOS build faqat macOS'da, Mac yo'q |

Versiya: `NSec.Cryptography 26.4.0` (`Directory.Packages.props`).

## Qaror

- **NSec** — `Chittak.Protocol`ning yagona kripto kutubxonasi. BouncyCastle fallback kerak bo'lmadi.
- HKDF va HMAC uchun .NET BCL (`System.Security.Cryptography.HKDF`, `HMACSHA256`) ham ishlatilishi mumkin — ular NSec'ga muqobil emas, to'ldiruvchi.
- AEAD tanlovi (ChaCha20-Poly1305 yoki AES-256-GCM) — alohida qaror, S1-01.

## Oqibatlar

- Native kutubxona: har yangi platforma (iOS, Windows, macOS desktop) birinchi marta shu spike bilan tekshiriladi.
- iOS: Mac paydo bo'lganda spike takrorlanadi va shu ADR yangilanadi. Mac bo'lmasa v1 Android + Linux desktop bo'lishi — foydalanuvchi qarori (S0 "Tuzoqlar").
- `NsecSpike.cs` va ilovalardagi chaqiruvlar vaqtinchalik — S01'da haqiqiy primitiv wrapper'lari yozilganda o'chiriladi.

## Ko'rib chiqilgan muqobillar

- **BouncyCastle** — native bog'liqlik yo'q (iOS/Android xavfsiz), lekin sekinroq, API kattaroq, xato ishlatish oson. NSec ishlagani uchun zaxirada qoladi.
- **O'z implementatsiyasi** — hech qachon (CLAUDE.md, S01 "Tuzoqlar").
