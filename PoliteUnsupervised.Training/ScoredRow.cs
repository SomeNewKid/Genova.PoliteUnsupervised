using Microsoft.ML.Data;

namespace Genova.PoliteUnsupervised.Training;

public sealed class ScoredRow
{
    public string Text { get; set; } = "";
    public uint PredictedLabel { get; set; }
    [ColumnName("Score")]
    public float[] Distances { get; set; } = Array.Empty<float>();
}
