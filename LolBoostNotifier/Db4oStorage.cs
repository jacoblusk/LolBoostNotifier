using System;
using System.Collections.Generic;
using Db4objects.Db4o;

namespace LolBoostNotifier {
	public class Db4oStorage : IStorage {
		private readonly IObjectContainer db;
		public Db4oStorage (string filename) {
			db = Db4oEmbedded.OpenFile (filename);
		}
		public void Put(Boost boost) {
			db.Store (boost);
		}
		public void PutBatch (List<Boost> boosts) {
			foreach (Boost b in boosts) {
				db.Store (b);
			}
		}
		public List<Boost> GetBoosts () {
			List<Boost> boosts = new List<Boost> ();
			IObjectSet results = db.QueryByExample (typeof(Boost));
			foreach (object o in results) {
				boosts.Add ((Boost)o);
			}
			return boosts;
		}
		public void Delete(Boost boost) {
			db.Delete (boost);
		}
		public void Close() {
			db.Close ();
		}
	}
}

