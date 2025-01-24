using System.Collections.Generic;

namespace Snowship.NPersistence {
	public class PersistenceUniverse {
		public string path;

		public Dictionary<PUniverse.ConfigurationProperty, string> configurationProperties;
		public Dictionary<PUniverse.UniverseProperty, string> universeProperties;

		public PersistenceUniverse(string path) {
			this.path = path;

			configurationProperties = GameManager.Get<PersistenceManager>().PUniverse.LoadConfiguration(path + "/configuration.snowship");
			universeProperties = GameManager.Get<PersistenceManager>().PUniverse.LoadUniverse(path + "/universe.snowship");
		}
	}
}