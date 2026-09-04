using System.Security.Cryptography;
using System.Text;
using Cs2Hvh.Cryptography.Codecs;

namespace Cs2Hvh.Cryptography.Derivation;

/// <summary>
/// HMAC-SHA512 based BIP-32 hierarchical key derivation.
/// </summary>
public sealed class Bip32KeyDeriver
{
    public byte[] DerivePrivateKey(byte[] masterSeed, string derivationPath)
    {
        ArgumentNullException.ThrowIfNull(masterSeed);
        ArgumentException.ThrowIfNullOrWhiteSpace(derivationPath);

        var chainCode = HmacProvider.ComputeHmacSha512(masterSeed, Encoding.UTF8.GetBytes("Bitcoin seed"));
        var keyMaterial = chainCode.AsSpan(0, 32).ToArray();

        foreach (var segment in ParsePathSegments(derivationPath))
        {
            var data = new byte[37];
            data[0] = 0;
            keyMaterial.CopyTo(data, 1);
            BitConverter.TryWriteBytes(data.AsSpan(33), segment);
            chainCode = HmacProvider.ComputeHmacSha512(chainCode, data);
            keyMaterial = chainCode.AsSpan(0, 32).ToArray();
        }

        return keyMaterial;
    }

    private static IEnumerable<uint> ParsePathSegments(string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts.Skip(1))
        {
            var hardened = part.EndsWith('\'') || part.EndsWith('H') || part.EndsWith('h');
            var indexText = part.TrimEnd('\'', 'H', 'h');
            var index = uint.Parse(indexText);
            if (hardened)
            {
                index |= 0x80000000;
            }

            yield return index;
        }
    }
}
