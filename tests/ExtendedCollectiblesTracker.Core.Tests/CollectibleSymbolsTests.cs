using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class CollectibleSymbolsTests {
		[Theory]
		[InlineData(true, true, CollectibleSymbols.PearlCollected)]
		[InlineData(true, false, CollectibleSymbols.PearlUncollected)]
		[InlineData(false, true, CollectibleSymbols.TokenCollected)]
		[InlineData(false, false, CollectibleSymbols.TokenUncollected)]
		public void GetElementName_PicksCorrectAtlasElement(bool isPearl, bool collected, string expected) {
			Assert.Equal(expected, CollectibleSymbols.GetElementName(isPearl, collected));
		}
	}
}
