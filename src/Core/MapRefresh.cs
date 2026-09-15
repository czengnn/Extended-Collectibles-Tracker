namespace ExtendedCollectiblesTracker.Core {
	// Pure, game-independent logic extracted for unit testing. See MapExtensions.Update: this
	// decides which ticks re-run LocatePearls so a relocated pearl's marker catches up.
	public static class MapRefresh {
		public static bool ShouldRefresh(int counter, int interval) {
			return interval > 0 && counter % interval == 0;
		}
	}
}
