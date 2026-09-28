# 0002. EF Core code-first + SQL kontrakt

- **Holat:** Qabul qilindi (2026-09-25)
- **Sprint:** S0-04, S0-05, S0-07

## Kontekst

Server sxemasi xavfsizlik tamoyilining bir qismi: nima saqlanadi va nima **ataylab saqlanmaydi** (`sessions`, `ratchet_state`, `contacts` yo'q). Buni o'qiladigan SQL'da ko'rish kerak. Shu bilan birga migratsiyalar, tiplangan so'rovlar va DI uchun EF Core qulay.

## Qaror

- `chittak-db.sql` — **etalon (kontrakt)**. U o'qish, muhokama va ko'rib chiqish uchun; izohlari inglizcha.
- Sxema EF Core code-first migratsiyalari bilan yaratiladi. Har entity'ga bitta `IEntityTypeConfiguration` (`src/server/Chittak.Infrastructure/Data/Configurations/`), `ToTable("<kontrakt nomi>")` aniq yoziladi, `idx_*` nomlari saqlanadi.
- Moslik **test bilan** tekshiriladi: `tests/Chittak.IntegrationTests/SchemaContractTests` Testcontainers'da ikkita baza yaratadi (biriga SQL fayl, biriga EF migratsiya) va katalogdan ustun / FK / CHECK / indeks **ta'riflarini** solishtiradi. Constraint va indeks **nomlari** farq qilishi mumkin (EF `ck_*` / `ix_*`).
- EF ifoda eta olmaydigan narsalar kodda raw SQL bilan: OTK'ni atomik berish — `DELETE ... FOR UPDATE SKIP LOCKED ... RETURNING` (EF `Remove` emas).
- Kontraktga mos kelish uchun tanlangan tafsilotlar: `message_queue.sender_device_id` FK'siz (navigatsiya yo'q); `discoverable` uchun `HasSentinel(true)`; `devices.platform` — enum, bazada kichik harfli `text` + CHECK.

## Oqibatlar

- Sxemani o'zgartirish = **ikki joy**: `chittak-db.sql` + EF konfiguratsiya/migratsiya. Biri unutilsa CI'dagi kontrakt testi yiqiladi.
- Test xatoni aniq ko'rsatadi (qaysi qatorda farq bor); qo'lda solishtirish shart emas.
- Test Docker talab qiladi (lokal va CI'da).

## Ko'rib chiqilgan muqobillar

- **Faqat EF, kontraktsiz** — dizayn niyati (nima yo'qligi va nega) C# konfiguratsiyalar orasida yo'qoladi; rad etildi.
- **SQL-first (DbUp / Flyway) + EF faqat so'rov uchun** — EF modeli bilan sxema sinxronligini hech narsa tekshirmaydi; rad etildi.
