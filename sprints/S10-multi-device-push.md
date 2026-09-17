[← S09](S09-maui-chat.md) · [Reja va qoidalar](README.md) · [S11 →](S11-turn-signaling.md)

# Sprint 10 — Multi-device sync, rotatsiya, push

**Maqsad:** foydalanuvchining o'z qurilmalari orasida sync; signed prekey rotatsiyasi; push orqali uyg'otish.

**Nega hozir:** asosiy oqim ishlaydi — endi "ikkinchi qurilma" va "telefon uxlaganda" holatlari.

**O'rganiladi:** Sesame (Signal multi-device modeli), FCM/APNs, background fetch cheklovlari, kalit rotatsiya gigienasi.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S10-01 | Yuborishda o'zining boshqa qurilmalariga ham envelope (sync copy): `GET /users/{me}/devices` minus o'zi | Ikki qurilmada bir akkaunt: birida yozilgan ikkinchisida ko'rinadi |
| S10-02 | Sync envelope turi (`.proto`da `SyncMessage { originalRecipientUserId, body }`) — qabul qiluvchi o'z qurilmasi bo'lsa "men yuborgan" deb ko'rsatadi | — |
| S10-03 | Kontakt qurilmalari o'zgarganda (yangi qurilma qo'shildi): `GET /users/{id}/devices`ni har yuborishda emas, 24 soatda / 409 javobda yangilash. Server: `POST /messages/batch`da qurilmalar ro'yxati to'liq bo'lmasa `409 { missingDevices, staleDevices }` (Signal usuli) | Integratsiya test |
| S10-04 | Signed prekey rotatsiya: klient har 7 kunda yangi, eskisini 14 kun saqlaydi (yo'ldagi prekey xabarlar uchun) | Test `IClock` bilan |
| S10-05 | Push: `PUT /devices/me/push-token`; server yangi xabar INSERT'da qabul qiluvchi offline bo'lsa **bo'sh** push (mazmunsiz, faqat "uyg'on") — FCM (Android), APNs (iOS, background push) | Ilova yopiq → push → ochilganda xabar tortiladi |
| S10-06 | Push'da rate-limit (bir qurilmaga 1 push / 30s) — batareya + xarajat | — |
| S10-07 | Qurilma ro'yxati ekrani: o'z qurilmalarim, "chiqarish" (`DELETE /devices/{id}` → cascade) | — |
| S10-08 | ADR `0008-device-linking-deferred.md`: v1'da har qurilma alohida OTP bilan kiradi; QR linking — v2 | ADR |

## Demo
Telefon + emulator bir akkaunt: birida yozilgan xabar ikkinchisida ko'rinadi; ilova yopiq bo'lganda push keladi.

## Resurslar
- **Sesame spec:** https://signal.org/docs/specifications/sesame/
- FCM: https://firebase.google.com/docs/cloud-messaging
- APNs: https://developer.apple.com/documentation/usernotifications
- Plugin.Firebase (MAUI): https://github.com/TobiasBuchholz/Plugin.Firebase
- Server tomonda FCM/APNs yuborish: `FirebaseAdmin` NuGet — https://firebase.google.com/docs/admin/setup ; APNs uchun `dotnet-apns` yoki HTTP/2 to'g'ridan-to'g'ri
- Signal-Server `MessageController`dagi 409 `mismatched devices` mantiqi (etalon)

## Tuzoqlar
- Push ichida xabar mazmuni — hech qachon. Faqat "yangi narsa bor".
- iOS background push cheklangan (Apple throttling) — "ilova ochilganda tortish" har doim ishlashi kerak, push — tezlashtirish, kafolat emas.
- Sync copy'ni o'z qurilmangga yuborishda o'zingga yuborma (deviceId != me).

---

---
[← S09](S09-maui-chat.md) · [Reja va qoidalar](README.md) · [S11 →](S11-turn-signaling.md)
