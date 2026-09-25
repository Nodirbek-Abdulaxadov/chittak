[← S06](S06-server-relay.md) · [Reja va qoidalar](README.md) · [S08 →](S08-maui-baza-kontaktlar.md)

# Sprint 7 — MAUI: kirish, kalitlar, xavfsiz saqlash

**Maqsad:** telefon ilovasi ro'yxatdan o'tadi, kalit yaratadi, maxfiy qismini Keystore/Keychain ortida saqlaydi, serverga yuklaydi, tokenni yangilab turadi.

**Nega hozir:** protokol va server isbotlangan — endi UI'ga vaqt sarflash xavfsiz.

**O'rganiladi:** MAUI Shell navigatsiya, MVVM (CommunityToolkit.Mvvm), UI'dan mustaqil klient kutubxonasi (`Chittak.Client`) chegarasi, `SecureStorage`, DI MAUI'da, HTTP klient (Refit) + Polly, platform farqlari (Android Keystore / iOS Keychain), MAUI'da kripto kutubxona (S0-08 spike natijasi).

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S7-01 | Klient strukturasi. **`Chittak.Client`** (net10.0, UI framework'ga bog'liq emas): `Features/Auth`, `Features/Chat`, `Features/Contacts` (ViewModel'lar), `Services/Api`, `Services/Storage`, `Services/Crypto` (Protocol wrapper), platforma interfeyslari — `ISecureStore`, `IContactsProvider`, `IConnectivityMonitor`, `IDispatcher`, `INavigator`. **`Chittak.Mobile`** — faqat View'lar (XAML) + shu interfeyslarning MAUI implementatsiyasi; DI `MauiProgram.cs`. CI'da Mobile uchun alohida job (`maui-android` workload) | Build Android (+iOS bo'lsa); `Chittak.Client`da `Microsoft.Maui.*` reference yo'q — S11'da Avalonia aynan shu ViewModel'larni ishlatadi |
| S7-02 | Refit interfeysi `IChittakApi` (auth, keys, users, messages) + `AuthHandler` (access token qo'shadi, 401 → refresh → retry) + Polly retry (tarmoq) | Unit test: 401 → refresh → qayta so'rov |
| S7-03 | `SecureKeyStore : IIdentityStore, IPreKeyStore` — `Chittak.Client`da, `ISecureStore` ustida (MAUI implementatsiyasi — `SecureStorage`); maxfiy kalitlar base64; **hech qachon** oddiy `Preferences`ga emas | Android'da `adb` bilan app data ochilganda plaintext kalit ko'rinmaydi |
| S7-04 | Ekranlar: Telefon kiritish → OTP kiritish → (birinchi marta) "Kalitlar yaratilmoqda" → Asosiy. Xato holatlari: noto'g'ri kod, 429, tarmoq yo'q | Qo'lda test ssenariylari `docs/qa/auth.md`da |
| S7-05 | Ro'yxatdan o'tgach: identity (Ed25519+X25519) + signed prekey + 100 OTK generatsiya → `SecureKeyStore` → serverga yuklash (S5 endpointlari) | Serverda kalitlar paydo bo'ladi |
| S7-06 | Ilova ochilganda: token yaroqlimi → asosiy ekran; yo'q → refresh; refresh ham yo'q → login | 3 ssenariy qo'lda |
| S7-07 | `GET /keys/count` < 20 bo'lsa 100 ta OTK to'ldirish (ilova ochilganda + har 24 soat) | Log'da ko'rinadi |
| S7-08 | Loading/error UI holatlari, `Toast`/`Snackbar` (MAUI Community Toolkit) | — |
| S7-09 | ADR `0007-mobile-key-storage.md`: nega `SecureStorage` (Keystore-wrapped), nega hardware-backed X25519 emas (Android API 33+ da qisman, iOS Secure Enclave X25519 yo'q) | ADR |

## Demo
Emulatorda ro'yxatdan o'tish → serverda user/device/kalitlar; ilovani yopib ochganda qayta OTP so'ramaydi.

## Resurslar
- MAUI hujjatlar (asosiy): https://learn.microsoft.com/dotnet/maui/
- Shell: https://learn.microsoft.com/dotnet/maui/fundamentals/shell/
- CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`): https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/
- MAUI Community Toolkit: https://learn.microsoft.com/dotnet/communitytoolkit/maui/
- SecureStorage: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/secure-storage
- Refit: https://github.com/reactiveui/refit · Polly: https://www.pollydocs.org/
- Android Keystore (fon): https://developer.android.com/privacy-and-security/keystore
- iOS Keychain (fon): https://developer.apple.com/documentation/security/keychain-services
- OWASP MASVS (mobil xavfsizlik standarti — `MASVS-STORAGE`, `MASVS-CRYPTO` bo'limlari): https://mas.owasp.org/
- Video kurs (bepul, MAUI asoslari): James Montemagno "MAUI for Beginners" — YouTube'da `.NET` kanalida

## Tuzoqlar
- ViewModel'da `SecureStorage`, `Shell.Current`, `MainThread` kabi MAUI API'larini to'g'ridan-to'g'ri chaqirish — S11'da desktop'da kompilyatsiya ham bo'lmaydi. Navigatsiya, UI thread, saqlash — faqat interfeys orqali.
- MAUI'da `HttpClient`ni har safar `new` qilish — socket exhaustion. `IHttpClientFactory`/Refit orqali.
- `SecureStorage` Android'da backup'ga tushishi mumkin — `android:allowBackup="false"` yoki backup qoidasi.
- Emulatorda `localhost` = emulatorning o'zi. Android: `10.0.2.2`; iOS simulator: `localhost` ishlaydi.
- HTTPS dev sertifikati emulatorda — `dotnet dev-certs` + Android network security config. Bunga 2 soat ajrat.

---

---
[← S06](S06-server-relay.md) · [Reja va qoidalar](README.md) · [S08 →](S08-maui-baza-kontaktlar.md)
