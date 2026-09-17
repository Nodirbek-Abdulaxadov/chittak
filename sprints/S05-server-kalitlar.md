[← S04](S04-server-auth.md) · [Reja va qoidalar](README.md) · [S06 →](S06-server-relay.md)

# Sprint 5 — Server: Qurilmalar va kalitlar kitobi

**Maqsad:** klient kalitlarini yuklaydi; boshqa klient bundle oladi; OTK atomik iste'mol qilinadi; `count` ishlaydi.

**Nega hozir:** relay'dan oldin "manzillar kitobi" kerak. Bu sprintda Postgres concurrency'ni amalda ko'rasan.

**O'rganiladi:** `FOR UPDATE SKIP LOCKED`, EF Core'da raw SQL vs LINQ, concurrency testlash (`Parallel.ForEachAsync`), idempotent upload, avtorizatsiya (faqat o'z qurilmang kalitini yuklaysan).

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S5-01 | `PUT /keys/identity { identityKey(64b) }` — bir marta; ikkinchi marta boshqa kalit → 409 (identity o'zgarishi = alohida oqim, v1'da yo'q) | Testlar |
| S5-02 | `PUT /keys/signed-prekey { keyId, publicKey, signature }` — server **imzoni tekshiradi** (`Chittak.Protocol` bilan) → noto'g'ri imzo 400 | Test: buzilgan imzo rad |
| S5-03 | `POST /keys/one-time-prekeys { keys: [{keyId, publicKey}] }` — batch, `ON CONFLICT DO NOTHING` (idempotent) | Test: ikki marta yuklash → dublikat yo'q |
| S5-04 | `GET /keys/count` → `{ oneTimePreKeys: n }` | Test |
| S5-05 | `GET /users/{userId}/devices` → `[ { deviceId, registrationId } ]` | Test: begona user ham ko'ra oladi (bu public), lekin faqat auth bilan |
| S5-06 | `GET /keys/bundle/{deviceId}` → `{ identityKey, signedPreKey{keyId,publicKey,signature}, oneTimePreKey?{keyId,publicKey} }` — OTK **bitta SQL** bilan: `DELETE ... WHERE id = (SELECT ... FOR UPDATE SKIP LOCKED LIMIT 1) RETURNING` | Test: 2 ta OTK, 50 parallel so'rov → aynan 2 tasi OTK bilan, 48 tasi OTK'siz, dublikat yo'q |
| S5-07 | Bundle so'roviga rate-limit (60/min/device) — OTK pool'ni bo'shatib qo'yish hujumiga qarshi | Test |
| S5-08 | Signed prekey rotatsiya: yangi `keyId` yuklansa eski saqlanadi; 3-si yuklansa eng eskisi o'chadi (joriy + oldingi) | Test |
| S5-09 | `Chittak.Core`da `IPreKeyRepository` + `Infrastructure`da implementatsiya; S5-06 raw SQL `FromSqlRaw`/Dapper | Core toza |
| S5-10 | Konsol demoni (S3-07) haqiqiy serverga ulash: `IPreKeyBundleSource` → HTTP | Konsol demo server orqali bundle oladi |

## Demo
Ikki konsol klient serverga kalit yuklaydi, biri ikkinchisining bundle'ini oladi, DBda OTK kamayadi; parallel test yashil.

## Resurslar
- Postgres `SELECT ... FOR UPDATE SKIP LOCKED`: https://www.postgresql.org/docs/current/sql-select.html#SQL-FOR-UPDATE-SHARE
- EF Core raw SQL: https://learn.microsoft.com/ef/core/querying/sql-queries
- Dapper (alternativ, raw SQL uchun qulay): https://github.com/DapperLib/Dapper
- Signal server (etalon, Java): https://github.com/signalapp/Signal-Server — `KeysController` ni o'qi
- Concurrency testlash: `Parallel.ForEachAsync` — https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel.foreachasync

## Tuzoqlar
- EF `Remove` + `SaveChanges` bilan OTK o'chirish — poyga. Faqat bitta SQL statement.
- Server imzoni tekshirmasa — buzilgan kalit yuklab, o'zingni DoS qilib qo'yasan (sessiya boshlanmaydi).

---

---
[← S04](S04-server-auth.md) · [Reja va qoidalar](README.md) · [S06 →](S06-server-relay.md)
