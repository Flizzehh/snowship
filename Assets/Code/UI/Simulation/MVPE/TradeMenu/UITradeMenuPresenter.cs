using JetBrains.Annotations;
using Snowship.NUI.Generic;

namespace Snowship.NUI.Simulation.TradeMenu {
	[UsedImplicitly]
	public class UITradeMenuPresenter : UIPresenter<UITradeMenuView> {

		private readonly List<TradeResourceElement> tradeResourceElements = new List<TradeResourceElement>();
		private readonly List<ConfirmedTradeResourceElement> confirmedTradeResourceElements = new List<ConfirmedTradeResourceElement>();
		private readonly List<TradeResourceElement> removedTradeResourceElements = new List<TradeResourceElement>();

		public UITradeMenuPresenter(UITradeMenuView view) : base(view) {
		}

		public override void OnCreate() {

		}

		public override void OnClose() {

		}

		public void SetTradeMenuActive(bool active) {
			tradeMenu.SetActive(active);

			foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
				Object.Destroy(tradeResourceElement.obj);
			}

			tradeResourceElements.Clear();
			foreach (ConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
				Object.Destroy(confirmedTradeResourceElement.obj);
			}

			confirmedTradeResourceElements.Clear();
		}

		public void SetTradeMenu() {
			Caravan caravan = GameManager.caravanM.selectedCaravan;
			if (caravan != null) {
				SetTradeMenuActive(true);

				tradeMenu.transform.Find("AffiliationCaravanName-Text").GetComponent<Text>().text = "Trade Caravan of " + caravan.location.name;
				tradeMenu.transform.Find("CaravanResourceGroup-Text").GetComponent<Text>().text = "Selling " + caravan.resourceGroup.name;
				tradeMenu.transform.Find("AffiliationDescription-Text").GetComponent<Text>().text = string.Format("{0} is a {1} {2} with {3} resources in a {4} climate.", caravan.location.name, caravan.location.wealth.ToString().ToLower(), caravan.location.citySize.ToString().ToLower(), caravan.location.resourceRichness.ToString().ToLower(), TileManager.Biome.GetBiomeByEnum(caravan.location.biomeType).name.ToLower());

				RemakeTradeResourceElements(caravan);
				RemakeConfirmedTradeResourceElements(caravan);
			} else {
				SetTradeMenuActive(false);
			}
		}

		private void RemakeTradeResourceElements(Caravan caravan) {
			foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
				Object.Destroy(tradeResourceElement.obj);
			}

