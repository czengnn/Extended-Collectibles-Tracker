using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class LateMarkersTests {
		[Fact]
		public void NothingToDrawWhenTheListHasnotGrown() {
			Assert.Equal(0, LateMarkers.UndrawnCount(entryCount: 12, markersDrawn: 12));
			Assert.Equal(12, LateMarkers.FirstUndrawnIndex(entryCount: 12, markersDrawn: 12));
		}

		[Fact]
		public void OnlyTheEntriesAddedSinceLastTime() {
			Assert.Equal(3, LateMarkers.UndrawnCount(entryCount: 15, markersDrawn: 12));
			Assert.Equal(12, LateMarkers.FirstUndrawnIndex(entryCount: 15, markersDrawn: 12));
		}

		// Drawing an entry twice is how one pearl ended up with a pile of markers in 1.0.8, and
		// the pile is what froze the map - the game walks every marker for each pixel it reveals.
		// So: the map is built, refreshes find nothing new, then a swallowed pearl adds an entry.
		[Fact]
		public void ADrawnEntryIsNeverDrawnAgain() {
			int entries = 4;
			int drawn = 0;

			Assert.Equal(4, LateMarkers.UndrawnCount(entries, drawn));
			drawn = entries;

			// a few refresh ticks with nothing new
			Assert.Equal(0, LateMarkers.UndrawnCount(entries, drawn));
			Assert.Equal(0, LateMarkers.UndrawnCount(entries, drawn));

			// the pearl in the slugcat's stomach turns out to be one this map had no entry for
			entries++;
			Assert.Equal(1, LateMarkers.UndrawnCount(entries, drawn));
			Assert.Equal(4, LateMarkers.FirstUndrawnIndex(entries, drawn));
			drawn = entries;

			Assert.Equal(0, LateMarkers.UndrawnCount(entries, drawn));
		}

		// a shorter list than we have drawn should ask for nothing rather than a negative range
		[Theory]
		[InlineData(4, 9)]
		[InlineData(0, 3)]
		public void AShrunkListAsksForNothing(int entryCount, int markersDrawn) {
			Assert.Equal(0, LateMarkers.UndrawnCount(entryCount, markersDrawn));
			Assert.Equal(entryCount, LateMarkers.FirstUndrawnIndex(entryCount, markersDrawn));
		}

		[Fact]
		public void ANegativeCountStartsFromTheBeginning() {
			Assert.Equal(0, LateMarkers.FirstUndrawnIndex(entryCount: 5, markersDrawn: -1));
		}

		// queueing this one would leave it invisible for good: its pixel is already revealed, and
		// a revealed pixel is never revealed again to trigger the fade
		[Fact]
		public void AMarkerOverRevealedGroundFadesInStraightAway() {
			Assert.Equal(LateMarkerReveal.FadeInNow, LateMarkers.HowToShow(spotAlreadyRevealed: true));
		}

		// and fading this one in would hand over a location the map hasn't earned yet
		[Fact]
		public void AMarkerOverHiddenGroundWaitsForTheReveal() {
			Assert.Equal(LateMarkerReveal.QueueForReveal, LateMarkers.HowToShow(spotAlreadyRevealed: false));
		}
	}
}
