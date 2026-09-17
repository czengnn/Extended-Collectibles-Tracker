# Changelog

All notable changes to this mod are documented here. Versions correspond to the
`version` field in `mod/modinfo.json` and `Plugin.VERSION`.

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
