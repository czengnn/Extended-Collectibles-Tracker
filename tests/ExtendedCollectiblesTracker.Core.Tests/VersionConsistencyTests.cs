using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	// The release version is written in four places, and they have drifted apart before: the
	// csproj once claimed 2.0.8 while the mod shipped 1.0.6. Nothing reads the copy in the
	// description, so it is the easiest one to leave stale on the Workshop page. These read the
	// real files rather than a fixture, so forgetting one of them fails the build's test run.
	public class VersionConsistencyTests {
		static string RepoRoot() {
			DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
			while (directory != null &&
				!File.Exists(Path.Combine(directory.FullName, "ExtendedCollectiblesTracker.csproj"))
			) {
				directory = directory.Parent;
			}

			Assert.NotNull(directory);
			return directory.FullName;
		}

		static string Read(string relativePath) {
			return File.ReadAllText(Path.Combine(RepoRoot(), relativePath));
		}

		static string Match(string content, string pattern, string describedAs) {
			Match match = Regex.Match(content, pattern);
			Assert.True(match.Success, $"could not find {describedAs}");
			return match.Groups[1].Value;
		}

		static string ModInfoVersion() {
			return Match(Read(Path.Combine("mod", "modinfo.json")),
				"\"version\"\\s*:\\s*\"([^\"]+)\"", "the version field in mod/modinfo.json");
		}

		[Fact]
		public void PluginVersion_MatchesModInfo() {
			string pluginVersion = Match(Read(Path.Combine("src", "Plugin.cs")),
				"VERSION\\s*=\\s*\"([^\"]+)\"", "Plugin.VERSION");

			Assert.Equal(ModInfoVersion(), pluginVersion);
		}

		[Fact]
		public void CsprojVersion_MatchesModInfo() {
			string csprojVersion = Match(Read("ExtendedCollectiblesTracker.csproj"),
				"<Version>([^<]+)</Version>", "<Version> in the csproj");

			Assert.Equal(ModInfoVersion(), csprojVersion);
		}

		[Fact]
		public void VersionStatedInDescription_MatchesModInfo() {
			string statedVersion = Match(Read(Path.Combine("mod", "modinfo.json")),
				@"Version (\d+\.\d+\.\d+)", "the version stated in the modinfo description");

			Assert.Equal(ModInfoVersion(), statedVersion);
		}

		[Fact]
		public void Changelog_HasAnEntryForTheCurrentVersion() {
			string changelog = Read("CHANGELOG.md");

			Assert.Contains($"## [{ModInfoVersion()}]", changelog);
		}
	}
}
