using System;
using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NResources;
using Snowship.NUI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Snowship.NUtilities.ColourUtilities;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel {
	public class UIColonistInfoPanelView : UIView {
		[Header("General Information")]
		[SerializeField] private Image colonistImage; // TODO Turn into prefab to show all layers
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
		// TODO Mood Element Prefab

		[Header("Skills Sub-Panel")]
		[SerializeField] private VerticalLayoutGroup skillsListVerticalLayoutGroup;
		// TODO Skill Element Prefab

		[Header("Inventory Tab")]
		[SerializeField] private Button inventoryTabButton;
		[SerializeField] private GameObject inventoryTab;
		[SerializeField] private Button emptyInventoryButton;
		[SerializeField] private GridLayoutGroup inventoryListGridLayoutGroup;
		// TODO Resource Amount Element Prefab

		[Header("Clothing Tab")]
		[SerializeField] private Button clothingTabButton;
		[SerializeField] private GameObject clothingTab;

		[Header("Current Clothing Sub-Panel")]
		[SerializeField] private Image colonistClothingImage; // TODO Turn into prefab to show all layers

		// TODO Turn these buttons into individual prefabs
		[SerializeField] private Button hatButton;
		[SerializeField] private Button scarfButton;
		[SerializeField] private Button topButton;
		[SerializeField] private Button bottomsButton;
		[SerializeField] private Button backpackButton;

		[Header("Clothing Selection Sub-Panel")]
		[SerializeField] private GameObject clothingSelectionPanel;
		[SerializeField] private Button disrobeButton;
		[SerializeField] private Button backButton;
		[SerializeField] private GridLayoutGroup clothesAvailableGridLayoutGroup;
		[SerializeField] private GridLayoutGroup clothesTakenGridLayoutGroup;
		// TODO Clothing Element Prefab

		public List<(Button button, GameObject tab)> ButtonToTabMap => new List<(Button, GameObject)>() {
			(needsSkillsTabButton, needsSkillsTab),
			(inventoryTabButton, inventoryTab),
			(clothingTabButton, clothingTab)
		};

		public event Action<Button> OnTabSelected;

		private readonly List<UINeedElement> needElements = new();
		private readonly List<UIMoodElement> moodElements = new();
		private readonly List<UISkillElement> skillElements = new();
		private readonly List<UIResourceAmountElement> inventoryResourceAmountElements = new();

		public override void OnOpen() {
			needsSkillsTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(needsSkillsTabButton));
			inventoryTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(inventoryTabButton));
			clothingTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(clothingTabButton));

			moodViewButton.onClick.AddListener(() => SetMoodPanelActive(true));
			moodPanelCloseButton.onClick.AddListener(() => SetMoodPanelActive(false));
			SetMoodPanelActive(false);
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
		}

		public void SetColonistInformation(Sprite colonistSprite, string colonistName, string colonistAffiliation) {
			colonistImage.sprite = colonistSprite;
			colonistNameText.SetText(colonistName);
			affiliationText.SetText(colonistAffiliation);
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

		private void SetupClothingTab() {
			SetupCurrentClothingSubPanel();
			SetupClothingSelectionSubPanel();
		}

		private void SetupCurrentClothingSubPanel() {

		}

		private void SetupClothingSelectionSubPanel() {

		}

	}
}
