using System.Collections.Generic;

namespace Snowship.NPersistence {
	public class PersistenceUniverse {
		public string path;

		public Dictionary<PUniverse.ConfigurationProperty, string> configurationProperties;
		public Dictionary<PUniverse.UniverseProperty, string> universeProperties;

		public PersistenceUniverse(string path) {
			this.path = path;

			configurationProperties = GameManager.persistenceM.PUniverse.LoadConfiguration(path + "/configuration.snowship");
			universeProperties = GameManager.persistenceM.PUniverse.LoadUniverse(path + "/universe.snowship");
		}
	}
}
