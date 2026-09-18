using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class RoomFootprintTests {
		const float DiscoverResolution = 7f;

		[Theory]
		// a room smaller than one pixel still covers one, the way vanilla's inclusive loop does
		[InlineData(1, 1)]
		[InlineData(6, 1)]
		[InlineData(7, 2)]
		[InlineData(20, 3)]
		[InlineData(70, 11)]
		public void SpanCoversTheRoomInclusively(int sizeInTiles, int expected) {
			Assert.Equal(expected, RoomFootprint.PixelSpan(sizeInTiles, DiscoverResolution));
		}

		// SizeOfRoom returns (0, 0) for a room this map doesn't have, and there is nothing to fill
		[Theory]
		[InlineData(0)]
		[InlineData(-5)]
		public void NoSpanForARoomThatIsntThere(int sizeInTiles) {
			Assert.Equal(0, RoomFootprint.PixelSpan(sizeInTiles, DiscoverResolution));
		}

		[Fact]
		public void NoSpanWithoutAResolution() {
			Assert.Equal(0, RoomFootprint.PixelSpan(20, 0f));
		}
	}
}
