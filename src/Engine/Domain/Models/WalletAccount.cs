namespace Cs2Hvh.Engine.Domain.Models;

/// <summary>
/// Represents a derived account within a hierarchical deterministic wallet.
/// </summary>
public sealed class WalletAccount
{
    public required string AccountId { get; init; }

    public required string NetworkId { get; init; }

    public required string DerivationPath { get; init; }

    public required string PublicAddress { get; init; }

    public int AccountIndex { get; init; }

    public int AddressIndex { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? LastSyncedAt { get; set; }

    public AccountSyncStatus SyncStatus { get; set; } = AccountSyncStatus.Pending;

    public IReadOnlyList<AssetBalance> Balances { get; set; } = Array.Empty<AssetBalance>();
}

public enum AccountSyncStatus
{
    Pending = 0,
    Syncing = 1,
    Synced = 2,
    Error = 3,
    Stale = 4
}
