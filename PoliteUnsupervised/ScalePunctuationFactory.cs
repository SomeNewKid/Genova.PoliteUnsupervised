// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Transforms;

namespace Genova.PoliteUnsupervised;

/// <summary>
/// Provides the custom mapping factory for the <c>ScalePunctuation</c> contract,
/// supplying the mapping delegate used by the ML.NET
/// <see cref="CustomMappingFactory{TSrc,TDst}"/> transform.
/// </summary>
[CustomMappingFactoryAttribute(ScalePunctuationMapping.ContractName)]
internal sealed class ScalePunctuationFactory : CustomMappingFactory<PunctuationFeatures, PunctuationFeatures>
{
    /// <summary>
    /// Gets the mapping delegate for the <c>ScalePunctuation</c> contract.
    /// </summary>
    /// <returns>
    /// An <see cref="Action{T1,T2}"/> that maps a <see cref="PunctuationFeatures"/> source
    /// to a <see cref="PunctuationFeatures"/> destination.
    /// </returns>
    public override Action<PunctuationFeatures, PunctuationFeatures> GetMapping() => ScalePunctuationMapping.Map;
}
