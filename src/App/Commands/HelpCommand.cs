using Cs2Hvh.App.Output;

namespace Cs2Hvh.App.Commands;

public sealed class HelpCommand
{
    private readonly IReadOnlyList<string> _commands;

    public HelpCommand(IReadOnlyList<string> commands)
    {
        _commands = commands;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        ConsoleOutputFormatter.WriteHeader("cs2hvh — command reference");
        Console.WriteLine();
        Console.WriteLine("  import    Create encrypted vault from BIP-39 mnemonic");
        Console.WriteLine("  list      List vault metadata stored locally");
        Console.WriteLine("  sync      Run multi-network balance and transaction sync");
        Console.WriteLine("  balance   Show cached native balances per derived account");
        Console.WriteLine("  export    Dump recent indexed transactions to stdout");
        Console.WriteLine("  status    Probe RPC endpoint health and sync cursors");
        Console.WriteLine("  version   Print build and runtime information");
        Console.WriteLine();
        Console.WriteLine("Registered handlers: " + string.Join(", ", _commands));
        return Task.FromResult(0);
    }
}
