using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	// LocatePearls makes three passes over the same shared entry - the region's saved objects,
	// then the save's object trackers, then the slugcat's stomach - and each one that finds the
	// pearl moves its marker. Which pass runs last therefore decides what you see, and the order
	// is the whole fix: swallowing a pearl deletes its tracker (Player.SwallowObject calls
	// RemovePersistentTracker), so the save's idea of where it lies is stale the moment you eat
	// it, while the slugcat always knows.
	//
	// The passes themselves need the game; this composes them the way LocatePearls does.
	public class SwallowedPearlScenarioTests {
		class Marker {
			public int Room;
			public Vector Pos;
		}

		struct Vector {
			public float X;
			public float Y;
			public Vector(float x, float y) { X = x; Y = y; }
		}

		const int ShelterRoom = 12;
		const int CorridorRoom = 30;

		static void LocatePearls(Marker marker, int? savedObjectRoom, int? trackerRoom, int? stomachRoom) {
			if (savedObjectRoom.HasValue) {
				marker.Room = savedObjectRoom.Value;
				marker.Pos = new Vector(TilePosition.Centre(4), TilePosition.Centre(4));
			}
			if (trackerRoom.HasValue) {
				marker.Room = trackerRoom.Value;
				marker.Pos = new Vector(TilePosition.Centre(8), TilePosition.Centre(8));
			}
			if (stomachRoom.HasValue) {
				marker.Room = stomachRoom.Value;
				marker.Pos = new Vector(123f, 456f); // the slugcat's own body position
			}
		}

		[Fact]
		public void TheStomachBeatsWhereTheSaveLastSawThePearl() {
			Marker marker = new Marker { Room = ShelterRoom };

			// you swallow the pearl and walk off: the region still has it saved in the shelter,
			// its tracker is gone, and you are three rooms away
			LocatePearls(marker, savedObjectRoom: ShelterRoom, trackerRoom: null, stomachRoom: CorridorRoom);

			Assert.Equal(CorridorRoom, marker.Room);
			Assert.Equal(123f, marker.Pos.X);
		}

		[Fact]
		public void TheMarkerFollowsYouFromRoomToRoom() {
			Marker marker = new Marker { Room = ShelterRoom };

			foreach (int room in new[] { 13, 14, 15, 16 }) {
				LocatePearls(marker, savedObjectRoom: ShelterRoom, trackerRoom: null, stomachRoom: room);
				Assert.Equal(room, marker.Room);
			}
		}

		// regurgitating puts the tracker back (Player.Regurgitate calls AddNewPersistentTracker),
		// and with nothing in the stomach the tracker is the last word again
		[Fact]
		public void SpittingThePearlBackOutHandsItToTheTracker() {
			Marker marker = new Marker { Room = CorridorRoom };

			LocatePearls(marker, savedObjectRoom: ShelterRoom, trackerRoom: CorridorRoom, stomachRoom: null);

			Assert.Equal(CorridorRoom, marker.Room);
			Assert.Equal(TilePosition.Centre(8), marker.Pos.X);
		}

		// nothing knows where it is: leave the marker alone rather than moving it somewhere wrong
		[Fact]
		public void NoSourceLeavesTheMarkerWhereItWas() {
			Marker marker = new Marker { Room = ShelterRoom, Pos = new Vector(1f, 2f) };

			LocatePearls(marker, savedObjectRoom: null, trackerRoom: null, stomachRoom: null);

			Assert.Equal(ShelterRoom, marker.Room);
			Assert.Equal(1f, marker.Pos.X);
		}
	}
}
