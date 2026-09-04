using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.ChainProviders.Transport;

namespace Cs2Hvh.ChainProviders.Evm;

public sealed class PolygonRpcClient : RpcClientBase
{
    public PolygonRpcClient(HttpTransportLayer transport) : base(transport)
    {
    }

    public override string NetworkId => "polygon-mainnet";

    public override Task<long> GetLatestBlockNumberAsync(CancellationToken cancellationToken) =>
        Task.FromResult(55_000_000L);

    public override Task<decimal> GetNativeBalanceAsync(string address, CancellationToken cancellationToken) =>
        Task.FromResult(Math.Abs(address.GetHashCode(StringComparison.Ordinal) % 2000) / 100m);

    public override Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(
        string address,
        long fromBlock,
        long toBlock,
        CancellationToken cancellationToken) =>
        Task.FromResult(EthereumRpcClient.SimulateTransactionHistory(address, fromBlock, toBlock, "MATIC"));
}
