using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NResources;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel {

	[UsedImplicitly]
	public class UIColonistInfoPanelPresenter : UIPresenter<UIColonistInfoPanelView, UIColonistInfoPanelParameters>
	{
		private readonly Colonist colonist;
		private readonly Inventory inventory;

		public UIColonistInfoPanelPresenter(UIColonistInfoPanelView view, UIColonistInfoPanelParameters parameters) : base(view, parameters) {
			colonist = Parameters.Colonist;
			inventory = colonist.GetInventory();
		}

		public override void OnCreate() {

			View.OnTabSelected += OnTabSelected;

			OnTabSelected(View.ButtonToTabMap.First().button);

			View.SetColonistInformation(
				colonist.moveSprites[0],
				$"{colonist.name} ({colonist.gender.ToString()[0]})",
				$"Colonist of {GameManager.colonyM.colony.name}"
			);

			SetupNeedsSkillsTab();

			View.SetupHealthSlider((0, 1), colonist.Health, true);
			View.OnHealthChanged(colonist.Health);
			colonist.OnHealthChanged += View.OnHealthChanged;

			View.SetupMoodSlider((0, 100), colonist.effectiveMood, true);
			View.OnMoodChanged(colonist.effectiveMood, colonist.moodModifiersSum);
			colonist.OnMoodAdded += OnMoodAdded;
			colonist.OnMoodChanged += View.OnMoodChanged;
			colonist.OnMoodRemoved += OnMoodRemoved;

			View.SetupInventorySliders(inventory);
			inventory.OnInventoryChanged += View.OnInventoryChanged;

			SetupInventoryTab();
			inventory.OnResourceAmountAdded += View.OnInventoryResourceAmountAdded;
			inventory.OnResourceAmountRemoved += View.OnInventoryResourceAmountRemoved;

			SetupClothingTab();
			colonist.OnClothingChanged += View.OnColonistClothingChanged;
			View.SetClothingSelectionPanelActive(false);
		}

		public override void OnClose() {
			base.OnClose();

			View.OnTabSelected -= OnTabSelected;

			colonist.OnHealthChanged -= View.OnHealthChanged;

			colonist.OnMoodAdded -= OnMoodAdded;
			colonist.OnMoodChanged -= View.OnMoodChanged;
			colonist.OnMoodRemoved -= OnMoodRemoved;

			inventory.OnInventoryChanged -= View.OnInventoryChanged;

			inventory.OnResourceAmountAdded -= View.OnInventoryResourceAmountAdded;
			inventory.OnResourceAmountRemoved -= View.OnInventoryResourceAmountRemoved;

			colonist.OnClothingChanged -= View.OnColonistClothingChanged;

			foreach (MoodModifierInstance mood in colonist.moodModifiers) {
				OnMoodRemoved(mood);
			}
		}

		private void OnTabSelected(Button tabButton) {
			foreach ((Button button, GameObject tab) mapping in View.ButtonToTabMap) {
				mapping.tab.SetActive(mapping.button == tabButton);
			}
		}

		private void OnMoodAdded(MoodModifierInstance mood) {
			View.OnMoodAdded(mood);
			mood.OnTimerAtZero += OnMoodRemoved;
		}

		private void OnMoodRemoved(MoodModifierInstance mood) {
			mood.OnTimerAtZero -= OnMoodAdded;
			View.OnMoodRemoved(mood);
		}

		private void SetupNeedsSkillsTab() {
			foreach (NeedInstance need in colonist.needs.OrderByDescending(need => need.GetValue())) {
				View.AddNeedElement(need);
			}
			foreach (MoodModifierInstance mood in colonist.moodModifiers) {
				View.AddMoodElement(mood);
			}
			foreach (SkillInstance skill in colonist.skills) {
				View.AddSkillElement(skill);
			}
		}

		private void SetupInventoryTab() {
			foreach (ResourceAmount resourceAmount in inventory.resources) {
				View.OnInventoryResourceAmountAdded(resourceAmount);
			}
		}

		private void SetupClothingTab() {
			foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> clothingMapping in colonist.clothes) {
				UIClothingButtonElement clothingButton = View.CreateClothingButton(clothingMapping.Key, clothingMapping.Value);
				clothingButton.OnButtonClicked += OnClothingButtonClicked;
			}
		}

		private void OnClothingButtonClicked(HumanManager.Human.Appearance appearance) {
			View.SetClothingSelectionPanelActive(true);
			View.SetClothesAvailableTitleTextPanelActive(false);
			View.SetClothesTakenTitleTextPanelActive(false);
			foreach (ResourceManager.Clothing clothing in GameManager.resourceM.GetClothesByAppearance(appearance)) {
				if (clothing.GetWorldTotalAmount() <= 0) {
					continue;
				}

				UIClothingElement clothingElement = null;

				if (clothing.GetAvailableAmount() > 0) {
					clothingElement = View.CreateAvailableClothingElement(clothing);
					View.SetClothesAvailableTitleTextPanelActive(true);
				} else if (colonist.clothes[appearance] == null || clothing.name != colonist.clothes[appearance].name) {
					clothingElement = View.CreateTakenClothingElement(clothing);
					View.SetClothesTakenTitleTextPanelActive(true);
				}

				if (clothingElement != null) {
					clothingElement.OnButtonClicked += OnClothingElementClicked;
				}
			}
		}

		private void OnClothingElementClicked(ResourceManager.Clothing clothing) {
			colonist.ChangeClothing(clothing.prefab.appearance, clothing);
			View.SetClothingSelectionPanelActive(false);
		}
	}
}
