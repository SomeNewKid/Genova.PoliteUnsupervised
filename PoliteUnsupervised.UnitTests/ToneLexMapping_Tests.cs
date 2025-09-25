// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised.UnitTests;

public class ToneLexMapping_Tests
{
    [Fact]
    public void ToneLexMapping_PolitePlease_SetsExpectedFlags()
    {
        var src = new InputRow { Text = "Please close the window." };
        var dst = new ToneOut();

        ToneLexMapping.Map(src, dst);

        Assert.Equal(1f, dst.HasPlease);
        Assert.Equal(1f, dst.StartsWithPolite);
        Assert.Equal(0f, dst.HasProfanity);
        Assert.Equal(0f, dst.HasExclaim);
        Assert.Equal(0f, dst.HasQuestion);
        Assert.Contains("please", dst.ToneText, StringComparison.Ordinal);
    }
}
