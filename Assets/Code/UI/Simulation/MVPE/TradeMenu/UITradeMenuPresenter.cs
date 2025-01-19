using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NCaravan;
using Snowship.NResource.Models;

namespace Snowship.NUI.Simulation.TradeMenu {
	[UsedImplicitly]
	public class UITradeMenuPresenter : UIPresenter<UITradeMenuView> {

		private List<TradeResourceAmount> tradeResourceAmounts = new();
		private readonly List<ConfirmedTradeResourceAmount> colonyConfirmedTradeResourceAmounts = new();
		private readonly List<ConfirmedTradeResourceAmount> caravanConfirmedTradeResourceAmounts = new();

		public UITradeMenuPresenter(UITradeMenuView view) : base(view) {
		}

		public override void OnCreate() {
			Caravan caravan = GameManager.caravanM.selectedCaravan;

			View.SetCaravanInformation(caravan);

			CreateTradeResourceElements(caravan);
			CreateConfirmedTradeResourceElements(caravan);

			View.OnConfirmTradeButtonClicked += ConfirmTrade;
			View.OnCloseButtonClicked += Close;
		}

		private void CreateTradeResourceElements(Caravan caravan) {
			tradeResourceAmounts = caravan.GenerateTradeResourceAmounts();
			View.CreateTradeResourceElements(tradeResourceAmounts);
		}

		private void CreateConfirmedTradeResourceElements(Caravan caravan) {
			View.CreateConfirmedTradeResourceElements(
				caravan.confirmedResourcesToTrade
					.Where(ctra => ctra.tradeAmount > 0)
					.OrderByDescending(ctra => ctra.tradeAmount),
				caravan.confirmedResourcesToTrade
					.Where(ctra => ctra.tradeAmount < 0)
					.OrderBy(ctra => ctra.tradeAmount)
			);
		}
		// TODO Probably needs to be two methods, one for adding/removing TREs as values resource totals change, and one for summing up the trade amounts as TradeAmounts change
		// public void UpdateTradeMenu() {
		//
		// 	if (GameManager.caravanM.selectedCaravan.traders.Count > 0) {
		// 		List<ResourceManager.ResourceAmount> availableResources = GameManager.resourceM.GetAvailableResourcesInTradingPostsInRegion(GameManager.caravanM.selectedCaravan.traders.Find(t => t != null).overTile.region);
		// 		foreach (ResourceManager.ResourceAmount resourceAmount in availableResources) {
		// 			View.AddTradeResourceElementIfMissing(
		// 				resourceAmount.resource,
		// 				0,
		// 				resourceAmount.amount,
		// 				GameManager.caravanM.selectedCaravan
		// 			);
		// 		}
		// 	}
		//
		// 	View.RemoveRemovedTradeResourceElements();
		//
		// 	int caravanTradeValue = 0, colonyTradeValue = 0, caravanTradeAmount = 0, colonyTradeAmount = 0;
		//
		// 	foreach (UITradeResourceElement tradeResourceElement in tradeResourceElements) {
		// 		TradeResourceAmount tradeResourceAmount = tradeResourceElement.tradeResourceAmount;
		// 		int tradeAmount = tradeResourceAmount.GetTradeAmount();
		//
		// 		if (tradeAmount < 0) {
		// 			colonyTradeValue += Mathf.Abs(tradeResourceAmount.resource.price * tradeAmount);
		// 			colonyTradeAmount += Mathf.Abs(tradeAmount);
		// 		} else if (tradeAmount > 0) {
		// 			caravanTradeValue += Mathf.Abs(tradeResourceAmount.caravanResourcePrice * tradeAmount);
		// 			caravanTradeAmount += Mathf.Abs(tradeAmount);
		// 		}
		// 	}
		//
		// 	int totalTradeAmount = caravanTradeAmount + colonyTradeAmount;
		//
		// 	tradeMenu.transform.Find("CaravanTradeAmount-Text").GetComponent<Text>().text = caravanTradeAmount != 0 ? caravanTradeAmount + " resource" + (caravanTradeAmount == 1 ? string.Empty : "s") : string.Empty;
		// 	tradeMenu.transform.Find("CaravanTradeValue-Text").GetComponent<Text>().text = caravanTradeAmount != 0 ? caravanTradeValue + " value" : string.Empty;
		//
		// 	tradeMenu.transform.Find("ColonyTradeAmount-Text").GetComponent<Text>().text = colonyTradeAmount != 0 ? colonyTradeAmount + " resource" + (colonyTradeAmount == 1 ? string.Empty : "s") : string.Empty;
		// 	tradeMenu.transform.Find("ColonyTradeValue-Text").GetComponent<Text>().text = colonyTradeAmount != 0 ? colonyTradeValue + " value" : string.Empty;
		//
		// 	int tradeValueDifference = caravanTradeValue - colonyTradeValue;
		// 	string tradeFairness = "Equal Trade";
		// 	if (tradeValueDifference > 0) {
		// 		tradeFairness = "Unfair to Caravan";
		// 	} else if (tradeValueDifference < 0) {
		// 		tradeFairness = "Unfair to Colony";
		// 	}
		//
		// 	tradeMenu.transform.Find("TradeValueDifference-Text").GetComponent<Text>().text = totalTradeAmount != 0 ? Mathf.Abs(tradeValueDifference).ToString() : "Select Resources to Trade";
		// 	tradeMenu.transform.Find("TradeFairness-Text").GetComponent<Text>().text = totalTradeAmount != 0 ? tradeFairness : string.Empty;
		//
		// 	tradeMenu.transform.Find("ConfirmTrade-Button").GetComponent<Button>().interactable = tradeValueDifference <= 0 && totalTradeAmount > 0;
		//
		// 	foreach (UIConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
		// 		confirmedTradeResourceElement.Update();
		// 	}
		// }

		public void ConfirmTrade() {
			Caravan caravan = GameManager.caravanM.selectedCaravan;

			if (caravan == null) {
				return;
			}

			caravan.ConfirmTrade();
			View.RefreshAfterTradeCompleted();
		}
	}
}
