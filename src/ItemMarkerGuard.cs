using HUD;

namespace ExtendedCollectiblesTracker {
	// Keeps vanilla's key item markers from taking the screen down with them.
	//
	// Map.ItemMarker.Draw only builds its symbol when the item has icon data:
	//
	//     ItemMakerData? itemMakerData = ItemMakerData.DataFromAbstractPhysical(obj);
	//     if (symbol == null && itemMakerData.HasValue) { symbol = ...; }
	//     symbol.Draw(timeStacker, ...);
	//
	// so an item it can't draw an icon for leaves symbol null and the next line throws. It
	// reads obj.Room before that, which throws just as readily for an object whose world is
	// gone. Either way the exception escapes Map.Draw into RainWorldGame.GrafUpdate and takes
	// the whole frame's drawing with it, every frame, while the map is up - the game carries
	// on updating behind a picture that stopped, which is what the map appearing to freeze
	// actually is.
	//
	// Both need a tracked item that can't be drawn. A pearl that has been through another
	// mod's storage comes back without its type, which is the same thing that was putting
	// phantom pearl markers on the map.
	static class ItemMarkerGuard {
		// Skipping the whole call rather than catching after the fact: the throw is per frame
		// per marker, and the vanilla method has nothing useful left to do once it can't draw
		// its icon. A marker with a symbol already built is left alone - it can still draw.
		public static bool CanDraw(Map.ItemMarker marker) {
			try {
				AbstractPhysicalObject obj = marker.obj;
				if (obj == null || !marker.map.visible) {
					return true;
				}

				AbstractRoom room = obj.Room;
				if (room == null) {
					return false;
				}

				if (room.shelter || marker.symbol != null) {
					return true;
				}

				return Map.ItemMarker.ItemMakerData.DataFromAbstractPhysical(obj).HasValue;
			} catch {
				return false;
			}
		}
	}
}
