namespace Cs2Hvh.Engine.Domain.Models;

/// <summary>
/// Root aggregate representing an encrypted HD wallet vault instance.
/// </summary>
public sealed class WalletVault
{
    public required string VaultId { get; init; }

    public required string Label { get; init; }

    public required byte[] EncryptedSeedBlob { get; init; }

    public required byte[] Salt { get; init; }

    public required byte[] Nonce { get; init; }

    public int KdfIterations { get; init; } = 100_000;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public IReadOnlyList<string> EnabledNetworkIds { get; set; } = Array.Empty<string>();

    public VaultMetadata Metadata { get; init; } = new();
}

public sealed class VaultMetadata
{
    public string? SourceApplication { get; init; }

    public string? ImportFormat { get; init; }

    public int WordCount { get; init; }

    public bool PassphraseProtected { get; init; }

    public string? Notes { get; init; }
}
