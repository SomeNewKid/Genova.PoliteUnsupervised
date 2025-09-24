using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genova.PoliteUnsupervised;

public sealed class ToneOut
{
    public string ToneText { get; set; } = ""; // filtered tokens for FeaturizeText
    public float HasPlease { get; set; }
    public float HasCould { get; set; }
    public float HasWould { get; set; }
    public float HasKindly { get; set; }
    public float HasReqPhrase { get; set; }     // "i would appreciate" / "i request"
    public float HasDontOrStop { get; set; }    // don't/dont/stop/quit
    public float HasInsult { get; set; }        // idiot/dolt/clown
    public float HasIntensifier { get; set; }   // now/already/immediately/"at once"
    public float HasProfanity { get; set; }     // "PROFANITY"
    public float HasExclaim { get; set; }       // '!' or '?!'
    public float HasQuestion { get; set; }      // '?'
    public float StartsWithPolite { get; set; } // starts with please/could/would/might
    public float EndsWithPlease { get; set; }
    public float TokenCount { get; set; }
}

