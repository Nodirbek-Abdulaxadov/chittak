# chittak

Signal protokoli (X3DH + Double Ratchet) asosidagi E2EE messenjer. Butun kriptografiya klientda;
server — "soqov pochta" (relay + ochiq kalitlar kitobi), bazada faqat public kalitlar va opaque navbat.

**Stack:** .NET MAUI (mobil) · Avalonia (desktop, Linux) · ASP.NET Core (server) · PostgreSQL · WebRTC + coturn (qo'ng'iroq)

- [chittak-arxitektura.md](chittak-arxitektura.md) — arxitektura, workflow, har jadvalning "nega"si
- [chittak-db.sql](chittak-db.sql) — server sxemasi (PostgreSQL 13+)
- [sprints/](sprints/README.md) — 16 sprintlik reja (TZ shaklida), har sprint alohida fayl, resurslar bilan

