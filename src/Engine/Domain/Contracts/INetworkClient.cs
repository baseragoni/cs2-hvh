using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Domain.Contracts;

public interface INetworkClient
{
    string NetworkId { get; }

    Task<long> GetLatestBlockNumberAsync(CancellationToken cancellationToken);

    Task<decimal> GetNativeBalanceAsync(string address, CancellationToken cancellationToken);

    Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(string address, long fromBlock, long toBlock, CancellationToken cancellationToken);

    Task<bool> IsEndpointHealthyAsync(CancellationToken cancellationToken);
}
