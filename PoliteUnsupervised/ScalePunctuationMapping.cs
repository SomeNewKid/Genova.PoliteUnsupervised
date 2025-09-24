// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides a custom mapping that scales punctuation-derived features by a constant weight.
/// </summary>
public static class ScalePunctuationMapping
{
    /// <summary>
    /// The contract name used to bind this custom mapping when saving and loading ML.NET models.
    /// </summary>
    public const string ContractName = "ScalePunctuation";

    /// <summary>
    /// The scalar weight applied to each punctuation feature.
    /// </summary>
    public const float PuctuationWeight = 0.10f;

    /// <summary>
    /// Scales values from the source punctuation features by <see cref="PuctuationWeight"/> and writes
    /// the results into the destination features.
    /// </summary>
    /// <param name="src">The source punctuation features.</param>
    /// <param name="dst">The destination that receives the scaled punctuation features.</param>
    public static void Map(PunctuationFeatures src, PunctuationFeatures dst)
    {
        dst.ExclCount = src.ExclCount * PuctuationWeight;
        dst.QuesCount = src.QuesCount * PuctuationWeight;
        dst.Interrobang = src.Interrobang * PuctuationWeight;
        dst.MultiExcl = src.MultiExcl * PuctuationWeight;
        dst.MultiQ = src.MultiQ * PuctuationWeight;
        dst.TerminalExcl = src.TerminalExcl * PuctuationWeight;
        dst.TerminalQues = src.TerminalQues * PuctuationWeight;
        dst.ExclPerChar = src.ExclPerChar * PuctuationWeight;
        dst.QuesPerChar = src.QuesPerChar * PuctuationWeight;
    }
}
