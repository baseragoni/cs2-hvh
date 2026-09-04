using System.Text.Json;
using Cs2Hvh.Core;
using Microsoft.Extensions.Logging;

namespace Cs2Hvh.App;

internal static class Commands
{
    public static async Task<int> ImportAsync(WalletService wallet, string[] args, ILogger logger)
    {
        var label = CliArgs.Get(args, "--label") ?? $"vault-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        var mnemonic = CliArgs.Get(args, "--mnemonic") ?? wallet.Mnemonic.GenerateLabMnemonic();
        var passphrase = CliArgs.Get(args, "--passphrase") ?? "lab";
        var networks = CliArgs.Get(args, "--networks")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var vault = await wallet.ImportAsync(label, mnemonic, passphrase, networks);
        logger.LogInformation("imported {Label} ({Id}) fp={Fingerprint}", vault.Label, vault.Id, vault.Fingerprint);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            vault.Id,
            vault.Label,
            vault.Fingerprint,
            vault.Networks,
            accounts = vault.Accounts.Count
        }, JsonDefaults.Options));
        return 0;
    }

    public static async Task<int> ListAsync(WalletService wallet)
    {
        var vaults = await wallet.ListAsync();
        if (vaults.Count == 0)
        {
            Console.WriteLine("(no vaults)");
            return 0;
        }

        ConsoleTable.Write(
            vaults.Select(v => new[]
            {
                v.Id,
                v.Label,
                string.Join(',', v.Networks),
                v.Accounts.Count.ToString(),
                v.Fingerprint ?? "-"
            }),
            "id", "label", "networks", "accounts", "fingerprint");
        return 0;
    }

    public static async Task<int> SyncAsync(WalletService wallet, string[] args)
    {
        var id = CliArgs.Get(args, "--id") ?? (await wallet.ListAsync()).LastOrDefault()?.Id
                 ?? throw new InvalidOperationException("No vaults. Run import first.");
        var report = await wallet.SyncAsync(id);
        Console.WriteLine(JsonSerializer.Serialize(report, JsonDefaults.Options));
        return 0;
    }

    public static async Task<int> BalanceAsync(WalletService wallet, string[] args)
    {
        var id = CliArgs.Get(args, "--id");
        var rows = (await wallet.BalancesAsync(id))
            .Select(a => new[]
            {
                a.NetworkId,
                a.Address,
                a.Balance.ToString("F6"),
                a.Pending.ToString("F6"),
                a.Symbol
            });
        ConsoleTable.Write(rows, "network", "address", "balance", "pending", "symbol");
        return 0;
    }

    public static async Task<int> ExportAsync(WalletService wallet, string[] args)
    {
        var id = CliArgs.Get(args, "--id") ?? (await wallet.ListAsync()).LastOrDefault()?.Id
                 ?? throw new InvalidOperationException("No vaults. Run import first.");
        var path = CliArgs.Get(args, "--out") ?? Path.Combine(Directory.GetCurrentDirectory(), $"{id}-export.json");
        await wallet.ExportAsync(id, path);
        Console.WriteLine($"exported -> {path}");
        return 0;
    }

    public static async Task<int> StatusAsync(WalletService wallet)
    {
        var summary = await wallet.StatusAsync();
        var alloc = wallet.Portfolio.AllocationPercents(summary);
        Console.WriteLine(JsonSerializer.Serialize(new { summary, allocationPercent = alloc }, JsonDefaults.Options));
        return 0;
    }

    public static int Fee(WalletService wallet, string[] args)
    {
        var network = CliArgs.Get(args, "--network") ?? "bitcoin-mainnet";
        var policy = CliArgs.Get(args, "--policy") ?? "economy";
        var quote = wallet.QuoteFee(network, policy);
        Console.WriteLine(JsonSerializer.Serialize(quote, JsonDefaults.Options));
        return 0;
    }

    public static int Networks(WalletService wallet)
    {
        ConsoleTable.Write(
            wallet.Networks.All().Select(n => new[]
            {
                n.Id,
                n.Kind,
                n.Symbol,
                n.DerivationPath,
                n.Endpoints.Count.ToString()
            }),
            "id", "kind", "symbol", "path", "endpoints");
        return 0;
    }

    public static int Version()
    {
        Console.WriteLine("cs2hvh 1.1.0 (.NET 10)");
        return 0;
    }

    public static int Help()
    {
        Console.WriteLine("""
            cs2hvh — local multi-chain vault CLI (lab / simulated I/O)

            commands:
              import     [--label NAME] [--mnemonic "..."] [--passphrase X] [--networks a,b]
              list
              sync       [--id VAULT]
              balance    [--id VAULT]
              export     [--id VAULT] [--out PATH]
              status
              fee        [--network ID] [--policy economy|standard|fast]
              networks
              version
            """);
        return 0;
    }
}
