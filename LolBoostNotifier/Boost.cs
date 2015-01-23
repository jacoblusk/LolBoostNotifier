using System;
using System.Globalization;
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
		[Description("Unknown")]
		Unknown

	}

	public class Boost {
		public Boost () {
		}

		public BoostType BoostType { get; set; }
		public DateTime PurchaseDate { get; set; }

		private List<string> rawData;

		public static Boost Decode(List<string> rawOrder) {
			var boost = new Boost ();
			var enumerator = rawOrder.GetEnumerator ();
			enumerator.MoveNext ();
			switch (enumerator.Current.ToUpper()) {
			case "PLACEMENT GAMES - 8/10 WINS MINIMUM!":
				boost.BoostType = BoostType.PlacementGames;
				break;
			case "DUOQUEUE EXTENDED":
				boost.BoostType = BoostType.DuoQueueBoostExtended;
				break;
			default:
				boost.BoostType = BoostType.Unknown;
				break;
			}

			enumerator.MoveNext ();
			DateTime time;
			if (!DateTime.TryParseExact (enumerator.Current, 
				"dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out time)) {
				Console.Write ("unable to parse boost time!");
			}
			boost.PurchaseDate = time;
			boost.rawData = rawOrder.GetRange (1, rawOrder.Count - 1);
			return boost;
		}

		public override string ToString () {
			string rawOutput = "";
			rawData.ForEach (s => rawOutput += s + ",");
			rawOutput = rawOutput.Remove (rawOutput.Length - 1);
			return string.Format ("[Boost: {0} @ {1}]", BoostType.ToDescription(), PurchaseDate.ToString(), rawOutput);
		}
	}
}

