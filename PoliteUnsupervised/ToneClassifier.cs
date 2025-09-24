using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Genova.Common.Utilities;
using Microsoft.ML;

namespace Genova.PoliteUnsupervised;

public static class ToneClassifier
{
    // Adjust to your actual resource names/namespaces:
    private const string ModelResourceName = "polite-rude-k8.zip";
    private const string MapResourceName = "polite-rude-k8-cluster_map.json";

    private static readonly Assembly _assembly = typeof(ToneClassifier).Assembly;
    private static readonly string _embeddedResourceFolder = "Data/";

    private static readonly Lazy<MLContext> _ml = new(() => new MLContext());
    private static readonly Lazy<PredictionEngine<InputRow, KMeansOut>> _engine = new(LoadEngine);
    private static readonly Lazy<ClusterMap> _map = new(LoadMap);

    public static ClassificationResult Classify(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ClassificationResult(ToneLabel.Neutral, 0, 0, 0, 0);

        var text = ProfanityHelper.Sanitize(rawText);
        var pred = _engine.Value.Predict(new InputRow { Text = text });

        var cluster = pred.PredictedLabel; // 1-based
        var ownDist = pred.Distances[(int)cluster - 1];

        var map = _map.Value;
        var key = cluster.ToString();
        if (!map.clusters.TryGetValue(key, out var info))
        {
            // unknown cluster id → neutral
            return new ClassificationResult(ToneLabel.Neutral, cluster, ownDist, 0, 0);
        }

        var label = ClusterMapLoader.ToTone(info.label);
        var threshold = info.maxDistance;

        // Thresholded decision → Neutral if too far from its centroid
        if (threshold > 0 && ownDist > threshold)
            label = ToneLabel.Neutral;

        var conf = threshold > 0 ? Math.Clamp(1f - ownDist / threshold, 0f, 1f) : 0f;

        return new ClassificationResult(label, cluster, ownDist, threshold, conf);
    }

    private static PredictionEngine<InputRow, KMeansOut> LoadEngine()
    {
        string resourceName = _embeddedResourceFolder + ModelResourceName;
        if (!FileHelper.EmbeddedFileExists(_assembly, resourceName))
        {
            throw new InvalidOperationException($"Embedded model not found: {resourceName}");
        }

        using var stream = FileHelper.GetEmbeddedResourceStream(_assembly, resourceName)
            ?? throw new InvalidOperationException($"Embedded model not found: {ModelResourceName}");
        var ml = _ml.Value;

        // REGISTER the assembly that contains the CustomMapping factory
        ml.ComponentCatalog.RegisterAssembly(_assembly);

        var model = ml.Model.Load(stream, out _);
        return ml.Model.CreatePredictionEngine<InputRow, KMeansOut>(model);
    }

    private static ClusterMap LoadMap()
    {
        string resourceName = _embeddedResourceFolder + MapResourceName;
        if (!FileHelper.EmbeddedFileExists(_assembly, resourceName))
        {
            throw new InvalidOperationException($"Embedded model not found: {resourceName}");
        }

        using var stream = FileHelper.GetEmbeddedResourceStream(_assembly, resourceName)
            ?? throw new InvalidOperationException($"Embedded model not found: {ModelResourceName}");
        return ClusterMapLoader.Load(stream);
    }
}
