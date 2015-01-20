using System;

namespace LolBoostNotifier {
	class Program {
		public static void Main (string[] args) {
			var lolboost = new LolBoost ();
			if (lolboost.Login (Credentials.FromFile("creds.txt"))) {
				lolboost.OnNewBoost += delegate(object sender, NewBoostEventArgs bargs) {
					Console.WriteLine(bargs.Boost.ToString());
				};
				lolboost.Start ();
			}
		}
	}
}
