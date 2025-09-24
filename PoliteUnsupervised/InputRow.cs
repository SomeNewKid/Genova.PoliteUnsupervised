// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Data;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Represents a single input row containing the raw text to be featurized and clustered.
/// </summary>
public sealed class InputRow
{
    /// <summary>
    /// Gets or sets the raw input text for this row.
    /// </summary>
    [LoadColumn(0)]
    public string Text { get; set; } = string.Empty;
}
