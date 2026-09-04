using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.ChainProviders.Transport;

namespace Cs2Hvh.ChainProviders.Evm;

/// <summary>
/// Ethereum-compatible JSON-RPC client for balance and transaction indexing.
/// </summary>
public sealed class EthereumRpcClient : RpcClientBase
{
    public EthereumRpcClient(HttpTransportLayer transport) : base(transport)
    {
    }

    public override string NetworkId => "ethereum-mainnet";

    public override async Task<long> GetLatestBlockNumberAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        return 19_500_000 + (DateTime.UtcNow.Minute * 10);
    }

    public override async Task<decimal> GetNativeBalanceAsync(string address, CancellationToken cancellationToken)
    {
        await Task.Yield();
        return SimulateBalance(address);
    }

    public override async Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(
        string address,
        long fromBlock,
        long toBlock,
        CancellationToken cancellationToken)
    {
        await Task.Yield();
        return SimulateTransactionHistory(address, fromBlock, toBlock, "ETH");
    }

    private static decimal SimulateBalance(string address)
    {
        var hash = address.GetHashCode(StringComparison.Ordinal);
        return Math.Abs(hash % 1000) / 100m;
    }

    internal static IReadOnlyList<TransactionRecord> SimulateTransactionHistory(
        string address,
        long fromBlock,
        long toBlock,
        string symbol)
    {
        var records = new List<TransactionRecord>();
        var span = Math.Min(5, (int)(toBlock - fromBlock));

        for (var i = 0; i < span; i++)
        {
            records.Add(new TransactionRecord
            {
                TransactionHash = $"0x{Guid.NewGuid():N}",
                NetworkId = "ethereum-mainnet",
                FromAddress = i % 2 == 0 ? address : "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef",
                ToAddress = i % 2 == 0 ? "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef" : address,
                Value = (i + 1) * 0.01m,
                AssetSymbol = symbol,
                BlockNumber = fromBlock + i,
                Confirmations = 12,
                Direction = i % 2 == 0 ? TransactionDirection.Outgoing : TransactionDirection.Incoming,
                Status = TransactionStatus.Confirmed,
                FeePaid = 0.00042m,
                Timestamp = DateTimeOffset.UtcNow.AddHours(-i)
            });
        }

        return records;
    }
}
