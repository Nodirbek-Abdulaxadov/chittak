# 0001. Solution strukturasi

- **Holat:** Qabul qilindi (2026-09-25)
- **Sprint:** S0-01, S0-07

## Kontekst

Bitta repoda uch xil kod yashaydi: server (relay + kalitlar kitobi), umumiy protokol (kripto) va ikki klient (mobil, desktop). Server Clean Architecture shablonidan boshlangan. Rejada server 3 qatlamli (`Api/Core/Infrastructure`) edi, shablon esa 4 qatlamli (`Server/Application/Domain/Infrastructure`) keldi.

## Qaror

```
src/
  server/  Chittak.Domain, Chittak.Application, Chittak.Infrastructure, Chittak.Server
  shared/  Chittak.Protocol
  client/  Chittak.Client, Chittak.Mobile, Chittak.Desktop
tests/     Chittak.UnitTests, Chittak.IntegrationTests
learn/     o'yinchoq misollar (solution'ga kirmaydi)
```

- Barcha loyiha va namespace'lar `Chittak.*`.
- Server 4 qatlam saqlanadi: **Server → Infrastructure → Application → Domain**. Domain hech narsaga bog'liq emas; Domain va Application Infrastructure'ni bilmaydi.
- **Application → Protocol** (envelope formatini biladi, ichini ochmaydi); **Client → Protocol**; **Mobile → Client**, **Desktop → Client**. `Chittak.Client` UI framework'ni bilmaydi.
- `Chittak.Mobile` `Chittak.slnx`ga kirmaydi: CI runner'da `maui-android` workload yo'q. Mobil uchun alohida CI job — S7-01.
- `learn/` solution'ga kirmaydi va Protocol kodi hisoblanmaydi.

## Oqibatlar

- Papka nomidan kod qaysi tomonda yashashi ko'rinadi: `server/` hech qachon maxfiy kalit ko'rmaydi, kripto `shared/Chittak.Protocol`da.
- Shablondagi `IApplicationDbContext` tufayli Application EF Core'ga bog'liq — "toza Core" talab qilinmaydi, lekin Application'da raw SQL / Npgsql yo'q (S5-09).
- `dotnet build Chittak.slnx` MAUI workload'isiz ishlaydi; Mobile alohida build qilinadi.

## Ko'rib chiqilgan muqobillar

- **Reja bo'yicha 3 qatlam (`Api/Core/Infrastructure`)** — Domain va Application'ni birlashtirish katta refaktor, foyda kam; rad etildi.
- **`src/` ichida tekis** — yo'llar qisqa, lekin server/klient chegarasi ko'rinmaydi; rad etildi.
