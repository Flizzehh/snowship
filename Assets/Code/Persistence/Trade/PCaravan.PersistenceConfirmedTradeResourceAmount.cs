using Snowship.NResource;

namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceConfirmedTradeResourceAmount {
			public EResource? type;
			public int? tradeAmount;
			public int? amountRemaining;

			public PersistenceConfirmedTradeResourceAmount(
				EResource? type,
				int? tradeAmount,
				int? amountRemaining
			) {
				this.type = type;
				this.tradeAmount = tradeAmount;
				this.amountRemaining = amountRemaining;
			}

		}

	}
}
