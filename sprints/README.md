# Chittak — Sprint rejasi (TZ shaklida)

Bu hujjat — bir kishi uchun, **o'rganish orqali qurish** rejimida yozilgan. Har sprint = 1 hafta (full-time) yoki 2 hafta (kechqurun/dam olish kunlari). Sprintlar tartibi **ataylab**: avval kripto (yurak), keyin server (relay), keyin telefon (UI), keyin desktop (ikkinchi qurilma), oxirida qo'ng'iroq va qattiqlashtirish. Telefon UI'sini S7'gacha ochmaysan — bu eng ko'p vaqtni yeydigan va eng kam o'rgatadigan qism.

---

## 0. Ish qoidalari (metodologiya)

**Scrumban, solo variant:**

| Qoida | Qanday bajariladi |
|---|---|
| 1 sprint = 1 hafta | Dushanba: sprint issue'larini `Sprint` ustuniga ko'chir. Yakshanba: demo o'zingga, retro 3 savol (nima o'rgandim / nima to'sdi / keyingi hafta nima o'zgaradi) — `docs/retro/YYYY-WW.md` ga 5 qator |
| WIP = 1 | GitHub Projects'da `Doing` ustunida bittadan ko'p karta bo'lmaydi |
| Vertikal kesim | Har sprint oxirida **ishga tushirib ko'rsatsa bo'ladigan** narsa bo'ladi (test bo'lsa ham) |
| DoD (Definition of Done) | ✅ kod `main`da · ✅ test yashil (CI) · ✅ dizayn o'zgargan bo'lsa `chittak-arxitektura.md` yangilangan · ✅ muhim qaror bo'lsa ADR yozilgan |
| Sprint cho'zilmaydi | Bitmagan ish keyingi sprintga o'tadi, sprint uzaytirilmaydi. Bu — baholash ko'nikmasini o'rgatadi |
| Spike = vaqt chegarasi | Noma'lum narsani tekshirish (masalan "NSec Android'da ishlaydimi?") — max 4 soat, natija: ADR |

**Vositalar:** GitHub Issues + Projects (Kanban: `Backlog → Sprint → Doing → Done`), har sprint = GitHub Milestone. Issue nomi: `S3-04: WS ACK → delete`. ADR'lar: `docs/adr/0001-nomi.md`.

**O'rganish rejimi:** har sprintda "O'rganiladi" ro'yxati bor. Kod yozishdan **oldin** resurslarni o'qish uchun sprintning birinchi kunini ajrat (Dushanba = o'qish kuni). Bu vaqt "isrof" emas — ikkinchi kundan tez yurasan.

**Umumiy resurslar (butun loyiha davomida):**
- GitHub Projects: https://docs.github.com/en/issues/planning-and-tracking-with-projects
- ADR nima: https://adr.github.io/
- Kriptografiya asoslari (bepul, qisqa): https://www.crypto101.io/
- Kitob: *Real-World Cryptography* (David Wong) — 1-, 2-, 6-boblari (hash, AEAD, key exchange) S1'gacha o'qilsa juda foydali
- Kitob: *Serious Cryptography* (J-P. Aumasson) — chuqurroq alternativ
- Signal spetsifikatsiyalari (asosiy manba): https://signal.org/docs/
- libsignal (etalon implementatsiya, Rust): https://github.com/signalapp/libsignal
- .NET testing: https://learn.microsoft.com/dotnet/core/testing/

---

## Sprintlar

| Sprint | Mavzu |
|---|---|
| [S00](S00-skelet.md) | Muhit va skelet (3–4 kun) |
| [S01](S01-x3dh.md) | Kripto primitivlar va X3DH |
| [S02](S02-double-ratchet.md) | Double Ratchet |
| [S03](S03-envelope-konsol-demo.md) | Envelope formati, sessiya menejeri, serversiz demo |
| [S04](S04-server-auth.md) | Server: Auth (OTP, JWT, refresh, rate-limit) |
| [S05](S05-server-kalitlar.md) | Server: Qurilmalar va kalitlar kitobi |
| [S06](S06-server-relay.md) | Server: Relay (navbat, SignalR, ACK, purge) → 🏁 MILESTONE 1 |
| [S07](S07-maui-auth.md) | MAUI: kirish, kalitlar, xavfsiz saqlash |
| [S08](S08-maui-baza-kontaktlar.md) | MAUI: shifrlangan lokal baza, sessiyalar, kontaktlar |
| [S09](S09-maui-chat.md) | MAUI: chat → 🏁 MILESTONE 2 |
| [S10](S10-multi-device-push.md) | Multi-device sync, rotatsiya, push |
| [S11](S11-desktop-avalonia.md) | Desktop: Avalonia klient (Linux) |
| [S12](S12-turn-signaling.md) | Qo'ng'iroq 1: coturn, TURN credential, signaling |
| [S13](S13-webrtc-media.md) | Qo'ng'iroq 2: WebRTC media (audio) |
| [S14](S14-xavfsizlik.md) | Xavfsizlikni qattiqlashtirish |
| [S15](S15-release.md) | Release: CI/CD, beta, monitoring |

