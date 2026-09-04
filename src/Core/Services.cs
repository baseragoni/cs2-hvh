namespace Cs2Hvh.Core;

public sealed class AccountDiscovery
{
    private readonly IAddressFactory _addresses;
    private readonly IChainClient _chain;
    private readonly WalletOptions _options;

    public AccountDiscovery(IAddressFactory addresses, IChainClient chain, WalletOptions options)
    {
        _addresses = addresses;
        _chain = chain;
        _options = options;
    }

    public async Task<List<WalletAccount>> DiscoverAsync(
        byte[] seed,
        NetworkDescriptor network,
        CancellationToken ct = default)
    {
        var found = new List<WalletAccount>();
        var emptyStreak = 0;

        for (var i = 0; i < _options.MaxAccountsPerNetwork; i++)
        {
            ct.ThrowIfCancellationRequested();
            var address = _addresses.Derive(seed, network, i);
            var balance = await _chain.FetchBalanceAsync(network.Id, address, ct);
            if (balance <= 0 && i > 0)
            {
                emptyStreak++;
                if (emptyStreak >= Math.Min(_options.GapLimit, 5))
                    break;
                continue;
            }

            emptyStreak = 0;
            found.Add(new WalletAccount
            {
                NetworkId = network.Id,
                Address = address,
                Index = i,
                Symbol = network.Symbol,
                Balance = balance,
                DerivationPath = $"{network.DerivationPath}/{i}",
                LastSyncedAt = DateTimeOffset.UtcNow
            });
        }

        if (found.Count == 0)
        {
            found.Add(new WalletAccount
            {
                NetworkId = network.Id,
                Address = _addresses.Derive(seed, network, 0),
                Index = 0,
                Symbol = network.Symbol,
                Balance = 0,
                DerivationPath = $"{network.DerivationPath}/0"
            });
        }

        return found;
    }
}

public sealed class SyncCoordinator
{
    private readonly IChainClient _chain;
    private readonly IVaultStore _store;

    public SyncCoordinator(IChainClient chain, IVaultStore store)
    {
        _chain = chain;
        _store = store;
    }

    public async Task<SyncReport> SyncVaultAsync(WalletVault vault, CancellationToken ct = default)
    {
        var started = DateTimeOffset.UtcNow;
        var warnings = new List<string>();
        var endpoints = 0;

        foreach (var account in vault.Accounts)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                if (!await _chain.PingAsync(account.NetworkId, ct))
                    warnings.Add($"ping failed: {account.NetworkId}");
                endpoints++;
                account.Balance = await _chain.FetchBalanceAsync(account.NetworkId, account.Address, ct);
                account.Pending = Math.Round(account.Balance * 0.01m, 8);
                account.LastSyncedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                warnings.Add($"{account.Address}: {ex.Message}");
            }
        }

        await _store.SaveAsync(vault, ct);
        return new SyncReport
        {
            VaultId = vault.Id,
            AccountsTouched = vault.Accounts.Count,
            EndpointsTried = endpoints,
            TotalBalance = vault.Accounts.Sum(a => a.Balance),
            Duration = DateTimeOffset.UtcNow - started,
            Warnings = warnings
        };
    }
}

public sealed class ExportService
{
    private readonly IVaultStore _store;
    private readonly IChainClient _chain;

    public ExportService(IVaultStore store, IChainClient chain)
    {
        _store = store;
        _chain = chain;
    }

    public async Task ExportTransactionsAsync(string vaultId, string outputPath, int limit = 20, CancellationToken ct = default)
    {
        var vault = await _store.LoadAsync(vaultId, ct)
                    ?? throw new InvalidOperationException($"Vault '{vaultId}' not found.");

        var rows = new List<TransactionRecord>();
        foreach (var account in vault.Accounts)
        {
            var recent = await _chain.FetchRecentAsync(account.NetworkId, account.Address, limit, ct);
            rows.AddRange(recent);
        }

        rows = rows.OrderByDescending(r => r.Timestamp).Take(limit * Math.Max(1, vault.Accounts.Count)).ToList();
        await File.WriteAllTextAsync(outputPath, System.Text.Json.JsonSerializer.Serialize(rows, JsonDefaults.Options), ct);
    }

    public async Task ExportVaultManifestAsync(string vaultId, string outputPath, CancellationToken ct = default)
    {
        var vault = await _store.LoadAsync(vaultId, ct)
                    ?? throw new InvalidOperationException($"Vault '{vaultId}' not found.");

        var manifest = new
        {
            vault.Id,
            vault.Label,
            vault.Fingerprint,
            vault.Networks,
            accounts = vault.Accounts.Select(a => new { a.NetworkId, a.Address, a.Index, a.Symbol })
        };

        await File.WriteAllTextAsync(outputPath, System.Text.Json.JsonSerializer.Serialize(manifest, JsonDefaults.Options), ct);
    }
}
