namespace Cs2Hvh.Engine.Domain.Extensions;

public static class StringExtensions
{
    public static string TruncateMiddle(this string value, int headLength = 6, int tailLength = 4)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (value.Length <= headLength + tailLength + 3)
        {
            return value;
        }

        return string.Concat(value.AsSpan(0, headLength), "...", value.AsSpan(value.Length - tailLength));
    }

    public static bool LooksLikeHexAddress(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? value[2..]
            : value;

        return trimmed.Length is 40 or 64 && trimmed.All(static c =>
            char.IsAsciiHexDigit(c));
    }

    public static string NormalizeWhitespace(this string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
