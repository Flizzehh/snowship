using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UIColonistInfoPanelPresenter : UIPresenter<UIColonistInfoPanelView, UIColonistInfoPanelParameters>
	{
		private readonly Colonist colonist;
		private readonly Inventory inventory;

		public UIColonistInfoPanelPresenter(UIColonistInfoPanelView view, UIColonistInfoPanelParameters parameters) : base(view, parameters) {
			colonist = Parameters.Colonist;
			inventory = colonist.Inventory;
		}

		public override void OnCreate() {

			View.OnTabSelected += OnTabSelected;

			OnTabSelected(View.ButtonToTabMap.First().button);

			View.SetColonistInformation(
				colonist.moveSprites[0],
				$"{colonist.Name} ({colonist.gender.ToString()[0]})",
				$"Colonist of {GameManager.Get<ColonyManager>().colony.name}"
			);

			SetupNeedsSkillsTab();

			colonist.OnJobChanged += OnJobChanged;
			OnJobChanged(colonist.Job);

			View.SetupHealthSlider((0, 1), colonist.Health, true);
			View.OnHealthChanged(colonist.Health);
			colonist.OnHealthChanged += View.OnHealthChanged;

			View.SetupMoodSlider((0, 100), colonist.Moods.EffectiveMood, true);
			View.OnMoodChanged(colonist.Moods.EffectiveMood, colonist.Moods.MoodModifiersSum);
			colonist.Moods.OnMoodAdded += OnMoodAdded;
			colonist.Moods.OnMoodChanged += View.OnMoodChanged;
			colonist.Moods.OnMoodRemoved += OnMoodRemoved;

			View.SetupInventorySliders(inventory);
			View.OnEmptyInventoryButtonClicked += OnEmptyInventoryButtonClicked;
			inventory.OnInventoryChanged += View.OnInventoryChanged;

			SetupInventoryTab();
			inventory.OnResourceAmountAdded += View.OnInventoryResourceAmountAdded;
			inventory.OnResourceAmountRemoved += View.OnInventoryResourceAmountRemoved;
			inventory.OnReservedResourcesAdded += View.OnInventoryReservedResourcesAdded;
			inventory.OnReservedResourcesRemoved += View.OnInventoryReservedResourcesRemoved;

			SetupClothingTab();
			colonist.OnClothingChanged += View.OnColonistClothingChanged;
			View.SetClothingSelectionPanelActive(false);
		}

		public override void OnClose() {
			base.OnClose();

			View.OnTabSelected -= OnTabSelected;

			colonist.OnJobChanged -= OnJobChanged;

			colonist.OnHealthChanged -= View.OnHealthChanged;

			colonist.Moods.OnMoodAdded -= OnMoodAdded;
			colonist.Moods.OnMoodChanged -= View.OnMoodChanged;
			colonist.Moods.OnMoodRemoved -= OnMoodRemoved;

			View.OnEmptyInventoryButtonClicked -= OnEmptyInventoryButtonClicked;
			inventory.OnInventoryChanged -= View.OnInventoryChanged;

			inventory.OnResourceAmountAdded -= View.OnInventoryResourceAmountAdded;
			inventory.OnResourceAmountRemoved -= View.OnInventoryResourceAmountRemoved;
			inventory.OnReservedResourcesAdded -= View.OnInventoryReservedResourcesAdded;
			inventory.OnReservedResourcesRemoved -= View.OnInventoryReservedResourcesRemoved;

			colonist.OnClothingChanged -= View.OnColonistClothingChanged;

			foreach (MoodModifierInstance mood in colonist.Moods.MoodModifiers) {
				OnMoodRemoved(mood);
			}
		}

		private void OnTabSelected(Button tabButton) {
			foreach ((Button button, GameObject tab) mapping in View.ButtonToTabMap) {
				mapping.tab.SetActive(mapping.button == tabButton);
			}
		}

		private void OnJobChanged(IJob job) {
			View.SetCurrentActionText(job?.Description);
			View.SetStoredActionText(string.Empty);
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
			foreach (MoodModifierInstance mood in colonist.Moods.MoodModifiers) {
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

		private void OnEmptyInventoryButtonClicked() {
			colonist.EmptyInventory(colonist.FindValidContainersToEmptyInventory());
		}

		private void SetupClothingTab() {
			foreach (KeyValuePair<BodySection, Clothing> clothingMapping in colonist.clothes) {
				UIClothingButtonElement clothingButton = View.CreateClothingButton(clothingMapping.Key, clothingMapping.Value);
				clothingButton.OnButtonClicked += OnClothingButtonClicked;
			}
		}

		private void OnClothingButtonClicked(BodySection bodySection) {

			View.SetClothingSelectionPanelActive(true);
			View.SetClothesAvailableTitleTextPanelActive(false);
			View.SetClothesTakenTitleTextPanelActive(false);

			View.SetDisrobeButtonTarget(bodySection);

			List<Clothing> clothesByAppearance = Clothing.GetClothesByAppearance(bodySection);
			foreach (Clothing clothing in clothesByAppearance) {
				if (clothing.GetWorldTotalAmount() <= 0) {
					continue;
				}

				UIClothingElement clothingElement = null;

				if (clothing.GetAvailableAmount() > 0) {
					clothingElement = View.CreateAvailableClothingElement(clothing);
					View.SetClothesAvailableTitleTextPanelActive(true);
				} else if (colonist.clothes[bodySection] == null || clothing.name != colonist.clothes[bodySection].name) {
					clothingElement = View.CreateTakenClothingElement(clothing);
					View.SetClothesTakenTitleTextPanelActive(true);
				}

				if (clothingElement != null) {
					clothingElement.OnButtonClicked += OnClothingElementClicked;
				}
			}
		}

		private void OnClothingElementClicked(Clothing clothing) {
			colonist.ChangeClothing(clothing.prefab.BodySection, clothing);
			View.SetClothingSelectionPanelActive(false);
		}
	}
}