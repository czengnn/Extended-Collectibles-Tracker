using System;
using System.Collections.Generic;

namespace ExtendedCollectiblesTracker.Core {
	// Which pearls the Watcher has read, kept by this mod because the game keeps nothing. Every
	// other campaign has an iterator read the pearl and a deciphered list in the misc progression
	// data to show for it; the Watcher reads them at Ancient Urban's pearl reader, which plays the
	// pearl and records nothing, so without this a Watcher pearl is never read however often it
	// has been played.
	//
	// The list lives in the misc progression data's unrecognized save strings - the game writes
	// back any entry it doesn't know, so it is saved and loaded with the rest of the slot without
	// a hook, and uninstalling the mod leaves one line the game carries along and ignores.
	public static class WatcherPearlReads {
		// An entry is "KEY<mpdB>VALUE"; the game separates entries with <mpdA>.
		public const string SaveKey = "EXTCOLLTRACKER_WATCHERPEARLSREAD";
		const string KeySeparator = "<mpdB>";
		const char ValueSeparator = ',';

		public static bool Contains(IList<string> saveStrings, string pearlType) {
			return !string.IsNullOrEmpty(pearlType) && Read(saveStrings).Contains(pearlType);
		}

		// Records a read, returning whether it was new - the caller saves only then.
		public static bool Add(IList<string> saveStrings, string pearlType) {
			if (saveStrings == null || !CanStore(pearlType)) {
				return false;
			}

			List<string> pearls = Read(saveStrings);
			if (pearls.Contains(pearlType)) {
				return false;
			}
			pearls.Add(pearlType);

			string entry = SaveKey + KeySeparator + string.Join(ValueSeparator.ToString(), pearls);
			int index = IndexOfEntry(saveStrings);
			if (index >= 0) {
				saveStrings[index] = entry;
			} else {
				saveStrings.Add(entry);
			}
			return true;
		}

		static List<string> Read(IList<string> saveStrings) {
			var pearls = new List<string>();
			int index = IndexOfEntry(saveStrings);
			if (index < 0) {
				return pearls;
			}

			string value = saveStrings[index].Substring(SaveKey.Length + KeySeparator.Length);
			foreach (string pearl in value.Split(ValueSeparator)) {
				if (pearl.Length > 0 && !pearls.Contains(pearl)) {
					pearls.Add(pearl);
				}
			}
			return pearls;
		}

		static int IndexOfEntry(IList<string> saveStrings) {
			if (saveStrings == null) {
				return -1;
			}
			for (int i = 0; i < saveStrings.Count; i++) {
				if (saveStrings[i] != null && saveStrings[i].StartsWith(SaveKey + KeySeparator, StringComparison.Ordinal)) {
					return i;
				}
			}
			return -1;
		}

		// A name holding either separator would split into something else on the next load.
		static bool CanStore(string pearlType) {
			return !string.IsNullOrEmpty(pearlType)
				&& pearlType.IndexOf(ValueSeparator) < 0
				&& pearlType.IndexOf('<') < 0;
		}
	}
}
