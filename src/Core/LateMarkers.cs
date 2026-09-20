namespace ExtendedCollectiblesTracker.Core {
	// How a marker that appears after the map was built gets on screen. See
	// MapExtensions.AddMarkersForNewCollectibles.
	public enum LateMarkerReveal {
		// the map already shows that spot, so nothing will ever reveal it again to trigger a fade
		FadeInNow,
		// the spot is still hidden: queue it and let the reveal bring it in, as at map build time
		QueueForReveal
	}

	public static class LateMarkers {
		// The collectible list only ever grows - a pearl found again updates its own entry rather
		// than being appended twice - so everything past the count already drawn is new. Clamped
		// because drawing an entry twice is how a single pearl ends up with a pile of markers.
		public static int FirstUndrawnIndex(int entryCount, int markersDrawn) {
			if (markersDrawn < 0) {
				return 0;
			}
			return markersDrawn > entryCount ? entryCount : markersDrawn;
		}

		public static int UndrawnCount(int entryCount, int markersDrawn) {
			return entryCount - FirstUndrawnIndex(entryCount, markersDrawn);
		}

		// Both answers are wrong in their own way if applied to the other case: fading a marker in
		// over a spot the map hasn't revealed hands you a location you haven't earned, and queuing
		// one whose spot is already revealed leaves it invisible for good, since a revealed pixel
		// is never revealed a second time.
		public static LateMarkerReveal HowToShow(bool spotAlreadyRevealed) {
			return spotAlreadyRevealed ? LateMarkerReveal.FadeInNow : LateMarkerReveal.QueueForReveal;
		}
	}
}
