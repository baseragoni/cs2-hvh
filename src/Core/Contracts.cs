using System.Text.Json;

namespace Cs2Hvh.Core;

public interface IVaultStore
{
    Task SaveAsync(WalletVault vault, CancellationToken ct = default);
    Task<WalletVault?> LoadAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<WalletVault>> ListAsync(CancellationToken ct = default);
}

public interface IChainClient
{
    Task<decimal> FetchBalanceAsync(string networkId, string address, CancellationToken ct = default);
    Task<bool> PingAsync(string networkId, CancellationToken ct = default);
    Task<IReadOnlyList<TransactionRecord>> FetchRecentAsync(string networkId, string address, int limit = 20, CancellationToken ct = default);
}

public interface IFeeEstimator
{
    FeeQuote Quote(string networkId, string policy);
}

public interface IAddressFactory
{
    string Derive(byte[] seed, NetworkDescriptor network, int accountIndex, int change = 0);
    bool LooksValid(string address, NetworkDescriptor network);
}

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
