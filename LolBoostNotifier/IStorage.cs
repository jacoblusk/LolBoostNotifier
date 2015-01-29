using System;
using System.Collections.Generic;

namespace LolBoostNotifier {
	public interface IStorage {
		void Put(Boost boost);
		void PutBatch (List<Boost> boosts);
		void Delete (Boost boost);
		List<Boost> GetBoosts ();
	}
}

