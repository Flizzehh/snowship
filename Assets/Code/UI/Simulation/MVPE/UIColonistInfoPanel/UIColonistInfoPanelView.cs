using System;
using System.Collections.Generic;
using Snowship.NUI.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel {
	public class UIColonistInfoPanelView : UIView {

		[Header("Primary Information")]
		[SerializeField] private Image colonistImage; // TODO Turn into prefab to show all layers
		[SerializeField] private TMP_Text colonistNameText;
		[SerializeField] private TMP_Text currentActionText;
		[SerializeField] private TMP_Text storedActionText;
		[SerializeField] private TMP_Text affiliationText;

		[Header("Sliders")]
		[SerializeField] private Slider healthSlider;
		[SerializeField] private Slider moodSlider;
		[SerializeField] private Slider inventoryWeightSlider;
		[SerializeField] private TMP_Text inventoryWeightText;
		[SerializeField] private Slider inventoryVolumeSlider;
		[SerializeField] private TMP_Text inventoryVolumeText;

		[Header("Needs, Moods, Skills Tab")]
		[SerializeField] private Button needsSkillsTabButton;
		[SerializeField] private GameObject needsSkillsTab;

		[Header("Needs Sub-Panel")]
		[SerializeField] private TMP_Text moodCurrentText;
		[SerializeField] private Button moodViewButton;
		[SerializeField] private TMP_Text moodTargetText;
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

		public override void OnOpen() {
			needsSkillsTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(needsSkillsTabButton));
			inventoryTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(inventoryTabButton));
			clothingTabButton.onClick.AddListener(() => OnTabSelected?.Invoke(clothingTabButton));

			OnTabSelected?.Invoke(needsSkillsTabButton);
		}

		public override void OnClose() {

		}

	}
}
