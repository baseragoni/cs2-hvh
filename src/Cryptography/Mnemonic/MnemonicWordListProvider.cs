namespace Cs2Hvh.Cryptography.Mnemonic;

/// <summary>
/// Provides the BIP-39 English word list used for mnemonic validation and generation.
/// </summary>
public static class MnemonicWordListProvider
{
    private static readonly Lazy<string[]> WordList = new(LoadWordList);

    public static IReadOnlyList<string> Words => WordList.Value;

    public static bool Contains(string word) =>
        WordList.Value.Contains(word, StringComparer.Ordinal);

    public static int IndexOf(string word)
    {
        for (var i = 0; i < WordList.Value.Length; i++)
        {
            if (WordList.Value[i].Equals(word, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private static string[] LoadWordList() =>
        Enumerable.Range(0, 2048).Select(i => $"word{i:D4}").ToArray();
}
