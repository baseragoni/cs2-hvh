using System.Text.Json;
using Cs2Hvh.Core;
using Microsoft.Extensions.Logging;

namespace Cs2Hvh.App;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var options = LoadOptions();
        Directory.CreateDirectory(options.DefaultVaultDirectory);

        using var loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole(o => o.SingleLine = true));
        var logger = loggerFactory.CreateLogger("cs2hvh");
        var wallet = WalletService.Create(options);

        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
            return Commands.Help();

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "import" => await Commands.ImportAsync(wallet, args, logger),
                "list" => await Commands.ListAsync(wallet),
                "sync" => await Commands.SyncAsync(wallet, args),
                "balance" => await Commands.BalanceAsync(wallet, args),
                "export" => await Commands.ExportAsync(wallet, args),
                "status" => await Commands.StatusAsync(wallet),
                "fee" => Commands.Fee(wallet, args),
                "networks" => Commands.Networks(wallet),
                "version" => Commands.Version(),
                _ => Unknown(args[0])
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "command failed");
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static int Unknown(string cmd)
    {
        Console.Error.WriteLine($"unknown command: {cmd}");
        return Commands.Help();
    }

    private static WalletOptions LoadOptions()
    {
        foreach (var path in CandidateSettingsPaths())
        {
            if (!File.Exists(path)) continue;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("Wallet", out var w))
                    return JsonSerializer.Deserialize<WalletOptions>(w.GetRawText(), JsonDefaults.Options) ?? new WalletOptions();
            }
            catch
            {
                // try next
            }
        }

        return new WalletOptions();
    }

    private static IEnumerable<string> CandidateSettingsPaths()
    {
        yield return Path.Combine(Directory.GetCurrentDirectory(), "appsettings.local.json");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        yield return Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");
        yield return Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }
}
