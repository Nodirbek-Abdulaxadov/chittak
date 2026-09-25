[← S13](S13-xavfsizlik.md) · [Reja va qoidalar](README.md) · [Roadmap →](README.md#roadmap-v1dan-keyin--backlog)

# Sprint 14 — Release: CI/CD, beta, monitoring

**Maqsad:** server VPS'da, ilova ichki beta'da (Google Play Internal Testing / TestFlight), xatolar ko'rinadi, deploy bir buyruq.

**Nega hozir:** "ishlaydi" ≠ "ishlatsa bo'ladi". Real foydalanuvchi (do'stlar) bilan haqiqiy muammolar chiqadi.

**O'rganiladi:** Docker image, GitHub Actions deploy, reverse proxy (Caddy — avtomatik TLS), health checks, OpenTelemetry/loglar, mobil release signing, store jarayonlari.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S14-01 | `Dockerfile` (multi-stage) `Chittak.Api`; `docker-compose.prod.yml`: api + postgres + coturn + caddy | VPS'da `docker compose up -d` |
| S14-02 | Caddy: domen, avtomatik Let's Encrypt, WS proxy, coturn uchun TLS sert (5349) | `https://api.chittak.uz/health` 200 |
| S14-03 | GitHub Actions: `main`ga merge → image build → GHCR → SSH deploy → migratsiya (`dotnet ef database update` yoki migration bundle) | Deploy 5 daqiqa, rollback yo'li hujjatlangan |
| S14-04 | Health checks (`/health`: DB, coturn port), `/metrics` (Prometheus format) — ixtiyoriy Grafana | — |
| S14-05 | Serilog → fayl + (ixtiyoriy) Seq/Loki; Sentry yoki oddiy exception log — **telefon/token'siz** | Xato ko'rinadi |
| S14-06 | Postgres backup (kunlik `pg_dump`, faqat public kalitlar va auth — navbat backup'ga kirmasa ham bo'ladi) | Restore sinovdan o'tgan |
| S14-07 | Android: release signing, `AAB`, Google Play Internal Testing | 5 do'st o'rnatdi |
| S14-08 | iOS (Mac bo'lsa): sertifikatlar, TestFlight | — |
| S14-09 | Ilova ichida crash reporting (Sentry MAUI) — metadata minimal | — |
| S14-10 | `CHANGELOG.md`, `docs/ops/runbook.md` (server yiqilsa nima qilish, sertifikat yangilash, coturn secret rotatsiya) | Yozilgan |
| S14-11 | Beta feedback → Backlog; v1.1 rejasi (qo'ng'iroq iOS, guruhlar — Sender Keys, sealed sender, QR linking) | Backlog to'ldirilgan |

## Demo
Do'stlaringiz telefonida Chittak; sizning telefoningizda "deploy" bir push bilan; Grafana/loglarda tirik trafik.

## Resurslar
- Docker .NET: https://learn.microsoft.com/dotnet/core/docker/build-container
- Caddy: https://caddyserver.com/docs/
- EF migration bundles (prod uchun): https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#bundles
- ASP.NET Core health checks: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
- OpenTelemetry .NET: https://opentelemetry.io/docs/languages/dotnet/
- MAUI Android publish: https://learn.microsoft.com/dotnet/maui/android/deployment/
- MAUI iOS publish: https://learn.microsoft.com/dotnet/maui/ios/deployment/
- Sentry .NET MAUI: https://docs.sentry.io/platforms/dotnet/guides/maui/
- Google Play Internal testing: https://support.google.com/googleplay/android-developer/answer/9845334

## Tuzoqlar
- coturn'ga ommaviy IP va UDP portlar (49152–65535) ochiq bo'lishi kerak — firewall'da unutiladi.
- Migratsiyani deploy'da avtomatik ishlatish — backup'siz emas.
- Beta'da do'stlar "ishlamayapti" deydi, log'da telefon raqam yo'q — foydalanuvchi ID (uuid) bilan qidirishni o'rgan; bu ataylab.

---

---
[← S13](S13-xavfsizlik.md) · [Reja va qoidalar](README.md) · [Roadmap →](README.md#roadmap-v1dan-keyin--backlog)
