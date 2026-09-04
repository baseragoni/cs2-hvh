namespace Cs2Hvh.Core;

public sealed class NetworkRegistry
{
    private readonly Dictionary<string, NetworkDescriptor> _networks;

    public NetworkRegistry()
    {
        _networks = new Dictionary<string, NetworkDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
            ["bitcoin-mainnet"] = new()
            {
                Id = "bitcoin-mainnet", Kind = "UTXO", Symbol = "BTC", CoinType = 0, Decimals = 8,
                DerivationPath = "m/84'/0'/0'/0", SupportsReplaceByFee = true,
                Endpoints = ["https://blockstream.info/api", "https://mempool.space/api"]
            },
            ["bitcoin-testnet"] = new()
            {
                Id = "bitcoin-testnet", Kind = "UTXO", Symbol = "tBTC", CoinType = 1, Decimals = 8,
                DerivationPath = "m/84'/1'/0'/0", IsTestnet = true, SupportsReplaceByFee = true,
                Endpoints = ["https://blockstream.info/testnet/api"]
            },
            ["ethereum-mainnet"] = new()
            {
                Id = "ethereum-mainnet", Kind = "EVM", Symbol = "ETH", CoinType = 60, Decimals = 18,
                DerivationPath = "m/44'/60'/0'/0",
                Endpoints = ["https://rpc.ankr.com/eth", "https://eth.llamarpc.com"]
            },
            ["polygon-mainnet"] = new()
            {
                Id = "polygon-mainnet", Kind = "EVM", Symbol = "MATIC", CoinType = 60, Decimals = 18,
                DerivationPath = "m/44'/60'/0'/0",
                Endpoints = ["https://polygon-rpc.com", "https://rpc.ankr.com/polygon"]
            },
            ["bsc-mainnet"] = new()
            {
                Id = "bsc-mainnet", Kind = "EVM", Symbol = "BNB", CoinType = 60, Decimals = 18,
                DerivationPath = "m/44'/60'/0'/0",
                Endpoints = ["https://bsc-dataseed.binance.org"]
            },
            ["arbitrum-one"] = new()
            {
                Id = "arbitrum-one", Kind = "EVM", Symbol = "ETH", CoinType = 60, Decimals = 18,
                DerivationPath = "m/44'/60'/0'/0",
                Endpoints = ["https://arb1.arbitrum.io/rpc"]
            }
        };
    }

    public NetworkDescriptor Get(string id) =>
        _networks.TryGetValue(id, out var n) ? n : throw new KeyNotFoundException($"Unknown network '{id}'.");

    public bool Exists(string id) => _networks.ContainsKey(id);

    public IReadOnlyCollection<NetworkDescriptor> All() => _networks.Values;

    public IReadOnlyList<NetworkDescriptor> ByKind(string kind) =>
        _networks.Values.Where(n => n.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)).ToList();

    public void Register(NetworkDescriptor descriptor) => _networks[descriptor.Id] = descriptor;
}

public sealed class EndpointRotator
{
    private readonly Dictionary<string, int> _cursors = new(StringComparer.OrdinalIgnoreCase);

    public string Next(NetworkDescriptor network)
    {
        if (network.Endpoints.Count == 0)
            throw new InvalidOperationException($"Network '{network.Id}' has no endpoints.");

        _cursors.TryGetValue(network.Id, out var idx);
        var endpoint = network.Endpoints[idx % network.Endpoints.Count];
        _cursors[network.Id] = idx + 1;
        return endpoint;
    }

    public IEnumerable<string> RoundRobin(NetworkDescriptor network, int attempts)
    {
        for (var i = 0; i < Math.Max(1, attempts); i++)
            yield return Next(network);
    }
}
