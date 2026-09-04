using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.ChainProviders.Transport;

namespace Cs2Hvh.ChainProviders.Evm;

public sealed class BscRpcClient : RpcClientBase
{
    public BscRpcClient(HttpTransportLayer transport) : base(transport)
    {
    }

    public override string NetworkId => "bsc-mainnet";

    public override Task<long> GetLatestBlockNumberAsync(CancellationToken cancellationToken) =>
        Task.FromResult(38_000_000L + DateTime.UtcNow.Second);

    public override Task<decimal> GetNativeBalanceAsync(string address, CancellationToken cancellationToken) =>
        Task.FromResult(Math.Abs(address.GetHashCode(StringComparison.Ordinal) % 800) / 10m);

    public override Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(
        string address,
        long fromBlock,
        long toBlock,
        CancellationToken cancellationToken) =>
        Task.FromResult(EthereumRpcClient.SimulateTransactionHistory(address, fromBlock, toBlock, "BNB"));
}
