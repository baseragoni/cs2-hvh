namespace Cs2Hvh.Engine.Domain.Models;

/// <summary>
/// Immutable record of a confirmed or pending blockchain transaction.
/// </summary>
public sealed class TransactionRecord
{
    public required string TransactionHash { get; init; }

    public required string NetworkId { get; init; }

    public required string FromAddress { get; init; }

    public required string ToAddress { get; init; }

    public required decimal Value { get; init; }

    public required string AssetSymbol { get; init; }

    public long BlockNumber { get; init; }

    public long Confirmations { get; init; }

    public TransactionDirection Direction { get; init; }

    public TransactionStatus Status { get; init; }

    public decimal? FeePaid { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public string? Memo { get; init; }
}

public enum TransactionDirection
{
    Incoming = 0,
    Outgoing = 1,
    Internal = 2
}

public enum TransactionStatus
{
    Pending = 0,
    Confirmed = 1,
    Failed = 2,
    Dropped = 3
}
