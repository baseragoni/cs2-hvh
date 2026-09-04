using System.Text.Json;
using Cs2Hvh.Engine.Domain.Contracts;
using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.ChainProviders.Transport;

public abstract class RpcClientBase : INetworkClient
{
    protected HttpTransportLayer Transport { get; }

    protected RpcClientBase(HttpTransportLayer transport)
    {
        Transport = transport;
    }

    public abstract string NetworkId { get; }

    public abstract Task<long> GetLatestBlockNumberAsync(CancellationToken cancellationToken);

    public abstract Task<decimal> GetNativeBalanceAsync(string address, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyList<TransactionRecord>> GetTransactionsAsync(
        string address,
        long fromBlock,
        long toBlock,
        CancellationToken cancellationToken);

    public virtual async Task<bool> IsEndpointHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            _ = await GetLatestBlockNumberAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected async Task<T?> SendRequestAsync<T>(string method, object[] parameters, CancellationToken cancellationToken)
    {
        var request = new JsonRpcRequest { Method = method, Params = parameters };
        var payload = JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
        var responseJson = await Transport.PostJsonAsync(NetworkId, payload, cancellationToken).ConfigureAwait(false);
        var envelope = await Transport.DeserializeAsync<JsonRpcResponse<T>>(responseJson, cancellationToken)
            .ConfigureAwait(false);

        if (envelope?.Error is not null)
        {
            throw new InvalidOperationException($"RPC error {envelope.Error.Code}: {envelope.Error.Message}");
        }

        return envelope is null ? default : envelope.Result;
    }
}
