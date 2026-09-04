using System.Security.Cryptography;
using System.Text;
using Cs2Hvh.Cryptography.Codecs;
using Cs2Hvh.Cryptography.Vault;

namespace Cs2Hvh.Cryptography.Derivation;

/// <summary>
/// BIP-39 mnemonic validation and entropy-to-seed conversion pipeline.
/// </summary>
public sealed class Bip39MnemonicProcessor 
{
    private static readonly string[] WordList = LoadWordList();

    private readonly Bip32KeyDeriver _bip32;

    public Bip39MnemonicProcessor(Bip32KeyDeriver bip32)
    {
        _bip32 = bip32;
    }

    public byte[] DeriveMasterSeed(string mnemonic, string? passphrase)
    {
        var normalized = mnemonic.NormalizeWhitespace().ToLowerInvariant();
        var salt = "mnemonic" + (passphrase ?? string.Empty);
        return Pbkdf2Provider.Derive(
            Encoding.UTF8.GetBytes(normalized),
            Encoding.UTF8.GetBytes(salt),
            iterations: 2048,
            length: 64);
    }

    public byte[] DerivePrivateKey(byte[] masterSeed, string derivationPath) =>
        _bip32.DerivePrivateKey(masterSeed, derivationPath);

    public string DerivePublicAddress(byte[] privateKey, string chainType) =>
        chainType.ToLowerInvariant() switch
        {
            "evm" => DeriveEvmAddress(privateKey),
            "utxo" => DeriveUtxoAddress(privateKey),
            _ => DeriveEvmAddress(privateKey)
        };

    public bool ValidateMnemonic(string mnemonic)
    {
        var words = mnemonic.NormalizeWhitespace().Split(' ');
        if (words.Length is not (12 or 15 or 18 or 21 or 24))
        {
            return false;
        }

        return words.All(w => WordList.Contains(w, StringComparer.Ordinal));
    }

    public string NormalizeDerivationPath(string path, string networkPrefix) =>
        path.StartsWith("m/", StringComparison.Ordinal) ? path : $"m/{networkPrefix}/{path}";

    private static string DeriveEvmAddress(byte[] privateKey)
    {
        var hash = SHA256.HashData(privateKey);
        return "0x" + Convert.ToHexString(hash.AsSpan(0, 20)).ToLowerInvariant();
    }

    private static string DeriveUtxoAddress(byte[] privateKey)
    {
        var hash = Ripemd160Hasher.Hash(Sha256Hasher.Hash(privateKey));
        return Base58Encoder.Encode(hash);
    }

    private static string[] LoadWordList()
    {
        return Enumerable.Range(0, 2048)
            .Select(i => $"word{i:D4}")
            .ToArray();
    }
}

internal static class MnemonicExtensions
{
    public static string NormalizeWhitespace(this string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
