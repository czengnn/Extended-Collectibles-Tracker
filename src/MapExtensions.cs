using System.Collections.Generic;
using System.Runtime.CompilerServices;

using HUD;
using RWCustom;
using UnityEngine;

using ExtendedCollectiblesTracker.Core;

namespace ExtendedCollectiblesTracker {
	static class MapExtensions {

		public class Extension {
			public List<CollectibleMarker> tokenMarkers = new();
			public int counter;
		}

		const int CollectibleRefreshInterval = 40;

		static ConditionalWeakTable<Map, Extension> extensions = new();

		public static Extension GetExtension(this Map self) {
			return extensions.GetOrCreateValue(self);
		}

		// public static void Pre_ctor(Map self, HUD.HUD hud, Map.MapData mapData) {
		// }

		public static void Post_ctor(Map self, HUD.HUD hud, Map.MapData mapData) {
			mapData.LocatePearls(hud.rainWorld);
			MapDataExtensions.Extension extendedMapData = mapData.GetExtension();

			foreach (var collectibleData in extendedMapData.collectibleData) {
				self.mapObjects.Add(new CollectibleMarker(self, collectibleData));
			}

			foreach (var roomName in extendedMapData.roomNames) {
				self.mapObjects.Add(new RoomNameMarker(self, roomName.Key, roomName.Value));
			}

			self.ResetNotRevealedMarkers();
		}

		public static void Update(Map self) {
			Extension extendedSelf = self.GetExtension();
			extendedSelf.counter++;

			// re-resolve pearl locations and token/broadcast collected state periodically so
			// markers follow pearls relocated after the map was built (e.g. carried to a
			// shelter) and fill in as soon as a token is collected, instead of only updating
			// the next time the map itself gets rebuilt (e.g. on hibernation)
			if (MapRefresh.ShouldRefresh(extendedSelf.counter, CollectibleRefreshInterval)) {
				self.mapData.LocatePearls(self.hud.rainWorld);
				self.mapData.RefreshTokens();
			}
		}
	}
}