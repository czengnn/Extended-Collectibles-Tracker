# Changelog

All notable changes to this mod are documented here. Versions correspond to the
`version` field in `mod/modinfo.json` and `Plugin.VERSION`.

## [1.0.6]
### Fixed
- Fixed an intermittent freeze on the hibernation/sleep screen: a saved or
  pending object that failed to parse threw an unhandled exception while the
  screen's collectibles tracker was being built, leaving it unresponsive with
  no clickable elements.
- Fixed map markers (dots) not following a pearl once it's relocated — e.g.
  carried to and hibernated in a shelter — instead of staying stuck at the
  pearl's original room for the rest of the session.
- Fixed a pearl's map marker not switching to its "read" appearance if it was
  already being tracked before the pearl was deciphered.
- Fixed the pulsing "uncollected" marker color staying frozen mid-pulse when a
  pearl transitions to collected, instead of snapping to its solid tint.
- Added defensive bounds/null checks around save-state parsing (region
  lookups, tracked-object lists, pending-object lists) so similar bad or
  unexpected save data can't freeze the map, fast-travel, or hibernation
  screens the same way again.

## [1.0.5]
### Fixed
- Moved pearl location lookup to run after the map constructor (fixes #1).

## [1.0.4]
### Changed
- Made compatible with "The Watcher" DLC.

## [1.0.3]
### Added
- Initial release.
