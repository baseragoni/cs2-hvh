namespace Cs2Hvh.ChainProviders.Transport;

public sealed class EndpointRotator
{
    private readonly Dictionary<string, string[]> _endpoints = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _cursors = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterNetwork(string networkId, IEnumerable<string> endpoints)
    {
        _endpoints[networkId] = endpoints.ToArray();
        _cursors[networkId] = 0;
    }

    public string GetNextEndpoint(string networkId)
    {
        if (!_endpoints.TryGetValue(networkId, out var list) || list.Length == 0)
        {
            throw new InvalidOperationException($"No RPC endpoints configured for {networkId}");
        }

        var cursor = _cursors[networkId];
        var endpoint = list[cursor % list.Length];
        _cursors[networkId] = (cursor + 1) % list.Length;
        return endpoint;
    }
}
