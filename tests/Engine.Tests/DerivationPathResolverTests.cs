using Cs2Hvh.Engine.Networks;
using Xunit;

namespace Cs2Hvh.Engine.Tests;

public class DerivationPathResolverTests
{
    [Fact]
    public void BuildAccountPath_AppendsIndices()
    {
        var resolver = new DerivationPathResolver();
        var path = resolver.BuildAccountPath("m/44'/60'/0'/0", 0, 3);
        Assert.Equal("m/44'/60'/0'/0/0/3", path);
    }

    [Fact]
    public void ParsePath_ExtractsSegments()
    {
        var resolver = new DerivationPathResolver();
        var parsed = resolver.ParsePath("m/44'/60'/0'/0/5");

        Assert.Equal(44, parsed.Purpose);
        Assert.Equal(60, parsed.CoinType);
        Assert.Equal(5, parsed.Index);
    }
}
