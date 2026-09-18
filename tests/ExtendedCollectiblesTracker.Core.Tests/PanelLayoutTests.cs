using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class PanelLayoutTests {
		const float ScreenWidth = 1366f;
		const float ScreenHeight = 768f;
		const float Margin = 20f;
		const float Spacing = 13f;

		[Fact]
		public void TheLastColumnSitsAtTheMargin() {
			Assert.Equal(ScreenWidth - Margin, PanelLayout.ColumnX(ScreenWidth, Margin, Spacing, columnCount: 5, column: 4));
		}

		// however many regions there are, the block ends in the same place and grows leftwards
		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(12)]
		public void ColumnsGrowLeftwardsFromTheRightEdge(int columnCount) {
			float last = PanelLayout.ColumnX(ScreenWidth, Margin, Spacing, columnCount, columnCount - 1);
			float first = PanelLayout.ColumnX(ScreenWidth, Margin, Spacing, columnCount, 0);

			Assert.Equal(ScreenWidth - Margin, last);
			Assert.Equal(last - (columnCount - 1) * Spacing, first);
		}

		[Fact]
		public void RowsRunDownwardsFromUnderTheIcon() {
			float icon = PanelLayout.IconY(ScreenHeight, Margin);
			float firstRow = PanelLayout.RowY(ScreenHeight, Margin, Spacing, 0f);
			float secondRow = PanelLayout.RowY(ScreenHeight, Margin, Spacing, 1f);

			Assert.Equal(ScreenHeight - Margin, icon);
			Assert.Equal(icon - Spacing, firstRow);
			Assert.Equal(firstRow - Spacing, secondRow);
		}

		// the divider between a region's tokens and its pearls is a fraction of a row, so rows
		// have to take a float rather than an index
		[Fact]
		public void AFractionalRowLandsBetweenTwoWholeOnes() {
			float half = PanelLayout.RowY(ScreenHeight, Margin, Spacing, 1.5f);

			Assert.True(half < PanelLayout.RowY(ScreenHeight, Margin, Spacing, 1f));
			Assert.True(half > PanelLayout.RowY(ScreenHeight, Margin, Spacing, 2f));
		}
	}
}
