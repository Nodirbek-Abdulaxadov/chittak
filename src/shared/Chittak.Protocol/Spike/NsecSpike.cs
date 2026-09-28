using System.Runtime.InteropServices;
using NSec.Cryptography;

namespace Chittak.Protocol.Spike;

// S0-08 spike: NSec (libsodium) shu platformada ishlaydimi? Natija ADR 0003'ga yozilgan.
// Protocol'ning haqiqiy kodi S01'da yoziladi — o'shanda bu fayl va ilovalardagi chaqiruvlar o'chiriladi.
public static class NsecSpike
{
    public static string Run()
    {
        string platform = $"{RuntimeInformation.RuntimeIdentifier} ({RuntimeInformation.OSDescription})";
        try
        {
            var x25519 = KeyAgreementAlgorithm.X25519;
            var exportable = new SharedSecretCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport };
            using var alice = Key.Create(x25519);
            using var bob = Key.Create(x25519);
            using var aliceSecret = x25519.Agree(alice, bob.PublicKey, exportable)!;
            using var bobSecret = x25519.Agree(bob, alice.PublicKey, exportable)!;
            bool dh = aliceSecret.Export(SharedSecretBlobFormat.RawSharedSecret).AsSpan()
                .SequenceEqual(bobSecret.Export(SharedSecretBlobFormat.RawSharedSecret));

            var ed25519 = SignatureAlgorithm.Ed25519;
            using var signer = Key.Create(ed25519);
            byte[] data = "chittak"u8.ToArray();
            bool signature = ed25519.Verify(signer.PublicKey, data, ed25519.Sign(signer, data));

            var aead = AeadAlgorithm.ChaCha20Poly1305;
            using var key = Key.Create(aead);
            byte[] nonce = new byte[aead.NonceSize];
            byte[] ciphertext = aead.Encrypt(key, nonce, [], data);
            bool roundTrip = aead.Decrypt(key, nonce, [], ciphertext)?.AsSpan().SequenceEqual(data) == true;

            return $"NSec OK on {platform}: X25519 DH={dh}, Ed25519={signature}, ChaCha20-Poly1305={roundTrip}";
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException
                                       or TypeInitializationException or PlatformNotSupportedException)
        {
            return $"NSec FAILED on {platform}: {ex.GetType().Name}: {ex.Message}";
        }
    }
}
