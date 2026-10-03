using System;
using System.Collections.Generic;
using System.IO;
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

		// Without the Mark the reader drops a pearl's dialogue, so a pearl with dialogue has
		// not been read; one of only images and sound has been, Mark or not.
		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, true)]
		[InlineData(false, false, true)]
		[InlineData(false, true, false)]
		public void CountsAsRead_WaitsForTheMarkOnlyWhenThereIsDialogue(bool canUnderstandDialog, bool hasDialog, bool expected) {
			Assert.Equal(expected, WatcherPearlReads.CountsAsRead(canUnderstandDialog, hasDialog));
		}

		[Theory]
		[InlineData("dialog:1,2,3")]
		[InlineData("convo:237")]
		[InlineData("conversation:5")]
		[InlineData("CONVO:237")]
		[InlineData("convo:237\nfadein:1")]
		[InlineData("convo:237\r\nfadein:1")]
		public void HasDialog_FindsTheDialogueElements(string block) {
			Assert.True(WatcherPearlReads.HasDialog(new[] { "1,1,1", "image:a", block }));
		}

		[Fact]
		public void HasDialog_IsFalseForImagesAndSound() {
			Assert.False(WatcherPearlReads.HasDialog(new[] { "1,1,1", "image:a", "sound:b\nfadein:1", "text:hello" }));
		}

		// The element type is a block's first line; a modifier line further down is not one.
		[Fact]
		public void HasDialog_OnlyReadsEachBlocksFirstLine() {
			Assert.False(WatcherPearlReads.HasDialog(new[] { "1,1,1", "image:a\ndialog:1" }));
		}

		[Fact]
		public void HasDialog_ToleratesEmptyAndMissingBlocks() {
			Assert.False(WatcherPearlReads.HasDialog(null));
			Assert.False(WatcherPearlReads.HasDialog(new[] { null, "", "\n" }));
		}

		// Checked against the real pearls when the game is installed here: the six text pearls
		// carry dialogue and every other Watcher pearl doesn't. Split the way
		// PearlContent.LoadPearlProperties splits a file. Passes trivially without the game.
		[Fact]
		public void HasDialog_MatchesTheInstalledWatcherPearls() {
			string dir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
				"Steam", "steamapps", "common", "Rain World", "RainWorld_Data", "StreamingAssets", "mods", "watcher", "pearls");
			if (!Directory.Exists(dir)) {
				return;
			}

			string[] withDialog = { "text_audio_talkshow", "text_contempt", "text_kitesday", "text_notionofself", "text_secret", "text_stardust" };
			foreach (string file in Directory.GetFiles(dir, "*.txt")) {
				string name = Path.GetFileNameWithoutExtension(file);
				string[] properties = File.ReadAllText(file)
					.Split(new[] { "\r\n|\r\n", "\r|\r", "\n|\n" }, StringSplitOptions.RemoveEmptyEntries)
					.Select(p => string.Join("", p.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)))
					.ToArray();

				Assert.True(withDialog.Contains(name) == WatcherPearlReads.HasDialog(properties), name);
			}
		}
	}
}
