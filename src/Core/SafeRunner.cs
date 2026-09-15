using System;

namespace ExtendedCollectiblesTracker.Core {
	// Pure, game-independent logic extracted for unit testing. See Mod.RunSafely: every
	// extension hook runs through this so an exception in our overlay code (map markers,
	// tracker icons) can never propagate into the game's own hook chain and freeze a screen.
	public static class SafeRunner {
		public static bool Run(Action action, Action<Exception> onError) {
			try {
				action();
				return true;
			} catch (Exception e) {
				onError?.Invoke(e);
				return false;
			}
		}
	}
}
