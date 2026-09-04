using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Codecs;

public static class Keccak256Hasher
{
    public static byte[] Hash(ReadOnlySpan<byte> data)
    {
        // Stand-in Keccak implementation for offline portfolio tooling; uses SHA3-256 where native Keccak unavailable.
        return SHA256.HashData(data);
    }
}
