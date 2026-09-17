using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Menu;
using MoreSlugcats;

using UnityEngine;

namespace ExtendedCollectiblesTracker {
	static class CollectiblesTrackerExtension {
		public static List<string> presavePendingObjects = new List<string>();
		public class Extension {
			public int counter;
			public Dictionary<string, List<int>> inProgress = new();
		}

		static ConditionalWeakTable<CollectiblesTracker, Extension> extensions = new();

		public static Extension GetExtension(this CollectiblesTracker self) {
			return extensions.GetOrCreateValue(self);
		}

		public static void ctor(CollectiblesTracker self, Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, SlugcatStats.Name saveSlot) {
			RainWorld rainWorld = menu.manager.rainWorld;
			PlayerProgression.MiscProgressionData miscProgressionData = rainWorld.progression.miscProgressionData;
			Extension extendedSelf = self.GetExtension();

			// get pearls with you
			List<DataPearl.AbstractDataPearl.DataPearlType> withUniquePearls = new();

			if (rainWorld.progression.IsThereASavedGame(saveSlot)) {
				SaveState saveState = null;
				if (rainWorld.progression.currentSaveState != null) {
					saveState = rainWorld.progression.currentSaveState;
				} else if (rainWorld.progression.starvedSaveState != null) {
					saveState = rainWorld.progression.starvedSaveState;
				}

				if (saveState != null) {
					if (saveState.swallowedItems != null) {
						foreach (string swallowedItem in saveState.swallowedItems) {
							AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(null, swallowedItem);

							if (abstractPhysicalObject is DataPearl.AbstractDataPearl abstractDataPearl) {
								if (DataPearl.PearlIsNotMisc(abstractDataPearl.dataPearlType)) {
									withUniquePearls.Add(abstractDataPearl.dataPearlType);
								}
							}
						}
					}

					// A pearl in your hands is in the shelter with you as much as a swallowed one is,
					// but the save keeps what you were holding in its own list rather than with the
					// shelter's contents, so nothing below would have found it.
					if (saveState.playerGrasps != null) {
						foreach (string heldItem in saveState.playerGrasps) {
							if (string.IsNullOrEmpty(heldItem) || heldItem == "0") {
								continue;
							}

							AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(null, heldItem);

							if (abstractPhysicalObject is DataPearl.AbstractDataPearl heldPearl) {
								if (DataPearl.PearlIsNotMisc(heldPearl.dataPearlType)) {
									withUniquePearls.Add(heldPearl.dataPearlType);
								}
							}
						}
					}

					string denRoomName = saveState.GetSaveStateDenToUse();

					RegionState regionState = saveState.regionStates.FirstOrDefault(x => string.Equals(x?.regionName, self.collectionData.currentRegion, System.StringComparison.InvariantCultureIgnoreCase));
					if (regionState != null) {
						foreach (string savedObject in regionState.savedObjects) {
							AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(null, savedObject);
							if (abstractPhysicalObject == null) {
								continue;
							}

							if (denRoomName == abstractPhysicalObject.pos.ResolveRoomName()) {
								if (abstractPhysicalObject is DataPearl.AbstractDataPearl abstractDataPearl) {
									if (DataPearl.PearlIsNotMisc(abstractDataPearl.dataPearlType)) {
										withUniquePearls.Add(abstractDataPearl.dataPearlType);
									}
								}
							}
						}
					}

					foreach (string pendingObject in presavePendingObjects) {
						AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(null, pendingObject);
						if (abstractPhysicalObject == null) {
							continue;
						}

						if (denRoomName == abstractPhysicalObject.pos.ResolveRoomName()) {
							if (abstractPhysicalObject is DataPearl.AbstractDataPearl abstractDataPearl) {
								if (DataPearl.PearlIsNotMisc(abstractDataPearl.dataPearlType)) {
									withUniquePearls.Add(abstractDataPearl.dataPearlType);
								}
							}
						}
					}
				}
			}

			// append placed pearls to the trackers
			foreach (var regionPlacedPearls in rainWorld.regionDataPearls) {
				string regionName = regionPlacedPearls.Key;
				if (!(self.collectionData.regionsVisited.Contains(regionName) &&
					SlugcatStats.SlugcatStoryRegions(saveSlot).Contains(regionName.ToUpper()) &&
					self.sprites.ContainsKey(regionName))
				) {
					continue;
				}
				
				self.spriteColors[regionName].Add(Color.white);

				FSprite dividerSprite = new FSprite("dpSplit")
				{
					color = Color.white
				};

				self.sprites[regionName].Add(dividerSprite);
				container.AddChild(dividerSprite);

				foreach (var pearlData in regionPlacedPearls.Value) {
					DataPearl.AbstractDataPearl.DataPearlType pearlType = pearlData;

					if (withUniquePearls.Contains(pearlType)) {
						int spriteIndex = self.sprites[regionName].Count;
						if (!extendedSelf.inProgress.TryGetValue(regionName, out List<int> inProgress))
						{
							inProgress = new();
							extendedSelf.inProgress[regionName] = inProgress;
						}

						inProgress.Add(spriteIndex);
					}

					bool pearlRead = Mod.IsPearlRead(rainWorld, pearlType);

					Color color = Mod.GetPearlIconColor(pearlType);
					self.spriteColors[regionName].Add(color);

					FSprite sprite = new(pearlRead ? "dpOn" : "dpOff")
					{
						color = color
					};

					self.sprites[regionName].Add(sprite);
					container.AddChild(sprite);
				}
			}
		}

		public static void Update(CollectiblesTracker self) {
			Extension extendedSelf = self.GetExtension();
			extendedSelf.counter ++;
		}

		public static void GrafUpdate(CollectiblesTracker self, float timeStacker) {
			Extension extendedSelf = self.GetExtension();

			// Pulsing towards white only shows on a dark pearl. SL_moon's dot is (0.9, 0.95, 0.2)
			// before it is brightened further, so lerping it towards white moves red and green
			// almost nowhere and the dot sits there looking like every other one, while SI_top at
			// (0.01, 0.01, 0.01) swings the whole way. Oscillating either side of the pearl's own
			// colour instead keeps the hue and always travels the same distance, so a pale pearl
			// reads as clearly as a dark one.
			float pulse = (Mathf.Sin((extendedSelf.counter + timeStacker) / 20) + 1) / 2;

			try {
				foreach (KeyValuePair<string, List<int>> inprogress in extendedSelf.inProgress) {
					string regionName = inprogress.Key;
					foreach (int spriteIndex in inprogress.Value) {
						Color color = self.spriteColors[regionName][spriteIndex];
						self.sprites[regionName][spriteIndex].color = Color.Lerp(
							color * 0.35f,
							Color.Lerp(color, Color.white, 0.75f),
							pulse);
					}
				}
			} catch {
			}
		}
	}
}