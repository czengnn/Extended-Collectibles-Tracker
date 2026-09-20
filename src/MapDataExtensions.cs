
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using HUD;
using MoreSlugcats;
using UnityEngine;

using ExtendedCollectiblesTracker.Core;

namespace ExtendedCollectiblesTracker {
	static class MapDataExtensions {
		public class Extension {
			public int region;
			public class CollectibleData {
				public int order;
				public int room;
				public Vector2 pos;
				public Color color;
				public Color innerColor;
				public bool collected;
				public bool isPearl;
				public bool isRelocated;
				// Which pearl this entry is, so a pearl found again later updates its own entry
				// instead of being added a second time. Null for tokens.
				public DataPearl.AbstractDataPearl.DataPearlType pearlType;
				// tokens have no live position to relocate, but their collected state can still
				// change while the map is open; pearls are refreshed separately by LocatePearls,
				// so this stays null for them.
				public Func<bool> refreshCollected;
			}
			public List<CollectibleData> collectibleData = new();
		}

		static ConditionalWeakTable<Map.MapData, Extension> extensions = new();

		public static Extension GetExtension(this Map.MapData self) {
			return extensions.GetOrCreateValue(self);
		}

		// Saved positions are in tiles; markers are placed in the room's own coordinates.
		static Vector2 TileToInRoomPos(WorldCoordinate pos) {
			return new Vector2(TilePosition.Centre(pos.x), TilePosition.Centre(pos.y));
		}

		static bool IsUsable(WorldCoordinate pos) {
			return TilePosition.IsUsable(pos.room, pos.x, pos.y);
		}

		// Misc pearls aren't tracked, and neither are pearls that come back from a save string
		// without a type: they carry no identity, so they can't be told apart, coloured, or found
		// again, and they turn into markers pointing at a pearl that isn't there.
		static bool IsTrackablePearl(DataPearl.AbstractDataPearl.DataPearlType pearlType) {
			return pearlType != null
				&& !string.IsNullOrEmpty(pearlType.value)
				&& DataPearl.PearlIsNotMisc(pearlType);
		}

