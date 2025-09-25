// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Genova.Common.Attributes;
using Genova.Common.Utilities;
using Microsoft.ML;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides classification of user input into <see cref="ToneLabel"/> values
/// (Polite, Rude, or Neutral) using a pre-trained unsupervised ML.NET model
/// embedded as a resource in this assembly.
/// </summary>
[CodeQuality(Public = true, Justification = "Intended for use by the Rusty Kane website.")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conflicting naming rules.")]
public static class ToneClassifier
{
    // Adjust to your actual resource names/namespaces:
    private const string ModelResourceName = "polite-rude-k8.zip";
    private const string MapResourceName = "polite-rude-k8-cluster_map.json";

    private static readonly Assembly _assembly = typeof(ToneClassifier).Assembly;
    private static readonly string _embeddedResourceFolder = "Data/";

    private static readonly Lazy<MLContext> _ml = new Lazy<MLContext>(() => new MLContext());
    private static readonly Lazy<PredictionEngine<InputRow, KMeansOut>> _engine = new (LoadEngine);
    private static readonly Lazy<ClusterMap> _map = new (LoadMap);

    /// <summary>
    /// Classifies the specified raw input text as polite, rude, or neutral using the
    /// embedded model and cluster map.
    /// </summary>
    /// <param name="rawText">The user-provided input sentence to classify.</param>
    /// <returns>
    /// A <see cref="ClassificationResult"/> containing the final label, the assigned
    /// cluster identifier, the distance to that cluster, the cluster threshold, and a
    /// simple confidence score.
    /// </returns>
    public static ClassificationResult Classify(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return new ClassificationResult(ToneLabel.Neutral, 0U, 0F, 0F, 0F);
        }

        string text = ProfanityHelper.Sanitize(rawText);
        PredictionEngine<InputRow, KMeansOut> engine = _engine.Value;
        KMeansOut pred = engine.Predict(new InputRow { Text = text });

        uint cluster = pred.PredictedLabel; // 1-based
        float ownDist = pred.Distances[(int)cluster - 1];

        ClusterMap map = _map.Value;
        string key = cluster.ToString();
        if (!map.Clusters.TryGetValue(key, out ClusterInfo? info))
        {
            // Unknown cluster id → neutral
            return new ClassificationResult(ToneLabel.Neutral, cluster, ownDist, 0F, 0F);
        }

        ToneLabel label = ClusterMapLoader.ToTone(info.Label);
        float threshold = info.MaxDistance;

        // Thresholded decision → Neutral if too far from its centroid.
        if (threshold > 0F && ownDist > threshold)
        {
            label = ToneLabel.Neutral;
        }

        float conf = threshold > 0F ? Math.Clamp(1F - (ownDist / threshold), 0F, 1F) : 0F;

        return new ClassificationResult(label, cluster, ownDist, threshold, conf);
    }

    private static PredictionEngine<InputRow, KMeansOut> LoadEngine()
    {
        string resourceName = _embeddedResourceFolder + ModelResourceName;
        if (!FileHelper.EmbeddedFileExists(_assembly, resourceName))
        {
            throw new InvalidOperationException($"Embedded model not found: {resourceName}");
        }

        using Stream stream = FileHelper.GetEmbeddedResourceStream(_assembly, resourceName)
            ?? throw new InvalidOperationException($"Embedded model not found: {ModelResourceName}");

        MLContext ml = _ml.Value;

        // REGISTER the assembly that contains the CustomMapping factory.
        ml.ComponentCatalog.RegisterAssembly(_assembly);

        ITransformer model = ml.Model.Load(stream, out DataViewSchema _);
        PredictionEngine<InputRow, KMeansOut> engine = ml.Model.CreatePredictionEngine<InputRow, KMeansOut>(model);
        return engine;
    }

    private static ClusterMap LoadMap()
    {
        string resourceName = _embeddedResourceFolder + MapResourceName;
        if (!FileHelper.EmbeddedFileExists(_assembly, resourceName))
        {
            throw new InvalidOperationException($"Embedded model not found: {resourceName}");
        }

        using Stream stream = FileHelper.GetEmbeddedResourceStream(_assembly, resourceName)
            ?? throw new InvalidOperationException($"Embedded model not found: {ModelResourceName}");

        ClusterMap map = ClusterMapLoader.Load(stream);
        return map;
    }
}
