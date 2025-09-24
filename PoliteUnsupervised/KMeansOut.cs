// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Data;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Represents the prediction output of the K-Means clustering model,
/// including the assigned cluster identifier and the per-cluster distance vector.
/// </summary>
public sealed class KMeansOut
{
    /// <summary>
    /// Gets or sets the 1-based identifier of the nearest cluster predicted by the model.
    /// </summary>
    public uint PredictedLabel { get; set; }

    /// <summary>
    /// Gets or sets the distances from the input vector to each cluster centroid,
    /// in the same order the model was trained. The length of this array equals <c>k</c>.
    /// </summary>
    [ColumnName("Score")]
    public float[] Distances { get; set; } = [];
}
