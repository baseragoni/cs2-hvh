namespace Cs2Hvh.Core;

public sealed class WalletOptions
{
    public string DefaultVaultDirectory { get; set; } = ".wallets";
    public int GapLimit { get; set; } = 20;
    public int MaxAccountsPerNetwork { get; set; } = 5;
    public int SyncConcurrency { get; set; } = 4;
    public decimal DustThreshold { get; set; } = 0.00001m;
    public string PreferredFeePolicy { get; set; } = "economy";
    public string[] EnabledNetworks { get; set; } = ["bitcoin-mainnet", "ethereum-mainnet"];
    public Dictionary<string, string> EndpointOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class NetworkDescriptor
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public required string Symbol { get; init; }
    public int CoinType { get; init; }
    public int Decimals { get; init; } = 8;
    public required string DerivationPath { get; init; }
    public IReadOnlyList<string> Endpoints { get; init; } = [];
    public bool SupportsReplaceByFee { get; init; }
    public bool IsTestnet { get; init; }
}

public sealed class WalletVault
{
    public required string Id { get; init; }
    public required string Label { get; set; }
    public required string EncryptedSeed { get; set; }
    public required string Salt { get; set; }
    public string? Fingerprint { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<string> Networks { get; init; } = [];
    public List<WalletAccount> Accounts { get; init; } = [];
    public Dictionary<string, string> Metadata { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class WalletAccount
{
    public required string NetworkId { get; init; }
    public required string Address { get; init; }
    public int Index { get; init; }
    public int Change { get; init; }
    public string DerivationPath { get; set; } = "";
    public decimal Balance { get; set; }
    public decimal Pending { get; set; }
    public string Symbol { get; set; } = "";
    public DateTimeOffset? LastSyncedAt { get; set; }
    public string? Label { get; set; }
}

public sealed class TransactionRecord
{
    public required string Hash { get; init; }
    public required string NetworkId { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public decimal Amount { get; init; }
    public decimal Fee { get; init; }
    public int Confirmations { get; init; }
    public string Status { get; init; } = "confirmed";
    public DateTimeOffset Timestamp { get; init; }
    public string? Memo { get; init; }
}

public sealed class PortfolioSummary
{
    public int VaultCount { get; init; }
    public int AccountCount { get; init; }
    public decimal TotalUnits { get; init; }
    public decimal TotalPending { get; init; }
    public IReadOnlyList<string> Networks { get; init; } = [];
    public IReadOnlyDictionary<string, decimal> BySymbol { get; init; } = new Dictionary<string, decimal>();
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class FeeQuote
{
    public required string NetworkId { get; init; }
    public required string Policy { get; init; }
    public decimal SuggestedFee { get; init; }
    public int EstimatedBlocks { get; init; }
    public DateTimeOffset QuotedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class SyncReport
{
    public required string VaultId { get; init; }
    public int AccountsTouched { get; init; }
    public int EndpointsTried { get; init; }
    public decimal TotalBalance { get; init; }
    public TimeSpan Duration { get; init; }
    public List<string> Warnings { get; init; } = [];
}

public sealed class UnsignedTransaction
{
    public required string NetworkId { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public decimal Amount { get; init; }
    public decimal Fee { get; init; }
    public string PayloadHex { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
