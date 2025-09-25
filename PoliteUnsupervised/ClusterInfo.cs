// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Describes a single cluster's post-hoc metadata used by the runtime classifier.
/// </summary>
internal sealed class ClusterInfo
{
    /// <summary>
    /// Gets or sets the human-interpretable label assigned to this cluster.
    /// Expected values are <c>"polite"</c>, <c>"rude"</c>, or <c>"neutral"</c>.
    /// </summary>
    public string Label { get; set; } = "neutral";

    /// <summary>
    /// Gets or sets the maximum accepted distance to the cluster centroid. Inputs assigned to this
    /// cluster with a distance greater than this threshold should be treated as neutral.
    /// </summary>
    public float MaxDistance { get; set; }
}
