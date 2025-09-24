// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Represents derived punctuation features for an input sentence,
/// used as part of tone-oriented clustering.
/// </summary>
public class PunctuationFeatures
{
    /// <summary>
    /// Gets or sets the total count of exclamation marks (<c>'!'</c>) in the sentence.
    /// </summary>
    public float ExclCount { get; set; }

    /// <summary>
    /// Gets or sets the total count of question marks (<c>'?'</c>) in the sentence.
    /// </summary>
    public float QuesCount { get; set; }

    /// <summary>
    /// Gets or sets the count of interrobang patterns (e.g., <c>"?!"</c> or <c>"!?"</c>).
    /// </summary>
    public float Interrobang { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether any repeated exclamation sequence
    /// (e.g., <c>"!!"</c>) occurs in the sentence.
    /// </summary>
    public float MultiExcl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether any repeated question-mark sequence
    /// (e.g., <c>"??"</c>) occurs in the sentence.
    /// </summary>
    public float MultiQ { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sentence ends with an exclamation mark.
    /// </summary>
    public float TerminalExcl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sentence ends with a question mark.
    /// </summary>
    public float TerminalQues { get; set; }

    /// <summary>
    /// Gets or sets the ratio of exclamation marks to total characters in the sentence.
    /// </summary>
    public float ExclPerChar { get; set; }

    /// <summary>
    /// Gets or sets the ratio of question marks to total characters in the sentence.
    /// </summary>
    public float QuesPerChar { get; set; }
}
