using System;
using ExtendedCollectiblesTracker.Core;
using Xunit;

namespace ExtendedCollectiblesTracker.Core.Tests {
	public class SafeRunnerTests {
		[Fact]
		public void Run_ReturnsTrue_AndDoesNotInvokeOnError_WhenActionSucceeds() {
			bool onErrorCalled = false;

			bool result = SafeRunner.Run(() => { }, e => onErrorCalled = true);

			Assert.True(result);
			Assert.False(onErrorCalled);
		}

		[Fact]
		public void Run_CatchesException_AndInvokesOnError_InsteadOfPropagating() {
			Exception captured = null;
			var thrown = new InvalidOperationException("boom");

			bool result = SafeRunner.Run(() => throw thrown, e => captured = e);

			Assert.False(result);
			Assert.Same(thrown, captured);
		}

		[Fact]
		public void Run_DoesNotThrow_WhenOnErrorIsNull() {
			var thrown = new Exception("boom");

			var exception = Record.Exception(() => SafeRunner.Run(() => throw thrown, null));

			Assert.Null(exception);
		}
	}
}
