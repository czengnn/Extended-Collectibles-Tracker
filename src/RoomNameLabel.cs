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

		// A room's position and size never change, so this is only worth computing once.
		readonly int room;
		readonly Vector2 aboveRoom;
		readonly FLabel label;

		// Whether the room has been visited, so an unexplored region doesn't hand over its
		// layout. Refreshed periodically rather than per frame; see MapExtensions.
		public bool visited;

		public RoomNameLabel(Map map, int room, string roomName) {
			this.room = room;
			this.roomName = roomName;

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
			label.isVisible = show && visited;
			if (!label.isVisible) {
				return;
			}

			Vector2 labelPos = map.RoomToMapPos(aboveRoom, room, timeStacker);
			label.x = labelPos.x;
			label.y = labelPos.y;
			label.alpha = Mathf.Lerp(map.lastFade, map.fade, timeStacker);
		}

		public void Destroy() {
			label.RemoveFromContainer();
		}
	}
}
