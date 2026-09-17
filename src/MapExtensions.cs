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
			public bool mapWasOpen;
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
				RefreshShownRooms(self);
			}
		}

		// Runs before the vanilla update. The map already knows how to do this: revealAllDiscovered
		// makes it fill in everything the save has discovered the moment it opens, and the map only
		// starts appearing once fadeCounter passes 30, which is the hold delay. Driving those two
		// is enough, so none of the reveal machinery itself is touched.
		public static void PreUpdate(Map self) {
			Extension extendedSelf = self.GetExtension();

			if (!Options.instantMap.Value || self.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player) {
				extendedSelf.mapWasOpen = false;
				return;
			}

			bool open = self.hud.owner.RevealMap;

			// Ask for the full reveal only on the frame the map opens. Snapping fade below means
			// lastFade hits zero far more readily than vanilla's easing ever let it, and vanilla
			// redoes RevealAllDiscovered() every time it sees that - a GetPixel and SetPixel over
			// the whole texture, which stalls the game for as long as the map is held.
			self.revealAllDiscovered = open && !extendedSelf.mapWasOpen;
			extendedSelf.mapWasOpen = open;

			if (open && self.fadeCounter <= 30) {
				self.fadeCounter = 31;
			}
		}

		// Runs after the vanilla update, which has just eased fade towards its target; snap it the
		// rest of the way so the map appears and disappears with the button instead of fading.
		public static void PostUpdate(Map self) {
			if (!Options.instantMap.Value || self.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player) {
				return;
			}

			self.fade = self.hud.owner.RevealMap && !self.hud.HideGeneralHud ? 1f : 0f;
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
				RefreshShownRooms(self);
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

		// Label a room once the map is actually drawing it, which is what the discover texture
		// records - rooms get revealed by being near them, not only by walking in, so the save's
		// visited list leaves named rooms blank. Sampling a texture is too slow for every frame,
		// so this rides the periodic tick and each label keeps its answer in between.
		static void RefreshShownRooms(Map self) {
			Extension extendedSelf = self.GetExtension();
			if (extendedSelf.roomLabels.Count == 0 || self.discoverTexture == null) {
				return;
			}

			foreach (var roomLabel in extendedSelf.roomLabels) {
				roomLabel.shown = roomLabel.IsRoomDiscovered(self);
			}
		}
	}
}
