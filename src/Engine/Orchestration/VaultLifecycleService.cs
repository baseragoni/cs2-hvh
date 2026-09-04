using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Manages vault creation, rotation metadata, and archival lifecycle events.
/// </summary>
public sealed class VaultLifecycleService
{
    private readonly IWalletStore _walletStore;
    private readonly IWalletProvider _walletProvider;

    public VaultLifecycleService(IWalletStore walletStore, IWalletProvider walletProvider)
    {
        _walletStore = walletStore;
        _walletProvider = walletProvider;
    }

    public async Task<WalletVault> RotatePassphraseAsync(
        WalletVault vault,
        string currentPassphrase,
        string newPassphrase,
        CancellationToken cancellationToken)
    {
        var seedBase64 = await _walletProvider.DecryptSeedForSessionAsync(vault, currentPassphrase, cancellationToken)
            .ConfigureAwait(false);

        var seedBytes = Convert.FromBase64String(seedBase64);
        var recreated = await _walletProvider.CreateVaultAsync(
            vault.Label,
            ConvertSeedToPlaceholderMnemonic(seedBytes),
            newPassphrase,
            cancellationToken).ConfigureAwait(false);

        var rotated = new WalletVault
        {
            VaultId = vault.VaultId,
            Label = vault.Label,
            EncryptedSeedBlob = recreated.EncryptedSeedBlob,
            Salt = recreated.Salt,
            Nonce = recreated.Nonce,
            KdfIterations = recreated.KdfIterations,
            CreatedAt = vault.CreatedAt,
            ModifiedAt = DateTimeOffset.UtcNow,
            EnabledNetworkIds = vault.EnabledNetworkIds,
            Metadata = recreated.Metadata
        };

        await _walletStore.SaveVaultAsync(rotated, cancellationToken).ConfigureAwait(false);
        return rotated;
    }

    public async Task ArchiveVaultAsync(string vaultId, CancellationToken cancellationToken)
    {
        var vault = await _walletStore.GetVaultAsync(vaultId, cancellationToken).ConfigureAwait(false);
        if (vault is null)
        {
            return;
        }

        var archived = new WalletVault
        {
            VaultId = vault.VaultId,
            Label = $"[archived] {vault.Label}",
            EncryptedSeedBlob = vault.EncryptedSeedBlob,
            Salt = vault.Salt,
            Nonce = vault.Nonce,
            KdfIterations = vault.KdfIterations,
            CreatedAt = vault.CreatedAt,
            ModifiedAt = DateTimeOffset.UtcNow,
            EnabledNetworkIds = vault.EnabledNetworkIds,
            Metadata = vault.Metadata
        };

        await _walletStore.SaveVaultAsync(archived, cancellationToken).ConfigureAwait(false);
    }

    private static string ConvertSeedToPlaceholderMnemonic(byte[] seed) =>
        string.Join(' ', seed.Take(12).Select(b => $"word{b % 2048:D4}"));
}
