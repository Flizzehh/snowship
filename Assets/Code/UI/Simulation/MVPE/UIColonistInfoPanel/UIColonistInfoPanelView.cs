using System;
using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NHuman;
using Snowship.NResource;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Snowship.NUtilities.ColourUtilities;

namespace Snowship.NUI
{
	public class UIColonistInfoPanelView : UIView {
		[Header("General Information")]
		[SerializeField] private UIHumanBodyElementComponent humanImage;
		[SerializeField] private TMP_Text colonistNameText;
		[SerializeField] private TMP_Text currentActionText;
		[SerializeField] private TMP_Text storedActionText;
		[SerializeField] private TMP_Text affiliationText;

		[Header("Sliders")]
		[SerializeField] private UISliderHandler healthSlider;
		[SerializeField] private UISliderHandler moodSlider;
		[SerializeField] private UISliderHandler inventoryWeightSlider;
		[SerializeField] private TMP_Text inventoryWeightText;
		[SerializeField] private UISliderHandler inventoryVolumeSlider;
		[SerializeField] private TMP_Text inventoryVolumeText;

		[Header("Needs, Moods, Skills Tab")]
		[SerializeField] private Button needsSkillsTabButton;
		[SerializeField] private GameObject needsSkillsTab;

		[Header("Needs Sub-Panel")]
		[SerializeField] private TMP_Text moodCurrentText;
		[SerializeField] private Button moodViewButton;
		[SerializeField] private TMP_Text moodModifiersSumText;
		[SerializeField] private VerticalLayoutGroup needsListVerticalLayoutGroup;

		[Header("Moods Sub-Panel")]
		[SerializeField] private GameObject moodPanel;
		[SerializeField] private Button moodPanelCloseButton;
		[SerializeField] private VerticalLayoutGroup moodListVerticalLayoutGroup;

		[Header("Skills Sub-Panel")]
		[SerializeField] private VerticalLayoutGroup skillsListVerticalLayoutGroup;

		[Header("Inventory Tab")]
		[SerializeField] private Button inventoryTabButton;
		[SerializeField] private GameObject inventoryTab;
		[SerializeField] private Button emptyInventoryButton;
		[SerializeField] private GridLayoutGroup inventoryListGridLayoutGroup;

		[Header("Clothing Tab")]
		[SerializeField] private Button clothingTabButton;
		[SerializeField] private GameObject clothingTab;

		[Header("Current Clothing Sub-Panel")]
		[SerializeField] private UIHumanBodyElementComponent humanBody;
		[SerializeField] private VerticalLayoutGroup clothingButtonsListVerticalLayoutGroup;

		[Header("Clothing Selection Sub-Panel")]
		[SerializeField] private GameObject clothingSelectionPanel;
		[SerializeField] private Button disrobeButton;
		[SerializeField] private Button clothingSelectionPanelBackButton;
		[SerializeField] private GameObject clothesAvailableTitleTextPanel;
		[SerializeField] private GridLayoutGroup clothesAvailableGridLayoutGroup;
		[SerializeField] private GameObject clothesTakenTitleTextPanel;
		[SerializeField] private GridLayoutGroup clothesTakenGridLayoutGroup;

		public List<(Button button, GameObject tab)> ButtonToTabMap => new List<(Button, GameObject)>() {
			(needsSkillsTabButton, needsSkillsTab),
			(inventoryTabButton, inventoryTab),
			(clothingTabButton, clothingTab)
		};

		public event Action<Button> OnTabSelected;
		public event Action OnEmptyInventoryButtonClicked;

		private readonly List<UINeedElement> needElements = new();
		private readonly List<UIMoodElement> moodElements = new();
		private readonly List<UISkillElement> skillElements = new();

		private readonly List<UIReservedResourcesElement> reservedResourcesElements = new();
		private readonly List<UIResourceAmountElement> inventoryResourceAmountElements = new();

		private readonly List<UIClothingButtonElement> clothingButtonElements = new();
		private readonly List<UIClothingElement> clothingElements = new();

