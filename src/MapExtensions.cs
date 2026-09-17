using System;
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
			public List<RoomNameLabel> roomLabels = new();
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

			self.ResetNotRevealedMarkers();

			// Room labels are kept out of mapObjects on purpose, and so are built after
			// ResetNotRevealedMarkers: see RoomNameLabel for why they must not be markers.
			if (Options.showRoomNames.Value) {
				Extension extendedSelf = self.GetExtension();
				for (int i = 0; i < mapData.roomNames.Length; i++) {
					extendedSelf.roomLabels.Add(new RoomNameLabel(self, mapData.roomIndices[i], mapData.roomNames[i]));
				}
				RefreshVisitedRooms(self);
			}
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
				RefreshVisitedRooms(self);
			}
		}

		public static void Draw(Map self, float timeStacker) {
			Extension extendedSelf = self.GetExtension();
			if (extendedSelf.roomLabels.Count == 0) {
				return;
			}

			bool show = Options.showRoomNames.Value && self.visible;
			foreach (var roomLabel in extendedSelf.roomLabels) {
				roomLabel.Draw(self, timeStacker, show);
			}
		}

		// Which rooms the player has actually been to, so an unexplored region doesn't give away
		// its layout. This walks the save's visited list, so it runs on the periodic tick rather
		// than every frame.
		static void RefreshVisitedRooms(Map self) {
			Extension extendedSelf = self.GetExtension();
			if (extendedSelf.roomLabels.Count == 0) {
				return;
			}

			SaveState saveState = self.GetSaveState();
			if (saveState?.regionStates == null) {
				return;
			}

			List<string> roomsVisited = null;
			foreach (RegionState regionState in saveState.regionStates) {
				if (regionState != null &&
					string.Equals(regionState.regionName, self.mapData.regionName, StringComparison.InvariantCultureIgnoreCase)
				) {
					roomsVisited = regionState.roomsVisited;
					break;
				}
			}

			if (roomsVisited == null) {
				return;
			}

			HashSet<string> visited = new HashSet<string>(roomsVisited);
			foreach (var roomLabel in extendedSelf.roomLabels) {
				roomLabel.visited = visited.Contains(roomLabel.roomName);
			}
		}
	}
}
