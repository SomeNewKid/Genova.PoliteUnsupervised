// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Common.Attributes;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Represents the result of classifying a single input sentence using the trained unsupervised model.
/// </summary>
/// <param name="label">
/// The tone label inferred from the assigned cluster after thresholding. Typical values are
/// <see cref="ToneLabel.Polite"/>, <see cref="ToneLabel.Rude"/>, or <see cref="ToneLabel.Neutral"/>.
/// </param>
/// <param name="clusterId">
/// The 1-based identifier of the nearest cluster returned by the K-Means model.
/// </param>
/// <param name="distance">
/// The distance from the input vector to the centroid of the assigned cluster. Smaller values indicate
/// a closer fit to that cluster.
/// </param>
/// <param name="threshold">
/// The maximum accepted distance for the assigned cluster. If <paramref name="distance"/> exceeds this
/// value, the final label is set to <see cref="ToneLabel.Neutral"/>.
/// </param>
/// <param name="confidence">
/// A simple confidence score derived from the distance and threshold,
/// computed as <c>1 - clamp(Distance / Threshold, 0..1)</c>. If no threshold is available, this value is 0.
/// </param>
[CodeQuality(Public = true, Justification = "Intended for use by the Rusty Kane website.")]
public sealed record ClassificationResult(
    ToneLabel label,
    uint clusterId,
    float distance,
    float threshold,
    float confidence);
