// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Data;

namespace Genova.PoliteUnsupervised;

public sealed class KMeansOut
{
    public uint PredictedLabel { get; set; }   // 1-based cluster id

    [ColumnName("Score")]
    public float[] Distances { get; set; } = Array.Empty<float>();
}
