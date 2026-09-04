namespace Cs2Hvh.Engine.Options;

public sealed class WalletOptions
{
    public const string SectionName = "Wallet";

    public string DefaultVaultDirectory { get; set; } = ".wallets";

    public int DefaultAccountScanDepth { get; set; } = 5;

    public int MaxConcurrentNetworkRequests { get; set; } = 4;

    public int RpcTimeoutSeconds { get; set; } = 30;

    public int KdfIterations { get; set; } = 100_000;

    public bool EnableTransactionIndexing { get; set; } = true;

    public bool PersistBalanceSnapshots { get; set; } = true;

    public IReadOnlyList<string> EnabledNetworks { get; set; } = new[]
    {
        "ethereum-mainnet",
        "bitcoin-mainnet",
        "polygon-mainnet"
    };
}
