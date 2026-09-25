[← S12](S12-turn-signaling.md) · [Reja va qoidalar](README.md) · [S14 →](S14-xavfsizlik.md)

# Sprint 13 — Qo'ng'iroq 2: WebRTC media (audio)

**Maqsad:** haqiqiy audio qo'ng'iroq, P2P yoki TURN orqali, DTLS-SRTP. **Eng xavfli sprint** — 2 haftaga cho'zilishi normal.

**Nega hozir:** signaling tayyor, faqat media qatlami qoldi.

**O'rganiladi:** MAUI'da native WebRTC binding, audio capture/playback, ICE ulanish diagnostikasi, DTLS fingerprint tekshiruvi (E2EE kafolati).

## Dizayn qarori (ADR `0009-webrtc-stack.md`, sprint boshida 1 kunlik spike)
Variantlar:
1. **SIPSorcery** (pure C#) — WebRTC stack bor, lekin mobil audio capture/playback o'zing qo'shasan. Boshlash oson, mobil'da ishlab ketishi noaniq.
2. **Native binding**: Android `org.webrtc` (Google AAR) + iOS `WebRTC.framework` → MAUI binding library. Eng ishonchli, eng ko'p ish (binding loyihalar).
3. **Hozircha Android-only + native binding** — ikkinchi platformani v2'ga.

Spike natijasi bo'yicha tanla. Tavsiya: 2 (yoki 3).

## Vazifalar
| ID | Vazifa | Qabul mezoni |
|---|---|---|
| S13-01 | Spike (1 kun): tanlangan stack bilan "loopback" — bitta qurilmada mikrofon → PeerConnection → dinamik | Eshitiladi |
| S13-02 | `IWebRtcService` abstraksiyasi (platform-specific implementatsiya): `CreateOffer/Answer`, `AddIceCandidate`, `OnIceCandidate`, `OnConnectionState`, `SetMuted`, `SetSpeaker` | Interfeys + Android impl |
| S13-03 | S12 signaling ↔ S13 PeerConnection ulash | Ikki qurilma bir Wi-Fi'da gaplashadi |
| S13-04 | TURN majburiy rejim (`iceTransportPolicy: relay`) bilan test — TURN orqali ham ishlaydi | Mobil internet ↔ Wi-Fi gaplashadi |
| S13-05 | **DTLS fingerprint tekshiruvi:** SDP ichidagi `a=fingerprint` E2EE signaling orqali kelgan — PeerConnection ulanganda haqiqiy remote sertifikat fingerprint'i bilan solishtir, mos kelmasa uzish | Test: fingerprint buzilsa qo'ng'iroq uzildi |
| S13-06 | Audio UI: mute, speaker, davomiylik, tarmoq sifati indikatori (`getStats` RTT/packet loss) | — |
| S13-07 | Ruxsatlar (mikrofon), audio session (iOS `AVAudioSession`, Android `AudioManager` mode), kiruvchi qo'ng'iroqda ring | — |
| S13-08 | Qo'ng'iroq tugashi: ikkala tomonda tozalash, `HANGUP` signal, "o'tkazib yuborilgan" xabar | — |
| S13-09 | `docs/calls/media.md`: stack, cheklovlar, ma'lum muammolar | Yozilgan |

## Demo
Ikki telefon, biri mobil internetda, biri Wi-Fi'da — audio qo'ng'iroq. `turnserver` loglarida relay ko'rinadi.

## Resurslar
- WebRTC for the Curious, 5–8 boblar (media, DTLS/SRTP, stats)
- SIPSorcery: https://github.com/sipsorcery-org/sipsorcery
- Google WebRTC Android (AAR): https://webrtc.github.io/webrtc-org/native-code/android/
- .NET Android binding library (AAR ni C# dan ishlatish): https://learn.microsoft.com/dotnet/android/binding-libs/binding-java-libs/
- WebRTC iOS: https://webrtc.github.io/webrtc-org/native-code/ios/
- DTLS-SRTP (RFC 5764): https://www.rfc-editor.org/rfc/rfc5764
- Signal qo'ng'iroq E2EE haqida (RingRTC, fon): https://github.com/signalapp/ringrtc

## Tuzoqlar
- Emulator mikrofoni — ishonchsiz. Haqiqiy 2 ta qurilma kerak (yoki 1 telefon + 1 eski telefon).
- Bluetooth/karnay/quloqchin audio routing — v1'da faqat oddiy + karnay.
- `IWebRtcService` interfeysi `Chittak.Client`da, implementatsiya faqat Mobile'da. Desktop qo'ng'iroq — roadmap (SIPSorcery desktop'da mikrofon/dinamik bilan ishlashi mumkin — v2'da spike).
- Bu sprint bitmasa — **loyiha muvaffaqiyatsiz emas.** Chat E2EE ishlayapti. Qo'ng'iroqni v1.1 qilib, S14'ga o't.

---

---
[← S12](S12-turn-signaling.md) · [Reja va qoidalar](README.md) · [S14 →](S14-xavfsizlik.md)
