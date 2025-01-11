using Snowship.NJob;
using Snowship.NProfession;
using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NPersistence;
using Snowship.NUI.Menu.LoadSave;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Time = Snowship.NTime;

namespace Snowship.NUI {
	public class UIManagerOld : IManager {
		// Old Code

		private GameManager startCoroutineReference;

		public void SetStartCoroutineReference(GameManager startCoroutineReference) {
			this.startCoroutineReference = startCoroutineReference;
		}

		public readonly string gameVersionString = $"Snowship {PersistenceManager.gameVersion.text}";

		public bool playerTyping = false;

		private int screenWidth = 0;
		private int screenHeight = 0;

		public GameObject canvas;

		public void SetupUI() {
			screenWidth = Screen.width;
			screenHeight = Screen.height;

			canvas = GameObject.Find("Canvas");

			// SetupMainMenu();
			//
			// SetupLoadUniverseUI();
			// SetupCreateUniverseUI();
			// SetupLoadPlanetUI();
			// SetupCreatePlanetUI();
			// SetupLoadColonyUI();
			// SetupCreateColonyUI();
			// SetupLoadSaveUI();

			SetupLoadingScreen();

			SetupGameUI();

			//SetupPauseMenu();

			// SetLoadUniverseActive(false);
			// SetCreateUniverseActive(false);
			// SetLoadPlanetActive(false);
			// SetCreatePlanetActive(false);
			// SetLoadColonyActive(false);
			// SetCreateColonyActive(false);
			// SetLoadSaveActive(false);

			SetSettingsMenuActive(false);

			SetPauseMenuActive(false);

			InitializeTileInformation();
			InitializeSelectedContainerIndicator();
			InitializeSelectedTradingPostIndicator();
			InitializeSelectedCraftingObjectIndicator();

			CreateActionsPanel();

			CreateProfessionsMenu();
			CreateClothesList();
			CreateResourcesList();

			SetSelectedColonistInformation(false);
			SetSelectedColonistTab(selectedColonistNeedsSkillsTabButton);
			SetSelectedTraderMenu();
			SetSelectedContainerInfo();
			SetSelectedTradingPostInfo();
			SetSelectedCraftingObjectPanel();

			// TODO SetTradeMenu();

			SetCaravanElements();
			SetJobElements();

			SetGameUIActive(false);
			SetLoadingScreenActive(false);
		}

		/*
		private GameObject loadPlanetPanel;
		private Transform planetsListPanel;
		private Button loadPlanetButton;
		private readonly List<PlanetElement> planetElements = new List<PlanetElement>();
		private PlanetElement selectedPlanetElement;

		private void SetSelectedPlanetElement(PlanetElement planetElement) {
			selectedPlanetElement = planetElement;

			loadPlanetButton.interactable = selectedPlanetElement != null;
			if (selectedPlanetElement != null) {
				loadPlanetButton.transform.Find("Text").GetComponent<Text>().text =
					"Load " + selectedPlanetElement.persistencePlanet.name;
			}
			else {
				loadPlanetButton.transform.Find("Text").GetComponent<Text>().text = "Select a Planet to Load";
			}
		}

		private void SetupLoadPlanetUI() {
			loadPlanetPanel = mainMenu.transform.Find("LoadPlanet-Panel").gameObject;

			Button backButton = loadPlanetPanel.transform.Find("Back-Button").GetComponent<Button>();
			backButton.onClick.AddListener(delegate {
				GameManager.planetM.SetPlanet(null);
				GameManager.colonyM.SetColony(null);

				SetSelectedPlanetElement(null);

				SetLoadPlanetActive(false);
				SetLoadUniverseActive(true);
			});

			Button createPlanetButton = loadPlanetPanel.transform.Find("CreatePlanet-Button").GetComponent<Button>();
			createPlanetButton.onClick.AddListener(delegate {
				GameManager.planetM.SetPlanet(null);
				GameManager.colonyM.SetColony(null);

				SetSelectedPlanetElement(null);

				SetLoadPlanetActive(false);
				SetCreatePlanetActive(true);
			});

			planetsListPanel = loadPlanetPanel.transform.Find("Planets-ScrollPanel/PlanetsList-Panel");

			loadPlanetButton = loadPlanetPanel.transform.Find("LoadPlanet-Button").GetComponent<Button>();
			loadPlanetButton.onClick.AddListener(delegate {
				if (selectedPlanetElement != null) {
					GameManager.persistenceM.ApplyLoadedPlanet(selectedPlanetElement.persistencePlanet);
					GameManager.colonyM.SetColony(null);

					SetSelectedPlanetElement(null);

					SetLoadPlanetActive(false);
					SetLoadColonyActive(true);
				}
			});
			SetSelectedPlanetElement(null);
		}

		private void SetLoadPlanetActive(bool active) {
			loadPlanetPanel.SetActive(active);
			ToggleMainMenuButtons(loadPlanetPanel);

			foreach (PlanetElement planetElement in planetElements) {
				planetElement.Delete();
			}

			planetElements.Clear();

			if (loadPlanetPanel.activeSelf) {
				foreach (PersistenceManager.PersistencePlanet persistencePlanet in GameManager.persistenceM
							.GetPersistencePlanets()) {
					planetElements.Add(new PlanetElement(persistencePlanet, planetsListPanel));
				}
			}
		}
		*/

		private Text loadingStateText;
		private Text subLoadingStateText;

		private void SetupLoadingScreen() {
			loadingStateText = canvas.transform.Find("LoadingScreen/LoadingState-Text").GetComponent<Text>();
			subLoadingStateText = canvas.transform.Find("LoadingScreen/SubLoadingState-Text").GetComponent<Text>();
			SetLoadingScreenActive(false);
		}

		private GameObject gameUI;

		public static Vector2 mousePosition;
		public TileManager.Tile mouseOverTile;

		private GameObject tileInformation;

		private GameObject colonistsPanel;
		private GameObject colonistListPanel;

		private GameObject caravansPanel;
		private GameObject caravanListPanel;

		private GameObject jobsPanel;
		private GameObject jobListPanel;

		private GameObject selectedColonistInformationPanel;

		private GameObject selectedColonistNeedsSkillsTabButton;
		private GameObject selectedColonistNeedsSkillsTabButtonLinkPanel;
		private GameObject selectedColonistInventoryTabButton;
		private GameObject selectedColonistInventoryTabButtonLinkPanel;
		private GameObject selectedColonistClothingTabButton;
		private GameObject selectedColonistClothingTabButtonLinkPanel;

		private GameObject selectedColonistNeedsSkillsPanel;
		private GameObject selectedColonistMoodModifiersPanel;
		private GameObject selectedColonistInventoryPanel;
		private GameObject selectedColonistClothingPanel;
		private GameObject selectedColonistClothingSelectionPanel;
		private KeyValuePair<GameObject, GameObject> availableClothingTitleAndList;
		private KeyValuePair<GameObject, GameObject> takenClothingTitleAndList;

		private GameObject selectedTraderMenu;
		private GameObject tradeMenu;

		private GameObject dateTimeInformationPanel;

		private SelectionSizePanel selectionSizePanel;

		public SelectionSizePanel GetSelectionSizePanel() {
			return selectionSizePanel;
		}

		private GameObject selectedContainerIndicator;
		private GameObject selectedContainerInventoryPanel;

		private GameObject selectedTradingPostIndicator;
		private GameObject selectedTradingPostPanel;

		private GameObject professionsMenuButton;
		private GameObject professionsMenu;

		private GameObject objectsMenuButton;
		private GameObject objectPrefabsList;

		private GameObject clothesMenuButton;
		private GameObject clothesMenuPanel;
		private InputField clothesSearchInputField;
		private GameObject clothesList;

		private GameObject resourcesMenuButton;
		private GameObject resourcesMenuPanel;
		private InputField resourcesSearchInputField;
		private GameObject resourcesList;

		private GameObject selectedCraftingObjectIndicator;
		private GameObject selectedCraftingObjectPanel;
		private Button selectedCraftingObjectPanelSelectResourcesButton;
		private GameObject selectedCraftingObjectSelectResourcesPanel;
		private Button selectedCraftingObjectPanelSelectFuelsButton;
		private GameObject selectedCraftingObjectSelectFuelsPanel;
		private Button selectedCraftingObjectPanelActiveButton;
		private Text selectedCraftingObjectPanelActiveButtonText;

		private void SetupGameUI() {
			gameUI = canvas.transform.Find("Game-BackgroundPanel").gameObject;

			gameUI.transform.Find("Disclaimer-Text").GetComponent<Text>().text = gameVersionString;

			tileInformation = gameUI.transform.Find("TileInformation-Panel").gameObject;

			colonistsPanel = gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel/Colonists-Panel").gameObject;
			colonistListPanel = colonistsPanel.transform.Find("ColonistList-Panel").gameObject;
			colonistsPanel.transform.Find("ColonistsTitle-Panel/CollapseList-Button").GetComponent<Button>().onClick.AddListener(delegate { SetListPanelCollapsed(colonistListPanel, colonistsPanel.transform.Find("ColonistsTitle-Panel/CollapseList-Button/Arrow-Text").GetComponent<Text>()); });

			caravansPanel = gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel/Caravans-Panel").gameObject;
			caravanListPanel = caravansPanel.transform.Find("CaravanList-Panel").gameObject;
			caravansPanel.transform.Find("CaravansTitle-Panel/CollapseList-Button").GetComponent<Button>().onClick.AddListener(delegate { SetListPanelCollapsed(caravanListPanel, caravansPanel.transform.Find("CaravansTitle-Panel/CollapseList-Button/Arrow-Text").GetComponent<Text>()); });

			jobsPanel = gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel/Jobs-Panel").gameObject;
			jobListPanel = jobsPanel.transform.Find("JobList-Panel").gameObject;
			jobsPanel.transform.Find("JobsTitle-Panel/CollapseList-Button").GetComponent<Button>().onClick.AddListener(delegate { SetListPanelCollapsed(jobListPanel, jobsPanel.transform.Find("JobsTitle-Panel/CollapseList-Button/Arrow-Text").GetComponent<Text>()); });

			selectedColonistInformationPanel = gameUI.transform.Find("SelectedColonistInfo-Panel").gameObject;

			selectedColonistNeedsSkillsPanel = selectedColonistInformationPanel.transform.Find("SelectedTab-Panel/NeedsSkills-Panel").gameObject;
			selectedColonistMoodModifiersPanel = selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodModifier-Panel").gameObject;
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodModifiers-Button").GetComponent<Button>().onClick.AddListener(delegate { selectedColonistMoodModifiersPanel.SetActive(!selectedColonistMoodModifiersPanel.activeSelf); });
			selectedColonistMoodModifiersPanel.transform.Find("Return-Button").GetComponent<Button>().onClick.AddListener(delegate { selectedColonistMoodModifiersPanel.SetActive(!selectedColonistMoodModifiersPanel.activeSelf); });
			selectedColonistMoodModifiersPanel.SetActive(false);

			selectedColonistInventoryPanel = selectedColonistInformationPanel.transform.Find("SelectedTab-Panel/Inventory-Panel").gameObject;

			selectedColonistClothingPanel = selectedColonistInformationPanel.transform.Find("SelectedTab-Panel/Clothing-Panel").gameObject;
			selectedColonistClothingSelectionPanel = selectedColonistClothingPanel.transform.Find("ClothingSelection-Panel").gameObject;
			selectedColonistClothingSelectionPanel.transform.Find("Return-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistClothingSelectionPanelActive(!selectedColonistClothingSelectionPanel.activeSelf); });
			availableClothingTitleAndList = new KeyValuePair<GameObject, GameObject>(selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesAvailableTitle-Panel").gameObject, selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesAvailable-Panel").gameObject);
			takenClothingTitleAndList = new KeyValuePair<GameObject, GameObject>(selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesTakenTitle-Panel").gameObject, selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesTaken-Panel").gameObject);

