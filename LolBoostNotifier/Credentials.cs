using System.IO;

namespace LolBoostNotifier {
	public class Credentials {
		public string Username { get; set; }
		public string Password { get; set; }

		public Credentials () {
		}

		public static Credentials FromFile(string filename) {
			var creds = new Credentials ();
			using (FileStream fs = File.Open (filename, FileMode.Open))
			using (var reader = new StreamReader (fs)) {
				creds.Username = reader.ReadLine ();
				creds.Password = reader.ReadLine ();
			}
			return creds;
		}
	}
}

