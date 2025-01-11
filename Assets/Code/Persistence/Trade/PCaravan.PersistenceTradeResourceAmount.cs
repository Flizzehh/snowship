namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceTradeResourceAmount {

			public ResourceManager.ResourceEnum? type;
			public int? caravanAmount;
			public int? tradeAmount;
			public int? caravanPrice;

			public PersistenceTradeResourceAmount(
				ResourceManager.ResourceEnum? type,
				int? caravanAmount,
				int? tradeAmount,
				int? caravanPrice
			) {
				this.type = type;
				this.caravanAmount = caravanAmount;
				this.tradeAmount = tradeAmount;
				this.caravanPrice = caravanPrice;
			}

		}

	}
}
