namespace ExtendedCollectiblesTracker.Core {
	// Where the collection tracker's columns sit on screen. The sleep screen packs its columns to
	// the right and runs its dots downwards, with each region's icon a row above its column; this
	// keeps the map's copy to the same shape. See CollectiblesPanel.
	public static class PanelLayout {
		// Columns fill leftwards from the right edge, so the last one is always the same distance
		// in however many regions you have been to.
		public static float ColumnX(float screenWidth, float margin, float spacing, int columnCount, int column) {
			return screenWidth - margin - (columnCount - 1 - column) * spacing;
		}

		public static float IconY(float screenHeight, float margin) {
			return screenHeight - margin;
		}

		// Row 0 is the first dot, a row below the icon heading it.
		public static float RowY(float screenHeight, float margin, float spacing, float row) {
			return IconY(screenHeight, margin) - (row + 1f) * spacing;
		}
	}
}