			selectedColonistNeedsSkillsTabButton = selectedColonistInformationPanel.transform.Find("TabButton-Panel/NeedsSkills-Button").gameObject;
			selectedColonistNeedsSkillsTabButton.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistTab(selectedColonistNeedsSkillsTabButton); });
			selectedColonistNeedsSkillsTabButtonLinkPanel = selectedColonistNeedsSkillsTabButton.transform.Find("Link-Panel").gameObject;
			selectedColonistInventoryTabButton = selectedColonistInformationPanel.transform.Find("TabButton-Panel/Inventory-Button").gameObject;
			selectedColonistInventoryTabButton.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistTab(selectedColonistInventoryTabButton); });
			selectedColonistInventoryTabButtonLinkPanel = selectedColonistInventoryTabButton.transform.Find("Link-Panel").gameObject;
			selectedColonistClothingTabButton = selectedColonistInformationPanel.transform.Find("TabButton-Panel/Clothing-Button").gameObject;
			selectedColonistClothingTabButton.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedColonistTab(selectedColonistClothingTabButton); });
			selectedColonistClothingTabButtonLinkPanel = selectedColonistClothingTabButton.transform.Find("Link-Panel").gameObject;

			selectedTraderMenu = gameUI.transform.Find("SelectedTraderMenu-Panel").gameObject;
			// tradeMenu = gameUI.transform.Find("TradeMenu-Panel").gameObject;
			// tradeMenu.transform.Find("Close-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeMenuActive(false); });
			// tradeMenu.transform.Find("ConfirmTrade-Button").GetComponent<Button>().onClick.AddListener(delegate { ConfirmTrade(); });

			dateTimeInformationPanel = gameUI.transform.Find("DateTimeInformation-Panel").gameObject;

			selectionSizePanel = new SelectionSizePanel(GameObject.Find("SelectionSize-Canvas").GetComponent<Canvas>());

			selectedContainerInventoryPanel = gameUI.transform.Find("SelectedContainerInventory-Panel").gameObject;

			selectedTradingPostPanel = gameUI.transform.Find("SelectedTradingPost-Panel").gameObject;

			professionsMenuButton = gameUI.transform.Find("Management-Panel/ProfessionsMenu-Button").gameObject;
			professionsMenu = professionsMenuButton.transform.Find("ProfessionsMenu-Panel").gameObject;
			professionsMenuButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleProfessionsMenu(); });

			objectsMenuButton = gameUI.transform.Find("Management-Panel/ObjectsMenu-Button").gameObject;
			objectPrefabsList = objectsMenuButton.transform.Find("ObjectPrefabsList-ScrollPanel").gameObject;
			objectsMenuButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleObjectPrefabsList(true); });
			objectPrefabsList.SetActive(false);

			clothesMenuButton = gameUI.transform.Find("Management-Panel/ClothesMenu-Button").gameObject;
			clothesMenuPanel = clothesMenuButton.transform.Find("ClothesMenu-Panel").gameObject;
			clothesSearchInputField = clothesMenuPanel.transform.Find("ClothesSearch-InputField").GetComponent<InputField>();
			clothesSearchInputField.onValueChanged.AddListener(delegate { FilterClothesList(clothesSearchInputField.text); });
			clothesList = clothesMenuPanel.transform.Find("ClothesList-ScrollPanel").gameObject;
			clothesMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetClothesList(); });
			clothesMenuPanel.SetActive(false);

			resourcesMenuButton = gameUI.transform.Find("Management-Panel/ResourcesMenu-Button").gameObject;
			resourcesMenuPanel = resourcesMenuButton.transform.Find("ResourcesMenu-Panel").gameObject;
			resourcesSearchInputField = resourcesMenuPanel.transform.Find("ResourcesSearch-InputField").GetComponent<InputField>();
			resourcesSearchInputField.onValueChanged.AddListener(delegate { FilterResourcesList(resourcesSearchInputField.text); });
			resourcesList = resourcesMenuPanel.transform.Find("ResourcesList-ScrollPanel").gameObject;
			resourcesMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetResourcesList(); });
			resourcesMenuPanel.SetActive(false);

			selectedCraftingObjectPanel = gameUI.transform.Find("SelectedCraftingObject-Panel").gameObject;
			selectedCraftingObjectSelectResourcesPanel = selectedCraftingObjectPanel.transform.Find("SelectResources-Panel").gameObject;
			selectedCraftingObjectSelectFuelsPanel = selectedCraftingObjectPanel.transform.Find("SelectFuels-Panel").gameObject;

			selectedCraftingObjectPanelSelectResourcesButton = selectedCraftingObjectPanel.transform.Find("Settings-Panel/SelectResources-Button").GetComponent<Button>();
			selectedCraftingObjectPanelSelectResourcesButton.onClick.AddListener(delegate { SetSelectedCraftingObjectSelectResourcesPanel(); });
			selectedCraftingObjectSelectResourcesPanel.transform.Find("Close-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSelectedCraftingObjectSelectResourcesPanel(); });

			selectedCraftingObjectPanelSelectFuelsButton = selectedCraftingObjectPanel.transform.Find("Settings-Panel/SelectFuels-Button").GetComponent<Button>();
			selectedCraftingObjectPanelSelectFuelsButton.onClick.AddListener(delegate { SetSelectedCraftingObjectSelectFuelsPanel(); });
			selectedCraftingObjectSelectFuelsPanel.transform.Find("Close-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSelectedCraftingObjectSelectFuelsPanel(); });

			selectedCraftingObjectPanelActiveButton = selectedCraftingObjectPanel.transform.Find("Settings-Panel/ActiveValueToggle-Button").GetComponent<Button>();
			selectedCraftingObjectPanelActiveButton.onClick.AddListener(delegate { selectedCraftingObject.SetActive(!selectedCraftingObject.active); });

			selectedCraftingObjectPanelActiveButtonText = selectedCraftingObjectPanelActiveButton.transform.Find("ActiveValue-Text").GetComponent<Text>();
		}

		public void Update() {
			if (GameManager.tileM.mapState == TileManager.MapState.Generated) {
				mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
				TileManager.Tile newMouseOverTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
				if (newMouseOverTile != mouseOverTile) {
					mouseOverTile = newMouseOverTile;
					//if (!pauseMenu.activeSelf) {
					UpdateTileInformation();
					//}
				}

				if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
					if (GameManager.jobM.firstTile != null) {
						GameManager.jobM.StopSelection();
					} else {
						if (GameManager.jobM.GetSelectedPrefab() != null) {
							GameManager.jobM.SetSelectedPrefab(null, null);
						} else {
							if (!playerTyping) {
								if (!Input.GetMouseButtonDown(1)) {
									//SetPauseMenuActive(!pauseMenu.activeSelf);
								}
							}
						}
					}
				}

				UpdateSelectedColonistInformation();
				UpdateSelectedTraderMenu();
				// UpdateTradeMenu();

				UpdateColonistElements();
				UpdateCaravanElements();
				UpdateJobElements();

				if (selectedContainer != null) {
					if (Input.GetMouseButtonDown(1)) {
						SetSelectedContainer(null);
					}

					UpdateSelectedContainerInfo();
				}

				if (selectedCraftingObject != null) {
					UpdateSelectedCraftingObjectPanel();
				}

				if (selectedTradingPost != null) {
					if (Input.GetMouseButtonDown(1)) {
						SetSelectedTradingPost(null);
					}

					UpdateSelectedTradingPostInfo();
				}

				if (selectedCraftingObject != null) {
					if (Input.GetMouseButtonDown(1) && !GetUIElementsUnderPointer().Find(result => result.gameObject.name.Contains("Priority")).isValid) {
						SetSelectedCraftingObject(null);
					}
				}

				if (Input.GetMouseButtonDown(0) && !IsPointerOverUI()) {
					ResourceManager.Container container = GameManager.resourceM.containers.Find(findContainer => findContainer.tile == newMouseOverTile || findContainer.additionalTiles.Contains(newMouseOverTile));
					if (container != null) {
						SetSelectedContainer(container);
					}

					ResourceManager.TradingPost tradingPost = GameManager.resourceM.tradingPosts.Find(tp => tp.tile == newMouseOverTile || tp.additionalTiles.Contains(newMouseOverTile));
					if (tradingPost != null) {
						SetSelectedTradingPost(tradingPost);
					}

					ResourceManager.CraftingObject mto = GameManager.resourceM.craftingObjectInstances.Find(mtoi => mtoi.tile == newMouseOverTile || mtoi.additionalTiles.Contains(newMouseOverTile));
					if (mto != null) {
						SetSelectedCraftingObject(mto);
					}
				}

				if (professionsMenu.activeSelf) {
					UpdateProfessionsMenu();
				}

				if (clothesMenuPanel.activeSelf) {
					UpdateClothesList();
				}

				if (resourcesMenuPanel.activeSelf) {
					UpdateResourcesList();
				}

				UpdateObjectPrefabButtons();
			}
			/*else {
				if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) &&
					GameManager.tileM.mapState != TileManager.MapState.Generating && !playerTyping) {
					if (loadUniversePanel.activeSelf) {
						SetSelectedUniverseElement(null);
					}

					if (loadPlanetPanel.activeSelf) {
						SetSelectedPlanetElement(null);
					}

					if (loadColonyPanel.activeSelf) {
						SetSelectedColonyElement(null);
					}

					if (loadSavePanel.activeSelf) {
						SetSelectedSaveElement(null);
					}

					SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);
				}

				UpdateMainMenuBackground();
			}*/

			playerTyping = IsPlayerTyping();
		}

		private bool IsPlayerTyping() {
			/*if (universeNameInputField.isFocused) {
				return true;
			}

			if (planetRegenerationCodeInputField.isFocused) {
				return true;
			}

			if (planetNameInputField.isFocused) {
				return true;
			}

			if (planetSeedInputField.isFocused) {
				return true;
			}

			if (mapRegenerationCodeInputField.isFocused) {
				return true;
			}

			if (colonyNameInputField.isFocused) {
				return true;
			}

			if (mapSeedInputField.isFocused) {
				return true;
			}*/

			if (GameManager.debugM.debugInput.isFocused) {
				return true;
			}

			if (clothesSearchInputField.isFocused) {
				return true;
			}

			if (resourcesSearchInputField.isFocused) {
				return true;
			}

			return false;
		}

		public ResourceManager.Container selectedContainer;

		public void SetSelectedContainer(ResourceManager.Container container) {
			selectedContainer = null;
			if (selectedTradingPost != null) {
				SetSelectedTradingPost(null);
			}

			if (selectedCraftingObject != null) {
				SetSelectedCraftingObject(null);
			}

			selectedContainer = container;
			SetSelectedContainerInfo();
		}

		public ResourceManager.TradingPost selectedTradingPost;

		public void SetSelectedTradingPost(ResourceManager.TradingPost tradingPost) {
			selectedTradingPost = null;
			if (selectedContainer != null) {
				SetSelectedContainer(null);
			}

			if (selectedCraftingObject != null) {
				SetSelectedCraftingObject(null);
			}

			selectedTradingPost = tradingPost;
			SetSelectedTradingPostInfo();
		}

		public ResourceManager.CraftingObject selectedCraftingObject;

		public void SetSelectedCraftingObject(ResourceManager.CraftingObject newSelectedCraftingObject) {
			selectedCraftingObject = null;
			if (selectedContainer != null) {
				SetSelectedContainer(null);
			}

			if (selectedTradingPost != null) {
				SetSelectedTradingPost(null);
			}

			selectedCraftingObject = newSelectedCraftingObject;
			SetSelectedCraftingObjectPanel();
		}


		private void ExitToDesktop() {
			Application.Quit();
		}

		public void UpdateLoadingStateText(string primaryText, string secondaryText) {
			if (loadingStateText.gameObject.activeSelf) {
				loadingStateText.text = primaryText;
			}

			if (subLoadingStateText.gameObject.activeSelf) {
				subLoadingStateText.text = secondaryText.ToUpper();
			}
		}

		public void SetLoadingScreenActive(bool active) {
			loadingStateText.transform.parent.gameObject.SetActive(active);
		}

		public void SetGameUIActive(bool state) {
			gameUI.SetActive(state);
		}

		private class MenuButton {
			public object referenceObject;
			public Transform parent;

			public GameObject buttonPanel;
			public Button button;
			public Text text;
			public GameObject panel;

			public List<object> childButtons = new List<object>();

			public MenuButton(object referenceObject, Transform parent, GameObject buttonPrefab, string buttonText) {
				this.referenceObject = referenceObject;
				this.parent = parent;

				buttonPanel = Object.Instantiate(buttonPrefab, parent, false);
				buttonPanel.name = buttonText + " (" + buttonPrefab.name + ")";
				button = buttonPanel.transform.Find("Button").GetComponent<Button>();
				text = button.transform.Find("Text").GetComponent<Text>();
				text.text = buttonText;
				panel = buttonPanel.transform.Find("Panel").gameObject;

				button.onClick.AddListener(delegate { SetPanelActive(!panel.activeSelf); });
				SetPanelActive(false);
			}

			public void AddChildButton(object menuButton) {
				childButtons.Add(menuButton);
			}

			public void SetPanelActive(bool active) {
				panel.SetActive(active);
				if (!panel.activeSelf) {
					foreach (MenuButton menuButton in childButtons.Where(cb => cb is MenuButton)) {
						menuButton.SetPanelActive(false);
					}
				}
			}
		}

		private class ObjectPrefabButton {
			public ResourceManager.ObjectPrefab prefab;
			public GameObject obj;
			public Transform parent;

			public GameObject requiredResourceElementsPanel;
			public List<RequiredResourceElement> requiredResourceElements = new List<RequiredResourceElement>();
			public GameObject variationElementsPanel;
			public List<VariationElement> variationElements = new List<VariationElement>();

			public GameObject variationsIndicator;

			public ObjectPrefabButton(ResourceManager.ObjectPrefab prefab, Transform parent) {
				this.prefab = prefab;
				this.parent = parent;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"), parent, false);

				ResourceManager.Variation variation = prefab.lastSelectedVariation;
				obj.transform.Find("Text").GetComponent<Text>().text = prefab.GetInstanceNameFromVariation(variation);
				obj.transform.Find("Image").GetComponent<Image>().sprite = prefab.GetBaseSpriteForVariation(variation);

				requiredResourceElementsPanel = obj.transform.Find("RequiredResources-Panel").gameObject;
				variationElementsPanel = obj.transform.Find("Variations-Panel").gameObject;

				variationsIndicator = obj.transform.Find("VariationsIndicator-Image").gameObject;

				obj.GetComponent<HoverToggleScript>().Initialize(requiredResourceElementsPanel, true, prefab.variations.Count > 0 ? new List<GameObject>() { variationElementsPanel } : null);

				SetVariation(prefab.lastSelectedVariation);

				SetVariationElements();
				UpdateVariationElements();

				obj.GetComponent<HandleClickScript>().Initialize(delegate { GameManager.jobM.SetSelectedPrefab(prefab, prefab.lastSelectedVariation); }, null, delegate {
					if (prefab.variations.Count > 0) {
						variationElementsPanel.SetActive(!variationElementsPanel.activeSelf);
						requiredResourceElementsPanel.SetActive(!variationElementsPanel.activeSelf);
					}
				});
			}

			public void Update() {
				UpdateRequiredResourceElements();
				UpdateVariationElements();
				UpdateVariationsIndicator();
			}

			public void SetVariation(ResourceManager.Variation variation) {
				prefab.SetVariation(variation);

				SetRequiredResourceElements();
				UpdateRequiredResourceElements();

				obj.transform.Find("Text").GetComponent<Text>().text = prefab.GetInstanceNameFromVariation(prefab.lastSelectedVariation);
				obj.transform.Find("Image").GetComponent<Image>().sprite = prefab.GetBaseSpriteForVariation(prefab.lastSelectedVariation);

				variationElementsPanel.SetActive(false);
			}

			private void SetRequiredResourceElements() {
				foreach (RequiredResourceElement requiredResourceElement in requiredResourceElements) {
					requiredResourceElement.Destroy();
				}

				requiredResourceElements.Clear();

				foreach (ResourceManager.ResourceAmount resourceAmount in prefab.commonResources) {
					requiredResourceElements.Add(new RequiredResourceElement(resourceAmount, requiredResourceElementsPanel.transform));
				}

				if (prefab.lastSelectedVariation != null) {
					foreach (ResourceManager.ResourceAmount resourceAmount in prefab.lastSelectedVariation.uniqueResources) {
						requiredResourceElements.Add(new RequiredResourceElement(resourceAmount, requiredResourceElementsPanel.transform));
					}
				}
			}

			private void SetVariationElements() {
				foreach (VariationElement variationElement in variationElements) {
					variationElement.Destroy();
				}

				variationElements.Clear();

				foreach (ResourceManager.Variation variation in prefab.variations) {
					variationElements.Add(new VariationElement(variation, variationElementsPanel.transform, this));
				}

				variationsIndicator.SetActive(prefab.variations.Count > 0);
			}

			private void UpdateRequiredResourceElements() {
				foreach (RequiredResourceElement requiredResourceElement in requiredResourceElements) {
					requiredResourceElement.Update();
				}
			}

			private void UpdateVariationElements() {
				foreach (VariationElement variationElement in variationElements) {
					variationElement.Update();
				}
			}

			private void UpdateVariationsIndicator() {
				bool requiredResourcesMet = false;
				if (prefab.commonResources.Count > 0) {
					foreach (ResourceManager.ResourceAmount resourceAmount in prefab.commonResources) {
						requiredResourcesMet = resourceAmount.amount <= resourceAmount.resource.GetAvailableAmount();
						if (!requiredResourcesMet) {
							break;
						}
					}
				} else {
					requiredResourcesMet = true;
				}

				bool anyVariationResourcesMet = false;
				if (prefab.variations.Count > 0) {
					foreach (ResourceManager.Variation variation in prefab.variations) {
						bool variationResourcesMet = false;
						if (variation.uniqueResources.Count > 0) {
							foreach (ResourceManager.ResourceAmount resourceAmount in variation.uniqueResources) {
								variationResourcesMet = resourceAmount.amount <= resourceAmount.resource.GetAvailableAmount();
								if (!variationResourcesMet) {
									break;
								}
							}
						} else {
							variationResourcesMet = true;
						}

						if (variationResourcesMet) {
							anyVariationResourcesMet = true;
							break;
						}
					}
				} else {
					anyVariationResourcesMet = true;
				}

				variationsIndicator.GetComponent<Image>().color = requiredResourcesMet && anyVariationResourcesMet ? ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen) : ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
			}
		}

		private class RequiredResourceElement {
			public ResourceManager.ResourceAmount resourceAmount;
			public GameObject obj;

			public RequiredResourceElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
				this.resourceAmount = resourceAmount;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), parent, false);

				obj.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resourceAmount.resource.image;
				obj.transform.Find("ResourceName-Text").GetComponent<Text>().text = resourceAmount.resource.name;

				Update();
			}

			public void Update() {
				int availableAmount = resourceAmount.resource.GetAvailableAmount();

				obj.GetComponent<Image>().color = availableAmount >= resourceAmount.amount ? ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen) : ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);

				obj.transform.Find("AvailableOverRequiredValue-Text").GetComponent<Text>().text = availableAmount + " / " + resourceAmount.amount;
			}

			public void Destroy() {
				Object.Destroy(obj);
			}
		}

		private class VariationElement {
			public ResourceManager.Variation variation;
			public GameObject obj;

			public GameObject requiredResourceElementsPanel;
			public List<RequiredResourceElement> requiredResourceElements = new List<RequiredResourceElement>();

			public VariationElement(ResourceManager.Variation variation, Transform parent, ObjectPrefabButton prefabElement) {
				this.variation = variation;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildVariation-Button-Prefab"), parent, false);
				obj.name = variation.name;

				obj.transform.Find("Text").GetComponent<Text>().text = variation.name;
				obj.transform.Find("Image").GetComponent<Image>().sprite = variation.prefab.GetBaseSpriteForVariation(variation);

				requiredResourceElementsPanel = obj.transform.Find("RequiredResources-Panel").gameObject;
				obj.GetComponent<HoverToggleScript>().Initialize(requiredResourceElementsPanel, true, null);
				SetRequiredResourceElements();
				UpdateRequiredResourceElements();

				obj.GetComponent<HandleClickScript>().Initialize(delegate {
					prefabElement.SetVariation(variation);
					requiredResourceElementsPanel.SetActive(false);
					GameManager.jobM.SetSelectedPrefab(prefabElement.prefab, variation);
				}, null, delegate {
					prefabElement.variationElementsPanel.SetActive(false);
					requiredResourceElementsPanel.SetActive(false);
				});
			}

			public void Update() {
				UpdateRequiredResourceElements();
			}

			public void SetRequiredResourceElements() {
				foreach (RequiredResourceElement requiredResourceElement in requiredResourceElements) {
					requiredResourceElement.Destroy();
				}

				requiredResourceElements.Clear();

				foreach (ResourceManager.ResourceAmount resourceAmount in variation.prefab.commonResources) {
					requiredResourceElements.Add(new RequiredResourceElement(resourceAmount, requiredResourceElementsPanel.transform));
				}

				foreach (ResourceManager.ResourceAmount resourceAmount in variation.uniqueResources) {
					requiredResourceElements.Add(new RequiredResourceElement(resourceAmount, requiredResourceElementsPanel.transform));
				}
			}

			public void UpdateRequiredResourceElements() {
				foreach (RequiredResourceElement requiredResourceElement in requiredResourceElements) {
					requiredResourceElement.Update();
				}
			}

			public void Destroy() {
				Object.Destroy(obj);
			}
		}

		private static readonly List<ResourceManager.ObjectGroupEnum> separateMenuGroups = new List<ResourceManager.ObjectGroupEnum>() {
			ResourceManager.ObjectGroupEnum.Farm,
			ResourceManager.ObjectGroupEnum.Terraform,
			ResourceManager.ObjectGroupEnum.Command
		};

		private static readonly List<ResourceManager.ObjectGroupEnum> noMenuGroups = new List<ResourceManager.ObjectGroupEnum>() {
			ResourceManager.ObjectGroupEnum.None
		};

		private readonly List<ObjectPrefabButton> objectPrefabButtons = new List<ObjectPrefabButton>();

		public void CreateActionsPanel() {
			Transform actionsPanel = gameUI.transform.Find("Actions-Panel");

			List<MenuButton> topLevelButtons = new List<MenuButton>();

			MenuButton buildButton = new MenuButton(null, actionsPanel, Resources.Load<GameObject>(@"UI/UIElements/BuildMenu-Panel-Prefab"), "Build");
			topLevelButtons.Add(buildButton);

			foreach (ResourceManager.ObjectPrefabGroup group in GameManager.resourceM.GetObjectPrefabGroups()) {
				if (noMenuGroups.Contains(group.type)) {
					continue;
				}

				bool separateMenu = separateMenuGroups.Contains(group.type);

				MenuButton groupButton = new MenuButton(group, separateMenu ? actionsPanel : buildButton.panel.transform, separateMenu ? Resources.Load<GameObject>(@"UI/UIElements/BuildMenu-Panel-Prefab") : Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Panel-Prefab"), group.name);

				if (!separateMenu) {
					buildButton.AddChildButton(groupButton);
				} else {
					topLevelButtons.Add(groupButton);
				}

				foreach (ResourceManager.ObjectPrefabSubGroup subGroup in group.subGroups) {
					MenuButton subGroupButton = new MenuButton(subGroup, groupButton.panel.transform, Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Panel-Prefab"), subGroup.name);

					groupButton.AddChildButton(subGroupButton);

					foreach (ResourceManager.ObjectPrefab prefab in subGroup.prefabs) {
						ObjectPrefabButton objectPrefabButton = new ObjectPrefabButton(prefab, subGroupButton.panel.transform);

						subGroupButton.AddChildButton(objectPrefabButton);

						objectPrefabButtons.Add(objectPrefabButton);
					}
				}

				List<MenuButton> subGroupButtons = groupButton.childButtons.Where(cb => cb is MenuButton).Cast<MenuButton>().ToList();
				foreach (MenuButton subGroupButton1 in subGroupButtons) {
					subGroupButton1.button.onClick.AddListener(delegate {
						foreach (MenuButton subGroupButton2 in subGroupButtons) {
							if (subGroupButton2 != subGroupButton1) {
								subGroupButton2.SetPanelActive(false);
							}
						}
					});
				}
			}

			List<MenuButton> buildButtonChildButtons = buildButton.childButtons.Where(cb => cb is MenuButton).Cast<MenuButton>().ToList();
			foreach (MenuButton buildButtonChildButton1 in buildButtonChildButtons) {
				buildButtonChildButton1.button.onClick.AddListener(delegate {
					foreach (MenuButton buildButtonChildButton2 in buildButtonChildButtons) {
						if (buildButtonChildButton2 != buildButtonChildButton1) {
							buildButtonChildButton2.SetPanelActive(false);
						}
					}
				});
			}

			foreach (MenuButton topLevelButton1 in topLevelButtons) {
				topLevelButton1.button.onClick.AddListener(delegate {
					foreach (MenuButton topLevelButton2 in topLevelButtons) {
						if (topLevelButton2 != topLevelButton1) {
							topLevelButton2.SetPanelActive(false);
						}
					}
				});
			}
		}

		public void UpdateObjectPrefabButtons() {
			foreach (ObjectPrefabButton objectPrefabButton in objectPrefabButtons) {
				if (objectPrefabButton.obj.activeSelf) {
					objectPrefabButton.Update();
				}
			}
		}

		public void InitializeTileInformation() {
			tileRoofElement = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-Label-Panel"), tileInformation.transform, false);
		}

		private GameObject tileRoofElement = null;
		private readonly List<GameObject> tileResourceElements = new List<GameObject>();
		private readonly List<GameObject> plantObjectElements = new List<GameObject>();
		private readonly Dictionary<int, List<GameObject>> objectElements = new Dictionary<int, List<GameObject>>();

		public static readonly Dictionary<int, string> layerToLayerNameMap = new Dictionary<int, string>() {
			{ 0, "Job" },
			{ 1, "Floor" },
			{ 2, "Object" }
		};

		public void UpdateTileInformation() {
			if (mouseOverTile != null) {
				tileRoofElement.SetActive(false);

				foreach (GameObject tileResourceElement in tileResourceElements) {
					tileResourceElement.SetActive(false);
				}

				foreach (GameObject plantObjectElement in plantObjectElements) {
					plantObjectElement.SetActive(false);
				}

				foreach (KeyValuePair<int, List<GameObject>> objectElementKVP in objectElements) {
					foreach (GameObject objectDataElement in objectElementKVP.Value) {
						objectDataElement.SetActive(false);
					}
				}

				if (mouseOverTile.visible) {
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage").GetComponent<Image>().sprite = mouseOverTile.sr.sprite;
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage").GetComponent<Image>().color = Color.white;

					string tileTypeString = mouseOverTile.tileType.name;
					if (mouseOverTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]) {
						tileTypeString = "Water";
					} else if (mouseOverTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
						tileTypeString = "Ice";
					}

					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Type").GetComponent<Text>().text = tileTypeString;
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = mouseOverTile.biome.name;
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Temperature").GetComponent<Text>().text = Mathf.RoundToInt(mouseOverTile.temperature) + "°C";
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Precipitation").GetComponent<Text>().text = Mathf.RoundToInt(mouseOverTile.GetPrecipitation() * 100f) + "%";

					if (mouseOverTile.HasRoof()) {
						tileRoofElement.SetActive(true);
						tileRoofElement.transform.Find("TileInfo-Label-Text").GetComponent<Text>().text = "Roof";
						tileRoofElement.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200);
					}

					if (mouseOverTile.tileType.resourceRanges.Count > 0) {
						for (int i = 0; i < mouseOverTile.tileType.resourceRanges.Count; i++) {
							ResourceManager.ResourceRange resourceRange = mouseOverTile.tileType.resourceRanges[i];

							if (tileResourceElements.Count <= i) {
								tileResourceElements.Add(Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ResourceData-Panel"), tileInformation.transform, false));
							}

							GameObject tileResourceElement = tileResourceElements[i];
							tileResourceElement.SetActive(true);
							tileResourceElement.transform.Find("TileInfo-ResourceData-Value").GetComponent<Text>().text = resourceRange.resource.name;
							tileResourceElement.transform.Find("TileInfo-ResourceData-Image").GetComponent<Image>().sprite = resourceRange.resource.image;
							tileResourceElement.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200);
						}
					}

					if (mouseOverTile.plant != null) {
						if (plantObjectElements.Count <= 0) {
							plantObjectElements.Add(Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"), tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage"), false));
							plantObjectElements.Add(Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"), tileInformation.transform, false));
						}

						foreach (GameObject plantObjectElement in plantObjectElements) {
							plantObjectElement.SetActive(true);
						}

						GameObject spriteObject = plantObjectElements[0];
						spriteObject.GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;

						GameObject dataObject = plantObjectElements[1];
						dataObject.transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "Plant";
						dataObject.transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = mouseOverTile.plant.name;
						dataObject.transform.Find("TileInfo-ObjectData-Image-Panel/TileInfo-ObjectData-Image").GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;

						Slider integritySlider = dataObject.transform.Find("Integrity-Slider").GetComponent<Slider>();
						integritySlider.minValue = 0;

						if (mouseOverTile.plant.prefab.integrity > 0) {
							integritySlider.maxValue = mouseOverTile.plant.prefab.integrity;
							integritySlider.value = mouseOverTile.plant.integrity;
							integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), mouseOverTile.plant.integrity / mouseOverTile.plant.prefab.integrity);
						} else {
							integritySlider.maxValue = 1;
							integritySlider.value = 1;
							integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200);
						}
					}

					if (mouseOverTile.GetAllObjectInstances().Count > 0) {
						foreach (ResourceManager.ObjectInstance objectInstance in mouseOverTile.GetAllObjectInstances().OrderBy(o => o.prefab.layer).ToList()) {
							if (!objectElements.ContainsKey(objectInstance.prefab.layer)) {
								GameObject spriteObject = Object.Instantiate(GameManager.resourceM.tileImage, tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage"), false);
								spriteObject.name = layerToLayerNameMap[objectInstance.prefab.layer];

								GameObject dataObject = Object.Instantiate(GameManager.resourceM.objectDataPanel, tileInformation.transform, false);
								dataObject.name = layerToLayerNameMap[objectInstance.prefab.layer];

								objectElements.Add(objectInstance.prefab.layer, new List<GameObject>() {
									spriteObject,
									dataObject
								});
							}

							GameObject tileLayerSpriteObject = objectElements[objectInstance.prefab.layer][0];
							tileLayerSpriteObject.GetComponent<Image>().sprite = objectInstance.obj.GetComponent<SpriteRenderer>().sprite;
							tileLayerSpriteObject.SetActive(true);

							GameObject objectDataObject = objectElements[objectInstance.prefab.layer][1];
							objectDataObject.transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = layerToLayerNameMap[objectInstance.prefab.layer];
							objectDataObject.transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = objectInstance.prefab.name;
							objectDataObject.transform.Find("TileInfo-ObjectData-Image-Panel/TileInfo-ObjectData-Image").GetComponent<Image>().sprite = objectInstance.obj.GetComponent<SpriteRenderer>().sprite;

							Slider integritySlider = objectDataObject.transform.Find("Integrity-Slider").GetComponent<Slider>();

							if (objectInstance.prefab.integrity > 0) {
								integritySlider.minValue = 0;
								integritySlider.maxValue = objectInstance.prefab.integrity;
								integritySlider.value = objectInstance.integrity;
								integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), objectInstance.integrity / objectInstance.prefab.integrity);
							} else {
								integritySlider.minValue = 0;
								integritySlider.maxValue = 1;
								integritySlider.value = 1;
								integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200);
							}

							objectDataObject.SetActive(true);
						}
					}

					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(140, 100);
				} else {
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage").GetComponent<Image>().sprite = GameManager.resourceM.whiteSquareSprite;
					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage").GetComponent<Image>().color = GameManager.cameraM.cameraComponent.backgroundColor;

					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Type").GetComponent<Text>().text = "Undiscovered";

					tileInformation.transform.Find("TileInformation-GeneralInfo-Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(140, 37);
				}

				if (!tileInformation.activeSelf) {
					tileInformation.SetActive(true);
				}
			} else {
				tileInformation.SetActive(false);
			}
		}

		public class SkillElement {
			public Colonist colonist;
			public ColonistManager.SkillInstance skill;
			public GameObject obj;

			public SkillElement(Colonist colonist, ColonistManager.SkillInstance skill, Transform parent) {
				this.colonist = colonist;
				this.skill = skill;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SkillInfoElement-Panel"), parent, false);

				obj.transform.Find("Name").GetComponent<Text>().text = skill.prefab.name;
				// ADD SKILL IMAGE

				Update();
			}

			public void Update() {
				obj.transform.Find("Level").GetComponent<Text>().text = skill.level + "." + Mathf.RoundToInt(skill.currentExperience / skill.nextLevelExperience * 100f);
				obj.transform.Find("Experience-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(skill.currentExperience / skill.nextLevelExperience * 100f);

				ColonistManager.SkillInstance highestSkillInstance = GameManager.colonistM.FindHighestSkillInstance(skill.prefab);

				if (highestSkillInstance != null) {
					obj.transform.Find("Level-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((float)skill.level / (highestSkillInstance.level > 0 ? highestSkillInstance.level : 1) * 100f);

					if (highestSkillInstance.colonist == colonist || Mathf.Approximately(highestSkillInstance.CalculateTotalSkillLevel(), skill.CalculateTotalSkillLevel())) {
						obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkYellow);
						obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightYellow);
					} else {
						obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
						obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
					}
				}
			}
		}

		public class InventoryElement {
			public ResourceManager.ResourceAmount resourceAmount;
			public GameObject obj;

			public InventoryElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
				this.resourceAmount = resourceAmount;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceInfoElement-Panel"), parent, false);

				obj.transform.Find("Name").GetComponent<Text>().text = resourceAmount.resource.name;
				obj.transform.Find("Image").GetComponent<Image>().sprite = resourceAmount.resource.image;

				Update();
			}

			public void Update() {
				obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
			}
		}

		public class ResourceTransferElement {
			public enum TransferType {
				In,
				Out
			}

			public ResourceManager.Resource resource;
			public ResourceManager.TradingPost tradingPost;

			public TransferType transferType;

			public GameObject obj;

			public InputField transferAmountInput;
			public Text transferAmountText;

			public int transferAmount = 0;

			public ResourceTransferElement(ResourceManager.Resource resource, ResourceManager.TradingPost tradingPost, TransferType transferType, Transform parent) {
				this.resource = resource;
				this.tradingPost = tradingPost;
				this.transferType = transferType;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceTransferElement-Panel"), parent, false);

				obj.transform.Find("Name").GetComponent<Text>().text = resource.name;

				obj.transform.Find("Image").GetComponent<Image>().sprite = resource.image;

				transferAmountInput = obj.transform.Find("TransferAmount-Input").GetComponent<InputField>();
				transferAmountText = transferAmountInput.transform.Find("Text").GetComponent<Text>();
				transferAmountInput.onEndEdit.AddListener(delegate {
					int newTransferAmount = 0;
					if (int.TryParse(transferAmountInput.text, out newTransferAmount)) {
						if (newTransferAmount != transferAmount && newTransferAmount >= 0) {
							int availableAmount = transferType == TransferType.In ? resource.GetAvailableAmount() : tradingPost.GetInventory().resources.Find(r => r.resource == resource).amount;
							if (newTransferAmount > availableAmount) {
								newTransferAmount = availableAmount;
							}

							transferAmount = newTransferAmount;
							if (newTransferAmount == 0) {
								transferAmountInput.text = string.Empty;
							}
						}
					} else {
						transferAmount = 0;
					}

					transferAmountInput.text = transferAmount.ToString();
				});

				Update();
			}

			public void Update() {
				int availableAmount = transferType == TransferType.In ? resource.GetAvailableAmount() : tradingPost.GetInventory().resources.Find(r => r.resource == resource).amount;
				obj.transform.Find("Amount").GetComponent<Text>().text = availableAmount.ToString();
				if (transferAmount > availableAmount) {
					transferAmount = availableAmount;
					transferAmountInput.text = transferAmount.ToString();
				}
			}
		}

		public class ReservedResourcesColonistElement {
			public ResourceManager.ReservedResources reservedResources;
			public List<InventoryElement> reservedResourceElements = new List<InventoryElement>();
			public GameObject obj;

			public ReservedResourcesColonistElement(HumanManager.Human human, ResourceManager.ReservedResources reservedResources, Transform parent) {
				this.reservedResources = reservedResources;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourcesColonistInfoElement-Panel"), parent, false);

				obj.transform.Find("ColonistInfo-Panel/ColonistName-Text").GetComponent<Text>().text = human.name;
				obj.transform.Find("ColonistInfo-Panel/ColonistReservedCount-Text").GetComponent<Text>().text = reservedResources.resources.Count.ToString();
				obj.transform.Find("ColonistInfo-Panel/ColonistImage").GetComponent<Image>().sprite = human.moveSprites[0];

				foreach (ResourceManager.ResourceAmount ra in reservedResources.resources.OrderByDescending(ra => ra.amount)) {
					InventoryElement inventoryElement = new InventoryElement(ra, obj.transform.Find("ReservedResourcesList-Panel"));
					inventoryElement.obj.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 32);
					reservedResourceElements.Add(inventoryElement);
				}
			}
		}

		public class ReservedResourceElement {
			public ResourceManager.ResourceAmount resourceAmount;
			public GameObject obj;

			public ReservedResourceElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
				this.resourceAmount = resourceAmount;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourceInfoElement-Panel"), parent, false);

				obj.transform.Find("Name").GetComponent<Text>().text = resourceAmount.resource.name;

				obj.transform.Find("Image").GetComponent<Image>().sprite = resourceAmount.resource.image;

				Update();
			}

			public void Update() {
				obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
			}
		}

		public class NeedElement {
			public ColonistManager.NeedInstance needInstance;
			public GameObject obj;

			public NeedElement(ColonistManager.NeedInstance needInstance, Transform parent) {
				this.needInstance = needInstance;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/NeedElement-Panel"), parent, false);

				obj.transform.Find("NeedName-Text").GetComponent<Text>().text = needInstance.prefab.name;

				Update();
			}

			public void Update() {
				obj.transform.Find("NeedValue-Text").GetComponent<Text>().text = needInstance.GetRoundedValue() + "%";

				obj.transform.Find("Need-Slider").GetComponent<Slider>().value = needInstance.GetValue();
				obj.transform.Find("Need-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen), ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed), needInstance.GetValue() / needInstance.prefab.clampValue);
				obj.transform.Find("Need-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), needInstance.GetValue() / needInstance.prefab.clampValue);
			}
		}

		public class MoodModifierElement {
			public ColonistManager.MoodModifierInstance moodModifierInstance;
			public GameObject obj;

			public MoodModifierElement(ColonistManager.MoodModifierInstance moodModifierInstance, Transform parent) {
				this.moodModifierInstance = moodModifierInstance;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/MoodModifierElement-Panel"), parent, false);

				obj.transform.Find("MoodModifierName-Text").GetComponent<Text>().text = moodModifierInstance.prefab.name;
				if (moodModifierInstance.prefab.effectAmount > 0) {
					obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
				} else if (moodModifierInstance.prefab.effectAmount < 0) {
					obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
				} else {
					obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);
				}

				Update();
			}

			public bool Update() {
				if (!moodModifierInstance.colonist.moodModifiers.Contains(moodModifierInstance)) {
					Object.Destroy(obj);
					return false;
				}

				if (moodModifierInstance.prefab.infinite) {
					obj.transform.Find("MoodModifierTime-Text").GetComponent<Text>().text = "Until Not";
				} else {
					obj.transform.Find("MoodModifierTime-Text").GetComponent<Text>().text = Mathf.RoundToInt(moodModifierInstance.timer) + "s (" + Mathf.RoundToInt(moodModifierInstance.prefab.effectLengthSeconds) + "s)";
				}

				if (moodModifierInstance.prefab.effectAmount > 0) {
					obj.transform.Find("MoodModifierAmount-Text").GetComponent<Text>().text = "+" + moodModifierInstance.prefab.effectAmount + "%";
				} else if (moodModifierInstance.prefab.effectAmount < 0) {
					obj.transform.Find("MoodModifierAmount-Text").GetComponent<Text>().text = moodModifierInstance.prefab.effectAmount + "%";
				} else {
					obj.transform.Find("MoodModifierAmount-Text").GetComponent<Text>().text = moodModifierInstance.prefab.effectAmount + "%";
				}

				return true;
			}
		}

		public void SetRightListPanelSize() {
			RectTransform rightListPanel = gameUI.transform.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			int verticalOffset = 5; // Nothing active default
			if (selectedColonistInformationPanel.activeSelf) {
				verticalOffset = 350;
			} else if (selectedTraderMenu.activeSelf) {
				verticalOffset = 100;
			}

			rightListPanel.offsetMin = new Vector2(rightListSize.x, verticalOffset);
		}

		private readonly List<NeedElement> selectedColonistNeedElements = new List<NeedElement>();

		private readonly List<MoodModifierElement> selectedColonistMoodModifierElements = new List<MoodModifierElement>();

		private readonly List<SkillElement> selectedColonistSkillElements = new List<SkillElement>();

		private readonly List<InventoryElement> selectedColonistInventoryElements = new List<InventoryElement>();

		private readonly List<ReservedResourcesColonistElement> selectedColonistReservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();

		public void SetSelectedColonistInformation(bool sameColonistSelected) {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Colonist) {
				Colonist selectedColonist = (Colonist)GameManager.humanM.selectedHuman;

				selectedColonistInformationPanel.SetActive(true);

				selectedColonistInformationPanel.transform.Find("ColonistName-Text").GetComponent<Text>().text = GameManager.humanM.selectedHuman.name + " (" + GameManager.humanM.selectedHuman.gender.ToString()[0] + ")";
				selectedColonistInformationPanel.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = GameManager.humanM.selectedHuman.moveSprites[0];

				selectedColonistInformationPanel.transform.Find("AffiliationName-Text").GetComponent<Text>().text = "Colonist of " + GameManager.colonyM.colony.name;

				RemakeSelectedColonistNeeds();
				RemakeSelectedColonistMoodModifiers();
				RemakeSelectedColonistSkills();
				RemakeSelectedColonistInventory(selectedColonist);
				RemakeSelectedColonistClothing(selectedColonist, sameColonistSelected);

				UpdateSelectedColonistInformation();
			} else {
				selectedColonistInformationPanel.SetActive(false);

				if (selectedColonistSkillElements.Count > 0) {
					foreach (SkillElement skillElement in selectedColonistSkillElements) {
						Object.Destroy(skillElement.obj);
					}

					selectedColonistSkillElements.Clear();
				}

				if (selectedColonistNeedElements.Count > 0) {
					foreach (NeedElement needElement in selectedColonistNeedElements) {
						Object.Destroy(needElement.obj);
					}

					selectedColonistNeedElements.Clear();
				}

				if (selectedColonistReservedResourcesColonistElements.Count > 0) {
					foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in selectedColonistReservedResourcesColonistElements) {
						foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
							Object.Destroy(reservedResourceElement.obj);
						}

						reservedResourcesColonistElement.reservedResourceElements.Clear();
						Object.Destroy(reservedResourcesColonistElement.obj);
					}

					selectedColonistReservedResourcesColonistElements.Clear();
				}

				if (selectedColonistInventoryElements.Count > 0) {
					foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
						Object.Destroy(inventoryElement.obj);
					}

					selectedColonistInventoryElements.Clear();
				}
			}
		}

		public void SetSelectedColonistTab(GameObject tabButtonClicked) {
			bool needsSkillsClicked = tabButtonClicked == selectedColonistNeedsSkillsTabButton;
			bool inventoryClicked = tabButtonClicked == selectedColonistInventoryTabButton;
			bool clothingClicked = tabButtonClicked == selectedColonistClothingTabButton;

			selectedColonistNeedsSkillsPanel.SetActive(needsSkillsClicked);
			selectedColonistNeedsSkillsTabButtonLinkPanel.SetActive(needsSkillsClicked);
			selectedColonistMoodModifiersPanel.SetActive(false);

			selectedColonistInventoryPanel.SetActive(inventoryClicked);
			selectedColonistInventoryTabButtonLinkPanel.SetActive(inventoryClicked);

			selectedColonistClothingPanel.SetActive(clothingClicked);
			selectedColonistClothingTabButtonLinkPanel.SetActive(clothingClicked);
			SetSelectedColonistClothingSelectionPanelActive(false);
		}

		public void RemakeSelectedColonistNeeds() {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Colonist selectedColonist) {
				foreach (NeedElement needElement in selectedColonistNeedElements) {
					Object.Destroy(needElement.obj);
				}

				selectedColonistNeedElements.Clear();
				List<ColonistManager.NeedInstance> sortedNeeds = selectedColonist.needs.OrderByDescending(need => need.GetValue()).ToList();
				foreach (ColonistManager.NeedInstance need in sortedNeeds) {
					selectedColonistNeedElements.Add(new NeedElement(need, selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/Needs-ScrollPanel/NeedsList-Panel")));
				}
			}
		}

		public void RemakeSelectedColonistMoodModifiers() {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Colonist selectedColonist) {
				foreach (MoodModifierElement moodModifierElement in selectedColonistMoodModifierElements) {
					Object.Destroy(moodModifierElement.obj);
				}

				selectedColonistMoodModifierElements.Clear();
				foreach (ColonistManager.MoodModifierInstance moodModifier in selectedColonist.moodModifiers) {
					selectedColonistMoodModifierElements.Add(new MoodModifierElement(moodModifier, selectedColonistMoodModifiersPanel.transform.Find("MoodModifier-ScrollPanel/MoodModifierList-Panel")));
				}
			}
		}

		public void RemakeSelectedColonistSkills() {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Colonist selectedColonist) {
				foreach (SkillElement skillElement in selectedColonistSkillElements) {
					Object.Destroy(skillElement.obj);
				}

				selectedColonistSkillElements.Clear();
				List<ColonistManager.SkillInstance> sortedSkills = selectedColonist.skills.OrderByDescending(skill => skill.CalculateTotalSkillLevel()).ToList();
				foreach (ColonistManager.SkillInstance skill in sortedSkills) {
					selectedColonistSkillElements.Add(new SkillElement(selectedColonist, skill, selectedColonistNeedsSkillsPanel.transform.Find("Skills-Panel/Skills-ScrollPanel/SkillsList-Panel")));
				}
			}
		}

		public void RemakeSelectedColonistInventory(Colonist selectedColonist) {
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in selectedColonistReservedResourcesColonistElements) {
				foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Object.Destroy(reservedResourceElement.obj);
				}

				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Object.Destroy(reservedResourcesColonistElement.obj);
			}

			selectedColonistReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
				Object.Destroy(inventoryElement.obj);
			}

			selectedColonistInventoryElements.Clear();
			foreach (ResourceManager.ReservedResources rr in GameManager.humanM.selectedHuman.GetInventory().reservedResources) {
				selectedColonistReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.human, rr, selectedColonistInventoryPanel.transform.Find("Inventory-ScrollPanel/InventoryList-Panel")));
			}

			foreach (ResourceManager.ResourceAmount ra in GameManager.humanM.selectedHuman.GetInventory().resources) {
				selectedColonistInventoryElements.Add(new InventoryElement(ra, selectedColonistInventoryPanel.transform.Find("Inventory-ScrollPanel/InventoryList-Panel")));
			}

			Button emptyInventoryButton = selectedColonistInventoryPanel.transform.Find("EmptyInventory-Button").GetComponent<Button>();
			emptyInventoryButton.onClick.RemoveAllListeners();
			emptyInventoryButton.onClick.AddListener(delegate { selectedColonist.EmptyInventory(selectedColonist.FindValidContainersToEmptyInventory()); });
		}

		public void RemakeSelectedColonistClothing(Colonist selectedColonist, bool selectionPanelKeepState) {
			selectedColonistClothingPanel.transform.Find("ColonistBody-Image").GetComponent<Image>().sprite = selectedColonist.moveSprites[0];

			foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothingKVP in selectedColonist.clothes) {
				ResourceManager.Clothing clothing = selectedColonist.clothes[appearanceToClothingKVP.Key];

				Button clothingTypeButton = selectedColonistClothingPanel.transform.Find("ClothingButtons-List/" + appearanceToClothingKVP.Key + "-Button").GetComponent<Button>();

				clothingTypeButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey180);

				if (clothing == null) {
					List<Job> checkJobs = new List<Job>();
					checkJobs.AddRange(selectedColonist.backlog);
					checkJobs.Add(selectedColonist.storedJob);
					checkJobs.Add(selectedColonist.job);

					foreach (Job checkJob in checkJobs) {
						if (checkJob != null && checkJob.objectPrefab.type == ResourceManager.ObjectEnum.WearClothes) {
							ResourceManager.Clothing clothingToWear = (ResourceManager.Clothing)checkJob.requiredResources[0].resource;
							if (clothingToWear.prefab.appearance == appearanceToClothingKVP.Key) {
								clothing = clothingToWear;
								clothingTypeButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightOrange);
							}
						}
					}

					checkJobs.Clear();
				}

				selectedColonistClothingPanel.transform.Find("ColonistBody-Image/Colonist" + appearanceToClothingKVP.Key + "-Image").GetComponent<Image>().sprite = clothing == null ? GameManager.resourceM.clearSquareSprite : clothing.moveSprites[0];

				clothingTypeButton.interactable = GameManager.resourceM.GetClothesByAppearance(appearanceToClothingKVP.Key).Count > 0 || selectedColonist.clothes[appearanceToClothingKVP.Key] != null;
				clothingTypeButton.onClick.RemoveAllListeners();
				clothingTypeButton.onClick.AddListener(delegate { SetSelectedColonistClothingSelectionPanel(true, appearanceToClothingKVP.Key, selectedColonist); });

				clothingTypeButton.transform.Find("Name").GetComponent<Text>().text = clothing == null ? "None" : clothing.name;
				clothingTypeButton.transform.Find("Image").GetComponent<Image>().sprite = clothing == null ? GameManager.resourceM.clearSquareSprite : clothing.image;
			}

			if (!selectionPanelKeepState) {
				SetSelectedColonistClothingSelectionPanelActive(false);
			}
		}

		private readonly List<ClothingElement> availableClothingElements = new List<ClothingElement>();
		private readonly List<ClothingElement> takenClothingElements = new List<ClothingElement>();

		public void SetSelectedColonistClothingSelectionPanelActive(bool active) {
			selectedColonistClothingSelectionPanel.SetActive(active);

			Button disrobeButton = selectedColonistClothingSelectionPanel.transform.Find("Disrobe-Button").GetComponent<Button>();
			disrobeButton.onClick.RemoveAllListeners();

			foreach (ClothingElement clothingElement in availableClothingElements) {
				Object.Destroy(clothingElement.obj);
			}

			availableClothingElements.Clear();
			foreach (ClothingElement clothingElement in takenClothingElements) {
				Object.Destroy(clothingElement.obj);
			}

			takenClothingElements.Clear();
		}

		public void SetSelectedColonistClothingSelectionPanel(bool active, HumanManager.Human.Appearance clothingType, Colonist selectedColonist) {
			SetSelectedColonistClothingSelectionPanelActive(active);

			Button disrobeButton = selectedColonistClothingSelectionPanel.transform.Find("Disrobe-Button").GetComponent<Button>();

			if (selectedColonistClothingSelectionPanel.activeSelf) {
				selectedColonistClothingSelectionPanel.transform.Find("SelectClothes-Text").GetComponent<Text>().text = "Select " + clothingType;

				disrobeButton.interactable = selectedColonist.clothes[clothingType] != null;
				disrobeButton.onClick.AddListener(delegate {
					selectedColonist.ChangeClothing(clothingType, null);
					SetSelectedColonistClothingSelectionPanelActive(false);
					SetSelectedColonistInformation(true);
				});

				List<ResourceManager.Clothing> clothes = GameManager.resourceM.GetClothesByAppearance(clothingType);
				foreach (ResourceManager.Clothing clothing in clothes.Where(c => c.GetWorldTotalAmount() > 0)) {
					if (clothing.GetAvailableAmount() > 0) {
						availableClothingElements.Add(new ClothingElement(clothing, GameManager.humanM.selectedHuman, availableClothingTitleAndList.Value.transform));
					} else if (selectedColonist.clothes[clothingType] == null || clothing.name != selectedColonist.clothes[clothingType].name) {
						takenClothingElements.Add(new ClothingElement(clothing, GameManager.humanM.selectedHuman, takenClothingTitleAndList.Value.transform));
					}
				}

				availableClothingTitleAndList.Key.SetActive(availableClothingElements.Count > 0);
				availableClothingTitleAndList.Value.SetActive(availableClothingElements.Count > 0);
				takenClothingTitleAndList.Key.SetActive(takenClothingElements.Count > 0);
				takenClothingTitleAndList.Value.SetActive(takenClothingElements.Count > 0);
			}
		}

		public class ClothingElement {
			public ResourceManager.Clothing clothing;
			public HumanManager.Human human;
			public GameObject obj;

			public ClothingElement(ResourceManager.Clothing clothing, HumanManager.Human human, Transform parent) {
				this.clothing = clothing;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ClothingElement-Panel"), parent, false);

				obj.transform.Find("Name").GetComponent<Text>().text = clothing.name;
				obj.transform.Find("Image").GetComponent<Image>().sprite = clothing.moveSprites[0];
				if (clothing.prefab.appearance == HumanManager.Human.Appearance.Backpack) {
					obj.transform.Find("InsulationWaterResistance").GetComponent<Text>().text = "+" + clothing.prefab.weightCapacity + "kg / +" + clothing.prefab.volumeCapacity + "m³";
				} else {
					obj.transform.Find("InsulationWaterResistance").GetComponent<Text>().text = "❄ " + clothing.prefab.insulation + " / ☂ " + clothing.prefab.waterResistance;
				}

				obj.GetComponent<Button>().onClick.AddListener(delegate {
					human.ChangeClothing(clothing.prefab.appearance, clothing);

					/*GameManager.uiMOld.SetSelectedColonistClothingSelectionPanelActive(false);
					GameManager.uiMOld.SetSelectedColonistInformation(true);*/
				});
			}
		}

		private static readonly Dictionary<int, int> moodModifierButtonSizeMap = new Dictionary<int, int>() {
			{ 1, 45 }, { 2, 60 }, { 3, 65 }
		};

		private static readonly Dictionary<int, int> moodModifierValueHorizontalPositionMap = new Dictionary<int, int>() {
			{ 1, -50 }, { 2, -65 }, { 3, -70 }
		};

		private readonly List<MoodModifierElement> removeHME = new List<MoodModifierElement>();

		public void UpdateSelectedColonistInformation() {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Colonist) {
				Colonist selectedColonist = (Colonist)GameManager.humanM.selectedHuman;

				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedColonist.health * 100);
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed), ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen), selectedColonist.health);
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), selectedColonist.health);
				//selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.health * 100) + "%";

				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMood-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedColonist.effectiveMood);
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMood-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed), ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen), selectedColonist.effectiveMood / 100f);
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMood-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), selectedColonist.effectiveMood / 100f);
				//selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMoodValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.effectiveMood) + "%";

				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider").GetComponent<Slider>().minValue = 0;
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider").GetComponent<Slider>().maxValue = selectedColonist.GetInventory().maxWeight;
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider").GetComponent<Slider>().value = selectedColonist.GetInventory().UsedWeight();
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider/Handle Slide Area/Handle/Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.GetInventory().UsedWeight() / (float)selectedColonist.GetInventory().maxWeight * 100).ToString();

				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider").GetComponent<Slider>().minValue = 0;
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider").GetComponent<Slider>().maxValue = selectedColonist.GetInventory().maxVolume;
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider").GetComponent<Slider>().value = selectedColonist.GetInventory().UsedVolume();
				selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider/Handle Slide Area/Handle/Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.GetInventory().UsedVolume() / (float)selectedColonist.GetInventory().maxVolume * 100).ToString();

				selectedColonistInformationPanel.transform.Find("ColonistCurrentAction-Text").GetComponent<Text>().text = selectedColonist.GetCurrentActionString();
				if (selectedColonist.storedJob != null) {
					selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = selectedColonist.GetStoredActionString();
				} else {
					selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = string.Empty;
				}

				int moodModifiersSum = Mathf.RoundToInt(selectedColonist.moodModifiersSum);

				int moodLength = Mathf.Abs(moodModifiersSum).ToString().Length;
				Text moodModifierAmountText = selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodModifiers-Button/MoodModifiersAmount-Text").GetComponent<Text>();
				if (moodModifiersSum > 0) {
					moodModifierAmountText.text = "+" + moodModifiersSum + "%";
					moodModifierAmountText.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
				} else if (moodModifiersSum < 0) {
					moodModifierAmountText.text = moodModifiersSum + "%";
					moodModifierAmountText.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
				} else {
					moodModifierAmountText.text = moodModifiersSum + "%";
					moodModifierAmountText.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50);
				}

				selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodModifiers-Button").GetComponent<RectTransform>().sizeDelta = new Vector2(moodModifierButtonSizeMap[moodLength], 20);
				selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodValue-Text").GetComponent<RectTransform>().offsetMax = new Vector2(moodModifierValueHorizontalPositionMap[moodLength], 0);
				selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.effectiveMood) + "%";
				selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodValue-Text").GetComponent<Text>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), selectedColonist.effectiveMood / 100f);

				foreach (SkillElement skillElement in selectedColonistSkillElements) {
					skillElement.Update();
				}

				foreach (NeedElement needElement in selectedColonistNeedElements) {
					needElement.Update();
				}

				foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
					inventoryElement.Update();
				}

				foreach (MoodModifierElement moodModifierElement in selectedColonistMoodModifierElements) {
					bool keep = moodModifierElement.Update();
					if (!keep) {
						removeHME.Add(moodModifierElement);
					}
				}

				if (removeHME.Count > 0) {
					foreach (MoodModifierElement moodModifierElement in removeHME) {
						Object.Destroy(moodModifierElement.obj);
						selectedColonistMoodModifierElements.Remove(moodModifierElement);
					}

					removeHME.Clear();
				}
			}
		}

		public class ColonistElement {
			public Colonist colonist;
			public GameObject obj;

			public ColonistElement(Colonist colonist, Transform transform) {
				this.colonist = colonist;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), transform, false);

				obj.GetComponent<RectTransform>().sizeDelta = new Vector2(135, obj.GetComponent<RectTransform>().sizeDelta.y);

				obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
				obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
				obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });

				Update();
			}

			public void Update() {
				obj.GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), colonist.health);
			}

			public void DestroyObject() {
				Object.Destroy(obj);
			}
		}

		private readonly List<ColonistElement> colonistElements = new List<ColonistElement>();

		public void RemoveColonistElements() {
			foreach (ColonistElement colonistElement in colonistElements) {
				colonistElement.DestroyObject();
			}

			colonistElements.Clear();
		}

		public void SetColonistElements() {
			RemoveColonistElements();
			if (colonistsPanel.activeSelf) {
				foreach (Colonist colonist in Colonist.colonists) {
					colonistElements.Add(new ColonistElement(colonist, colonistsPanel.transform.Find("ColonistList-Panel")));
				}
			}
		}

		public void UpdateColonistElements() {
			foreach (ColonistElement colonistElement in colonistElements) {
				colonistElement.Update();
			}
		}

		public class CaravanElement {
			public Caravan caravan;
			public GameObject obj;

			private readonly Text affiliatedColonyNameText;
			private readonly Text resourceGroupNameText;

			public CaravanElement(Caravan caravan, Transform transform) {
				this.caravan = caravan;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/CaravanElement-Panel"), transform, false);

				obj.GetComponent<RectTransform>().sizeDelta = new Vector2(135, obj.GetComponent<RectTransform>().sizeDelta.y);

				obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(caravan.traders[0]); });

				affiliatedColonyNameText = obj.transform.Find("AffiliatedColonyName-Text").GetComponent<Text>();
				affiliatedColonyNameText.text = caravan.location.name;

				resourceGroupNameText = obj.transform.Find("ResourceGroupName-Text").GetComponent<Text>();
				resourceGroupNameText.text = caravan.resourceGroup.name;

				obj.transform.Find("TradeWithCaravan-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.caravanM.SetSelectedCaravan(caravan); });

				Update();
			}

			public void Update() {
				obj.GetComponent<Image>().color = caravan.confirmedResourcesToTrade.Count > 0 ? ColourUtilities.GetColour(ColourUtilities.EColour.LightYellow) : ColourUtilities.GetColour(ColourUtilities.EColour.LightPurple);

				Color textColour = caravan.confirmedResourcesToTrade.Count > 0 ? ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50) : ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);
				affiliatedColonyNameText.color = textColour;
				resourceGroupNameText.color = textColour;

				obj.GetComponent<Button>().enabled = caravan.traders.Count > 0;
			}

			public void DestroyObject() {
				Object.Destroy(obj);
			}
		}

		private readonly List<CaravanElement> caravanElements = new List<CaravanElement>();

		public void RemoveCaravanElements() {
			foreach (CaravanElement caravanElement in caravanElements) {
				caravanElement.DestroyObject();
			}

			caravanElements.Clear();
		}

		public void SetCaravanElements() {
			if (GameManager.caravanM.caravans.Count > 0) {
				RemoveCaravanElements();
				caravansPanel.SetActive(true);
				if (caravansPanel.activeSelf) {
					foreach (Caravan caravan in GameManager.caravanM.caravans.Where(c => c.traders[0].overTile.IsVisibleToAColonist()).OrderByDescending(c => c.confirmedResourcesToTrade.Count > 0)) {
						caravanElements.Add(new CaravanElement(caravan, caravansPanel.transform.Find("CaravanList-Panel")));
					}
				}
			} else {
				caravansPanel.SetActive(false);
				RemoveCaravanElements();
			}
		}

		public void UpdateCaravanElements() {
			foreach (CaravanElement caravanElement in caravanElements) {
				caravanElement.Update();
			}
		}

		public class JobElement {
			public Job job;
			public Colonist colonist;
			public GameObject obj;
			public GameObject colonistObj;

			public JobElement(Job job, Colonist colonist, Transform parent) {
				this.job = job;
				this.colonist = colonist;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/JobInfoElement-Panel"), parent, false);
				GameObject jobInfo = obj.transform.Find("Content/JobInfo").gameObject;
				Text jobInfoNameText = jobInfo.transform.Find("Name").GetComponent<Text>();

				if (job.objectPrefab.type == ResourceManager.ObjectEnum.CreateResource) {
					jobInfo.transform.Find("Image").GetComponent<Image>().sprite = job.createResource.resource.image;
				} else {
					jobInfo.transform.Find("Image").GetComponent<Image>().sprite = job.objectPrefab.GetBaseSpriteForVariation(job.variation);
				}

				jobInfoNameText.text = job.prefab.GetJobInfoNameText(job);

				jobInfo.transform.Find("Type").GetComponent<Text>().text = StringUtilities.SplitByCapitals(job.objectPrefab.jobType.ToString());
				obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.cameraM.SetCameraPosition(job.tile.obj.transform.position); });

				bool hasPriority = job.priority != 0;

				Text priorityText = jobInfo.transform.Find("Priority").GetComponent<Text>();

				if (hasPriority) {
					priorityText.text = job.priority.ToString();
					if (job.priority > 0) {
						priorityText.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkYellow);
					} else if (job.priority < 0) {
						priorityText.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
					}
				} else {
					priorityText.text = string.Empty;
				}

				if (job.requiredResources.Count > 0) {
					obj.transform.Find("Content/RequiredResources-Panel").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.WhiteAlpha64);

					foreach (ResourceManager.ResourceAmount resourceAmount in job.requiredResources) {
						GameObject resourceAmountObj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), obj.transform.Find("Content/RequiredResources-Panel"), false);

						resourceAmountObj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.Clear);

						resourceAmountObj.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resourceAmount.resource.image;
						resourceAmountObj.transform.Find("ResourceName-Text").GetComponent<Text>().text = resourceAmount.resource.name;
						resourceAmountObj.transform.Find("AvailableOverRequiredValue-Text").GetComponent<Text>().text = resourceAmount.amount.ToString();
					}
				}

				if (colonist != null) {
					colonistObj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), obj.transform.Find("Content"), false);
					colonistObj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
					colonistObj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
					colonistObj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });
					colonistObj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.WhiteAlpha64);

					colonistObj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<LayoutElement>().minWidth - 6, colonistObj.GetComponent<RectTransform>().sizeDelta.y);
					obj.transform.Find("Content").GetComponent<VerticalLayoutGroup>().padding.bottom = 3;

					if (job.started) {
						obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);

						obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().minValue = 0;
						obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
						obj.transform.Find("JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
					} else {
						obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightOrange);

						obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().minValue = 0;
						obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().maxValue = colonist.startPathLength;
						obj.transform.Find("JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkOrange);
					}
				} else {
					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().minValue = 0;
					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
					obj.transform.Find("JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
				}

				job.jobUIElement = this;

				obj.name = $"{job.prefab.name} {job.objectPrefab.name}";

				Update();
			}

			public void Update() {
				if (colonist != null) {
					//colonistObj.GetComponent<Image>().color = Color.Lerp(ChangeAlpha(ColourUtilities.GetColour(Colours.LightRed), 150f / 255f), ChangeAlpha(ColourUtilities.GetColour(Colours.LightGreen), 150f / 255f), colonist.health);
				}

				if (colonist != null && !job.started && colonist.startPathLength > 0 && colonist.path.Count > 0) {
					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().value = (1 - colonist.path.Count / (float)colonist.startPathLength) * colonist.startPathLength; //Mathf.Lerp((1 - (colonist.path.Count / (float)colonist.startPathLength)) * colonist.startPathLength, (1 - ((colonist.path.Count - 1) / (float)colonist.startPathLength)) * colonist.startPathLength, (1 - Vector2.Distance(colonist.obj.transform.position, colonist.path[0].obj.transform.position)) + 0.5f);
				} else {
					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().value = job.colonistBuildTime - job.jobProgress;
				}
			}

			public void DestroyObjects() {
				job.jobUIElement = null;
				Object.Destroy(obj);
				Object.Destroy(colonistObj);
			}

			public void Remove() {
				GameManager.uiMOld.jobElements.Remove(this);
				job.jobUIElement = null;
				DestroyObjects();
			}
		}

		private readonly List<JobElement> jobElements = new List<JobElement>();

		public void RemoveJobElements() {
			foreach (JobElement jobElement in jobElements) {
				jobElement.job.jobUIElement = null;
				jobElement.DestroyObjects();
			}

			jobElements.Clear();
		}

		public void SetJobElements() {
			if (Job.jobs.Count > 0 || Colonist.colonists.Where(colonist => colonist.job != null).ToList().Count > 0) {
				RemoveJobElements();
				List<Colonist> orderedColonists = Colonist.colonists.Where(colonist => colonist.job != null).ToList();
				foreach (Colonist jobColonist in orderedColonists.Where(colonist => colonist.job.started).OrderBy(colonist => colonist.job.jobProgress)) {
					jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobListPanel.transform));
				}

				foreach (Colonist jobColonist in orderedColonists.Where(colonist => !colonist.job.started).OrderBy(colonist => colonist.path.Count)) {
					jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobListPanel.transform));
				}

				foreach (Job job in Job.jobs.Where(j => j.started).OrderBy(j => j.jobProgress / j.colonistBuildTime)) {
					jobElements.Add(new JobElement(job, null, jobListPanel.transform));
				}

				foreach (Job job in Job.jobs.Where(j => !j.started).OrderByDescending(j => j.priority)) {
					jobElements.Add(new JobElement(job, null, jobListPanel.transform));
				}

				jobsPanel.SetActive(true);
			} else {
				jobsPanel.SetActive(false);
				RemoveJobElements();
			}
		}

		public void UpdateJobElements() {
			foreach (JobElement jobElement in jobElements) {
				jobElement.Update();
			}
		}

		public void SetListPanelCollapsed(GameObject listPanel, Text arrowText) {
			listPanel.SetActive(!listPanel.activeSelf);
			if (listPanel.activeSelf) {
				arrowText.text = ">";
			} else {
				arrowText.text = "<";
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(gameUI.transform.Find("RightList-Panel/RightList-ScrollPanel/RightList-Panel").GetComponent<RectTransform>());
		}

		public void UpdateDateTimeInformation() {
			dateTimeInformationPanel.transform.Find("DateTimeInformation-Speed-Text").GetComponent<Text>().text = GameManager.timeM.GetTimeModifier() > 0 ? new string('>', GameManager.timeM.GetTimeModifier()) : "-";
			dateTimeInformationPanel.transform.Find("DateTimeInformation-TimeDayNight-Panel/DateTimeInformation-Time-Panel/DateTimeInformation-Time-Text").GetComponent<Text>().text = $"{(Time.Time.Hour < 10 ? $"0{Time.Time.Hour}" : Time.Time.Hour)}:{(Time.Time.Minute < 10 ? $"0{Time.Time.Minute}" : Time.Time.Minute)}";
			dateTimeInformationPanel.transform.Find("DateTimeInformation-TimeDayNight-Panel/DateTimeInformation-DayNight-Panel/DateTimeInformation-DayNight-Text").GetComponent<Text>().text = $"{GameManager.timeM.GetDayNightString()}";
			dateTimeInformationPanel.transform.Find("DateTimeInformation-Date-Text").GetComponent<Text>().text = $"{GameManager.timeM.GetDayWithSuffix()} of {Time.Time.Season}";
			dateTimeInformationPanel.transform.Find("DateTimeInformation-Year-Text").GetComponent<Text>().text = $"Year {(Time.Time.Year <= 9 ? $"0{Time.Time.Year}" : Time.Time.Year)}";
		}

		public class SelectionSizePanel {
			public Canvas canvas;

			public GameObject panel;

			public Text sizeValue;
			public Text countValue;
			public Text selectedValue;

			public SelectionSizePanel(Canvas canvas) {
				this.canvas = canvas;
				canvas.sortingOrder = 100; // Selection Area Size Canvas

				panel = canvas.transform.Find("SelectionSize-Panel").gameObject;

				sizeValue = panel.transform.Find("SizeValue-Text").GetComponent<Text>();
				countValue = panel.transform.Find("AreaValue-Text").GetComponent<Text>();
				selectedValue = panel.transform.Find("SelectedValue-Text").GetComponent<Text>();
			}

			public void SetActive(bool active) {
				canvas.gameObject.SetActive(active);
			}

			public void Update(float xSizeRaw, float ySizeRaw, int selectionAreaCount) {
				int xSizeFloored = Mathf.Abs(Mathf.FloorToInt(xSizeRaw));
				int ySizeFloored = Mathf.Abs(Mathf.FloorToInt(ySizeRaw));

				sizeValue.text = xSizeFloored + " × " + ySizeFloored;
				countValue.text = (xSizeFloored * ySizeFloored).ToString();
				selectedValue.text = selectionAreaCount.ToString();

				canvas.transform.localScale = Vector2.one * 0.005f * 0.5f * GameManager.cameraM.cameraComponent.orthographicSize;
				canvas.transform.position = new Vector2(mousePosition.x + (canvas.GetComponent<RectTransform>().sizeDelta.x / 2f + 10) * canvas.transform.localScale.x, mousePosition.y + (canvas.GetComponent<RectTransform>().sizeDelta.y / 2f + 10) * canvas.transform.localScale.y);
			}
		}

		public void InitializeSelectedContainerIndicator() {
			selectedContainerIndicator = Object.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
			SpriteRenderer sCISR = selectedContainerIndicator.GetComponent<SpriteRenderer>();
			sCISR.sprite = GameManager.resourceM.selectionCornersSprite;
			sCISR.name = "SelectedContainerIndicator";
			sCISR.sortingOrder = 20; // Selected Container Indicator Sprite
			sCISR.color = new Color(1f, 1f, 1f, 0.75f);
			selectedContainerIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;
			selectedContainerIndicator.SetActive(false);
		}

		private readonly List<ReservedResourcesColonistElement> containerReservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();

		private readonly List<InventoryElement> containerInventoryElements = new List<InventoryElement>();

		public void SetSelectedContainerInfo() {
			selectedContainerIndicator.SetActive(false);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Object.Destroy(reservedResourceElement.obj);
				}

				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Object.Destroy(reservedResourcesColonistElement.obj);
			}

			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				Object.Destroy(inventoryElement.obj);
			}

			containerInventoryElements.Clear();

			GameObject weightSliderPanel = selectedContainerInventoryPanel.transform.Find("SliderSplitter-Panel/SelectedContainerInventoryWeightSlider-Panel").gameObject;
			GameObject volumeSliderPanel = selectedContainerInventoryPanel.transform.Find("SliderSplitter-Panel/SelectedContainerInventoryVolumeSlider-Panel").gameObject;

			if (selectedContainer != null) {
				selectedContainerIndicator.SetActive(true);
				selectedContainerIndicator.transform.position = selectedContainer.obj.transform.position;

				selectedContainerInventoryPanel.SetActive(true);

				selectedContainerInventoryPanel.transform.Find("SelectedContainerInventoryName-Text").GetComponent<Text>().text = selectedContainer.prefab.name;
				selectedContainerInventoryPanel.transform.Find("SelectedContainerSprite-Image").GetComponent<Image>().sprite = selectedContainer.obj.GetComponent<SpriteRenderer>().sprite;

				if (selectedContainer.GetInventory().maxWeight != int.MaxValue) {
					weightSliderPanel.SetActive(true);

					Slider weightSlider = weightSliderPanel.transform.Find("SelectedContainerInventoryWeight-Slider").GetComponent<Slider>();

					weightSlider.minValue = 0;
					weightSlider.maxValue = selectedContainer.GetInventory().maxWeight;
					weightSlider.value = selectedContainer.GetInventory().UsedWeight();
				} else {
					weightSliderPanel.SetActive(false);
				}

				if (selectedContainer.GetInventory().maxVolume != int.MaxValue) {
					volumeSliderPanel.SetActive(true);

					Slider volumeSlider = volumeSliderPanel.transform.Find("SelectedContainerInventoryVolume-Slider").GetComponent<Slider>();

					volumeSlider.minValue = 0;
					volumeSlider.maxValue = selectedContainer.GetInventory().maxVolume;
					volumeSlider.value = selectedContainer.GetInventory().UsedVolume();
				} else {
					volumeSliderPanel.SetActive(false);
				}

				foreach (ResourceManager.ReservedResources rr in selectedContainer.GetInventory().reservedResources) {
					containerReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.human, rr, selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel")));
				}

				foreach (ResourceManager.ResourceAmount ra in selectedContainer.GetInventory().resources.OrderByDescending(ra => ra.amount)) {
					InventoryElement inventoryElement = new InventoryElement(ra, selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel"));
					inventoryElement.obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200);
					containerInventoryElements.Add(inventoryElement);
				}
			} else {
				weightSliderPanel.SetActive(true);
				volumeSliderPanel.SetActive(true);

				selectedContainerInventoryPanel.SetActive(false);
			}
		}

		public void UpdateSelectedContainerInfo() {
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				inventoryElement.Update();
			}
		}

		public void InitializeSelectedTradingPostIndicator() {
			selectedTradingPostIndicator = Object.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
			SpriteRenderer sTPISR = selectedTradingPostIndicator.GetComponent<SpriteRenderer>();
			sTPISR.sprite = GameManager.resourceM.selectionCornersSprite;
			sTPISR.name = "SelectedTradingPostIndicator";
			sTPISR.sortingOrder = 20; // Selected Trading Post Indicator Sprite
			sTPISR.color = new Color(1f, 1f, 1f, 0.75f);
			selectedTradingPostIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;
			selectedTradingPostIndicator.SetActive(false);
		}

		private readonly List<ResourceTransferElement> tradingPostResourceTransferElements = new List<ResourceTransferElement>();

		private readonly List<ReservedResourcesColonistElement> tradingPostReservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();

		private readonly List<ResourceTransferElement> tradingPostInventoryElements = new List<ResourceTransferElement>();

		public void SetSelectedTradingPostInfo() {
			foreach (ResourceTransferElement resourceTransferElement in tradingPostResourceTransferElements) {
				Object.Destroy(resourceTransferElement.obj);
			}

			tradingPostResourceTransferElements.Clear();
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in tradingPostReservedResourcesColonistElements) {
				foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Object.Destroy(reservedResourceElement.obj);
				}

				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Object.Destroy(reservedResourcesColonistElement.obj);
			}

			tradingPostReservedResourcesColonistElements.Clear();
			foreach (ResourceTransferElement inventoryElement in tradingPostInventoryElements) {
				Object.Destroy(inventoryElement.obj);
			}

			tradingPostInventoryElements.Clear();
			selectedTradingPostPanel.transform.Find("AvailableResources-Panel/TransferIn-Button").GetComponent<Button>().onClick.RemoveAllListeners();
			selectedTradingPostPanel.transform.Find("Inventory-Panel/TransferOut-Button").GetComponent<Button>().onClick.RemoveAllListeners();

			selectedTradingPostPanel.SetActive(selectedTradingPost != null);
			selectedTradingPostIndicator.SetActive(selectedTradingPost != null);

			if (selectedTradingPost != null) {
				selectedTradingPostIndicator.transform.position = selectedTradingPost.obj.transform.position;

				selectedTradingPostPanel.transform.Find("Name-Text").GetComponent<Text>().text = selectedTradingPost.prefab.name;
				selectedTradingPostPanel.transform.Find("Sprite-Image").GetComponent<Image>().sprite = selectedTradingPost.obj.GetComponent<SpriteRenderer>().sprite;

				// Available Resources
				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/SliderSplitter-Panel/PlannedSpaceWeight-Slider").GetComponent<Slider>().minValue = 0;
				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/SliderSplitter-Panel/PlannedSpaceWeight-Slider").GetComponent<Slider>().maxValue = selectedTradingPost.GetInventory().maxWeight;

				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/SliderSplitter-Panel/PlannedSpaceVolume-Slider").GetComponent<Slider>().minValue = 0;
				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/SliderSplitter-Panel/PlannedSpaceVolume-Slider").GetComponent<Slider>().maxValue = selectedTradingPost.GetInventory().maxVolume;

				foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources()) {
					if (resource.GetAvailableAmount() > 0) {
						tradingPostResourceTransferElements.Add(new ResourceTransferElement(resource, selectedTradingPost, ResourceTransferElement.TransferType.In, selectedTradingPostPanel.transform.Find("AvailableResources-Panel/AvailableResources-ScrollPanel/AvailableResourcesList-Panel")));
					}
				}

				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/TransferIn-Button").GetComponent<Button>().onClick.AddListener(delegate {
					Job job = new Job(JobPrefab.GetJobPrefabByName("TransferResources"), selectedTradingPost.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.TransferResources), null, 0);
					foreach (ResourceTransferElement rte in tradingPostResourceTransferElements) {
						if (rte.transferAmount > 0) {
							job.requiredResources.Add(new ResourceManager.ResourceAmount(rte.resource, rte.transferAmount));
						}
					}

					if (job.requiredResources.Count > 0) {
						GameManager.jobM.CreateJob(job);
					}
				});

				// Inventory
				selectedTradingPostPanel.transform.Find("Inventory-Panel/SliderSplitter-Panel/InventorySpaceWeight-Slider").GetComponent<Slider>().minValue = 0;
				selectedTradingPostPanel.transform.Find("Inventory-Panel/SliderSplitter-Panel/InventorySpaceWeight-Slider").GetComponent<Slider>().maxValue = selectedTradingPost.GetInventory().maxWeight;
				selectedTradingPostPanel.transform.Find("Inventory-Panel/SliderSplitter-Panel/InventorySpaceWeight-Slider").GetComponent<Slider>().value = selectedTradingPost.GetInventory().UsedWeight();

				selectedTradingPostPanel.transform.Find("Inventory-Panel/SliderSplitter-Panel/InventorySpaceVolume-Slider").GetComponent<Slider>().minValue = 0;
				selectedTradingPostPanel.transform.Find("Inventory-Panel/SliderSplitter-Panel/InventorySpaceVolume-Slider").GetComponent<Slider>().maxValue = selectedTradingPost.GetInventory().maxVolume;
				selectedTradingPostPanel.transform.Find("Inventory-Panel/SliderSplitter-Panel/InventorySpaceVolume-Slider").GetComponent<Slider>().value = selectedTradingPost.GetInventory().UsedVolume();

				//selectedTradingPostPanel.transform.Find("Inventory-Panel/InventorySpacePercentage-Text").GetComponent<Text>().text = Mathf.RoundToInt((inventorySpace / (float)selectedTradingPost.prefab.maxInventoryAmount) * 100) + "%";

				foreach (ResourceManager.ReservedResources rr in selectedTradingPost.GetInventory().reservedResources) {
					tradingPostReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.human, rr, selectedTradingPostPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryResourcesList-Panel")));
				}

				foreach (ResourceManager.ResourceAmount ra in selectedTradingPost.GetInventory().resources) {
					ResourceTransferElement inventoryElement = new ResourceTransferElement(ra.resource, selectedTradingPost, ResourceTransferElement.TransferType.Out, selectedTradingPostPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryResourcesList-Panel"));
					tradingPostInventoryElements.Add(inventoryElement);
				}

				selectedTradingPostPanel.transform.Find("Inventory-Panel/TransferOut-Button").GetComponent<Button>().onClick.AddListener(delegate {
					Job job = new Job(JobPrefab.GetJobPrefabByName("CollectResources"), selectedTradingPost.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectResources), null, 0) {
						transferResources = new List<ResourceManager.ResourceAmount>()
					};
					foreach (ResourceTransferElement rte in tradingPostInventoryElements) {
						if (rte.transferAmount > 0) {
							job.transferResources.Add(new ResourceManager.ResourceAmount(rte.resource, rte.transferAmount));
							rte.transferAmountInput.text = "0";
						}
					}

					if (job.transferResources.Count > 0) {
						GameManager.jobM.CreateJob(job);
					}
				});

				UpdateSelectedTradingPostInfo();
			}
		}

		public void UpdateSelectedTradingPostInfo() {
			if (selectedTradingPost != null) {
				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/TransferIn-Button").GetComponent<Button>().interactable = tradingPostResourceTransferElements.Sum(rte => rte.transferAmount) > 0;
				selectedTradingPostPanel.transform.Find("Inventory-Panel/TransferOut-Button").GetComponent<Button>().interactable = tradingPostInventoryElements.Sum(rte => rte.transferAmount) > 0;

				int plannedSpaceWeight = selectedTradingPost.GetInventory().UsedWeight() + tradingPostResourceTransferElements.Sum(rte => rte.transferAmount * rte.resource.weight);
				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/SliderSplitter-Panel/PlannedSpaceWeight-Slider").GetComponent<Slider>().value = plannedSpaceWeight;

				int plannedSpaceVolume = selectedTradingPost.GetInventory().UsedVolume() + tradingPostResourceTransferElements.Sum(rte => rte.transferAmount * rte.resource.volume);
				selectedTradingPostPanel.transform.Find("AvailableResources-Panel/SliderSplitter-Panel/PlannedSpaceVolume-Slider").GetComponent<Slider>().value = plannedSpaceVolume;

				//selectedTradingPostPanel.transform.Find("AvailableResources-Panel/PlannedSpacePercentage-Text").GetComponent<Text>().text = Mathf.RoundToInt((plannedSpace / (float)selectedTradingPost.prefab.maxInventoryAmount) * 100) + "%";

				foreach (ResourceTransferElement resourceTransferElement in tradingPostResourceTransferElements) {
					resourceTransferElement.Update();
				}
			}
		}

		/*
		public void DisableAdminPanels(GameObject parentObj) {
			if (professionsList.activeSelf && parentObj != professionsList) {
				DisableProfessionsList();
			}
			if (objectPrefabsList.activeSelf && parentObj != objectPrefabsList) {
				DisableObjectPrefabsList();
			}
			if (clothesMenuPanel.activeSelf && parentObj != clothesMenuPanel) {
				DisableClothesList();
			}
			if (resourcesMenuPanel.activeSelf && parentObj != resourcesMenuPanel) {
				DisableResourcesList();
			}
		}
		*/

		public readonly List<ProfessionColumn> professionColumns = new List<ProfessionColumn>();
		public readonly List<ColonistProfessionsRow> colonistProfessionsRows = new List<ColonistProfessionsRow>();
		public readonly List<GameObject> colonistProfessionsRowBackgrounds = new List<GameObject>();

		public class ProfessionColumn {
			public ProfessionPrefab professionPrefab;
			public GameObject obj;

			public Dictionary<Colonist, Button> colonistToPriorityButtons = new Dictionary<Colonist, Button>();

			public ProfessionColumn(ProfessionPrefab professionPrefab, Transform parent, int index) {
				this.professionPrefab = professionPrefab;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ProfessionColumn-Panel"), parent, false);

				obj.transform.Find("ProfessionName-Text").GetComponent<Text>().text = professionPrefab.name;
				if (index % 2 == 1) {
					obj.transform.Find("ProfessionName-Text").GetComponent<Text>().alignment = TextAnchor.LowerCenter;
				}
			}

			public void AddButton(Profession profession) {
				GameObject buttonObj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/Priority-Button"), obj.transform, false);
				Button button = buttonObj.GetComponent<Button>();

				UpdateButton(button, profession);

				button.GetComponent<HandleClickScript>().Initialize(delegate {
					profession.DecreasePriority();
					UpdateButton(button, profession);
				}, null, delegate {
					profession.IncreasePriority();
					UpdateButton(button, profession);
				});

				colonistToPriorityButtons.Add(profession.colonist, button);
			}

			public void UpdateButton(Button button, Profession profession) {
				int priority = profession.GetPriority();

				button.transform.Find("Text").GetComponent<Text>().text = priority == 0 ? string.Empty : priority.ToString();

				button.transform.Find("Text").GetComponent<Text>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen), ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed), (priority - 1f) / (ProfessionPrefab.maxPriority - 1f));

				ColonistManager.SkillInstance highestSkillInstance = GameManager.colonistM.FindHighestSkillInstance(GameManager.colonistM.GetSkillPrefabFromEnum(profession.prefab.relatedSkill));

				if (highestSkillInstance.colonist == profession.colonist) {
					button.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightYellow);
				} else {
					ColonistManager.SkillInstance skillInstance = profession.colonist.GetSkillFromEnum(profession.prefab.relatedSkill);

					button.GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50), ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen), skillInstance.CalculateTotalSkillLevel() / (highestSkillInstance.CalculateTotalSkillLevel() > 0 ? highestSkillInstance.CalculateTotalSkillLevel() : 1));
				}
			}

			public void RemoveButton(Colonist colonist) {
				if (colonistToPriorityButtons.ContainsKey(colonist)) {
					Object.Destroy(colonistToPriorityButtons[colonist].gameObject);
				}

				colonistToPriorityButtons.Remove(colonist);
			}
		}

		public class ColonistProfessionsRow {
			public Colonist colonist;
			public GameObject obj;

			public ColonistProfessionsRow(Colonist colonist, Transform parent) {
				this.colonist = colonist;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistProfessionsRow-Panel"), parent, false);

				obj.transform.Find("Colonist-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });

				obj.transform.Find("Colonist-Button/Text").GetComponent<Text>().text = colonist.name;
				obj.transform.Find("Colonist-Button/Image").GetComponent<Image>().sprite = colonist.moveSprites[0];
			}

			public void Destroy() {
				Object.Destroy(obj);
			}
		}

		public void CreateProfessionsMenu() {
			int index = 0;
			foreach (ProfessionPrefab profession in ProfessionPrefab.professionPrefabs) {
				professionColumns.Add(new ProfessionColumn(profession, professionsMenu.transform.Find("ProfessionsColumns-Panel"), index));
				index++;
			}

			professionsMenu.SetActive(false);
		}

		public void SetProfessionsMenu() {
			// Removal

			foreach (ColonistProfessionsRow colonistProfessionsRow in colonistProfessionsRows) {
				foreach (ProfessionColumn professionColumn in professionColumns) {
					professionColumn.RemoveButton(colonistProfessionsRow.colonist);
				}

				colonistProfessionsRow.Destroy();
			}

			colonistProfessionsRows.Clear();
			foreach (GameObject colonistProfessionsRowBackground in colonistProfessionsRowBackgrounds) {
				Object.Destroy(colonistProfessionsRowBackground);
			}

			colonistProfessionsRowBackgrounds.Clear();

			// Creation

			foreach (Colonist colonist in Colonist.colonists) {
				colonistProfessionsRows.Add(new ColonistProfessionsRow(colonist, professionsMenu.transform.Find("ColonistsColumn-Panel")));

				colonistProfessionsRowBackgrounds.Add(Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistProfessionsRowBackground-Panel"), professionsMenu.transform.Find("ColonistProfessionsRowBackgrounds-Panel"), false));

				foreach (Profession profession in colonist.professions) {
					professionColumns.Find(pc => pc.professionPrefab == profession.prefab).AddButton(profession);
				}
			}
		}

		public void UpdateProfessionsMenu() {
			foreach (Colonist colonist in Colonist.colonists) {
				if (colonistProfessionsRows.Find(cpr => cpr.colonist == colonist) == null) {
					SetProfessionsMenu();
				}
			}

			foreach (ColonistProfessionsRow colonistProfessionsRow in colonistProfessionsRows) {
				if (Colonist.colonists.Find(c => c == colonistProfessionsRow.colonist) == null) {
					SetProfessionsMenu();
				}
			}
		}

		public void ToggleProfessionsMenu() {
			professionsMenu.SetActive(!professionsMenu.activeSelf);

			UpdateProfessionsMenu();
		}

		public void SetSelectedTraderMenu() {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Trader selectedTrader) {
				Caravan caravan = selectedTrader.caravan;

				selectedTraderMenu.SetActive(true);

				selectedTraderMenu.transform.Find("TraderBaseSprite-Image").GetComponent<Image>().sprite = selectedTrader.moveSprites[0];

				selectedTraderMenu.transform.Find("TraderName-Text").GetComponent<Text>().text = selectedTrader.name;

				selectedTraderMenu.transform.Find("TraderAffiliationName-Text").GetComponent<Text>().text = "Trader of " + caravan.location.name;
				selectedTraderMenu.transform.Find("TraderCurrentAction-Text").GetComponent<Text>().text = caravan.leaving ? "Leaving the Area" : "Ready to Trade";
				selectedTraderMenu.transform.Find("TraderStoredAction-Text").GetComponent<Text>().text = string.Empty;

				selectedTraderMenu.transform.Find("TradeWithCaravan-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.caravanM.SetSelectedCaravan(caravan); });
			} else {
				selectedTraderMenu.SetActive(false);
			}
		}

		public void UpdateSelectedTraderMenu() {
			if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is Trader selectedTrader) {
				selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedTrader.health * 100);
				selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed), ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen), selectedTrader.health);
				selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed), ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen), selectedTrader.health);
				selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedTrader.health * 100) + "%";
			}
		}



		public class ObjectPrefabElement {
			public ResourceManager.ObjectPrefab prefab;
			public GameObject obj;
			public GameObject objectInstancesList;
			public List<ObjectInstanceElement> instanceElements = new List<ObjectInstanceElement>();

			public ObjectPrefabElement(ResourceManager.ObjectPrefab prefab, Transform parent) {
				this.prefab = prefab;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectPrefab-Button"), parent, false);

				//obj.transform.Find("ObjectPrefabSprite-Panel/ObjectPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite; // TODO Fix
				obj.transform.Find("ObjectPrefabName-Text").GetComponent<Text>().text = prefab.name;

				objectInstancesList = obj.transform.Find("ObjectInstancesList-ScrollPanel").gameObject;
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					objectInstancesList.SetActive(!objectInstancesList.activeSelf);
					if (objectInstancesList.activeSelf) {
						objectInstancesList.transform.SetParent(GameManager.uiMOld.canvas.transform);
						foreach (ObjectPrefabElement objectPrefabElement in GameManager.uiMOld.objectPrefabElements) {
							if (objectPrefabElement != this) {
								objectPrefabElement.objectInstancesList.SetActive(false);
							}
						}
					} else {
						objectInstancesList.transform.SetParent(obj.transform);
					}
				});

				AddObjectInstancesList(true);

				Update();
			}

			public void AddObjectInstancesList(bool newList) {
				RemoveObjectInstances();
				bool objectInstancesListState = objectInstancesList.activeSelf;
				if (newList) {
					objectInstancesListState = false;
				}

				objectInstancesList.SetActive(true);
				foreach (ResourceManager.ObjectInstance instance in GameManager.resourceM.GetObjectInstancesByPrefab(prefab)) {
					instanceElements.Add(new ObjectInstanceElement(instance, objectInstancesList.transform.Find("ObjectInstancesList-Panel")));
				}

				objectInstancesList.SetActive(objectInstancesListState);
				Update();
			}

			public void Remove() {
				RemoveObjectInstances();
				Object.Destroy(obj);
			}

			public void RemoveObjectInstances() {
				foreach (ObjectInstanceElement instance in instanceElements) {
					Object.Destroy(instance.obj);
				}

				instanceElements.Clear();
			}

			public void Update() {
				obj.transform.Find("ObjectPrefabAmount-Text").GetComponent<Text>().text = instanceElements.Count.ToString();
			}
		}

		public class ObjectInstanceElement {
			public ResourceManager.ObjectInstance instance;
			public GameObject obj;

			public ObjectInstanceElement(ResourceManager.ObjectInstance instance, Transform parent) {
				this.instance = instance;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectInstance-Button"), parent, false);

				obj.transform.Find("ObjectInstanceSprite-Panel/ObjectInstanceSprite-Image").GetComponent<Image>().sprite = instance.obj.GetComponent<SpriteRenderer>().sprite;
				obj.transform.Find("ObjectInstanceName-Text").GetComponent<Text>().text = instance.prefab.name;
				obj.transform.Find("TilePosition-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(instance.tile.obj.transform.position.x) + ", " + Mathf.FloorToInt(instance.tile.obj.transform.position.y) + ")";

				obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.cameraM.SetCameraPosition(instance.obj.transform.position); });

				ResourceManager.Container container = GameManager.resourceM.containers.Find(findContainer => findContainer == instance);
				if (container != null) {
					obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiMOld.SetSelectedContainer(container); });
				}

				ResourceManager.TradingPost tradingPost = GameManager.resourceM.tradingPosts.Find(tp => tp == instance);
				if (tradingPost != null) {
					obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiMOld.SetSelectedTradingPost(tradingPost); });
				}

				ResourceManager.CraftingObject mto = GameManager.resourceM.craftingObjectInstances.Find(findMTO => findMTO == instance);
				if (mto != null) {
					obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiMOld.SetSelectedCraftingObject(mto); });
				}
			}
		}

		public List<ObjectPrefabElement> objectPrefabElements = new List<ObjectPrefabElement>();

		public void ToggleObjectPrefabsList(bool changeActiveState) {
			if (changeActiveState) {
				//DisableAdminPanels(objectPrefabsList);
				objectPrefabsList.SetActive(!objectPrefabsList.activeSelf);
			}

			foreach (ObjectPrefabElement objectPrefabElement in objectPrefabElements) {
				objectPrefabElement.objectInstancesList.transform.SetParent(objectPrefabElement.obj.transform);
				objectPrefabElement.objectInstancesList.SetActive(false);
			}

			if (objectPrefabElements.Count <= 0) {
				objectPrefabsList.SetActive(false);
			}
		}

		public void DisableObjectPrefabsList() {
			objectPrefabsList.SetActive(!objectPrefabsList.activeSelf);
			ToggleObjectPrefabsList(false);
		}

		public enum ChangeTypeEnum {
			Add,
			Update,
			Remove
		};

		public void ChangeObjectPrefabElements(ChangeTypeEnum changeType, ResourceManager.ObjectPrefab prefab) {
			if (prefab.subGroupType == ResourceManager.ObjectSubGroupEnum.None || prefab.groupType == ResourceManager.ObjectGroupEnum.Command) {
				return;
			}

			switch (changeType) {
				case ChangeTypeEnum.Add:
					AddObjectPrefabElement(prefab);
					break;
				case ChangeTypeEnum.Update:
					UpdateObjectPrefabElement(prefab);
					break;
				case ChangeTypeEnum.Remove:
					RemoveObjectPrefabElement(prefab);
					break;
			}
		}

		private void AddObjectPrefabElement(ResourceManager.ObjectPrefab prefab) {
			ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
			if (objectPrefabElement == null) {
				objectPrefabElements.Add(new ObjectPrefabElement(prefab, objectPrefabsList.transform.Find("ObjectPrefabsList-Panel")));
			} else {
				UpdateObjectPrefabElement(prefab);
			}
		}

		private void UpdateObjectPrefabElement(ResourceManager.ObjectPrefab prefab) {
			ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
			if (objectPrefabElement != null) {
				objectPrefabElement.AddObjectInstancesList(false);
			}
		}

		private void RemoveObjectPrefabElement(ResourceManager.ObjectPrefab prefab) {
			ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
			if (objectPrefabElement != null) {
				objectPrefabElement.Remove();
				objectPrefabElements.Remove(objectPrefabElement);
			}

			if (objectPrefabElements.Count <= 0) {
				objectPrefabsList.SetActive(true);
				ToggleObjectPrefabsList(false);
			}
		}

		public class ResourceElement {
			public ResourceManager.Resource resource;
			public GameObject obj;

			public bool filterActive = true; // True if the resources filter deems this element visible
			public bool amountActive; // True if the total world amount is > 0

			public ResourceElement(ResourceManager.Resource resource, Transform parent) {
				this.resource = resource;

				obj = Object.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceListResourceElement-Panel"), parent, false);

				obj.transform.Find("Name").GetComponent<Text>().text = resource.name;

				obj.transform.Find("Image").GetComponent<Image>().sprite = resource.image;

				Update();
			}

			private int availableAmountPrev = 0;

			public void Update() {
				amountActive = resource.GetWorldTotalAmount() > 0;
				obj.SetActive(filterActive && amountActive);

				if (resource.GetWorldTotalAmount() != resource.GetAvailableAmount()) {
					obj.transform.Find("Amount").GetComponent<Text>().text = resource.GetAvailableAmount() + " / " + resource.GetWorldTotalAmount();
				} else {
					obj.transform.Find("Amount").GetComponent<Text>().text = resource.GetWorldTotalAmount().ToString();
				}

				availableAmountPrev = resource.GetAvailableAmount();

				if (availableAmountPrev != resource.GetAvailableAmount()) {
					if (resource.GetAvailableAmount() > 0) {
						obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);
					} else {
						obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.Grey150);
					}
				}
			}
		}

		public List<ResourceElement> clothingElements = new List<ResourceElement>();

		public void CreateClothesList() {
			foreach (ResourceElement clothingElement in clothingElements) {
				Object.Destroy(clothingElement.obj);
			}

			clothingElements.Clear();
			Transform clothesListParent = clothesList.transform.Find("ClothesList-Panel");
			foreach (ResourceManager.Clothing clothing in ResourceManager.GetResourcesInClass(ResourceManager.ResourceClassEnum.Clothing).Select(r => (ResourceManager.Clothing)r)) {
				ResourceElement newClothingElement = new ResourceElement(clothing, clothesListParent);
				newClothingElement.resource.resourceListElement = newClothingElement;
				clothingElements.Add(newClothingElement);
			}
		}

		public void SetClothesList() {
			//DisableAdminPanels(clothesMenuPanel);
			clothesMenuPanel.SetActive(!clothesMenuPanel.activeSelf);
			if (clothingElements.Count <= 0) {
				clothesMenuPanel.SetActive(false);
			}
		}

		public void FilterClothesList(string searchString) {
			foreach (ResourceElement clothingElement in clothingElements) {
				clothingElement.filterActive = string.IsNullOrEmpty(searchString) || clothingElement.resource.name.ToLower().Contains(searchString.ToLower());
			}
		}

		public void DisableClothesList() {
			clothesMenuPanel.SetActive(false);
		}

		public void UpdateClothesList() {
			foreach (ResourceElement clothingElement in clothingElements) {
				clothingElement.Update();
			}

			int index = 0;
			foreach (ResourceElement clothingElement in clothingElements.OrderByDescending(ce => ce.resource.GetWorldTotalAmount()).ThenBy(ce => ce.resource.name)) {
				clothingElement.obj.transform.SetSiblingIndex(index);
				index += 1;
			}
		}

		public List<ResourceElement> resourceElements = new List<ResourceElement>();

		public void CreateResourcesList() {
			foreach (ResourceElement resourceElement in resourceElements) {
				Object.Destroy(resourceElement.obj);
			}

			resourceElements.Clear();
			Transform resourcesListParent = resourcesList.transform.Find("ResourcesList-Panel");
			foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources().Where(r => !r.classes.Contains(ResourceManager.ResourceClassEnum.Clothing))) {
				ResourceElement newResourceElement = new ResourceElement(resource, resourcesListParent);
				newResourceElement.resource.resourceListElement = newResourceElement;
				resourceElements.Add(newResourceElement);
			}
		}

		public void SetResourcesList() {
			//DisableAdminPanels(resourcesMenuPanel);
			resourcesMenuPanel.SetActive(!resourcesMenuPanel.activeSelf);
			if (resourceElements.Count <= 0) {
				resourcesMenuPanel.SetActive(false);
			}
		}

		public void FilterResourcesList(string searchString) {
			foreach (ResourceElement resourceElement in resourceElements) {
				resourceElement.filterActive = string.IsNullOrEmpty(searchString) || resourceElement.resource.name.ToLower().Contains(searchString.ToLower());
			}
		}

		public void DisableResourcesList() {
			resourcesMenuPanel.SetActive(false);
		}

		public void UpdateResourcesList() {
			foreach (ResourceElement resourceElement in resourceElements) {
				resourceElement.Update();
			}

			int index = 0;
			foreach (ResourceElement resourceElement in resourceElements.OrderByDescending(re => re.resource.GetWorldTotalAmount()).ThenBy(re => re.resource.name)) {
				resourceElement.obj.transform.SetSiblingIndex(index);
				index += 1;
			}
		}

		public void InitializeSelectedCraftingObjectIndicator() {
			selectedCraftingObjectIndicator = Object.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
			SpriteRenderer spriteRenderer = selectedCraftingObjectIndicator.GetComponent<SpriteRenderer>();
			spriteRenderer.sprite = GameManager.resourceM.selectionCornersSprite;
			spriteRenderer.name = "SelectedCraftingObjectIndicator";
			spriteRenderer.sortingOrder = 20; // Selected MTO Indicator Sprite
			spriteRenderer.color = new Color(1f, 1f, 1f, 0.75f);
			selectedCraftingObjectIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;
			selectedCraftingObjectIndicator.SetActive(false);
		}

		private static readonly List<CraftingResourceElement> craftingResourceElements = new List<CraftingResourceElement>();

		public void SetSelectedCraftingObjectPanel() {
			selectedCraftingObjectPanel.SetActive(selectedCraftingObject != null);
			selectedCraftingObjectIndicator.SetActive(selectedCraftingObject != null);

			craftingResourceElements.Clear();

			selectedCraftingObjectSelectResourcesPanel.SetActive(false);
			selectedCraftingObjectSelectFuelsPanel.SetActive(false);

			if (selectedCraftingObject != null) {
				selectedCraftingObjectIndicator.transform.position = selectedCraftingObject.tile.obj.transform.position;

				GameObject settingsPanel = selectedCraftingObjectPanel.transform.Find("Settings-Panel").gameObject;

				settingsPanel.transform.Find("SelectedCraftingObjectInfo-Panel/Name-Text").GetComponent<Text>().text = selectedCraftingObject.prefab.GetInstanceNameFromVariation(selectedCraftingObject.variation);
				settingsPanel.transform.Find("SelectedCraftingObjectInfo-Panel/Sprite-Panel/Sprite-Image").GetComponent<Image>().sprite = selectedCraftingObject.prefab.GetBaseSpriteForVariation(selectedCraftingObject.variation);

				selectedCraftingObjectPanelSelectFuelsButton.gameObject.SetActive(selectedCraftingObject.prefab.usesFuel);

				Transform selectedResourcesPanel = settingsPanel.transform.Find("SelectResources-Button/SelectedResources-Panel");
				foreach (Transform child in selectedResourcesPanel) {
					Object.Destroy(child.gameObject);
				}

				Transform craftableResourcesPanel = settingsPanel.transform.Find("CraftableResources-Panel/CraftableResources-ScrollPanel/CraftableResourcesList-Panel");
				foreach (Transform child in craftableResourcesPanel) {
					Object.Destroy(child.gameObject);
				}

				foreach (ResourceManager.CraftableResourceInstance resource in selectedCraftingObject.resources.OrderBy(r => r.priority.Get())) {
					GameObject selectedResourcePreview = Object.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/SelectedResourcePreview-Image"), selectedResourcesPanel, false);
					selectedResourcePreview.GetComponent<Image>().sprite = resource.resource.image;

					CraftingResourceElement craftableResourceElement = new CraftingResourceElement(resource, craftableResourcesPanel);
					craftingResourceElements.Add(craftableResourceElement);
				}

				Transform selectedFuelsPanel = settingsPanel.transform.Find("SelectFuels-Button/SelectedFuels-Panel");
				foreach (Transform child in selectedFuelsPanel) {
					Object.Destroy(child.gameObject);
				}

				foreach (ResourceManager.PriorityResourceInstance fuel in selectedCraftingObject.fuels.OrderBy(f => f.priority.Get())) {
					GameObject selectedFuelPreview = Object.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/SelectedResourcePreview-Image"), selectedFuelsPanel, false);
					selectedFuelPreview.GetComponent<Image>().sprite = fuel.resource.image;
				}

				UpdateSelectedCraftingObjectPanel();
			}
		}

		public void UpdateSelectedCraftingObjectPanel() {
			foreach (CraftingResourceElement craftingResourceElement in craftingResourceElements) {
				craftingResourceElement.Update();
			}

			if (selectedCraftingObject.active) {
				selectedCraftingObjectPanelActiveButtonText.text = "Enabled";
				if (selectedCraftingObject.resources.Any(resource => resource.enableable)) {
					selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
				} else {
					selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightOrange);
				}
			} else {
				selectedCraftingObjectPanelActiveButtonText.text = "Disabled";
				if (selectedCraftingObject.resources.Any(resource => resource.enableable)) {
					selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightOrange);
				} else {
					selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
				}
			}
		}

		private readonly List<CraftableResourceElement> craftableResourceElements = new List<CraftableResourceElement>();

		private void SetSelectedCraftingObjectSelectResourcesPanel() {
			selectedCraftingObjectSelectResourcesPanel.SetActive(!selectedCraftingObjectSelectResourcesPanel.activeSelf);

			foreach (CraftableResourceElement craftableResourceElement in craftableResourceElements) {
				craftableResourceElement.Remove();
			}

			craftableResourceElements.Clear();

			if (selectedCraftingObjectSelectResourcesPanel.activeSelf) {
				foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResourcesByCraftingObject(selectedCraftingObject).OrderBy(r => r.name)) {
					craftableResourceElements.Add(new CraftableResourceElement(PriorityResourceElement.Type.Resource, resource, selectedCraftingObjectSelectResourcesPanel.transform.Find("SelectResources-ScrollPanel/SelectResourcesList-Panel"), selectedCraftingObject));
				}
			} else {
				SetSelectedCraftingObjectPanel();
			}
		}

		private readonly List<FuelResourceElement> fuelResourceElements = new List<FuelResourceElement>();

		private void SetSelectedCraftingObjectSelectFuelsPanel() {
			selectedCraftingObjectSelectFuelsPanel.SetActive(!selectedCraftingObjectSelectFuelsPanel.activeSelf);

			foreach (FuelResourceElement fuelResourceElement in fuelResourceElements) {
				fuelResourceElement.Remove();
			}

			fuelResourceElements.Clear();

			if (selectedCraftingObjectSelectFuelsPanel.activeSelf) {
				foreach (ResourceManager.Resource resource in ResourceManager.GetResourcesInClass(ResourceManager.ResourceClassEnum.Fuel).OrderBy(r => r.fuelEnergy)) {
					fuelResourceElements.Add(new FuelResourceElement(PriorityResourceElement.Type.Fuel, resource, selectedCraftingObjectSelectFuelsPanel.transform.Find("SelectFuels-ScrollPanel/SelectFuelsList-Panel"), selectedCraftingObject));
				}
			} else {
				SetSelectedCraftingObjectPanel();
			}
		}

		private class FuelResourceElement : PriorityResourceElement {
			public FuelResourceElement(Type type, ResourceManager.Resource resource, Transform parent, ResourceManager.CraftingObject craftingObject) : base(type, resource, parent, craftingObject) {
			}

			public override void Update() {
				base.Update();

				panel.transform.Find("ActiveIndicator-Panel").GetComponent<Image>().color = craftingObject.GetFuelFromFuelResource(resource) != null ? resource.GetAvailableAmount() > 0 ? ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen) : ColourUtilities.GetColour(ColourUtilities.EColour.LightOrange) : ColourUtilities.GetColour(ColourUtilities.EColour.Clear);
			}

			public override void ChangePriority(int amount) {
				ResourceManager.PriorityResourceInstance fuel = craftingObject.GetFuelFromFuelResource(resource);
				if (fuel == null) {
					fuel = craftingObject.ToggleFuel(resource, 0);
				}

				int priority = fuel.priority.Change(amount);

				if (priority <= fuel.priority.min && fuel != null) {
					craftingObject.ToggleFuel(resource, 0);
				}

				UpdatePriorityButtonText(priority);

				Update();
			}
		}

		private class CraftingResourceElement {
			public ResourceManager.CraftableResourceInstance resource;

			public GameObject panel;
			public List<RequiredResourceElement> requiredResourceElements = new List<RequiredResourceElement>();

			private readonly Transform singleRunButton;
			private readonly Transform maintainStockButton;
			private readonly Transform continuousRunButton;

			private readonly Transform createAmountPanel;
			private readonly InputField createAmountInputField;
			private readonly Text remainingAmountText;

			public CraftingResourceElement(ResourceManager.CraftableResourceInstance resource, Transform parent) {
				this.resource = resource;

				panel = Object.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/CraftableResource-Panel"), parent, false);

				panel.transform.Find("ResourceData-Panel/Name").GetComponent<Text>().text = resource.resource.name;
				panel.transform.Find("ResourceData-Panel/Image").GetComponent<Image>().sprite = resource.resource.image;

				foreach (ResourceManager.ResourceAmount resourceAmount in resource.resource.craftingResources) {
					requiredResourceElements.Add(new RequiredResourceElement(resourceAmount, panel.transform.Find("RequiredResources-Panel")));
				}

				singleRunButton = panel.transform.Find("CreationMethod-Panel/CreationMethodsList-Panel/SingleRun-Button");
				maintainStockButton = panel.transform.Find("CreationMethod-Panel/CreationMethodsList-Panel/MaintainStock-Button");
				continuousRunButton = panel.transform.Find("CreationMethod-Panel/CreationMethodsList-Panel/ContinuousRun-Button");

				singleRunButton.GetComponent<Button>().onClick.AddListener(delegate {
					resource.ResetAmounts();
					SetCreationMethod(ResourceManager.CreationMethod.SingleRun);
				});
				maintainStockButton.GetComponent<Button>().onClick.AddListener(delegate {
					resource.ResetAmounts();
					SetCreationMethod(ResourceManager.CreationMethod.MaintainStock);
				});
				continuousRunButton.GetComponent<Button>().onClick.AddListener(delegate {
					resource.ResetAmounts();
					SetCreationMethod(ResourceManager.CreationMethod.ContinuousRun);
				});

				createAmountPanel = panel.transform.Find("CreateAmount-Panel");
				remainingAmountText = createAmountPanel.transform.Find("RemainingAmount-Text").GetComponent<Text>();
				createAmountInputField = createAmountPanel.transform.Find("CreateAmount-InputField").GetComponent<InputField>();

				createAmountInputField.onEndEdit.AddListener(delegate {
					if (int.TryParse(createAmountInputField.text, out int targetAmount)) {
						switch (resource.creationMethod) {
							case ResourceManager.CreationMethod.SingleRun:
								resource.UpdateTargetAmount(targetAmount);
								break;
							case ResourceManager.CreationMethod.MaintainStock:
								resource.UpdateTargetAmount(targetAmount);
								break;
							case ResourceManager.CreationMethod.ContinuousRun:
								resource.SetTargetAmount(0);
								break;
						}
					} else {
						resource.SetTargetAmount(0);
					}
				});

				SetCreationMethod(resource.creationMethod);
			}

			public void Update() {
				foreach (RequiredResourceElement requiredResourceElement in requiredResourceElements) {
					requiredResourceElement.Update();
				}

				remainingAmountText.text = resource.GetRemainingAmount().ToString();
			}

			public void SetCreationMethod(ResourceManager.CreationMethod creationMethod) {
				resource.creationMethod = creationMethod;

				singleRunButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);
				maintainStockButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);
				continuousRunButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);

				createAmountPanel.gameObject.SetActive(true);

				switch (resource.creationMethod) {
					case ResourceManager.CreationMethod.SingleRun:
						singleRunButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50);
						createAmountPanel.transform.Find("CreateAmount-Text").GetComponent<Text>().text = "Create Amount";
						break;
					case ResourceManager.CreationMethod.MaintainStock:
						maintainStockButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50);
						createAmountPanel.transform.Find("CreateAmount-Text").GetComponent<Text>().text = "Maintain Amount";
						break;
					case ResourceManager.CreationMethod.ContinuousRun:
						continuousRunButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50);
						createAmountPanel.gameObject.SetActive(false);
						break;
					default:
						Debug.LogError("Unknown creation method selected.");
						break;
				}

				createAmountInputField.text = resource.GetTargetAmount() > 0 ? resource.GetTargetAmount().ToString() : string.Empty;

				// The following lines (including the redundant-looking disable/enable) are required to get the ScrollRect to
				// update its size when enabling or disabling the createAmountPanel.
				Canvas.ForceUpdateCanvases();
				panel.GetComponent<VerticalLayoutGroup>().enabled = false;
				panel.GetComponent<VerticalLayoutGroup>().enabled = true;
			}
		}

		public void SetPauseMenuActive(bool active) {
			SetSettingsMenuActive(false);

			// pauseMenu.SetActive(active);

			// pauseSaveButton.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200);

			// GameManager.timeM.SetPaused(pauseMenu.activeSelf);
		}

		public void TogglePauseMenuButtons(bool state) {
			// pauseMenuButtons.SetActive(state);
			// pauseLabel.SetActive(pauseMenuButtons.activeSelf);
		}

		public enum UIScaleMode {
			ConstantPixelSize,
			ScaleWithScreenSize
		};

		public void SetSettingsMenuActive(bool active) {
			// settingsPanel.SetActive(active);
			//
			// ToggleMainMenuButtons(settingsPanel);
			// TogglePauseMenuButtons(!settingsPanel.activeSelf);
			// if (settingsPanel.activeSelf) {
			// 	GameObject resolutionSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/ResolutionSettings-Panel").gameObject;
			// 	Slider resolutionSlider = resolutionSettingsPanel.transform.Find("Resolution-Slider").GetComponent<Slider>();
			// 	resolutionSlider.minValue = 0;
			// 	resolutionSlider.maxValue = Screen.resolutions.Length - 1;
			// 	resolutionSlider.onValueChanged.AddListener(delegate {
			// 		Resolution r = Screen.resolutions[Mathf.RoundToInt(resolutionSlider.value)];
			// 		GameManager.persistenceM.settingsState.resolution = r;
			// 		GameManager.persistenceM.settingsState.resolutionWidth = r.width;
			// 		GameManager.persistenceM.settingsState.resolutionHeight = r.height;
			// 		GameManager.persistenceM.settingsState.refreshRate = r.refreshRate;
			// 		resolutionSettingsPanel.transform.Find("ResolutionValue-Text").GetComponent<Text>().text = GameManager.persistenceM.settingsState.resolutionWidth + " × " + GameManager.persistenceM.settingsState.resolutionHeight + " @ " + r.refreshRate + "hz";
			// 	});
			// 	resolutionSlider.value = Screen.resolutions.ToList().IndexOf(GameManager.persistenceM.settingsState.resolution);
			//
			// 	GameObject fullscreenSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/FullscreenSettings-Panel").gameObject;
			// 	Toggle fullscreenToggle = fullscreenSettingsPanel.transform.Find("Fullscreen-Toggle").GetComponent<Toggle>();
			// 	fullscreenToggle.onValueChanged.AddListener(delegate { GameManager.persistenceM.settingsState.fullscreen = fullscreenToggle.isOn; });
			// 	fullscreenToggle.isOn = GameManager.persistenceM.settingsState.fullscreen;
			//
			// 	GameObject UIScaleModeSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/UIScaleModeSettings-Panel").gameObject;
			// 	Toggle UIScaleModeToggle = UIScaleModeSettingsPanel.transform.Find("UIScaleMode-Toggle").GetComponent<Toggle>();
			// 	UIScaleModeToggle.onValueChanged.AddListener(delegate {
			// 		if (UIScaleModeToggle.isOn) {
			// 			GameManager.persistenceM.settingsState.scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			// 		} else {
			// 			GameManager.persistenceM.settingsState.scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			// 		}
			// 	});
			// 	if (GameManager.persistenceM.settingsState.scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize) {
			// 		UIScaleModeToggle.isOn = true;
			// 	} else if (GameManager.persistenceM.settingsState.scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) {
			// 		UIScaleModeToggle.isOn = false;
			// 	}
			// }
		}

		public void ExitToMenu() {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public bool IsPointerOverUI() {
			return EventSystem.current.IsPointerOverGameObject();
		}

		public List<RaycastResult> GetUIElementsUnderPointer() {
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current) {
				pointerId = -1
			};

			pointerEventData.position = Input.mousePosition;

			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, results);

			return results;
		}
	}
}
