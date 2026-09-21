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
			public List<RoomNameLabel> roomLabels = new();
			public CollectiblesPanel collectiblesPanel;
			// how many collectibles already have a marker; the list only ever grows, so anything
			// past this came along after the map was built
			public int markersCreated;
			public int counter;
			public bool mapWasOpen;
			public int lastRevealedRoom = -1;
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
			self.GetExtension().markersCreated = extendedMapData.collectibleData.Count;

			// Room labels are kept out of mapObjects on purpose, and so are built after
			// ResetNotRevealedMarkers: see RoomNameLabel for why they must not be markers.
			if (Options.showRoomNames.Value) {
				Extension extendedSelf = self.GetExtension();
				for (int i = 0; i < mapData.roomNames.Length; i++) {
					extendedSelf.roomLabels.Add(new RoomNameLabel(self, mapData.roomIndices[i], mapData.roomNames[i]));
				}
				RefreshShownRooms(self);
			}

			// Only for the map you hold while playing: the fast travel screen has its own summary
			// of the region it is showing, and a grid of every region over it would say nothing
			// about where you are about to travel to.
			if (Options.showCollectionTracker.Value && hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player) {
				self.GetExtension().collectiblesPanel = new CollectiblesPanel(self);
			}
		}

		// Runs before the vanilla update, and everything here depends on being before it.
		//
		// Vanilla decides whether the map is on screen with
		//
		//     lastFade = fade;                          // at the top of Update
		//     ...
		//     visible = fade > 0f && lastFade > 0f;
		//
		// so a fade set after the update can't be seen until the update after that. Setting it
		// here instead means lastFade picks it up on the same frame the button goes down, and
		// the map is on screen for that frame's draw rather than two frames later.
		//
		// That costs the one thing vanilla does for us. Its own setup,
		//
		//     if (lastFade == 0f) { if (revealAllDiscovered) RevealAllDiscovered(); InitiateMapView(); }
		//
		// only runs while lastFade is still zero, which is exactly what we've just stopped being
		// true, so the map would open centred wherever it was left. Hence doing both ourselves.
		// SchuhBaum's MapOptions drives its own skip-fade this way, for the same reason.
		public static void PreUpdate(Map self) {
			Extension extendedSelf = self.GetExtension();

			if (!Options.instantMap.Value || self.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player ||
				!self.mapLoaded || !self.discLoaded
			) {
				extendedSelf.mapWasOpen = false;
				return;
			}

			if (!self.hud.owner.RevealMap) {
				// Closing is the same problem in reverse: leave lastFade behind and the map hangs
				// on screen for a frame after the button comes up.
				extendedSelf.mapWasOpen = false;
				self.fadeCounter = 0;
				self.fade = 0f;
				self.lastFade = 0f;
				return;
			}

			bool justOpened = !extendedSelf.mapWasOpen;
			extendedSelf.mapWasOpen = true;

			// Past the hold delay, which is what fadeCounter counts out, and fully faded in.
			self.fadeCounter = 31;
			self.fade = 1f;

			if (!justOpened) {
				return;
			}

			// Centre on the slugcat, then fill in everything the save has discovered. In that
			// order, unlike vanilla's: InitiateMapView can call ResetReveal, which wipes the
			// reveal texture, and doing that second would wipe what we had just revealed. Only on
			// the frame it opens - RevealAllDiscovered is a GetPixel and a SetPixel for every
			// pixel of the region, which is not something to repeat while the map is held.
			self.InitiateMapView();
			self.revealAllDiscovered = true;
			self.RevealAllDiscovered();
		}

		// The vanilla update has just eased fade back towards its target; hold it where PreUpdate
		// put it so the map stays put instead of easing in behind the first frame.
		public static void PostUpdate(Map self) {
			if (!Options.instantMap.Value || self.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player) {
				return;
			}

			self.fade = self.hud.owner.RevealMap && !self.hud.HideGeneralHud ? 1f : 0f;
		}

		public static void Update(Map self) {
			Extension extendedSelf = self.GetExtension();
			extendedSelf.counter++;

			RevealRoomEntered(self, extendedSelf);

			// re-resolve pearl locations and token/broadcast collected state periodically so
			// markers follow pearls relocated after the map was built (e.g. carried to a
			// shelter) and fill in as soon as a token is collected, instead of only updating
			// the next time the map itself gets rebuilt (e.g. on hibernation)
			if (MapRefresh.ShouldRefresh(extendedSelf.counter, CollectibleRefreshInterval)) {
				self.mapData.LocatePearls(self.hud.rainWorld);
				self.mapData.RefreshTokens();
				AddMarkersForNewCollectibles(self, extendedSelf);
				RefreshShownRooms(self);
				extendedSelf.collectiblesPanel?.RefreshProgress(self);
			}

			// What you're carrying every tick, so a pearl's ring appears as you pick it up; what
			// you've collected on the interval above, since that is the half that asks the file
			// system.
			extendedSelf.collectiblesPanel?.RefreshCarried(self);
		}

		public static void Draw(Map self, float timeStacker) {
			Extension extendedSelf = self.GetExtension();

			extendedSelf.collectiblesPanel?.Draw(self, timeStacker,
				Options.showCollectionTracker.Value && self.visible && !self.hud.HideGeneralHud);

			if (extendedSelf.roomLabels.Count == 0) {
				return;
			}

			bool show = Options.showRoomNames.Value && self.visible;
			foreach (var roomLabel in extendedSelf.roomLabels) {
				roomLabel.Draw(self, timeStacker, show);
			}
		}

		// A pearl can become known to the map after the map was built - carried in from another
		// region, or swallowed, which drops it out of the save's trackers entirely. LocatePearls
		// adds an entry for it, but markers were only ever created alongside the map, so that
		// entry had nothing drawing it.
		static void AddMarkersForNewCollectibles(Map self, Extension extendedSelf) {
			var collectibleData = self.mapData.GetExtension().collectibleData;

			for (int i = LateMarkers.FirstUndrawnIndex(collectibleData.Count, extendedSelf.markersCreated);
				i < collectibleData.Count;
				i++
			) {
				CollectibleMarker marker = new CollectibleMarker(self, collectibleData[i]);
				self.mapObjects.Add(marker);

				// Markers built with the map are queued in notRevealedFadeMarkers and faded in as
				// the reveal reaches them. A marker built later has missed that pass, and
				// ResetNotRevealedMarkers can't be used to catch it up - that one hides every
				// marker again, and a marker whose pixel is already revealed would never be
				// revealed a second time to bring it back. See Core.LateMarkers.
				if (LateMarkers.HowToShow(IsSpotRevealed(self, collectibleData[i])) == LateMarkerReveal.FadeInNow) {
					marker.FadeIn(30f);
				} else {
					marker.SetInvisible();
					self.notRevealedFadeMarkers.Add(marker);
				}
			}

			extendedSelf.markersCreated = collectibleData.Count;
		}

		static bool IsSpotRevealed(Map self, MapDataExtensions.Extension.CollectibleData collectibleData) {
			if (self.revealTexture == null) {
				return false;
			}

			IntVector2 texturePos = IntVector2.FromVector2(
				self.OnTexturePos(collectibleData.pos, collectibleData.room, accountForLayer: true) / self.DiscoverResolution);

			if (texturePos.x < 0 || texturePos.y < 0 ||
				texturePos.x >= self.revealTexture.width || texturePos.y >= self.revealTexture.height
			) {
				return false;
			}

			return self.revealTexture.GetPixel(texturePos.x, texturePos.y).r > 0f;
		}

		// The map takes its own sprites out of the container here; ours live in the same container
		// and would otherwise be left behind, drawn over whatever the HUD builds next.
		public static void ClearSprites(Map self) {
			Extension extendedSelf = self.GetExtension();

			foreach (var roomLabel in extendedSelf.roomLabels) {
				roomLabel.Destroy();
			}
			extendedSelf.roomLabels.Clear();

			extendedSelf.collectiblesPanel?.Destroy();
			extendedSelf.collectiblesPanel = null;
		}

		// Once per room entered rather than every tick: filling a room is cheap, but it walks the
		// room's pixels and applies the texture, which is not worth repeating while you stand still.
		// The map keeps discovering around you as you walk either way, so a room you enter before
		// this can run - while the discover texture is still loading - is not left blank.
		static void RevealRoomEntered(Map self, Extension extendedSelf) {
			if (!Options.revealWholeRoom.Value || !self.discLoaded ||
				self.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player
			) {
				return;
			}

			int room = self.hud.owner.MapOwnerRoom;
			if (room == extendedSelf.lastRevealedRoom) {
				return;
			}

			extendedSelf.lastRevealedRoom = room;
			RoomReveal.RevealRoom(self, room);
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
