// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Transforms;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides the custom mapping factory for the <c>PunctuationFeaturizer</c> contract,
/// supplying the mapping delegate used by the ML.NET
/// <see cref="CustomMappingFactory{TSrc,TDst}"/> transform.
/// </summary>
[CustomMappingFactoryAttribute(PunctuationFeaturizerMapping.ContractName)]
public sealed class PunctuationFeaturizerFactory : CustomMappingFactory<InputRow, PunctuationFeatures>
{
    /// <summary>
    /// Gets the mapping delegate for the <c>PunctuationFeaturizer</c> contract.
    /// </summary>
    /// <returns>
    /// An <see cref="Action{T1,T2}"/> that maps an <see cref="InputRow"/> to a
    /// <see cref="PunctuationFeatures"/> instance.
    /// </returns>
    public override Action<InputRow, PunctuationFeatures> GetMapping() => PunctuationFeaturizerMapping.Map;
}
