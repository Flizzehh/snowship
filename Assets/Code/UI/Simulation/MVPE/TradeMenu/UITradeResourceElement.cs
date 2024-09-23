using System;
using Snowship.NUI.Generic;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Snowship.NUI.Simulation.TradeMenu {
	public class UITradeResourceElement : UIElement<UITradeResourceElementComponent> {

		private readonly ResourceManager.TradeResourceAmount tradeResourceAmount;

		public event Action OnTradeResourceElementShouldBeRemoved;

		public UITradeResourceElement(ResourceManager.TradeResourceAmount tradeResourceAmount, Transform parent) : base(parent) {
			this.tradeResourceAmount = tradeResourceAmount;

			component.OnTradeAmountChanged += OnTradeAmountChanged;
			component.OnClearButtonClicked += OnClearButtonClicked;

			component.SetResourceImageSprite(tradeResourceAmount.resource.image);
			component.SetResourceName(tradeResourceAmount.resource.name);
			component.SetResourcePrice($"{tradeResourceAmount.resource.price}");
			component.SetTradeAmountText($"{tradeResourceAmount.GetTradeAmount()}");
		}

		private void OnTradeAmountChanged(string amountString) {
			Color tradeAmountTextColour = ColourUtilities.GetColour(ColourUtilities.Colours.DarkGrey50);
			if (int.TryParse(amountString, out int amount)) {
				amount = Mathf.Clamp(amount, -tradeResourceAmount.colonyAmount, tradeResourceAmount.caravanAmount);
				tradeResourceAmount.SetTradeAmount(amount);
				component.SetTradeAmountTextWithoutNotify($"{amount}");
				tradeAmountTextColour = amount switch {
					0 => tradeAmountTextColour,
					< 0 => ColourUtilities.GetColour(ColourUtilities.Colours.LightRed), // Selling: Colony -> Caravan
					> 0 => ColourUtilities.GetColour(ColourUtilities.Colours.LightGreen), // Buying: Caravan -> Colony
				};
			} else {
				component.SetTradeAmountText("0");
			}
			component.SetTradeAmountTextColour(tradeAmountTextColour);
		}

		private void OnClearButtonClicked() {
			tradeResourceAmount.SetTradeAmount(0);
			component.SetTradeAmountText($"{tradeResourceAmount.GetTradeAmount()}");
		}

		public bool Update() {
			tradeResourceAmount.Update();

			int tradeAmount = tradeResourceAmount.GetTradeAmount();

			if (tradeResourceAmount.caravanAmount == 0 && tradeResourceAmount.colonyAmount == 0) {
				Object.Destroy(obj);
				return true; // Removed
			}

			int caravanAmount = tradeResourceAmount.caravanAmount - tradeAmount;
			obj.transform.Find("CaravanResourceAmount-Text").GetComponent<Text>().text = caravanAmount == 0 ? string.Empty : caravanAmount.ToString();
			int colonyAmount = tradeResourceAmount.colonyAmount + tradeAmount;
			obj.transform.Find("ColonyResourceAmount-Text").GetComponent<Text>().text = colonyAmount == 0 ? string.Empty : colonyAmount.ToString();

			obj.transform.Find("Clear-Button").GetComponent<Button>().interactable = tradeResourceAmount.GetTradeAmount() != 0;

			return false; // Not Removed
		}
	}
}

public class ConfirmedTradeResourceElement {
	public GameObject obj;

	public ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount;

	public ConfirmedTradeResourceElement(ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount, Transform parent) {
		this.confirmedTradeResourceAmount = confirmedTradeResourceAmount;

		obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ConfirmedTradeResourceElement-Panel"), parent, false);

		obj.transform.Find("Resource-Image").GetComponent<Image>().sprite = confirmedTradeResourceAmount.resource.image;
		obj.transform.Find("ResourceName-Text").GetComponent<Text>().text = confirmedTradeResourceAmount.resource.name;

		Update();
	}

	public void Update() {
		obj.transform.Find("CollectedVsRemainingAmounts-Text").GetComponent<Text>().text = Mathf.Abs(confirmedTradeResourceAmount.tradeAmount - confirmedTradeResourceAmount.amountRemaining) + " / " + Mathf.Abs(confirmedTradeResourceAmount.tradeAmount);

		if (confirmedTradeResourceAmount.amountRemaining == 0) {
			obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.Colours.LightGreen);
		} else {
			obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.Colours.LightGrey200);
		}
	}
}
