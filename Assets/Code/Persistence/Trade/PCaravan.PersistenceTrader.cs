using System.Collections.Generic;
using Snowship.NResource;

namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceTrader {

			public PLife.PersistenceLife persistenceLife;
			public PHuman.PersistenceHuman persistenceHuman;
			public TileManager.Tile leaveTile;
			public List<TradingPost> tradingPosts;

			public PersistenceTrader(
				PLife.PersistenceLife persistenceLife,
				PHuman.PersistenceHuman persistenceHuman,
				TileManager.Tile leaveTile,
				List<TradingPost> tradingPosts
			) {
				this.persistenceLife = persistenceLife;
				this.persistenceHuman = persistenceHuman;
				this.leaveTile = leaveTile;
				this.tradingPosts = tradingPosts;
			}

		}

	}
}
