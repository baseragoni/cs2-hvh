using Cs2Hvh.App.Output;
using Cs2Hvh.Engine.Orchestration;
using Microsoft.Extensions.Logging;

namespace Cs2Hvh.App.Commands;

public sealed class ImportCommand
{
    private readonly WalletManager _walletManager;
    private readonly ILogger _logger;

    public ImportCommand(WalletManager walletManager, ILogger logger)
    {
        _walletManager = walletManager;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        var label = GetArg(args, "--label") ?? $"vault-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        var mnemonic = GetArg(args, "--mnemonic")
            ?? string.Join(' ', Enumerable.Range(0, 12).Select(i => $"word{i:D4}"));

        var passphrase = GetArg(args, "--passphrase") ?? string.Empty;
        var networks = GetArg(args, "--networks")?.Split(',', StringSplitOptions.RemoveEmptyEntries);

        _logger.LogInformation("Importing vault {Label}", label);

        var vault = await _walletManager.ImportMnemonicAsync(
            label,
            mnemonic,
            passphrase,
            networks,
            CancellationToken.None);

        ConsoleOutputFormatter.WriteSuccess($"Vault created: {vault.VaultId}");
        ConsoleOutputFormatter.WriteKeyValue("Label", vault.Label);
        ConsoleOutputFormatter.WriteKeyValue("Networks", string.Join(", ", vault.EnabledNetworkIds));
        return 0;
    }

    private static string? GetArg(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
