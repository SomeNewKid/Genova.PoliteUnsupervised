// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised.UnitTests;

public class ScalePunctuationMapping_Tests
{
    [Fact]
    public void ScalePunctuation_scales_all_fields_by_constant()
    {
        var src = new PunctuationFeatures
        {
            ExclCount = 2,
            QuesCount = 1,
            Interrobang = 1,
            MultiExcl = 1,
            MultiQ = 0,
            TerminalExcl = 1,
            TerminalQues = 0,
            ExclPerChar = 0.05f,
            QuesPerChar = 0.02f
        };
        var dst = new PunctuationFeatures();

        // Use the same logic your mapping uses
        ScalePunctuationMapping.Map(src, dst);

        const float w = ScalePunctuationMapping.PuctuationWeight;
        Assert.Equal(src.ExclCount * w, dst.ExclCount);
        Assert.Equal(src.QuesCount * w, dst.QuesCount);
        Assert.Equal(src.Interrobang * w, dst.Interrobang);
        Assert.Equal(src.MultiExcl * w, dst.MultiExcl);
        Assert.Equal(src.MultiQ * w, dst.MultiQ);
        Assert.Equal(src.TerminalExcl * w, dst.TerminalExcl);
        Assert.Equal(src.TerminalQues * w, dst.TerminalQues);
        Assert.Equal(src.ExclPerChar * w, dst.ExclPerChar);
        Assert.Equal(src.QuesPerChar * w, dst.QuesPerChar);
    }
}
