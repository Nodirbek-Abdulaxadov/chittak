[← S10](S10-multi-device-push.md) · [Reja va qoidalar](README.md) · [S12 →](S12-turn-signaling.md)

# Sprint 11 — Desktop: Avalonia klient (Linux)

**Maqsad:** Linux'da ishlaydigan desktop klient — akkauntning **alohida qurilmasi**: kirish, kalitlar, shifrlangan lokal baza, chat, o'z qurilmalari bilan sync. Mantiq S07–S09'da yozilgan `Chittak.Client`dan keladi; bu sprintda faqat Avalonia View'lar va desktop platforma implementatsiyalari yoziladi.

**Nega hozir:** desktop — telefonning "ikkinchi qurilmasi", shuning uchun S10 (multi-device sync) tayyor bo'lishi shart: busiz telefonda yozgan xabaring desktopda ko'rinmaydi. Qo'ng'iroqlardan (S12–S13) oldin, chunki desktop v1'da qo'ng'iroqsiz — sprint arzon va M2 natijasini kengaytiradi.

**O'rganiladi:** Avalonia (MAUI XAML'dan farqlari, compiled bindings, ViewLocator, style selector'lar), bitta ViewModel — ikki UI framework (umumiy kutubxona chegarasi), desktop'da maxfiy saqlash (Linux Secret Service / D-Bus), XDG papkalar, desktop lifecycle (oyna yopilishi ≠ ilova tugashi, tray, bildirishnomalar).

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S11-01 | `Chittak.Desktop` (`avalonia.mvvm`, CommunityToolkit) → `Chittak.Client`ga reference; DI (`Microsoft.Extensions.DependencyInjection`) `App.axaml.cs`da; ViewModel ↔ View bog'lash ViewLocator orqali | Linux'da `dotnet run` → oyna ochiladi. Desktop loyihasida biznes mantiq yo'q — faqat View'lar + platforma implementatsiyalari |
| S11-02 | `ISecureStore` desktop implementatsiyasi: Linux — Secret Service (GNOME Keyring / KWallet, D-Bus orqali, masalan `Tmds.DBus`) | `~/.local/share/chittak` ichida `grep` bilan maxfiy kalit topilmaydi. Secret Service yo'q muhitda ilova aniq xato beradi — **plaintext faylga fallback yo'q** |
| S11-03 | SQLCipher baza desktop'da (S8-01 mantiqi): fayl `$XDG_DATA_HOME/chittak/` (odatda `~/.local/share/chittak/`), papka `0700`, fayl `0600`; baza kaliti `ISecureStore`da | Faylni `sqlite3` bilan ochib bo'lmaydi; `ls -l` → `-rw-------` |
| S11-04 | Ekranlar (Avalonia View'lar, ViewModel'lar `Chittak.Client`dan): telefon + OTP → kalit generatsiya → suhbatlar ro'yxati → chat → sozlamalar / qurilmalar | S07–S10 ssenariylari desktop'da ham o'tadi; chat 1000 xabarda silliq skroll (virtualizatsiya) |
| S11-05 | Kontakt topish: desktop'da manzillar kitobi yo'q → `IContactsProvider` bo'sh ro'yxat qaytaradi; qo'lda raqam kiritish → `POST /contacts/discover` (S8-06) | Raqam kiritib kontakt qo'shiladi. Kontaktlarni qurilmalar orasida sync qilish — roadmap |
| S11-06 | SignalR ulanish: desktop'da push yo'q — ilova ishlab turgan ekan ulanish ochiq. Oyna yopilsa tray'ga (`TrayIcon`), tray menyusidagi "Chiqish" ulanishni yopib ilovani tugatadi | Oyna yopiq, ilova tray'da → xabar keladi, decrypt → DB → ACK |
| S11-07 | Bildirishnoma: yangi xabarda desktop notification (freedesktop Notifications, D-Bus). Xabar matnini ko'rsatish — sozlamada, standart holatda faqat "Yangi xabar" | Standart holatda bildirishnomada matn yo'q |
| S11-08 | Single-instance: ikkinchi nusxa ishga tushsa birinchisining oynasini ochadi va chiqadi (lock fayl) | Ikki marta ishga tushirish → bitta jarayon |
| S11-09 | CI: Desktop uchun alohida job (Linux runner, workload kerak emas) — build + `Chittak.Client` testlari | Job yashil |
| S11-10 | ADR `0012-desktop-key-storage.md`: nega Secret Service, headless muhitda nima bo'ladi, nega plaintext fallback yo'q; Windows (DPAPI) / macOS (Keychain) — keyinga | ADR |

