[← S07](S07-maui-auth.md) · [Reja va qoidalar](README.md) · [S09 →](S09-maui-chat.md)

# Sprint 8 — MAUI: shifrlangan lokal baza, sessiyalar, kontaktlar

**Maqsad:** telefonda SQLCipher baza (xabarlar, sessiyalar, kontaktlar); `ISessionStore` diskda; manzillar kitobidan kontakt topish.

**Nega hozir:** chat UI'dan oldin ma'lumot qatlami. Sessiya holati diskda bo'lmasa ilova qayta ochilganda sessiya yo'qoladi.

**O'rganiladi:** SQLCipher + sqlite-net, baza kaliti `SecureStorage`da, migratsiya sxemasi (klient tomonida), MAUI Contacts API + ruxsatlar, hash'lash va batch so'rov.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S8-01 | `SQLitePCLRaw.bundle_e_sqlcipher` + `sqlite-net-pcl`; baza kaliti (32 bayt random) `ISecureStore`da (MAUI: `SecureStorage`); birinchi ochilishda yaratiladi. Baza kodi `Chittak.Client`da — S11'da desktop ham ishlatadi | Baza faylini `adb pull` qilib `sqlite3` bilan ochib bo'lmaydi |
| S8-02 | Jadvallar: `contacts(userId, phone, displayName)`, `contact_devices(userId, deviceId, identityKey, trusted)`, `sessions(remoteDeviceId, stateBlob, updatedAt)`, `messages(id, clientMessageId, conversationId, direction, body, status, sentAt)`, `outbox(clientMessageId, recipientDeviceId, envelope, attempts)` | Sxema `docs/mobile/local-db.md`da |
| S8-03 | `SqliteSessionStore : ISessionStore` — `RatchetState` serialize (S2-01) → `sessions` | Test: saqla → o'qi → bir xil |
| S8-04 | `IIdentityStore.IsTrusted(remoteDeviceId, identityKey)` — TOFU (trust on first use): birinchi ko'rilgan identity saqlanadi, o'zgarsa `UntrustedIdentity` → UI ogohlantirish (S14 safety numbers uchun zamin) | Test |
| S8-05 | Manzillar kitobi: `IContactsProvider` (MAUI implementatsiyasi — `Contacts.GetAllAsync()`), ruxsat so'rash, E.164 normalizatsiya (mamlakat kodi telefon SIM/localidan) | Ruxsat rad etilsa ilova ishlayveradi (qo'lda raqam kiritish) |
| S8-06 | Server: `POST /contacts/discover { hashes[] }` (≤5000, 1/min/device) → `{ matches: [{hash, userId}] }`, `discoverable=true` filtri; `PATCH /me { discoverable }` | Integratsiya test |
| S8-07 | Klient: hash'lar → discover → `contacts`ga yozish; natija UI'da "Chittak'da bor" ro'yxati | Emulator kontaktlari bilan ishlaydi |
| S8-08 | Kontakt qurilmalari: `GET /users/{id}/devices` → `contact_devices` (keshlanadi, 24 soat) | — |
| S8-09 | Sozlamalar ekrani: `discoverable` toggle, chiqish (logout → lokal baza o'chirish) | — |

## Demo
Ilovada kontaktlar ro'yxatida Chittak ishlatadiganlar ko'rinadi; ilova qayta ochilganda sessiya diskdan yuklanadi (S9'da tekshiriladi).

## Resurslar
- sqlite-net: https://github.com/praeclarum/sqlite-net
- SQLitePCLRaw (SQLCipher bundle): https://github.com/ericsink/SQLitePCL.raw
- SQLCipher: https://www.zetetic.net/sqlcipher/
- MAUI Contacts: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/contacts
- MAUI ruxsatlar: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions
- Signal: private contact discovery (nega hash yetarli emas): https://signal.org/blog/private-contact-discovery/

## Tuzoqlar
- Baza kalitini kodga yozish / `Preferences`ga qo'yish — yo'q. Faqat `SecureStorage`.
- Sessiya blob'ini o'qish→o'zgartirish→yozish o'rtasida ikkinchi xabar kelsa — poyga. Sessiya bo'yicha `SemaphoreSlim` (per remoteDeviceId).
- Kontaktlar ruxsati iOS'da rad etilsa qayta so'rab bo'lmaydi — UI'da sozlamalarga yo'naltir.

---

---
[← S07](S07-maui-auth.md) · [Reja va qoidalar](README.md) · [S09 →](S09-maui-chat.md)
