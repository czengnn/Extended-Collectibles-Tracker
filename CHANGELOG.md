# Changelog

All notable changes to this mod are documented here. Versions correspond to the
`version` field in `mod/modinfo.json`, the version stated in its description,
`Plugin.VERSION` and `<Version>` in the csproj; `VersionConsistencyTests` fails the
test run if any of them, or the newest heading below, falls out of step.

## [1.0.9]
### Fixed
- Fixed the screen appearing to freeze while the map is open, while the game
  carried on running behind it, so releasing the map jumped forward to a world
  that had moved on. Vanilla's key item markers only build their icon when the
  item has icon data, then draw it either way, so an item they can't draw threw
  a NullReferenceException out of `Map.Draw` and into `RainWorldGame.GrafUpdate`
  — abandoning the rest of the frame's drawing, every frame, while the map was
  up. Updating is a separate call, which is why only the picture stopped. Such
  an item is now skipped instead, so "Slug Senses" and "Key Item Tracking" can
  stay on. An item stored and returned by another mod can come back without the
  type its icon is chosen by, which is the same thing that was producing phantom
  pearl markers.

## [1.0.8]
### Fixed
- Fixed a pearl picking up a duplicate map marker every time its location was
  refreshed, so a single pearl could end up marked both where it was found and
  where it currently is, along with any number of copies stacked on top. The
  refresh matched pearls through a lookup table that one of its two callers never
  updated, so the pearl was never recognised again and another marker was appended
  a few times a second for as long as the map existed. The markers now carry which
  pearl they are, so there is nothing left to fall out of step. Sleeping cleared it
  temporarily, because a new cycle builds the map's data from scratch.
- Fixed the map freezing while held, and taking the rest of the game down with it,
  which those accumulating markers caused: the game walks every marker for each
  pixel of map it reveals.
- Fixed markers appearing for pearls that have no type, pointing at pearls that
  aren't there. A pearl can come back from a save string without its type — a mod
  storing and returning it can strip it — and with no identity it can't be told
  apart, coloured, or found again, so it is no longer tracked. Only "Misc" pearls
  were being skipped before.
- Fixed a carried pearl's marker sitting where the pearl was rather than where it
  is. Saved data only records where a pearl would respawn, so while the pearl is
  loaded in the world its actual position is used instead.

### Changed
- Room name labels now appear on every room the map is showing, rather than only
  rooms that have been entered. Rooms get revealed by being near them, so named
  rooms were being left blank.
- Room name labels now fade with the layer their room is on, like the rest of the
  map does, instead of every layer's labels being drawn at full strength on top of
  each other.

## [1.0.7]
### Added
- Optional room name labels on the map, off by default. Each room you've visited gets
  its name drawn just above its shape. Toggle it with "Show Room Names" in the Remix
  options.
- Optional instant map, off by default. The map opens as soon as the button is pressed,
  with everything already explored shown, instead of waiting out the hold delay and
  watching it reveal itself. Toggle it with "Instant Map" in the Remix options.

## [1.0.6]
### Fixed
- Fixed the hibernation and death screens freezing, with the slugcat animation
  still playing and no button responding. Two separate causes: a saved or
  pending object that failed to parse threw while the screen's collectibles
  tracker was being built, and pearl location lookup stored an object parsed
  with a null world back into the save state's object trackers, which the
  game's own map constructor then threw on every frame while retrying.
- Fixed map markers (dots) not following a pearl once it's relocated — e.g.
  carried to and hibernated in a shelter — instead of staying stuck at the
  pearl's original room for the rest of the session.
- Fixed a pearl's map marker not switching to its "read" appearance if it was
  already being tracked before the pearl was deciphered.
- Fixed the pulsing "uncollected" marker color staying frozen mid-pulse when a
  pearl transitions to collected, instead of snapping to its solid tint.
- Fixed token markers (arena/safari/master/broadcast diamonds) not filling in
  until the map was rebuilt on the next hibernation. They now refresh on the
  same periodic tick as pearl locations.
- Added defensive bounds/null checks around save-state parsing (region
  lookups, tracked-object lists, pending-object lists) so similar bad or
  unexpected save data can't freeze the map, fast-travel, or hibernation
  screens the same way again.

### Changed
- Renamed the mod to "Extended Collectibles Tracker (iotrip fix)" under its own
  id so this fork installs alongside the original instead of replacing it, and
  credited iotrip as an author.

## [1.0.5]
### Fixed
- Moved pearl location lookup to run after the map constructor (fixes #1).

## [1.0.4]
### Changed
- Made compatible with "The Watcher" DLC.

## [1.0.3]
### Added
- Initial release.
