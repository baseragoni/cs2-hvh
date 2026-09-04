using Cs2Hvh.Cryptography.Vault;
using Xunit;

namespace Cs2Hvh.Cryptography.Tests;

public class VaultEncryptorTests
{
    [Fact]
    public void EncryptDecrypt_RoundTrip_PreservesSeed()
    {
        var encryptor = new VaultEncryptor();
        var seed = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        var payload = encryptor.EncryptSeed(seed, "test-passphrase");
        var decrypted = encryptor.DecryptSeed(payload.CipherText, payload.Salt, payload.Nonce, "test-passphrase");

        Assert.Equal(seed, decrypted);
    }
}
