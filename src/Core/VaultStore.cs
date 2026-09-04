using System.Text.Json;

namespace Cs2Hvh.Core;

public sealed class VaultStore : IVaultStore
{
    private readonly string _root;

    public VaultStore(string rootDirectory)
    {
        _root = rootDirectory;
        Directory.CreateDirectory(_root);
    }

    public string Root => _root;

    public async Task SaveAsync(WalletVault vault, CancellationToken ct = default)
    {
        MigrationRunner.EnsureCurrent(vault);
        vault.UpdatedAt = DateTimeOffset.UtcNow;
        var path = PathFor(vault.Id);
        var tmp = path + ".tmp";
        await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(vault, JsonDefaults.Options), ct);
        File.Copy(tmp, path, overwrite: true);
        File.Delete(tmp);
    }

    public async Task<WalletVault?> LoadAsync(string id, CancellationToken ct = default)
    {
        var path = PathFor(id);
        if (!File.Exists(path)) return null;
        var json = await File.ReadAllTextAsync(path, ct);
        var vault = JsonSerializer.Deserialize<WalletVault>(json, JsonDefaults.Options);
        if (vault is not null)
            MigrationRunner.EnsureCurrent(vault);
        return vault;
    }

    public async Task<IReadOnlyList<WalletVault>> ListAsync(CancellationToken ct = default)
    {
        var list = new List<WalletVault>();
        foreach (var file in Directory.EnumerateFiles(_root, "*.json"))
        {
            if (file.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)) continue;
            var json = await File.ReadAllTextAsync(file, ct);
            var vault = JsonSerializer.Deserialize<WalletVault>(json, JsonDefaults.Options);
            if (vault is null) continue;
            MigrationRunner.EnsureCurrent(vault);
            list.Add(vault);
        }

        return list.OrderBy(v => v.CreatedAt).ToList();
    }

    public Task DeleteAsync(string id)
    {
        var path = PathFor(id);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string PathFor(string id) => Path.Combine(_root, $"{id}.json");
}

public static class MigrationRunner
{
    public const int CurrentSchema = 2;

    public static void EnsureCurrent(WalletVault vault)
    {
        if (vault.SchemaVersion < 1)
            vault.SchemaVersion = 1;

        if (vault.SchemaVersion < 2)
        {
            vault.Metadata.TryAdd("migratedTo", "2");
            foreach (var account in vault.Accounts)
            {
                if (string.IsNullOrEmpty(account.DerivationPath))
                    account.DerivationPath = $"{account.NetworkId}/{account.Index}";
            }
            vault.SchemaVersion = 2;
        }
    }
}
