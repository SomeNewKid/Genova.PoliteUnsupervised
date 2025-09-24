// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Text.Json;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides utilities to load a <see cref="ClusterMap"/> from JSON and
/// convert serialized labels to <see cref="ToneLabel"/> values.
/// </summary>
internal static class ClusterMapLoader
{
    private static JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Loads and deserializes a <see cref="ClusterMap"/> from the specified JSON stream.
    /// </summary>
    /// <param name="jsonStream">The input stream containing the cluster-map JSON.</param>
    /// <returns>
    /// The deserialized <see cref="ClusterMap"/> instance. If deserialization yields
    /// <see langword="null"/>, a new <see cref="ClusterMap"/> is returned.
    /// </returns>
    public static ClusterMap Load(Stream jsonStream)
    {
        return JsonSerializer.Deserialize<ClusterMap>(jsonStream, _jsonSerializerOptions) ?? new ClusterMap();
    }

    /// <summary>
    /// Converts a string label to a <see cref="ToneLabel"/> value.
    /// </summary>
    /// <param name="s">The string label to convert (e.g., <c>"polite"</c>, <c>"rude"</c>).</param>
    /// <returns>
    /// The corresponding <see cref="ToneLabel"/>. Any unrecognized value maps to
    /// <see cref="ToneLabel.Neutral"/>.
    /// </returns>
    public static ToneLabel ToTone(string s) => s?.ToLowerInvariant() switch
    {
        "polite" => ToneLabel.Polite,
        "rude" => ToneLabel.Rude,
        _ => ToneLabel.Neutral
    };
}
