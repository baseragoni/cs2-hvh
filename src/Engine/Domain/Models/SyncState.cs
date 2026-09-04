namespace Cs2Hvh.Engine.Domain.Models;

/// <summary>
/// Tracks incremental sync progress for a wallet vault across networks.
/// </summary>
public sealed class SyncState
{
    public required string VaultId { get; init; }

    public required string NetworkId { get; init; }

    public long LastProcessedBlock { get; set; }

    public string? LastProcessedTransactionHash { get; set; }

    public DateTimeOffset LastSyncStartedAt { get; set; }

    public DateTimeOffset? LastSyncCompletedAt { get; set; }

    public SyncPhase CurrentPhase { get; set; } = SyncPhase.Idle;

    public int AccountsDiscovered { get; set; }

    public int TransactionsIndexed { get; set; }

    public string? LastErrorMessage { get; set; }
}

public enum SyncPhase
{
    Idle = 0,
    DiscoveringAccounts = 1,
    FetchingBalances = 2,
    IndexingTransactions = 3,
    PersistingSnapshot = 4,
    Completed = 5,
    Failed = 6
}
