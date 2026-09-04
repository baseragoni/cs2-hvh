using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Networks;

/// <summary>
/// Static registry of supported blockchain networks and derivation path templates.
/// </summary>
public sealed class NetworkRegistry
{
    private readonly Dictionary<string, NetworkDescriptor> _networks;

    public NetworkRegistry()
    {
        _networks = BuildDefaultNetworks().ToDictionary(n => n.NetworkId, StringComparer.OrdinalIgnoreCase);
    }

    public NetworkDescriptor GetNetwork(string networkId) =>
        _networks.TryGetValue(networkId, out var descriptor)
            ? descriptor
            : throw new KeyNotFoundException($"Unknown network id: {networkId}");

    public IReadOnlyList<NetworkDescriptor> GetAllNetworks() => _networks.Values.ToList();

    public bool IsSupported(string networkId) => _networks.ContainsKey(networkId);

    private static IEnumerable<NetworkDescriptor> BuildDefaultNetworks()
    {
        yield return new NetworkDescriptor
        {
            NetworkId = "ethereum-mainnet",
            DisplayName = "Ethereum Mainnet",
            ChainType = "evm",
            ChainId = 1,
            NativeAssetSymbol = "ETH",
            NativeAssetDecimals = 18,
            DefaultDerivationPath = "m/44'/60'/0'/0",
            RpcEndpoints = new[] { "eth-mainnet.rpc.vault-labs.local:8545", "eth-mainnet.rpc.vault-labs.local:8546" },
            ExplorerBaseUrls = new[] { "explorer.eth.vault-labs.local" }
        };

        yield return new NetworkDescriptor
        {
            NetworkId = "bitcoin-mainnet",
            DisplayName = "Bitcoin Mainnet",
            ChainType = "utxo",
            ChainId = 0,
            NativeAssetSymbol = "BTC",
            NativeAssetDecimals = 8,
            DefaultDerivationPath = "m/84'/0'/0'/0",
            RpcEndpoints = new[] { "btc-mainnet.rpc.vault-labs.local:8332" },
            ExplorerBaseUrls = new[] { "explorer.btc.vault-labs.local" }
        };

        yield return new NetworkDescriptor
        {
            NetworkId = "polygon-mainnet",
            DisplayName = "Polygon PoS",
            ChainType = "evm",
            ChainId = 137,
            NativeAssetSymbol = "MATIC",
            NativeAssetDecimals = 18,
            DefaultDerivationPath = "m/44'/60'/0'/0",
            RpcEndpoints = new[] { "polygon-mainnet.rpc.vault-labs.local:8545" },
            ExplorerBaseUrls = new[] { "explorer.polygon.vault-labs.local" }
        };

        yield return new NetworkDescriptor
        {
            NetworkId = "bsc-mainnet",
            DisplayName = "BNB Smart Chain",
            ChainType = "evm",
            ChainId = 56,
            NativeAssetSymbol = "BNB",
            NativeAssetDecimals = 18,
            DefaultDerivationPath = "m/44'/60'/0'/0",
            RpcEndpoints = new[] { "bsc-mainnet.rpc.vault-labs.local:8545" },
            ExplorerBaseUrls = new[] { "explorer.bsc.vault-labs.local" }
        };
    }
}
