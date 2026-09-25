// O'YINCHOQ MISOL — X3DH va Double Ratchet g'oyasini "qo'l bilan his qilish" uchun.
// Bu Chittak.Protocol EMAS: xatolarni tekshirish, kalitlarni xotiradan tozalash,
// MKSKIPPED, header, serializatsiya yo'q. Faqat g'oya ko'rinsin deb soddalashtirilgan.

using System.Security.Cryptography;
using System.Text;
using NSec.Cryptography;

var x25519 = KeyAgreementAlgorithm.X25519;
var ed25519 = SignatureAlgorithm.Ed25519;
var aead = AeadAlgorithm.ChaCha20Poly1305;

// ============================================================
// 1-QADAM. Diffie-Hellman: ochiq kalit almashib, bir xil sir
// ============================================================
Sarlavha("1. Diffie-Hellman");

using var alisaMaxfiy = YangiX25519();
using var bobMaxfiy = YangiX25519();

// Alisa: o'z MAXFIY kaliti + Bob'ning OCHIQ kaliti
byte[] alisaHisobladi = Dh(alisaMaxfiy, bobMaxfiy.PublicKey);
// Bob:   o'z MAXFIY kaliti + Alisa'ning OCHIQ kaliti
byte[] bobHisobladi = Dh(bobMaxfiy, alisaMaxfiy.PublicKey);

Console.WriteLine($"Alisa'ning ochiq kaliti : {Hex(alisaMaxfiy.PublicKey)}");
Console.WriteLine($"Bob'ning ochiq kaliti   : {Hex(bobMaxfiy.PublicKey)}");
Console.WriteLine($"Alisa hisoblagan sir    : {Hex(alisaHisobladi)}");
Console.WriteLine($"Bob hisoblagan sir      : {Hex(bobHisobladi)}");
Console.WriteLine($"Bir xilmi? {alisaHisobladi.AsSpan().SequenceEqual(bobHisobladi)}   <- server faqat ochiq kalitlarni ko'rdi");

// ============================================================
// 2-QADAM. X3DH: Bob OFFLINE, Alisa bundle orqali birinchi sirni oladi
// ============================================================
Sarlavha("2. X3DH");

// --- Bob oldindan (onlayn paytida) tayyorlaydi va serverga PUBLIC qismini qo'yadi ---
using var bobIdentityImzo = Key.Create(ed25519);  // IK_B (Ed25519 qismi — imzo uchun)
using var bobIdentityDh = YangiX25519();           // IK_B (X25519 qismi — DH uchun)
using var bobSignedPreKey = YangiX25519();         // SPK_B
using var bobOneTimePreKey = YangiX25519();        // OPK_B

// SPK'ni identity bilan imzolaydi — Alisa "bu kalit haqiqatan Bob'niki"ga ishonishi uchun
byte[] spkOchiq = bobSignedPreKey.PublicKey.Export(KeyBlobFormat.RawPublicKey);
byte[] spkImzo = ed25519.Sign(bobIdentityImzo, spkOchiq);

// --- Server'dagi bundle: FAQAT ochiq kalitlar (device_identity_keys, signed_prekeys, one_time_prekeys) ---
var bundle = new
{
    IdentityImzoOchiq = bobIdentityImzo.PublicKey,
    IdentityDhOchiq = bobIdentityDh.PublicKey,
    SignedPreKeyOchiq = bobSignedPreKey.PublicKey,
    SignedPreKeyImzo = spkImzo,
    OneTimePreKeyOchiq = bobOneTimePreKey.PublicKey,
};

// --- Alisa (Bob uxlayapti) ---
bool imzoTogri = ed25519.Verify(bundle.IdentityImzoOchiq,
    bundle.SignedPreKeyOchiq.Export(KeyBlobFormat.RawPublicKey), bundle.SignedPreKeyImzo);
Console.WriteLine($"Alisa SPK imzosini tekshirdi: {imzoTogri}");

using var alisaIdentityDh = YangiX25519();  // IK_A
using var alisaEphemeral = YangiX25519();   // EK_A — shu suhbat uchun, bir martalik

