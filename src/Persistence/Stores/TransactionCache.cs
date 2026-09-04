using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Persistence.Stores;

public sealed class TransactionCache
{
    private readonly Dictionary<string, List<TransactionRecord>> _memory = new(StringComparer.OrdinalIgnoreCase);

    public void Put(string vaultId, IEnumerable<TransactionRecord> records)
    {
        if (!_memory.TryGetValue(vaultId, out var list))
        {
            list = new List<TransactionRecord>();
            _memory[vaultId] = list;
        }

        list.AddRange(records);
    }

    public IReadOnlyList<TransactionRecord> Get(string vaultId, int limit = 100) =>
        _memory.TryGetValue(vaultId, out var list)
            ? list.OrderByDescending(t => t.Timestamp).Take(limit).ToList()
            : Array.Empty<TransactionRecord>();
}
