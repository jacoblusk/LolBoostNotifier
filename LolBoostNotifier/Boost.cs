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
		public string Name { get; set; }
		public string Region { get; set; }
		public float Amount { get; set; }

		public string[] UnparsedData { get; set; }

		public static Boost Decode(string[] rawOrder) {
			int counter = 0;
			var boost = new Boost ();
			switch (rawOrder[counter++].ToUpper()) {
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
				
			DateTime time;
			if (!DateTime.TryParseExact (rawOrder[counter++], 
				"dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out time)) {
				Console.Write ("unable to parse boost time!");
			}
			boost.PurchaseDate = time;

			boost.Name = rawOrder [counter++];
			boost.Region = rawOrder [counter++];

			for (int i = counter; i < rawOrder.Length; i++) {
				if (rawOrder [i] == "USD") {
					float amount;
					if (float.TryParse (rawOrder [i - 1], out amount)) {
						boost.Amount = amount;
						break;
					}
				}
			}

			boost.UnparsedData = new string[rawOrder.Length - counter + 1];
			if(rawOrder.Length != counter) {
				Array.Copy (rawOrder, 0, boost.UnparsedData, counter, rawOrder.Length - counter);
			}
			return boost;
		}

		public override string ToString () {
			return string.Format ("[{0}] {1} {2} USD:{3}", Region, this.BoostType.ToDescription(), Name, Amount);
		}

		public override bool Equals (object obj) {
			if (obj == null)
				return false;
			Boost boost = (Boost)obj;
			if (boost.UnparsedData.Length != this.UnparsedData.Length)
				return false;
			for (int i = 0; i < boost.UnparsedData.Length; i++) {
				if (boost.UnparsedData [i] != this.UnparsedData [i])
					return false;
			}
			if (boost.BoostType != this.BoostType)
				return false;
			if (boost.PurchaseDate != this.PurchaseDate)
				return false;
			return true;
		}

		public override int GetHashCode () {
			return ((int)UnparsedData.Length ^ (int)PurchaseDate.TimeOfDay.TotalSeconds);
		}
	}
}

