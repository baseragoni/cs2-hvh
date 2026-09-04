namespace Cs2Hvh.Engine.Networks;

/// <summary>
/// Resolves BIP-44 style derivation paths for account and change chains.
/// </summary>
public sealed class DerivationPathResolver
{
    public string BuildAccountPath(string basePath, int accountIndex, int addressIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        var normalized = basePath.TrimEnd('/');
        return $"{normalized}/{accountIndex}/{addressIndex}";
    }

    public (int Purpose, int CoinType, int Account, int Change, int Index) ParsePath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 5 || segments[0] != "m")
        {
            throw new FormatException($"Invalid derivation path: {path}");
        }

        static int ParseSegment(string segment) =>
            int.Parse(segment.TrimEnd('\'', 'H', 'h'));

        return (
            ParseSegment(segments[1]),
            ParseSegment(segments[2]),
            ParseSegment(segments[3]),
            ParseSegment(segments[4]),
            segments.Length > 5 ? ParseSegment(segments[5]) : 0
        );
    }
}
