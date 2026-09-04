using Cs2Hvh.App.Output;
using Cs2Hvh.Engine.Orchestration;

namespace Cs2Hvh.App.Commands;

public sealed class SyncCommand
{
    private readonly SyncCoordinator _syncCoordinator;
    private readonly WalletManager _walletManager;

    public SyncCommand(SyncCoordinator syncCoordinator, WalletManager walletManager)
    {
        _syncCoordinator = syncCoordinator;
        _walletManager = walletManager;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var vaultId = args.Length > 1 ? args[1] : null;
        if (vaultId is null)
        {
            var vaults = await _walletManager.ListVaultsAsync(CancellationToken.None);
            vaultId = vaults.FirstOrDefault()?.VaultId;
        }

        if (vaultId is null)
        {
            ConsoleOutputFormatter.WriteError("No vault available. Run import first.");
            return 1;
        }

        var vault = await _walletManager.GetVaultAsync(vaultId, CancellationToken.None);
        if (vault is null)
        {
            ConsoleOutputFormatter.WriteError($"Vault not found: {vaultId}");
            return 1;
        }

        ConsoleOutputFormatter.WriteHeader($"Syncing vault {vault.Label}");
        foreach (var networkId in vault.EnabledNetworkIds)
        {
            Console.WriteLine($"  → {networkId} ...");
            var state = await _syncCoordinator.RunFullSyncAsync(vaultId, networkId, CancellationToken.None);
            ConsoleOutputFormatter.WriteKeyValue("phase", state.CurrentPhase.ToString());
            ConsoleOutputFormatter.WriteKeyValue("accounts", state.AccountsDiscovered.ToString());
            ConsoleOutputFormatter.WriteKeyValue("transactions", state.TransactionsIndexed.ToString());
            ConsoleOutputFormatter.WriteKeyValue("block", state.LastProcessedBlock.ToString());
        }

        return 0;
    }
}
