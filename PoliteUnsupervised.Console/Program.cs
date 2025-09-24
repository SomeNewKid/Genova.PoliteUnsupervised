namespace Genova.PoliteUnsupervised.Console;

internal static class Program
{
    static void Main(string[] args)
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
            var input = System.Console.ReadLine();
            if (input is null) break;

            var trimmed = input.Trim();
            if (trimmed.Length == 0) continue;
            if (trimmed.Equals(":q", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals(":quit", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {
                var result = ToneClassifier.Classify(trimmed);

                // Pretty-print with simple coloring
                var (fg, labelText) = result.Label switch
                {
                    ToneLabel.Polite => (ConsoleColor.Green, "Polite"),
                    ToneLabel.Rude => (ConsoleColor.Red, "Rude"),
                    _ => (ConsoleColor.Yellow, "Neutral")
                };

                System.Console.ForegroundColor = fg;
                System.Console.WriteLine($"Result: {labelText}");
                System.Console.ResetColor();

                System.Console.WriteLine($"  Cluster:   {result.ClusterId}");
                System.Console.WriteLine($"  Distance:  {result.Distance:0.###}");
                System.Console.WriteLine($"  Threshold: {result.Threshold:0.###}");
                System.Console.WriteLine($"  Confidence:{result.Confidence:P0}");
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
