using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public sealed class ClusterInfo
{
    public string label { get; set; } = "neutral";        // "polite" | "rude" | "neutral"
    public float maxDistance { get; set; }                // acceptance threshold
}
