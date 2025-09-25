// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Specifies the tone classification assigned to an input sentence.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Rusty Kane website.")]
public enum ToneLabel
{
    /// <summary>
    /// The sentence is considered polite after thresholding.
    /// </summary>
    Polite,

    /// <summary>
    /// The sentence is considered rude after thresholding.
    /// </summary>
    Rude,

    /// <summary>
    /// The sentence is not confidently polite or rude.
    /// </summary>
    Neutral,
}
