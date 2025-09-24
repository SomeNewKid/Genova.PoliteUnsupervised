using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public sealed class ClusterMap
{
    public int k { get; set; }
    public Dictionary<string, ClusterInfo> clusters { get; set; } = new();
}
