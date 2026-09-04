using Cs2Hvh.App.Output;
using Cs2Hvh.Engine.Orchestration;

namespace Cs2Hvh.App.Commands;

public sealed class ListCommand
{
    private readonly WalletManager _walletManager;

    public ListCommand(WalletManager walletManager)
    {
        _walletManager = walletManager;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var vaults = await _walletManager.ListVaultsAsync(CancellationToken.None);
        if (vaults.Count == 0)
        {
            ConsoleOutputFormatter.WriteInfo("No vaults found. Use `import` to create one.");
            return 0;
        }

        ConsoleOutputFormatter.WriteHeader("Stored Vaults");
        foreach (var vault in vaults)
        {
            ConsoleOutputFormatter.WriteKeyValue(vault.VaultId, vault.Label);
            Console.WriteLine($"    networks: {string.Join(", ", vault.EnabledNetworkIds)}");
            Console.WriteLine($"    created:  {vault.CreatedAt:u}");
        }

        return 0;
    }
}
