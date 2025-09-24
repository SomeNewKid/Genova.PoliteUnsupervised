using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public static class PunctuationFeaturizerMapping
{
    public const string ContractName = "PunctuationFeaturizer";
    public static void Map(InputRow src, PunctFeatures dst)
    {
        var s = (src.Text ?? string.Empty).Trim();
        int len = s.Length == 0 ? 1 : s.Length;

        int excl = 0, ques = 0;
        foreach (char c in s) { if (c == '!') excl++; else if (c == '?') ques++; }

        dst.ExclCount = excl;
        dst.QuesCount = ques;
        dst.Interrobang = Regex.Matches(s, @"\?!|!\?").Count;
        dst.MultiExcl = Regex.IsMatch(s, @"!{2,}") ? 1 : 0;
        dst.MultiQ = Regex.IsMatch(s, @"\?{2,}") ? 1 : 0;
        dst.TerminalExcl = s.EndsWith("!") ? 1 : 0;
        dst.TerminalQues = s.EndsWith("?") ? 1 : 0;
        dst.ExclPerChar = (float)excl / len;
        dst.QuesPerChar = (float)ques / len;
    }
}
