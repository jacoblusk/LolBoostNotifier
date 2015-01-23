using System.IO;
using IrcDotNet;

namespace LolBoostNotifier {
	public class IrcConnectInfo {
		private short port = 6667;

		public IrcUserRegistrationInfo RegistrationInfo { get; set; }
		public string Hostname { get; set; }
		public short Port {
			get { return port; }
			set { port = value; }
		}
		public IrcConnectInfo () {
		}
		public static IrcConnectInfo FromFile(string filename) {
			var info = new IrcConnectInfo ();
			info.RegistrationInfo = new IrcUserRegistrationInfo ();
			using (FileStream fs = File.Open (filename, FileMode.Open))
			using (var reader = new StreamReader (fs)) {
				short port;
				info.RegistrationInfo.UserName = reader.ReadLine ();
				info.RegistrationInfo.NickName = reader.ReadLine ();
				info.RegistrationInfo.RealName = reader.ReadLine ();
				info.Hostname = reader.ReadLine ();
				if (!reader.EndOfStream && short.TryParse (reader.ReadLine (), out port)) {
					info.Port = port;
				}

			}
			return info;
		}
	}
}

