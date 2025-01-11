namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceConfirmedTradeResourceAmount {

			public ResourceManager.ResourceEnum? type;
			public int? tradeAmount;
			public int? amountRemaining;

			public PersistenceConfirmedTradeResourceAmount(
				ResourceManager.ResourceEnum? type,
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
