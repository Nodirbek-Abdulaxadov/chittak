[← S05](S05-server-kalitlar.md) · [Reja va qoidalar](README.md) · [S07 →](S07-maui-auth.md)

# Sprint 6 — Server: Relay (navbat, SignalR, ACK, purge) → 🏁 MILESTONE 1

**Maqsad:** `POST /messages` navbatga; online bo'lsa WS push; klient ACK → delete; reconnect'da ACK'siz qayta yuborish; TTL purge. **Ikki konsol klient server orqali E2EE yozishadi.**

**Nega hozir:** bu — loyihaning "ishlaydi" nuqtasi. Shu yerdan keyin faqat UI va qo'shimchalar.

**O'rganiladi:** SignalR (WS ustidagi abstraksiya, reconnect), connection ↔ device mapping, at-least-once delivery, idempotency, backpressure, integratsiya test WS bilan.

## Qaror (qabul qilingan) — ADR `0006-realtime-transport.md`
**SignalR** (WebSocket transport, MessagePack protokol). Sabab: reconnect, connection ↔ user mapping, guruhlash tayyor; MAUI uchun rasmiy klient bor. Xom WebSocket ko'proq o'rgatgan bo'lardi, lekin 1 hafta ko'proq olardi. Cheklov: WebSocket transportini majburlash (`SkipNegotiation`, faqat WS) — long-polling'ga tushib ketmasin. S6-03 birinchi kuni ADR'ni yozib qo'y.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S6-01 | `POST /messages { recipientDeviceId, clientMessageId, envelopeType, envelope(base64) }` → `message_queue` INSERT `ON CONFLICT (recipient_device_id, client_message_id) DO NOTHING` → 200 (ikkala holda) | Test: bir xil so'rov 2 marta → 1 qator |
| S6-02 | Batch variant: `POST /messages/batch` — bir user'ning barcha qurilmalari uchun envelope'lar bitta tranzaksiyada (multi-device S10'da kerak) | Test |
| S6-03 | `MessagesHub` (SignalR): ulanishda JWT'dan `deviceId`; `IConnectionRegistry` (deviceId → connectionId, in-memory, keyin Redis) | Ulanish/uzilish test |
| S6-04 | Ulanganda: ACK'siz barcha navbat yozuvlarini `id` tartibida push (`ReceiveEnvelope { queueId, senderDeviceId, type, envelope }`) | Test: offline'da 3 xabar → ulanishda 3 ta keladi |
| S6-05 | Yangi xabar INSERT'dan keyin: qabul qiluvchi online bo'lsa darhol push | Test: online klient 200ms ichida oladi |
| S6-06 | `Ack(queueId)` hub metodi → `DELETE WHERE id=? AND recipient_device_id=?` (faqat o'zingniki) | Test: ACK'dan keyin DBda yo'q; begona id ACK qilsa hech narsa bo'lmaydi |
| S6-07 | Socket uzilib qayta ulansa — ACK qilinmaganlar qayta keladi | Test: push → uzish (ACK'siz) → ulash → yana keladi |
| S6-08 | TTL purge `BackgroundService` (har 10 min, `expires_at < now()`) | Test `IClock` bilan |
| S6-09 | Presence: `GET /users/{id}/presence` → `online|lastSeen` (lastSeen soatgacha yaxlitlangan — metadata minimallash) | Test |
| S6-10 | Envelope hajmi limiti (64 KB) va per-device yuborish rate-limit (30/min) | Test: 65 KB → 413 |
| S6-11 | Konsol demo → to'liq server orqali: auth (mock OTP), kalitlar, yuborish, WS qabul, ACK | 🏁 Ikki terminal, bitta server, DBda faqat ciphertext |
| S6-12 | `docs/protocol/delivery.md` — yetkazish shartnomasi (at-least-once, ACK, dedup) | Yozilgan |

## Demo (🏁 MILESTONE 1)
Ikki terminalda `Chittak.ConsoleDemo --phone +99890...`; biri offline bo'lganda yozilgan xabar u ulanganda keladi; `psql`da `message_queue` ACK'dan keyin bo'sh. **Buni video qilib saqlab qo'y.**

## Resurslar
- SignalR: https://learn.microsoft.com/aspnet/core/signalr/introduction
- SignalR hub + auth: https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz
- MessagePack protokoli: https://learn.microsoft.com/aspnet/core/signalr/messagepackhubprotocol
- WebSocket transportini majburlash: https://learn.microsoft.com/aspnet/core/signalr/configuration#configure-client-options
- SignalR .NET klient (konsol + MAUI): https://learn.microsoft.com/aspnet/core/signalr/dotnet-client
- At-least-once / idempotency tushunchasi: https://microservices.io/patterns/communication-style/idempotent-consumer.html
- Signal-Server `MessageController` / `WebSocketConnection` (etalon)

## Tuzoqlar
- "Push qildim = yetkazdim" deb o'chirish — S6-07 testi buni ushlaydi. Test avval yoz.
- In-memory connection registry — bitta server instansiyasi uchun. Ikki instansiya bo'lsa Redis backplane kerak (S15).
- WS orqali katta envelope — hajm limiti + base64 emas, binary (MessagePack) ishlat.

---

---
[← S05](S05-server-kalitlar.md) · [Reja va qoidalar](README.md) · [S07 →](S07-maui-auth.md)
