[← Loyiha](../README.md) · [Sprintlar](../sprints/README.md) · [O'yinchoq misol →](../learn/README.md)

# Resurslar — X3DH va Double Ratchet'ni **kod orqali** o'rganish

Spec o'qish bilan tushunish qiyin bo'lsa — tayyor implementatsiyani o'qing, ishga tushiring, buzing. Quyidagi tartib **oddiydan murakkabga**.

> ⚠️ **Litsenziya:** Chittak — MIT. Signal'ning rasmiy repolari va C# porti **GPL-3.0 / AGPL-3.0** — ulardan kod **nusxa ko'chirmang** (butun loyiha GPL bo'lib qoladi). O'qing, tushuning, o'zingiz yozing. MIT repolardan g'oya olish bemalol.

## 0. Avval: o'z qo'lingiz bilan

[`learn/KriptoOyin`](../learn/README.md) — ~180 qatorlik C# misol (NSec bilan): DH → X3DH → simmetrik zanjir → DH ratchet. Ishga tushiring, keyin README'dagi mashqlar bilan buzing.

## 1. Eng o'qiladigan: Python (MIT)

Kichik, izohli, bitta g'oya = bitta fayl. Birinchi o'qish uchun eng yaxshisi.

| Repo | Nimani o'qish kerak |
|---|---|
| [Syndace/python-x3dh](https://github.com/Syndace/python-x3dh) | [`x3dh/base_state.py`](https://github.com/Syndace/python-x3dh/blob/main/x3dh/base_state.py) — `bundle`, `get_shared_secret_active` (= Alisa, S1-04 `Initiate`) va `get_shared_secret_passive` (= Bob, S1-05 `Respond`) |
| [Syndace/python-doubleratchet](https://github.com/Syndace/python-doubleratchet) | [`kdf_chain.py`](https://github.com/Syndace/python-doubleratchet/blob/main/doubleratchet/kdf_chain.py) → [`symmetric_key_ratchet.py`](https://github.com/Syndace/python-doubleratchet/blob/main/doubleratchet/symmetric_key_ratchet.py) → [`diffie_hellman_ratchet.py`](https://github.com/Syndace/python-doubleratchet/blob/main/doubleratchet/diffie_hellman_ratchet.py) → [`double_ratchet.py`](https://github.com/Syndace/python-doubleratchet/blob/main/doubleratchet/double_ratchet.py) (S02, shu tartibda) |
| | Ishga tushiriladigan chat: [`examples/dr_chat.py`](https://github.com/Syndace/python-doubleratchet/blob/main/examples/dr_chat.py) |

Fayllar tartibi spec'ning o'zidagi tartib: avval bitta zanjir, keyin simmetrik ratchet, keyin DH ratchet, oxirida ikkalasini birlashtirgan Double Ratchet.

## 2. Kichik va aniq: Go (MIT)

| Repo | Nimani o'qish kerak |
|---|---|
| [status-im/doubleratchet](https://github.com/status-im/doubleratchet) | [`state.go`](https://github.com/status-im/doubleratchet/blob/develop/state.go) — `State` struct: ratchet holati maydonlari (`DHr`, `RootCh`, `MkSkipped`, `MaxSkip` — S2-01, S2-09) · [`chains.go`](https://github.com/status-im/doubleratchet/blob/develop/chains.go) — KDF zanjirlari · [`session.go`](https://github.com/status-im/doubleratchet/blob/develop/session.go) — `RatchetEncrypt`/`RatchetDecrypt`, skipped keys (S2-05…S2-08) |

Go o'qish C# biluvchiga oson; kod spec pseudo-kodiga deyarli qatorma-qator mos.

## 3. Sizning tilingizda, to'liq: C# (GPL-3.0 — faqat o'qish uchun!)

| Repo | Nimani o'qish kerak |
|---|---|
| [signal-csharp/libsignal-protocol-dotnet](https://github.com/signal-csharp/libsignal-protocol-dotnet) | [`ratchet/RatchetingSession.cs`](https://github.com/signal-csharp/libsignal-protocol-dotnet/blob/master/libsignal-protocol-dotnet/ratchet/RatchetingSession.cs) — X3DH (S1-04/05) · [`ratchet/ChainKey.cs`](https://github.com/signal-csharp/libsignal-protocol-dotnet/blob/master/libsignal-protocol-dotnet/ratchet/ChainKey.cs), [`ratchet/RootKey.cs`](https://github.com/signal-csharp/libsignal-protocol-dotnet/blob/master/libsignal-protocol-dotnet/ratchet/RootKey.cs) — ratchet · [`SessionBuilder.cs`](https://github.com/signal-csharp/libsignal-protocol-dotnet/blob/master/libsignal-protocol-dotnet/SessionBuilder.cs), [`SessionCipher.cs`](https://github.com/signal-csharp/libsignal-protocol-dotnet/blob/master/libsignal-protocol-dotnet/SessionCipher.cs) — S03'dagi `SessionCipher` fasadining asl nusxasi |

Bu libsignal-protocol-java'ning C# porti; 2022'dan beri yangilanmagan, lekin arxitekturani ko'rish uchun eng yaqin misol.

## 4. Etalon: Signal'ning o'zi (GPL-3.0 / AGPL-3.0 — faqat o'qish uchun!)

| Repo | Nimani o'qish kerak |
|---|---|
| [signalapp/libsignal-protocol-java](https://github.com/signalapp/libsignal-protocol-java) (arxivlangan, klassik) | [`RatchetingSession.java`](https://github.com/signalapp/libsignal-protocol-java/blob/master/java/src/main/java/org/whispersystems/libsignal/ratchet/RatchetingSession.java), [`SessionCipher.java`](https://github.com/signalapp/libsignal-protocol-java/blob/master/java/src/main/java/org/whispersystems/libsignal/SessionCipher.java) |
| [signalapp/libsignal](https://github.com/signalapp/libsignal) (hozirgi, Rust) | [`rust/protocol/src/ratchet.rs`](https://github.com/signalapp/libsignal/blob/main/rust/protocol/src/ratchet.rs), [`ratchet/keys.rs`](https://github.com/signalapp/libsignal/blob/main/rust/protocol/src/ratchet/keys.rs), [`session.rs`](https://github.com/signalapp/libsignal/blob/main/rust/protocol/src/session.rs) — PQXDH va boshqa kengaytmalar bilan, shuning uchun murakkabroq |

## Spec'lar (kod bilan yonma-yon o'qing)

- X3DH: https://signal.org/docs/specifications/x3dh/ — §2 (kalitlar), §3 (protokol)
- Double Ratchet: https://signal.org/docs/specifications/doubleratchet/ — §3 (chizmalar), §5 (pseudo-kod)
- Sesame (multi-device, S10): https://signal.org/docs/specifications/sesame/
- NSec API (loyiha kutubxonasi): https://nsec.rocks/

## Qanday o'qish samarali

1. Kodda funksiyani toping (masalan `ratchet_encrypt`) → spec §5'dagi pseudo-kod bilan **qatorma-qator** solishtiring.
2. O'qiganingizni `learn/KriptoOyin`da takrorlang yoki o'zgartiring — o'qish emas, **buzish** o'rgatadi.
3. Tushunganingizni `docs/crypto/*.md`ga o'z so'zingiz bilan yozing (S1-10, S2-13).
