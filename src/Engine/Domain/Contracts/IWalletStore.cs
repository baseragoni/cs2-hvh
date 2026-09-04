using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Domain.Contracts;

public interface IWalletStore
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task SaveVaultAsync(WalletVault vault, CancellationToken cancellationToken);

    Task<WalletVault?> GetVaultAsync(string vaultId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WalletVault>> ListVaultsAsync(CancellationToken cancellationToken);

    Task SaveAccountsAsync(string vaultId, IEnumerable<WalletAccount> accounts, CancellationToken cancellationToken);

    Task<IReadOnlyList<WalletAccount>> GetAccountsAsync(string vaultId, string? networkId, CancellationToken cancellationToken);

    Task SaveSyncStateAsync(SyncState state, CancellationToken cancellationToken);

    Task<SyncState?> GetSyncStateAsync(string vaultId, string networkId, CancellationToken cancellationToken);

    Task CacheTransactionsAsync(string vaultId, IEnumerable<TransactionRecord> transactions, CancellationToken cancellationToken);

    Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(string vaultId, string? networkId, int limit, CancellationToken cancellationToken);
}
