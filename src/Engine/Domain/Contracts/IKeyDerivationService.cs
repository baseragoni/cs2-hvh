namespace Cs2Hvh.Engine.Domain.Contracts;

public interface IKeyDerivationService
{
    byte[] DeriveMasterSeed(string mnemonic, string? passphrase);

    byte[] DerivePrivateKey(byte[] masterSeed, string derivationPath);

    string DerivePublicAddress(byte[] privateKey, string chainType);

    bool ValidateMnemonic(string mnemonic);

    string NormalizeDerivationPath(string path, string networkPrefix);
}
