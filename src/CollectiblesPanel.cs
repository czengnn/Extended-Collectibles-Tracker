using System;
using System.Collections.Generic;

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
		}

		readonly List<Cell> cells = new();
		readonly RainWorld rainWorld;
		int columnCount;

		public CollectiblesPanel(Map map) {
			rainWorld = map.hud.rainWorld;

			SlugcatStats.Name slugcat = rainWorld.progression.PlayingAsSlugcat;
			PlayerProgression.MiscProgressionData progress = rainWorld.progression.miscProgressionData;

			foreach (string region in VisitedStoryRegions(slugcat)) {
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

				if (row > 0f) {
					columnCount++;
				}
			}

			Refresh(null);
		}

		void Add(Map map, Cell cell) {
			cell.sprite = new FSprite("ctOff") {
				color = cell.color,
				isVisible = false
			};
			map.inFrontContainer.AddChild(cell.sprite);
			cells.Add(cell);
		}

		List<string> VisitedStoryRegions(SlugcatStats.Name slugcat) {
			List<string> visited = new();
			SaveState saveState = rainWorld.progression.currentSaveState;
			if (saveState?.regionStates == null) {
				return visited;
			}

			foreach (string region in SlugcatStats.SlugcatStoryRegions(slugcat)) {
				string lower = region.ToLowerInvariant();
				foreach (RegionState regionState in saveState.regionStates) {
					if (regionState != null && string.Equals(regionState.regionName, region, StringComparison.InvariantCultureIgnoreCase)) {
						visited.Add(lower);
						break;
					}
				}
			}

			return visited;
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

		// Reading progression means a file existence check on some campaigns, so this rides the
		// map's periodic tick rather than running every frame. What you are carrying is cheap
		// enough to answer here too - two hands and a stomach.
		public void Refresh(Map map) {
			HashSet<string> withYou = PearlsOnYou(map);

			foreach (Cell cell in cells) {
				if (cell.pearlType != null) {
					bool read = Mod.IsPearlRead(rainWorld, cell.pearlType);
					SetElement(cell, PearlSymbols.GetElementName(read, withYou.Contains(cell.pearlType.value)));
				} else {
					SetElement(cell, CollectibleSymbols.GetElementName(isPearl: false, collected: cell.collected()));
				}
			}
		}

		static void SetElement(Cell cell, string element) {
			if (cell.sprite.element?.name != element) {
				cell.sprite.element = Futile.atlasManager.GetElementWithName(element);
			}
		}

		static HashSet<string> PearlsOnYou(Map map) {
			HashSet<string> onYou = new();
			if (map?.hud?.owner is not Player player) {
				return onYou;
			}

			AddIfPearl(onYou, player.objectInStomach);
			if (player.grasps != null) {
				foreach (Creature.Grasp grasp in player.grasps) {
					AddIfPearl(onYou, grasp?.grabbed?.abstractPhysicalObject);
				}
			}

			return onYou;
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
				return;
			}

			Vector2 screenSize = rainWorld.options.ScreenSize;

			foreach (Cell cell in cells) {
				cell.sprite.isVisible = true;
				cell.sprite.alpha = alpha;
				cell.sprite.x = screenSize.x - Margin - (columnCount - 1 - cell.column) * Spacing;
				cell.sprite.y = screenSize.y - Margin - cell.row * Spacing;
			}
		}

		public void Destroy() {
			foreach (Cell cell in cells) {
				cell.sprite.RemoveFromContainer();
			}
			cells.Clear();
		}
	}
}
