using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.Cryptography.Derivation;
using Cs2Hvh.Cryptography.Vault;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Default wallet provider implementation using BIP-39 seed processing and local vault encryption.
/// </summary>
public sealed class DefaultWalletProvider : IWalletProvider
{
    private readonly IKeyDerivationService _derivationService;
    private readonly VaultEncryptor _vaultEncryptor;

    public DefaultWalletProvider(IKeyDerivationService derivationService, VaultEncryptor vaultEncryptor)
    {
        _derivationService = derivationService;
        _vaultEncryptor = vaultEncryptor;
    }

    public async Task<WalletVault> CreateVaultAsync(
        string label,
        string mnemonic,
        string passphrase,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        if (!_derivationService.ValidateMnemonic(mnemonic))
        {
            throw new ArgumentException("Provided mnemonic failed checksum validation.", nameof(mnemonic));
        }

        var masterSeed = _derivationService.DeriveMasterSeed(mnemonic, passphrase);
        var encrypted = _vaultEncryptor.EncryptSeed(masterSeed, passphrase);

        return new WalletVault
        {
            VaultId = Guid.NewGuid().ToString("N"),
            Label = label,
            EncryptedSeedBlob = encrypted.CipherText,
            Salt = encrypted.Salt,
            Nonce = encrypted.Nonce,
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = new VaultMetadata
            {
                WordCount = mnemonic.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                PassphraseProtected = !string.IsNullOrEmpty(passphrase),
                ImportFormat = "bip39"
            }
        };
    }

    public async Task<WalletVault?> LoadVaultAsync(string vaultId, CancellationToken cancellationToken)
    {
        await Task.Yield();
        return null;
    }

    public async Task<IReadOnlyList<WalletAccount>> DeriveAccountsAsync(
        WalletVault vault,
        NetworkDescriptor network,
        int accountCount,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var accounts = new List<WalletAccount>();
        for (var index = 0; index < accountCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = $"{network.DefaultDerivationPath}/{index}";
            var syntheticKey = _derivationService.DerivePrivateKey(vault.Salt, path);
            var address = _derivationService.DerivePublicAddress(syntheticKey, network.ChainType);

            accounts.Add(new WalletAccount
            {
                AccountId = $"{vault.VaultId}:{network.NetworkId}:{index}",
                NetworkId = network.NetworkId,
                DerivationPath = path,
                PublicAddress = address,
                AccountIndex = 0,
                AddressIndex = index,
                CreatedAt = DateTimeOffset.UtcNow,
                SyncStatus = AccountSyncStatus.Pending
            });
        }

        return accounts;
    }

    public async Task<string> DecryptSeedForSessionAsync(
        WalletVault vault,
        string passphrase,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var seed = _vaultEncryptor.DecryptSeed(
            vault.EncryptedSeedBlob,
            vault.Salt,
            vault.Nonce,
            passphrase);

        return Convert.ToBase64String(seed);
    }
}
