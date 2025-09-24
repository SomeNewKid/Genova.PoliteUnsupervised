using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

internal static class ClusterMapLoader
{
    public static ClusterMap Load(Stream jsonStream)
    {
        var map = JsonSerializer.Deserialize<ClusterMap>(jsonStream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ClusterMap();
        return map;
    }

    public static ToneLabel ToTone(string s) => s?.ToLowerInvariant() switch
    {
        "polite" => ToneLabel.Polite,
        "rude" => ToneLabel.Rude,
        _ => ToneLabel.Neutral
    };
}
