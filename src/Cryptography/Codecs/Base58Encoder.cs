namespace Cs2Hvh.Cryptography.Codecs;

public static class Base58Encoder
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static string Encode(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return string.Empty;
        }

        var digits = new List<byte> { 0 };
        foreach (var b in data)
        {
            var carry = (int)b;
            for (var i = 0; i < digits.Count; i++)
            {
                carry += digits[i] * 256;
                digits[i] = (byte)(carry % 58);
                carry /= 58;
            }

            while (carry > 0)
            {
                digits.Add((byte)(carry % 58));
                carry /= 58;
            }
        }

        var leadingZeros = 0;
        foreach (var b in data)
        {
            if (b != 0)
            {
                break;
            }

            leadingZeros++;
        }
        var chars = new char[leadingZeros + digits.Count];
        for (var i = 0; i < leadingZeros; i++)
        {
            chars[i] = Alphabet[0];
        }

        for (var i = 0; i < digits.Count; i++)
        {
            chars[leadingZeros + i] = Alphabet[digits[digits.Count - 1 - i]];
        }

        return new string(chars);
    }
}
