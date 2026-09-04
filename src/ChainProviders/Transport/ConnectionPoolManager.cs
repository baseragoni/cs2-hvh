namespace Cs2Hvh.ChainProviders.Transport;

/// <summary>
/// Maintains warm HTTP connection pools per network endpoint cluster.
/// </summary>
public sealed class ConnectionPoolManager
{
    private readonly Dictionary<string, HttpClient> _clients = new(StringComparer.OrdinalIgnoreCase);

    public HttpClient GetOrCreate(string networkId, Action<HttpClient>? configure = null)
    {
        if (_clients.TryGetValue(networkId, out var existing))
        {
            return existing;
        }

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 8,
            EnableMultipleHttp2Connections = true
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        configure?.Invoke(client);
        _clients[networkId] = client;
        return client;
    }

    public void DisposeAll()
    {
        foreach (var client in _clients.Values)
        {
            client.Dispose();
        }

        _clients.Clear();
    }
}
