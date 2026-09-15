namespace ExtendedCollectiblesTracker.Core {
	// Pure, game-independent logic extracted for unit testing. See CollectibleMarker.Update:
	// this picks which atlas element a marker's dot should show, kept in sync with the shared
	// collectible data every tick so a pearl's dot updates once it's read.
	public static class CollectibleSymbols {
		public const string PearlCollected = "dpOn";
		public const string PearlUncollected = "dpOff";
		public const string TokenCollected = "ctOn";
		public const string TokenUncollected = "ctOff";

		public static string GetElementName(bool isPearl, bool collected) {
			if (isPearl) {
				return collected ? PearlCollected : PearlUncollected;
			}
			return collected ? TokenCollected : TokenUncollected;
		}
	}
}