			tradeResourceElements.Clear();
			foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in caravan.GenerateTradeResourceAmounts()) {
				tradeResourceElements.Add(new TradeResourceElement(tradeResourceAmount, tradeMenu.transform.Find("TradeResources-Panel/TradeResources-ScrollPanel/TradeResourcesList-Panel")));
			}
		}

		private void RemakeConfirmedTradeResourceElements(Caravan caravan) {
			foreach (ConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
				Object.Destroy(confirmedTradeResourceElement.obj);
			}

			confirmedTradeResourceElements.Clear();
			foreach (ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade.Where(crtt => crtt.tradeAmount > 0).OrderByDescending(crtt => crtt.tradeAmount)) {
				confirmedTradeResourceElements.Add(new ConfirmedTradeResourceElement(confirmedTradeResourceAmount, tradeMenu.transform.Find("ConfirmedTradeResources-Panel/ConfirmedTradeResources-ScrollPanel/ConfirmedTradeResourcesList-Panel")));
			}

			foreach (ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade.Where(crtt => crtt.tradeAmount < 0).OrderBy(crtt => crtt.tradeAmount)) {
				confirmedTradeResourceElements.Add(new ConfirmedTradeResourceElement(confirmedTradeResourceAmount, tradeMenu.transform.Find("ConfirmedTradeResources-Panel/ConfirmedTradeResources-ScrollPanel/ConfirmedTradeResourcesList-Panel")));
			}
		}



		public void UpdateTradeMenu() {
			if (tradeMenu.activeSelf) {
				int caravanTradeValue = 0;
				int colonyTradeValue = 0;

				int caravanTradeAmount = 0;
				int colonyTradeAmount = 0;

				if (GameManager.caravanM.selectedCaravan.traders.Count > 0) {
					List<ResourceManager.ResourceAmount> availableResources = GameManager.resourceM.GetAvailableResourcesInTradingPostsInRegion(GameManager.caravanM.selectedCaravan.traders.Find(t => t != null).overTile.region);
					foreach (ResourceManager.ResourceAmount resourceAmount in availableResources) {
						TradeResourceElement tradeResourceElement = tradeResourceElements.Find(tre => tre.tradeResourceAmount.resource == resourceAmount.resource);
						if (tradeResourceElement != null) {
							tradeResourceElement.tradeResourceAmount.SetColonyAmount(resourceAmount.amount);
							tradeResourceElement.ValidateTradeAmountInputField();
						} else {
							tradeResourceElements.Add(new TradeResourceElement(new ResourceManager.TradeResourceAmount(resourceAmount.resource, 0, GameManager.caravanM.selectedCaravan), tradeMenu.transform.Find("TradeResources-Panel/TradeResources-ScrollPanel/TradeResourcesList-Panel")));
						}
					}

					foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
						ResourceManager.ResourceAmount resourceAmount = availableResources.Find(ra => ra.resource == tradeResourceElement.tradeResourceAmount.resource);
						if (resourceAmount != null) {
							tradeResourceElement.tradeResourceAmount.SetColonyAmount(resourceAmount.amount);
						} else {
							tradeResourceElement.tradeResourceAmount.SetColonyAmount(0);
						}

						tradeResourceElement.ValidateTradeAmountInputField();
					}
				}

				foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
					bool removed = tradeResourceElement.Update();
					if (removed) {
						removedTradeResourceElements.Add(tradeResourceElement);
						continue;
					}

					ResourceManager.TradeResourceAmount tradeResourceAmount = tradeResourceElement.tradeResourceAmount;
					int tradeAmount = tradeResourceAmount.GetTradeAmount();

					if (tradeAmount < 0) {
						colonyTradeValue += Mathf.Abs(tradeResourceAmount.resource.price * tradeAmount);
						colonyTradeAmount += Mathf.Abs(tradeAmount);
					} else if (tradeAmount > 0) {
						caravanTradeValue += Mathf.Abs(tradeResourceAmount.caravanResourcePrice * tradeAmount);
						caravanTradeAmount += Mathf.Abs(tradeAmount);
					}
				}

				foreach (TradeResourceElement tradeResourceElement in removedTradeResourceElements) {
					tradeResourceElements.Remove(tradeResourceElement);
				}

				removedTradeResourceElements.Clear();

				int totalTradeAmount = caravanTradeAmount + colonyTradeAmount;

				tradeMenu.transform.Find("CaravanTradeAmount-Text").GetComponent<Text>().text = caravanTradeAmount != 0 ? caravanTradeAmount + " resource" + (caravanTradeAmount == 1 ? string.Empty : "s") : string.Empty;
				tradeMenu.transform.Find("CaravanTradeValue-Text").GetComponent<Text>().text = caravanTradeAmount != 0 ? caravanTradeValue + " value" : string.Empty;

				tradeMenu.transform.Find("ColonyTradeAmount-Text").GetComponent<Text>().text = colonyTradeAmount != 0 ? colonyTradeAmount + " resource" + (colonyTradeAmount == 1 ? string.Empty : "s") : string.Empty;
				tradeMenu.transform.Find("ColonyTradeValue-Text").GetComponent<Text>().text = colonyTradeAmount != 0 ? colonyTradeValue + " value" : string.Empty;

				int tradeValueDifference = caravanTradeValue - colonyTradeValue;
				string tradeFairness = "Equal Trade";
				if (tradeValueDifference > 0) {
					tradeFairness = "Unfair to Caravan";
				} else if (tradeValueDifference < 0) {
					tradeFairness = "Unfair to Colony";
				}

				tradeMenu.transform.Find("TradeValueDifference-Text").GetComponent<Text>().text = totalTradeAmount != 0 ? Mathf.Abs(tradeValueDifference).ToString() : "Select Resources to Trade";
				tradeMenu.transform.Find("TradeFairness-Text").GetComponent<Text>().text = totalTradeAmount != 0 ? tradeFairness : string.Empty;

				tradeMenu.transform.Find("ConfirmTrade-Button").GetComponent<Button>().interactable = tradeValueDifference <= 0 && totalTradeAmount > 0;

				foreach (ConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
					confirmedTradeResourceElement.Update();
				}
			}
		}

		public void ConfirmTrade() {
			Caravan caravan = GameManager.caravanM.selectedCaravan;

			if (caravan != null) {
				caravan.ConfirmTrade();
				SetTradeMenu();

				foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
					tradeResourceElement.SetTradeAmount(0);
				}
			}
		}
	}
}
