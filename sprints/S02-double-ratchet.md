[← S01](S01-x3dh.md) · [Reja va qoidalar](README.md) · [S03 →](S03-envelope-konsol-demo.md)

# Sprint 2 — Double Ratchet

**Maqsad:** X3DH'dan chiqqan root key ustida Double Ratchet: har xabarga yangi kalit, out-of-order xabarlar, yo'qolgan xabarlar.

**Nega hozir:** X3DH faqat *birinchi* kalitni beradi. Xabar almashinuvining o'zi shu yerda.

**O'rganiladi:** KDF zanjirlari (root/chain/message key), DH ratchet vs symmetric ratchet, forward secrecy va post-compromise security farqi, skipped message keys, state machine testlash.

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S2-01 | `Ratchet/RatchetState` — spec'dagi barcha maydonlar: `DHs, DHr, RK, CKs, CKr, Ns, Nr, PN, MKSKIPPED` — **immutable record** yoki aniq mutatsiya | Serialize/deserialize round-trip (S8'da diskka yoziladi) |
| S2-02 | `KdfRk(rk, dhOut)`, `KdfCk(ck)` — HKDF/HMAC bilan, spec §5.2 tavsiyalari bo'yicha | Deterministik test: bir xil kirish → bir xil chiqish |
| S2-03 | `MessageHeader { DhPublic, Pn, N }` + serialize | Round-trip test |
| S2-04 | `DoubleRatchet.InitAlice(sharedSecret, bobSignedPreKeyPublic)` / `InitBob(sharedSecret, bobSignedPreKeyPair)` | Alice birinchi xabar yuboradi, Bob ochadi |
| S2-05 | `Encrypt(state, plaintext, ad) → (newState, header, ciphertext)` | Test: 1 xabar |
| S2-06 | `Decrypt(state, header, ciphertext, ad) → (newState, plaintext)` — DH ratchet qadami, skipped keys | Test: A→B, B→A, A→B (DH ratchet 2 marta aylanadi) |
| S2-07 | Out-of-order: A 1,2,3 yuboradi; B 3,1,2 tartibda oladi | Hammasi ochiladi, `MKSKIPPED` oxirida bo'sh |
| S2-08 | Yo'qolgan xabar: A 1,2,3 yuboradi; B faqat 1 va 3 ni oladi | 1 va 3 ochiladi, 2 ning kaliti `MKSKIPPED`da (limit bilan) |
| S2-09 | `MAX_SKIP = 1000` — oshsa exception (DoS himoyasi) | Test |
| S2-10 | Replay: bir xabar ikki marta `Decrypt` → ikkinchisi xato (kalit o'chirilgan) | Test |
| S2-11 | Wrong key / buzilgan ciphertext / buzilgan header → AEAD xato, **state o'zgarmaydi** | Test: xatodan keyin state avvalgi bilan teng |
| S2-12 | Uzun suhbat testi: 10 000 xabar, tasodifiy yo'nalish, tasodifiy tartib (seedlangan random) | Barchasi ochiladi, xotira o'smaydi |
| S2-13 | `docs/crypto/double-ratchet.md` — 1 sahifa, chizma bilan | Yozilgan |

## Demo
`dotnet test --filter Ratchet` → yashil, shu jumladan 10 000 xabarlik fuzz.

## Resurslar
- **Avval ishga tushiring:** [`learn/KriptoOyin`](../learn/README.md) (mashqlar bilan) · kod misollari: [`resources/`](../resources/README.md)
- **Double Ratchet spec (majburiy):** https://signal.org/docs/specifications/doubleratchet/ — §3 (tushuncha), §5 (algoritm, pseudo-kod), §6 (xavfsizlik)
- Vizual tushuntirish: spec'ning o'zidagi chizmalar eng yaxshisi — ularni qo'lda qayta chiz
- libsignal `session_cipher.rs` — solishtirish
- Property-based testing .NET (S2-12 uchun): FsCheck — https://fscheck.github.io/FsCheck/

## Tuzoqlar
- State'ni `Decrypt` xato bo'lganda ham o'zgartirib yuborish — klassik xato. Avval yangi state hisobla, faqat AEAD o'tgach almashtir.
- `MKSKIPPED` cheksiz o'ssa — xotira DoS. Limit + eng eskisini tashlash.
- Header'ni AEAD'ga **associated data** sifatida ber — aks holda header o'zgartirilsa sezilmaydi.

---

---
[← S01](S01-x3dh.md) · [Reja va qoidalar](README.md) · [S03 →](S03-envelope-konsol-demo.md)
