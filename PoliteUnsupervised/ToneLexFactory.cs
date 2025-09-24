using Microsoft.ML.Transforms;

namespace Genova.PoliteUnsupervised;

[CustomMappingFactoryAttribute(ToneLexMapping.ContractName)]
public sealed class ToneLexFactory : CustomMappingFactory<InputRow, ToneOut>
{
    public override Action<InputRow, ToneOut> GetMapping() => ToneLexMapping.Map;
}

[CustomMappingFactoryAttribute(PunctuationFeaturizerMapping.ContractName)]
public sealed class PunctFeaturizerFactory : CustomMappingFactory<InputRow, PunctFeatures>
{
    public override Action<InputRow, PunctFeatures> GetMapping() => PunctuationFeaturizerMapping.Map;
}

[CustomMappingFactoryAttribute(ScalePunctMapping.ContractName)]
public sealed class ScalePunctFactory : CustomMappingFactory<PunctFeatures, PunctFeatures>
{
    public override Action<PunctFeatures, PunctFeatures> GetMapping() => ScalePunctMapping.Map;
}
