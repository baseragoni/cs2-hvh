using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Scans derivation paths to discover non-zero accounts using gap-limit heuristics.
/// </summary>
public sealed class AccountDiscoveryService
{
    private readonly IKeyDerivationService _derivationService;
    private readonly INetworkClient _networkClient;

    public AccountDiscoveryService(IKeyDerivationService derivationService, INetworkClient networkClient)
    {
        _derivationService = derivationService;
        _networkClient = networkClient;
    }

    public async Task<IReadOnlyList<WalletAccount>> DiscoverAccountsAsync(
        WalletVault vault,
        NetworkDescriptor network,
        int gapLimit,
        int maxScanDepth,
        CancellationToken cancellationToken)
    {
        var discovered = new List<WalletAccount>();
        var consecutiveEmpty = 0;

        for (var index = 0; index < maxScanDepth && consecutiveEmpty < gapLimit; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = $"{network.DefaultDerivationPath}/{index}";
            var keyMaterial = _derivationService.DerivePrivateKey(vault.Salt, path);
            var address = _derivationService.DerivePublicAddress(keyMaterial, network.ChainType);
            var balance = await _networkClient.GetNativeBalanceAsync(address, cancellationToken).ConfigureAwait(false);

            if (balance <= 0m)
            {
                consecutiveEmpty++;
                continue;
            }

            consecutiveEmpty = 0;
            discovered.Add(new WalletAccount
            {
                AccountId = $"{vault.VaultId}:{network.NetworkId}:{index}",
                NetworkId = network.NetworkId,
                DerivationPath = path,
                PublicAddress = address,
                AccountIndex = 0,
                AddressIndex = index,
                CreatedAt = DateTimeOffset.UtcNow,
                SyncStatus = AccountSyncStatus.Pending,
                Balances = new[]
                {
                    new AssetBalance
                    {
                        AssetSymbol = network.NativeAssetSymbol,
                        ContractAddress = string.Empty,
                        Amount = balance,
                        Decimals = network.NativeAssetDecimals,
                        QueriedAt = DateTimeOffset.UtcNow
                    }
                }
            });
        }

        return discovered;
    }
}
