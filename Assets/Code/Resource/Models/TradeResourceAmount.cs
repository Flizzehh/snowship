using System;
using Snowship.NCaravan;

namespace Snowship.NResource.Models {
	public class TradeResourceAmount : IDisposable {

		public ResourceManager.Resource resource;

		public int caravanAmount;
		public int colonyAmount;

		private int tradeAmount;

		public int caravanResourcePrice;

		private readonly Caravan caravan;

		public event Action<int> OnCaravanAmountUpdated;
		public event Action<int> OnColonyAmountUpdated;
		public event Action<int> OnTradeAmountUpdated;

		public TradeResourceAmount(ResourceManager.Resource resource, int caravanAmount, int colonyAmount, Caravan caravan) {
			this.resource = resource;
			resource.OnUnreservedTradingPostTotalAmountChanged += SetColonyAmount;

			this.caravanAmount = caravanAmount;
			this.colonyAmount = colonyAmount;

			caravanResourcePrice = caravan.DeterminePriceForResource(resource);

			this.caravan = caravan;
		}

		public void Dispose() {
			resource.OnUnreservedTradingPostTotalAmountChanged -= SetColonyAmount;
		}

		public void Update() {
			UpdateCaravanAmount();
		}

		public void UpdateCaravanAmount() {

			ResourceManager.ResourceAmount caravanResourceAmount = caravan.GetInventory().resources.Find(ra => ra.resource == resource);
			if (caravanResourceAmount == null) {
				OnCaravanAmountUpdated?.Invoke(-1);
				return;
			}

			if (caravanAmount == caravanResourceAmount.amount) {
				return;
			}

			caravanAmount = caravanResourceAmount.amount;
			OnCaravanAmountUpdated?.Invoke(caravanAmount);
		}

		public void SetColonyAmount(int colonyAmount) {
			if (colonyAmount <= 0) {
				OnColonyAmountUpdated?.Invoke(-1);
				return;
			}

			if (this.colonyAmount == colonyAmount) {
				return;
			}

			this.colonyAmount = colonyAmount;
			OnColonyAmountUpdated?.Invoke(colonyAmount);
		}

		public int GetTradeAmount() {
			return tradeAmount;
		}

		public void SetTradeAmount(int tradeAmount) {
			if (this.tradeAmount == tradeAmount) {
				return;
			}

			caravan.SetSelectedResource(this);
			OnTradeAmountUpdated?.Invoke(tradeAmount);
		}
	}
}
