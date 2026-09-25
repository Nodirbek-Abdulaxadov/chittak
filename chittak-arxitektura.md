# Chittak — Arxitektura, Workflow, Struktura (chuqur tushuntirish)

Bu hujjat "nima uchun shunday" ga javob beradi — kodni o'zing yozganda har bo'lakning sababini bilishing uchun.

---

## 1. Katta rasm — 3 aktyor va 1 tamoyil

Butun tizimni bitta jumla bilan tushun:

> **Ikki telefon butun shifrlashni qiladi. O'rtadagi server — "soqov pochta" — muhrlangan quti'larni tashiydi, lekin ochib o'qiy olmaydi.**

**Analogiya — pochta bo'limi:**
- Sen (klient) xat yozasan → uni **faqat qabul qiluvchi ocha oladigan** qutiga solib muhrlaysan (E2EE).
- Pochta (server) muhrlangan qutini manzilga tashiydi — **kaliti yo'q, ichini ko'rmaydi.**
- Pochta yana **"ochiq manzillar kitobi"** (public prekey bundle) yuritadi — shu orqali odamlar senga birinchi muhrlangan qutini yubora oladi.

**Eng muhim xulosa:** xavfsizlik **klientda** yashaydi, serverda emas. Server — **ishonchsiz relay.** Shu tamoyil butun dizaynni boshqaradi.

```
   [ ALISA telefoni ]                              [ BOB telefoni ]
   - identity kaliti (maxfiy: Keystore)            - identity kaliti (maxfiy)
   - X3DH + Double Ratchet                         - X3DH + Double Ratchet
   - local shifrlangan baza                        - local shifrlangan baza
          \                                              /
           \   muhrlangan quti (opaque ciphertext)      /
            \                                           /
             +----->  [ SERVER = SOQOV POCHTA ]  <-----+
                      - ochiq kalitlar kitobi (prekey bundle)
                      - navbat (opaque, ACK'gacha turadi, TTL)
                      - kontakt topish (phone hash -> bor/yo'q)
                      - SignalR (WS): yetkazish + presence + qo'ng'iroq signal
                      - telefon/OTP auth + TURN credential
                               |
                        [ PostgreSQL ]
             faqat: PUBLIC kalit + OPAQUE navbat + auth
             hech qachon: plaintext, maxfiy kalit, sessiya holati
```

---

## 2. Uch qism — har biri nima qiladi (va NIMANI qilmaydi)

