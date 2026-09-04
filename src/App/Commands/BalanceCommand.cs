using Cs2Hvh.App.Output;
using Cs2Hvh.Engine.Orchestration;

namespace Cs2Hvh.App.Commands;

public sealed class BalanceCommand
{
    private readonly WalletManager _walletManager;

    public BalanceCommand(WalletManager walletManager)
    {
        _walletManager = walletManager;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var vaultId = args.Length > 1 ? args[1] : null;
        var vaults = await _walletManager.ListVaultsAsync(CancellationToken.None);
        vaultId ??= vaults.FirstOrDefault()?.VaultId;

        if (vaultId is null)
        {
            ConsoleOutputFormatter.WriteError("No vault found.");
            return 1;
        }

        var accounts = await _walletManager.GetAccountsAsync(vaultId, null, CancellationToken.None);
        ConsoleOutputFormatter.WriteHeader("Portfolio Balances");

        foreach (var group in accounts.GroupBy(a => a.NetworkId))
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{group.Key}]");
            Console.ResetColor();

            foreach (var account in group)
            {
                Console.WriteLine($"  {account.PublicAddress}");
                foreach (var balance in account.Balances)
                {
                    Console.WriteLine($"    {balance.DisplayAmount} {balance.AssetSymbol}");
                }

                if (account.Balances.Count == 0)
                {
                    Console.WriteLine("    (not synced — run `sync`)");
                }
            }
        }

        return 0;
    }
}
