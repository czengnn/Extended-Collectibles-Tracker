using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class PearlPositionTests {
		[Fact]
		public void ARealizedBodyBeatsEverything() {
			Assert.Equal(PearlPositionSource.RealizedBody, PearlPosition.Choose(
				hasRealizedBody: true, liveAbstractUsable: true,
				savedRepresentationUsable: true, desiredSpawnUsable: true));
		}

		[Fact]
		public void TheLiveAbstractObjectComesNext() {
			Assert.Equal(PearlPositionSource.LiveAbstract, PearlPosition.Choose(
				hasRealizedBody: false, liveAbstractUsable: true,
				savedRepresentationUsable: true, desiredSpawnUsable: true));
		}

		// the bug this ordering exists for: the tracker's spawn location read (-1, 0) while its
		// own saved representation held the shelter tile the pearl was lying on, and preferring
		// the spawn location drew the marker a tile off the room's corner
		[Fact]
		public void SavedRepresentationBeatsDesiredSpawn() {
			Assert.Equal(PearlPositionSource.SavedRepresentation, PearlPosition.Choose(
				hasRealizedBody: false, liveAbstractUsable: false,
				savedRepresentationUsable: true, desiredSpawnUsable: true));
		}

		[Fact]
		public void DesiredSpawnIsUsedOnlyWhenNothingElseCan() {
			Assert.Equal(PearlPositionSource.DesiredSpawn, PearlPosition.Choose(
				hasRealizedBody: false, liveAbstractUsable: false,
				savedRepresentationUsable: false, desiredSpawnUsable: true));
		}

		// nothing usable means leave the marker wherever a better source already put it, rather
		// than drawing it at a coordinate that isn't a place
		[Fact]
		public void NoUsableSourceIsReported() {
			Assert.Equal(PearlPositionSource.None, PearlPosition.Choose(
				hasRealizedBody: false, liveAbstractUsable: false,
				savedRepresentationUsable: false, desiredSpawnUsable: false));
		}
	}
}
