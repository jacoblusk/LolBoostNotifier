using System;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace LolBoostNotifier {
	public enum BoostType {
		[Description("Placement Games")]
		PlacementGames,
		[Description("Guaranteed Division")]
		GuaranteedDivison,
		[Description("Unranked Division")]
		UnrankedDivisionBoost,
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

		private static readonly Regex LPRegex = new Regex ("LPG?:\\s?([0-9]{1,3})", RegexOptions.Compiled);
		private static readonly Regex PercentRegex = new Regex ("([0-9]{1,3})%", RegexOptions.Compiled);
		private int percent;

		public BoostType BoostType { get; set; }
		public DateTime PurchaseDate { get; set; }
		public string Name { get; set; }
		public string Region { get; set; }
		public float Amount { get; set; }
		public Rank Rank { get; set; }
		public int Points { get; set; }
		public int Percentage {
			get {
				return this.percent;
			}
			set {
				if (value > 100)
					this.percent = 100;
				else if (value < 0)
					this.percent = 0;
				else this.percent = value;
			}
		}
		public Desire Desire { get; set; }

		public string[] UnparsedData { get; set; }

		public static Boost Decode(string[] rawOrder) {
			int counter = 1;
			var boost = new Boost ();

			switch (rawOrder[counter++].ToUpper()) {
			case "PLACEMENT GAMES":
			case "PLACEMENT GAMES - 8/10 WINS MINIMUM!":
				boost.BoostType = BoostType.PlacementGames;
				break;
			case "DUOQUEUE EXTENDED":
				boost.BoostType = BoostType.DuoQueueBoostExtended;
				break;
			case "DUO QUEUE BOOSTING":
				boost.BoostType = BoostType.DuoQueue;
				break;
			case "DIVISION BOOSTING":
			case "GUARANTEED LEAGUE / DIVISION BOOSTING":
				boost.BoostType = BoostType.GuaranteedDivison;
				break;
			case "UNRANKED LEAGUE / DIVISION BOOSTING":
				boost.BoostType = BoostType.UnrankedDivisionBoost;
				break;
			case "WIN BOOSTING":
				boost.BoostType = BoostType.Win;
				break;
			default:
				throw new BoostParseException ("Unable to determine boost type.");
			}
				
			DateTime time;
			if (!DateTime.TryParseExact (rawOrder [counter++], "dd/MM/yyyy HH:mm:ss", 
				    CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out time))
				throw new BoostParseException ("Failed to parse boost purchase date.");
			boost.PurchaseDate = time;

			boost.Name = rawOrder [counter++];
			boost.Region = rawOrder [counter++];

			Rank rank;
			if (!Rank.TryParse (rawOrder [counter++], out rank))
				throw new BoostParseException ("Failed to parse boost rank.");
			boost.Rank = rank;

			Match lpMatch = LPRegex.Match (rawOrder [counter++]);
			if (lpMatch.Success)
				boost.Rank.LP = int.Parse (lpMatch.Groups [1].Value);

			Match lpgMatch = LPRegex.Match (rawOrder [counter++]);
			if (lpgMatch.Success)
				boost.Rank.LPG = int.Parse (lpgMatch.Groups [1].Value);
				
			switch (boost.BoostType) {
			case BoostType.UnrankedDivisionBoost:
			case BoostType.GuaranteedDivison:
				Rank desiredRank;
				var rankDesire = new RankDesire ();
				rankDesire.DesireType = DesireType.Rank;
				if (Rank.TryParse (rawOrder [counter++], out desiredRank))
					rankDesire.Rank = desiredRank;
				else
					throw new BoostParseException ("Unable to parse GuaranteedDivision boost rank.");
				boost.Desire = rankDesire;
				break;
			default:
				int desiredGames;
				var gameDesire = new GamesDesire ();
				gameDesire.DesireType = DesireType.Games;
				if (int.TryParse (rawOrder [counter++], out desiredGames))
					gameDesire.Games = desiredGames;
				else
					throw new BoostParseException ("Unable to parse boost desired games.");
				boost.Desire = gameDesire;
				counter++;
				break;
			}

			int points;
			if (int.TryParse (rawOrder [counter++], out points))
				boost.Points = points;
			else
				throw new BoostParseException ("Unable to parse boost points.");

			Match percentMatch = PercentRegex.Match (rawOrder [counter++]);
			if (percentMatch.Success)
				boost.Percentage = int.Parse (percentMatch.Groups [1].Value);
			else
				throw new BoostParseException ("Unable to parse boost percentage.");

			float amount;
			if (float.TryParse (rawOrder [counter++], out amount))
				boost.Amount = amount;
			else
				throw new BoostParseException ("Unable to parse boost amount.");

			boost.UnparsedData = new string[rawOrder.Length - counter + 1];
			if(rawOrder.Length != counter) {
				Array.Copy (rawOrder, counter, boost.UnparsedData, 0, rawOrder.Length - counter);
			}
			return boost;
		}


		public override string ToString () {
			return string.Format ("[{0}] {1} : {2} @ [{3} -> {4}] USD: {5}$", Region, this.BoostType.ToDescription(), Name, Rank, Desire, Amount);
		}

		public override bool Equals (object obj) {
			if (obj == null)
				return false;
			Boost boost = (Boost)obj;
			if (boost.BoostType != this.BoostType ||
				boost.PurchaseDate != this.PurchaseDate ||
				boost.UnparsedData.Length != this.UnparsedData.Length)
				return false;
			for (int i = 0; i < boost.UnparsedData.Length; i++) {
				if (boost.UnparsedData [i] != this.UnparsedData [i])
					return false;
			}
			return true;
		}

		public override int GetHashCode () {
			return (UnparsedData.Length ^ (int)PurchaseDate.TimeOfDay.TotalSeconds);
		}
	}
}

