namespace ExtendedCollectiblesTracker.Core {
	// Pure, game-independent logic extracted for unit testing. See MapDataExtensions.LocatePearls.
	public static class RegionIndex {
		public static bool IsValid(int region, int regionCount) {
			return region >= 0 && region < regionCount;
		}
	}
}
