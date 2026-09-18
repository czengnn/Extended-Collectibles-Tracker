using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class TilePositionTests {
		[Theory]
		[InlineData(4, 28, 22, true)]
		[InlineData(4, 0, 0, true)]
		// what a tracker's desiredSpawnLocation reads before the game has placed the item
		[InlineData(4, -1, 0, false)]
		[InlineData(4, 0, -1, false)]
		[InlineData(-1, 28, 22, false)]
		public void IsUsable_RejectsCoordinatesThatArentAPlace(int room, int x, int y, bool expected) {
			Assert.Equal(expected, TilePosition.IsUsable(room, x, y));
		}

		[Theory]
		[InlineData(0, 10f)]
		[InlineData(1, 30f)]
		[InlineData(28, 570f)]
		public void Centre_IsTheMiddleOfTheTile(int tile, float expected) {
			Assert.Equal(expected, TilePosition.Centre(tile));
		}
	}
}
