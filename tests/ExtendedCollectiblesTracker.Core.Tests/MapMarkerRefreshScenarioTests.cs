using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	// A genuine in-game integration test (load Rain World, carry a pearl to a shelter,
	// hibernate, reopen the map) isn't something that can be automated here — it needs the
	// actual game/Unity/BepInEx runtime, which isn't available outside the real install (see
	// the PR description's manual test plan for that). This instead composes the extracted
	// pieces the same way Map.Update and CollectibleMarker.Update do in the real code, to
	// verify the end-to-end refresh behavior the bug fix depends on.
	public class MapMarkerRefreshScenarioTests {
		class FakeSaveData {
			public int TrueRoom;
			public bool TrueCollected;
		}

		// stands in for the shared MapDataExtensions.Extension.CollectibleData instance that
		// LocatePearls mutates and CollectibleMarker reads from every tick
		class FakeCollectibleData {
			public int Room;
			public bool Collected;
		}

		[Fact]
		public void PearlRelocation_OnlyVisibleToMarkerAfterNextRefreshTick() {
			const int interval = 40;
			var trueState = new FakeSaveData { TrueRoom = 5, TrueCollected = false };
			var sharedData = new FakeCollectibleData { Room = trueState.TrueRoom, Collected = trueState.TrueCollected };

			int markerRoom = sharedData.Room;
			bool markerCollected = sharedData.Collected;

			void Tick(int tick) {
				// Map.Update
				if (MapRefresh.ShouldRefresh(tick, interval)) {
					// LocatePearls: re-resolve the shared data from the current save state
					sharedData.Room = trueState.TrueRoom;
					sharedData.Collected = trueState.TrueCollected;
				}

				// CollectibleMarker.Update: re-sync from the shared data every tick
				markerRoom = sharedData.Room;
				markerCollected = sharedData.Collected;
			}

			for (int tick = 1; tick < interval; tick++) {
				Tick(tick);
			}
			Assert.Equal(5, markerRoom);
			Assert.False(markerCollected);

			// the player carries the pearl to a shelter and hibernates with it partway through
			// this window; the save data now reflects the new room, but nothing has refreshed
			// the marker's view of it yet
			trueState.TrueRoom = 12;
			trueState.TrueCollected = true;
			Assert.Equal(5, markerRoom);

			// Map's periodic refresh tick fires
			Tick(interval);

			Assert.Equal(12, markerRoom);
			Assert.True(markerCollected);
			Assert.Equal(CollectibleSymbols.PearlCollected, CollectibleSymbols.GetElementName(isPearl: true, markerCollected));
		}
	}
}
