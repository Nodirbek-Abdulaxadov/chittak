[← S08](S08-maui-baza-kontaktlar.md) · [Reja va qoidalar](README.md) · [S10 →](S10-multi-device-push.md)

# Sprint 9 — MAUI: chat → 🏁 MILESTONE 2

**Maqsad:** ikki telefon (yoki telefon + emulator) real vaqtda E2EE yozishadi; offline outbox; delivered/read receipt.

**Nega hozir:** hamma qism tayyor — birlashtirish.

**O'rganiladi:** `CollectionView` performans, SignalR MAUI klienti + fon rejimi, outbox pattern, `Connectivity` API, UI thread vs background.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S9-01 | Suhbatlar ro'yxati ekrani (oxirgi xabar, vaqt, o'qilmaganlar soni) | — |
| S9-02 | Chat ekrani: `CollectionView` (inverted), matn kiritish, yuborish | 1000 xabarda silliq skroll |
| S9-03 | Yuborish oqimi: `messages` INSERT (status=pending) → har `contact_devices` uchun `SessionCipher.Encrypt` → `outbox` → `POST /messages/batch` → status=sent. Tarmoq yo'q → `outbox`da qoladi | Airplane mode → yoqish → yuboriladi |
| S9-04 | `OutboxWorker`: `Connectivity.ConnectivityChanged` + har 30s; exponential backoff (`attempts`) | Test (unit) |
| S9-05 | Qabul: SignalR `ReceiveEnvelope` → `SessionCipher.Decrypt` → `messages` INSERT → **keyin** `Ack(queueId)`; dedup `clientMessageId` bo'yicha | Ilovani decrypt o'rtasida o'ldirsang — xabar qayta keladi, dublikat yo'q |
| S9-06 | Receipt: qabul qilingach `delivered` receipt (envelope_type=3, ratchet ichida), chat ochilganda `read`; jo'natuvchida ✓ / ✓✓ | Ko'rinadi |
| S9-07 | SignalR ulanish menejeri: ilova foreground'ga chiqganda ulanish, background'da uzish (batareya), `WithAutomaticReconnect` | — |
| S9-08 | `UntrustedIdentity` (S8-04) UI: "X ning xavfsizlik kaliti o'zgardi" banner, "qabul qilish" tugmasi | Qo'lda: ikkinchi qurilmani qayta ro'yxatdan o'tkazib tekshir |
| S9-09 | Xabar o'chirish (lokal), suhbatni o'chirish | — |
| S9-10 | End-to-end qo'lda test protokoli `docs/qa/chat.md`: 12 ssenariy (offline, reconnect, qayta o'rnatish, tartib) | Barchasi o'tgan |

## Demo (🏁 MILESTONE 2)
Ikki qurilma yozishadi; birini airplane mode'ga qo'yib yozilgan xabar yoqilganda keladi; serverda `psql` — faqat ciphertext, ACK'dan keyin bo'sh. **Video.**

## Resurslar
- CollectionView: https://learn.microsoft.com/dotnet/maui/user-interface/controls/collectionview/
- Connectivity: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/networking
- MAUI app lifecycle: https://learn.microsoft.com/dotnet/maui/fundamentals/app-lifecycle
- SignalR klient reconnect: https://learn.microsoft.com/aspnet/core/signalr/dotnet-client#handle-lost-connection
- Outbox pattern: https://microservices.io/patterns/data/transactional-outbox.html

## Tuzoqlar
- Decrypt'dan **oldin** ACK — xabar yo'qoladi. Tartib: decrypt → DB → ACK.
- Bir xabarni 3 qurilmaga 3 marta shifrlash — 3 ta alohida sessiya, 3 ta alohida ratchet. Bitta ciphertext'ni 3 joyga yuborma.
- UI thread'da kripto — 100 xabar sync bo'lganda UI qotadi. `Task.Run`.

---

---
[← S08](S08-maui-baza-kontaktlar.md) · [Reja va qoidalar](README.md) · [S10 →](S10-multi-device-push.md)
