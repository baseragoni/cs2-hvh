namespace Cs2Hvh.Engine.Domain.Models;

/// <summary>
/// On-chain or token balance snapshot for a single asset symbol.
/// </summary>
public sealed class AssetBalance
{
    public required string AssetSymbol { get; init; }

    public required string ContractAddress { get; init; }

    public required decimal Amount { get; init; }

    public int Decimals { get; init; }

    public decimal? UsdValue { get; init; }

    public DateTimeOffset QueriedAt { get; init; }

    public string DisplayAmount =>
        Decimals <= 0
            ? Amount.ToString("0.########")
            : Amount.ToString($"0.{new string('#', Math.Min(Decimals, 8))}");
}
