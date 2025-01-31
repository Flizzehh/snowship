using System;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI
{
	public class UITradeResourceElement : UIElement<UITradeResourceElementComponent> {

		public readonly TradeResourceAmount tradeResourceAmount;

		public event Action<UITradeResourceElement> OnTradeResourceElementShouldBeRemoved;

		public UITradeResourceElement(TradeResourceAmount tradeResourceAmount, Transform parent) : base(parent) {
			this.tradeResourceAmount = tradeResourceAmount;

			tradeResourceAmount.OnColonyAmountUpdated += Component.SetColonyAmount;
			tradeResourceAmount.OnCaravanAmountUpdated += Component.SetCaravanAmount;

			Component.OnTradeAmountChanged += OnTradeAmountChanged;
			Component.OnClearButtonClicked += OnClearButtonClicked;
			Component.OnBuyIncreaseButtonClicked += OnBuyIncreaseButtonClicked;
			Component.OnSellIncreaseButtonClicked += OnSellIncreaseButtonClicked;

			Component.SetResourceImageSprite(tradeResourceAmount.resource.image);
			Component.SetResourceName(tradeResourceAmount.resource.name);
			Component.SetResourcePrice($"{tradeResourceAmount.resource.price}");
			Component.SetTradeAmountText($"{tradeResourceAmount.GetTradeAmount()}");
		}

		public void OnTradeAmountChanged(string amountString) {
			if (int.TryParse(amountString, out int amount)) {
				OnTradeAmountChanged(amount);
			} else {
				Component.SetTradeAmountText("0");
			}
		}

		public void OnTradeAmountChanged(int amount) {
			amount = Mathf.Clamp(amount, -tradeResourceAmount.colonyAmount, tradeResourceAmount.caravanAmount);
			tradeResourceAmount.SetTradeAmount(amount);
			Component.SetTradeAmountTextWithoutNotify($"{amount}");
			Color tradeAmountTextColour = amount switch {
				0 => ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50),
				< 0 => ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), // Selling: Colony -> Caravan
				> 0 => ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), // Buying: Caravan -> Colony
			};
			Component.SetTradeAmountTextColour(tradeAmountTextColour);
		}

		private void OnClearButtonClicked() {
			tradeResourceAmount.SetTradeAmount(0);
			Component.SetTradeAmountText($"{tradeResourceAmount.GetTradeAmount()}");
		}

		// TODO Make this update from the backing global resource amount being updated (once implemented)
		public void OnUpdate() {

			//tradeResourceAmount.Update();

			int tradeAmount = tradeResourceAmount.GetTradeAmount();

			if (tradeResourceAmount.caravanAmount == 0 && tradeResourceAmount.colonyAmount == 0) {
				OnTradeResourceElementShouldBeRemoved?.Invoke(this);
			}

			int caravanAmount = tradeResourceAmount.caravanAmount - tradeAmount;
			Component.SetCaravanAmount(caravanAmount);
			int colonyAmount = tradeResourceAmount.colonyAmount + tradeAmount;
			Component.SetColonyAmount(colonyAmount);

			Component.SetClearButtonInteractable(tradeResourceAmount.GetTradeAmount() != 0);
		}

		private void OnBuyIncreaseButtonClicked(bool all) {
			if (all) {
				OnTradeAmountChanged(tradeResourceAmount.caravanAmount);
			} else {
				OnTradeAmountChanged(tradeResourceAmount.GetTradeAmount() + 1);
			}
		}

		private void OnSellIncreaseButtonClicked(bool all) {
			if (all) {
				OnTradeAmountChanged(tradeResourceAmount.colonyAmount);
			} else {
				OnTradeAmountChanged(tradeResourceAmount.GetTradeAmount() - 1);
			}
		}
	}
}