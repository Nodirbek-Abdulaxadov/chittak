# chittak

[![Build](https://github.com/Nodirbek-Abdulaxadov/chittak/actions/workflows/build.yml/badge.svg)](https://github.com/Nodirbek-Abdulaxadov/chittak/actions/workflows/build.yml)

Signal protokoli (X3DH + Double Ratchet) asosidagi E2EE messenjer. Butun kriptografiya klientda;
server — "soqov pochta" (relay + ochiq kalitlar kitobi), bazada faqat public kalitlar va opaque navbat.

**Stack:** .NET MAUI (mobil) · Avalonia (desktop, Linux) · ASP.NET Core (server) · PostgreSQL · WebRTC + coturn (qo'ng'iroq)

- [chittak-arxitektura.md](chittak-arxitektura.md) — arxitektura, workflow, har jadvalning "nega"si
- [chittak-db.sql](chittak-db.sql) — server sxemasi (PostgreSQL 13+)
- [sprints/](sprints/README.md) — 16 sprintlik reja (TZ shaklida), har sprint alohida fayl, resurslar bilan
- [learn/](learn/README.md) — o'rganish uchun ishga tushiriladigan o'yinchoq misollar (DH → X3DH → Double Ratchet) va mashqlar
- [resources/](resources/README.md) — X3DH / Double Ratchet'ni kod orqali o'rganish: tanlangan repolar, aniq fayllar, litsenziya ogohlantirishi

## Ishga tushirish

Kerak: .NET 10 SDK, Docker.

```bash
cp .env.example .env                              # lokal Postgres sozlamalari
docker compose up -d postgres                     # Postgres :54123
dotnet run --project src/server/Chittak.Server    # Development'da migratsiya avtomatik qo'llanadi
```

Tekshirish: `dotnet test Chittak.slnx` (Docker kerak — sxema kontrakti testi Testcontainers'da ishlaydi).
Klientlar: `dotnet run --project src/client/Chittak.Desktop` · mobil (Android, USB telefon): `dotnet build src/client/Chittak.Mobile -t:Run -f net10.0-android` — [maui-android workload](sprints/S00-skelet.md) kerak.

## Loyihalar xaritasi

```
src/server/  Chittak.Server → Chittak.Infrastructure → Chittak.Application → Chittak.Domain
src/shared/  Chittak.Protocol    X3DH, Double Ratchet, envelope — butun kripto shu yerda (klientda ishlaydi)
src/client/  Chittak.Client      Mobile va Desktop uchun umumiy mantiq (UI framework'siz)
             Chittak.Mobile      .NET MAUI (Android)
             Chittak.Desktop     Avalonia (Linux)
tests/       Chittak.UnitTests, Chittak.IntegrationTests (API + sxema kontrakti)
docs/adr/    arxitektura qarorlari — nima uchun shunday
```

Batafsil: [ADR'lar](docs/adr/README.md) · [arxitektura](chittak-arxitektura.md).