byte[] alisaSk = Hkdf(Birlashtir(
    Dh(alisaIdentityDh, bundle.SignedPreKeyOchiq),   // DH1: Alisa'ning shaxsi
    Dh(alisaEphemeral, bundle.IdentityDhOchiq),      // DH2: Bob'ning shaxsi
    Dh(alisaEphemeral, bundle.SignedPreKeyOchiq),    // DH3: yangi material
    Dh(alisaEphemeral, bundle.OneTimePreKeyOchiq)),  // DH4: takrorlanmaslik
    "X3DH");

// Alisa birinchi xabar bilan IK_A va EK_A OCHIQ kalitlarini yuboradi...

// --- Bob uyg'ondi: xuddi shu 4 ta DH, lekin o'z MAXFIY kalitlari bilan ---
byte[] bobSk = Hkdf(Birlashtir(
    Dh(bobSignedPreKey, alisaIdentityDh.PublicKey),  // DH1
    Dh(bobIdentityDh, alisaEphemeral.PublicKey),     // DH2
    Dh(bobSignedPreKey, alisaEphemeral.PublicKey),   // DH3
    Dh(bobOneTimePreKey, alisaEphemeral.PublicKey)), // DH4  (keyin OPK_B o'chiriladi!)
    "X3DH");

Console.WriteLine($"Alisa'ning SK : {Hex(alisaSk)}");
Console.WriteLine($"Bob'ning SK   : {Hex(bobSk)}");
Console.WriteLine($"Bir xilmi? {alisaSk.AsSpan().SequenceEqual(bobSk)}");

// ============================================================
// 3-QADAM. Simmetrik zanjir: har xabarga YANGI kalit
// ============================================================
Sarlavha("3. Simmetrik zanjir (1-g'ildirak)");

byte[] alisaZanjir = alisaSk;  // soddalik uchun: zanjir SK'dan boshlanadi
byte[] bobZanjir = bobSk;
string[] xabarlar = ["salom", "qalaysan?", "ertaga uchrashamizmi?"];

foreach (var matn in xabarlar)
{
    // Alisa: zanjirni bir qadam aylantiradi -> xabar kaliti + keyingi zanjir
    (byte[] xabarKaliti, alisaZanjir) = ZanjirQadam(alisaZanjir);
    byte[] shifr = Shifrla(xabarKaliti, matn);

    // Bob: xuddi shunday aylantiradi va ochadi
    (byte[] bobXabarKaliti, bobZanjir) = ZanjirQadam(bobZanjir);
    string ochildi = Och(bobXabarKaliti, shifr);

    Console.WriteLine($"kalit {Hex(xabarKaliti)[..16]}...  server ko'rgani: {Hex(shifr)[..24]}...  Bob o'qidi: \"{ochildi}\"");
}
Console.WriteLine("Har xabar kaliti boshqacha. Zanjir faqat OLDINGA yuradi: yangi kalitdan eskisini hisoblab bo'lmaydi (forward secrecy).");

// ============================================================
// 4-QADAM. DH ratchet: Bob javob yozadi -> root key yangilanadi
// ============================================================
Sarlavha("4. DH ratchet (2-g'ildirak)");

byte[] rootKey = alisaSk;  // ikkala tomonda bir xil
using var alisaRatchet = YangiX25519();  // Alisa'ning joriy ratchet kaliti
using var bobRatchet = YangiX25519();    // Bob javob uchun YANGI kalit yaratdi (ochig'i header'da ketadi)

// Ikkala tomon: yangiRoot, yangiZanjir = KDF(root, DH(...))
(byte[] bobYangiRoot, byte[] bobYuborishZanjiri) = RootQadam(rootKey, Dh(bobRatchet, alisaRatchet.PublicKey));
(byte[] alisaYangiRoot, byte[] alisaQabulZanjiri) = RootQadam(rootKey, Dh(alisaRatchet, bobRatchet.PublicKey));

