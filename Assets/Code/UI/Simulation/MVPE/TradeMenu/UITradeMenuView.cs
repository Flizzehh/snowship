using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCaravan;
using Snowship.NResource.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.TradeMenu {
	public class UITradeMenuView : UIView {

		[Header("Buttons")]
		[SerializeField] private Button confirmTradeButton;
		[SerializeField] private Button closeButton;

		[Header("Title Texts")]
		[SerializeField] private Text tradeCaravanAffiliationText;
		[SerializeField] private Text tradeCaravanDescriptionText;
		[SerializeField] private Text sellingResourceGroupText;

		[Header("Resource Lists")]
		[SerializeField] private VerticalLayoutGroup tradeResourcesLayoutGroup;
		[SerializeField] private VerticalLayoutGroup confirmedTradeResourcesLayoutGroup;

		[Header("Value Texts")]
		[SerializeField] private Text tradeResourcesCaravanText;
		[SerializeField] private Text tradeValueCaravanText;
		[SerializeField] private Text tradeResourcesColonyText;
		[SerializeField] private Text tradeValueColonyText;

		[SerializeField] private Text tradeValueDifferenceText;
		[SerializeField] private Text tradeFairnessText;

		private readonly List<UITradeResourceElement> tradeResourceElements = new();
		private readonly List<UIConfirmedTradeResourceElement> confirmedTradeResourceElements = new();
		private readonly List<UITradeResourceElement> removedTradeResourceElements = new();

		public event Action OnConfirmTradeButtonClicked;
		public event Action OnCloseButtonClicked;

		public override void OnOpen() {
			confirmTradeButton.onClick.AddListener(() => OnConfirmTradeButtonClicked?.Invoke());
			closeButton.onClick.AddListener(() => OnCloseButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}

		public void SetCaravanInformation(Caravan caravan) {

			if (caravan == null) {
				return;
			}

			tradeCaravanAffiliationText.text = $"Trade Caravan of {caravan.location.name}";
			tradeCaravanDescriptionText.text = string.Format($"{caravan.location.name} is a {caravan.location.wealth.ToLower()} {caravan.location.citySize.ToLower()} with {caravan.location.resourceRichness.ToLower()} resources in a {TileManager.Biome.GetBiomeByEnum(caravan.location.biomeType).name.ToLower()} climate."); // TODO This needs to be done in a better way
			sellingResourceGroupText.text = $"Selling {caravan.resourceGroup.name}";
		}

		public void CreateTradeResourceElements(List<TradeResourceAmount> tradeResourceAmounts) {

			// Remove any old-existing elements
			foreach (UITradeResourceElement tradeResourceElement in tradeResourceElements) {
				tradeResourceElement.Close();
			}
			tradeResourceElements.Clear();

			// Create new elements
			foreach (TradeResourceAmount tradeResourceAmount in tradeResourceAmounts) {
				UITradeResourceElement tradeResourceElement = new UITradeResourceElement(
					tradeResourceAmount,
					tradeResourcesLayoutGroup.transform
				);
				tradeResourceElements.Add(tradeResourceElement);
				tradeResourceElement.OnTradeResourceElementShouldBeRemoved += OnTradeResourceElementShouldBeRemoved;
			}
		}

		private void OnTradeResourceElementShouldBeRemoved(UITradeResourceElement tradeResourceElement) {
			removedTradeResourceElements.Add(tradeResourceElement);
		}

		public void AddTradeResourceElementIfMissing(
			ResourceManager.Resource resource,
			int caravanAmount,
			int colonyAmount,
			Caravan caravan
		) {
			if (tradeResourceElements.Find(tre => tre.tradeResourceAmount.resource == resource) == null) {
				tradeResourceElements.Add(new UITradeResourceElement(
					new TradeResourceAmount(resource, caravanAmount, colonyAmount, caravan),
					tradeResourcesLayoutGroup.transform
				));
			}
		}

		public void CreateConfirmedTradeResourceElements(
			IOrderedEnumerable<ConfirmedTradeResourceAmount> colonyConfirmedTradeResourceAmounts,
			IOrderedEnumerable<ConfirmedTradeResourceAmount> caravanConfirmedTradeResourceAmounts
		) {
			// Remove any old-existing elements
			foreach (UIConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
				confirmedTradeResourceElement.Close();
			}
			confirmedTradeResourceElements.Clear();

			// Create new elements
			foreach (ConfirmedTradeResourceAmount confirmedTradeResourceAmount in colonyConfirmedTradeResourceAmounts) {
				confirmedTradeResourceElements.Add(new UIConfirmedTradeResourceElement(
					confirmedTradeResourceAmount,
					confirmedTradeResourcesLayoutGroup.transform
				));
			}

			foreach (ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravanConfirmedTradeResourceAmounts) {
				confirmedTradeResourceElements.Add(new UIConfirmedTradeResourceElement(
					confirmedTradeResourceAmount,
					confirmedTradeResourcesLayoutGroup.transform
				));
			}
		}

		public void RefreshAfterTradeCompleted() {
			foreach (UITradeResourceElement tradeResourceElement in tradeResourceElements) {
				tradeResourceElement.OnTradeAmountChanged(string.Empty);
				tradeResourceElement.tradeResourceAmount.UpdateCaravanAmount();
			}
		}

		public void RemoveRemovedTradeResourceElements() {
			foreach (UITradeResourceElement tradeResourceElement in removedTradeResourceElements) {
				tradeResourceElement.Close();
			}
			removedTradeResourceElements.Clear();
		}
	}
}
