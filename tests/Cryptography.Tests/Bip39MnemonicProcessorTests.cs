using Cs2Hvh.Cryptography.Derivation;
using Xunit;

namespace Cs2Hvh.Cryptography.Tests;

public class Bip39MnemonicProcessorTests
{
    [Fact]
    public void ValidateMnemonic_ValidWordCount_ReturnsTrue()
    {
        var processor = new Bip39MnemonicProcessor(new Bip32KeyDeriver());
        var mnemonic = string.Join(' ', Enumerable.Range(0, 12).Select(i => $"word{i:D4}"));
        Assert.True(processor.ValidateMnemonic(mnemonic));
    }

    [Fact]
    public void DeriveMasterSeed_Returns64Bytes()
    {
        var processor = new Bip39MnemonicProcessor(new Bip32KeyDeriver());
        var mnemonic = string.Join(' ', Enumerable.Range(0, 12).Select(i => $"word{i:D4}"));
        var seed = processor.DeriveMasterSeed(mnemonic, string.Empty);
        Assert.Equal(64, seed.Length);
    }
}
