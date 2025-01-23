namespace Snowship.NResource
{
	public class ConfirmedTradeResourceAmount {
		public Resource resource;
		public int tradeAmount;
		public int amountRemaining;

		public ConfirmedTradeResourceAmount(Resource resource, int tradeAmount) {
			this.resource = resource;
			this.tradeAmount = tradeAmount;
			amountRemaining = tradeAmount;
		}
	}
}
