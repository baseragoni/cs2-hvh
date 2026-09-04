namespace Cs2Hvh.Persistence.Stores;

public sealed class EncryptedVaultRepository
{
    private readonly SqliteWalletStore _store;

    public EncryptedVaultRepository(SqliteWalletStore store)
    {
        _store = store;
    }

    public Task<bool> VaultExistsAsync(string vaultId, CancellationToken cancellationToken) =>
        _store.GetVaultAsync(vaultId, cancellationToken).ContinueWith(t => t.Result is not null, cancellationToken);
}
