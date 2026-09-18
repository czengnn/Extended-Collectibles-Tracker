using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class PearlSymbolsTests {
		[Theory]
		[InlineData(false, false, PearlSymbols.Unread)]
		[InlineData(false, true, PearlSymbols.UnreadWithYou)]
		[InlineData(true, false, PearlSymbols.Read)]
		[InlineData(true, true, PearlSymbols.ReadWithYou)]
		public void GetElementName_PicksCorrectAtlasElement(bool read, bool withYou, string expected) {
			Assert.Equal(expected, PearlSymbols.GetElementName(read, withYou));
		}

		// Being read must not hide that the pearl is on you: the two facts are drawn by different
		// parts of the dot, so all four combinations have to be distinguishable.
		[Fact]
		public void EveryCombination_HasItsOwnElement() {
			string[] elements = {
				PearlSymbols.GetElementName(read: false, withYou: false),
				PearlSymbols.GetElementName(read: false, withYou: true),
				PearlSymbols.GetElementName(read: true, withYou: false),
				PearlSymbols.GetElementName(read: true, withYou: true)
			};

			Assert.Equal(elements.Length, new System.Collections.Generic.HashSet<string>(elements).Count);
		}
	}
}
