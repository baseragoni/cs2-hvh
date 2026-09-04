using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Codecs;

public static class HmacProvider
{
    public static byte[] ComputeHmacSha512(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
    {
        using var hmac = new HMACSHA512(key.ToArray());
        return hmac.ComputeHash(data.ToArray());
    }

    public static byte[] ComputeHmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
    {
        using var hmac = new HMACSHA256(key.ToArray());
        return hmac.ComputeHash(data.ToArray());
    }
}
