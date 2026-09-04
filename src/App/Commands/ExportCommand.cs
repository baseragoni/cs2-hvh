using Cs2Hvh.App.Output;
using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Orchestration;

namespace Cs2Hvh.App.Commands;

public sealed class ExportCommand
{
    private readonly WalletManager _walletManager;
    private readonly IWalletStore _walletStore;

    public ExportCommand(WalletManager walletManager, IWalletStore walletStore)
    {
        _walletManager = walletManager;
        _walletStore = walletStore;
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

        var transactions = await _walletStore.GetTransactionsAsync(vaultId, null, 50, CancellationToken.None);
        ConsoleOutputFormatter.WriteHeader("Transaction Export (last 50)");

        foreach (var tx in transactions)
        {
            Console.WriteLine($"{tx.Timestamp:u}  {tx.Direction,-8}  {tx.Value,12:F8} {tx.AssetSymbol,-6}  {tx.TransactionHash}");
        }

        if (transactions.Count == 0)
        {
            ConsoleOutputFormatter.WriteInfo("No indexed transactions. Run `sync` first.");
        }

        return 0;
    }
}
