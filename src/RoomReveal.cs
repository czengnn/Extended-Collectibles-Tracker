using HUD;
using RWCustom;
using UnityEngine;

using ExtendedCollectiblesTracker.Core;

namespace ExtendedCollectiblesTracker {
	// Marks a whole room as discovered the moment you stand in it, rather than the patch around you
	// that walking through it uncovers.
	//
	// The map keeps what you have seen in discoverTexture, one pixel per DiscoverResolution tiles,
	// and vanilla already fills a room's whole footprint that way - it is how a save that has lost
	// its discovery texture is rebuilt from the list of rooms you have visited. This does the same
	// thing for the room you just walked into, so the recipe below is vanilla's, not a guess at it:
	// the room's corner in texture space, then a pixel for every DiscoverResolution tiles across
	// and up.
	static class RoomReveal {
		public static void RevealRoom(Map map, int room) {
			Texture2D discoverTexture = map.discoverTexture;
			if (discoverTexture == null) {
				return;
			}

			IntVector2 roomSize = map.mapData.SizeOfRoom(room);
			if (roomSize.x <= 0 || roomSize.y <= 0) {
				return;
			}

			IntVector2 corner = IntVector2.FromVector2(
				map.OnTexturePos(Vector2.zero, room, accountForLayer: true) / map.DiscoverResolution);

			int columns = RoomFootprint.PixelSpan(roomSize.x, map.DiscoverResolution);
			int rows = RoomFootprint.PixelSpan(roomSize.y, map.DiscoverResolution);
			bool discoveredAnything = false;

			for (int x = 0; x < columns; x++) {
				for (int y = 0; y < rows; y++) {
					IntVector2 texturePos = new IntVector2(corner.x + x, corner.y + y);

					if (texturePos.x < 0 || texturePos.y < 0 ||
						texturePos.x >= discoverTexture.width || texturePos.y >= discoverTexture.height
					) {
						continue;
					}

					if (discoverTexture.GetPixel(texturePos.x, texturePos.y).r >= 1f) {
						continue;
					}

					discoverTexture.SetPixel(texturePos.x, texturePos.y, new Color(1f, 0f, 0f));

					// Discovered is not the same as drawn: the map fills in discovered pixels through
					// its own reveal list, so a pixel that isn't queued would sit there unseen until
					// something else reveals it.
					map.AddPixelToRevealList(texturePos);
					discoveredAnything = true;
				}
			}

			if (discoveredAnything) {
				discoverTexture.Apply();
			}
		}
	}
}
