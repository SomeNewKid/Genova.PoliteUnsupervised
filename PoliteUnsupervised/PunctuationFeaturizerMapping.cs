// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides a custom mapping that derives punctuation-based features from an input sentence.
/// </summary>
public static class PunctuationFeaturizerMapping
{
    /// <summary>
    /// The contract name used to bind this custom mapping when saving and loading ML.NET models.
    /// </summary>
    public const string ContractName = "PunctuationFeaturizer";

    /// <summary>
    /// Populates <see cref="PunctuationFeatures"/> by analyzing punctuation patterns within the source text.
    /// </summary>
    /// <param name="src">The source row containing the input text.</param>
    /// <param name="dst">The destination structure to receive computed punctuation features.</param>
    public static void Map(InputRow src, PunctuationFeatures dst)
    {
        string s = (src.Text ?? string.Empty).Trim();
        int len = s.Length == 0 ? 1 : s.Length;

        int excl = 0;
        int ques = 0;
        foreach (char c in s)
        {
            if (c == '!')
            {
                excl++;
            }
            else if (c == '?')
            {
                ques++;
            }
        }

        dst.ExclCount = excl;
        dst.QuesCount = ques;
        dst.Interrobang = Regex.Matches(s, @"\?!|!\?").Count;
        dst.MultiExcl = Regex.IsMatch(s, @"!{2,}") ? 1 : 0;
        dst.MultiQ = Regex.IsMatch(s, @"\?{2,}") ? 1 : 0;
        dst.TerminalExcl = s.EndsWith("!") ? 1 : 0;
        dst.TerminalQues = s.EndsWith("?") ? 1 : 0;
        dst.ExclPerChar = (float)excl / len;
        dst.QuesPerChar = (float)ques / len;
    }
}
