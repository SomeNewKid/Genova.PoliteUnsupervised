// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Genova.PoliteUnsupervised.UnitTests;

public class ClusterMapLoader_Tests
{
    [Theory]
    [InlineData("polite", ToneLabel.Polite)]
    [InlineData("rude", ToneLabel.Rude)]
    [InlineData("neutral", ToneLabel.Neutral)]
    [InlineData("unknown", ToneLabel.Neutral)]
    [InlineData(null, ToneLabel.Neutral)]
    public void ToTone_MapsLabels_AsExpected(string input, ToneLabel expected)
    {
        ToneLabel actual = ClusterMapLoader.ToTone(input);
        Assert.Equal(expected, actual);
    }
}
