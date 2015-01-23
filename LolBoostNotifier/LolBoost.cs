using System.Net;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using System.Linq;
using System.Threading;
using System.IO;

namespace LolBoostNotifier {
	public class LolBoost {
		private const string Host = "https://lolboost.net";
		private const string LoginLocation = "Login.aspx";
		private const string DashboardLocation = "Dashboard.aspx";
		private const string Domain = ".lolboost.net";
		private const int WaitTime = 5000;
		private const int MaxLoginAttempts = 5;
		private static readonly string OrdersUrl = string.Join("/", Host, "AccountServicer/AvailableOrders.aspx");
		private static readonly string LoginUrl = string.Join("/", Host, LoginLocation);
		private static readonly Regex DdosGuardCookieRegex = new Regex("(_ddn_intercept_2_)=([a-z0-9]+);", RegexOptions.Compiled);

		private string username;
		private string password;

		private bool loggedIn = false;
		private int loginAttempts;
		private Cookie sessionCookie;
		private Cookie ddosGuardCookie;

		private bool catchBoosts;
		private Thread catchingThread;

		private List<Boost> oldBoosts = new List<Boost>();

		public delegate void NewBoostHandler(object sender, NewBoostEventArgs args);
		public event NewBoostHandler OnNewBoost;

		public LolBoost () { 
			catchingThread = new Thread (new ThreadStart (CatchBoosts));
		}

		public bool AcceptAllCerts(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) {
			return true;
		}

		private Cookie DdosGuardBypass(string url) {
			var web = new HtmlWeb ();
			HtmlDocument doc;
			web.UseCookies = true;

			web.PreRequest += delegate(HttpWebRequest request) {
				request.ServerCertificateValidationCallback = AcceptAllCerts;
				return true;
			};

			try {
				doc = web.Load (url, "GET");
			} catch(WebException) {
				return null;
			}

			if (doc == null)
				return null;

			HtmlNode script = doc.DocumentNode.SelectSingleNode ("//script");
			Match m = DdosGuardCookieRegex.Match (script.InnerText);
			if (!m.Success)
				return null;

			if (m.Groups.Count < 3)
				return null;

			string name = m.Groups[1].Value;
			string value = m.Groups[2].Value;
			var cookie = new Cookie (name, value, "/");
			cookie.Domain = Domain;
			return cookie;
		}

		private string GenerateLoginPostBody(HtmlDocument doc) {
			string viewState;
			string viewStateGenerator;
			string eventValidation;

			try {
				viewState = doc.GetElementbyId ("__VIEWSTATE").Attributes ["value"].Value;
				viewStateGenerator = doc.GetElementbyId ("__VIEWSTATEGENERATOR").Attributes ["value"].Value;
				eventValidation = doc.GetElementbyId ("__EVENTVALIDATION").Attributes ["value"].Value;
			} catch {
				return string.Empty;
			}

			NameValueCollection postBody = HttpUtility.ParseQueryString (string.Empty);
			postBody.Add ("__EVENTTARGET", "ctl00$cphBody$hfLogin");
			postBody.Add ("__VIEWSTATE", viewState);
			postBody.Add ("__VIEWSTATEGENERATOR", viewStateGenerator);
			postBody.Add ("__EVENTVALIDATION", eventValidation);
			postBody.Add ("ctl00$cphBody$txtUsername", username);
			postBody.Add ("ctl00$cphBody$txtPassword", password);
			postBody.Add ("ctl00$ucLatestNews1$hfCount", "3");
			postBody.Add ("ctl00$ucLatestGuids1$hfCount", "3");
			postBody.Add ("ctl00$ucLastestOffers$hfCount", "3");
			return postBody.ToString ();
		}

		public bool Login(Credentials creds) {
			this.username = creds.Username;
			this.password = creds.Password;
			return Login ();
		}

		public bool Login(string username, string password) {
			this.username = username;
			this.password = password;
			return Login ();
		}

