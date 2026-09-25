# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Loyiha holati

**Chittak** — Signal protokoli (X3DH + Double Ratchet) asosidagi E2EE messenjer. Hozircha **dizayn bosqichi: kod yo'q**, faqat hujjatlar. Kod paydo bo'lganda bu faylga build/test buyruqlarini qo'shing.

Hujjatlar (hammasi o'zbek tilida, **lotin alifbosida**):
- `chittak-arxitektura.md` — "nima uchun shunday" hujjati: 3 komponent, uchidan-uchiga workflow, har jadvalning sababi, roadmap. Dizayn o'zgarsa **shu fayl ham yangilanadi** (DoD qoidasi).
- `chittak-db.sql` — server sxemasining **kontrakti**. Kelajakda EF Core code-first bo'ladi, lekin bu fayl etalon bo'lib qoladi (test bilan solishtiriladi, `sprints/S00-skelet.md` → S0-05).
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
| Rejalashtirilgan struktura | `src/server/Chittak.{Api,Core,Infrastructure}`, `src/client/Chittak.{Client,Mobile,Desktop}` (umumiy mantiq, MAUI, Avalonia), `src/shared/Chittak.Protocol`, `tests/`. Core → Infrastructure'ni bilmaydi; Protocol'da interfeyslar, implementatsiya klientda. `Chittak.Client`da `Microsoft.Maui.*`/`Avalonia.*` reference bo'lmaydi |

## Sxemani tekshirish

Kod yo'q, lekin `chittak-db.sql` haqiqiy Postgres'da tekshiriladi. Mavjud konteynerlarga tegmang (5432/5433 band) — vaqtinchalik konteyner ishlating:

```bash
docker run --rm -d --name chittak-schema-test -e POSTGRES_PASSWORD=t -p 127.0.0.1:55432:5432 postgres:17-alpine
until docker exec chittak-schema-test pg_isready -U postgres -q; do sleep 1; done
PGPASSWORD=t psql -h 127.0.0.1 -p 55432 -U postgres -v ON_ERROR_STOP=1 -q -f chittak-db.sql
docker rm -f chittak-schema-test
```

## Hujjat konventsiyalari

- O'zbek tili, **faqat lotin alifbosi** — bitta ham kirill harfi bo'lmasin (avval aralash bo'lgan, tozalangan). Tekshirish: `grep -rnP '[\x{0400}-\x{04FF}]' --include=*.md --include=*.sql .`
- `chittak-db.sql` ichidagi izohlar inglizcha, hujjatlar o'zbekcha — shunday qoladi.
- Har sprint fayli bir xil shablonda: Maqsad → Nega hozir → O'rganiladi → Vazifalar (jadval, ID + qabul mezoni) → Demo → Resurslar → Tuzoqlar; yuqori/pastda navigatsiya havolalari. Yangi sprint/bo'lim qo'shsangiz `sprints/README.md` jadvalini ham yangilang.
- Havolalar qo'shsangiz ishlashini tekshiring (`curl -sIL -o /dev/null -w '%{http_code}'`).
- Loyiha bir kishi tomonidan **o'rganish maqsadida** qurilmoqda: tayyor yechim berishdan oldin sababini tushuntiring, tavsiya bering, lekin qarorni foydalanuvchiga qoldiring.
