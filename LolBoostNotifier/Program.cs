using System;
using System.IO;
using IrcDotNet;

namespace LolBoostNotifier {
	class Program {
		private static IrcClient client;
		public static void Main (string[] args) {
			client = new IrcClient ();
			var connectInfo = IrcConnectInfo.FromFile ("ircinfo.txt");
			client.FloodPreventer = new IrcStandardFloodPreventer (4, 2000);
			client.Connected += HandleConnected;
			client.Registered += HandleRegistered;
			client.ConnectFailed += HandleConnectFailed;
			client.ProtocolError += HandleProtocolError;
			client.Connect (connectInfo.Hostname, connectInfo.Port, false, connectInfo.RegistrationInfo);
			while (Console.ReadLine () != "quit");
			client.Disconnect ();
		}

		static void HandleProtocolError (object sender, IrcProtocolErrorEventArgs e){
			Console.WriteLine (e.Message);
		}

		static void HandleConnectFailed (object sender, IrcErrorEventArgs e){
			Console.WriteLine (e.Error.ToString ());
		}

		static void HandleJoinedChannel (object sender, IrcChannelEventArgs e) {
			Console.WriteLine ("joined channel");
			var lolboost = new LolBoost ();
			if (lolboost.Login (Credentials.FromFile("creds.txt"))) {
				lolboost.OnNewBoost += delegate(object sender1, NewBoostEventArgs bargs) {
					client.LocalUser.SendMessage(e.Channel, bargs.Boost.ToString());
				};
				lolboost.Start ();
			}
		}

		static void HandleRegistered (object sender, EventArgs e) {
			Console.WriteLine ("registered");
			client.LocalUser.JoinedChannel += HandleJoinedChannel;
		}

		static void HandleConnected (object sender, EventArgs e) {
			Console.WriteLine ("connected");
			client.SendRawMessage ("join #boosts");
		}
	}
}
