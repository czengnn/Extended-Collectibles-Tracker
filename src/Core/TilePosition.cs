namespace ExtendedCollectiblesTracker.Core {
	// Room tiles, and which ones are real. A WorldCoordinate's tile is not always a place: a
	// tracker's desiredSpawnLocation reads (-1, 0) when the game has not worked out where the item
	// would respawn yet, and taking that literally puts a marker a tile outside the room's corner.
	public static class TilePosition {
		public const float TileSize = 20f;

		public static bool IsUsable(int room, int x, int y) {
			return room >= 0 && x >= 0 && y >= 0;
		}

		// The middle of the tile rather than its corner, which is where the game draws an item it
		// only knows the tile of.
		public static float Centre(int tile) {
			return tile * TileSize + TileSize / 2f;
		}
	}
}
