using Menu.Remix.MixedUI;
using UnityEngine;

namespace ExtendedCollectiblesTracker {
	class Options : OptionInterface {
		public static Options instance = new Options();

		public static Configurable<bool> showRoomGlow = instance.config.Bind("showRoomGlow", true, new ConfigurableInfo(
			"Glow over a room holding a collectible, so an unexplored one still tells you something is there without " +
			"handing you the spot. It pins to the screen edge when the room is off the map view and fades with distance, " +
			"which makes it a rough compass.",
			tags: "Show Room Glow"));

		public static Configurable<bool> showMapMarkers = instance.config.Bind("showMapMarkers", true, new ConfigurableInfo(
			"Mark the exact spot of every collectible in a room the map has revealed, in its own colour. " +
			"A marker is empty and pulsing while something is left to do with it - collecting a token, " +
			"having a pearl read - and solid once there isn't.",
			tags: "Show Map Markers"));

		public static Configurable<bool> showRoomNames = instance.config.Bind("showRoomNames", false, new ConfigurableInfo(
			"Write each room's name just above its shape, on every room the map is showing, so you can tell where " +
			"you are without counting shapes. Labels fade with the layer their room is on, like the rest of the map.",
			tags: "Show Room Names"));

		public static Configurable<bool> showCollectionTracker = instance.config.Bind("showCollectionTracker", false, new ConfigurableInfo(
			"The sleep screen's collection grid, drawn in the top right of the map: a column per region you've visited, " +
			"a dot for each of its tokens and pearls. Filled means collected, or read by an iterator. " +
			"A ring around a pearl means it's on you right now - held, swallowed, or beside you in the shelter.",
			tags: "Show Collection Tracker"));

		public static Configurable<bool> instantMap = instance.config.Bind("instantMap", false, new ConfigurableInfo(
			"Open the map the moment the button goes down, already showing everywhere you have explored, " +
			"instead of waiting out the hold delay and watching it reveal itself. It closes just as sharply.",
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
			checkBox = new OpCheckBox(showCollectionTracker, position) {description = showCollectionTracker.info.description};
			label = new OpLabel(position.x + 30, position.y + 3, showCollectionTracker.info.Tags[0] as string) {description = showCollectionTracker.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });

			position.y -= 40;
			checkBox = new OpCheckBox(instantMap, position) {description = instantMap.info.description};
			label = new OpLabel(position.x + 30, position.y + 3, instantMap.info.Tags[0] as string) {description = instantMap.info.description};
			Tabs[0].AddItems(new UIelement[] { checkBox, label });
		}
	}
}