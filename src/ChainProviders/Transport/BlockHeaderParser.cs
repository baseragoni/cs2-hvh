namespace Cs2Hvh.ChainProviders.Transport;

public static class BlockHeaderParser
{
    public static long ParseHexBlockNumber(string hexValue)
    {
        if (string.IsNullOrWhiteSpace(hexValue))
        {
            return 0;
        }

        var normalized = hexValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? hexValue[2..]
            : hexValue;

        return Convert.ToInt64(normalized, 16);
    }

    public static DateTimeOffset ParseTimestamp(long unixSeconds) =>
        DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
}