**Klient (telefonda — .NET MAUI; desktop'da — Avalonia):**
- Identity va prekey kalitlarni **generatsiya** qiladi; maxfiylarini Keystore/Keychain'da (desktop'da — OS keyring, Linux'da Secret Service) saqlaydi.
- **Butun kripto** shu yerda: X3DH, Double Ratchet, envelope, session.
- Local shifrlangan xabar bazasi + offline outbox + UI.
- Mantiq (ViewModel'lar, servislar, kripto wrapper) bitta umumiy `Chittak.Client` kutubxonasida; MAUI va Avalonia faqat View'lar va platforma implementatsiyalarini (`ISecureStore`, `IContactsProvider`, ...) beradi. Sabab: MAUI'da Linux desktop target yo'q, ikki UI mantiqni ikki marta yozishga majburlamasligi kerak.
- Desktop — telefonning nusxasi emas, **alohida qurilma**: o'z identity kaliti, o'z sessiyalari; telefon bilan faqat sync envelope (multi-device) orqali bog'lanadi.
- *Qilmaydi:* maxfiy kalitni hech qachon serverga bermaydi.

**Server (ASP.NET Core):**
- **Relay** (muhrlangan quti'larni tashiydi) + **kalitlar kitobi** (public bundle beradi) + **auth** (OTP) + **real-time** (SignalR ustida WebSocket, MessagePack: yetkazish, presence, qo'ng'iroq signal).
- *Qilmaydi:* ciphertext'ni **ochmaydi**; sessiya/ratchet holatini **saqlamaydi**; xabar tarixini **tutmaydi** (klient ACK qilgach o'chiradi); kontaktlar ro'yxatini **saqlamaydi**.

**PostgreSQL:**
- Faqat: PUBLIC kalitlar + OPAQUE navbat + auth metadata.
- *Qilmaydi:* plaintext, maxfiy kalit, sessiya holati **yo'q**.

Bu ajratma — "butunlay xavfsizlik" emas, lekin **to'g'ri E2EE:** server buzilsa ham, u faqat muhrlangan qutilar va ochiq kalitlarni ko'radi.

---

## 3. Workflow — uchidan-uchiga (butun hikoya)

Bu — eng muhim qism. Xabar qanday tug'iladi, sayohat qiladi, o'qiladi:

### (a) Ro'yxatdan o'tish (bir marta)
```
Alisa: telefon raqam (E.164: +998...) -> serverga "OTP yubor"
Server: rate-limit tekshiradi (3 ta / 10 min / raqam, per-IP ham)
        -> SMS OTP yuboradi, HASH saqlaydi   [phone_verifications]
Alisa: OTP ni kiritadi -> server tekshiradi:
        consumed_at IS NULL AND expires_at > now() AND attempts < 5
        -> to'g'ri: consumed_at = now() (kod qayta ISHLAMAYDI)
        -> device yaratiladi
       [users] (phone + phone_hash) + [devices] + [auth_sessions] (token)
Alisa telefoni: identity keypair GENERATSIYA qiladi
       - maxfiy qism -> Keystore (HECH QACHON chiqmaydi)
       - public qism -> serverga   [device_identity_keys]
Alisa telefoni: signed prekey + bir dasta one-time prekey generatsiya
       - hammasi PUBLIC -> serverga   [signed_prekeys] + [one_time_prekeys]
```
Endi Alisa "ochiq manzillar kitobida" bor — unga xabar yuborsa bo'ladi.

### (a2) Kontakt topish — Alisa Bob'ni qanday topadi?
```
Alisa telefoni: manzillar kitobidagi raqamlarni E.164 ga normalizatsiya qiladi
                -> har birining SHA-256 hash'ini oladi
Alisa: POST /contacts/discover  { hashes: [...] }   (<= 5000 ta, 1 so'rov / min)
Server: users.phone_hash bilan solishtiradi, discoverable = true bo'lganlarini qaytaradi
        -> { matches: [{ hash, user_id }] }
        So'rovdagi ro'yxatni SAQLAMAYDI, log qilmaydi.
Alisa telefoni: mos kelganlarni LOCAL kontakt sifatida saqlaydi (serverda contacts jadvali YO'Q)
```
**Halol eslatma:** telefon raqam hash'i brute-force'ga bardosh bermaydi (keyspace kichik). Bu tasodifiy loglashdan himoya, **private contact discovery emas**. Foydalanuvchi `discoverable = false` qilib raqam bo'yicha topilishdan chiqib keta oladi. Haqiqiy private discovery (SGX / OPRF) — roadmap'da (§8).

### (b) Alisa Bob'ga BIRINCHI xabar (X3DH)
```
Alisa: GET /users/{bob}/devices -> Bob'ning BARCHA qurilmalari ro'yxati
Alisa: har qurilma uchun BUNDLE so'raydi (+ O'ZINING boshqa qurilmalari uchun ham — sync)
Server: {identity + signed prekey + bitta one-time prekey}ni beradi
        va o'sha one-time prekey'ni ATOMIK o'chiradi   [one_time_prekeys dan -1]
        (bitta DELETE ... FOR UPDATE SKIP LOCKED ... RETURNING —
         ikki jo'natuvchi bir vaqtda so'rasa ham bir xil kalit ketmaydi;
         pool bo'sh bo'lsa bundle OTK'siz qaytadi — X3DH bunga ruxsat beradi)
Alisa: X3DH bajaradi — bir nechta DH hisoblab, HKDF orqali
       -> SHARED ROOT KEY (Bob bilan umumiy maxfiy)
Alisa: Double Ratchet boshlaydi -> shu xabar uchun MESSAGE KEY
Alisa: xabarni AEAD bilan shifrlaydi -> ENVELOPE (muhrlangan quti)
       (har qabul qiluvchi qurilma uchun ALOHIDA envelope — o'z sessiyasi bilan)
Alisa: envelope'larni serverga yuboradi, har biriga client_message_id (uuid) bilan
```

### (c) Server — yetkazish
```
Server: envelope'ni HAR DOIM avval NAVBATGA yozadi   [message_queue]
        (opaque — server ichini ko'rmaydi)
        UNIQUE (recipient_device_id, client_message_id):
        Alisa tarmoq uzilib RETRY qilsa — dublikat rad etiladi, 200 qaytadi (idempotent)
Bob ONLINE bo'lsa:  server WS orqali darrov PUSH qiladi (queue id bilan)
Bob:                decrypt qilib LOCAL bazaga yozgach -> WS orqali ACK { queue_id }
Server:             ACK KELGACH yozuvni O'CHIRADI (tarix saqlanmaydi)
                    (push qilingach EMAS — socket o'rtada uzilsa xabar yo'qolar edi)
Bob qayta ulansa:   ACK qilinmagan hamma yozuv qayta yuboriladi (klient dedup qiladi)
ACK kelmasa:        TTL (30 kun) tugagach purge
```

### (d) Bob o'qiydi (X3DH — Bob tomoni)
```
Bob: prekey-envelope'ni oladi -> O'ZI ham X3DH bajaradi
     (Alisa yuborgan ephemeral + o'zining identity/signed/one-time kalitlari)
     -> XUDDI O'SHA shared root key (mustaqil chiqaradi!)
Bob: ratchet -> message key -> AEAD decrypt -> plaintext
```
Sehr shu yerda: Alisa va Bob **hech qachon kalitni ochiq yubormasdan**, bir xil maxfiy kalitni mustaqil chiqaradi. Server o'rtada turadi, lekin hech narsani bilmaydi.

### (e) Keyingi xabarlar (ratchet)
```
Endi sessiya o'rnatilgan — prekey KERAK EMAS.
Har xabarda Double Ratchet OLDINGA yuradi -> har xabarga YANGI kalit.
=> Forward secrecy: bugungi kalit sizsa, kechagi xabar ochilmaydi.
```

### (f) Offline / receipt
```
Alisa offline yozsa: xabar telefondagi OUTBOX'da turadi (klient-side)
Internet qaytganda: outbox serverga sync qiladi
Yetkazildi/o'qildi: receipt qaytadi
```

**Diqqat:** har qadam qaysi jadvalga tegishini yuqorida ko'rsatdim — jadvallar **workflowga xizmat qiladi**, aksincha emas.

---

## 4. Struktura — qatlamlar va nega

```
Chittak/
  src/
    server/
      Chittak.Api/            <- HTTP endpointlar (auth, keys, messages, contacts, calls), SignalR hub, DI
      Chittak.Core/           <- domain (device, key, queue) + interfeyslar
      Chittak.Infrastructure/ <- EF Core + Postgres, repolar, SMS, TURN credential, rate-limit
    client/
      Chittak.Client/         <- umumiy klient mantiqi (UI framework'ni bilmaydi)
        Services/
          Crypto/                <- X3DH, DoubleRatchet, Envelope (SENING protokol koding)
          Api/                   <- REST (Refit) + SignalR klient
          Storage/               <- SecureKeyStore, SessionStore, Outbox
        Features/                <- Auth, Chat, Contacts, Calls (ViewModel'lar)
        Platform/                <- ISecureStore, IContactsProvider, IConnectivityMonitor, IDispatcher
      Chittak.Mobile/         <- MAUI ilova: View'lar + Platform/ implementatsiyasi (SecureStorage, Contacts)
      Chittak.Desktop/        <- Avalonia ilova: View'lar + Platform/ implementatsiyasi (Secret Service)
    shared/
      Chittak.Protocol/       <- umumiy DTO: envelope + bundle formati
  tests/
    Chittak.Crypto.Tests/     <- X3DH shared-secret, ratchet, wrong-key, replay
```

**Nega bu qatlamlar:**
- **Core** interfeys beradi, **Infrastructure** uni Postgres bilan bajaradi → domain bazani bilmaydi (almashtirsa bo'ladi, test oson).
- **Crypto** alohida papka — chunki u yurak; **Storage**dan ajratilgan, primitivlar NSec'dan.
- **Client** UI framework'ni bilmaydi → bitta ViewModel MAUI'da ham, Avalonia'da ham ishlaydi; platformaga bog'liq narsa faqat `Platform/` interfeyslari orqali.
- **Protocol** (shared) — envelope/bundle formati klient ham, server ham bir xil tushunishi uchun (server ichini ochmasa ham, formatni biladi).

---

## 5. Entity'lar — har biri NIMA UCHUN, QANDAY

Har jadvalni: *nima saqlaydi → nega bor (bo'lmasa nima buziladi) → qanday ishlatiladi.*

### `users` — odam (telefon shaxsi)
- **Nima:** telefon raqam (E.164, CHECK bilan) = akkaunt; `phone_hash` (SHA-256) — kontakt topish uchun; `discoverable` — raqam bo'yicha topilishga rozilik.
- **Nega:** kimdir bo'lishi kerak; xabar shu shaxsga boradi. `phone_hash` — discovery so'rovida server ochiq raqam emas, hash solishtiradi; `discoverable` — enumeratsiyadan opt-out.
- **Qanday:** ro'yxatdan o'tganda yaratiladi (hash server hisoblaydi); `POST /contacts/discover` shu jadvalga qaraydi.

### `devices` — QURILMA (per-device model)
- **Nima:** bir userning har telefoni alohida yozuv (registration_id, push_token).
- **Nega:** Signal **per-device** — bir odamda bir necha telefon; **har qurilmaning O'Z kalitlari va sessiyalari** bor. Bitta qurilmaga shifrlangan xabar boshqasida ochilmaydi. Bu — multi-device xavfsizligining asosi.
- **Qanday:** har qurilma ro'yxatdan o'tganda yaratiladi (`UNIQUE (user_id, registration_id)`); xabar user'ga emas, uning **qurilmalariga** yuboriladi. `GET /users/{id}/devices` — jo'natuvchi kimlarga shifrlashini shu yerdan biladi. Yangi qurilma qo'shish — mavjud qurilmadan QR orqali "linking" (roadmap, §8).

### `device_identity_keys` — qurilmaning uzoq muddatli PUBLIC identity kaliti
- **Nima:** qurilmaning barqaror kriptografik "pasporti" (public). **Format (qaror):** 64 bayt = Ed25519 public (32, imzo uchun) ‖ X25519 public (32, DH uchun). Signal bitta X25519 kalitni XEdDSA bilan imzolaydi; NSec/BouncyCastle'da XEdDSA yo'q — ikkita standart kalit soddaroq va xavfsiz.
- **Nega:** hamma narsa shunga bog'lanadi — X3DH, imzolar. Bob "bu rostan Alisaning qurilmasimi?" ni shu kalit orqali biladi. Bu o'zgarmasa — sessiya uzluksiz.
- **Qanday:** ro'yxatda bir marta yuklanadi; **har X3DH'da** ishlatiladi. (Maxfiy jufti telefonda, Keystoreda — hech qachon serverga kelmaydi.)

### `signed_prekeys` — o'rta muddatli PUBLIC kalit, identity bilan IMZOlangan
- **Nima:** X25519 public kalit (32 bayt) + uning Ed25519 imzosi (64 bayt, identity'ning Ed25519 qismi bilan).
- **Nega (ikki sabab):**
  1. **Async boshlash:** Bob OFFLINE bo'lsa ham, Alisa uning oldindan qo'ygan signed prekey'i bilan sessiya boshlay oladi — Bob'ni kutmay.
  2. **Imzo = ishonch:** imzo bu prekey rostan Bob'ning identity'sidan kelganini isbotlaydi → o'rtadagi MITM soxta kalit qo'ya olmaydi.
- **Qanday:** yuklanadi, bundle'ga qo'shiladi, X3DH'da ishlatiladi. Vaqti-vaqti bilan **rotatsiya** (gigiena) — joriy + oldingisi saqlanadi, chunki yo'lda bo'lgan prekey-xabar hali eski `key_id`ga ishora qilishi mumkin.

### `one_time_prekeys` — bir martalik PUBLIC kalitlar dastasi (pool)
- **Nima:** har biri **bir marta** ishlatiladigan public kalitlar to'plami.
- **Nega:** har yangi suhbat bittasini iste'mol qiladi → **birinchi handshake'ga qo'shimcha forward secrecy** beradi (har birinchi-aloqa yangi kalit materiali bilan). Signal'ning nozik ustunligi shu.
- **Qanday:** pool yuklanadi; server har bundle so'raganda bittani **atomik beradi + o'chiradi** (bitta `DELETE ... FOR UPDATE SKIP LOCKED ... RETURNING`, aniq SQL — `chittak-db.sql`da); pool kamayganda (< 20) klient **to'ldiradi** (`GET /keys/count`). Pool bo'sh bo'lsa bundle OTK'siz qaytadi.

### `message_queue` — qabul qiluvchi ACK qilguncha OPAQUE ciphertext
- **Nima:** muhrlangan quti (envelope) + `client_message_id` (jo'natuvchi bergan uuid), qabul qiluvchi qurilma **ACK qilguncha** turadi.
- **Nega:** **async yetkazish** — qabul qiluvchi offline bo'lsa ham xabar yo'qolmasin (messenger'ning butun ma'nosi). Server ichini **ko'rmaydi**. `UNIQUE (recipient_device_id, client_message_id)` — jo'natuvchi retry qilsa dublikat tushmaydi.
- **Qanday:** jo'natuvchi qo'yadi (har doim avval DB, keyin push); server WS orqali push qiladi; klient **ACK qilgach** o'chiradi (push qilingach emas); qayta ulanishda ACK'siz yozuvlar qayta yuboriladi; ACK kelmasa TTL purge qiladi.

### `phone_verifications` — OTP kodlari (HASHlangan)
- **Nima:** SMS OTP kodi (ochiq emas — hashlangan), muddati (5 min), urinishlar (max 5), `consumed_at`.
- **Nega:** ro'yxatda telefon egaligini isbotlash. `consumed_at` — bir marta to'g'ri kiritilgan kod muddati tugaguncha **qayta ishlatilmasin**. Rate-limit (3 ta / 10 min / raqam, 10 ta / kun, per-IP) — SMS-bombing va brute-force'ga qarshi; hisoblash `idx_phone_verif` orqali.
- **Qanday:** "OTP yubor"da yaratiladi (limit tekshirilib), "verify"da `consumed_at IS NULL AND expires_at > now() AND attempts < 5` sharti bilan tekshiriladi, muvaffaqiyatda `consumed_at` qo'yiladi; 24 soatdan keyin purge.

### `auth_sessions` — qurilma tokenlari
- **Nima:** refresh token (HASHlangan), muddat.
- **Nega:** qurilmani "kirgan" holatda saqlash (har safar OTP so'ramaslik); API/WS'ni avtorizatsiya qilish.
- **Qanday:** verify'da yaratiladi, har so'rovda ishlatiladi.

### YO'Q jadval: `sessions` / `ratchet_state`
- **Nega YO'Q:** Double Ratchet va sessiya holati **faqat klientda** yashaydi. Agar server sessiya holatini saqlasa — u xabarni **decrypt qila olar edi** → E2EE buzilar edi. Shu "yo'qlik" — ataylab, xavfsizlik uchun.

### YO'Q jadval: `contacts`
- **Nega YO'Q:** kim kim bilan gaplashishini server bilmasligi kerak (metadata privacy). Manzillar kitobi klientda; server discovery so'roviga javob beradi va unutadi.

### YO'Q jadval: `groups` / `group_members`
- **Nega YO'Q (hozircha):** v1 — faqat 1:1. Guruh uchun Sender Keys protokoli kerak (§8).

---

## 6. Uchta "nega" — mustahkamlash uchun

**Nega per-device, per-user emas?**
Bir odamda bir necha qurilma; har qurilma o'z kaliti bilan mustaqil shifrlaydi. Telefon yo'qolsa — faqat o'sha qurilma sessiyasi ketadi, boshqasi xavfsiz. Xabar user'ning **har qurilmasiga** alohida shifrlanadi.

**Nega prekey kerak — "ulanganda kalit almashsak" bo'lmaydimi?**
Chunki qabul qiluvchi sen xabar yozganda **offline** bo'lishi mumkin. Prekey'lar (oldindan qo'yilgan public kalitlar) Alisaga Bob'ni kutmay, muhrlangan quti tuzishga imkon beradi. Bu — async messenger'ning kaliti.

**Nega server sessiyani saqlamaydi?**
Agar saqlasa — decrypt qila olar edi. E2EE'ning ta'rifi: **o'rta (server) o'qiy olmasligi.** Shuning uchun butun sessiya holati klientda, server faqat opaque qutini ko'radi.

**Nega navbatdan "push qilingach" emas, "ACK kelgach" o'chiriladi?**
WS'ga yozildi ≠ telefon oldi. Socket o'rtada uzilsa, push ketgan-u, klient olmagan bo'ladi — yozuv allaqachon o'chirilgan bo'lsa, xabar abadiy yo'qoladi. ACK = "decrypt qildim, local bazaga yozdim" — shundan keyingina o'chirish xavfsiz. Dublikat kelsa klient `client_message_id` bo'yicha tashlab yuboradi.

**Nega jo'natuvchi o'zining boshqa qurilmalariga ham shifrlaydi?**
Aks holda Alisa telefonida yozgan xabari planshetida ko'rinmaydi. Server tarix saqlamagani uchun sync'ni faqat jo'natuvchining o'zi qila oladi — o'z qurilmalariga ham alohida envelope yuborib.

---

## 7. Qo'ng'iroqlar — signaling, media, NAT

```
Alisa -> WS -> Server -> WS -> Bob:   offer / answer / ICE candidate
        (signaling xabarlari ham E2EE envelope ichida — server SDP'ni ko'rmaydi,
         DTLS fingerprint ham ichida => server media kalitini almashtira olmaydi)
Media: WebRTC P2P, DTLS-SRTP (shifrlangan), server orqali O'TMAYDI
```

**NAT / STUN / TURN (bularsiz ko'p telefonlar bir-biriga ulanolmaydi):**
- **STUN** — telefon o'zining ommaviy IP:port'ini biladi (arzon, ko'pchilik hollarda yetadi).
- **TURN** — ikkala tomon ham "yopiq" NAT / mobil operator CGNAT ortida bo'lsa, trafik TURN relay orqali o'tadi. **Shart** — real hayotda 20–30% qo'ng'iroq faqat TURN bilan ishlaydi.
- Server: **coturn** (STUN + TURN). Klient `GET /calls/turn-credentials` orqali qisqa muddatli (1 soat) credential oladi — TURN REST API: `username = expiry:deviceId`, `password = HMAC(secret, username)`. DB'da hech narsa saqlanmaydi.
- TURN faqat SRTP (shifrlangan) paketlarni tashiydi — mediani ko'rmaydi. Ya'ni relay bo'lsa ham E2EE buzilmaydi.

---

## 8. Doira (scope) va keyingi bosqichlar

**v1 (hozirgi dizayn):** telefon/OTP auth, per-device kalitlar, 1:1 E2EE chat (X3DH + Double Ratchet), offline navbat (ACK + TTL), hash bo'yicha kontakt topish, 1:1 qo'ng'iroq (WebRTC + TURN, faqat mobil), multi-device (sync envelope), Linux desktop klient (Avalonia, qo'ng'iroqsiz).

**Ataylab v1'da YO'Q — keyingi bosqichlar:**
1. **Guruh chat — Sender Keys.** Har a'zo o'z "sender key"ini guruhdagi har qurilmaga 1:1 sessiya orqali tarqatadi, keyin xabarni bir marta shifrlaydi. Server tomonida `groups`, `group_members` jadvallari va guruh bo'yicha fan-out kerak. 1:1 sessiyalar tayyor bo'lgach ustiga quriladi.
2. **Sealed sender.** Hozir `message_queue.sender_device_id` server'ga kim kimga yozganini ko'rsatadi. Sealed sender'da jo'natuvchi identifikatori ham envelope ichida shifrlanadi, server faqat qabul qiluvchini biladi (ustun shuning uchun nullable).
3. **Private contact discovery.** Hash yechim brute-force'ga bardosh bermaydi. Yechim: OPRF (server raqamni ko'rmay hash'laydi) yoki SGX enclave (Signal usuli).
4. **Qurilma linking.** Yangi qurilma mavjud qurilmadan QR orqali qo'shiladi; identity kalit **ko'chirilmaydi** — yangi qurilma o'z kalitini yaratadi, eski qurilma uni tasdiqlaydi.
5. **Safety numbers.** Alisa va Bob identity kalitlarini yuzma-yuz solishtira olishi (MITM'ga qarshi oxirgi himoya).
6. **Push orqali uyg'otish.** `devices.push_token` — FCM/APNs orqali faqat "yangi narsa bor" signali, mazmunsiz.
7. **Desktop kengaytmalari.** Desktop'da qo'ng'iroq, Windows/macOS versiyalari (DPAPI / Keychain), kontaktlarni o'z qurilmalari orasida sync qilish (desktop'da manzillar kitobi yo'q).

---

## Xulosa — bir qatorda

**Klient = sandiq + kalit (butun kripto). Server = soqov pochta (relay + ochiq manzillar kitobi). DB = faqat public kalit + muhrlangan navbat.** Har entity shu workflow'ga xizmat qiladi; sessiya holatining serverda **yo'qligi** — dizaynning eng muhim qarori.
