using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

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

		private List<string> rawData;

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
			boost.rawData = rawOrder.GetRange (1, rawOrder.Count - 1);
			return boost;
		}

		public override string ToString () {
			string rawOutput = "";
			rawData.ForEach (s => rawOutput += "\n" + s);
			return string.Format ("[Boost: {0}]{1}", BoostType.ToDescription(), rawOutput);
		}
	}
}

