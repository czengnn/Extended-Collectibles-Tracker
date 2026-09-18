using Menu.Remix.MixedUI;
using UnityEngine;

namespace ExtendedCollectiblesTracker {
	class Options : OptionInterface {
		public static Options instance = new Options();

		public static Configurable<bool> showRoomGlow = instance.config.Bind("showRoomGlow", true, new ConfigurableInfo(
			"Show a subtle glow on the room where a collectible or pearl is located but not explored.",
			tags: "Show Room Glow"));

		public static Configurable<bool> showMapMarkers = instance.config.Bind("showMapMarkers", true, new ConfigurableInfo(
			"Show exact locations of collectibles or pearls if the area is explored.",
			tags: "Show Map Markers"));

		public static Configurable<bool> showRoomNames = instance.config.Bind("showRoomNames", false, new ConfigurableInfo(
			"Show each room's name on the map, just above every room the map is showing.",
			tags: "Show Room Names"));

		public static Configurable<bool> showProgressPanel = instance.config.Bind("showProgressPanel", false, new ConfigurableInfo(
			"Show how many tokens and pearls you have found in each region, in the top right while the map is open.",
			tags: "Region Progress"));

		public static Configurable<bool> instantMap = instance.config.Bind("instantMap", false, new ConfigurableInfo(
			"Open the map instantly with everything already explored shown, instead of waiting for it to fade in and reveal itself.",
			tags: "Instant Map"));

		public override void Initialize() {
			base.Initialize();

			Debug.Log("Initializing Config...");

			Tabs = new OpTab[]{ new OpTab(this, "Options") };

			Vector2 position = new Vector2(50, 600);

			position.y -= 40;
			OpCheckBox checkBox = new OpCheckBox(showRoomGlow, position) {description = showRoomGlow.info.description};
			OpLabel label = new OpLabel(position.x + 30, position.y + 3, showRoomGlow.info.Tags[0] as string) {description = showRoomGlow.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });

			position.y -= 40;
			checkBox = new OpCheckBox(showMapMarkers, position) {description = showMapMarkers.info.description};
			label = new OpLabel(position.x + 30, position.y + 3, showMapMarkers.info.Tags[0] as string) {description = showMapMarkers.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });

			position.y -= 40;
			checkBox = new OpCheckBox(showRoomNames, position) {description = showRoomNames.info.description};
			label = new OpLabel(position.x + 30, position.y + 3, showRoomNames.info.Tags[0] as string) {description = showRoomNames.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });

			position.y -= 40;
			checkBox = new OpCheckBox(showProgressPanel, position) {description = showProgressPanel.info.description};
			label = new OpLabel(position.x + 30, position.y + 3, showProgressPanel.info.Tags[0] as string) {description = showProgressPanel.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });

			position.y -= 40;
			checkBox = new OpCheckBox(instantMap, position) {description = instantMap.info.description};
			label = new OpLabel(position.x + 30, position.y + 3, instantMap.info.Tags[0] as string) {description = instantMap.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });
		}
	}
}