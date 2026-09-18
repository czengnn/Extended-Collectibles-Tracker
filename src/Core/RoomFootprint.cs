namespace ExtendedCollectiblesTracker.Core {
	// How much of the map's discovery texture a room covers. The texture holds one pixel per
	// DiscoverResolution tiles, and vanilla walks a room's footprint with `n <= size / resolution`
	// - inclusive, so a room narrower than one pixel still gets one. See RoomReveal.
	public static class RoomFootprint {
		public static int PixelSpan(int sizeInTiles, float discoverResolution) {
			if (sizeInTiles <= 0 || discoverResolution <= 0f) {
				return 0;
			}

			return (int)(sizeInTiles / discoverResolution) + 1;
		}
	}
}
