using Cs2Hvh.Engine.Domain.Models;
using Cs2Hvh.Engine.Validation;

namespace Cs2Hvh.Persistence.Indexing;

public sealed class TransactionIndexBuilder
{
    private readonly TransactionValidator _validator = new();

    public IReadOnlyList<TransactionRecord> BuildIndex(
        IEnumerable<TransactionRecord> incoming,
        IEnumerable<TransactionRecord> existing)
    {
        var known = existing.Select(t => t.TransactionHash).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var validIncoming = _validator.FilterValid(incoming);

        return validIncoming
            .Where(t => !known.Contains(t.TransactionHash))
            .OrderByDescending(t => t.BlockNumber)
            .ThenByDescending(t => t.Timestamp)
            .ToList();
    }

    public Dictionary<string, int> CountByAsset(IEnumerable<TransactionRecord> records) =>
        records.GroupBy(r => r.AssetSymbol).ToDictionary(g => g.Key, g => g.Count());
}
