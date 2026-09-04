using Cs2Hvh.Engine.Domain.Extensions;
using Cs2Hvh.Engine.Domain.Models;

namespace Cs2Hvh.Engine.Validation;

public sealed class TransactionValidator
{
    public bool IsStructurallyValid(TransactionRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.TransactionHash))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(record.FromAddress) || string.IsNullOrWhiteSpace(record.ToAddress))
        {
            return false;
        }

        if (record.Value < 0m)
        {
            return false;
        }

        if (record.FromAddress.LooksLikeHexAddress() && !record.FromAddress.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public IReadOnlyList<TransactionRecord> FilterValid(IEnumerable<TransactionRecord> source) =>
        source.Where(IsStructurallyValid).ToList();
}
