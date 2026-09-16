using RWCustom;
using HUD;

using UnityEngine;

namespace ExtendedCollectiblesTracker {
	// Labels a room with its name just above its map shape, so it never covers anything
	// drawn inside the room itself. Unlike CollectibleMarker, a room's identity and size
	// never change, so this only needs to compute its position once.
	class RoomNameMarker : Map.FadeInMarker {
		FLabel label;

		public RoomNameMarker(Map map, int room, string roomName) : base(map, room, AboveRoom(map, room), 3f) {
			label = new FLabel("font", roomName) {
				scale = 0.5f,
				alignment = FLabelAlignment.Center,
				isVisible = false
			};
			map.inFrontContainer.AddChild(label);
		}

		static Vector2 AboveRoom(Map map, int room) {
			IntVector2 roomSize = map.mapData.SizeOfRoom(room);
			return new Vector2(roomSize.x * 10f, roomSize.y * 20f + 15f);
		}

		public override void Draw(float timeStacker) {
			base.Draw(timeStacker);

			bool mapVisible = map.visible;
			label.isVisible = Options.showRoomNames.Value && mapVisible;
			if (!mapVisible) return;

			float mapAlpha = Mathf.Lerp(map.lastFade, map.fade, timeStacker);
			float markerAlpha = Mathf.Lerp(lastFade, fade, timeStacker);

			Vector2 labelPos = map.RoomToMapPos(inRoomPos, room, timeStacker);
			label.x = labelPos.x;
			label.y = labelPos.y;
			label.alpha = mapAlpha * markerAlpha;
		}

		public override void Destroy() {
			base.Destroy();
			label.RemoveFromContainer();
		}
	}
}
