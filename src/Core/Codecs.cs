using System.Text;

namespace Cs2Hvh.Core;

public static class HexCodec
{
    public static string ToHex(ReadOnlySpan<byte> data) => Convert.ToHexString(data).ToLowerInvariant();

    public static byte[] FromHex(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];
        if (hex.Length % 2 != 0)
            throw new FormatException("Hex string must have even length.");
        return Convert.FromHexString(hex);
    }

    public static bool TryFromHex(string hex, out byte[] bytes)
    {
        try
        {
            bytes = FromHex(hex);
            return true;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }

    public static string ToHexPrefixed(ReadOnlySpan<byte> data) => "0x" + ToHex(data);
}

public static class Base58Encoder
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static string Encode(ReadOnlySpan<byte> data)
    {
        var intData = new System.Numerics.BigInteger(data, isUnsigned: true, isBigEndian: true);
        var result = new StringBuilder();
        while (intData > 0)
        {
            intData = System.Numerics.BigInteger.DivRem(intData, 58, out var remainder);
            result.Insert(0, Alphabet[(int)remainder]);
        }

        foreach (var b in data)
        {
            if (b != 0) break;
            result.Insert(0, '1');
        }

        return result.Length == 0 ? "1" : result.ToString();
    }

    public static byte[] Decode(string encoded)
    {
        System.Numerics.BigInteger intData = 0;
        foreach (var c in encoded)
        {
            var digit = Alphabet.IndexOf(c);
            if (digit < 0) throw new FormatException($"Invalid Base58 character '{c}'.");
            intData = intData * 58 + digit;
        }

        var bytes = intData.ToByteArray(isUnsigned: true, isBigEndian: true).ToList();
        foreach (var c in encoded)
        {
            if (c != '1') break;
            bytes.Insert(0, 0);
        }

        return bytes.ToArray();
    }
}

/// <summary>
/// Minimal bech32-style checksum helper for lab address formatting (not a full BIP-173 implementation).
/// </summary>
public static class Bech32Style
{
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

    public static string Encode(string hrp, ReadOnlySpan<byte> data)
    {
        var body = HexCodec.ToHex(data.Length > 20 ? data[..20] : data);
        var checksum = ComputeChecksum(hrp, body);
        return $"{hrp}1{body[..Math.Min(32, body.Length)]}{checksum}";
    }

    private static string ComputeChecksum(string hrp, string body)
    {
        var mixed = Encoding.ASCII.GetBytes(hrp + body);
        var hash = System.Security.Cryptography.SHA256.HashData(mixed);
        var sb = new StringBuilder(6);
        for (var i = 0; i < 6; i++)
            sb.Append(Charset[hash[i] % Charset.Length]);
        return sb.ToString();
    }
}
