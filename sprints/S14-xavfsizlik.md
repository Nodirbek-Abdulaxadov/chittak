[← S13](S13-webrtc-media.md) · [Reja va qoidalar](README.md) · [S15 →](S15-release.md)

# Sprint 14 — Xavfsizlikni qattiqlashtirish

**Maqsad:** o'zingga "hujumchi" bo'lib qarash: threat model, safety numbers, cheklovlar, log tozaligi, bog'liqliklar auditi.

**Nega hozir:** hamma funksiya joyida — endi butun tizimga xavfsizlik nuqtai nazaridan qarash mumkin.

**O'rganiladi:** threat modeling (STRIDE), safety numbers (fingerprint), secure coding review, dependency audit, mobil tahlil vositalari (MobSF), server hardening.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S14-01 | `docs/security/threat-model.md`: aktivlar, hujumchilar (server egasi, tarmoq MITM, o'g'irlangan telefon, o'g'irlangan / umumiy kompyuter (desktop), yomon niyatli foydalanuvchi), har biri uchun nima himoyalangan / nima **emas** (metadata!) | Yozilgan, halol |
| S14-02 | Safety numbers: ikkala identity public kalitidan 60 xonali raqam (Signal usuli: SHA-512 iteratsiya) + QR; UI'da "Xavfsizlik raqamini tekshirish" ekrani, QR skan → `trusted=verified` | Ikki qurilmada raqam bir xil; QR skan ishlaydi |
| S14-03 | Identity o'zgarganda (S8-04) — `verified` bo'lsa qattiq ogohlantirish, yuborish bloklanadi to qabul qilinguncha | Qo'lda test |
| S14-04 | Server: barcha endpointlar uchun rate-limit jadvali `docs/security/rate-limits.md`; `POST /messages`ga qo'shimcha: kunlik limit | Testlar |
| S14-05 | Server log auditi: grep bo'yicha `phone`, `token`, `envelope` loglarda yo'q; request logging'da body o'chirilgan | Skript CI'da |
| S14-06 | Metadata minimallash: `last_seen_at` soatgacha; `presence` faqat kontaktlarga (server kontaktni bilmaydi → faqat "so'ragan" qurilmaga, rate-limit bilan); `sender_device_id` — sealed sender v2 uchun ADR | ADR `0010-metadata-policy.md` |
| S14-07 | Klient: screenshot bloklash opsiyasi (Android `FLAG_SECURE`), ilova fonda bo'lganda preview yashirish, PIN/biometrik qulf (ixtiyoriy) | — |
| S14-08 | Bog'liqliklar auditi: `dotnet list package --vulnerable`, Dependabot yoqish | CI'da |
| S14-09 | MobSF bilan APK statik tahlil — topilmalar bo'yicha issue'lar | Hisobot `docs/security/mobsf-YYYY-MM.md` |
| S14-10 | Server hardening: HTTPS only, HSTS, JWT secret env'dan, Postgres foydalanuvchisi minimal huquq, `docker-compose.prod.yml` | Checklist |
| S14-11 | Kod review checklist `docs/security/review-checklist.md` — kripto tegadigan har PR uchun | Yozilgan |
| S14-12 | (ixtiyoriy, kuchli) Bitta do'stingdan/hamkasbdan "buzib ko'r" — 1 kun, topilmalar issue | — |
| S14-13 | Desktop auditi: `~/.local/share/chittak` ruxsatlari (`0700`/`0600`), maxfiy kalitlar faqat Secret Service'da, log va bildirishnomalarda plaintext yo'q, core dump o'chirilgan | Checklist `docs/security/desktop.md` |

## Demo
Threat model hujjati; ikki telefon safety number solishtiradi; `dotnet list package --vulnerable` toza.

## Resurslar
- Signal safety numbers: https://signal.org/blog/safety-number-updates/
- Signal sealed sender (v2 uchun tushunish): https://signal.org/blog/sealed-sender/
- OWASP MASVS + MASTG (test qo'llanma): https://mas.owasp.org/
- MobSF: https://github.com/MobSF/Mobile-Security-Framework-MobSF
- STRIDE threat modeling: https://learn.microsoft.com/azure/security/develop/threat-modeling-tool-threats
- OWASP API Security Top 10: https://owasp.org/API-Security/
- ASP.NET Core security best practices: https://learn.microsoft.com/aspnet/core/security/
- Android `FLAG_SECURE`: https://developer.android.com/reference/android/view/WindowManager.LayoutParams#FLAG_SECURE
- `dotnet list package --vulnerable`: https://learn.microsoft.com/nuget/concepts/auditing-packages

## Tuzoqlar
- Threat model'da "server hamma narsani ko'ra olmaydi" deb yozma — server **kim kimga qachon** yozganini ko'radi (sealed sender'gacha). Halollik — hujjatning qiymati.
- Safety number'ni faqat identity kalitidan hisobla, `deviceId` yoki telefon qo'shma (spec'ga qara).

---

---
[← S13](S13-webrtc-media.md) · [Reja va qoidalar](README.md) · [S15 →](S15-release.md)
