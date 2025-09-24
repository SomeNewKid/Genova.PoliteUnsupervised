using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace Genova.PoliteUnsupervised.Training;

internal class Program
{
    private static string InputDir = "input";
    private static string OutputDir = "output";

    // --- Tone lexicons / helpers for the custom mapping ---
    private static readonly string[] PoliteStart = { "please", "could", "would", "might", "kindly" };
    private static readonly string[] PolitePhrases = { "would you mind", "i would appreciate", "i request", "i ask" };
    private static readonly string[] RudeInsults = { "idiot", "dolt", "clown", "fool" };
    private static readonly string[] Intensifiers = { "now", "already", "immediately", "right", "instantly", "at", "once" };

    private static readonly HashSet<string> ToneLex = new(StringComparer.OrdinalIgnoreCase)
    {
        "please","could","would","might","kindly",
        "don't","dont","stop","quit",
        "now","already","immediately","at","once",
        "idiot","dolt","clown","fool",
        "profanity" // normalized marker
        // Note: we no longer keep "!" or "?" here; punctuation is handled in PunctFeatures
    };

    // Dial punctuation influence: 0.0 = ignore; 0.05–0.20 = mild nudge
    private const float PUNCT_WEIGHT = 0.10f;

    private static int Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        InputDir = configuration["InputDirectory"] ?? "";
        OutputDir = configuration["OutputDirectory"] ?? "";

        var datasetPath = Path.Combine(InputDir, "polite-rude-dataset.txt");
        if (!File.Exists(datasetPath))
        {
            Console.Error.WriteLine($"Dataset not found: {datasetPath}");
            return 3;
        }

        int k = 8;

        Directory.CreateDirectory(OutputDir);

