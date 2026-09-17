using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Menu;
using MoreSlugcats;

using UnityEngine;

namespace ExtendedCollectiblesTracker {
	static class CollectiblesTrackerExtension {
		public static List<string> presavePendingObjects = new List<string>();

		public static void ctor(CollectiblesTracker self, Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, SlugcatStats.Name saveSlot) {
			RainWorld rainWorld = menu.manager.rainWorld;
			PlayerProgression.MiscProgressionData miscProgressionData = rainWorld.progression.miscProgressionData;

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

					bool pearlRead = Mod.IsPearlRead(rainWorld, pearlType);

					// Filled once an iterator has read it, half filled while it is in the shelter
					// with you and still unread, empty otherwise. Read wins: there is nothing left
					// to do with that pearl wherever it happens to be lying.
					string element = pearlRead ? "dpOn"
						: withUniquePearls.Contains(pearlType) ? "dpHalf"
						: "dpOff";

					Color color = Mod.GetPearlIconColor(pearlType);
					self.spriteColors[regionName].Add(color);

					FSprite sprite = new(element)
					{
						color = color
					};

					self.sprites[regionName].Add(sprite);
					container.AddChild(sprite);
				}
			}
		}

	}
}