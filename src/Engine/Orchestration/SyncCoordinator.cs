using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.Engine.Analytics;
using Cs2Hvh.Engine.Networks;
using Microsoft.Extensions.Logging;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Coordinates multi-network balance and transaction synchronization pipelines.
/// </summary>
public sealed class SyncCoordinator
{
    private readonly IWalletStore _walletStore;
    private readonly NetworkRegistry _networkRegistry;
    private readonly IEnumerable<INetworkClient> _networkClients;
    private readonly BalanceAggregator _balanceAggregator;
    private readonly ILogger<SyncCoordinator> _logger;

    public SyncCoordinator(
        IWalletStore walletStore,
        NetworkRegistry networkRegistry,
        IEnumerable<INetworkClient> networkClients,
        BalanceAggregator balanceAggregator,
        ILogger<SyncCoordinator> logger)
    {
        _walletStore = walletStore;
        _networkRegistry = networkRegistry;
        _networkClients = networkClients;
        _balanceAggregator = balanceAggregator;
        _logger = logger;
    }

    public async Task<SyncState> RunFullSyncAsync(
        string vaultId,
        string networkId,
        CancellationToken cancellationToken)
    {
        var network = _networkRegistry.GetNetwork(networkId);
        var client = ResolveClient(networkId);

        var state = await _walletStore.GetSyncStateAsync(vaultId, networkId, cancellationToken).ConfigureAwait(false)
            ?? new SyncState
            {
                VaultId = vaultId,
                NetworkId = networkId,
                LastSyncStartedAt = DateTimeOffset.UtcNow
            };

        state.CurrentPhase = SyncPhase.DiscoveringAccounts;
        state.LastSyncStartedAt = DateTimeOffset.UtcNow;
        state.LastErrorMessage = null;
        await _walletStore.SaveSyncStateAsync(state, cancellationToken).ConfigureAwait(false);

        try
        {
            var accounts = await _walletStore.GetAccountsAsync(vaultId, networkId, cancellationToken)
                .ConfigureAwait(false);

            state.AccountsDiscovered = accounts.Count;
            state.CurrentPhase = SyncPhase.FetchingBalances;

            var updatedAccounts = new List<WalletAccount>();
            foreach (var account in accounts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var balances = await _balanceAggregator.FetchBalancesAsync(client, network, account, cancellationToken)
                    .ConfigureAwait(false);

                account.Balances = balances;
                account.LastSyncedAt = DateTimeOffset.UtcNow;
                account.SyncStatus = AccountSyncStatus.Synced;
                updatedAccounts.Add(account);
            }

            await _walletStore.SaveAccountsAsync(vaultId, updatedAccounts, cancellationToken).ConfigureAwait(false);

            state.CurrentPhase = SyncPhase.IndexingTransactions;
            var latestBlock = await client.GetLatestBlockNumberAsync(cancellationToken).ConfigureAwait(false);
            var fromBlock = Math.Max(0, state.LastProcessedBlock);

            var allTransactions = new List<TransactionRecord>();
            foreach (var account in updatedAccounts)
            {
                var txs = await client.GetTransactionsAsync(
                    account.PublicAddress,
                    fromBlock,
                    latestBlock,
                    cancellationToken).ConfigureAwait(false);

                allTransactions.AddRange(txs);
            }

            await _walletStore.CacheTransactionsAsync(vaultId, allTransactions, cancellationToken)
                .ConfigureAwait(false);

            state.TransactionsIndexed += allTransactions.Count;
            state.LastProcessedBlock = latestBlock;
            state.CurrentPhase = SyncPhase.Completed;
            state.LastSyncCompletedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Sync completed for vault {VaultId} on {NetworkId}. Block={Block}, Txs={TxCount}",
                vaultId,
                networkId,
                latestBlock,
                allTransactions.Count);
        }
        catch (Exception ex)
        {
            state.CurrentPhase = SyncPhase.Failed;
            state.LastErrorMessage = ex.Message;
            _logger.LogError(ex, "Sync failed for vault {VaultId} network {NetworkId}", vaultId, networkId);
        }

        await _walletStore.SaveSyncStateAsync(state, cancellationToken).ConfigureAwait(false);
        return state;
    }

    private INetworkClient ResolveClient(string networkId) =>
        _networkClients.FirstOrDefault(c => c.NetworkId == networkId)
        ?? throw new InvalidOperationException($"No network client registered for '{networkId}'.");
}
