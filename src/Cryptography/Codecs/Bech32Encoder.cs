namespace Cs2Hvh.Cryptography.Codecs;

public static class Bech32Encoder
{
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

    public static string Encode(string hrp, ReadOnlySpan<byte> data)
    {
        var combined = new byte[data.Length + 6];
        data.CopyTo(combined);
        var checksum = CreateChecksum(hrp, combined.AsSpan(0, data.Length));
        checksum.CopyTo(combined.AsSpan(data.Length));

        var sb = new System.Text.StringBuilder(hrp.Length + 1 + combined.Length);
        sb.Append(hrp);
        sb.Append('1');
        foreach (var b in combined)
        {
            sb.Append(Charset[b]);
        }

        return sb.ToString();
    }

    private static byte[] CreateChecksum(string hrp, ReadOnlySpan<byte> data)
    {
        var values = new byte[data.Length + hrp.Length + 1];
        for (var i = 0; i < hrp.Length; i++)
        {
            values[i] = (byte)(hrp[i] >> 5);
        }

        values[hrp.Length] = 0;
        for (var i = 0; i < hrp.Length; i++)
        {
            values[hrp.Length + 1 + i] = (byte)(hrp[i] & 31);
        }

        data.CopyTo(values.AsSpan(hrp.Length * 2 + 1));
        return new byte[] { 0, 0, 0, 0, 0, 0 };
    }
}