Har sprint fayli bir xil tuzilishda: **Maqsad → Nega hozir → O'rganiladi → Vazifalar (ID + qabul mezoni) → Demo → Resurslar → Tuzoqlar.** Vazifa ID'lari (`S6-04`) — GitHub issue nomlari.

---

## Roadmap (v1'dan keyin — Backlog)

| Mavzu | Nima | Resurs |
|---|---|---|
| Guruhlar | Sender Keys: har a'zo sender key'ini 1:1 sessiya orqali tarqatadi; server `groups`, `group_members`; fan-out | https://signal.org/blog/signal-private-group-system/ · libsignal `sender_keys` |
| Sealed sender | Jo'natuvchi identifikatori ham shifrlanadi; server faqat qabul qiluvchini biladi | https://signal.org/blog/sealed-sender/ |
| Private contact discovery | OPRF yoki SGX; hash yechim o'rniga | https://signal.org/blog/private-contact-discovery/ |
| Qurilma linking (QR) | Yangi qurilma identity kalitini **ko'chirmay**, tasdiq orqali qo'shiladi | Sesame spec |
| Media xabarlar | Fayl: klientda shifrlash (random kalit) → S3-compatible storage (opaque) → kalit xabar ichida | Signal attachments dizayni |
| Disappearing messages | Timer klientda, receipt bilan sinxron | — |
| Post-quantum (PQXDH) | X3DH → PQXDH (Kyber/ML-KEM) — Signal 2023'dan | https://signal.org/docs/specifications/pqxdh/ |
| Desktop: Windows / macOS | `ISecureStore` — DPAPI (Windows), Keychain (macOS); paketlash (MSIX / `.app`) | https://learn.microsoft.com/dotnet/api/system.security.cryptography.protecteddata |
| Desktop qo'ng'iroq | S13 stack desktop'da (SIPSorcery + mikrofon/dinamik) | https://github.com/sipsorcery-org/sipsorcery |
| Kontaktlar sync | Kontakt ro'yxati o'z qurilmalari orasida E2EE sync (desktop'da manzillar kitobi yo'q) | Sesame spec |
| Ko'p server instansiyasi | SignalR Redis backplane, connection registry Redis'da | https://learn.microsoft.com/aspnet/core/signalr/redis-backplane |

---

## Vaqt jadvali (taxminiy)

```
S0  ▓▓▓        3–4 kun    skelet
S1  ▓▓▓▓▓▓▓    1 hafta    X3DH
S2  ▓▓▓▓▓▓▓    1 hafta    Double Ratchet
S3  ▓▓▓▓▓▓▓    1 hafta    envelope + konsol demo
S4  ▓▓▓▓▓▓▓    1 hafta    auth
S5  ▓▓▓▓▓▓▓    1 hafta    kalitlar
S6  ▓▓▓▓▓▓▓    1 hafta    relay            🏁 M1: konsol ↔ konsol
S7  ▓▓▓▓▓▓▓    1 hafta    MAUI auth
S8  ▓▓▓▓▓▓▓    1 hafta    MAUI baza + kontakt
S9  ▓▓▓▓▓▓▓    1 hafta    MAUI chat        🏁 M2: telefon ↔ telefon
S10 ▓▓▓▓▓▓▓    1 hafta    multi-device + push
S11 ▓▓▓▓▓▓▓    1 hafta    desktop (Avalonia)
S12 ▓▓▓▓▓▓▓    1 hafta    TURN + signaling
S13 ▓▓▓▓▓▓▓▓▓▓ 1–2 hafta  WebRTC media     (xavfli)
S14 ▓▓▓▓▓▓▓    1 hafta    xavfsizlik
S15 ▓▓▓▓▓▓▓    1 hafta    release          🏁 M3: beta
                ≈ 16–17 hafta full-time / 8–9 oy part-time
```

Eng muhim eslatma: **S6 (M1) ga yetib bor.** Undan keyin loyiha "tashlab qo'yilsa" ham — sen X3DH, Double Ratchet, ASP.NET Core relay, Postgres concurrency'ni haqiqiy kodda o'rgangan bo'lasan. Bu — o'z-o'zidan katta natija.
