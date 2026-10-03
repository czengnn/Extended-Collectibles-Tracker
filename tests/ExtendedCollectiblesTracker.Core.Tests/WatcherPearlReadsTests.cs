using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class WatcherPearlReadsTests {
		[Fact]
		public void NothingIsRead_OnASaveWithoutTheEntry() {
			var saveStrings = new List<string> { "SOMEOTHERMOD<mpdB>1" };

			Assert.False(WatcherPearlReads.Contains(saveStrings, "TEXT_SECRET"));
		}

		[Fact]
		public void Add_RecordsARead_AndReportsItNew() {
			var saveStrings = new List<string>();

			Assert.True(WatcherPearlReads.Add(saveStrings, "TEXT_SECRET"));
			Assert.True(WatcherPearlReads.Contains(saveStrings, "TEXT_SECRET"));
			Assert.False(WatcherPearlReads.Contains(saveStrings, "WORA"));
		}

		// The caller saves the slot only on a new read, so a pearl played again must not count.
		[Fact]
		public void Add_ReportsARepeatReadAsNotNew() {
			var saveStrings = new List<string>();
			WatcherPearlReads.Add(saveStrings, "WORA");

			Assert.False(WatcherPearlReads.Add(saveStrings, "WORA"));
			Assert.Single(saveStrings);
		}

		[Fact]
		public void Add_KeepsEveryPearlInOneEntry() {
			var saveStrings = new List<string>();
			WatcherPearlReads.Add(saveStrings, "WORA");
			WatcherPearlReads.Add(saveStrings, "WAUA");
			WatcherPearlReads.Add(saveStrings, "DRONE");

			Assert.Single(saveStrings);
			Assert.True(WatcherPearlReads.Contains(saveStrings, "WORA"));
			Assert.True(WatcherPearlReads.Contains(saveStrings, "WAUA"));
			Assert.True(WatcherPearlReads.Contains(saveStrings, "DRONE"));
		}

		// Other mods keep their own lines in the same list; ours must leave them where they are.
		[Fact]
		public void Add_LeavesOtherEntriesAlone() {
			var saveStrings = new List<string> { "OTHER<mpdB>a", "ANOTHER<mpdB>b" };
			WatcherPearlReads.Add(saveStrings, "WORA");
			WatcherPearlReads.Add(saveStrings, "WAUA");

			Assert.Equal(3, saveStrings.Count);
			Assert.Equal("OTHER<mpdB>a", saveStrings[0]);
			Assert.Equal("ANOTHER<mpdB>b", saveStrings[1]);
		}

		[Fact]
		public void Contains_MatchesWholeNamesOnly() {
			var saveStrings = new List<string>();
			WatcherPearlReads.Add(saveStrings, "AUDIO_JAM1");

			Assert.False(WatcherPearlReads.Contains(saveStrings, "AUDIO_JAM"));
			Assert.False(WatcherPearlReads.Contains(saveStrings, "audio_jam1"));
		}

		// A key that merely starts with ours is some other entry.
		[Fact]
		public void Contains_IgnoresAKeyThatOnlyStartsWithOurs() {
			var saveStrings = new List<string> { WatcherPearlReads.SaveKey + "_OLD<mpdB>WORA" };

			Assert.False(WatcherPearlReads.Contains(saveStrings, "WORA"));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("A,B")]
		[InlineData("A<mpdA>B")]
		public void Add_RefusesANameThatWouldNotSurviveTheSave(string pearlType) {
			var saveStrings = new List<string>();

			Assert.False(WatcherPearlReads.Add(saveStrings, pearlType));
			Assert.Empty(saveStrings);
		}

		[Fact]
		public void NullSaveStrings_AreTreatedAsEmpty() {
			Assert.False(WatcherPearlReads.Contains(null, "WORA"));
			Assert.False(WatcherPearlReads.Add(null, "WORA"));
		}

		// The game writes each unrecognized string followed by <mpdA> (MiscProgressionData.ToString)
		// and on load splits on <mpdA> and <mpdB>, keeping any entry whose key it doesn't know
		// (FromString's default case). Mirrored here so a read survives a save and a load.
		[Fact]
		public void Reads_SurviveTheGamesSaveAndLoad() {
			var saveStrings = new List<string> { "OTHER<mpdB>x" };
			WatcherPearlReads.Add(saveStrings, "TEXT_SECRET");
			WatcherPearlReads.Add(saveStrings, "AUDIO_GROOVE");

			List<string> loaded = GameLoad(GameSave(saveStrings));

			Assert.True(WatcherPearlReads.Contains(loaded, "TEXT_SECRET"));
			Assert.True(WatcherPearlReads.Contains(loaded, "AUDIO_GROOVE"));
			Assert.Contains("OTHER<mpdB>x", loaded);
		}

		static string GameSave(List<string> unrecognizedSaveStrings) {
			string text = "CURRENTSLUGCAT<mpdB>Watcher<mpdA>";
			foreach (string s in unrecognizedSaveStrings) {
				text = text + s + "<mpdA>";
			}
			return text;
		}

		static List<string> GameLoad(string s) {
			var unrecognized = new List<string>();
			string[] entries = Regex.Split(s, "<mpdA>");
			foreach (string entry in entries) {
				string[] parts = Regex.Split(entry, "<mpdB>");
				if (parts[0] == "CURRENTSLUGCAT") {
					continue;
				}
				if (entry.Trim().Length > 0 && parts.Length >= 1) {
					unrecognized.Add(entry);
				}
			}
			return unrecognized;
		}

		// Every pearl the Watcher can read, as WatcherEnums.DataPearlType registers them. Reported
		// in 1.0.13: the pearls with dialogue never filled in, because a read only counted once the
		// Watcher had the Mark. All of them have to be recordable, the six text pearls included.
		static readonly string[] WatcherPearls = {
			"WORA", "WAUA", "DRONE", "ABSTRACT",
			"AUDIO_VOICEWIND1", "AUDIO_VOICEWIND2", "AUDIO_JAM1", "AUDIO_JAM2", "AUDIO_JAM3", "AUDIO_JAM4", "AUDIO_GROOVE",
			"TEXT_AUDIO_TALKSHOW", "TEXT_NOTIONOFSELF", "TEXT_SECRET", "TEXT_CONTEMPT", "TEXT_STARDUST", "TEXT_KITESDAY"
		};

		public static IEnumerable<object[]> EachWatcherPearl() {
			return WatcherPearls.Select(pearl => new object[] { pearl });
		}

		[Theory]
		[MemberData(nameof(EachWatcherPearl))]
		public void EveryWatcherPearl_CanBeRecordedAsRead(string pearlType) {
			var saveStrings = new List<string>();

			Assert.True(WatcherPearlReads.Add(saveStrings, pearlType));
			Assert.True(WatcherPearlReads.Contains(saveStrings, pearlType));
		}

		[Fact]
		public void EveryWatcherPearl_ReadTogether_SurvivesTheGamesSaveAndLoad() {
			var saveStrings = new List<string>();
			foreach (string pearl in WatcherPearls) {
				WatcherPearlReads.Add(saveStrings, pearl);
			}

			List<string> loaded = GameLoad(GameSave(saveStrings));

			foreach (string pearl in WatcherPearls) {
				Assert.True(WatcherPearlReads.Contains(loaded, pearl), pearl);
			}
		}
	}
}
