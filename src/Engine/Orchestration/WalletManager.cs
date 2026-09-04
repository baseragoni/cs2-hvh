using Cs2Hvh.Engine.Options;
using Cs2Hvh.Engine.Networks;
using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Central orchestrator for vault lifecycle, account derivation, and persistence coordination.
/// </summary>
public sealed class WalletManager
{
    private readonly IWalletProvider _walletProvider;
    private readonly IWalletStore _walletStore;
    private readonly NetworkRegistry _networkRegistry;
    private readonly WalletOptions _options;
    private readonly ILogger<WalletManager> _logger;

    public WalletManager(
        IWalletProvider walletProvider,
        IWalletStore walletStore,
        NetworkRegistry networkRegistry,
        IOptions<WalletOptions> options,
        ILogger<WalletManager> logger)
    {
        _walletProvider = walletProvider;
        _walletStore = walletStore;
        _networkRegistry = networkRegistry;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing wallet manager. Vault directory: {Directory}", _options.DefaultVaultDirectory);
        await _walletStore.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<WalletVault> ImportMnemonicAsync(
        string label,
        string mnemonic,
        string passphrase,
        IEnumerable<string>? networkIds,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(mnemonic);

        _logger.LogInformation("Creating vault from mnemonic import. Label={Label}", label);

        var vault = await _walletProvider.CreateVaultAsync(label, mnemonic, passphrase, cancellationToken)
            .ConfigureAwait(false);

        var enabled = networkIds?.ToArray() ?? _options.EnabledNetworks.ToArray();
        vault.EnabledNetworkIds = enabled;

        await _walletStore.SaveVaultAsync(vault, cancellationToken).ConfigureAwait(false);

        foreach (var networkId in enabled)
        {
            var network = _networkRegistry.GetNetwork(networkId);
            var accounts = await _walletProvider.DeriveAccountsAsync(
                vault,
                network,
                _options.DefaultAccountScanDepth,
                cancellationToken).ConfigureAwait(false);

            await _walletStore.SaveAccountsAsync(vault.VaultId, accounts, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation(
                "Derived {Count} accounts for network {NetworkId} in vault {VaultId}",
                accounts.Count,
                networkId,
                vault.VaultId);
        }

        return vault;
    }

    public async Task<IReadOnlyList<WalletVault>> ListVaultsAsync(CancellationToken cancellationToken) =>
        await _walletStore.ListVaultsAsync(cancellationToken).ConfigureAwait(false);

    public async Task<WalletVault?> GetVaultAsync(string vaultId, CancellationToken cancellationToken) =>
        await _walletStore.GetVaultAsync(vaultId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<WalletAccount>> GetAccountsAsync(
        string vaultId,
        string? networkId,
        CancellationToken cancellationToken) =>
        await _walletStore.GetAccountsAsync(vaultId, networkId, cancellationToken).ConfigureAwait(false);
}
