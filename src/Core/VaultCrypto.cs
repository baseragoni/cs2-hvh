using System.Security.Cryptography;
using System.Text;

namespace Cs2Hvh.Core;

public sealed class VaultCrypto
{
    private const int Iterations = 100_000;
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int SaltSize = 16;

    public (string CipherText, string Salt) Encrypt(string plaintext, string passphrase)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);
        ArgumentException.ThrowIfNullOrEmpty(passphrase);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = DeriveKey(passphrase, salt);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using (var aes = new AesGcm(key, TagSize))
            aes.Encrypt(nonce, plain, cipher, tag);

        var payload = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, payload, NonceSize, TagSize);
        Buffer.BlockCopy(cipher, 0, payload, NonceSize + TagSize, cipher.Length);
        return (Convert.ToBase64String(payload), Convert.ToBase64String(salt));
    }

    public string Decrypt(string cipherText, string saltB64, string passphrase)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);
        ArgumentException.ThrowIfNullOrEmpty(saltB64);
        ArgumentException.ThrowIfNullOrEmpty(passphrase);

        var salt = Convert.FromBase64String(saltB64);
        var key = DeriveKey(passphrase, salt);
        var payload = Convert.FromBase64String(cipherText);
        if (payload.Length < NonceSize + TagSize)
            throw new CryptographicException("Cipher payload too short.");

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var cipher = payload.AsSpan(NonceSize + TagSize);
        var plain = new byte[cipher.Length];

        using (var aes = new AesGcm(key, TagSize))
            aes.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }

    public string Fingerprint(byte[] seed)
    {
        var hash = SHA256.HashData(seed);
        return HexCodec.ToHex(hash.AsSpan(0, 8));
    }

    private static byte[] DeriveKey(string passphrase, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(passphrase), salt, Iterations, HashAlgorithmName.SHA256, KeySize);
}
