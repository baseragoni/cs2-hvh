namespace Cs2Hvh.App.Bootstrap;

public sealed class CommandRouter
{
    private readonly Dictionary<string, Func<string[], Task<int>>> _routes;

    public CommandRouter(Dictionary<string, Func<string[], Task<int>>> routes)
    {
        _routes = routes;
    }

    public async Task<int> RouteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            return -1;
        }

        var command = args[0].ToLowerInvariant();
        if (!_routes.TryGetValue(command, out var handler))
        {
            return -2;
        }

        return await handler(args).ConfigureAwait(false);
    }

    public IReadOnlyList<string> GetRegisteredCommands() => _routes.Keys.OrderBy(k => k).ToList();
}
