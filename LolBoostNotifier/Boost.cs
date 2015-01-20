using System.ComponentModel;
using System.Collections.Generic;

namespace LolBoostNotifier {
	public enum BoostType {
		[Description("Placement Games")]
		PlacementGames,
		[Description("Guaranteed Division")]
		GuaranteedDivison,
		[Description("DuoQueue")]
		DuoQueue,
		[Description("DuoQueue Extended")]
		DuoQueueBoostExtended,
		[Description("Win")]
		Win,
		[Description("Unranked")]
		Unranked,
		[Description("Unknoqn")]
		Unknown

	}

	public class Boost {
		public Boost () {
		}

		public BoostType BoostType { get; set; }

		public static Boost Decode(List<string> rawOrder) {
			var boost = new Boost ();
			switch (rawOrder [0]) {
			case "PLACEMENT GAMES - 8/10 WINS MINIMUM!":
				boost.BoostType = BoostType.PlacementGames;
				break;
			case "DUOQUEUE EXTENDED":
				boost.BoostType = BoostType.DuoQueueBoostExtended;
				break;
			}
			return boost;
		}

		public override string ToString () {
			return string.Format ("[Boost: {0}]", BoostType.ToDescription());
		}
	}
}

