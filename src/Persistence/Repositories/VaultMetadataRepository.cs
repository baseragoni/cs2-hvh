using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.Persistence.Stores;

namespace Cs2Hvh.Persistence.Repositories;

public sealed class VaultMetadataRepository
{
    private readonly SqliteWalletStore _store;

    public VaultMetadataRepository(SqliteWalletStore store)
    {
        _store = store;
    }

    public Task SaveAsync(WalletVault vault, CancellationToken cancellationToken) =>
        _store.SaveVaultAsync(vault, cancellationToken);

    public Task<WalletVault?> GetAsync(string vaultId, CancellationToken cancellationToken) =>
        _store.GetVaultAsync(vaultId, cancellationToken);

    public Task<IReadOnlyList<WalletVault>> ListAsync(CancellationToken cancellationToken) =>
        _store.ListVaultsAsync(cancellationToken);
}
