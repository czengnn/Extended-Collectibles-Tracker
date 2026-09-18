using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HUD;
using MoreSlugcats;
using UnityEngine;

using ExtendedCollectiblesTracker.Core;

namespace ExtendedCollectiblesTracker {
	// The sleep screen's collectible grid, drawn over the map while you hold it: a column per
	// region you've been to, its tokens, then its pearls under a divider.
	//
	// Vanilla's CollectiblesTracker can't be borrowed for this. It is a PositionedMenuObject whose
	// constructor wants a Menu.Menu, and there isn't one in a running game - faking one to get at
	// a grid of sprites is the kind of thing that breaks on the next update. The data behind it is
	// all public though: the region token lists hang off RainWorld, and the unlocked lists vanilla
	// builds are just GetTokenCollected called over every id, so asking per token is the same
	// answer for less work.
	//
	// What is better here than on the sleep screen: the game is running, so what you are carrying
	// is simply what is in your hands and your stomach, rather than something to infer from a save.
	class CollectiblesPanel {
		const float Spacing = 13f;
		const float Margin = 20f;
		const float DividerGap = 5f;

		class Cell {
			public FSprite sprite;
			public Color color;
			public int column;
			public float row;

			// null for a token, which has no "with you" state of its own
			public DataPearl.AbstractDataPearl.DataPearlType pearlType;
			public Func<bool> collected;
			// pearls only: kept from the last progress refresh so the per-tick pass doesn't have
			// to ask again
			public bool read;
		}

		readonly List<Cell> cells = new();
		readonly List<FSprite> regionIcons = new();
		readonly HashSet<string> carriedPearls = new();
		readonly RainWorld rainWorld;
		int columnCount;

		public CollectiblesPanel(Map map) {
			rainWorld = map.hud.rainWorld;

			SlugcatStats.Name slugcat = rainWorld.progression.PlayingAsSlugcat;
			PlayerProgression.MiscProgressionData progress = rainWorld.progression.miscProgressionData;
			string currentRegion = map.mapData.regionName?.ToLowerInvariant();

			foreach (string region in VisitedStoryRegions(slugcat)) {
				int cellsBefore = cells.Count;
				float row = 0f;

				foreach (Cell token in TokensOfRegion(region, slugcat, progress)) {
					token.column = columnCount;
					token.row = row++;
					Add(map, token);
				}

				row += DividerGap / Spacing;

				if (rainWorld.regionDataPearls.TryGetValue(region, out var pearls)) {
					foreach (var pearlType in pearls) {
						Add(map, new Cell {
							column = columnCount,
							row = row++,
							color = Mod.GetPearlIconColor(pearlType),
							pearlType = pearlType
						});
					}
				}

				// a region with nothing to collect gets no column rather than a blank gap
				if (cells.Count > cellsBefore) {
					AddRegionIcon(map, region, region == currentRegion);
					columnCount++;
				}
			}

			RefreshProgress(null);
		}

		// Heading each column, the way the sleep screen does it: a dot in the region's own colour,
		// and for the region you're in an arrow pointing down at it instead.
		void AddRegionIcon(Map map, string region, bool current) {
			FSprite icon = current
				? new FSprite("keyShiftB") { rotation = 180f, scale = 0.5f }
				: new FSprite("Circle4");

			icon.color = Color.Lerp(Region.RegionColor(region), Color.white, 0.25f);
			icon.isVisible = false;
			map.inFrontContainer.AddChild(icon);
			regionIcons.Add(icon);
		}

		void Add(Map map, Cell cell) {
			cell.sprite = new FSprite("ctOff") {
				color = cell.color,
				isVisible = false
			};
			map.inFrontContainer.AddChild(cell.sprite);
			cells.Add(cell);
		}

