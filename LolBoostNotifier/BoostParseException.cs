using System;

namespace LolBoostNotifier {
	public class BoostParseException : Exception {
		public BoostParseException () {
		}
		public BoostParseException(string message) : base(message) {
		}
		public BoostParseException(string message, Exception inner) : base(message, inner) {
		}
	}
}

