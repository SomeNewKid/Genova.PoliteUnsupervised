// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Represents the cluster-map metadata persisted alongside the trained model,
/// including the number of clusters and per-cluster information.
/// </summary>
internal sealed class ClusterMap
{
    /// <summary>
    /// Gets or sets the number of clusters (<c>k</c>) used during training.
    /// </summary>
    public int K { get; set; }

    /// <summary>
    /// Gets or sets the mapping of cluster identifiers to their corresponding metadata.
    /// The key is the 1-based cluster identifier serialized as a string.
    /// </summary>
    public Dictionary<string, ClusterInfo> Clusters { get; set; } = new();
}
