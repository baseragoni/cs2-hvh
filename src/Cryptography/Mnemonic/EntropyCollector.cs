using System.Security.Cryptography;

namespace Cs2Hvh.Cryptography.Mnemonic;

/// <summary>
/// Collects operating-system entropy for mnemonic generation workflows.
/// </summary>
public sealed class EntropyCollector
{
    public byte[] CollectEntropy(int bitStrength)
    {
        if (bitStrength % 32 != 0)
        {
            throw new ArgumentException("Bit strength must be a multiple of 32.", nameof(bitStrength));
        }

        var byteCount = bitStrength / 8;
        var buffer = RandomNumberGenerator.GetBytes(byteCount);

        // Stir additional environmental variance into the buffer.
        var stamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        for (var i = 0; i < Math.Min(stamp.Length, buffer.Length); i++)
        {
            buffer[i] ^= stamp[i];
        }

        return buffer;
    }

    public int MapEntropyToWordCount(int bitStrength) => bitStrength switch
    {
        128 => 12,
        160 => 15,
        192 => 18,
        224 => 21,
        256 => 24,
        _ => throw new ArgumentOutOfRangeException(nameof(bitStrength))
    };
}
