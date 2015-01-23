using System;

namespace LolBoostNotifier {
	public enum DesireType {
		Games,
		Rank,
	}

	public abstract class Desire {
		public DesireType DesireType { get; set; }
		public abstract override string ToString ();
	}

	public class RankDesire : Desire {
		public Rank Rank { get; set; }
		public override string ToString () {
			return this.Rank.ToString ();
		}
	}

	public class GamesDesire : Desire {
		public int Games { get; set; }
		public override string ToString () {
			return string.Format ("Games: {0}", this.Games);
		}
	}
}

