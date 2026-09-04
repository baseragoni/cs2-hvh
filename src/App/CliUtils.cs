namespace Cs2Hvh.App;

internal static class CliArgs
{
    public static string? Get(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    public static bool HasFlag(string[] args, string name) =>
        args.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static string[] Rest(string[] args) =>
        args.Length <= 1 ? [] : args[1..];
}

internal static class ConsoleTable
{
    public static void Write(IEnumerable<string[]> rows, params string[] headers)
    {
        var data = rows.ToList();
        var cols = headers.Length;
        var widths = new int[cols];
        for (var c = 0; c < cols; c++)
            widths[c] = headers[c].Length;

        foreach (var row in data)
        {
            for (var c = 0; c < cols && c < row.Length; c++)
                widths[c] = Math.Max(widths[c], row[c]?.Length ?? 0);
        }

        Console.WriteLine(string.Join("  ", headers.Select((h, i) => h.PadRight(widths[i]))));
        Console.WriteLine(string.Join("  ", widths.Select(w => new string('-', w))));
        foreach (var row in data)
        {
            var cells = new string[cols];
            for (var c = 0; c < cols; c++)
                cells[c] = (c < row.Length ? row[c] ?? "" : "").PadRight(widths[c]);
            Console.WriteLine(string.Join("  ", cells));
        }
    }
}
