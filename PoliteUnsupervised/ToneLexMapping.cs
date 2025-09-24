using Microsoft.ML.Transforms;

namespace Genova.PoliteUnsupervised;

public static class ToneLexMapping
{
    public const string ContractName = "ToneLex";

    // This method contains your mapping logic (same as you had inline before)
    public static void Map(InputRow src, ToneOut dst)
    {
        var s = src.Text ?? "";
        var lower = s.ToLowerInvariant();

        bool Has(string term) => lower.Contains(term, StringComparison.Ordinal);

        dst.HasPlease = Has("please") ? 1f : 0f;
        dst.HasCould = (lower.StartsWith("could ") || Has(" could ")) ? 1f : 0f;
        dst.HasWould = (lower.StartsWith("would ") || Has(" would ")) ? 1f : 0f;
        dst.HasKindly = Has("kindly") ? 1f : 0f;
        dst.HasReqPhrase = (Has("would you mind") || Has("i would appreciate") || Has("i request") || Has("i ask")) ? 1f : 0f;

        dst.HasDontOrStop = (Has("don't") || Has("dont") || lower.StartsWith("stop ") || Has(" stop ")
                             || lower.StartsWith("quit ") || Has(" quit ")) ? 1f : 0f;

        dst.HasInsult = ContainsWholeWord(lower, "idiot") || ContainsWholeWord(lower, "dolt")
                           || ContainsWholeWord(lower, "clown") || ContainsWholeWord(lower, "fool") ? 1f : 0f;

        dst.HasIntensifier = ContainsWholeWord(lower, "now") || ContainsWholeWord(lower, "already")
                           || ContainsWholeWord(lower, "immediately") || ContainsWholeWord(lower, "right")
                           || ContainsWholeWord(lower, "instantly") || lower.Contains(" at once ") ? 1f : 0f;

        dst.HasProfanity = Has("profanity") ? 1f : 0f;
        dst.HasExclaim = lower.Contains('!') ? 1f : 0f;
        dst.HasQuestion = lower.Contains('?') ? 1f : 0f;

        var trimmed = lower.Trim();
        dst.StartsWithPolite = (trimmed.StartsWith("please ") || trimmed.StartsWith("could ")
                             || trimmed.StartsWith("would ") || trimmed.StartsWith("might ")
                             || trimmed.StartsWith("kindly ")) ? 1f : 0f;

        dst.EndsWithPlease = trimmed.EndsWith(" please") ? 1f : 0f;

        var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        dst.TokenCount = tokens.Length;

        // Build ToneText (markers + !/? + PROFANITY)
        var kept = new List<string>(tokens.Length);
        foreach (var t in tokens)
        {
            var bare = t.Trim('\"', '\'', '“', '”', '‘', '’', '.', ',', ';', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}');
            if (_toneLex.Contains(bare)) kept.Add(bare);
            if (t.Contains('!')) kept.Add("!");
            if (t.Contains('?')) kept.Add("?");
        }
        dst.ToneText = string.Join(' ', kept);
    }

    private static readonly HashSet<string> _toneLex = new(StringComparer.OrdinalIgnoreCase)
    {
        "please","could","would","might","kindly",
        "don't","dont","stop","quit",
        "now","already","immediately","at","once",
        "idiot","dolt","clown","fool",
        "profanity","!","?"
    };

    private static bool ContainsWholeWord(string src, string term) =>
        src.Equals(term, StringComparison.Ordinal) ||
        src.StartsWith(term + " ", StringComparison.Ordinal) ||
        src.EndsWith(" " + term, StringComparison.Ordinal) ||
        src.Contains(" " + term + " ", StringComparison.Ordinal);
}
