using System.Reflection;
using System.Text.Json;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace Genova.PoliteUnsupervised;

public sealed class InputRow
{
    [LoadColumn(0)]
    public string Text { get; set; } = "";
}