		private bool Login() {
			ddosGuardCookie = DdosGuardBypass (LoginUrl);
			if (ddosGuardCookie == null)
				return false;

			var web = new HtmlWeb ();
			HtmlDocument doc;
			web.UseCookies = true;
			web.PreRequest += delegate(HttpWebRequest request) {
				request.ServerCertificateValidationCallback = AcceptAllCerts;
				request.CookieContainer.Add(ddosGuardCookie);
				return true;
			};

			try {
				doc = web.Load (LoginUrl, "GET");
			} catch(WebException) {
				return false;
			}

			string postBody = GenerateLoginPostBody (doc);

			web.PreRequest += delegate(HttpWebRequest request) {
				request.ContentType = "application/x-www-form-urlencoded";
				request.AllowAutoRedirect = false;
				using (Stream requestStream = request.GetRequestStream ())
				using (var writer = new StreamWriter (requestStream)) {
					writer.Write (postBody);
				}
				return true;
			};

			web.PostResponse += delegate(HttpWebRequest request, HttpWebResponse response) {
				sessionCookie = response.Cookies["ASP.NET_SessionId"];
				string redirectHeader = response.Headers.Get("Location");
				if(redirectHeader != null && sessionCookie != null) {
					loggedIn = true;
				}
			};

			try{
				web.Load (LoginUrl, "POST");
			} catch(WebException) {
				return false;
			}

			return loggedIn;
		}

		public List<Boost> GetBoosts() {
			var web = new HtmlWeb ();
			web.UseCookies = true;
			HtmlDocument doc;
			web.PreRequest += delegate(HttpWebRequest request) {
				request.ServerCertificateValidationCallback += AcceptAllCerts;
				request.CookieContainer.Add (sessionCookie);
				request.CookieContainer.Add (ddosGuardCookie);
				return true;
			};

			try {
				doc = web.Load (OrdersUrl, "GET");
			} catch(WebException) {
				return null;
			}

			HtmlNode tableOrders;
			HtmlNode tableBody;

			try {
				tableOrders = doc.GetElementbyId ("tblOrders");
				tableBody = tableOrders.Element ("tbody");
			} catch {
				return null;
			}

			IEnumerable<HtmlNode> tableRows = tableBody.Elements ("tr");
			List<Boost> boosts = new List<Boost> ();
			foreach (HtmlNode n in tableRows) {
				IEnumerable<HtmlNode> tableCells = n.Elements ("td");
				var rawOrder = new List<string> ();
				foreach (HtmlNode m in tableCells) {
					rawOrder.AddRange (m.InnerText
						.Split ('\n')
						.Select (s => s.Trim ())
						.Where (s => !string.IsNullOrEmpty (s)));
				}
				Boost boost = Boost.Decode (rawOrder);
				boosts.Add (boost);
			}

			return boosts;
		}

		private void CatchBoosts() {
			while (catchBoosts) {
				List<Boost> boosts = GetBoosts ();
				if (boosts == null) {
					if (loginAttempts++ > MaxLoginAttempts) {
						//TODO: Add logging here!
						catchBoosts = false;
						break;
					}
					Thread.Sleep (WaitTime);
					Login ();
					continue;
				}
				loginAttempts = 0;
				lock (oldBoosts) {
					IEnumerable<Boost> newBoosts = boosts.Where (b => !oldBoosts.Contains (b));
					foreach (Boost b in newBoosts) {
						if (OnNewBoost != null) {
							OnNewBoost (this, new NewBoostEventArgs (b));
							oldBoosts.Add (b);
						}
					}
				}
				//Prevents us from being locked out by DDOSGuard
				Thread.Sleep (WaitTime);
			}
		}

		public void Start() {
			catchBoosts = true;
			catchingThread.Start ();
		}

		public void Stop() {
			catchBoosts = false;
		}

		public void Stop(bool force) {
			if (force) {
				Stop ();
				catchingThread.Abort ();
			} else
				Stop ();
		}
	}
}

