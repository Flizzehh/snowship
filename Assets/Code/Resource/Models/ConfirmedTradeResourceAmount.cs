namespace Snowship.NResource.Models {
	public class ConfirmedTradeResourceAmount {
		public ResourceManager.Resource resource;
		public int tradeAmount;
		public int amountRemaining;

		public ConfirmedTradeResourceAmount(ResourceManager.Resource resource, int tradeAmount) {
			this.resource = resource;
			this.tradeAmount = tradeAmount;
			amountRemaining = tradeAmount;
		}
	}
}
