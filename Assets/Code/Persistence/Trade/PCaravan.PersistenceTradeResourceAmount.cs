using Snowship.NResource;

namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceTradeResourceAmount {
			public EResource? type;
			public int? caravanAmount;
			public int? tradeAmount;
			public int? caravanPrice;

			public PersistenceTradeResourceAmount(
				EResource? type,
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
