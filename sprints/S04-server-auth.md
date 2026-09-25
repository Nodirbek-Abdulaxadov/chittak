[← S03](S03-envelope-konsol-demo.md) · [Reja va qoidalar](README.md) · [S05 →](S05-server-kalitlar.md)

# Sprint 4 — Server: Auth (OTP, JWT, refresh, rate-limit)

**Maqsad:** `POST /auth/otp/send`, `POST /auth/otp/verify` (→ user + device + tokenlar), `POST /auth/refresh`, `POST /auth/logout`. Rate-limit ishlaydi. SMS — mock (konsolga yoziladi).

**Nega hozir:** server'ning eng "oddiy" qismi — ASP.NET Core'ni o'rganish uchun yaxshi kirish. Kalitlar va navbat auth'siz ishlamaydi.

**O'rganiladi:** Minimal API, JWT (access, 15 min) + refresh token (hashlangan, 30 kun), `IHostedService`, rate limiting middleware, Testcontainers bilan integratsiya test, FluentValidation yoki qo'lda validatsiya, Options pattern, Serilog.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S4-01 | `Chittak.Domain`: `User`, `Device`, `PhoneVerification`, `AuthSession` domain modellari; `Chittak.Application`: `IPhoneVerificationRepository`, `IDeviceRepository`, `IAuthSessionRepository`, `ISmsSender`, `IClock` | Core'da EF/HTTP yo'q |
| S4-02 | E.164 normalizatsiya: `+998 90 123-45-67` → `+998901234567`; `libphonenumber-csharp` yoki regex | Test: 10 ta kirish formati |
| S4-03 | `POST /auth/otp/send { phone }`: rate-limit (3/10min/raqam, 10/kun/raqam, 20/soat/IP) → 6 xonali kod → `Argon2id` yoki `PBKDF2` hash → `phone_verifications` → `ISmsSender` | Integratsiya test: 4-so'rov 429 qaytaradi; DBda `code_hash` plaintext emas |
| S4-04 | `POST /auth/otp/verify { phone, code, registrationId, deviceName, platform }`: `consumed_at IS NULL AND expires_at > now() AND attempts < 5`; noto'g'ri → `attempts++`; to'g'ri → `consumed_at`, user (yo'q bo'lsa) + `phone_hash`, device, `auth_sessions` → `{ accessToken, refreshToken, userId, deviceId }` | Testlar: to'g'ri; 5 marta noto'g'ri → 6-si rad; ishlatilgan kod qayta rad; muddati o'tgan rad |
| S4-05 | JWT: `sub=deviceId`, `uid=userId`, 15 min, HS256 (secret `appsettings`dan, dev uchun) | `[Authorize]` endpoint tokensiz 401 |
| S4-06 | `POST /auth/refresh { refreshToken }`: hash bo'yicha topish, `revoked_at IS NULL`, `expires_at > now()` → yangi juftlik, eskisini `revoked_at` (rotation) | Test: eski refresh ikkinchi marta rad |
| S4-07 | `POST /auth/logout` → session revoke | Test |
| S4-08 | `ISmsSender` implementatsiyalari: `ConsoleSmsSender` (dev), `EskizSmsSender` (skeleton, keyin) | Dev'da kod konsolda ko'rinadi |
| S4-09 | `BackgroundService`: 24 soatdan eski `phone_verifications`, muddati o'tgan `auth_sessions` purge | Test: `IClock` bilan vaqtni oldinga surib |
| S4-10 | Serilog: structured log, **telefon raqam va tokenlar hech qachon logga tushmaydi** (`Destructure` qoidasi) | Log tekshiruvi |
| S4-11 | Integratsiya test bazasi: `WebApplicationFactory` + Testcontainers Postgres, har test klassida toza DB | 1 ta helper, keyingi sprintlar shuni ishlatadi |
| S4-12 | OpenAPI (`Swashbuckle`/built-in) — dev'da Swagger UI | `/swagger` ochiladi |

## Demo
Swagger'dan `send` → konsolda kod → `verify` → token → `[Authorize]` `GET /me` ishlaydi; 4-`send` 429.

## Resurslar
- Minimal APIs: https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- JWT bearer: https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication
- Rate limiting middleware: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- Hosted services: https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services
- Integratsiya testlar: https://learn.microsoft.com/aspnet/core/test/integration-tests
- Options pattern: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options
- Serilog: https://serilog.net/
- libphonenumber-csharp: https://github.com/twcclegg/libphonenumber-csharp
- OWASP OTP/auth cheat sheet: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- O'zbekiston SMS shlyuzlari (keyin): Eskiz.uz, Play Mobile — dev'da mock yetadi

## Tuzoqlar
- OTP'ni `SHA-256` bilan hash'lash yetarli emas (6 xona = 1M variant, sekundda brute-force) — `attempts` limiti + qisqa muddat asosiy himoya; hash — DB sizib chiqsa uchun.
- JWT'ni revoke qilib bo'lmaydi — shuning uchun 15 min. Uzunroq qilma.
- Rate-limit'ni test'da o'chirib qo'yish oson — faqat aniq belgilangan testlarda o'chir.

---

---
[← S03](S03-envelope-konsol-demo.md) · [Reja va qoidalar](README.md) · [S05 →](S05-server-kalitlar.md)
