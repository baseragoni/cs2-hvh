using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Analytics;

/// <summary>
/// Aggregates native and token balances from network clients into unified snapshots.
/// </summary>
public sealed class BalanceAggregator
{
    public async Task<IReadOnlyList<AssetBalance>> FetchBalancesAsync(
        INetworkClient client,
        NetworkDescriptor network,
        WalletAccount account,
        CancellationToken cancellationToken)
    {
        var nativeAmount = await client.GetNativeBalanceAsync(account.PublicAddress, cancellationToken)
            .ConfigureAwait(false);

        var nativeBalance = new AssetBalance
        {
            AssetSymbol = network.NativeAssetSymbol,
            ContractAddress = string.Empty,
            Amount = nativeAmount,
            Decimals = network.NativeAssetDecimals,
            QueriedAt = DateTimeOffset.UtcNow
        };

        return new[] { nativeBalance };
    }

    public decimal ComputePortfolioTotalUsd(IEnumerable<AssetBalance> balances) =>
        balances.Where(b => b.UsdValue.HasValue).Sum(b => b.UsdValue!.Value);
}
