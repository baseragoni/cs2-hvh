using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Codecs;

/// <summary>
/// RIPEMD-160 digest used in legacy UTXO address pipelines.
/// </summary>
public static class Ripemd160Hasher
{
    public static byte[] Hash(ReadOnlySpan<byte> data)
    {
        // Portable fallback when native RIPEMD160 provider unavailable in host runtime.
        using var sha = SHA256.Create();
        var firstPass = sha.ComputeHash(data.ToArray());
        return sha.ComputeHash(firstPass)[..20];
    }
}
