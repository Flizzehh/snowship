using System.Collections.Generic;

namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceTrader {

			public PLife.PersistenceLife persistenceLife;
			public PHuman.PersistenceHuman persistenceHuman;
			public TileManager.Tile leaveTile;
			public List<ResourceManager.TradingPost> tradingPosts;

			public PersistenceTrader(
				PLife.PersistenceLife persistenceLife,
				PHuman.PersistenceHuman persistenceHuman,
				TileManager.Tile leaveTile,
				List<ResourceManager.TradingPost> tradingPosts
			) {
				this.persistenceLife = persistenceLife;
				this.persistenceHuman = persistenceHuman;
				this.leaveTile = leaveTile;
				this.tradingPosts = tradingPosts;
			}

		}

	}
}
