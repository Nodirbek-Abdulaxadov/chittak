# ADR — Architecture Decision Records

Har muhim qaror uchun bitta fayl: kontekst → qaror → oqibatlar → rad etilgan muqobillar. Qabul qilingan qaror qayta muhokama qilinmaydi; o'zgarsa — yangi ADR yoziladi va eskisi "O'rniga: 00NN" deb belgilanadi.

| # | Qaror | Holat |
|---|---|---|
| [0001](0001-solution-structure.md) | Solution strukturasi (`Chittak.*`, server 4 qatlam, `server/shared/client`) | Qabul qilindi |
| [0002](0002-ef-core-code-first-with-sql-contract.md) | EF Core code-first + `chittak-db.sql` kontrakti, test bilan tekshiriladi | Qabul qilindi |
| [0003](0003-crypto-library.md) | Kripto kutubxona: NSec (Linux ✅, Android ✅, iOS ochiq) | Qabul qilindi |

Rejadagi keyingilari: 0004 identity kalit formati (S1), 0005 envelope formati (S3), 0006 real-time transport (S6), 0007 mobil kalit saqlash (S7), 0008 qurilma linking (S10), 0009 WebRTC stack (S13), 0010 metadata siyosati (S14), 0011 klient UI: MAUI + Avalonia (S0-10), 0012 desktop kalit saqlash (S11).
