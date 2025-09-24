using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public class PunctFeatures
{
    public float ExclCount { get; set; }           // total '!'
    public float QuesCount { get; set; }           // total '?'
    public float Interrobang { get; set; }         // '?!' or '!?'
    public float MultiExcl { get; set; }           // any "!!"
    public float MultiQ { get; set; }              // any "??"
    public float TerminalExcl { get; set; }        // ends with '!'
    public float TerminalQues { get; set; }        // ends with '?'
    public float ExclPerChar { get; set; }         // excl / len
    public float QuesPerChar { get; set; }         // ques / len
}
