using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Analytics;

/// <summary>
/// Builds transaction history queries and filters for portfolio reporting.
/// </summary>
public sealed class TransactionPipeline
{
    public IReadOnlyList<TransactionRecord> FilterByDirection(
        IEnumerable<TransactionRecord> source,
        TransactionDirection direction) =>
        source.Where(t => t.Direction == direction).OrderByDescending(t => t.Timestamp).ToList();

    public IReadOnlyList<TransactionRecord> FilterByDateRange(
        IEnumerable<TransactionRecord> source,
        DateTimeOffset from,
        DateTimeOffset to) =>
        source.Where(t => t.Timestamp >= from && t.Timestamp <= to)
            .OrderByDescending(t => t.Timestamp)
            .ToList();

    public decimal SumIncomingValue(IEnumerable<TransactionRecord> source, string assetSymbol) =>
        source.Where(t => t.Direction == TransactionDirection.Incoming && t.AssetSymbol == assetSymbol)
            .Sum(t => t.Value);

    public decimal SumOutgoingValue(IEnumerable<TransactionRecord> source, string assetSymbol) =>
        source.Where(t => t.Direction == TransactionDirection.Outgoing && t.AssetSymbol == assetSymbol)
            .Sum(t => t.Value);
}
