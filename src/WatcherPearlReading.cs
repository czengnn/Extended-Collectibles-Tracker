using Watcher;

using ExtendedCollectiblesTracker.Core;

namespace ExtendedCollectiblesTracker {
	// Records the pearls the Watcher reads at Ancient Urban's pearl reader, which the game itself
	// never does. See Core.WatcherPearlReads.
	internal static class WatcherPearlReading {
		public static bool IsRead(RainWorld rainWorld, DataPearl.AbstractDataPearl.DataPearlType pearlType) {
			return WatcherPearlReads.Contains(rainWorld.progression.miscProgressionData.unrecognizedSaveStrings, pearlType?.value);
		}

		public static bool IsWatcher(SlugcatStats.Name slugcat) {
			return slugcat != null && slugcat == WatcherEnums.SlugcatStatsName.Watcher;
		}

		// After LoadPearl, the reader holds the pearl and the content it is about to play.
		public static void PearlLoaded(PearlReader reader) {
			DataPearl.AbstractDataPearl.DataPearlType pearlType = reader.targetPearl?.AbstractPearl?.dataPearlType;
			if (pearlType == null || string.IsNullOrEmpty(pearlType.value) || !DataPearl.PearlIsNotMisc(pearlType)) {
				return;
			}

			RainWorldGame game = reader.room?.game;
			if (game == null || !game.IsStorySession || !IsWatcher(game.StoryCharacter)) {
				return;
			}

			bool canUnderstandDialog = reader.currentContent != null && reader.currentContent.CanUnderstandDialog();
			if (!WatcherPearlReads.CountsAsRead(canUnderstandDialog, HasDialog(pearlType))) {
				return;
			}

			PlayerProgression progression = game.rainWorld.progression;
			if (WatcherPearlReads.Add(progression.miscProgressionData.unrecognizedSaveStrings, pearlType.value)) {
				Mod.Logger.LogInfo($"[ExtendedCollectiblesTracker] Watcher read pearl {pearlType.value}");
				// What the game does when an iterator deciphers one, so the read survives a crash
				// or quitting before the next hibernation.
				progression.SaveProgression(saveMaps: false, saveMiscProg: true);
			}
		}

		// A pearl without its own content file plays something picked from a folder, which could
		// be anything, so it waits for the Mark like one known to have dialogue.
		static bool HasDialog(DataPearl.AbstractDataPearl.DataPearlType pearlType) {
			string path = PearlContent.ResolveFilePath(pearlType.value.ToLowerInvariant());
			if (path == null) {
				return true;
			}
			return WatcherPearlReads.HasDialog(PearlContent.LoadPearlProperties(path));
		}
	}
}
