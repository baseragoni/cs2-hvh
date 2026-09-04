using Cs2Hvh.Engine.Domain.Contracts;

namespace Cs2Hvh.Engine.Orchestration;

/// <summary>
/// Probes registered RPC endpoints and records latency statistics.
/// </summary>
public sealed class NetworkHealthMonitor
{
    private readonly Dictionary<string, NetworkHealthSnapshot> _snapshots = new(StringComparer.OrdinalIgnoreCase);

    public async Task<NetworkHealthSnapshot> ProbeAsync(INetworkClient client, CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;
        var healthy = await client.IsEndpointHealthyAsync(cancellationToken).ConfigureAwait(false);
        var elapsed = DateTimeOffset.UtcNow - started;

        var snapshot = new NetworkHealthSnapshot
        {
            NetworkId = client.NetworkId,
            IsHealthy = healthy,
            LastProbeAt = DateTimeOffset.UtcNow,
            LastRoundTripMs = (int)elapsed.TotalMilliseconds
        };

        _snapshots[client.NetworkId] = snapshot;
        return snapshot;
    }

    public IReadOnlyList<NetworkHealthSnapshot> GetAllSnapshots() => _snapshots.Values.ToList();

    public NetworkHealthSnapshot? GetSnapshot(string networkId) =>
        _snapshots.TryGetValue(networkId, out var snapshot) ? snapshot : null;
}

public sealed class NetworkHealthSnapshot
{
    public required string NetworkId { get; init; }
    public bool IsHealthy { get; init; }
    public DateTimeOffset LastProbeAt { get; init; }
    public int LastRoundTripMs { get; init; }
}
