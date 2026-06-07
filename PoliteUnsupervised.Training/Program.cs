// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Text;

namespace Genova.PoliteUnsupervised.Training;

/// <summary>
/// Console entry point for training the unsupervised clustering model and emitting artifacts
/// (model ZIP, schema JSON, and cluster-map JSON) to the configured output directory.
/// </summary>
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conflicting naming styles")]
internal class Program
{
    private static string InputDir = "input";
    private static string OutputDir = "output";

    /// <summary>
    /// Application entry point. Loads configuration, locates the dataset, and invokes training.
    /// </summary>
    /// <param name="args">Command-line arguments (unused).</param>
    /// <returns>Zero on success; non-zero on failure.</returns>
    private static int Main(string[] args)
    {
        string solutionFolder = FindSolutionFolder();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        InputDir = ResolvePath(configuration["InputDirectory"] ?? string.Empty, solutionFolder);
        OutputDir = ResolvePath(configuration["OutputDirectory"] ?? string.Empty, solutionFolder);

        string datasetPath = Path.Combine(InputDir, "polite-rude-dataset.txt");
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

    private static string ResolvePath(string path, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.GetFullPath(Path.Combine(baseDirectory, path));
    }

    private static string FindSolutionFolder()
    {
        const string solutionFileName = "Genova.PoliteUnsupervised.sln";
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, solutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Solution folder containing '{solutionFileName}' could not be found.");
    }

    /// <summary>
    /// Trains the ML.NET pipeline on the specified dataset and writes model and diagnostics to disk.
    /// </summary>
    /// <param name="datasetPath">The full path to the training dataset (one sentence per line).</param>
    /// <param name="k">The number of clusters to learn.</param>
    private static void TrainAndSave(string datasetPath, int k)
    {
        MLContext ml = new MLContext(seed: 42);

        // Load: one sentence per line; ignore empty/whitespace lines.
        IEnumerable<InputRow> lines = File.ReadLines(datasetPath)
                        .Select(l => l?.Trim() ?? string.Empty)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Select(l => new InputRow { Text = l });

        int lineCount = lines.Count();

        IDataView data = ml.Data.LoadFromEnumerable(lines);

        // Tone mapping (contract implemented in shared library).
        IEstimator<ITransformer> toneMap = ml.Transforms.CustomMapping<InputRow, ToneOut>(
            ToneLexMapping.Map,
            ToneLexMapping.ContractName);

        // Punctuation features (contract implemented in shared library).
        IEstimator<ITransformer> punctMap = ml.Transforms.CustomMapping<InputRow, PunctuationFeatures>(
            PunctuationFeaturizerMapping.Map,
            PunctuationFeaturizerMapping.ContractName);

        // Scale punctuation (contract implemented in shared library) and vectorize.
        IEstimator<ITransformer> scalePunct = ml.Transforms.CustomMapping<PunctuationFeatures, PunctuationFeatures>(
                ScalePunctuationMapping.Map,
                ScalePunctuationMapping.ContractName)
            .Append(ml.Transforms.Concatenate(
                "PunctVec",
                nameof(PunctuationFeatures.ExclCount),
                nameof(PunctuationFeatures.QuesCount),
                nameof(PunctuationFeatures.Interrobang),
                nameof(PunctuationFeatures.MultiExcl),
                nameof(PunctuationFeatures.MultiQ),
                nameof(PunctuationFeatures.TerminalExcl),
                nameof(PunctuationFeatures.TerminalQues),
                nameof(PunctuationFeatures.ExclPerChar),
                nameof(PunctuationFeatures.QuesPerChar)));

        // Tone-text featurization (small, focused vocab)
        // IMPORTANT: Drop punctuation and disable char n-grams to avoid punctuation leakage
        TextFeaturizingEstimator.Options toneTextOpts = new TextFeaturizingEstimator.Options
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

        IEstimator<ITransformer> toneTextFeats = ml.Transforms.Text.FeaturizeText(
            outputColumnName: "ToneTextFeats",
            options: toneTextOpts,
            nameof(ToneOut.ToneText));

        // Dense tone features (booleans + counts), normalized
        string[] toneDenseCols = new[]
        {
            nameof(ToneOut.HasPlease), nameof(ToneOut.HasCould), nameof(ToneOut.HasWould),
            nameof(ToneOut.HasKindly), nameof(ToneOut.HasReqPhrase),
            nameof(ToneOut.HasDontOrStop), nameof(ToneOut.HasInsult),
            nameof(ToneOut.HasIntensifier), nameof(ToneOut.HasProfanity),
            nameof(ToneOut.HasExclaim), nameof(ToneOut.HasQuestion),
            nameof(ToneOut.StartsWithPolite), nameof(ToneOut.EndsWithPlease),
            nameof(ToneOut.TokenCount)
        };

        IEstimator<ITransformer> toneDense =
            ml.Transforms.Concatenate("ToneDense", toneDenseCols)
              .Append(ml.Transforms.NormalizeMinMax("ToneDense"));

        // Final Features = [ToneTextFeats ; ToneDense ; PunctVec]
        IEstimator<ITransformer> features =
            ml.Transforms.Concatenate("Features", "ToneTextFeats", "ToneDense", "PunctVec");

        KMeansTrainer.Options kmeansOpts = new ()
        {
            FeatureColumnName = "Features",
            NumberOfClusters = k,
            MaximumNumberOfIterations = 200
            // InitializationAlgorithm = KMeansTrainer.InitializationAlgorithm.KMeansPlusPlus
        };

        IEstimator<ITransformer> pipeline = toneMap
            .Append(punctMap)
            .Append(scalePunct)
            .Append(toneTextFeats)
            .Append(toneDense)
            .Append(features)
            .Append(ml.Clustering.Trainers.KMeans(kmeansOpts));

        Console.WriteLine("Fitting model...");
        ITransformer model = pipeline.Fit(data);

        // Transform and preview
        IDataView preview = model.Transform(data);
        List<KMeansOut> few = ml.Data.CreateEnumerable<KMeansOut>(preview, reuseRowObject: false)
                         .Take(5)
                         .ToList();

        foreach (KMeansOut r in few)
        {
            string distances = string.Join(",", r.Distances.Select(d => d.ToString("0.###")));
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
        List<ScoredRow> scored = ml.Data.CreateEnumerable<ScoredRow>(preview, reuseRowObject: false).ToList();

        IEnumerable<(uint Cluster, int Count, List<ScoredRow> Nearest)> byClusterSummary =
            scored.GroupBy(r => r.PredictedLabel)
                  .OrderBy(g => g.Key)
                  .Select(g => (Cluster: g.Key, Count: g.Count(),
                                Nearest: g.OrderBy(r => r.Distances[(int)g.Key - 1]).Take(3).ToList()));

        foreach ((uint Cluster, int Count, List<ScoredRow> Nearest) g in byClusterSummary)
        {
            Console.WriteLine($"Cluster {g.Cluster}  Count={g.Count}");
            foreach (ScoredRow r in g.Nearest)
            {
                Console.WriteLine($"  d={r.Distances[(int)g.Cluster - 1]:0.###}  \"{r.Text}\"");
            }
        }

        double wcss = scored.Sum(r =>
        {
            float d = r.Distances[(int)r.PredictedLabel - 1];
            return d * d;
        });
        Console.WriteLine($"WCSS (k={k}): {wcss:0.###}");

        IEnumerable<IGrouping<uint, ScoredRow>> byCluster = scored.GroupBy(r => r.PredictedLabel).OrderBy(g => g.Key);

        // Note: anonymous type → 'var' is required by the language
        var mapJson = new
        {
            k,
            clusters = byCluster.ToDictionary(
                g => g.Key.ToString(),
                g => new
                {
                    label = (g.Key is 1 or 4) ? "polite" : "rude", // heuristic; adjust after inspection
                    maxDistance = Percentile(g.Select(r => r.Distances[(int)g.Key - 1]), 0.90f)
                })
        };

        File.WriteAllText(
            Path.Combine(OutputDir, $"polite-rude-k{k}-cluster_map.json"),
            JsonSerializer.Serialize(mapJson, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    /// <summary>
    /// Writes a compact JSON summary of the data schema to the specified path.
    /// </summary>
    /// <param name="schema">The schema to summarize.</param>
    /// <param name="path">The output file path.</param>
    private static void SaveSchemaSummary(DataViewSchema schema, string path)
    {
        // Anonymous objects are used here; 'var' is required
        var cols = schema.Select(col => new
        {
            col.Name,
            Type = col.Type.ToString(),
            IsVector = col.Type is VectorDataViewType
        });

        string json = JsonSerializer.Serialize(cols, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Computes the p-th percentile of a sequence of single-precision values.
    /// </summary>
    /// <param name="xs">The sequence of values.</param>
    /// <param name="p">The percentile in the range [0,1].</param>
    /// <returns>The percentile value.</returns>
    private static double Percentile(IEnumerable<float> xs, float p)
    {
        float[] arr = xs.OrderBy(v => v).ToArray();
        if (arr.Length == 0)
        {
            return 0;
        }

        int idx = (int)Math.Floor((arr.Length - 1) * p);
        return arr[idx];
    }
}
