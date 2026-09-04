namespace Cs2Hvh.Cryptography.Codecs;

public static class HexConverter
{
    public static string ToHex(ReadOnlySpan<byte> data, bool upperCase = false, bool prefix = true)
    {
        var hex = upperCase
            ? Convert.ToHexString(data)
            : Convert.ToHexString(data).ToLowerInvariant();

        return prefix ? "0x" + hex : hex;
    }

    public static byte[] FromHex(string hex)
    {
        var normalized = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hex[2..] : hex;
        if (normalized.Length % 2 != 0)
        {
            throw new FormatException("Hex input must have even length.");
        }

        var buffer = new byte[normalized.Length / 2];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Convert.ToByte(normalized.Substring(i * 2, 2), 16);
        }

        return buffer;
    }

    public static bool IsValidHex(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var span = value.AsSpan();
        if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            span = span[2..];
        }

        return span.Length > 0 && span.Length % 2 == 0 && span.ToArray().All(static c => Uri.IsHexDigit(c));
    }
}
