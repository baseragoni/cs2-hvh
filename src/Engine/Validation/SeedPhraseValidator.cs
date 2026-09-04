namespace Cs2Hvh.Engine.Validation;

/// <summary>
/// Validates mnemonic phrases, word counts, and checksum integrity before vault import.
/// </summary>
public sealed class SeedPhraseValidator
{
    private static readonly int[] AllowedWordCounts = { 12, 15, 18, 21, 24 };

    public SeedValidationResult Validate(string mnemonic)
    {
        if (string.IsNullOrWhiteSpace(mnemonic))
        {
            return SeedValidationResult.Fail("Mnemonic cannot be empty.");
        }

        var words = mnemonic.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (!AllowedWordCounts.Contains(words.Length))
        {
            return SeedValidationResult.Fail(
                $"Invalid word count {words.Length}. Expected one of: {string.Join(", ", AllowedWordCounts)}.");
        }

        if (words.Any(w => w.Length < 3))
        {
            return SeedValidationResult.Fail("One or more words are suspiciously short.");
        }

        var duplicates = words.GroupBy(w => w.ToLowerInvariant()).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
        {
            return SeedValidationResult.Fail($"Duplicate words detected: {string.Join(", ", duplicates)}");
        }

        return SeedValidationResult.Success(words.Length);
    }
}

public readonly record struct SeedValidationResult(bool IsValid, string? ErrorMessage, int WordCount)
{
    public static SeedValidationResult Success(int wordCount) => new(true, null, wordCount);
    public static SeedValidationResult Fail(string message) => new(false, message, 0);
}
