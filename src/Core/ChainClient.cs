using System.Security.Cryptography;
using System.Text;

namespace Cs2Hvh.Core;

/// <summary>
/// Simulated chain I/O — deterministic balances, no live RPC in the lab build.
/// </summary>
public sealed class ChainClient : IChainClient
{
    private readonly NetworkRegistry _registry;
    private readonly EndpointRotator _rotator = new();

    public ChainClient(NetworkRegistry registry) => _registry = registry;

    public Task<decimal> FetchBalanceAsync(string networkId, string address, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var network = _registry.Get(networkId);
        _ = _rotator.Next(network);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(networkId + "|" + address.ToLowerInvariant()));
        var units = (hash[0] * 256 + hash[1]) / 10000m;
        if (network.Decimals > 8)
            units /= 100m;
        return Task.FromResult(Math.Round(units, 8));
    }

    public Task<bool> PingAsync(string networkId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var network = _registry.Get(networkId);
        foreach (var _ in _rotator.RoundRobin(network, Math.Min(2, network.Endpoints.Count)))
        {
            // Simulated success.
        }
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<TransactionRecord>> FetchRecentAsync(
        string networkId,
        string address,
        int limit = 20,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _ = _registry.Get(networkId);
        limit = Math.Clamp(limit, 1, 100);
        var rows = new List<TransactionRecord>(limit);
        for (var i = 0; i < limit; i++)
        {
            var material = SHA256.HashData(Encoding.UTF8.GetBytes($"{networkId}:{address}:{i}"));
            rows.Add(new TransactionRecord
            {
                Hash = HexCodec.ToHex(material.AsSpan(0, 16)),
                NetworkId = networkId,
                From = address,
                To = HexCodec.ToHexPrefixed(material.AsSpan(12, 20)),
                Amount = (material[31] + 1) / 1000m,
                Fee = 0.0001m * (i + 1),
                Confirmations = 6 + i,
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15 * i)
            });
        }

        return Task.FromResult<IReadOnlyList<TransactionRecord>>(rows);
    }
}

public sealed class FeeEstimator : IFeeEstimator
{
    public FeeQuote Quote(string networkId, string policy)
    {
        var normalized = policy.ToLowerInvariant();
        var (fee, blocks) = normalized switch
        {
            "fast" or "priority" => (0.00042m, 1),
            "standard" or "normal" => (0.00021m, 3),
            _ => (0.00008m, 6)
        };

        if (networkId.Contains("ethereum", StringComparison.OrdinalIgnoreCase)
            || networkId.Contains("polygon", StringComparison.OrdinalIgnoreCase)
            || networkId.Contains("bsc", StringComparison.OrdinalIgnoreCase)
            || networkId.Contains("arbitrum", StringComparison.OrdinalIgnoreCase))
        {
            fee = normalized switch
            {
                "fast" or "priority" => 0.0025m,
                "standard" or "normal" => 0.0012m,
                _ => 0.0006m
            };
        }

        return new FeeQuote
        {
            NetworkId = networkId,
            Policy = normalized,
            SuggestedFee = fee,
            EstimatedBlocks = blocks
        };
    }
}