		public static void ctor(Map.MapData self, World initWorld, RainWorld rainWorld) {
			if (initWorld == null) {
				return; // no data between map loads, no world exists in The Watcher DLC
			}
			
			RainWorldGame game = rainWorld.processManager.currentMainLoop as RainWorldGame;
			Extension extendedSelf = self.GetExtension();
			extendedSelf.region = initWorld.region.regionNumber;
			
			PlayerProgression.MiscProgressionData miscProgressionData = rainWorld.progression.miscProgressionData;

			SaveState saveState = null;
			if (saveState == null && rainWorld.progression.IsThereASavedGame(rainWorld.progression.PlayingAsSlugcat)) {
				if (rainWorld.progression.starvedSaveState != null) {
					saveState = rainWorld.progression.starvedSaveState;
				} else if (rainWorld.progression.currentSaveState != null) {
					saveState = rainWorld.progression.currentSaveState;
				}
			}

			// per room
			foreach (var roomIndex in self.roomIndices) {
				AbstractRoom abstractRoom = initWorld.GetAbstractRoom(roomIndex);

				RoomSettings roomSettings = new RoomSettings(abstractRoom.name, initWorld.region, false, false, game?.TimelinePoint, game);

				// per object
				for (int i = 0; i < roomSettings.placedObjects.Count; i++) {
					PlacedObject placedObject = roomSettings.placedObjects[i];

					if (placedObject.active && placedObject.data is PlacedObject.DataPearlData pearlData) {
						var pearlType = pearlData.pearlType;
						if (!IsTrackablePearl(pearlType))
							continue;

						bool pearlRead = Mod.IsPearlRead(rainWorld, pearlType);

						extendedSelf.collectibleData.Add(new Extension.CollectibleData() {
							pearlType = pearlType,
							room = roomIndex,
							pos = placedObject.pos,
							color = Mod.GetPearlIconColor(pearlType),
							innerColor = DataPearl.UniquePearlHighLightColor(pearlType).GetValueOrDefault(Color.white),
							collected = pearlRead || (saveState != null && saveState.ItemConsumed(initWorld, false, roomIndex, i)),
							isPearl = true
						});
					} else if (placedObject.type == PlacedObject.Type.BlueToken || placedObject.type == PlacedObject.Type.GoldToken) {
						CollectToken.CollectTokenData tokenData = (CollectToken.CollectTokenData)placedObject.data;
						if (!tokenData.availableToPlayers.Contains(rainWorld.progression.PlayingAsSlugcat))
							continue;

						extendedSelf.collectibleData.Add(new Extension.CollectibleData() {
							order = tokenData.isBlue ? 0 : 1,
							room = roomIndex,
							pos = placedObject.pos,
							color = tokenData.isBlue ? RainWorld.AntiGold.rgb : RainWorld.GoldRGB,
							collected = miscProgressionData.GetTokenCollected(tokenData.tokenString, tokenData.isBlue),
							refreshCollected = () => miscProgressionData.GetTokenCollected(tokenData.tokenString, tokenData.isBlue)
						});
					} else if (placedObject.type == MoreSlugcatsEnums.PlacedObjectType.RedToken) {
						CollectToken.CollectTokenData tokenData = (CollectToken.CollectTokenData)placedObject.data;
						if (!tokenData.availableToPlayers.Contains(rainWorld.progression.PlayingAsSlugcat))
							continue;
						
						extendedSelf.collectibleData.Add(new Extension.CollectibleData() {
							order = -1,
							room = roomIndex,
							pos = placedObject.pos,
							color = CollectToken.RedColor.rgb,
							collected = miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID(tokenData.tokenString, false)),
							refreshCollected = () => miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID(tokenData.tokenString, false))
						});
					} else if (placedObject.type == PlacedObject.Type.GreenToken) {
						CollectToken.CollectTokenData tokenData = (CollectToken.CollectTokenData)placedObject.data;
						if (!tokenData.availableToPlayers.Contains(rainWorld.progression.PlayingAsSlugcat))
							continue;
						
						extendedSelf.collectibleData.Add(new Extension.CollectibleData() {
							order = -2,
							room = roomIndex,
							pos = placedObject.pos,
							color = CollectToken.GreenColor.rgb,
							collected = miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SlugcatUnlockID(tokenData.tokenString, false)),
							refreshCollected = () => miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SlugcatUnlockID(tokenData.tokenString, false))
						});
					} else if (placedObject.type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken) {
						if (rainWorld.progression.PlayingAsSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear) {
							CollectToken.CollectTokenData tokenData = (CollectToken.CollectTokenData)placedObject.data;
							if (!tokenData.availableToPlayers.Contains(rainWorld.progression.PlayingAsSlugcat))
								continue;

							if (ChatlogData.HasUnique(tokenData.ChatlogCollect)) {
								extendedSelf.collectibleData.Add(new Extension.CollectibleData() {

									order = 2,
									room = roomIndex,
									pos = placedObject.pos,
									color = CollectToken.WhiteColor.rgb,
									collected = miscProgressionData.GetBroadcastListened(tokenData.ChatlogCollect),
									refreshCollected = () => miscProgressionData.GetBroadcastListened(tokenData.ChatlogCollect)
								});
							} else {
								bool ChatlogRead() {
									SaveState currentSaveState = rainWorld.progression.currentSaveState;
									if (currentSaveState == null) {
										return false;
									}
									return currentSaveState.miscWorldSaveData.SSaiConversationsHad == 0 ?
										currentSaveState.deathPersistentSaveData.prePebChatlogsRead.Contains(tokenData.ChatlogCollect) :
										currentSaveState.deathPersistentSaveData.chatlogsRead.Contains(tokenData.ChatlogCollect);
								}

								extendedSelf.collectibleData.Add(new Extension.CollectibleData() {
									order = 2,
									room = roomIndex,
									pos = placedObject.pos,
									color = Color.white,
									collected = ChatlogRead(),
									refreshCollected = ChatlogRead
								});
							}
						}
					}
				}
			}
		}

		public static void LocatePearls(this Map.MapData self, RainWorld rainWorld) {
			Extension extendedSelf = self.GetExtension();

			LocateSavedPearls(self, extendedSelf, rainWorld);

			// Last, so it wins: a swallowed pearl is somewhere the save still thinks it left it,
			// and the slugcat knows better.
			LocateSwallowedPearls(extendedSelf, rainWorld);
		}

		// A pearl in a slugcat's stomach is in none of the places above. Swallowing one throws its
		// tracker away - Player.SwallowObject calls RemovePersistentTracker, which takes it out of
		// objectTrackers altogether - and it isn't among the region's saved objects either, since
		// it isn't in a room. So nothing refreshed its marker: it sat where the pearl was eaten,
		// still showing whatever read state it had at the time, until hibernation rebuilt the map
		// from scratch. Asking the slugcat is the only way to know.
		static void LocateSwallowedPearls(Extension extendedSelf, RainWorld rainWorld) {
			if (rainWorld.processManager.currentMainLoop is not RainWorldGame game || game.Players == null) {
				return;
			}

			foreach (AbstractCreature abstractPlayer in game.Players) {
				if (abstractPlayer?.realizedCreature is not Player player) {
					continue;
				}

				if (player.objectInStomach is not DataPearl.AbstractDataPearl swallowedPearl) {
					continue;
				}

				var pearlType = swallowedPearl.dataPearlType;
				if (!IsTrackablePearl(pearlType)) {
					continue;
				}

				if (player.firstChunk != null && IsUsable(abstractPlayer.pos)) {
					RelocatePearl(extendedSelf, rainWorld, pearlType,
						abstractPlayer.pos.room, player.firstChunk.pos);
				} else if (IsUsable(abstractPlayer.pos)) {
					RelocatePearl(extendedSelf, rainWorld, pearlType,
						abstractPlayer.pos.room, TileToInRoomPos(abstractPlayer.pos));
				}
			}
		}

		static void LocateSavedPearls(Map.MapData self, Extension extendedSelf, RainWorld rainWorld) {
			if (!rainWorld.progression.IsThereASavedGame(rainWorld.progression.PlayingAsSlugcat) ||
				rainWorld.progression.currentSaveState == null
			) {
				return;
			}

			RegionState[] regionStates = rainWorld.progression.currentSaveState.regionStates;
			if (!RegionIndex.IsValid(extendedSelf.region, regionStates.Length)) {
				return;
			}

			RegionState currentRegion = regionStates[extendedSelf.region];
			if (currentRegion == null || currentRegion.savedObjects == null) {
				return;
			}

			foreach (string savedObject in currentRegion.savedObjects) {
				AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(null, savedObject);
				
				if (abstractPhysicalObject is DataPearl.AbstractDataPearl abstractDataPearl) {
					var pearlType = abstractDataPearl.dataPearlType;
					if (!IsTrackablePearl(pearlType))
						continue;

					WorldCoordinate savedPos = abstractPhysicalObject.pos;
					if (IsUsable(savedPos)) {
						RelocatePearl(extendedSelf, rainWorld, pearlType,
							savedPos.room, TileToInRoomPos(savedPos));
					}
				}
			}

			var objectTrackers = rainWorld.progression.currentSaveState.objectTrackers;
			if (objectTrackers == null) {
				return;
			}

			foreach (PersistentObjectTracker trackedObject in objectTrackers) {
				if (trackedObject.lastSeenRegion != self.regionName)
					continue;

				// Never store this back: we parse with a null world, and HUD.Map's constructor
				// throws reading .Room on it, which hangs the sleep/death screen.
				AbstractPhysicalObject trackedPhysicalObject = trackedObject.obj
					?? SaveState.AbstractPhysicalObjectFromString(null, trackedObject.objRepresentation);

				if (trackedPhysicalObject is DataPearl.AbstractDataPearl abstractDataPearl) {
					var pearlType = abstractDataPearl.dataPearlType;
					if (!IsTrackablePearl(pearlType))
						continue;

					// While the pearl is loaded in the world its body knows exactly where it is,
					// which matters when it's being carried: the tracker only records where the
					// pearl would respawn, so the marker would otherwise sit at a stale tile
					// while the pearl moves around the room in your hands.
					// Which of the pearl's recorded positions to believe, in order of how much each
					// knows about where it actually is. See Core.PearlPosition.
					PhysicalObject realizedPearl = trackedObject.obj?.realizedObject;
					WorldCoordinate liveAbstractPos = trackedObject.obj?.pos ?? default;

					switch (PearlPosition.Choose(
						hasRealizedBody: realizedPearl?.firstChunk != null,
						liveAbstractUsable: IsUsable(liveAbstractPos),
						savedRepresentationUsable: IsUsable(trackedPhysicalObject.pos),
						desiredSpawnUsable: IsUsable(trackedObject.desiredSpawnLocation)
					)) {
						case PearlPositionSource.RealizedBody:
							RelocatePearl(extendedSelf, rainWorld, pearlType,
								trackedObject.obj.pos.room, realizedPearl.firstChunk.pos);
							break;

						case PearlPositionSource.LiveAbstract:
							RelocatePearl(extendedSelf, rainWorld, pearlType,
								liveAbstractPos.room, TileToInRoomPos(liveAbstractPos));
							break;

						case PearlPositionSource.SavedRepresentation:
							RelocatePearl(extendedSelf, rainWorld, pearlType,
								trackedPhysicalObject.pos.room, TileToInRoomPos(trackedPhysicalObject.pos));
							break;

						case PearlPositionSource.DesiredSpawn:
							RelocatePearl(extendedSelf, rainWorld, pearlType,
								trackedObject.desiredSpawnLocation.room,
								TileToInRoomPos(trackedObject.desiredSpawnLocation));
							break;

						// nothing usable: leave the marker wherever a better source already put it
					}
				}
			}
		}

		// Move a pearl's marker to where the save says it is now, adding one if this pearl has no
		// marker yet. Matching is by pearl type held on the entry itself: a parallel lookup table
		// used to serve this, and one of the two callers forgot to keep it updated, so that pearl
		// was never found again and a duplicate entry was appended on every refresh - several a
		// second, for as long as the map existed.
		static void RelocatePearl(
			Extension extendedSelf,
			RainWorld rainWorld,
			DataPearl.AbstractDataPearl.DataPearlType pearlType,
			int room,
			Vector2 inRoomPos
		) {
			bool pearlRead = Mod.IsPearlRead(rainWorld, pearlType);

			foreach (Extension.CollectibleData collectibleData in extendedSelf.collectibleData) {
				if (!collectibleData.isPearl || collectibleData.pearlType == null ||
					!collectibleData.pearlType.Equals(pearlType)
				) {
					continue;
				}

				collectibleData.room = room;
				collectibleData.pos = inRoomPos;
				collectibleData.isRelocated = true;
				collectibleData.collected = pearlRead;
				return;
			}

			extendedSelf.collectibleData.Add(new Extension.CollectibleData() {
				pearlType = pearlType,
				room = room,
				pos = inRoomPos,
				color = Mod.GetPearlIconColor(pearlType),
				innerColor = DataPearl.UniquePearlHighLightColor(pearlType).GetValueOrDefault(Color.white),
				collected = pearlRead,
				isPearl = true,
				isRelocated = true,
			});
		}

		public static void RefreshTokens(this Map.MapData self) {
			Extension extendedSelf = self.GetExtension();

			foreach (Extension.CollectibleData collectibleData in extendedSelf.collectibleData) {
				if (collectibleData.refreshCollected != null) {
					collectibleData.collected = collectibleData.refreshCollected();
				}
			}
		}
	}
}