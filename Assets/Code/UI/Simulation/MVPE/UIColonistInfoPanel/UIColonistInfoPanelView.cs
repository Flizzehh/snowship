using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Snowship.NColonist;
using Snowship.NUI.Components;
using Snowship.NUI.Simulation.UIColonistInfoPanel.UIInventoryResource;
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

		private Colonist colonist;

		private readonly List<UINeedElement> needElements = new();
		private readonly List<UIMoodElement> moodElements = new();
		private readonly List<UIResourceAmountElement> inventoryResourceAmountElements = new();

		public override void OnOpen() {
			needsSkillsTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(needsSkillsTabButton));
			inventoryTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(inventoryTabButton));
			clothingTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(clothingTabButton));
		}

		public override void OnClose() {
			base.OnClose();

			colonist.OnHealthChanged -= healthSlider.SetValue;
			colonist.OnMoodChanged -= OnMoodChanged;
			colonist.OnMoodAdded -= OnMoodAdded;
			colonist.OnMoodRemoved -= OnMoodRemoved;
			colonist.GetInventory().OnInventoryChanged -= OnInventoryChanged;

			foreach (UINeedElement needElement in needElements) {
				needElement.Close();
			}
			foreach (UIMoodElement moodElement in moodElements) {
				moodElement.Close();
			}
		}

		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		public void SetColonist(Colonist colonist) {
			this.colonist = colonist;

			colonist.OnMoodAdded += OnMoodAdded;
			colonist.OnMoodRemoved += OnMoodRemoved;
		}

		public void SetupUI() {
			SetupGeneralInformation();
			SetupSliders();
			SetupNeedsSkillsTab();
			SetupInventoryTab();
			SetupClothingTab();
		}

		private void SetupGeneralInformation() {
			colonistImage.sprite = colonist.moveSprites[0];
			colonistNameText.SetText($"{colonist.name} ({colonist.gender.ToString()[0]})");
			affiliationText.SetText($"Colonist of {GameManager.colonyM.colony.name}");
		}

		private void SetupSliders() {
			SetupHealthSlider();
			SetupMoodSlider();
			SetupInventorySliders();
		}

		private void SetupHealthSlider() {
			colonist.OnHealthChanged += healthSlider.SetValue;
			healthSlider.SetFillColours(EColour.DarkRed, EColour.DarkGreen);
			healthSlider.SetHandleColours(EColour.LightRed, EColour.LightGreen);
			healthSlider.SetSliderRange(0, 1, colonist.Health, false);
		}

		private void SetupMoodSlider() {
			colonist.OnMoodChanged += OnMoodChanged;
			moodSlider.SetFillColours(EColour.DarkRed, EColour.DarkGreen);
			moodSlider.SetHandleColours(EColour.LightRed, EColour.LightGreen);
			moodSlider.SetSliderRange(0, 100, colonist.effectiveMood);
			OnMoodChanged(colonist.effectiveMood, colonist.moodModifiersSum);
		}

		private void SetupInventorySliders() {
			ResourceManager.Inventory inventory = colonist.GetInventory();
			inventory.OnInventoryChanged += OnInventoryChanged;
			inventoryWeightSlider.SetSliderRange(0, inventory.maxWeight, inventory.UsedWeight());
			inventoryVolumeSlider.SetSliderRange(0, inventory.maxVolume, inventory.UsedVolume());
			OnInventoryChanged(inventory);
		}

		private void OnInventoryChanged(ResourceManager.Inventory inventory) {
			int usedWeight = inventory.UsedWeight();
			int usedVolume = inventory.UsedVolume();
			inventoryWeightSlider.SetValue(usedWeight, $"{Mathf.RoundToInt(usedWeight / (float)inventory.maxWeight * 100)}");
			inventoryVolumeSlider.SetValue(usedVolume, $"{Mathf.RoundToInt(usedVolume / (float)inventory.maxVolume * 100)}");
		}

		private void SetupNeedsSkillsTab() {
			SetupNeedsSubPanel();
			SetupMoodSubPanel();
			SetupSkillsSubPanel();
		}

		private void SetupNeedsSubPanel() {
			moodViewButton.onClick.AddListener(() => SetMoodPanelActive(true));
			foreach (NeedInstance need in colonist.needs.OrderByDescending(need => need.GetValue())) {
				needElements.Add(new UINeedElement(needsListVerticalLayoutGroup.transform, need));
			}
		}

		private void SetupMoodSubPanel() {
			SetMoodPanelActive(false);
			moodPanelCloseButton.onClick.AddListener(() => SetMoodPanelActive(false));
			foreach (MoodModifierInstance mood in colonist.moodModifiers) {
				AddMoodElement(mood);
			}
		}

		private void SetMoodPanelActive(bool active) {
			moodPanel.SetActive(active);
		}

		private void AddMoodElement(MoodModifierInstance mood) {
			moodElements.Add(new UIMoodElement(moodListVerticalLayoutGroup.transform, mood));
		}

		private void OnMoodAdded(MoodModifierInstance mood) {
			AddMoodElement(mood);
			mood.OnTimerAtZero += OnMoodTimerAtZero;
		}

		private void OnMoodChanged(float effectiveMood, float moodModifiersSum) {
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

		private void OnMoodRemoved(MoodModifierInstance mood) {
			UIMoodElement moodElementToRemove = moodElements.Find(moodElement => moodElement.Mood == mood);
			if (moodElementToRemove == null) {
				return;
			}
			moodElements.Remove(moodElementToRemove);
			moodElementToRemove.Close();
		}

		private void OnMoodTimerAtZero(MoodModifierInstance mood) {
			mood.OnTimerAtZero -= OnMoodTimerAtZero;
			OnMoodRemoved(mood);
		}

		private void SetupSkillsSubPanel() {

		}

		private void SetupInventoryTab() {
			foreach (ResourceManager.ResourceAmount resourceAmount in colonist.GetInventory().resources) {
				inventoryResourceAmountElements.Add(new UIResourceAmountElement(inventoryListGridLayoutGroup.transform, resourceAmount));
			}
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
