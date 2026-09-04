using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Analytics;

/// <summary>
/// Builds human-readable portfolio summaries from cached account snapshots.
/// </summary>
public sealed class PortfolioReporter
{
    public PortfolioSummary BuildSummary(IEnumerable<WalletAccount> accounts)
    {
        var accountList = accounts.ToList();
        var balances = accountList.SelectMany(a => a.Balances).ToList();

        return new PortfolioSummary
        {
            TotalAccounts = accountList.Count,
            NetworksRepresented = accountList.Select(a => a.NetworkId).Distinct().Count(),
            AssetBreakdown = balances
                .GroupBy(b => b.AssetSymbol)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount)),
            LastActivity = accountList
                .Where(a => a.LastSyncedAt.HasValue)
                .Select(a => a.LastSyncedAt!.Value)
                .DefaultIfEmpty(DateTimeOffset.MinValue)
                .Max(),
            EstimatedUsdTotal = balances.Where(b => b.UsdValue.HasValue).Sum(b => b.UsdValue!.Value)
        };
    }

    public IReadOnlyList<string> FormatSummaryLines(PortfolioSummary summary)
    {
        var lines = new List<string>
        {
            $"Accounts tracked: {summary.TotalAccounts}",
            $"Networks: {summary.NetworksRepresented}",
            $"Last sync: {(summary.LastActivity == DateTimeOffset.MinValue ? "never" : summary.LastActivity.ToString("u"))}"
        };

        foreach (var (symbol, amount) in summary.AssetBreakdown.OrderByDescending(kvp => kvp.Value))
        {
            lines.Add($"  {symbol,-8} {amount,18:0.########}");
        }

        if (summary.EstimatedUsdTotal > 0m)
        {
            lines.Add($"Estimated USD: ${summary.EstimatedUsdTotal:N2}");
        }

        return lines;
    }
}

public sealed class PortfolioSummary
{
    public int TotalAccounts { get; init; }
    public int NetworksRepresented { get; init; }
    public Dictionary<string, decimal> AssetBreakdown { get; init; } = new();
    public DateTimeOffset LastActivity { get; init; }
    public decimal EstimatedUsdTotal { get; init; }
}
