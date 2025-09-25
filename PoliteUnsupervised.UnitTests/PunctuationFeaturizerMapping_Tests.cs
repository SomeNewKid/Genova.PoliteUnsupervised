// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised.UnitTests;

public class PunctuationFeaturizerMapping_Tests
{
    [Fact]
    public void PunctuationFeaturizer_Counts_And_Flags_Work()
    {
        var src = new InputRow { Text = "Wait?! Really!!" };
        var dst = new PunctuationFeatures();

        PunctuationFeaturizerMapping.Map(src, dst);

        Assert.Equal(3f, dst.ExclCount);      // "?!", then "!!" → total 3
        Assert.Equal(1f, dst.QuesCount);      // one '?'
        Assert.Equal(1f, dst.Interrobang);    // "?!" once
        Assert.Equal(1f, dst.MultiExcl);      // "!!"
        Assert.Equal(0f, dst.MultiQ);         // no "??"
        Assert.Equal(1f, dst.TerminalExcl);   // ends with '!'
        Assert.Equal(0f, dst.TerminalQues);
        Assert.True(dst.ExclPerChar > 0f);
        Assert.True(dst.QuesPerChar > 0f);
    }
}
