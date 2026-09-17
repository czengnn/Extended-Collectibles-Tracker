using RWCustom;
using HUD;

using UnityEngine;

namespace ExtendedCollectiblesTracker {
	// A room's name drawn just above its map shape, so it never covers anything inside the room.
	//
	// Deliberately not a Map.FadeInMarker, even though that would handle fading and positioning
	// for free: Map.ResetNotRevealedMarkers() collects every FadeInMarker into
	// notRevealedFadeMarkers, and the map walks that whole list for each pixel it reveals. One
	// marker per room turned that into several times the work over the region's entire reveal,
	// which froze the map and stalled the cycle transition on a black screen.
	class RoomNameLabel {
		public readonly string roomName;

		// A room's position, size and layer never change, so these are only worth computing once.
		readonly int room;
		readonly int layer;
		readonly Vector2 aboveRoom;
		readonly FLabel label;

		// Whether the map is drawing this room yet, so an unexplored region doesn't hand over its
		// layout. Refreshed periodically rather than per frame; see MapExtensions.
		public bool shown;

		public RoomNameLabel(Map map, int room, string roomName) {
			this.room = room;
			this.roomName = roomName;
			layer = map.mapData.LayerOfRoom(room);

			IntVector2 roomSize = map.mapData.SizeOfRoom(room);
			aboveRoom = new Vector2(roomSize.x * 10f, roomSize.y * 20f + 15f);

			label = new FLabel("font", roomName) {
				scale = 1.5f,
				alignment = FLabelAlignment.Center,
				isVisible = false
			};
			map.inFrontContainer.AddChild(label);
		}

		public void Draw(Map map, float timeStacker, bool show) {
			label.isVisible = show && shown;
			if (!label.isVisible) {
				return;
			}

			Vector2 labelPos = map.RoomToMapPos(aboveRoom, room, timeStacker);
			label.x = labelPos.x;
			label.y = labelPos.y;

			// Fade with the room's layer, the way the map fades everything else on a layer you
			// aren't looking at. Without this every layer's labels are drawn at full strength on
			// top of each other, which is unreadable wherever layers overlap.
			label.alpha = map.Alpha(layer, timeStacker, compensateForLayersInFront: true);
		}

		// Has the map revealed any of this room yet? The discover texture is what the map draws
		// rooms from, so this matches what you can see rather than where you have walked. Room
		// corners are sampled as well as the middle, since a big room is often revealed from one
		// end long before its centre is.
		public bool IsRoomDiscovered(Map map) {
			Texture2D discoverTexture = map.discoverTexture;
			if (discoverTexture == null) {
				return false;
			}

			IntVector2 roomSize = map.mapData.SizeOfRoom(room);
			Vector2 roomExtent = new Vector2(roomSize.x * 20f, roomSize.y * 20f);

			foreach (Vector2 sample in new[] {
				roomExtent / 2f,
				Vector2.zero,
				new Vector2(roomExtent.x, 0f),
				new Vector2(0f, roomExtent.y),
				roomExtent
			}) {
				IntVector2 texturePos = IntVector2.FromVector2(
					map.OnTexturePos(sample, room, accountForLayer: true) / map.DiscoverResolution);

				if (texturePos.x < 0 || texturePos.y < 0 ||
					texturePos.x >= discoverTexture.width || texturePos.y >= discoverTexture.height
				) {
					continue;
				}

				if (discoverTexture.GetPixel(texturePos.x, texturePos.y).r > 0f) {
					return true;
				}
			}

			return false;
		}

		public void Destroy() {
			label.RemoveFromContainer();
		}
	}
}
