[← Reja](README.md) · [Reja va qoidalar](README.md) · [S01 →](S01-x3dh.md)

# Sprint 0 — Muhit va skelet (3–4 kun)

**Maqsad:** bo'sh, lekin *build bo'ladigan va CI'dan o'tadigan* solution; Postgres bir buyruq bilan ko'tariladi; sxema migratsiya sifatida qo'llanadi.

**Nega hozir:** keyingi 14 hafta davomida "muhit bilan urishish" bo'lmasligi uchun. Skelet bir marta to'g'ri qurilsa, har sprint faqat mazmunga ketadi.

**O'rganiladi:** .NET solution tuzilishi (Clean Architecture engil variant), `Directory.Build.props`, `Directory.Packages.props` (central package management), Docker Compose, EF Core migrations, GitHub Actions.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S0-01 | `.sln` + loyihalar: `Chittak.Protocol` (shared, netstandard2.1/net9), `Chittak.Core`, `Chittak.Infrastructure`, `Chittak.Api`, `Chittak.Client` (umumiy klient kutubxona, net10.0, hozircha bo'sh), `Chittak.Mobile` (MAUI, hozircha bo'sh), `Chittak.Desktop` (Avalonia, hozircha bo'sh), `tests/Chittak.Protocol.Tests`, `tests/Chittak.Api.Tests` | `dotnet build` xatosiz. Loyihalar orasidagi bog'liqlik: Api→Infrastructure→Core; Core→Protocol; Mobile→Client→Protocol; Desktop→Client. **Core Infrastructure'ni bilmaydi, Client UI framework'ni bilmaydi** |
| S0-02 | Central package management (`Directory.Packages.props`), `Directory.Build.props`da `Nullable=enable`, `TreatWarningsAsErrors=true`, `ImplicitUsings` | Barcha csproj'larda versiya yo'q, faqat markazda |
| S0-03 | `docker-compose.yml`: postgres:17 (port 5432, volume), keyinchalik coturn uchun joy | `docker compose up -d` → `psql` bilan ulanish |
| S0-04 | EF Core: `ChittakDbContext`, entity'lar `chittak-db.sql`ga 1:1 mos, `Initial` migration | `dotnet ef database update` → `\dt` da 8 jadval. Migration SQL'i `chittak-db.sql` bilan solishtirilgan (CHECK, UNIQUE, indekslar bor) |
| S0-05 | `chittak-db.sql` "kontrakt" sifatida qoladi: `tests/Chittak.Api.Tests/SchemaContractTests.cs` — Testcontainers'da toza Postgres'ga (a) sql fayl, (b) EF migration qo'llanib, `information_schema` bo'yicha jadval/ustun ro'yxati bir xilligi tekshiriladi | Test yashil |
| S0-06 | GitHub Actions: `build-test.yml` — push va PR'da `dotnet build` + `dotnet test` (Testcontainers Docker service bilan) | Badge README'da yashil |
| S0-07 | `docs/adr/0001-solution-structure.md`, `docs/adr/0002-ef-core-code-first-with-sql-contract.md` | ADR'lar yozilgan |
| S0-08 | **Spike (4 soat):** NSec (libsodium) Android emulator + iOS simulatorda ishlaydimi? Bo'sh MAUI ilovada `X25519` keypair generatsiya; xuddi shu Linux'da bo'sh Avalonia ilovada. Ishlamasa — BouncyCastle (pure C#) | ADR `0003-crypto-library.md`: tanlov + sabab |
| S0-09 | README: qanday ishga tushirish (3 buyruq), loyihalar xaritasi | Yangi odam 10 daqiqada ko'tara oladi |
| S0-10 | ADR `0011-client-ui-maui-avalonia.md`: mobil — MAUI, desktop — Avalonia (MAUI'da Linux target yo'q), mantiq umumiy `Chittak.Client`da, View'lar ikki marta; iOS faqat Mac bilan | ADR |

## Demo
`docker compose up -d && dotnet ef database update && dotnet test` — hammasi yashil, CI yashil.

## Resurslar
- Central package management: https://learn.microsoft.com/nuget/consume-packages/central-package-management
- EF Core migrations: https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- Npgsql EF provider: https://www.npgsql.org/efcore/
- Testcontainers .NET: https://dotnet.testcontainers.org/
- Docker Compose: https://docs.docker.com/compose/
- GitHub Actions .NET: https://docs.github.com/actions/use-cases-and-examples/building-and-testing/building-and-testing-net
- NSec: https://nsec.rocks/ · BouncyCastle C#: https://github.com/bcgit/bc-csharp
- MAUI o'rnatish: https://learn.microsoft.com/dotnet/maui/get-started/installation
- Avalonia boshlash (`dotnet new install Avalonia.Templates`, `avalonia.mvvm` shabloni): https://docs.avaloniaui.net/

## Tuzoqlar
- MAUI workload o'rnatish (Android SDK, Xcode) bir kunni yeyishi mumkin — S0'da qilib qo'y, S7'da emas.
- iOS build faqat macOS'da. Mac yo'q bo'lsa — v1 Android-only, ADR'ga yoz. Bu uyat emas, qaror.
- Linux'da MAUI faqat Android uchun (`maui-android` workload, `sudo` bilan — SDK tizim papkasida bo'lsa). Linux desktop target MAUI'da yo'q — shuning uchun desktop Avalonia'da.
- `Directory.Build.props`dagi `TargetFramework=net10.0` MAUI loyihasiga meros o'tadi va csproj'dagi `TargetFrameworks`ni (`net10.0-android`) **jim** bekor qiladi. MAUI csproj'da `<TargetFramework></TargetFramework>` bilan bo'shatiladi. Tekshirish: `dotnet msbuild -getProperty:IsCrossTargetingBuild` → `true`.
- `dotnet new maui` / `dotnet new avalonia.mvvm` versiyalarni csproj'ga yozadi → CPM bilan `NU1008`. Versiyalar `Directory.Packages.props`ga ko'chiriladi, csproj'da faqat nom qoladi.
- MAUI loyihasi `.slnx`da bo'lsa CI ham `maui-android` workload'ini talab qiladi — Mobile uchun alohida CI job (S7-01).
- Postgres'da `gen_random_uuid()` 13+ da o'rnatilgan — `pgcrypto` kerak emas.

---

---
[← Reja](README.md) · [Reja va qoidalar](README.md) · [S01 →](S01-x3dh.md)
