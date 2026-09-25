# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Loyiha holati

**Chittak** — Signal protokoli (X3DH + Double Ratchet) asosidagi E2EE messenjer. Hozir **S00 (skelet) bosqichi**: server skeleti bor (8 entity, EF konfiguratsiyalar, `Initial` migratsiya — kontraktga to'liq mos); `Chittak.Protocol`, Mobile, Desktop — bo'sh shablonlar; kripto kodi va testlar hali yo'q.

Hujjatlar (hammasi o'zbek tilida, **lotin alifbosida**):
- `chittak-arxitektura.md` — "nima uchun shunday" hujjati: 3 komponent, uchidan-uchiga workflow, har jadvalning sababi, roadmap. Dizayn o'zgarsa **shu fayl ham yangilanadi** (DoD qoidasi).
- `chittak-db.sql` — server sxemasining **kontrakti**. Sxema EF Core code-first bilan yaratiladi, lekin bu fayl etalon bo'lib qoladi (test bilan solishtiriladi, `sprints/S00-skelet.md` → S0-05).
- `sprints/README.md` — ish qoidalari (Scrumban solo, WIP=1, DoD, ADR), sprint indeksi; `sprints/S00..S15-*.md` — har sprint TZ shaklida (vazifa ID'lari `S6-04` = GitHub issue nomlari).

## Buzilmaydigan tamoyil

Server — "soqov pochta": faqat **public** kalitlar, **opaque** ciphertext navbati, auth metadata. Serverda hech qachon: plaintext, maxfiy kalit, ratchet/sessiya holati, yetkazilgan xabar tarixi, kontaktlar ro'yxati. Shuning uchun `sessions`, `ratchet_state`, `contacts` jadvallari **ataylab yo'q** — qo'shishni taklif qilmang. Butun kripto klientda (`Chittak.Protocol`).

## Qabul qilingan dizayn qarorlari (ADR'larga yoziladi, qayta muhokama qilinmaydi)

| Qaror | Mazmuni |
|---|---|
| Identity kalit | Ed25519 (imzo) + X25519 (DH), public qismi 64 bayt `Ed25519 ‖ X25519`; DB'da `CHECK octet_length = 64`. XEdDSA ishlatilmaydi |
| Real-time | SignalR ustida WebSocket (MessagePack), transport majburan WS; xom WebSocket emas |
| OTK berish | Faqat bitta SQL: `DELETE ... WHERE id = (SELECT ... FOR UPDATE SKIP LOCKED LIMIT 1) RETURNING` — EF `Remove` bilan emas |
| Yetkazish | At-least-once: avval DB'ga INSERT, keyin push; yozuv faqat klient **ACK** qilgach o'chiriladi; `UNIQUE (recipient_device_id, client_message_id)` idempotency |
| Kontakt topish | `users.phone_hash` (SHA-256, E.164) + `discoverable`; server so'rovni saqlamaydi; hash brute-force'ga bardosh bermasligi hujjatda halol yozilgan |
| Klient UI | Mobil — .NET MAUI (Linux'da faqat Android build), desktop — Avalonia (MAUI'da Linux target yo'q). Mantiq umumiy `Chittak.Client`da (UI framework'ni bilmaydi), View'lar ikki marta. Desktop = alohida qurilma, v1'da qo'ng'iroqsiz. Sprint: S11 |
| Kripto kutubxona | NSec (libsodium); MAUI'da ishlamasa BouncyCastle — S0-08 spike hal qiladi |
| Struktura | `src/server/Chittak.{Domain,Application,Infrastructure,Server}`, `src/client/Chittak.{Client,Mobile,Desktop}` (umumiy mantiq, MAUI, Avalonia), `src/shared/Chittak.Protocol`, `tests/Chittak.{UnitTests,IntegrationTests}`. Server → Infrastructure → Application → Domain; Domain/Application Infrastructure'ni bilmaydi; Protocol'da interfeyslar, implementatsiya klientda. `Chittak.Client`da `Microsoft.Maui.*`/`Avalonia.*` reference bo'lmaydi |

## Amaldagi struktura

Clean Architecture (4 qatlam), barcha loyiha va namespace'lar `Chittak.*`:

- `src/server/Chittak.Domain` — entity'lar (`*Entity`), enum'lar. Hech narsaga bog'liq emas.
- `src/server/Chittak.Application` → Domain. `IApplicationDbContext`, PediatR, FluentValidation, Mapperly.
- `src/server/Chittak.Infrastructure` → Application. `ApplicationDbContext`, `Data/Configurations/` (har entity'ga bitta), `Migrations/`, snake_case naming convention.
- `src/server/Chittak.Server` → Infrastructure. Minimal API endpoint'lar, OpenTelemetry, health checks, `Dockerfile`.
- `src/shared/Chittak.Protocol` — hozircha bo'sh. `src/client/Chittak.{Mobile,Desktop}` — shablon; `Chittak.Client` hali yaratilmagan.
- `tests/Chittak.{UnitTests,IntegrationTests}` — hozircha testsiz; integratsiya testlari Testcontainers (Docker) ishlatadi.

## Build va ishga tushirish

```bash
dotnet build Chittak.slnx                    # Mobile slnx'da YO'Q (CI'da maui workload yo'q) — S7-01'da alohida job
dotnet test Chittak.slnx                     # integratsiya testlari uchun Docker kerak
docker compose up -d postgres                # port .env'dagi POSTGRES_PORT (54123)
dotnet run --project src/server/Chittak.Server   # Development'da Database:AutoMigrate=true
dotnet ef migrations add <Nom> -p src/server/Chittak.Infrastructure -s src/server/Chittak.Server -o Migrations
dotnet run --project src/client/Chittak.Desktop
dotnet build src/client/Chittak.Mobile -t:Run -f net10.0-android   # USB telefon; Linux'da faqat Android
```

- `Directory.Build.props`: `TreatWarningsAsErrors`, `latest-recommended` analyzer'lar — har warning build'ni yiqitadi.
- `Directory.Packages.props` (CPM): versiyalar faqat shu yerda (`PackageVersion`), csproj'da faqat `PackageReference Include` versiyasiz. Shablonlar (`dotnet new maui/avalonia.*`) versiyani csproj'ga yozadi → `NU1008`.
- MAUI csproj'da `<TargetFramework></TargetFramework>` ataylab bo'sh: umumiy `net10.0` meros qolsa `TargetFrameworks` jim e'tiborsiz qoladi.

## EF konfiguratsiya qoidalari

- Kontrakt etalon: jadval/ustun/CHECK/indeks `chittak-db.sql` bilan 1:1. Har konfiguratsiyada `ToTable("<kontrakt nomi>")` aniq yoziladi, kontraktdagi `idx_*` nomlari `HasDatabaseName` bilan saqlanadi.
- Kalitlar kontraktdagidek: `users`/`devices`/`auth_sessions` — `Guid` (v7); prekey'lar, `message_queue`, `phone_verifications` — `long` + `UseIdentityAlwaysColumn()`; `device_identity_keys` — PK `device_id`. `updated_at` yo'q.
- `message_queue.sender_device_id` — ataylab FK'siz (navigatsiya yo'q), yuboruvchi o'chsa ham xabar yetkaziladi.
- DB default'li `bool` (`discoverable DEFAULT true`) — `HasSentinel(true)` bilan, aks holda EF `false`ni yubormaydi.
- `devices.platform` — enum, bazada kichik harfli `text` (`android`, `linux`, ...) + CHECK.
- Migratsiyani o'zgartirgach kontrakt bilan solishtiring (quyida).

## Sxemani tekshirish

`chittak-db.sql` haqiqiy Postgres'da tekshiriladi. Mavjud konteynerlarga tegmang (5432/5433 band) — vaqtinchalik konteyner ishlating:

```bash
docker run --rm -d --name chittak-schema-test -e POSTGRES_PASSWORD=t -p 127.0.0.1:55432:5432 postgres:17-alpine
until docker exec chittak-schema-test pg_isready -U postgres -q; do sleep 1; done
PGPASSWORD=t psql -h 127.0.0.1 -p 55432 -U postgres -v ON_ERROR_STOP=1 -q -f chittak-db.sql
docker rm -f chittak-schema-test
```

Migratsiya ↔ kontrakt: `dotnet ef migrations script -p src/server/Chittak.Infrastructure -s src/server/Chittak.Server -o ef.sql` (avval build — `--no-build` yangi migratsiyani ko'rmaydi). Shu konteynerda ikki baza yarating (`contract`, `ef`), birinchisiga `chittak-db.sql`, ikkinchisiga `ef.sql`ni qo'llang va har ikkisidan `information_schema.columns` (tur, nullable, default, identity), `pg_constraint` (FK + delete rule, CHECK ta'rifi), `pg_index` (ustunlar, tartib, unique) ro'yxatini tartiblangan matn qilib `diff` qiling. Constraint/indeks **nomlari** farq qiladi (EF `ck_*`/`ix_*`) — bu kutilgan; ta'riflar bir xil bo'lishi shart. Bo'sh chiqish = mos emas, so'rov xato bo'lishi mumkin — qator sonini tekshiring. S0-05 shu mantiqni testga aylantiradi.

## Hujjat konventsiyalari

- O'zbek tili, **faqat lotin alifbosi** — bitta ham kirill harfi bo'lmasin (avval aralash bo'lgan, tozalangan). Tekshirish: `grep -rnP '[\x{0400}-\x{04FF}]' --include=*.md --include=*.sql .`
- `chittak-db.sql` ichidagi izohlar inglizcha, hujjatlar o'zbekcha — shunday qoladi.
- Har sprint fayli bir xil shablonda: Maqsad → Nega hozir → O'rganiladi → Vazifalar (jadval, ID + qabul mezoni) → Demo → Resurslar → Tuzoqlar; yuqori/pastda navigatsiya havolalari. Yangi sprint/bo'lim qo'shsangiz `sprints/README.md` jadvalini ham yangilang.
- Havolalar qo'shsangiz ishlashini tekshiring (`curl -sIL -o /dev/null -w '%{http_code}'`).
- Loyiha bir kishi tomonidan **o'rganish maqsadida** qurilmoqda: tayyor yechim berishdan oldin sababini tushuntiring, tavsiya bering, lekin qarorni foydalanuvchiga qoldiring.