		// A region only has a RegionState while it is loaded - the one you are standing in. Every
		// other region you have been to is still sitting in regionLoadStrings, saved but not
		// parsed, so looking at regionStates alone finds exactly one region and draws one column.
		// Vanilla reads both, and the region's name is the second field of the load string's first
		// chunk.
		List<string> VisitedStoryRegions(SlugcatStats.Name slugcat) {
			HashSet<string> visited = new(StringComparer.InvariantCultureIgnoreCase);
			SaveState saveState = rainWorld.progression.currentSaveState;

			if (saveState?.regionStates != null) {
				foreach (RegionState regionState in saveState.regionStates) {
					if (regionState?.regionName != null) {
						visited.Add(regionState.regionName);
					}
				}
			}

			if (saveState?.regionLoadStrings != null) {
				foreach (string loadString in saveState.regionLoadStrings) {
					if (string.IsNullOrEmpty(loadString)) {
						continue;
					}

					string[] fields = Regex.Split(Regex.Split(loadString, "<rgA>")[0], "<rgB>");
					if (fields.Length > 1 && !string.IsNullOrEmpty(fields[1])) {
						visited.Add(fields[1]);
					}
				}
			}

			List<string> regions = new();
			foreach (string region in SlugcatStats.SlugcatStoryRegions(slugcat)) {
				if (visited.Contains(region)) {
					regions.Add(region.ToLowerInvariant());
				}
			}

			return regions;
		}

		// Same tokens vanilla's own grid counts, filtered the same way: a token only belongs to a
		// campaign that can reach it.
		IEnumerable<Cell> TokensOfRegion(string region, SlugcatStats.Name slugcat, PlayerProgression.MiscProgressionData progress) {
			List<Cell> tokens = new();

			if (rainWorld.regionGoldTokens.TryGetValue(region, out var golds)) {
				for (int i = 0; i < golds.Count; i++) {
					if (!rainWorld.regionGoldTokensAccessibility[region][i].Contains(slugcat)) {
						continue;
					}
					var token = golds[i];
					tokens.Add(new Cell { color = RainWorld.GoldRGB, collected = () => progress.GetTokenCollected(token) });
				}
			}

			if (rainWorld.regionBlueTokens.TryGetValue(region, out var blues)) {
				for (int i = 0; i < blues.Count; i++) {
					if (!rainWorld.regionBlueTokensAccessibility[region][i].Contains(slugcat)) {
						continue;
					}
					var token = blues[i];
					tokens.Add(new Cell { color = RainWorld.AntiGold.rgb, collected = () => progress.GetTokenCollected(token) });
				}
			}

			if (ModManager.DLCShared && rainWorld.regionGreenTokens.TryGetValue(region, out var greens)) {
				for (int i = 0; i < greens.Count; i++) {
					if (!rainWorld.regionGreenTokensAccessibility[region][i].Contains(slugcat)) {
						continue;
					}
					var token = greens[i];
					tokens.Add(new Cell { color = CollectToken.GreenColor.rgb, collected = () => progress.GetTokenCollected(token) });
				}
			}

			if (ModManager.MSC) {
				if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear && rainWorld.regionGreyTokens.TryGetValue(region, out var greys)) {
					foreach (var token in greys) {
						tokens.Add(new Cell { color = CollectToken.WhiteColor.rgb, collected = () => progress.GetBroadcastListened(token) });
					}
				}

				if (rainWorld.regionRedTokens.TryGetValue(region, out var reds)) {
					for (int i = 0; i < reds.Count; i++) {
						if (!rainWorld.regionRedTokensAccessibility[region][i].Contains(slugcat)) {
							continue;
						}
						var token = reds[i];
						tokens.Add(new Cell { color = CollectToken.RedColor.rgb, collected = () => progress.GetTokenCollected(token) });
					}
				}
			}

			return tokens;
		}

		// What you are carrying, answered every tick: two hands and a stomach, so a pearl's ring
		// appears as you pick it up.
		public void RefreshCarried(Map map) {
			PearlsOnYou(map, carriedPearls);

			foreach (Cell cell in cells) {
				if (cell.pearlType != null) {
					SetElement(cell, PearlSymbols.GetElementName(cell.read, carriedPearls.Contains(cell.pearlType.value)));
				}
			}
		}

