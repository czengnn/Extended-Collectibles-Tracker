# Extended Collectibles Tracker (iotrip fix)

Where the remaining stuff at?

A fork of [FranklyGD's Extended Collectibles Tracker](https://steamcommunity.com/sharedfiles/filedetails/?id=3244633122)
that fixes bugs in the original. It installs under its own mod id, so it sits
alongside the original rather than replacing it — enable only one.

### What this fork fixes

- **Hibernation and death screens no longer freeze.** Two separate causes: save
  data that failed to parse while the collectibles tracker was being built, and
  a pearl lookup that stored an object the game then crashed on every frame.
- **Pearl markers follow the pearl.** They now track a pearl carried to a
  shelter instead of pointing at where it was first found, and switch to their
  "read" look as soon as it's deciphered.
- **Token markers fill in when collected**, rather than only after the map is
  rebuilt on the next hibernation.

---

This mod allows you to locate any of the collectibles and display their current state of unlocked or not. Pearls are also included being tracked since their progress of being read are also persistent within the save.

Map shows locations of them when viewed. Exact locations if the area is explored, otherwise a faint glowing aura is shown over the room.

Sleep screen also has the status of pearls appended under the unlocks for each respective column. An additional status can be interpreted here if you have pearls with you in shelter, via color pulsation.

## Steam Workshop

[![thumbnail](mod/thumbnail.png)](https://steamcommunity.com/sharedfiles/filedetails/?id=3244633122)

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for release notes.