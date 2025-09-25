// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Data;

namespace Genova.PoliteUnsupervised.Training;

/// <summary>
/// Represents a transformed row containing the original text, the predicted cluster,
/// and the per-cluster distance vector produced by the K-Means model.
/// </summary>
internal sealed class ScoredRow
{
    /// <summary>
    /// Gets or sets the original input sentence text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 1-based identifier of the predicted cluster.
    /// </summary>
    public uint PredictedLabel { get; set; }

    /// <summary>
    /// Gets or sets the distances from the input vector to each cluster centroid.
    /// The array length equals the number of clusters (<c>k</c>).
    /// </summary>
    [ColumnName("Score")]
    public float[] Distances { get; set; } = Array.Empty<float>();
}