		// Whether a token is collected or a pearl read, answered on the map's periodic tick. This
		// half is the expensive one and the one that barely changes: IsPearlRead asks the file
		// system whether a conversation file exists on the Spearmaster, Artificer and Saint
		// campaigns, and doing that for every pearl of every region forty times a second is a lot
		// of asking for an answer that changes when an iterator reads a pearl.
		public void RefreshProgress(Map map) {
			foreach (Cell cell in cells) {
				if (cell.pearlType != null) {
					cell.read = Mod.IsPearlRead(rainWorld, cell.pearlType);
				} else {
					SetElement(cell, CollectibleSymbols.GetElementName(isPearl: false, collected: cell.collected()));
				}
			}

			RefreshCarried(map);
		}

		static void SetElement(Cell cell, string element) {
			if (cell.sprite.element?.name != element) {
				cell.sprite.element = Futile.atlasManager.GetElementWithName(element);
			}
		}

		// Fills the set it's given rather than returning a new one: this runs every tick, and the
		// answer is at most three pearls.
		static void PearlsOnYou(Map map, HashSet<string> onYou) {
			onYou.Clear();
			if (map?.hud?.owner is not Player player) {
				return;
			}

			AddIfPearl(onYou, player.objectInStomach);
			if (player.grasps != null) {
				foreach (Creature.Grasp grasp in player.grasps) {
					AddIfPearl(onYou, grasp?.grabbed?.abstractPhysicalObject);
				}
			}

			// A pearl lying on the shelter floor is in the shelter with you as much as one in your
			// hands, which is what the sleep screen counts - it reads the shelter's contents, not
			// only what you were holding. Shelters only: in any other room "in the room with you"
			// is not the same claim, and ringing a pearl merely because you walked past it would
			// leave the ring meaning nothing.
			AbstractRoom abstractRoom = player.abstractCreature?.Room;
			if (abstractRoom == null || !abstractRoom.shelter || abstractRoom.entities == null) {
				return;
			}

			foreach (AbstractWorldEntity entity in abstractRoom.entities) {
				AddIfPearl(onYou, entity as AbstractPhysicalObject);
			}
		}

		static void AddIfPearl(HashSet<string> onYou, AbstractPhysicalObject obj) {
			if (obj is DataPearl.AbstractDataPearl pearl
				&& pearl.dataPearlType != null
				&& DataPearl.PearlIsNotMisc(pearl.dataPearlType)
			) {
				onYou.Add(pearl.dataPearlType.value);
			}
		}

		public void Draw(Map map, float timeStacker, bool show) {
			float alpha = show ? Mathf.Lerp(map.lastFade, map.fade, timeStacker) : 0f;
			if (alpha <= 0f) {
				foreach (Cell cell in cells) {
					cell.sprite.isVisible = false;
				}
				foreach (FSprite icon in regionIcons) {
					icon.isVisible = false;
				}
				return;
			}

			Vector2 screenSize = rainWorld.options.ScreenSize;

			for (int column = 0; column < regionIcons.Count; column++) {
				regionIcons[column].isVisible = true;
				regionIcons[column].alpha = alpha;
				regionIcons[column].x = PanelLayout.ColumnX(screenSize.x, Margin, Spacing, columnCount, column);
				regionIcons[column].y = PanelLayout.IconY(screenSize.y, Margin);
			}

			foreach (Cell cell in cells) {
				cell.sprite.isVisible = true;
				cell.sprite.alpha = alpha;
				cell.sprite.x = PanelLayout.ColumnX(screenSize.x, Margin, Spacing, columnCount, cell.column);
				cell.sprite.y = PanelLayout.RowY(screenSize.y, Margin, Spacing, cell.row);
			}
		}

		public void Destroy() {
			foreach (Cell cell in cells) {
				cell.sprite.RemoveFromContainer();
			}
			cells.Clear();

			foreach (FSprite icon in regionIcons) {
				icon.RemoveFromContainer();
			}
			regionIcons.Clear();
		}
	}
}
