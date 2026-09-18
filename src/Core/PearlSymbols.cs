namespace ExtendedCollectiblesTracker.Core {
	// Which dot a pearl shows, on the sleep screen and on the map's panel alike. Two independent
	// facts: whether an iterator has read it, and whether it is on you right now. Fill says the
	// first, the ring around it says the second - a read pearl is still worth carrying (scavengers
	// pay well for one), so "read" must not swallow "and it's in my hands".
	public static class PearlSymbols {
		public const string Unread = "dpUnread";
		public const string UnreadWithYou = "dpUnreadHeld";
		public const string Read = "dpRead";
		public const string ReadWithYou = "dpReadHeld";

		public static string GetElementName(bool read, bool withYou) {
			if (read) {
				return withYou ? ReadWithYou : Read;
			}
			return withYou ? UnreadWithYou : Unread;
		}
	}
}
