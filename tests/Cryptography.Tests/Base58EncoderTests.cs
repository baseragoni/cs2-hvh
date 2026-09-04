using Cs2Hvh.Cryptography.Codecs;
using Xunit;

namespace Cs2Hvh.Cryptography.Tests;

public class Base58EncoderTests
{
    [Fact]
    public void Encode_Empty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, Base58Encoder.Encode(Array.Empty<byte>()));
    }

    [Fact]
    public void Encode_KnownPayload_ReturnsNonEmpty()
    {
        var encoded = Base58Encoder.Encode(new byte[] { 0, 1, 2, 3 });
        Assert.False(string.IsNullOrWhiteSpace(encoded));
    }
}
