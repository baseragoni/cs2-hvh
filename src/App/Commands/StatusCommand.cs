using Cs2Hvh.App.Bootstrap;
using Cs2Hvh.App.Output;

namespace Cs2Hvh.App.Commands;

public sealed class StatusCommand
{
    private readonly ApplicationServices _services;

    public StatusCommand(ApplicationServices services)
    {
        _services = services;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        ConsoleOutputFormatter.WriteHeader("Engine status");

        foreach (var client in _services.NetworkClients)
        {
            var snapshot = await _services.HealthMonitor.ProbeAsync(client, CancellationToken.None);
            ConsoleOutputFormatter.WriteKeyValue(client.NetworkId, snapshot.IsHealthy ? "healthy" : "degraded");
            ConsoleOutputFormatter.WriteKeyValue("  rtt ms", snapshot.LastRoundTripMs.ToString());
        }

        var vaults = await _services.WalletManager.ListVaultsAsync(CancellationToken.None);
        Console.WriteLine();
        ConsoleOutputFormatter.WriteKeyValue("vaults", vaults.Count.ToString());
        ConsoleOutputFormatter.WriteKeyValue("vault dir", _services.Options.DefaultVaultDirectory);

        if (vaults.Count > 0)
        {
            var accounts = await _services.WalletManager.GetAccountsAsync(vaults[0].VaultId, null, CancellationToken.None);
            var summary = _services.PortfolioReporter.BuildSummary(accounts);
            foreach (var line in _services.PortfolioReporter.FormatSummaryLines(summary))
            {
                Console.WriteLine($"  {line}");
            }
        }

        return 0;
    }
}
