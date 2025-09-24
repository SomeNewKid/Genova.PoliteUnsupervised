using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public static class ScalePunctMapping
{
    public const string ContractName = "ScalePunct";
    public const float PUNCT_WEIGHT = 0.10f;

    public static void Map(PunctFeatures src, PunctFeatures dst)
    {
        dst.ExclCount = src.ExclCount * PUNCT_WEIGHT;
        dst.QuesCount = src.QuesCount * PUNCT_WEIGHT;
        dst.Interrobang = src.Interrobang * PUNCT_WEIGHT;
        dst.MultiExcl = src.MultiExcl * PUNCT_WEIGHT;
        dst.MultiQ = src.MultiQ * PUNCT_WEIGHT;
        dst.TerminalExcl = src.TerminalExcl * PUNCT_WEIGHT;
        dst.TerminalQues = src.TerminalQues * PUNCT_WEIGHT;
        dst.ExclPerChar = src.ExclPerChar * PUNCT_WEIGHT;
        dst.QuesPerChar = src.QuesPerChar * PUNCT_WEIGHT;
    }
}
