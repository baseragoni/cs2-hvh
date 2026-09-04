using System.Text;

namespace Cs2Hvh.Engine.Domain.Extensions;

public static class ByteArrayExtensions
{
    public static string ToHexString(this ReadOnlySpan<byte> bytes, bool prefix = true)
    {
        var builder = new StringBuilder(bytes.Length * 2 + (prefix ? 2 : 0));
        if (prefix)
        {
            builder.Append("0x");
        }

        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    public static byte[] FromHexString(string hex)
    {
        ArgumentException.ThrowIfNullOrEmpty(hex);

        var normalized = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? hex[2..]
            : hex;

        if (normalized.Length % 2 != 0)
        {
            throw new FormatException("Hex string must have an even number of characters.");
        }

        var buffer = new byte[normalized.Length / 2];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Convert.ToByte(normalized.Substring(i * 2, 2), 16);
        }

        return buffer;
    }

    public static bool ConstantTimeEquals(this ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        var diff = 0;
        for (var i = 0; i < left.Length; i++)
        {
            diff |= left[i] ^ right[i];
        }

        return diff == 0;
    }
}
