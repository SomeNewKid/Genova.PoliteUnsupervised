// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised.Console;

/// <summary>
/// Console host that accepts user input, classifies each sentence using
/// <see cref="ToneClassifier"/>, and prints the result to the console.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Entry point for the console application. Continuously reads user input,
    /// classifies it using <see cref="ToneClassifier"/>, and prints the label,
    /// cluster, distance, threshold, and confidence. Use <c>:q</c>, <c>:quit</c>,
    /// or <c>exit</c> to terminate the application.
    /// </summary>
    /// <param name="args">Command-line arguments (unused).</param>
    private static void Main(string[] args)
    {
        System.Console.Title = "Polite vs. Rude — Runtime Console";
        System.Console.WriteLine("Polite/Rude classifier is ready.");
        System.Console.WriteLine("Type a sentence and press <Enter> to classify.");
        System.Console.WriteLine("Type :q or :quit to exit.\n");

        // Optional: plug in your profanity sanitizer so runtime preprocessing matches training.
        // ToneClassifier.Sanitizer = s => ProfanityHelper.Sanitize(s);

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Environment.Exit(0);
        };

        while (true)
        {
            System.Console.Write("> ");
            string? input = System.Console.ReadLine();
            if (input is null)
            {
                break;
            }

            string trimmed = input.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (trimmed.Equals(":q", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals(":quit", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {
                ClassificationResult result = ToneClassifier.Classify(trimmed);

                // Pretty-print with simple coloring
                (ConsoleColor fg, string labelText) tuple = result.label switch
                {
                    ToneLabel.Polite => (ConsoleColor.Green, "Polite"),
                    ToneLabel.Rude => (ConsoleColor.Red, "Rude"),
                    _ => (ConsoleColor.Yellow, "Neutral")
                };

                System.Console.ForegroundColor = tuple.fg;
                System.Console.WriteLine($"Result: {tuple.labelText}");
                System.Console.ResetColor();

                System.Console.WriteLine($"  Cluster:   {result.clusterId}");
                System.Console.WriteLine($"  Distance:  {result.distance:0.###}");
                System.Console.WriteLine($"  Threshold: {result.threshold:0.###}");
                System.Console.WriteLine($"  Confidence:{result.confidence:P0}");
                System.Console.WriteLine();
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Error: " + ex.Message);
                System.Console.ResetColor();
            }
        }
    }
}
