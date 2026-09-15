using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class MapRefreshTests {
		[Theory]
		[InlineData(40, 40, true)]
		[InlineData(80, 40, true)]
		[InlineData(1, 40, false)]
		[InlineData(39, 40, false)]
		[InlineData(41, 40, false)]
		[InlineData(0, 40, true)]
		public void ShouldRefresh_FiresOnlyOnIntervalTicks(int counter, int interval, bool expected) {
			Assert.Equal(expected, MapRefresh.ShouldRefresh(counter, interval));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void ShouldRefresh_NeverFires_WhenIntervalIsNotPositive(int interval) {
			Assert.False(MapRefresh.ShouldRefresh(40, interval));
		}
	}
}
