using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public sealed record ClassificationResult(
    ToneLabel Label,
    uint ClusterId,
    float Distance,
    float Threshold,
    float Confidence // simple: 1 - clamp(dist/threshold, 0..1)
);
