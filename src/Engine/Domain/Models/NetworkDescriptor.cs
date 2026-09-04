namespace Cs2Hvh.Engine.Domain.Models;

/// <summary>
/// Describes a supported blockchain network and its RPC endpoint configuration.
/// </summary>
public sealed class NetworkDescriptor
{
    public required string NetworkId { get; init; }

    public required string DisplayName { get; init; }

    public required string ChainType { get; init; }

    public required int ChainId { get; init; }

    public required string NativeAssetSymbol { get; init; }

    public required int NativeAssetDecimals { get; init; }

    public required string DefaultDerivationPath { get; init; }

    public IReadOnlyList<string> RpcEndpoints { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ExplorerBaseUrls { get; init; } = Array.Empty<string>();

    public bool IsTestnet { get; init; }

    public TimeSpan RecommendedPollInterval { get; init; } = TimeSpan.FromSeconds(12);
}
