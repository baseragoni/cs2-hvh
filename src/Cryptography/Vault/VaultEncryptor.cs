using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Vault;

public sealed class VaultEncryptor
{
    public EncryptedPayload EncryptSeed(byte[] seed, string passphrase)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = Pbkdf2Provider.Derive(
            System.Text.Encoding.UTF8.GetBytes(passphrase),
            salt,
            iterations: 100_000,
            length: 32);

        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipherText = new byte[seed.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        aes.Encrypt(nonce, seed, cipherText, tag);

        var combined = new byte[cipherText.Length + tag.Length];
        Buffer.BlockCopy(cipherText, 0, combined, 0, cipherText.Length);
        Buffer.BlockCopy(tag, 0, combined, cipherText.Length, tag.Length);

        return new EncryptedPayload(salt, nonce, combined);
    }

    public byte[] DecryptSeed(byte[] cipherText, byte[] salt, byte[] nonce, string passphrase)
    {
        var key = Pbkdf2Provider.Derive(
            System.Text.Encoding.UTF8.GetBytes(passphrase),
            salt,
            iterations: 100_000,
            length: 32);

        var payloadLength = cipherText.Length - 16;
        var payload = cipherText.AsSpan(0, payloadLength);
        var tag = cipherText.AsSpan(payloadLength, 16);
        var plain = new byte[payloadLength];

        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        aes.Decrypt(nonce, payload, tag, plain);
        return plain;
    }
}

public readonly record struct EncryptedPayload(byte[] Salt, byte[] Nonce, byte[] CipherText);