        TrainAndSave(datasetPath, k);
        return 0;
    }

    private static void TrainAndSave(string datasetPath, int k)
    {
        var ml = new MLContext(seed: 42);

        // Load: one sentence per line; ignore empty/whitespace lines.
        IEnumerable<InputRow> lines = File.ReadLines(datasetPath)
                        .Select(l => l?.Trim() ?? "")
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Select(l => new InputRow { Text = l });

        int lineCount = lines.Count();

        IDataView data = ml.Data.LoadFromEnumerable(lines);

        // -------------------------
        // Custom mapping: InputRow -> ToneOut (tone heuristics)
        // -------------------------
        Action<InputRow, ToneOut> map = (src, dst) =>
        {
            var s = src.Text ?? "";
            var lower = s.ToLowerInvariant();

            bool Has(string term) => lower.Contains(term, StringComparison.Ordinal);

            dst.HasPlease = Has("please") ? 1 : 0;
            dst.HasCould = lower.StartsWith("could ") || Has(" could ") ? 1 : 0;
            dst.HasWould = lower.StartsWith("would ") || Has(" would ") ? 1 : 0;
            dst.HasKindly = Has("kindly") ? 1 : 0;
            dst.HasReqPhrase = (Has("would you mind") || Has("i would appreciate") || Has("i request") || Has("i ask")) ? 1 : 0;

            dst.HasDontOrStop = (Has("don't") || Has("dont") || lower.StartsWith("stop ") || Has(" stop ") || lower.StartsWith("quit ") || Has(" quit ")) ? 1 : 0;
            dst.HasInsult = RudeInsults.Any(t => ContainsWholeWord(lower, t)) ? 1f : 0f;
            dst.HasIntensifier = Intensifiers.Any(t => ContainsWholeWord(lower, t)
                                                    || (t == "at" && lower.Contains(" at once ")))
                                                    ? 1f : 0f;
            dst.HasProfanity = Has("profanity") ? 1 : 0;

            // Keep binary flags for punctuation in tone dense features (low impact once normalized)
            dst.HasExclaim = lower.Contains('!') ? 1 : 0;
            dst.HasQuestion = lower.Contains('?') ? 1 : 0;

            var trimmed = lower.Trim();
            dst.StartsWithPolite = PoliteStart.Any(p => trimmed.StartsWith(p + " ")) ? 1 : 0;
            dst.EndsWithPlease = trimmed.EndsWith(" please") ? 1 : 0;

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            dst.TokenCount = tokens.Length;

            // Build ToneText from a small tone lexicon ONLY (no punctuation kept here)
            var kept = new List<string>(tokens.Length);
            foreach (var t in tokens)
            {
                var bare = t.Trim('\"', '\'', '“', '”', '‘', '’', '.', ',', ';', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}');
                if (ToneLex.Contains(bare))
                    kept.Add(bare);
            }
            dst.ToneText = string.Join(' ', kept);
        };
        // Tone mapping
        var toneMap = ml.Transforms.CustomMapping<InputRow, ToneOut>(
            Genova.PoliteUnsupervised.ToneLexMapping.Map,
            Genova.PoliteUnsupervised.ToneLexMapping.ContractName);

        // Punctuation features
        var punctMap = ml.Transforms.CustomMapping<InputRow, Genova.PoliteUnsupervised.PunctFeatures>(
            Genova.PoliteUnsupervised.PunctuationFeaturizerMapping.Map,
            Genova.PoliteUnsupervised.PunctuationFeaturizerMapping.ContractName);

        // Scale punctuation
        var scalePunct = ml.Transforms.CustomMapping<Genova.PoliteUnsupervised.PunctFeatures, Genova.PoliteUnsupervised.PunctFeatures>(
            Genova.PoliteUnsupervised.ScalePunctMapping.Map,
            Genova.PoliteUnsupervised.ScalePunctMapping.ContractName)
            .Append(ml.Transforms.Concatenate(
                "PunctVec",
                nameof(Genova.PoliteUnsupervised.PunctFeatures.ExclCount),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.QuesCount),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.Interrobang),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.MultiExcl),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.MultiQ),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.TerminalExcl),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.TerminalQues),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.ExclPerChar),
                nameof(Genova.PoliteUnsupervised.PunctFeatures.QuesPerChar)));

        // -------------------------
        // Tone-text featurization (small, focused vocab)
        // IMPORTANT: Drop punctuation and disable char n-grams to avoid punctuation leakage
        // -------------------------
        var toneTextOpts = new TextFeaturizingEstimator.Options
        {
            CaseMode = TextNormalizingEstimator.CaseMode.Lower,
            KeepDiacritics = true,
            KeepPunctuations = false, // <— key: no punctuation in vocab
            KeepNumbers = true,

            WordFeatureExtractor = new WordBagEstimator.Options
            {
                NgramLength = 2,
                UseAllLengths = true,
                Weighting = NgramExtractingEstimator.WeightingCriteria.TfIdf
            },
            CharFeatureExtractor = null // <— disable char n-grams
        };

        var toneTextFeats = ml.Transforms.Text.FeaturizeText(
            outputColumnName: "ToneTextFeats",
            options: toneTextOpts,
            nameof(ToneOut.ToneText));

        // -------------------------
        // Dense tone features (booleans + counts), normalized
        // -------------------------
        var toneDenseCols = new[]
        {
            nameof(ToneOut.HasPlease), nameof(ToneOut.HasCould), nameof(ToneOut.HasWould),
            nameof(ToneOut.HasKindly), nameof(ToneOut.HasReqPhrase),
            nameof(ToneOut.HasDontOrStop), nameof(ToneOut.HasInsult),
            nameof(ToneOut.HasIntensifier), nameof(ToneOut.HasProfanity),
            nameof(ToneOut.HasExclaim), nameof(ToneOut.HasQuestion),
            nameof(ToneOut.StartsWithPolite), nameof(ToneOut.EndsWithPlease),
            nameof(ToneOut.TokenCount)
        };

        var toneDense =
            ml.Transforms.Concatenate("ToneDense", toneDenseCols)
              .Append(ml.Transforms.NormalizeMinMax("ToneDense"));

        // -------------------------
        // Final Features = [ToneTextFeats ; ToneDense ; PunctVec]
        // -------------------------
        var features = ml.Transforms.Concatenate("Features", "ToneTextFeats", "ToneDense", "PunctVec");

        var kmeansOpts = new KMeansTrainer.Options
        {
            FeatureColumnName = "Features",
            NumberOfClusters = k,
            MaximumNumberOfIterations = 200,
            // InitializationAlgorithm = KMeansTrainer.InitializationAlgorithm.KMeansPlusPlus
        };

        var pipeline = toneMap
            .Append(punctMap)
            .Append(scalePunct)
            .Append(toneTextFeats)
            .Append(toneDense)
            .Append(features)
            .Append(ml.Clustering.Trainers.KMeans(kmeansOpts));

        Console.WriteLine("Fitting model...");
        var model = pipeline.Fit(data);

        // Transform and preview
        IDataView preview = model.Transform(data);
        var few = ml.Data.CreateEnumerable<KMeansOut>(preview, reuseRowObject: false)
                         .Take(5)
                         .ToList();

        foreach (var r in few)
        {
            var distances = string.Join(",", r.Distances.Select(d => d.ToString("0.###")));
            Console.WriteLine($"Cluster={r.PredictedLabel}  Distances=[{distances}]");
        }

        // Save model (schema is embedded by ML.NET).
        string modelPath = Path.Combine(OutputDir, $"polite-rude-k{k}.zip");
        ml.Model.Save(model, data.Schema, modelPath);
        Console.WriteLine($"Saved model: {modelPath}");

        // Also save a human-readable schema summary for reference.
        string schemaPath = Path.Combine(OutputDir, $"polite-rude-k{k}-schema.json");
        SaveSchemaSummary(data.Schema, schemaPath);
        Console.WriteLine($"Saved schema summary: {schemaPath}");

        // Inspect cluster distribution + WCSS and nearest examples per cluster
        var scored = ml.Data.CreateEnumerable<ScoredRow>(preview, reuseRowObject: false).ToList();

        var byClusterSummary = scored.GroupBy(r => r.PredictedLabel)
                              .OrderBy(g => g.Key)
                              .Select(g => (Cluster: g.Key, Count: g.Count(),
                                           Nearest: g.OrderBy(r => r.Distances[(int)g.Key - 1]).Take(3).ToList()));
        foreach (var g in byClusterSummary)
        {
            Console.WriteLine($"Cluster {g.Cluster}  Count={g.Count}");
            foreach (var r in g.Nearest)
                Console.WriteLine($"  d={r.Distances[(int)g.Cluster - 1]:0.###}  \"{r.Text}\"");
        }

        double wcss = scored.Sum(r => {
            var d = r.Distances[(int)r.PredictedLabel - 1];
            return d * d;
        });
        Console.WriteLine($"WCSS (k={k}): {wcss:0.###}");

        var byCluster = scored.GroupBy(r => r.PredictedLabel).OrderBy(g => g.Key);

        var mapJson = new
        {
            k,
            clusters = byCluster.ToDictionary(
            g => g.Key.ToString(),
            g => new {
                label = (g.Key is 1 or 4) ? "polite" : "rude", // heuristic; adjust after inspection
                maxDistance = Percentile(g.Select(r => r.Distances[(int)g.Key - 1]), 0.90f)
            })
        };

        File.WriteAllText(
          Path.Combine(OutputDir, $"polite-rude-k{k}-cluster_map.json"),
          JsonSerializer.Serialize(mapJson, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    private static void SaveSchemaSummary(DataViewSchema schema, string path)
    {
        var cols = schema.Select(col => new
        {
            col.Name,
            Type = col.Type.ToString(),
            IsVector = col.Type is VectorDataViewType
        });

        var json = JsonSerializer.Serialize(cols, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }

    private static double Percentile(IEnumerable<float> xs, float p)
    {
        var arr = xs.OrderBy(v => v).ToArray();
        if (arr.Length == 0) return 0;
        var idx = (int)Math.Floor((arr.Length - 1) * p);
        return arr[idx];
    }

    private static bool ContainsWholeWord(string src, string term)
    {
        // quick boundary check without regex
        return src.Equals(term, StringComparison.Ordinal)
            || src.StartsWith(term + " ", StringComparison.Ordinal)
            || src.EndsWith(" " + term, StringComparison.Ordinal)
            || src.Contains(" " + term + " ", StringComparison.Ordinal);
    }
}
