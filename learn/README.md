[← Loyiha](../README.md) · [Sprintlar](../sprints/README.md) · [Resurslar →](../resources/README.md)

# learn/ — o'rganish uchun o'yinchoq misollar

Bu yerdagi kod **Chittak.Protocol emas**: xatolarni tekshirish, kalitni xotiradan tozalash, serializatsiya yo'q. Maqsad bitta — g'oyani ishga tushirib, ko'z bilan ko'rish va **buzib** tushunish. `Chittak.slnx`ga kirmaydi, CI'da build bo'lmaydi, lekin repo qoidalari (analyzer'lar, CPM) ishlaydi.

## KriptoOyin — DH → X3DH → Double Ratchet (S01–S02)

```bash
dotnet run --project learn/KriptoOyin
```

[`KriptoOyin/Program.cs`](KriptoOyin/Program.cs) 4 qadamni ko'rsatadi, har birida ikki tomon **bir xil** natijaga keladi:

| Qadam | Nima | Sprint |
|---|---|---|
| 1. Diffie-Hellman | `DH(alisa_maxfiy, bob_ochiq) == DH(bob_maxfiy, alisa_ochiq)` | S1-01 |
| 2. X3DH | Bob offline; Alisa bundle bilan 4 ta DH → bir xil `SK` | S1-04, S1-05 |
| 3. Simmetrik zanjir | Har xabarga yangi kalit (`HMAC(zanjir, 0x01/0x02)`) | S2-02, S2-05 |
| 4. DH ratchet | Bob yangi DH kaliti bilan javob → yangi root va zanjir | S2-04, S2-06 |

## Mashqlar — buzing va nima bo'lishini oldindan ayting

Har mashqdan **oldin** natijani taxmin qiling, keyin ishga tushiring. Taxmin noto'g'ri chiqsa — aynan shu joyni hali tushunmagansiz.

1. **Tartib muhimmi?** 2-qadamda faqat Alisa tomonida `DH1` va `DH2` o'rnini almashtiring. SK'lar mos keladimi? Nega spec tartibni qat'iy belgilaydi?
2. **OTK'siz X3DH.** Ikkala tomonda ham `DH4`ni olib tashlang. Hali ham mos keladimi? Bu S1-06 — bundle'da OTK tugaganda nima bo'ladi?
3. **Soxta bundle.** Bob'ning `spkImzo[0] ^= 1` qilib imzoni buzing. Hozir kod `False` chiqaradi-yu, davom etaveradi — shunday bo'lmasligi kerak. To'xtatadigan qiling (S1-07).
4. **O'rtadagi odam.** Mallory o'z identity'si bilan imzolangan SPK beradi, lekin bundle'da Bob'ning identity'si turibdi. Imzo tekshiruvi buni ushlaydimi?
5. **Yo'qolgan xabar.** 3-qadamda Bob 2-xabarni olmasin (zanjirni aylantirmasdan o'tkazib yuboring). 3-xabar ochiladimi? Bu muammoni `MKSKIPPED` qanday hal qiladi (S2-08)?
6. **Buzilgan shifr.** `shifr[0] ^= 1` qiling va oching. Nima bo'ladi va nega bu yaxshi (S2-11)?
7. **Navbat Alisa'ga.** 4-qadamdan keyin Alisa ham yangi ratchet kaliti bilan javob yozsin — yana bitta `RootQadam`. Root key necha marta yangilandi?
8. **Forward secrecy'ni isbotlang.** 3-qadamdagi uchinchi zanjir kalitidan birinchi xabar kalitini hisoblashga urinib ko'ring. Nega imkonsiz?

Mashqlar tugagach — [resurslar](../resources/README.md)dagi Python/Go implementatsiyalarini o'qing: endi ular tanish ko'rinadi.
