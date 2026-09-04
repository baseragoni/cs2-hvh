using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Cryptography.Derivation;

namespace Cs2Hvh.Engine.Orchestration;

public sealed class KeyDerivationService : IKeyDerivationService
{
    private readonly Bip39MnemonicProcessor _processor;

    public KeyDerivationService(Bip39MnemonicProcessor processor)
    {
        _processor = processor;
    }

    public byte[] DeriveMasterSeed(string mnemonic, string? passphrase) =>
        _processor.DeriveMasterSeed(mnemonic, passphrase);

    public byte[] DerivePrivateKey(byte[] masterSeed, string derivationPath) =>
        _processor.DerivePrivateKey(masterSeed, derivationPath);

    public string DerivePublicAddress(byte[] privateKey, string chainType) =>
        _processor.DerivePublicAddress(privateKey, chainType);

    public bool ValidateMnemonic(string mnemonic) =>
        _processor.ValidateMnemonic(mnemonic);

    public string NormalizeDerivationPath(string path, string networkPrefix) =>
        _processor.NormalizeDerivationPath(path, networkPrefix);
}