		public override void OnOpen() {
			needsSkillsTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(needsSkillsTabButton));
			inventoryTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(inventoryTabButton));
			clothingTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(clothingTabButton));

			moodViewButton.onClick.AddListener(() => SetMoodPanelActive(true));
			moodPanelCloseButton.onClick.AddListener(() => SetMoodPanelActive(false));
			SetMoodPanelActive(false);

			emptyInventoryButton.onClick.AddListener(() => OnEmptyInventoryButtonClicked?.Invoke());

			clothingSelectionPanelBackButton.onClick.AddListener(() => SetClothingSelectionPanelActive(false));
		}

		public override void OnClose() {
			base.OnClose();

			foreach (UINeedElement needElement in needElements) {
				needElement.Close();
			}
			needElements.Clear();

			foreach (UISkillElement skillElement in skillElements) {
				skillElement.Close();
			}
			skillElements.Clear();

			foreach (UIResourceAmountElement resourceAmountElement in inventoryResourceAmountElements) {
				resourceAmountElement.Close();
			}
			inventoryResourceAmountElements.Clear();

			foreach (UIClothingButtonElement clothingButtonElement in clothingButtonElements) {
				clothingButtonElement.Close();
			}
			clothingButtonElements.Clear();

			foreach (UIClothingElement clothingElement in clothingElements) {
				clothingElement.Close();
			}
			clothingElements.Clear();
		}

		public void SetColonistInformation(Sprite colonistSprite, string colonistName, string colonistAffiliation) {
			humanImage.SetBodySectionSprite(BodySection.Skin, colonistSprite);
			humanBody.SetBodySectionSprite(BodySection.Skin, colonistSprite);
			colonistNameText.SetText(colonistName);
			affiliationText.SetText(colonistAffiliation);
		}

		public void SetCurrentActionText(string text) {
			currentActionText.SetText(text);
		}

		public void SetStoredActionText(string text) {
			storedActionText.SetText(text);
		}

		public void SetupHealthSlider((float min, float max) sliderRange, float healthValue, bool wholeNumbers) {
			healthSlider.SetFillColours(EColour.DarkRed, EColour.DarkGreen);
			healthSlider.SetHandleColours(EColour.LightRed, EColour.LightGreen);
			healthSlider.SetSliderRange(sliderRange.min, sliderRange.max, healthValue, wholeNumbers);
		}

		public void OnHealthChanged(float healthValue) {
			healthSlider.SetValue(healthValue);
		}

		public void SetupInventorySliders(Inventory inventory) {
			inventoryWeightSlider.SetSliderRange(0, inventory.maxWeight, inventory.UsedWeight());
			inventoryVolumeSlider.SetSliderRange(0, inventory.maxVolume, inventory.UsedVolume());
			OnInventoryChanged(inventory);
		}

		public void OnInventoryChanged(Inventory inventory) {
			int usedWeight = inventory.UsedWeight();
			int usedVolume = inventory.UsedVolume();
			inventoryWeightSlider.SetValue(usedWeight, $"{Mathf.RoundToInt(usedWeight / (float)inventory.maxWeight * 100)}");
			inventoryVolumeSlider.SetValue(usedVolume, $"{Mathf.RoundToInt(usedVolume / (float)inventory.maxVolume * 100)}");
		}

		public void AddNeedElement(NeedInstance need) {
			needElements.Add(new UINeedElement(needsListVerticalLayoutGroup.transform, need));
		}

		private void SetMoodPanelActive(bool active) {
			moodPanel.SetActive(active);
		}

		public void SetupMoodSlider((float min, float max) sliderRange, float moodValue, bool wholeNumbers) {
			moodSlider.SetFillColours(EColour.DarkRed, EColour.DarkGreen);
			moodSlider.SetHandleColours(EColour.LightRed, EColour.LightGreen);
			moodSlider.SetSliderRange(sliderRange.min, sliderRange.max, moodValue, wholeNumbers);
		}

		public void AddMoodElement(MoodModifierInstance mood) {
			moodElements.Add(new UIMoodElement(moodListVerticalLayoutGroup.transform, mood));
		}

		public void OnMoodAdded(MoodModifierInstance mood) {
			AddMoodElement(mood);
		}

		public void OnMoodChanged(float effectiveMood, float moodModifiersSum) {
			moodSlider.SetValue(effectiveMood);

			moodCurrentText.SetText($"{Mathf.RoundToInt(effectiveMood)}%");
			moodCurrentText.color = Color.Lerp(GetColour(EColour.DarkRed), GetColour(EColour.DarkGreen), effectiveMood / 100);

			moodModifiersSumText.SetText($"{(moodModifiersSum > 0 ? '+' : string.Empty)}{moodModifiersSum}%");
			moodModifiersSumText.color = moodModifiersSum switch {
				> 0 => GetColour(EColour.LightGreen),
				< 0 => GetColour(EColour.LightRed),
				_ => GetColour(EColour.DarkGrey50)
			};
		}

		public void OnMoodRemoved(MoodModifierInstance mood) {
			UIMoodElement moodElementToRemove = moodElements.Find(moodElement => moodElement.Mood == mood);
			if (moodElementToRemove == null) {
				return;
			}
			moodElements.Remove(moodElementToRemove);
			moodElementToRemove.Close();
		}

		public void AddSkillElement(SkillInstance skill) {
			skillElements.Add(new UISkillElement(skillsListVerticalLayoutGroup.transform, skill));
		}

		public void OnInventoryResourceAmountAdded(ResourceAmount resourceAmount) {
			inventoryResourceAmountElements.Add(new UIResourceAmountElement(inventoryListGridLayoutGroup.transform, resourceAmount));
		}

		public void OnInventoryResourceAmountRemoved(ResourceAmount resourceAmount) {
			UIResourceAmountElement resourceAmountElementToRemove = inventoryResourceAmountElements.Find(e => e.ResourceAmount == resourceAmount);
			inventoryResourceAmountElements.Remove(resourceAmountElementToRemove);
			resourceAmountElementToRemove.Close();
		}

		public UIClothingButtonElement CreateClothingButton(BodySection bodySection, Clothing clothing) {
			UIClothingButtonElement clothingButton = new(
				clothingButtonsListVerticalLayoutGroup.transform,
				bodySection,
				clothing
			);
			clothingButtonElements.Add(clothingButton);
			return clothingButton;
		}

		public void SetClothingSelectionPanelActive(bool active) {

			foreach (UIClothingElement clothingElement in clothingElements) {
				clothingElement.Close();
			}
			clothingElements.Clear();

			clothingSelectionPanel.SetActive(active);
		}

		public void SetDisrobeButtonTarget(BodySection bodySection) {
			disrobeButton.onClick.RemoveAllListeners();
			disrobeButton.onClick.AddListener(() => OnDisrobeButtonClicked(bodySection));
		}

		private void OnDisrobeButtonClicked(BodySection bodySection) {
			OnColonistClothingChanged(bodySection, null);
			SetClothingSelectionPanelActive(false);
		}

		public UIClothingElement CreateAvailableClothingElement(Clothing clothing) {
			return CreateClothingElement(clothing, true);
		}

		public UIClothingElement CreateTakenClothingElement(Clothing clothing) {
			return CreateClothingElement(clothing, false);
		}

		private UIClothingElement CreateClothingElement(Clothing clothing, bool available) {
			UIClothingElement clothingElement = new((available ? clothesAvailableGridLayoutGroup : clothesTakenGridLayoutGroup).transform, clothing);
			clothingElements.Add(clothingElement);
			return clothingElement;
		}

		public void SetClothesAvailableTitleTextPanelActive(bool active) {
			clothesAvailableTitleTextPanel.SetActive(active);
		}

		public void SetClothesTakenTitleTextPanelActive(bool active) {
			clothesTakenTitleTextPanel.SetActive(active);
		}

		public void OnColonistClothingChanged(BodySection bodySection, Clothing clothing) {
			foreach (UIClothingButtonElement clothingButtonElement in clothingButtonElements) {
				if (clothingButtonElement.BodySection != bodySection) {
					continue;
				}
				clothingButtonElement.SetClothing(clothing);
				humanBody.SetClothingOnBodySection(bodySection, clothing);
				humanImage.SetClothingOnBodySection(bodySection, clothing);
				return;
			}
		}

		public void OnInventoryReservedResourcesAdded(ReservedResources reservedResources) {
			reservedResourcesElements.Add(new UIReservedResourcesElement(inventoryListGridLayoutGroup.transform, reservedResources));
		}

		public void OnInventoryReservedResourcesRemoved(ReservedResources reservedResources) {
			UIReservedResourcesElement reservedResourcesElement = reservedResourcesElements.Find(rre => rre.ReservedResources == reservedResources);
			if (reservedResourcesElement == null) {
				return;
			}
			reservedResourcesElements.Remove(reservedResourcesElement);
			reservedResourcesElement.Close();
		}
	}
}