using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class RegionIndexTests {
		[Theory]
		[InlineData(0, 3, true)]
		[InlineData(2, 3, true)]
		[InlineData(3, 3, false)]
		[InlineData(-1, 3, false)]
		[InlineData(0, 0, false)]
		[InlineData(int.MinValue, 3, false)]
		public void IsValid_MatchesExpectedBounds(int region, int regionCount, bool expected) {
			Assert.Equal(expected, RegionIndex.IsValid(region, regionCount));
		}
	}
}