Console.WriteLine($"Bob'ning yangi yuborish zanjiri  : {Hex(bobYuborishZanjiri)}");
Console.WriteLine($"Alisa'ning yangi qabul zanjiri   : {Hex(alisaQabulZanjiri)}");
Console.WriteLine($"Bir xilmi? {bobYuborishZanjiri.AsSpan().SequenceEqual(alisaQabulZanjiri)}");
Console.WriteLine("Yangi DH = yangi tasodif. Eski holatni o'g'irlagan hujumchi bu yangi zanjirni hisoblay olmaydi (post-compromise security).");

// ============================================================
// Yordamchi funksiyalar
// ============================================================

static Key YangiX25519() => Key.Create(KeyAgreementAlgorithm.X25519);

// DH natijasining xom baytlari (X3DH uchun 4 tasini ketma-ket ulash kerak)
static byte[] Dh(Key maxfiy, PublicKey ochiq)
{
    using var sir = KeyAgreementAlgorithm.X25519.Agree(maxfiy, ochiq,
        new SharedSecretCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport })!;
    return sir.Export(SharedSecretBlobFormat.RawSharedSecret);
}

// X3DH spec: oldiga 32 ta 0xFF qo'shiladi, keyin HKDF
static byte[] Hkdf(byte[] material, string info) =>
    HKDF.DeriveKey(HashAlgorithmName.SHA256,
        [.. Enumerable.Repeat((byte)0xFF, 32), .. material],
        32, new byte[32], Encoding.UTF8.GetBytes(info));

// Double Ratchet spec §5.2: KDF_CK — HMAC(zanjir, 0x01) = xabar kaliti, HMAC(zanjir, 0x02) = keyingi zanjir
static (byte[] xabarKaliti, byte[] keyingiZanjir) ZanjirQadam(byte[] zanjir) =>
    (HMACSHA256.HashData(zanjir, new byte[] { 0x01 }), HMACSHA256.HashData(zanjir, new byte[] { 0x02 }));

// KDF_RK: root key "salt", DH natijasi "material" -> 64 bayt: yangi root + yangi zanjir
static (byte[] yangiRoot, byte[] zanjir) RootQadam(byte[] root, byte[] dhNatija)
{
    byte[] chiqish = HKDF.DeriveKey(HashAlgorithmName.SHA256, dhNatija, 64, root, "Ratchet"u8.ToArray());
    return (chiqish[..32], chiqish[32..]);
}

// Har kalit bitta xabar uchun ishlatilgani uchun nonce'ni ham shu kalitdan chiqaramiz
static byte[] Shifrla(byte[] xabarKaliti, string matn)
{
    using var kalit = Key.Import(AeadAlgorithm.ChaCha20Poly1305, xabarKaliti, KeyBlobFormat.RawSymmetricKey);
    return AeadAlgorithm.ChaCha20Poly1305.Encrypt(kalit, Nonce(xabarKaliti), [], Encoding.UTF8.GetBytes(matn));
}

static string Och(byte[] xabarKaliti, byte[] shifr)
{
    using var kalit = Key.Import(AeadAlgorithm.ChaCha20Poly1305, xabarKaliti, KeyBlobFormat.RawSymmetricKey);
    byte[] ochiq = AeadAlgorithm.ChaCha20Poly1305.Decrypt(kalit, Nonce(xabarKaliti), [], shifr)
        ?? throw new CryptographicException("Buzilgan xabar yoki noto'g'ri kalit");
    return Encoding.UTF8.GetString(ochiq);
}

static byte[] Nonce(byte[] xabarKaliti) => SHA256.HashData(xabarKaliti)[..12];

static byte[] Birlashtir(params byte[][] qismlar) => [.. qismlar.SelectMany(q => q)];

static string Hex(object x) => Convert.ToHexString(x switch
{
    PublicKey p => p.Export(KeyBlobFormat.RawPublicKey),
    byte[] b => b,
    _ => throw new ArgumentException(null, nameof(x)),
}).ToLowerInvariant();

static void Sarlavha(string s) => Console.WriteLine($"\n===== {s} =====");
