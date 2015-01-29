using System;
using System.IO;
using IrcDotNet;

namespace LolBoostNotifier {
	class Program {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IrcClient client = new IrcClient ();
		private static readonly Db4oStorage storage = new Db4oStorage ("boosts.data");
		private static readonly LolBoost lolboost = new LolBoost (storage);
		public static void Main (string[] args) {
			var connectInfo = IrcConnectInfo.FromFile ("ircinfo.txt");
			client.FloodPreventer = new IrcStandardFloodPreventer (4, 2000);

			client.Connected += HandleConnected;
			client.Registered += HandleRegistered;
			client.ConnectFailed += HandleConnectFailed;
			client.ProtocolError += HandleProtocolError;

			client.Connect (connectInfo.Hostname, connectInfo.Port, false, connectInfo.RegistrationInfo);

			while (Console.ReadLine () != "quit");
			lolboost.Stop(true);
			client.Disconnect ();
			storage.Close();
		}

		static void HandleProtocolError (object sender, IrcProtocolErrorEventArgs e){
			Console.WriteLine (e.Message);
		}

		static void HandleConnectFailed (object sender, IrcErrorEventArgs e){
			Console.WriteLine (e.Error.ToString ());
		}

		static void HandleJoinedChannel (object sender, IrcChannelEventArgs e) {
			log.Info ("joined channel");
			if (lolboost.Login (Credentials.FromFile("creds.txt"))) {
				lolboost.OnNewBoost += delegate(object sender1, NewBoostEventArgs bargs) {
					if(bargs.Boost.Region == "NA")
						client.LocalUser.SendMessage(e.Channel, bargs.Boost.ToString());
				};
				lolboost.Start ();
			}
		}

		static void HandleRegistered (object sender, EventArgs e) {
			log.Info ("registered");
			client.LocalUser.JoinedChannel += HandleJoinedChannel;
		}

		static void HandleConnected (object sender, EventArgs e) {
			log.Info ("connected");
			client.SendRawMessage ("join #boosts");
		}
	}
}
