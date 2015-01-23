using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace LolBoostNotifier {

	public enum League {
		[Description("Unknown")]
		Unknown,
		[Description("Bronze")]
		Bronze,
		[Description("Silver")]
		Silver,
		[Description("Gold")]
		Gold,
		[Description("Platinum")]
		Platinum,
		[Description("Diamond")]
		Diamond,
		[Description("Master")]
		Masters,
		[Description("Challenger")]
		Challenger
	}

	public enum Division {
		[Description("Bronze")]
		Unknown,
		[Description("V")]
		V,
		[Description("IV")]
		IV,
		[Description("III")]
		III,
		[Description("II")]
		II,
		[Description("I")]
		I,
	}

	public class Rank {
		private static readonly Regex RankRegex = new Regex (
			"([a-z]+)\\s?([IV]{1,3})?\\s?(?:LP:\\s?([0-9]{1,3})\\s?LPG:\\s?([0-9]{1,3}))?",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private int lp;
		private int lpg;

		public League League { get; set; }

		public Division Division { get; set; }

		public int LP { 
			get { return this.lp; }
			set {
				if (value > 100)
					this.lp = 100;
				else if (value < 0)
					this.lp = 0;
				else
					this.lp = value;
			}
		}

		public int LPG { 
			get { return this.lpg; }
			set {
				if (value > 100)
					this.lpg = 100;
				if (value < 0)
					this.lpg = 0;
			}
		}

		public static bool TryParse(string s, out Rank rank) {
			Match match = RankRegex.Match (s);
			rank = null;
			if (!match.Success)
				return false;
			rank = new Rank ();
			if (match.Groups.Count >= 2)
				rank.League = ParseLeague(match.Groups [1].Value);
			if (match.Groups.Count >= 3)
				rank.Division = ParseDivision(match.Groups [2].Value);
			if (match.Groups.Count == 5) {
				rank.LP = int.Parse (match.Groups [3].Value == "" ? "0" : match.Groups [3].Value);
				rank.LPG = int.Parse (match.Groups [4].Value == "" ? "0" : match.Groups [4].Value);
			}
			return true;
		}

		public static League ParseLeague(string s) {
			switch (s.ToLower ()) {
			case "bronze":
				return League.Bronze;
			case "silver":
				return League.Silver;
			case "gold":
				return League.Gold;
			case "platinum":
			case "plat":
				return League.Platinum;
			case "diamond":
				return League.Diamond;
			case "master":
				return League.Masters;
			case "challenger":
				return League.Challenger;
			default:
				return League.Unknown;
			}
		}

		public static Division ParseDivision(string s) {
			switch (s.ToLower ()) {
			case "i":
				return Division.I;
			case "ii":
				return Division.II;
			case "iii":
				return Division.III;
			case "iv":
				return Division.IV;
			case "v":
				return Division.V;
			default:
				return Division.Unknown;
			}
		}

		public override string ToString () {
			var sb = new StringBuilder ();
			sb.Append (this.League.ToDescription ());
			if (this.Division != Division.Unknown)
				sb.Append (string.Format (" {0}", this.Division.ToDescription ()));
			if(LPG > 0)
				sb.Append(string.Format(" LP: {0} LPG: {1}", LP, LPG));
			return sb.ToString ();
		}
	}
}

