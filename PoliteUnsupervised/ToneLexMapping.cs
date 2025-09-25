// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides the tone-lexicon custom mapping used during training to project raw text
/// into tone-oriented features and a compact tone-token text column.
/// </summary>
internal static class ToneLexMapping
{
    /// <summary>
    /// The contract name used to bind this custom mapping when saving and loading ML.NET models.
    /// </summary>
    public const string ContractName = "ToneLex";

    private static readonly HashSet<string> _toneLex = new(StringComparer.OrdinalIgnoreCase)
    {
        "please",
        "could",
        "would",
        "might",
        "kindly",
        "don't",
        "dont",
        "stop",
        "quit",
        "now",
        "already",
        "immediately",
        "at",
        "once",
        "idiot",
        "dolt",
        "clown",
        "fool",
        "profanity",
        "!",
        "?",
    };

    /// <summary>
    /// Maps an <see cref="InputRow"/> into a <see cref="ToneOut"/> by computing boolean/numeric
    /// tone features (please/could/would, profanity marker, punctuation flags, etc.) and by
    /// emitting a tone-only text sequence for subsequent featurization.
    /// </summary>
    /// <param name="src">The source row containing the input text.</param>
    /// <param name="dst">The destination structure to receive computed tone features.</param>
    public static void Map(InputRow src, ToneOut dst)
    {
        string s = src.Text ?? string.Empty;
        string lower = s.ToLowerInvariant();

        bool Has(string term) => lower.Contains(term, StringComparison.Ordinal);

        dst.HasPlease = Has("please") ? 1f : 0f;
        dst.HasCould = (lower.StartsWith("could ", StringComparison.Ordinal) || Has(" could ")) ? 1f : 0f;
        dst.HasWould = (lower.StartsWith("would ", StringComparison.Ordinal) || Has(" would ")) ? 1f : 0f;
        dst.HasKindly = Has("kindly") ? 1f : 0f;
        dst.HasReqPhrase =
            (Has("would you mind") || Has("i would appreciate") || Has("i request") || Has("i ask")) ? 1f : 0f;

        dst.HasDontOrStop =
            (Has("don't") || Has("dont") ||
             lower.StartsWith("stop ", StringComparison.Ordinal) || Has(" stop ") ||
             lower.StartsWith("quit ", StringComparison.Ordinal) || Has(" quit "))
             ? 1f : 0f;

        dst.HasInsult =
            ContainsWholeWord(lower, "idiot") ||
            ContainsWholeWord(lower, "dolt") ||
            ContainsWholeWord(lower, "clown") ||
            ContainsWholeWord(lower, "fool")
            ? 1f : 0f;

        dst.HasIntensifier =
            ContainsWholeWord(lower, "now") ||
            ContainsWholeWord(lower, "already") ||
            ContainsWholeWord(lower, "immediately") ||
            ContainsWholeWord(lower, "right") ||
            ContainsWholeWord(lower, "instantly") ||
            lower.Contains(" at once ", StringComparison.Ordinal)
            ? 1f : 0f;

        dst.HasProfanity = Has("profanity") ? 1f : 0f;
        dst.HasExclaim = lower.Contains('!') ? 1f : 0f;
        dst.HasQuestion = lower.Contains('?') ? 1f : 0f;

        string trimmed = lower.Trim();
        dst.StartsWithPolite =
            (trimmed.StartsWith("please ", StringComparison.Ordinal) ||
             trimmed.StartsWith("could ", StringComparison.Ordinal) ||
             trimmed.StartsWith("would ", StringComparison.Ordinal) ||
             trimmed.StartsWith("might ", StringComparison.Ordinal) ||
             trimmed.StartsWith("kindly ", StringComparison.Ordinal))
            ? 1f : 0f;

        dst.EndsWithPlease = trimmed.EndsWith(" please", StringComparison.Ordinal) ? 1f : 0f;

        string[] tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        dst.TokenCount = tokens.Length;

        // Build ToneText (markers + !/? + PROFANITY)
        List<string> kept = new List<string>(tokens.Length);
        foreach (string t in tokens)
        {
            string bare =
                t.Trim('\"', '\'', '“', '”', '‘', '’', '.', ',', ';', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}');
            if (_toneLex.Contains(bare))
            {
                kept.Add(bare);
            }

            if (t.Contains('!'))
            {
                kept.Add("!");
            }

            if (t.Contains('?'))
            {
                kept.Add("?");
            }
        }

        dst.ToneText = string.Join(' ', kept);
    }

    private static bool ContainsWholeWord(string src, string term) =>
        src.Equals(term, StringComparison.Ordinal) ||
        src.StartsWith(term + " ", StringComparison.Ordinal) ||
        src.EndsWith(" " + term, StringComparison.Ordinal) ||
        src.Contains(" " + term + " ", StringComparison.Ordinal);
}
