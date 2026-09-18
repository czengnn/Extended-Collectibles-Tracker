# Extended Collectibles Tracker (iotrip fix)

Where the remaining stuff at?

A fork of [FranklyGD's Extended Collectibles Tracker](https://steamcommunity.com/sharedfiles/filedetails/?id=3244633122)
with the freezes and stale markers fixed. It installs under its own mod id, so it sits
alongside the original rather than replacing it — enable only one.

## Features

One Remix toggle each:

- **Show Map Markers** — every collectible's exact spot on the map, in its own colour.
- **Show Room Glow** — a glow over a room holding one, before you've explored it.
- **Show Room Names** — each room's name above its shape.
- **Show Collection Tracker** — the sleep screen's grid in the map's top right, a column
  per region.
- **Instant Map** — the map opens and closes with the button, no delay or fade.
- **Reveal Whole Rooms** — a room appears complete when you enter it, not just where you
  walked.

Always on: a dot per pearl per region on the sleep screen — filled once deciphered, ringed
while it's with you — and the region's progress on the fast travel screen.

## What this fork fixes

- Hibernation and death screens freezing, with the slugcat animation still playing — two
  separate causes.
- The picture freezing while the map is held, the game still running behind it. Vanilla's
  key item markers throw on an item they have no icon for, abandoning the rest of that
  frame's drawing — so "Slug Senses" and "Key Item Tracking" can both stay on.
- A pearl collecting a duplicate marker a few times a second, until the pile froze the map.
- Markers for pearls with no type, pointing at pearls that aren't there.
- Markers sitting where a pearl used to be, or a room away from where it is.
- Pearl and token markers going stale until the next hibernation.
- A pearl in your hands as you slept not counting as being in the shelter with you.
- Instant Map revealing itself gradually the first time it opened in a region.

See [CHANGELOG.md](CHANGELOG.md) for the full list.

## Steam Workshop

[![thumbnail](mod/thumbnail.png)](https://steamcommunity.com/sharedfiles/filedetails/?id=3802490629)