## Demo
Linux desktop + telefon bitta akkauntda (S10): telefonda yozilgan xabar desktopda "men yuborgan" bo'lib ko'rinadi, Bob'ning javobi ikkala qurilmaga keladi. Desktop'ni yopib ochganda qayta OTP so'ramaydi, sessiyalar saqlangan. `sqlite3` bazani ocholmaydi.

## Resurslar
- Avalonia hujjatlar (asosiy): https://docs.avaloniaui.net/
- **MAUI'dan Avalonia'ga (farqlar jadvali — S07–S09 tajribang shu yerda qo'l keladi):** https://docs.avaloniaui.net/docs/migration/maui/
- MVVM Avalonia'da: https://docs.avaloniaui.net/docs/fundamentals/the-mvvm-pattern
- Compiled bindings (`x:DataType`): https://docs.avaloniaui.net/docs/data-binding/compiled-bindings
- ViewLocator: https://docs.avaloniaui.net/docs/concepts/view-locator
- TrayIcon: https://docs.avaloniaui.net/controls/navigation/trayicon
- Avalonia Linux xususiyatlari: https://docs.avaloniaui.net/docs/platform-specific-guides/linux
- CommunityToolkit.Mvvm: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/
- Secret Service API (spec): https://specifications.freedesktop.org/secret-service/latest/
- Tmds.DBus (.NET'dan D-Bus): https://github.com/tmds/Tmds.DBus
- XDG Base Directory (qaysi fayl qayerga): https://specifications.freedesktop.org/basedir-spec/latest/
- Desktop Notifications (spec): https://specifications.freedesktop.org/notification-spec/latest/
- Signal Desktop (etalon — qanday saqlashini ko'r): https://github.com/signalapp/Signal-Desktop

## Tuzoqlar
- MAUI XAML ≠ Avalonia XAML: `VerticalStackLayout`, `CollectionView`, `Shell` yo'q (`StackPanel`, `ListBox`/`ItemsRepeater`, o'z navigatsiyang), style'lar CSS'ga o'xshash selector'lar bilan. View'larni ko'chirib bo'lmaydi — faqat ViewModel umumiy. Buni oldindan bil.
- `Chittak.Client` ichida `using Microsoft.Maui...` yoki `using Avalonia...` paydo bo'lsa — chegara buzildi. UI thread'ga qaytish `IDispatcher` orqali (S7-01).
- Compiled bindings yoqilmasa binding xatolari build'da emas, runtime'da jim chiqadi.
- Secret Service faqat desktop sessiyasida ishlaydi (keyring ochiq bo'lishi kerak); SSH/headless'da yo'q. "Vaqtincha faylga yozib turaman" — yo'q. Signal Desktop ham uzoq vaqt baza kalitini `config.json`da ochiq saqlagani uchun tanqid qilingan (keyin OS keyring'ga o'tkazildi) — takrorlama.
- Desktop'ni qayta o'rnatish = yangi qurilma (yangi identity) → kontaktlarda `UntrustedIdentity` banner (S9-08). Bu xato emas, kutilgan xulq.
- Ikki nusxa bir vaqtda — bitta SQLCipher bazaga ikki yozuvchi, bitta ratchet holatiga ikki decrypt → sessiya buziladi. Shuning uchun S11-08 majburiy.

---

---
[← S10](S10-multi-device-push.md) · [Reja va qoidalar](README.md) · [S12 →](S12-turn-signaling.md)
