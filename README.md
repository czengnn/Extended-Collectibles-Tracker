# Extended Collectibles Tracker (iotrip fix)

Where the remaining stuff at?

A fork of [FranklyGD's Extended Collectibles Tracker](https://steamcommunity.com/sharedfiles/filedetails/?id=3244633122)
with the freezes and stale markers fixed. It installs under its own mod id, so it sits
alongside the original rather than replacing it — enable only one.

## Features

- **On the map** — a marker per collectible, each in its own colour: data pearls, blue and
  gold arena tokens, red safari, green slugcat, white broadcast (Spearmaster only).
  - **Empty and pulsing** while something's left to do, **solid** once it isn't.
  - **Unexplored rooms** get a glow instead of a spot, pinned to the screen edge when
    off-screen and fading with distance — a rough compass.
- **On the sleep screen** — a dot per pearl, per region visited. **Filled** means
  deciphered; a **ring** means it's with you, held, swallowed or on the shelter floor,
  read or not.
- **On the fast travel screen** — the region's token and pearl progress at a glance.
- **Collection tracker on the map** — optional; that same grid in the top right while you
  hold the map. It reads your hands and stomach live, so a ring appears the moment you
  pick a pearl up.
- **Room names** — optional labels above each room the map is showing.
- **Instant map** — optional; opens and closes with the button, everything you've explored
  already shown.
- **Reveal whole rooms** — optional; a room appears complete the moment you enter it,
  rather than uncovering only where you walked.
- **In Remix options** — six separate toggles: **Show Map Markers**, **Show Room Glow**,
  **Show Room Names**, **Show Collection Tracker**, **Instant Map** and **Reveal Whole
  Rooms**.

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
