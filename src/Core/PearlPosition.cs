namespace ExtendedCollectiblesTracker.Core {
	// Where a tracked pearl's marker takes its position from. See MapDataExtensions.LocatePearls.
	public enum PearlPositionSource {
		// the pearl is loaded in the world and has a body, which knows exactly where it is
		RealizedBody,
		// no body, but the live abstract object stays in step with whatever is carrying it
		LiveAbstract,
		// no live object either: the tracker's saved representation records where it last was
		SavedRepresentation,
		// last resort - where the pearl would reappear if abandoned, which is not where it is
		DesiredSpawn,
		// every source held a coordinate that isn't a real tile
		None
	}

	public static class PearlPosition {
		// Ordered by how much each source knows about where the pearl actually is. Getting this
		// order wrong is not a small error: desiredSpawnLocation reads (-1, 0) until the game has
		// worked a spawn out, so preferring it drew the marker a tile off the room's corner while
		// the saved representation held the real tile all along.
		public static PearlPositionSource Choose(
			bool hasRealizedBody,
			bool liveAbstractUsable,
			bool savedRepresentationUsable,
			bool desiredSpawnUsable
		) {
			if (hasRealizedBody) {
				return PearlPositionSource.RealizedBody;
			}
			if (liveAbstractUsable) {
				return PearlPositionSource.LiveAbstract;
			}
			if (savedRepresentationUsable) {
				return PearlPositionSource.SavedRepresentation;
			}
			if (desiredSpawnUsable) {
				return PearlPositionSource.DesiredSpawn;
			}
			return PearlPositionSource.None;
		}
	}
}
