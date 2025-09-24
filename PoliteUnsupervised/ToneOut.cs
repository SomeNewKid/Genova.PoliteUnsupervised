// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Represents derived tone features for an input sentence used by the training pipeline,
/// together with a compact token string that contains only tone markers and punctuation.
/// </summary>
public sealed class ToneOut
{
    /// <summary>
    /// Gets or sets the tone-only token sequence (e.g., markers like <c>please</c>, <c>PROFANITY</c>, <c>!</c>, <c>?</c>)
    /// used by text featurization.
    /// </summary>
    public string ToneText { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the sentence contains “please”.</summary>
    public float HasPlease { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains or starts with “could”.</summary>
    public float HasCould { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains or starts with “would”.</summary>
    public float HasWould { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains “kindly”.</summary>
    public float HasKindly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sentence contains request phrases
    /// (e.g., “would you mind”, “I would appreciate”, “I request”, “I ask”).
    /// </summary>
    public float HasReqPhrase { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains “don't”, “dont”, “stop”, or “quit”.</summary>
    public float HasDontOrStop { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sentence contains an insult term
    /// (e.g., “idiot”, “dolt”, “clown”, “fool”).
    /// </summary>
    public float HasInsult { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sentence contains intensifiers
    /// (e.g., “now”, “already”, “immediately”, “right”, “instantly”, “at once”).
    /// </summary>
    public float HasIntensifier { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains the profanity placeholder “PROFANITY”.</summary>
    public float HasProfanity { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains an exclamation mark signal.</summary>
    public float HasExclaim { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence contains a question mark signal.</summary>
    public float HasQuestion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sentence starts with a polite marker
    /// (please/could/would/might/kindly).
    /// </summary>
    public float StartsWithPolite { get; set; }

    /// <summary>Gets or sets a value indicating whether the sentence ends with “please”.</summary>
    public float EndsWithPlease { get; set; }

    /// <summary>Gets or sets the number of whitespace-delimited tokens in the sentence.</summary>
    public float TokenCount { get; set; }
}
