using System;

namespace LolBoostNotifier {
	public class NewBoostEventArgs : EventArgs {
		public Boost Boost { get; set; }
		public NewBoostEventArgs (Boost boost) {
			Boost = boost;
		}
	}
}

