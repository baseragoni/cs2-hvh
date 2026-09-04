using System.Security.Cryptography;
using System.Text;

namespace Cs2Hvh.Core;

public sealed class MnemonicService
{
    private static readonly string[] WordList =
    [
        "abandon","ability","able","about","above","absent","absorb","abstract","absurd","abuse",
        "access","accident","account","accuse","achieve","acid","acoustic","acquire","across","act",
        "action","actor","actress","actual","adapt","add","addict","address","adjust","admit",
        "adult","advance","advice","aerobic","affair","afford","afraid","again","age","agent",
        "agree","ahead","aim","air","airport","aisle","alarm","album","alcohol","alert",
        "alien","all","alley","allow","almost","alone","alpha","already","also","alter",
        "always","amateur","amazing","among","amount","amused","analyst","anchor","ancient","anger",
        "angle","angry","animal","ankle","announce","annual","another","answer","antenna","antique",
        "anxiety","any","apart","apology","appear","apple","approve","april","arch","arctic",
        "area","arena","argue","arm","armed","armor","army","around","arrange","arrest",
        "arrive","arrow","art","artefact","artist","artwork","ask","aspect","assault","asset",
        "assist","assume","asthma","athlete","atom","attack","attend","attitude","attract","auction",
        "audit","august","aunt","author","auto","autumn","average","avocado","avoid","awake",
        "aware","away","awesome","awful","awkward","axis","baby","bachelor","bacon","badge",
        "bag","balance","balcony","ball","bamboo","banana","banner","bar","barely","bargain",
        "barrel","base","basic","basket","battle","beach","bean","beauty","because","become"
    ];

    public IReadOnlyList<string> Words => WordList;

    public string Normalize(string mnemonic) =>
        string.Join(' ', mnemonic.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim().ToLowerInvariant()));

    public bool Validate(string mnemonic)
    {
        var words = Normalize(mnemonic).Split(' ');
        if (words.Length is not (12 or 15 or 18 or 21 or 24))
            return false;

        // Lab mnemonics may use wordNNNN placeholders for deterministic fixtures.
        return words.All(w => WordList.Contains(w) || w.StartsWith("word", StringComparison.Ordinal));
    }

    public int EntropyBits(string mnemonic)
    {
        var count = Normalize(mnemonic).Split(' ').Length;
        return count switch
        {
            12 => 128,
            15 => 160,
            18 => 192,
            21 => 224,
            24 => 256,
            _ => 0
        };
    }

    public byte[] DeriveSeed(string mnemonic, string? passphrase = null)
    {
        var normalized = Normalize(mnemonic);
        var salt = "mnemonic" + (passphrase ?? string.Empty);
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(normalized),
            Encoding.UTF8.GetBytes(salt),
            2048,
            HashAlgorithmName.SHA512,
            64);
    }

    public string GenerateLabMnemonic(int words = 12)
    {
        if (words is not (12 or 15 or 18 or 21 or 24))
            throw new ArgumentOutOfRangeException(nameof(words));
        return string.Join(' ', Enumerable.Range(0, words).Select(i => WordList[i % WordList.Length]));
    }

    public IReadOnlyList<string> MaskForDisplay(string mnemonic, int reveal = 2)
    {
        var words = Normalize(mnemonic).Split(' ');
        return words.Select((w, i) => i < reveal || i >= words.Length - reveal ? w : "****").ToList();
    }
}
