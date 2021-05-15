using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : BaseManager {

	private GameManager startCoroutineReference;

	public void SetStartCoroutineReference(GameManager startCoroutineReference) {
		this.startCoroutineReference = startCoroutineReference;
	}

	public static readonly string gameVersionString = "Snowship " + PersistenceManager.gameVersion.Value;
	public static readonly string disclaimerText = "Snowship by Ryan White - rywh.itch.io/snowship\n<size=20>" + gameVersionString + "</size>";

	public bool playerTyping = false;

	public static string SplitByCapitals(string combinedString) {
		var r = new Regex(
			@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])",
			RegexOptions.IgnorePatternWhitespace);
		return r.Replace(combinedString, " ");
	}

	public static Color HexToColor(string hexString) {
		int r = int.Parse("" + hexString[0] + hexString[1], System.Globalization.NumberStyles.HexNumber);
		int g = int.Parse("" + hexString[2] + hexString[3], System.Globalization.NumberStyles.HexNumber);
		int b = int.Parse("" + hexString[4] + hexString[5], System.Globalization.NumberStyles.HexNumber);
		return new Color(r, g, b, 255f) / 255f;
	}

	public static string RemoveNonAlphanumericChars(string removeFromString) {
		return new Regex("[^a-zA-Z0-9 -]").Replace(removeFromString, string.Empty);
	}

	public enum Colours { 
		Clear, WhiteAlpha128, WhiteAlpha64, WhiteAlpha20, White, DarkRed, DarkGreen, LightRed, LightRed100, LightGreen, LightGreen100,
		LightGrey220, LightGrey200, LightGrey180, Grey150, Grey120, DarkGrey50, LightBlue, LightOrange, DarkOrange, DarkYellow, LightYellow, LightPurple, LightPurple100, DarkPurple
	};

	private static readonly Dictionary<Colours, Color> colourMap = new Dictionary<Colours, Color>() {
		{ Colours.Clear, new Color(255f, 255f, 255f, 0f) / 255f },
		{ Colours.WhiteAlpha128, new Color(255f, 255f, 255f, 128f) / 255f },
		{ Colours.WhiteAlpha64, new Color(255f, 255f, 255f, 64f) / 255f },
		{ Colours.WhiteAlpha20, new Color(255f, 255f, 255f, 20f) / 255f },
		{ Colours.White, new Color(255f, 255f, 255f, 255f) / 255f },
		{ Colours.DarkRed, new Color(192f, 57f, 43f, 255f) / 255f },
		{ Colours.DarkGreen, new Color(39f, 174f, 96f, 255f) / 255f },
		{ Colours.LightRed, new Color(231f, 76f, 60f, 255f) / 255f },
		{ Colours.LightRed100, new Color(231f, 76f, 60f, 100f) / 255f },
		{ Colours.LightGreen, new Color(46f, 204f, 113f, 255f) / 255f },
		{ Colours.LightGreen100, new Color(46f, 204f, 113f, 100f) / 255f },
		{ Colours.LightGrey220, new Color(220f, 220f, 220f, 255f) / 255f },
		{ Colours.LightGrey200, new Color(200f, 200f, 200f, 255f) / 255f },
		{ Colours.LightGrey180, new Color(180f, 180f, 180f, 255f) / 255f },
		{ Colours.Grey150, new Color(150f, 150f, 150f, 255f) / 255f },
		{ Colours.Grey120, new Color(120f, 120f, 120f, 255f) / 255f },
		{ Colours.DarkGrey50, new Color(50f, 50f, 50f, 255f) / 255f },
		{ Colours.LightBlue, new Color(52f, 152f, 219f, 255f) / 255f },
		{ Colours.LightOrange, new Color(230f, 126f, 34f, 255f) / 255f },
		{ Colours.DarkOrange, new Color(211f, 84f, 0f, 255f) / 255f },
		{ Colours.DarkYellow, new Color(216f, 176f, 15f, 255f) / 255f },
		{ Colours.LightYellow, new Color(241f, 196f, 15f, 255f) / 255f },
		{ Colours.LightPurple, new Color(155f, 89f, 182f, 255f) / 255f },
		{ Colours.LightPurple100, new Color(155f, 89f, 182f, 100f) / 255f },
		{ Colours.DarkPurple, new Color(142f, 68f, 173f, 255f) / 255f }
	};

	public static Color GetColour(Colours colourKey) {
		return colourMap[colourKey];
	}

	public static Color ChangeAlpha(Color colour, float alpha) {
		return new Color(colour.r, colour.g, colour.b, alpha);
	}

	public static bool IsAlphanumericWithSpaces(string text) {
		return Regex.IsMatch(text, @"^[A-Za-z0-9 ]*[A-Za-z0-9][A-Za-z0-9 ]*$");
	}

	public interface ISelectable {
		void Select();

		void Deselect();
	}

	public int screenWidth = 0;
	public int screenHeight = 0;

	public GameObject canvas;

	public void SetupUI() {

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		canvas = GameObject.Find("Canvas");

		SetupMainMenu();

		SetupLoadUniverseUI();
		SetupCreateUniverseUI();
		SetupLoadPlanetUI();
		SetupCreatePlanetUI();
		SetupLoadColonyUI();
		SetupCreateColonyUI();
		SetupLoadSaveUI();

		SetupLoadingScreen();

		SetupGameUI();

		SetupPauseMenu();

		SetLoadUniverseActive(false);
		SetCreateUniverseActive(false);
		SetLoadPlanetActive(false);
		SetCreatePlanetActive(false);
		SetLoadColonyActive(false);
		SetCreateColonyActive(false);
		SetLoadSaveActive(false);

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

		SetTradeMenu();

		SetCaravanElements();
		SetJobElements();

		SetGameUIActive(false);
		SetLoadingScreenActive(false);
	}

	public GameObject mainMenu;
	private GameObject mainMenuBackground;
	private GameObject mainMenuButtonsPanel;
	private GameObject snowshipLogo;
	private GameObject darkBackground;

	private GameObject settingsPanel;

	private void SetupMainMenu() {
		mainMenu = canvas.transform.Find("MainMenu").gameObject;

		mainMenu.transform.Find("Disclaimer-Text").GetComponent<Text>().text = disclaimerText;

		mainMenuBackground = mainMenu.transform.Find("MainMenuBackground-Image").gameObject;
		snowshipLogo = mainMenu.transform.Find("SnowshipLogo-Image").gameObject;
		SetMainMenuBackground(true);

		darkBackground = mainMenu.transform.Find("DarkBackground-Image").gameObject;

		mainMenuButtonsPanel = mainMenu.transform.Find("MainMenuButtons-Panel").gameObject;

		settingsPanel = canvas.transform.Find("Settings-Panel").gameObject;
		settingsPanel.transform.Find("SettingsCancel-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSettingsMenuActive(false); });
		settingsPanel.transform.Find("SettingsApply-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.persistenceM.ApplySettings(); });
		settingsPanel.transform.Find("SettingsAccept-Button").GetComponent<Button>().onClick.AddListener(delegate {
			GameManager.persistenceM.ApplySettings();
			SetSettingsMenuActive(false);
		});

		mainMenuButtonsPanel.transform.Find("New-Button").GetComponent<Button>().onClick.AddListener(delegate { SetCreateUniverseActive(true); });

		Button continueButton = mainMenuButtonsPanel.transform.Find("Continue-Button").GetComponent<Button>();

		bool continueUniverseLoadable = GameManager.persistenceM.IsLastSaveUniverseLoadable();
		continueButton.interactable = continueUniverseLoadable;

		Image continuePreviewImage = continueButton.transform.Find("Preview-Image").GetComponent<Image>();
		continueButton.GetComponent<HoverToggleScript>().Initialize(continuePreviewImage.gameObject, false, null);

		PersistenceManager.LastSaveProperties lastSaveProperties = GameManager.persistenceM.GetLastSaveProperties();

		if (continueUniverseLoadable && lastSaveProperties != null) {
			continueButton.onClick.AddListener(delegate { GameManager.persistenceM.ContinueFromMostRecentSave(); });
			continuePreviewImage.sprite = GameManager.persistenceM.LoadSaveImageFromSaveDirectoryPath(lastSaveProperties.lastSaveSavePath);
		} else {
			continuePreviewImage.sprite = GameManager.resourceM.clearSquareSprite;
		}

		mainMenuButtonsPanel.transform.Find("Load-Button").GetComponent<Button>().onClick.AddListener(delegate { SetLoadUniverseActive(true); });
		mainMenuButtonsPanel.transform.Find("Settings-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSettingsMenuActive(true); });
		mainMenuButtonsPanel.transform.Find("Exit-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToDesktop(); });
	}

	private enum PlanetPreviewState {
		CreatePlanet, CreateColony, LoadColony, Nothing
	}

	private GameObject loadUniversePanel;

	private Transform universesListPanel;

	private Button loadUniverseButton;

	private readonly List<UniverseElement> universeElements = new List<UniverseElement>();

	private UniverseElement selectedUniverseElement;

	private class UniverseElement {
		public PersistenceManager.PersistenceUniverse persistenceUniverse;

		public GameObject obj;

		public UniverseElement(PersistenceManager.PersistenceUniverse persistenceUniverse, Transform parent) {
			this.persistenceUniverse = persistenceUniverse;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/UniverseElement-Panel"), parent, false);

			obj.transform.Find("UniverseName-Text").GetComponent<Text>().text = persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.Name];

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.LastSaveDateTime];

			string saveVersion = persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.SaveVersion];
			string gameVersion = persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.GameVersion];

			string saveVersionColour = ColorUtility.ToHtmlStringRGB(GetColour(Colours.DarkGrey50));
			string gameVersionColour = ColorUtility.ToHtmlStringRGB(GetColour(Colours.DarkGrey50));

			if (saveVersion == PersistenceManager.saveVersion.Value) {
				obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiM.SetSelectedUniverseElement(this); });
				if (gameVersion != PersistenceManager.gameVersion.Value) {
					obj.GetComponent<Image>().color = GetColour(Colours.DarkYellow);
					gameVersionColour = ColorUtility.ToHtmlStringRGB((GetColour(Colours.DarkRed) + GetColour(Colours.DarkGrey50)) / 2f);
				}
			} else {
				obj.GetComponent<Image>().color = GetColour(Colours.DarkRed);
				obj.GetComponent<Button>().interactable = false;

				if (saveVersion != PersistenceManager.saveVersion.Value) {
					saveVersionColour = ColorUtility.ToHtmlStringRGB((GetColour(Colours.DarkRed) + GetColour(Colours.DarkGrey50)) / 2f);
				}
				if (gameVersion != PersistenceManager.gameVersion.Value) {
					gameVersionColour = ColorUtility.ToHtmlStringRGB((GetColour(Colours.DarkRed) + GetColour(Colours.DarkGrey50)) / 2f);
				}
			}

			obj.transform.Find("LastSaveData-Panel/VersionGameSave-Text").GetComponent<Text>().text = string.Format(
				"{0}{1}{2}{3} {4}{5}{6}{7}",
				"<color=#" + gameVersionColour + ">",
				"G",
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.GameVersion],
				"</color>",
				"<color=#" + saveVersionColour + ">",
				"S",
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.SaveVersion],
				"</color>"
			);
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedUniverseElement(UniverseElement universeElement) {
		selectedUniverseElement = universeElement;

		loadUniverseButton.interactable = selectedUniverseElement != null;
		if (selectedUniverseElement != null) {
			loadUniverseButton.transform.Find("Text").GetComponent<Text>().text = "Load " + selectedUniverseElement.persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.Name];
		} else {
			loadUniverseButton.transform.Find("Text").GetComponent<Text>().text = "Select a Universe to Load";
		}
	}

	private void SetupLoadUniverseUI() {
		loadUniversePanel = mainMenu.transform.Find("LoadUniverse-Panel").gameObject;

		Button backButton = loadUniversePanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.universeM.SetUniverse(null);
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			SetSelectedUniverseElement(null);

			SetLoadUniverseActive(false);
		});

		universesListPanel = loadUniversePanel.transform.Find("Universes-ScrollPanel/UniversesList-Panel");

		loadUniverseButton = loadUniversePanel.transform.Find("LoadUniverse-Button").GetComponent<Button>();
		loadUniverseButton.onClick.AddListener(delegate {
			if (selectedUniverseElement != null) {
				GameManager.persistenceM.ApplyLoadedConfiguration(selectedUniverseElement.persistenceUniverse);
				GameManager.persistenceM.ApplyLoadedUniverse(selectedUniverseElement.persistenceUniverse);

				SetSelectedUniverseElement(null);

				SetLoadUniverseActive(false);
				SetLoadPlanetActive(true);
			}
		});
		SetSelectedUniverseElement(null);
	}

	private void SetLoadUniverseActive(bool active) {
		loadUniversePanel.SetActive(active);
		ToggleMainMenuButtons(loadUniversePanel);

		foreach (UniverseElement universeElement in universeElements) {
			universeElement.Delete();
		}
		universeElements.Clear();

		if (loadUniversePanel.activeSelf) {
			foreach (PersistenceManager.PersistenceUniverse persistenceUniverse in GameManager.persistenceM.GetPersistenceUniverses()) {
				universeElements.Add(new UniverseElement(persistenceUniverse, universesListPanel));
			}
		}
	}

	private GameObject createUniversePanel;

	private InputField universeNameInputField;

	private void SetupCreateUniverseUI() {
		createUniversePanel = mainMenu.transform.Find("CreateUniverse-Panel").gameObject;

		Button backButton = createUniversePanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.universeM.SetUniverse(null);
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			universeNameInputField.text = UniverseManager.GetRandomUniverseName();

			SetCreateUniverseActive(false);
		});

		universeNameInputField = createUniversePanel.transform.Find("UniverseName-Panel/InputField").GetComponent<InputField>();

		Button saveUniverseButton = createUniversePanel.transform.Find("SaveUniverse-Button").GetComponent<Button>();
		saveUniverseButton.onClick.AddListener(delegate {
			GameManager.universeM.CreateUniverse(universeNameInputField.text);
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			universeNameInputField.text = UniverseManager.GetRandomUniverseName();

			SetCreateUniverseActive(false);
			SetCreatePlanetActive(true);
		});
		saveUniverseButton.interactable = false;
		saveUniverseButton.transform.Find("Image").GetComponent<Image>().color = GetColour(Colours.Grey120);

		universeNameInputField.onValueChanged.AddListener(delegate {
			ValidateUniverseName(universeNameInputField, saveUniverseButton);
		});

		universeNameInputField.text = UniverseManager.GetRandomUniverseName();
		ValidateUniverseName(universeNameInputField, saveUniverseButton);
	}

	private void ValidateUniverseName(InputField universeNameInputField, Button saveUniverseButton) {
		bool validUniverseName = !string.IsNullOrEmpty(universeNameInputField.text) && IsAlphanumericWithSpaces(universeNameInputField.text);
		saveUniverseButton.interactable = validUniverseName;
		saveUniverseButton.transform.Find("Image").GetComponent<Image>().color = validUniverseName ? GetColour(Colours.LightGrey220) : GetColour(Colours.Grey120);
	}

	private void SetCreateUniverseActive(bool active) {
		createUniversePanel.SetActive(active);
		ToggleMainMenuButtons(createUniversePanel);
	}

	private GameObject loadPlanetPanel;

	private Transform planetsListPanel;

	private Button loadPlanetButton;

	private readonly List<PlanetElement> planetElements = new List<PlanetElement>();

	private PlanetElement selectedPlanetElement;

	private class PlanetElement {
		public PersistenceManager.PersistencePlanet persistencePlanet;

		public GameObject obj;

		public PlanetElement(PersistenceManager.PersistencePlanet persistencePlanet, Transform parent) {
			this.persistencePlanet = persistencePlanet;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/PlanetElement-Panel"), parent, false);

			obj.transform.Find("PlanetName-Text").GetComponent<Text>().text = persistencePlanet.name;

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistencePlanet.lastSaveDateTime;

			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiM.SetSelectedPlanetElement(this); });
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedPlanetElement(PlanetElement planetElement) {
		selectedPlanetElement = planetElement;

		loadPlanetButton.interactable = selectedPlanetElement != null;
		if (selectedPlanetElement != null) {
			loadPlanetButton.transform.Find("Text").GetComponent<Text>().text = "Load " + selectedPlanetElement.persistencePlanet.name;
		} else {
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
			foreach (PersistenceManager.PersistencePlanet persistencePlanet in GameManager.persistenceM.GetPersistencePlanets()) {
				planetElements.Add(new PlanetElement(persistencePlanet, planetsListPanel));
			}
		}
	}

	private GameObject createPlanetPanel;

	private InputField planetRegenerationCodeInputField;
	private Button planetRegenerationCodeButton;

	private RectTransform createPlanetPlanetTilesRectTransform;

	private InputField planetNameInputField;
	private InputField planetSeedInputField;
	private Slider planetSizeSlider;
	private Slider planetDistanceSlider;
	private Slider temperatureRangeSlider;
	private Toggle randomOffsetsToggle;
	private Slider windDirectionSlider;

	private GameObject createPlanetSelectedPlanetTileInfoPanel;
	private Image createPlanetSelectedPlanetTileSpriteImage;
	private Text createPlanetSelectedPlanetTileBiomeText;
	private Text createPlanetSelectedPlanetTilePositionText;

	private void SetupCreatePlanetUI() {
		createPlanetPanel = mainMenu.transform.Find("CreatePlanet-Panel").gameObject;

		createPlanetPlanetTilesRectTransform = createPlanetPanel.transform.Find("PlanetPreview-Panel").GetComponent<RectTransform>();

		Button backButton = createPlanetPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.planetM.SetPlanet(null);
			GameManager.colonyM.SetColony(null);

			SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);

			SetCreatePlanetDefaultSettings();

			SetCreatePlanetActive(false);
			SetLoadUniverseActive(true);
		});

		planetRegenerationCodeInputField = createPlanetPanel.transform.Find("PlanetRegenerationCode-Panel/InputField").GetComponent<InputField>();
		planetRegenerationCodeButton = createPlanetPanel.transform.Find("PlanetRegenerationCode-Panel/LoadPlanetCode-Button").GetComponent<Button>();

		planetNameInputField = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetName-Panel/InputField").GetComponent<InputField>();

		planetSeedInputField = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetSeed-Panel/InputField").GetComponent<InputField>();

		planetSizeSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetSize-Panel/PlanetSize-Slider").GetComponent<Slider>();
		planetSizeSlider.minValue = 0;
		planetSizeSlider.maxValue = PlanetManager.GetNumPlanetSizes() - 1;
		Text planetSizeText = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetSize-Panel/PlanetSizeValue-Text").GetComponent<Text>();
		planetSizeText.text = PlanetManager.GetPlanetSizeByIndex(Mathf.FloorToInt(planetSizeSlider.value)).ToString();
		planetSizeSlider.onValueChanged.AddListener(delegate {
			planetSizeText.text = PlanetManager.GetPlanetSizeByIndex(Mathf.FloorToInt(planetSizeSlider.value)).ToString();
		});

		planetDistanceSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetDistance-Panel/PlanetDistance-Slider").GetComponent<Slider>();
		planetDistanceSlider.minValue = PlanetManager.GetMinPlanetDistance();
		planetDistanceSlider.maxValue = PlanetManager.GetMaxPlanetDistance();
		Text planetDistanceText = createPlanetPanel.transform.Find("PlanetSettings-Panel/PlanetDistance-Panel/PlanetDistanceValue-Text").GetComponent<Text>();
		planetDistanceText.text = PlanetManager.GetPlanetDistanceByIndex(Mathf.FloorToInt(planetDistanceSlider.value)).ToString() + " AU";
		planetDistanceSlider.onValueChanged.AddListener(delegate {
			planetDistanceText.text = PlanetManager.GetPlanetDistanceByIndex(Mathf.FloorToInt(planetDistanceSlider.value)).ToString() + " AU";
		});

		temperatureRangeSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/TemperatureRange-Panel/TemperatureRange-Slider").GetComponent<Slider>();
		temperatureRangeSlider.minValue = 0;
		temperatureRangeSlider.maxValue = 10;
		Text temperatureRangeText = createPlanetPanel.transform.Find("PlanetSettings-Panel/TemperatureRange-Panel/TemperatureRangeValue-Text").GetComponent<Text>();
		temperatureRangeText.text = PlanetManager.GetTemperatureRangeByIndex(Mathf.FloorToInt(temperatureRangeSlider.value)) + "°C";
		temperatureRangeSlider.onValueChanged.AddListener(delegate {
			temperatureRangeText.text = PlanetManager.GetTemperatureRangeByIndex(Mathf.FloorToInt(temperatureRangeSlider.value)) + "°C";
		});

		randomOffsetsToggle = createPlanetPanel.transform.Find("PlanetSettings-Panel/RandomOffsets-Toggle").GetComponent<Toggle>();

		windDirectionSlider = createPlanetPanel.transform.Find("PlanetSettings-Panel/WindDirection-Panel/WindDirection-Slider").GetComponent<Slider>();
		windDirectionSlider.minValue = 0;
		windDirectionSlider.maxValue = PlanetManager.GetNumWindDirections() - 1;
		Text windDirectionText = createPlanetPanel.transform.Find("PlanetSettings-Panel/WindDirection-Panel/WindDirectionValue-Text").GetComponent<Text>();
		windDirectionText.text = PlanetManager.GetWindCardinalDirectionByIndex(Mathf.FloorToInt(windDirectionSlider.value));
		windDirectionSlider.onValueChanged.AddListener(delegate {
			windDirectionText.text = PlanetManager.GetWindCardinalDirectionByIndex(Mathf.FloorToInt(windDirectionSlider.value));
		});

		SetCreatePlanetDefaultSettings();

		Button refreshPreviewButton = createPlanetPanel.transform.Find("PlanetSettings-Panel/RefreshAndRandomize-Panel/RefreshPreview-Button").GetComponent<Button>();
		refreshPreviewButton.onClick.AddListener(delegate {
			CreateNewPlanetFromSettings(true);
		});

		Button randomizeButton = createPlanetPanel.transform.Find("PlanetSettings-Panel/RefreshAndRandomize-Panel/Randomize-Button").GetComponent<Button>();
		randomizeButton.onClick.AddListener(delegate {
			RandomizePlanetSettings();
		});

		createPlanetSelectedPlanetTileInfoPanel = createPlanetPanel.transform.Find("SelectedPlanetTileInfo-Panel").gameObject;

		createPlanetSelectedPlanetTileSpriteImage = createPlanetSelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileSprite-Image-Panel/SelectedPlanetTileSprite-Image").GetComponent<Image>();
		createPlanetSelectedPlanetTileBiomeText = createPlanetSelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileBiome-Panel/BiomeValue-Text").GetComponent<Text>();
		createPlanetSelectedPlanetTilePositionText = createPlanetPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>();

		Button savePlanetButton = createPlanetPanel.transform.Find("SavePlanet-Button").GetComponent<Button>();
		savePlanetButton.onClick.AddListener(delegate {
			GameManager.persistenceM.CreatePlanet(CreateNewPlanetFromSettings(false));
			GameManager.colonyM.SetColony(null);

			SetCreatePlanetActive(false);
			SetCreateColonyActive(true);
		});

		planetNameInputField.onValueChanged.AddListener(delegate {
			bool validPlanetName = IsAlphanumericWithSpaces(planetNameInputField.text);
			savePlanetButton.interactable = validPlanetName;
			savePlanetButton.transform.Find("Image").GetComponent<Image>().color = validPlanetName ? GetColour(Colours.LightGrey220) : GetColour(Colours.Grey120);
		});
	}

	private void SetCreatePlanetDefaultSettings() {
		planetNameInputField.text = PlanetManager.GetRandomPlanetName();

		planetSeedInputField.text = PlanetManager.GetRandomPlanetSeed().ToString();

		planetSizeSlider.value = Mathf.FloorToInt((planetSizeSlider.minValue + planetSizeSlider.maxValue) / 2f) + 2; // 60

		planetDistanceSlider.value = Mathf.FloorToInt((planetDistanceSlider.minValue + planetDistanceSlider.maxValue) / 2f); // 1 AU

		temperatureRangeSlider.value = Mathf.FloorToInt((temperatureRangeSlider.minValue + temperatureRangeSlider.maxValue) / 2f) + 2; // 70°C

		randomOffsetsToggle.isOn = true;

		windDirectionSlider.value = UnityEngine.Random.Range(windDirectionSlider.minValue, windDirectionSlider.maxValue);
	}

	private PlanetManager.Planet CreateNewPlanetFromSettings(bool displayPlanet) {
		PlanetManager.Planet planet = GameManager.planetM.CreatePlanet(
			planetNameInputField.text,
			ParseSeed(planetSeedInputField.text),
			PlanetManager.GetPlanetSizeByIndex(Mathf.FloorToInt(planetSizeSlider.value)),
			PlanetManager.GetPlanetDistanceByIndex(Mathf.FloorToInt(planetDistanceSlider.value)),
			PlanetManager.GetTemperatureRangeByIndex(Mathf.FloorToInt(temperatureRangeSlider.value)),
			randomOffsetsToggle.isOn,
			PlanetManager.GetWindCircularDirectionByIndex(Mathf.FloorToInt(windDirectionSlider.value))
		);

		if (displayPlanet) {
			DisplayPlanet(
				planet,
				createPlanetPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<GridLayoutGroup>(),
				createColonyPlanetTilesRectTransform,
				PlanetPreviewState.CreatePlanet,
				null
			);
		}

		return planet;
	}

	private void RandomizePlanetSettings() {
		planetSeedInputField.text = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
	}

	private void SetCreatePlanetActive(bool active) {
		createPlanetPanel.SetActive(active);
		ToggleMainMenuButtons(createPlanetPanel);

		SetCreatePlanetDefaultSettings();

		if (createPlanetPanel.activeSelf) {
			RandomizePlanetSettings();
			CreateNewPlanetFromSettings(true);
			SetSelectedPlanetTile(null, PlanetPreviewState.CreatePlanet);
		} else {
			ClearPlanet();
		}
	}

	private GameObject loadColonyPanel;

	private RectTransform loadColonyPlanetTilesRectTransform;

	private Transform coloniesListPanel;

	private Button loadColonyButton;

	private readonly List<ColonyElement> colonyElements = new List<ColonyElement>();

	private ColonyElement selectedColonyElement;

	private class ColonyElement {
		public PersistenceManager.PersistenceColony persistenceColony;

		public GameObject obj;

		public ColonyElement(PersistenceManager.PersistenceColony persistenceColony, Transform parent) {
			this.persistenceColony = persistenceColony;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonyElement-Panel"), parent, false);

			obj.transform.Find("ColonyName-Panel/ColonyName-Text").GetComponent<Text>().text = persistenceColony.name;

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistenceColony.lastSaveDateTime;

			if (persistenceColony.lastSaveImage != null) {
				obj.transform.Find("ColonyLastSave-Image-Panel/ColonyLastSave-Image").GetComponent<Image>().sprite = persistenceColony.lastSaveImage;
			}

			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.uiM.SetSelectedColonyElement(this); });
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedColonyElement(ColonyElement colonyElement) {
		selectedColonyElement = colonyElement;

		loadColonyButton.interactable = selectedColonyElement != null;
		if (selectedColonyElement != null) {
			loadColonyButton.transform.Find("Text").GetComponent<Text>().text = "Load " + selectedColonyElement.persistenceColony.name;
		} else {
			loadColonyButton.transform.Find("Text").GetComponent<Text>().text = "Select a Colony to Load";
		}
	}

	private void SetupLoadColonyUI() {
		loadColonyPanel = mainMenu.transform.Find("LoadColony-Panel").gameObject;

		Button backButton = loadColonyPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			GameManager.colonyM.SetColony(null);

			SetSelectedColonyElement(null);

			SetLoadColonyActive(false);
			SetLoadPlanetActive(true);
		});

		Button createColonyButton = loadColonyPanel.transform.Find("CreateColony-Button").GetComponent<Button>();
		createColonyButton.onClick.AddListener(delegate {
			GameManager.colonyM.SetColony(null);

			SetSelectedColonyElement(null);

			SetLoadColonyActive(false);
			SetCreateColonyActive(true);
		});

		loadColonyPlanetTilesRectTransform = loadColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<RectTransform>();

		coloniesListPanel = loadColonyPanel.transform.Find("Colonies-ScrollPanel/ColoniesList-Panel");

		loadColonyButton = loadColonyPanel.transform.Find("LoadColony-Button").GetComponent<Button>();
		loadColonyButton.onClick.AddListener(delegate {
			GameManager.persistenceM.ApplyLoadedColony(selectedColonyElement.persistenceColony);

			SetSelectedColonyElement(null);

			SetLoadColonyActive(false);
			SetLoadSaveActive(true);
		});
		SetSelectedColonyElement(null);
	}

	private void SetLoadColonyActive(bool active) {
		loadColonyPanel.SetActive(active);
		ToggleMainMenuButtons(loadColonyPanel);

		foreach (ColonyElement colonyElement in colonyElements) {
			colonyElement.Delete();
		}
		colonyElements.Clear();

		if (loadColonyPanel.activeSelf) {
			List<PersistenceManager.PersistenceColony> persistenceColonies = GameManager.persistenceM.GetPersistenceColonies();

			DisplayPlanet(
				GameManager.planetM.planet,
				loadColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<GridLayoutGroup>(),
				loadColonyPlanetTilesRectTransform,
				PlanetPreviewState.LoadColony,
				persistenceColonies
			);

			foreach (PersistenceManager.PersistenceColony persistenceColony in persistenceColonies) {
				colonyElements.Add(new ColonyElement(persistenceColony, coloniesListPanel));
			}
		} else {
			ClearPlanet();
		}
	}

	private GameObject createColonyPanel;

	private InputField mapRegenerationCodeInputField;
	private Button mapRegenerationCodeButton;

	private RectTransform createColonyPlanetTilesRectTransform;

	private InputField colonyNameInputField;
	private InputField mapSeedInputField;
	private Slider mapSizeSlider;

	private GameObject createColonySelectedPlanetTileInfoPanel;
	private Image createColonySelectedPlanetTileSpriteImage;
	private Text createColonySelectedPlanetTileBiomeText;
	private Text createColonySelectedPlanetTileAverageTemperatureText;
	private Text createColonySelectedPlanetTileAveragePrecipitationText;
	private Text createColonySelectedPlanetTileAltitudeText;
	private Text createColonySelectedPlanetTilePositionText;

	private Button saveColonyButton;

	private void SetupCreateColonyUI() {
		createColonyPanel = mainMenu.transform.Find("CreateColony-Panel").gameObject;

		Button backButton = createColonyPanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			SetCreateColonyDefaultSettings();

			GameManager.colonyM.SetColony(null);

			SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);

			SetCreateColonyActive(false);
			SetLoadPlanetActive(true);
		});

		mapRegenerationCodeInputField = createColonyPanel.transform.Find("MapRegenerationCode-Panel/InputField").GetComponent<InputField>();
		mapRegenerationCodeButton = createColonyPanel.transform.Find("MapRegenerationCode-Panel/LoadMapCode-Button").GetComponent<Button>();

		createColonyPlanetTilesRectTransform = createColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<RectTransform>();

		colonyNameInputField = createColonyPanel.transform.Find("MapSettings-Panel/ColonyNameOptions-Panel/ColonyName-Panel/InputField").GetComponent<InputField>();

		Button randomizeColonyNameButton = createColonyPanel.transform.Find("MapSettings-Panel/ColonyNameOptions-Panel/RandomizeColonyName-Button").GetComponent<Button>();
		randomizeColonyNameButton.onClick.AddListener(delegate {
			colonyNameInputField.text = ColonyManager.GetRandomColonyName();
		});

		mapSeedInputField = createColonyPanel.transform.Find("MapSettings-Panel/MapSeed-Panel/InputField").GetComponent<InputField>();

		mapSizeSlider = createColonyPanel.transform.Find("MapSettings-Panel/MapSize-Panel/MapSize-Slider").GetComponent<Slider>();
		mapSizeSlider.minValue = 0;
		mapSizeSlider.maxValue = ColonyManager.GetNumMapSizes() - 1;
		Text mapSizeText = createColonyPanel.transform.Find("MapSettings-Panel/MapSize-Panel/MapSizeValue-Text").GetComponent<Text>();
		mapSizeText.text = ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(mapSizeSlider.value)).ToString();
		mapSizeSlider.onValueChanged.AddListener(delegate {
			mapSizeText.text = ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(mapSizeSlider.value)).ToString();
		});

		SetCreateColonyDefaultSettings();

		createColonySelectedPlanetTileInfoPanel = createColonyPanel.transform.Find("SelectedPlanetTileInfo-Panel").gameObject;

		createColonySelectedPlanetTileSpriteImage = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileSprite-Image-Panel/SelectedPlanetTileSprite-Image").GetComponent<Image>();
		createColonySelectedPlanetTileBiomeText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileBiome-Panel/BiomeValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTileAverageTemperatureText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileAverageTemperature-Panel/TemperatureValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTileAveragePrecipitationText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileAveragePrecipitation-Panel/PrecipitationValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTileAltitudeText = createColonySelectedPlanetTileInfoPanel.transform.Find("SelectedPlanetTileAltitude-Panel/AltitudeValue-Text").GetComponent<Text>();
		createColonySelectedPlanetTilePositionText = createColonyPanel.transform.Find("PlanetPreview-Panel/SelectedPlanetTileCoordinates-Text").GetComponent<Text>();

		saveColonyButton = createColonyPanel.transform.Find("SaveColony-Button").GetComponent<Button>();
		saveColonyButton.onClick.AddListener(delegate {
			ColonyManager.Colony colony = GameManager.colonyM.CreateColony(
				colonyNameInputField.text,
				GameManager.planetM.selectedPlanetTile.tile.position,
				ParseSeed(mapSeedInputField.text),
				ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(mapSizeSlider.value)),
				GameManager.planetM.selectedPlanetTile.averageTemperature,
				GameManager.planetM.selectedPlanetTile.tile.GetPrecipitation(),
				GameManager.planetM.selectedPlanetTile.terrainTypeHeights,
				GameManager.planetM.selectedPlanetTile.surroundingPlanetTileHeightDirections,
				GameManager.planetM.selectedPlanetTile.isRiver,
				GameManager.planetM.selectedPlanetTile.surroundingPlanetTileRivers
			);

			GameManager.colonyM.SetupNewColony(colony, false);

			SetCreateColonyActive(false);

			SetSelectedPlanetTile(null, PlanetPreviewState.Nothing);
		});

		colonyNameInputField.onValueChanged.AddListener(delegate {
			SetSaveColonyButtonInteractable();
		});
	}

	public void SetCreateColonyDefaultSettings() {
		colonyNameInputField.text = ColonyManager.GetRandomColonyName();

		mapSeedInputField.text = TileManager.Map.GetRandomMapSeed().ToString();

		mapSizeSlider.value = 1;
	}

	private void SetCreateColonyActive(bool active) {
		createColonyPanel.SetActive(active);
		ToggleMainMenuButtons(createColonyPanel);

		SetCreateColonyDefaultSettings();

		if (createColonyPanel.activeSelf) {
			DisplayPlanet(
				GameManager.planetM.planet,
				createColonyPanel.transform.Find("PlanetPreview-Panel/PlanetPreviewTiles-Panel").GetComponent<GridLayoutGroup>(),
				createColonyPlanetTilesRectTransform,
				PlanetPreviewState.CreateColony,
				GameManager.persistenceM.GetPersistenceColonies()
			);

			SetSelectedPlanetTile(GameManager.planetM.selectedPlanetTile, PlanetPreviewState.CreateColony);
		} else {
			ClearPlanet();
		}
	}

	private GameObject loadSavePanel;

	private Transform savesListPanel;

	private Button loadSaveButton;

	private readonly List<SaveElement> saveElements = new List<SaveElement>();

	private SaveElement selectedSaveElement;

	private class SaveElement {
		public PersistenceManager.PersistenceSave persistenceSave;

		public GameObject obj;

		public SaveElement(PersistenceManager.PersistenceSave persistenceSave, Transform parent) {
			this.persistenceSave = persistenceSave;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SaveElement-Panel"), parent, false);

			if (persistenceSave.loadable) {
				if (persistenceSave.image != null) {
					obj.transform.Find("Save-Image-Panel/Save-Image").GetComponent<Image>().sprite = persistenceSave.image;
				}

				obj.transform.Find("SaveData-Panel/SavedDateTime-Text").GetComponent<Text>().text = persistenceSave.saveDateTime;

				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.SetSelectedSaveElement(this);
				});
			} else {
				obj.transform.Find("SaveData-Panel/Saved-Text").GetComponent<Text>().text = "Error while reading save.";
				obj.transform.Find("SaveData-Panel/SavedDateTime-Text").GetComponent<Text>().text = string.Empty;
				obj.GetComponent<Image>().color = GetColour(Colours.LightRed);
			}
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private void SetSelectedSaveElement(SaveElement saveElement) {
		selectedSaveElement = saveElement;

		loadSaveButton.interactable = selectedSaveElement != null;
		if (selectedSaveElement != null) {
			loadSaveButton.transform.Find("Text").GetComponent<Text>().text = "Load Save (" + selectedSaveElement.persistenceSave.saveDateTime + ")";
		} else {
			loadSaveButton.transform.Find("Text").GetComponent<Text>().text = "Select a Save to Load";
		}
	}

	private void SetupLoadSaveUI() {
		loadSavePanel = mainMenu.transform.Find("LoadSave-Panel").gameObject;

		Button backButton = loadSavePanel.transform.Find("Back-Button").GetComponent<Button>();
		backButton.onClick.AddListener(delegate {
			SetSelectedSaveElement(null);

			SetLoadSaveActive(false);
			SetLoadColonyActive(true);
		});

		savesListPanel = loadSavePanel.transform.Find("Saves-ScrollPanel/SavesList-Panel");

		loadSaveButton = loadSavePanel.transform.Find("LoadSave-Button").GetComponent<Button>();
		loadSaveButton.onClick.AddListener(delegate {
			if (selectedSaveElement != null) {
				startCoroutineReference.StartCoroutine(GameManager.persistenceM.ApplyLoadedSave(selectedSaveElement.persistenceSave));

				SetSelectedSaveElement(null);

				SetLoadSaveActive(false);
			}
		});
		SetSelectedSaveElement(null);
	}

	private void SetLoadSaveActive(bool active) {
		loadSavePanel.SetActive(active);
		ToggleMainMenuButtons(loadSavePanel);

		foreach (SaveElement saveElement in saveElements) {
			saveElement.Delete();
		}
		saveElements.Clear();

		if (loadSavePanel.activeSelf) {
			foreach (PersistenceManager.PersistenceSave persistenceSave in GameManager.persistenceM.GetPersistenceSaves()) {
				saveElements.Add(new SaveElement(persistenceSave, savesListPanel));
			}
		}
	}

	private readonly List<GameObject> planetTileObjs = new List<GameObject>();

	private void ClearPlanet() {
		foreach (GameObject planetTileObj in planetTileObjs) {
			MonoBehaviour.Destroy(planetTileObj);
		}
		planetTileObjs.Clear();
	}

	private void DisplayPlanet(PlanetManager.Planet planet, GridLayoutGroup planetGrid, RectTransform planetRectTransform, PlanetPreviewState planetPreviewState, List<PersistenceManager.PersistenceColony> persistenceColonies) {

		ClearPlanet();

		planetGrid.cellSize = new Vector2(
			createPlanetPlanetTilesRectTransform.sizeDelta.x / planet.mapData.mapSize,
			createPlanetPlanetTilesRectTransform.sizeDelta.y / planet.mapData.mapSize
		);
		planetGrid.constraintCount = planet.mapData.mapSize;

		foreach (PlanetManager.Planet.PlanetTile planetTile in planet.planetTiles) {
			GameObject planetTileObj = MonoBehaviour.Instantiate(GameManager.resourceM.planetTilePrefab, planetGrid.transform, false);
			planetTileObj.name = "Planet Tile: " + planetTile.tile.position;
			planetTileObj.GetComponent<Image>().sprite = planetTile.sprite;
			planetTileObj.GetComponent<Button>().onClick.AddListener(delegate {
				SetSelectedPlanetTile(planetTile, planetPreviewState);
			});

			if (persistenceColonies != null) {
				PersistenceManager.PersistenceColony persistenceColony = persistenceColonies.Find(pc => pc.planetPosition == planetTile.tile.position);
				if (persistenceColony != null) {
					GameObject colonyObj = MonoBehaviour.Instantiate(GameManager.resourceM.colonyObj, planetTileObj.transform, false);
					colonyObj.name = "Colony: " + persistenceColony.name;
					colonyObj.GetComponent<Button>().onClick.AddListener(delegate {
						SetSelectedColonyElement(colonyElements.Find(ce => ce.persistenceColony == persistenceColony));
					});
				}
			}

			planetTileObjs.Add(planetTileObj);
		}
	}

	public static int ParseSeed(string seedString) {
		if (string.IsNullOrEmpty(seedString)) {
			seedString = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
		}
		int seed = 0;
		if (!int.TryParse(seedString, out seed)) {
			int seedCharacterIndex = 1;
			foreach (char seedCharacter in seedString) {
				seed += seedCharacter * seedCharacterIndex;
				seedCharacterIndex += 1;
			}
		}
		return seed;
	}

	private void SetSelectedPlanetTile(PlanetManager.Planet.PlanetTile selectedPlanetTile, PlanetPreviewState planetPreviewState) {
		GameManager.planetM.SetSelectedPlanetTile(selectedPlanetTile);
		DisplaySelectedPlanetTileInfo(selectedPlanetTile, planetPreviewState);

		SetSaveColonyButtonInteractable();
	}

	public void SetSaveColonyButtonInteractable() {
		bool selectedPlanetTileNotNull = GameManager.planetM.selectedPlanetTile != null;

		bool validColonyName = IsAlphanumericWithSpaces(colonyNameInputField.text);

		bool interactable = selectedPlanetTileNotNull && validColonyName;
		saveColonyButton.interactable = interactable;
		saveColonyButton.transform.Find("Image").GetComponent<Image>().color = interactable ? GetColour(Colours.LightGrey220) : GetColour(Colours.Grey120);
	}

	private void DisplaySelectedPlanetTileInfo(PlanetManager.Planet.PlanetTile selectedPlanetTile, PlanetPreviewState planetPreviewState) {
		createPlanetSelectedPlanetTileInfoPanel.SetActive(false);
		createPlanetSelectedPlanetTilePositionText.gameObject.SetActive(false);
		createColonySelectedPlanetTileInfoPanel.SetActive(false);
		createColonySelectedPlanetTilePositionText.gameObject.SetActive(false);

		if (planetPreviewState == PlanetPreviewState.CreatePlanet) {
			createPlanetSelectedPlanetTileInfoPanel.SetActive(selectedPlanetTile != null);
			createPlanetSelectedPlanetTilePositionText.gameObject.SetActive(createPlanetSelectedPlanetTileInfoPanel.activeSelf);
			if (createPlanetSelectedPlanetTileInfoPanel.activeSelf) {
				createPlanetSelectedPlanetTileSpriteImage.sprite = selectedPlanetTile.sprite;
				createPlanetSelectedPlanetTileBiomeText.text = selectedPlanetTile.tile.biome.name;
				createPlanetSelectedPlanetTilePositionText.text = "(" + Mathf.FloorToInt(selectedPlanetTile.tile.position.x) + ", " + Mathf.FloorToInt(selectedPlanetTile.tile.position.y) + ")";
			}
		} else if (planetPreviewState == PlanetPreviewState.CreateColony) {
			createColonySelectedPlanetTileInfoPanel.SetActive(selectedPlanetTile != null);
			createColonySelectedPlanetTilePositionText.gameObject.SetActive(createColonySelectedPlanetTileInfoPanel.activeSelf);
			if (createColonySelectedPlanetTileInfoPanel.activeSelf) {
				createColonySelectedPlanetTileSpriteImage.sprite = selectedPlanetTile.sprite;
				createColonySelectedPlanetTileBiomeText.text = selectedPlanetTile.tile.biome.name;
				createColonySelectedPlanetTileAverageTemperatureText.text = Mathf.RoundToInt(selectedPlanetTile.tile.temperature).ToString() + "°C";
				createColonySelectedPlanetTileAveragePrecipitationText.text = Mathf.RoundToInt(selectedPlanetTile.tile.GetPrecipitation() * 100f).ToString() + "%";
				createColonySelectedPlanetTileAltitudeText.text = selectedPlanetTile.altitude;
				createColonySelectedPlanetTilePositionText.text = "(" + Mathf.FloorToInt(selectedPlanetTile.tile.position.x) + ", " + Mathf.FloorToInt(selectedPlanetTile.tile.position.y) + ")";
			}
		} else if (planetPreviewState == PlanetPreviewState.LoadColony) {

		}
	}

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
		availableClothingTitleAndList = new KeyValuePair<GameObject, GameObject>(
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesAvailableTitle-Panel").gameObject,
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesAvailable-Panel").gameObject
		);
		takenClothingTitleAndList = new KeyValuePair<GameObject, GameObject>(
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesTakenTitle-Panel").gameObject,
			selectedColonistClothingSelectionPanel.transform.Find("ClothingSelection-ScrollPanel/ClothingList-Panel/ClothesTaken-Panel").gameObject
		);

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
		tradeMenu = gameUI.transform.Find("TradeMenu-Panel").gameObject;
		tradeMenu.transform.Find("Close-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeMenuActive(false); });
		tradeMenu.transform.Find("ConfirmTrade-Button").GetComponent<Button>().onClick.AddListener(delegate { ConfirmTrade(); });

		dateTimeInformationPanel = gameUI.transform.Find("DateTimeInformation-Panel").gameObject;

		selectionSizePanel = new SelectionSizePanel(GameObject.Find("SelectionSize-Canvas").GetComponent<Canvas>());

		selectedContainerInventoryPanel = gameUI.transform.Find("SelectedContainerInventory-Panel").gameObject;

		selectedTradingPostPanel = gameUI.transform.Find("SelectedTradingPost-Panel").gameObject;

		professionsMenuButton = gameUI.transform.Find("Management-Panel/ProfessionsMenu-Button").gameObject;
		professionsMenu = professionsMenuButton.transform.Find("ProfessionsMenu-Panel").gameObject;
		professionsMenuButton.GetComponent<Button>().onClick.AddListener(delegate { ToggleProfessionsMenu(); });

		objectsMenuButton = gameUI.transform.Find("Management-Panel/ObjectsMenu-Button").gameObject;
		objectPrefabsList = objectsMenuButton.transform.Find("ObjectPrefabsList-ScrollPanel").gameObject;
		objectsMenuButton.GetComponent<Button>().onClick.AddListener(delegate {
			ToggleObjectPrefabsList(true);
		});
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
		selectedCraftingObjectPanelSelectResourcesButton.onClick.AddListener(delegate {
			SetSelectedCraftingObjectSelectResourcesPanel();
		});
		selectedCraftingObjectSelectResourcesPanel.transform.Find("Close-Button").GetComponent<Button>().onClick.AddListener(delegate {
			SetSelectedCraftingObjectSelectResourcesPanel();
		});

		selectedCraftingObjectPanelSelectFuelsButton = selectedCraftingObjectPanel.transform.Find("Settings-Panel/SelectFuels-Button").GetComponent<Button>();
		selectedCraftingObjectPanelSelectFuelsButton.onClick.AddListener(delegate {
			SetSelectedCraftingObjectSelectFuelsPanel();
		});
		selectedCraftingObjectSelectFuelsPanel.transform.Find("Close-Button").GetComponent<Button>().onClick.AddListener(delegate {
			SetSelectedCraftingObjectSelectFuelsPanel();
		});

		selectedCraftingObjectPanelActiveButton = selectedCraftingObjectPanel.transform.Find("Settings-Panel/ActiveValueToggle-Button").GetComponent<Button>();
		selectedCraftingObjectPanelActiveButton.onClick.AddListener(delegate {
			selectedCraftingObject.SetActive(!selectedCraftingObject.active);
		});

		selectedCraftingObjectPanelActiveButtonText = selectedCraftingObjectPanelActiveButton.transform.Find("ActiveValue-Text").GetComponent<Text>();
	}

	public GameObject pauseMenu;
	private GameObject pauseMenuButtons;
	private GameObject pauseLabel;

	private Button pauseSaveButton;

	private void SetupPauseMenu() {
		pauseMenu = canvas.transform.Find("PauseMenu-BackgroundPanel").gameObject;
		pauseMenuButtons = pauseMenu.transform.Find("ButtonsList-Panel").gameObject;
		pauseLabel = pauseMenu.transform.Find("PausedLabel-Text").gameObject;

		pauseMenuButtons.transform.Find("PauseContinue-Button").GetComponent<Button>().onClick.AddListener(delegate { SetPauseMenuActive(false); });

		pauseSaveButton = pauseMenuButtons.transform.Find("PauseSave-Button").GetComponent<Button>();
		pauseSaveButton.onClick.AddListener(delegate { SaveClicked(); });

		pauseMenuButtons.transform.Find("PauseSettings-Button").GetComponent<Button>().onClick.AddListener(delegate { SetSettingsMenuActive(true); });

		//pauseMenuButtons.transform.Find("PauseExitToMainMenu-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToMenu(); });

		pauseMenuButtons.transform.Find("PauseExitToDesktop-Button").GetComponent<Button>().onClick.AddListener(delegate { ExitToDesktop(); });
	}

	public void SaveClicked() {
		try {
			GameManager.persistenceM.CreateSave(GameManager.colonyM.colony);
			pauseSaveButton.GetComponent<Image>().color = GetColour(Colours.LightGreen);
		} catch (Exception e) {
			pauseSaveButton.GetComponent<Image>().color = GetColour(Colours.LightRed);
			throw e.InnerException != null ? e.InnerException : e;
		}
	}

	public override void Update() {
		if (GameManager.tileM.mapState == TileManager.MapState.Generated) {
			mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile newMouseOverTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
			if (newMouseOverTile != mouseOverTile) {
				mouseOverTile = newMouseOverTile;
				if (!pauseMenu.activeSelf) {
					UpdateTileInformation();
				}
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
								SetPauseMenuActive(!pauseMenu.activeSelf);
							}
						}
					}
				}
			}

			UpdateSelectedColonistInformation();
			UpdateSelectedTraderMenu();
			UpdateTradeMenu();

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
		} else {
			if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) && GameManager.tileM.mapState != TileManager.MapState.Generating && !playerTyping) {
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
		}

		playerTyping = IsPlayerTyping();
	}

	private bool IsPlayerTyping() {
		if (universeNameInputField.isFocused) {
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
		}

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

	void ToggleMainMenuButtons(GameObject coverPanel) {
		if (mainMenuButtonsPanel.activeSelf && coverPanel.activeSelf) {
			mainMenuButtonsPanel.SetActive(false);
			snowshipLogo.SetActive(false);
		} else if (!mainMenuButtonsPanel.activeSelf && !coverPanel.activeSelf) {
			mainMenuButtonsPanel.SetActive(true);
			snowshipLogo.SetActive(true);
		}
		darkBackground.SetActive(!snowshipLogo.activeSelf);
	}

	void ExitToDesktop() {
		Application.Quit();
	}

	public void SetMainMenuActive(bool active) {
		mainMenu.SetActive(active);
	}

	public void SetMainMenuBackground(bool randomBackgroundImage) {

		Vector2 screenResolution = new Vector2(screenWidth, screenHeight);
		float targetNewSize = Mathf.Max(screenResolution.x, screenResolution.y);

		if (randomBackgroundImage) {
			List<Sprite> backgroundImages = Resources.LoadAll<Sprite>(@"UI/Backgrounds/SingleMap").ToList();
			mainMenuBackground.GetComponent<Image>().sprite = backgroundImages[UnityEngine.Random.Range(0, backgroundImages.Count)];
		}

		Vector2 menuBackgroundSize = new Vector2(mainMenuBackground.GetComponent<Image>().sprite.texture.width, mainMenuBackground.GetComponent<Image>().sprite.texture.height);
		float menuBackgroundTargetSize = Mathf.Max(menuBackgroundSize.x, menuBackgroundSize.y);
		float menuBackgroundRatio = menuBackgroundTargetSize / targetNewSize;
		Vector2 newMenuBackgroundSize = menuBackgroundSize / menuBackgroundRatio;
		mainMenuBackground.GetComponent<RectTransform>().sizeDelta = newMenuBackgroundSize * 1.5f;
		originalBackgroundPosition = mainMenuBackground.GetComponent<RectTransform>().position;

		Vector2 logoSize = new Vector2(snowshipLogo.GetComponent<Image>().sprite.texture.width, snowshipLogo.GetComponent<Image>().sprite.texture.height);
		float logoTargetSize = Mathf.Max(logoSize.x, logoSize.y);
		float logoRatio = logoTargetSize / targetNewSize;
		Vector2 newLogoSize = logoSize / logoRatio;
		snowshipLogo.GetComponent<RectTransform>().sizeDelta = newLogoSize * 1.05f;
	}

	private Vector3 originalBackgroundPosition;
	private static readonly float movementMultiplier = 25f;
	void UpdateMainMenuBackground() {
		mainMenuBackground.GetComponent<RectTransform>().position = originalBackgroundPosition + new Vector3((-Input.mousePosition.x) / (screenWidth / movementMultiplier), (-Input.mousePosition.y) / (screenHeight / movementMultiplier)) + (new Vector3(screenWidth, screenHeight) / movementMultiplier / 2);
	}

	public void UpdateLoadingStateText(string primaryText, string secondaryText) {
		if (loadingStateText.gameObject.activeSelf) {
			loadingStateText.text = primaryText.ToUpper();
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

			buttonPanel = MonoBehaviour.Instantiate(buttonPrefab, parent, false);
			buttonPanel.name = buttonText + " (" + buttonPrefab.name + ")";
			button = buttonPanel.transform.Find("Button").GetComponent<Button>();
			text = button.transform.Find("Text").GetComponent<Text>();
			text.text = buttonText;
			panel = buttonPanel.transform.Find("Panel").gameObject;

			button.onClick.AddListener(delegate {
				SetPanelActive(!panel.activeSelf);
			});
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

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"), parent, false);

			ResourceManager.Variation variation = prefab.lastSelectedVariation;
			obj.transform.Find("Text").GetComponent<Text>().text = prefab.GetInstanceNameFromVariation(variation);
			obj.transform.Find("Image").GetComponent<Image>().sprite = prefab.GetBaseSpriteForVariation(variation);

			requiredResourceElementsPanel = obj.transform.Find("RequiredResources-Panel").gameObject;
			variationElementsPanel = obj.transform.Find("Variations-Panel").gameObject;

			variationsIndicator = obj.transform.Find("VariationsIndicator-Image").gameObject;

			obj.GetComponent<HoverToggleScript>().Initialize(
				requiredResourceElementsPanel,
				true,
				prefab.variations.Count > 0
					? new List<GameObject>() { variationElementsPanel }
					: null
			);

			SetVariation(prefab.lastSelectedVariation);

			SetVariationElements();
			UpdateVariationElements();

			obj.GetComponent<HandleClickScript>().Initialize(
				delegate {
					GameManager.jobM.SetSelectedPrefab(prefab, prefab.lastSelectedVariation);
				},
				null,
				delegate {
					if (prefab.variations.Count > 0) {
						variationElementsPanel.SetActive(!variationElementsPanel.activeSelf);
						requiredResourceElementsPanel.SetActive(!variationElementsPanel.activeSelf);
					}
				}
			);
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
				requiredResourceElements.Add(
					new RequiredResourceElement(
						resourceAmount,
						requiredResourceElementsPanel.transform
					)
				);
			}

			if (prefab.lastSelectedVariation != null) {
				foreach (ResourceManager.ResourceAmount resourceAmount in prefab.lastSelectedVariation.uniqueResources) {
					requiredResourceElements.Add(
						new RequiredResourceElement(
							resourceAmount,
							requiredResourceElementsPanel.transform
						)
					);
				}
			}
		}

		private void SetVariationElements() {
			foreach (VariationElement variationElement in variationElements) {
				variationElement.Destroy();
			}
			variationElements.Clear();

			foreach (ResourceManager.Variation variation in prefab.variations) {
				variationElements.Add(
					new VariationElement(
						variation,
						variationElementsPanel.transform,
						this
					)
				);
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

			variationsIndicator.GetComponent<Image>().color =
				requiredResourcesMet && anyVariationResourcesMet
					? GetColour(Colours.LightGreen)
					: GetColour(Colours.LightRed);
		}
	}

	private class RequiredResourceElement {
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public RequiredResourceElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
			this.resourceAmount = resourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), parent, false);

			obj.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resourceAmount.resource.image;
			obj.transform.Find("ResourceName-Text").GetComponent<Text>().text = resourceAmount.resource.name;

			Update();
		}

		public void Update() {

			int availableAmount = resourceAmount.resource.GetAvailableAmount();

			obj.GetComponent<Image>().color = availableAmount >= resourceAmount.amount ? GetColour(Colours.LightGreen) : GetColour(Colours.LightRed);

			obj.transform.Find("AvailableOverRequiredValue-Text").GetComponent<Text>().text = availableAmount + " / " + resourceAmount.amount;
		}

		public void Destroy() {
			MonoBehaviour.Destroy(obj);
		}
	}

	private class VariationElement {
		public ResourceManager.Variation variation;
		public GameObject obj;

		public GameObject requiredResourceElementsPanel;
		public List<RequiredResourceElement> requiredResourceElements = new List<RequiredResourceElement>();

		public VariationElement(ResourceManager.Variation variation, Transform parent, ObjectPrefabButton prefabElement) {
			this.variation = variation;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildVariation-Button-Prefab"), parent, false);
			obj.name = variation.name;

			obj.transform.Find("Text").GetComponent<Text>().text = variation.name;
			obj.transform.Find("Image").GetComponent<Image>().sprite = variation.prefab.GetBaseSpriteForVariation(variation);

			requiredResourceElementsPanel = obj.transform.Find("RequiredResources-Panel").gameObject;
			obj.GetComponent<HoverToggleScript>().Initialize(
				requiredResourceElementsPanel,
				true,
				null
			);
			SetRequiredResourceElements();
			UpdateRequiredResourceElements();

			obj.GetComponent<HandleClickScript>().Initialize(
				delegate {
					prefabElement.SetVariation(variation);
					requiredResourceElementsPanel.SetActive(false);
					GameManager.jobM.SetSelectedPrefab(prefabElement.prefab, variation);
				},
				null,
				delegate {
					prefabElement.variationElementsPanel.SetActive(false);
					requiredResourceElementsPanel.SetActive(false);
				}
			);
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
				requiredResourceElements.Add(
					new RequiredResourceElement(
						resourceAmount,
						requiredResourceElementsPanel.transform
					)
				);
			}

			foreach (ResourceManager.ResourceAmount resourceAmount in variation.uniqueResources) {
				requiredResourceElements.Add(
					new RequiredResourceElement(
						resourceAmount,
						requiredResourceElementsPanel.transform
					)
				);
			}
		}

		public void UpdateRequiredResourceElements() {
			foreach (RequiredResourceElement requiredResourceElement in requiredResourceElements) {
				requiredResourceElement.Update();
			}
		}

		public void Destroy() {
			MonoBehaviour.Destroy(obj);
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

		MenuButton buildButton = new MenuButton(
			null,
			actionsPanel,
			Resources.Load<GameObject>(@"UI/UIElements/BuildMenu-Panel-Prefab"),
			"Build"
		);
		topLevelButtons.Add(buildButton);

		foreach (ResourceManager.ObjectPrefabGroup group in GameManager.resourceM.GetObjectPrefabGroups()) {

			if (noMenuGroups.Contains(group.type)) {
				continue;
			}

			bool separateMenu = separateMenuGroups.Contains(group.type);

			MenuButton groupButton = new MenuButton(
				group,
				separateMenu
					? actionsPanel
					: buildButton.panel.transform,
				separateMenu
					? Resources.Load<GameObject>(@"UI/UIElements/BuildMenu-Panel-Prefab")
					: Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Panel-Prefab"),
				group.name
			);

			if (!separateMenu) {
				buildButton.AddChildButton(groupButton);
			} else {
				topLevelButtons.Add(groupButton);
			}

			foreach (ResourceManager.ObjectPrefabSubGroup subGroup in group.subGroups) {

				MenuButton subGroupButton = new MenuButton(
					subGroup,
					groupButton.panel.transform,
					Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Panel-Prefab"),
					subGroup.name
				);

				groupButton.AddChildButton(subGroupButton);

				foreach (ResourceManager.ObjectPrefab prefab in subGroup.prefabs) {

					ObjectPrefabButton objectPrefabButton = new ObjectPrefabButton(
						prefab,
						subGroupButton.panel.transform
					);

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

		tileRoofElement = MonoBehaviour.Instantiate(
			Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-Label-Panel"),
			tileInformation.transform,
			false
		);
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
					tileRoofElement.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
				}

				if (mouseOverTile.tileType.resourceRanges.Count > 0) {
					for (int i = 0; i < mouseOverTile.tileType.resourceRanges.Count; i++) {
						ResourceManager.ResourceRange resourceRange = mouseOverTile.tileType.resourceRanges[i];

						if (tileResourceElements.Count <= i) {
							tileResourceElements.Add(MonoBehaviour.Instantiate(
								Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ResourceData-Panel"),
								tileInformation.transform,
								false
							));
						}

						GameObject tileResourceElement = tileResourceElements[i];
						tileResourceElement.SetActive(true);
						tileResourceElement.transform.Find("TileInfo-ResourceData-Value").GetComponent<Text>().text = resourceRange.resource.name;
						tileResourceElement.transform.Find("TileInfo-ResourceData-Image").GetComponent<Image>().sprite = resourceRange.resource.image;
						tileResourceElement.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
					}
				}

				if (mouseOverTile.plant != null) {

					if (plantObjectElements.Count <= 0) {
						plantObjectElements.Add(MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"), tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage"), false));
						plantObjectElements.Add(MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"), tileInformation.transform, false));
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
						integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(
							GetColour(Colours.LightRed),
							GetColour(Colours.LightGreen),
							mouseOverTile.plant.integrity / mouseOverTile.plant.prefab.integrity
						);
					} else {
						integritySlider.maxValue = 1;
						integritySlider.value = 1;
						integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.LightGrey200);
					}
				}

				if (mouseOverTile.GetAllObjectInstances().Count > 0) {
					foreach (ResourceManager.ObjectInstance objectInstance in mouseOverTile.GetAllObjectInstances().OrderBy(o => o.prefab.layer).ToList()) {
						if (!objectElements.ContainsKey(objectInstance.prefab.layer)) {
							GameObject spriteObject = MonoBehaviour.Instantiate(GameManager.resourceM.tileImage, tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage-Panel/TileInfoElement-TileImage"), false);
							spriteObject.name = layerToLayerNameMap[objectInstance.prefab.layer];

							GameObject dataObject = MonoBehaviour.Instantiate(GameManager.resourceM.objectDataPanel, tileInformation.transform, false);
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
							integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(
								GetColour(Colours.LightRed),
								GetColour(Colours.LightGreen),
								objectInstance.integrity / objectInstance.prefab.integrity
							);
						} else {
							integritySlider.minValue = 0;
							integritySlider.maxValue = 1;
							integritySlider.value = 1;
							integritySlider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.LightGrey200);
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
		public ColonistManager.Colonist colonist;
		public ColonistManager.SkillInstance skill;
		public GameObject obj;

		public SkillElement(ColonistManager.Colonist colonist, ColonistManager.SkillInstance skill, Transform parent) {
			this.colonist = colonist;
			this.skill = skill;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SkillInfoElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = skill.prefab.name;
			// ADD SKILL IMAGE

			Update();
		}

		public void Update() {
			obj.transform.Find("Level").GetComponent<Text>().text = skill.level + "." + Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f);
			obj.transform.Find("Experience-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f);

			float highestLevel = 0;
			float highestSkill = 0;
			ColonistManager.Colonist highestSkillColonist = colonist;
			foreach (ColonistManager.Colonist otherColonist in GameManager.colonistM.colonists) {
				ColonistManager.SkillInstance foundSkill = otherColonist.skills.Find(findSkill => findSkill.prefab == skill.prefab);
				float otherColonistSkill = foundSkill.level + (foundSkill.currentExperience / foundSkill.nextLevelExperience);
				if (otherColonistSkill > highestSkill) {
					highestSkill = otherColonistSkill;
					highestSkillColonist = otherColonist;
				}
				if (foundSkill.level > highestLevel) {
					highestLevel = foundSkill.level;
				}
			}

			obj.transform.Find("Level-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((skill.level / (highestLevel > 0 ? highestLevel : 1)) * 100f);

			float skillValue = skill.level + (skill.currentExperience / skill.nextLevelExperience);
			if (highestSkillColonist == colonist || Mathf.Approximately(highestSkill, skillValue)) {
				obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkYellow);
				obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = GetColour(Colours.LightYellow);
			} else {
				obj.transform.Find("Level-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkGreen);
				obj.transform.Find("Level-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = GetColour(Colours.LightGreen);
			}
		}
	}

	public class InventoryElement {
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public InventoryElement(ResourceManager.ResourceAmount resourceAmount, Transform parent) {
			this.resourceAmount = resourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceInfoElement-Panel"), parent, false);

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

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceTransferElement-Panel"), parent, false);

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
							transferAmountInput.text = String.Empty;
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

	public class TradeResourceElement {
		public ResourceManager.TradeResourceAmount tradeResourceAmount;

		public GameObject obj;

		private readonly InputField tradeAmountInputField;

		public TradeResourceElement(ResourceManager.TradeResourceAmount tradeResourceAmount, Transform parent) {
			this.tradeResourceAmount = tradeResourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TradeResourceElement-Panel"), parent, false);

			tradeAmountInputField = obj.transform.Find("TradeAmount-Panel/TradeAmount-InputField").GetComponent<InputField>();
			tradeAmountInputField.text = tradeResourceAmount.GetTradeAmount().ToString();
			ValidateTradeAmountInputField();
			tradeAmountInputField.onEndEdit.AddListener(delegate { ValidateTradeAmountInputField(); });

			obj.transform.Find("CaravanResource-Image").GetComponent<Image>().sprite = tradeResourceAmount.resource.image;
			obj.transform.Find("CaravanResourceName-Text").GetComponent<Text>().text = tradeResourceAmount.resource.name;
			obj.transform.Find("CaravanResourceValue-Text").GetComponent<Text>().text = tradeResourceAmount.caravanResourcePrice.ToString();

			obj.transform.Find("TradeAmount-Panel/BuyIncreaseOne-Button").GetComponent<Button>().onClick.AddListener(delegate { ChangeTradeAmount(1); });
			obj.transform.Find("TradeAmount-Panel/BuyIncreaseAll-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeAmount(tradeResourceAmount.caravanAmount); });

			obj.transform.Find("TradeAmount-Panel/SellIncreaseOne-Button").GetComponent<Button>().onClick.AddListener(delegate { ChangeTradeAmount(-1); });
			obj.transform.Find("TradeAmount-Panel/SellIncreaseAll-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeAmount(-tradeResourceAmount.colonyAmount); });

			obj.transform.Find("ColonyResource-Image").GetComponent<Image>().sprite = tradeResourceAmount.resource.image;
			obj.transform.Find("ColonyResourceName-Text").GetComponent<Text>().text = tradeResourceAmount.resource.name;
			obj.transform.Find("ColonyResourceValue-Text").GetComponent<Text>().text = tradeResourceAmount.resource.price.ToString();

			obj.transform.Find("Clear-Button").GetComponent<Button>().onClick.AddListener(delegate { SetTradeAmount(0); });
		}

		public void ChangeTradeAmount(int amount) {
			SetTradeAmountInputField(tradeResourceAmount.GetTradeAmount() + amount);
		}

		public void SetTradeAmount(int tradeAmount) {
			SetTradeAmountInputField(tradeAmount);
		}

		public void SetTradeAmountInputField(int tradeAmount) {
			tradeAmountInputField.text = tradeAmount.ToString();
			ValidateTradeAmountInputField();
		}

		public void ValidateTradeAmountInputField() {
			Text text = tradeAmountInputField.transform.Find("Text").GetComponent<Text>();
			text.color = GetColour(Colours.DarkGrey50);
			int tradeAmount = 0;
			if (tradeAmountInputField.text == "-" || int.TryParse(tradeAmountInputField.text, out tradeAmount)) {
				if (tradeAmountInputField.text == "-") {
					tradeAmount = -1;
				}
				if (tradeAmount == 0) {
					tradeAmountInputField.text = string.Empty;
				} else if (tradeAmount > tradeResourceAmount.caravanAmount) {
					tradeAmount = tradeResourceAmount.caravanAmount;
					tradeAmountInputField.text = tradeAmount.ToString();
				} else if (tradeAmount < -tradeResourceAmount.colonyAmount) {
					tradeAmount = -tradeResourceAmount.colonyAmount;
					tradeAmountInputField.text = tradeAmount.ToString();
				}
			} else {
				tradeAmountInputField.text = string.Empty;
			}
			tradeResourceAmount.SetTradeAmount(tradeAmount);
			if (tradeAmount == 0) {
				tradeAmountInputField.text = string.Empty;
			}
			if (!string.IsNullOrEmpty(tradeAmountInputField.text)) {
				if (tradeAmount > 0) {
					text.color = GetColour(Colours.LightGreen);
				} else if (tradeAmount < 0) {
					text.color = GetColour(Colours.LightRed);
				} else {
					text.color = GetColour(Colours.DarkGrey50);
				}
			}
		}

		public bool Update() {
			tradeResourceAmount.Update();

			int tradeAmount = tradeResourceAmount.GetTradeAmount();

			if (tradeResourceAmount.caravanAmount == 0 && tradeResourceAmount.colonyAmount == 0) {
				MonoBehaviour.Destroy(obj);
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

	public class ConfirmedTradeResourceElement {

		public GameObject obj;

		public ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount;

		public ConfirmedTradeResourceElement(ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount, Transform parent) {
			this.confirmedTradeResourceAmount = confirmedTradeResourceAmount;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ConfirmedTradeResourceElement-Panel"), parent, false);

			obj.transform.Find("Resource-Image").GetComponent<Image>().sprite = confirmedTradeResourceAmount.resource.image;
			obj.transform.Find("ResourceName-Text").GetComponent<Text>().text = confirmedTradeResourceAmount.resource.name;

			Update();
		}

		public void Update() {
			obj.transform.Find("CollectedVsRemainingAmounts-Text").GetComponent<Text>().text = Mathf.Abs(confirmedTradeResourceAmount.tradeAmount - confirmedTradeResourceAmount.amountRemaining) + " / " + Mathf.Abs(confirmedTradeResourceAmount.tradeAmount);

			if (confirmedTradeResourceAmount.amountRemaining == 0) {
				obj.GetComponent<Image>().color = GetColour(Colours.LightGreen);
			} else {
				obj.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
			}
		}
	}

	public class ReservedResourcesColonistElement {
		public ResourceManager.ReservedResources reservedResources;
		public List<InventoryElement> reservedResourceElements = new List<InventoryElement>();
		public GameObject obj;

		public ReservedResourcesColonistElement(HumanManager.Human human, ResourceManager.ReservedResources reservedResources, Transform parent) {
			this.reservedResources = reservedResources;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourcesColonistInfoElement-Panel"), parent, false);

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

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourceInfoElement-Panel"), parent, false);

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

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/NeedElement-Panel"), parent, false);

			obj.transform.Find("NeedName-Text").GetComponent<Text>().text = needInstance.prefab.name;

			Update();
		}

		public void Update() {
			obj.transform.Find("NeedValue-Text").GetComponent<Text>().text = needInstance.GetRoundedValue() + "%";

			obj.transform.Find("Need-Slider").GetComponent<Slider>().value = needInstance.GetValue();
			obj.transform.Find("Need-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkGreen), GetColour(Colours.DarkRed), (needInstance.GetValue() / needInstance.prefab.clampValue));
			obj.transform.Find("Need-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightGreen), GetColour(Colours.LightRed), (needInstance.GetValue() / needInstance.prefab.clampValue));
		}
	}

	public class MoodModifierElement {

		public ColonistManager.MoodModifierInstance moodModifierInstance;
		public GameObject obj;

		public MoodModifierElement(ColonistManager.MoodModifierInstance moodModifierInstance, Transform parent) {
			this.moodModifierInstance = moodModifierInstance;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/MoodModifierElement-Panel"), parent, false);

			obj.transform.Find("MoodModifierName-Text").GetComponent<Text>().text = moodModifierInstance.prefab.name;
			if (moodModifierInstance.prefab.effectAmount > 0) {
				obj.GetComponent<Image>().color = GetColour(Colours.LightGreen);
			} else if (moodModifierInstance.prefab.effectAmount < 0) {
				obj.GetComponent<Image>().color = GetColour(Colours.LightRed);
			} else {
				obj.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
			}

			Update();
		}

		public bool Update() {
			if (!moodModifierInstance.colonist.moodModifiers.Contains(moodModifierInstance)) {
				MonoBehaviour.Destroy(obj);
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
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;

			selectedColonistInformationPanel.SetActive(true);

			selectedColonistInformationPanel.transform.Find("ColonistName-Text").GetComponent<Text>().text = GameManager.humanM.selectedHuman.name + " (" + GameManager.humanM.selectedHuman.gender.ToString()[0] + ")";
			selectedColonistInformationPanel.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = GameManager.humanM.selectedHuman.moveSprites[0];

			selectedColonistInformationPanel.transform.Find("AffiliationName-Text").GetComponent<Text>().text = "Colonist of " + GameManager.colonyM.colony.name;

			RemakeSelectedColonistNeeds();
			RemakeSelectedColonistMoodModifiers();
			RemakeSelectedColonistSkills();
			RemakeSelectedColonistInventory(selectedColonist);
			RemakeSelectedColonistClothing(selectedColonist, sameColonistSelected);
		} else {
			selectedColonistInformationPanel.SetActive(false);

			if (selectedColonistSkillElements.Count > 0) {
				foreach (SkillElement skillElement in selectedColonistSkillElements) {
					MonoBehaviour.Destroy(skillElement.obj);
				}
				selectedColonistSkillElements.Clear();
			}
			if (selectedColonistNeedElements.Count > 0) {
				foreach (NeedElement needElement in selectedColonistNeedElements) {
					MonoBehaviour.Destroy(needElement.obj);
				}
				selectedColonistNeedElements.Clear();
			}
			if (selectedColonistReservedResourcesColonistElements.Count > 0) {
				foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in selectedColonistReservedResourcesColonistElements) {
					foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
						MonoBehaviour.Destroy(reservedResourceElement.obj);
					}
					reservedResourcesColonistElement.reservedResourceElements.Clear();
					MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
				}
				selectedColonistReservedResourcesColonistElements.Clear();
			}
			if (selectedColonistInventoryElements.Count > 0) {
				foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
					MonoBehaviour.Destroy(inventoryElement.obj);
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
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist selectedColonist) {
			foreach (NeedElement needElement in selectedColonistNeedElements) {
				MonoBehaviour.Destroy(needElement.obj);
			}
			selectedColonistNeedElements.Clear();
			List<ColonistManager.NeedInstance> sortedNeeds = selectedColonist.needs.OrderByDescending(need => need.GetValue()).ToList();
			foreach (ColonistManager.NeedInstance need in sortedNeeds) {
				selectedColonistNeedElements.Add(new NeedElement(need, selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/Needs-ScrollPanel/NeedsList-Panel")));
			}
		}
	}

	public void RemakeSelectedColonistMoodModifiers() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist selectedColonist) {
			foreach (MoodModifierElement moodModifierElement in selectedColonistMoodModifierElements) {
				MonoBehaviour.Destroy(moodModifierElement.obj);
			}
			selectedColonistMoodModifierElements.Clear();
			foreach (ColonistManager.MoodModifierInstance moodModifier in selectedColonist.moodModifiers) {
				selectedColonistMoodModifierElements.Add(new MoodModifierElement(moodModifier, selectedColonistMoodModifiersPanel.transform.Find("MoodModifier-ScrollPanel/MoodModifierList-Panel")));
			}
		}
	}

	public void RemakeSelectedColonistSkills() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist selectedColonist) {
			foreach (SkillElement skillElement in selectedColonistSkillElements) {
				MonoBehaviour.Destroy(skillElement.obj);
			}
			selectedColonistSkillElements.Clear();
			List<ColonistManager.SkillInstance> sortedSkills = selectedColonist.skills.OrderByDescending(skill => skill.CalculateTotalSkillLevel()).ToList();
			foreach (ColonistManager.SkillInstance skill in sortedSkills) {
				selectedColonistSkillElements.Add(new SkillElement(selectedColonist, skill, selectedColonistNeedsSkillsPanel.transform.Find("Skills-Panel/Skills-ScrollPanel/SkillsList-Panel")));
			}
		}
	}

	public void RemakeSelectedColonistInventory(ColonistManager.Colonist selectedColonist) {
		foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in selectedColonistReservedResourcesColonistElements) {
			foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
				MonoBehaviour.Destroy(reservedResourceElement.obj);
			}
			reservedResourcesColonistElement.reservedResourceElements.Clear();
			MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
		}
		selectedColonistReservedResourcesColonistElements.Clear();
		foreach (InventoryElement inventoryElement in selectedColonistInventoryElements) {
			MonoBehaviour.Destroy(inventoryElement.obj);
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

	public void RemakeSelectedColonistClothing(ColonistManager.Colonist selectedColonist, bool selectionPanelKeepState) {
		selectedColonistClothingPanel.transform.Find("ColonistBody-Image").GetComponent<Image>().sprite = selectedColonist.moveSprites[0];

		foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothingKVP in selectedColonist.clothes) {

			ResourceManager.Clothing clothing = selectedColonist.clothes[appearanceToClothingKVP.Key];

			Button clothingTypeButton = selectedColonistClothingPanel.transform.Find("ClothingButtons-List/" + appearanceToClothingKVP.Key + "-Button").GetComponent<Button>();

			clothingTypeButton.GetComponent<Image>().color = GetColour(Colours.LightGrey180);

			if (clothing == null) {
				
				List<JobManager.Job> checkJobs = new List<JobManager.Job>();
				checkJobs.AddRange(selectedColonist.backlog);
				checkJobs.Add(selectedColonist.storedJob);
				checkJobs.Add(selectedColonist.job);

				foreach (JobManager.Job checkJob in checkJobs) {
					if (checkJob != null && checkJob.prefab.type == ResourceManager.ObjectEnum.WearClothes) {
						ResourceManager.Clothing clothingToWear = (ResourceManager.Clothing)checkJob.resourcesToBuild[0].resource;
						if (clothingToWear.prefab.appearance == appearanceToClothingKVP.Key) {
							clothing = clothingToWear;
							clothingTypeButton.GetComponent<Image>().color = GetColour(Colours.LightOrange);
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
			MonoBehaviour.Destroy(clothingElement.obj);
		}
		availableClothingElements.Clear();
		foreach (ClothingElement clothingElement in takenClothingElements) {
			MonoBehaviour.Destroy(clothingElement.obj);
		}
		takenClothingElements.Clear();
	}

	public void SetSelectedColonistClothingSelectionPanel(bool active, HumanManager.Human.Appearance clothingType, ColonistManager.Colonist selectedColonist) {
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

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ClothingElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = clothing.name;
			obj.transform.Find("Image").GetComponent<Image>().sprite = clothing.moveSprites[0];
			if (clothing.prefab.appearance == HumanManager.Human.Appearance.Backpack) {
				obj.transform.Find("InsulationWaterResistance").GetComponent<Text>().text = "+" + clothing.prefab.weightCapacity + "kg / +" + clothing.prefab.volumeCapacity + "m³";
			} else {
				obj.transform.Find("InsulationWaterResistance").GetComponent<Text>().text = "❄ " + clothing.prefab.insulation + " / ☂ " + clothing.prefab.waterResistance;
			}

			obj.GetComponent<Button>().onClick.AddListener(delegate {

				human.ChangeClothing(clothing.prefab.appearance, clothing);

				GameManager.uiM.SetSelectedColonistClothingSelectionPanelActive(false);
				GameManager.uiM.SetSelectedColonistInformation(true);
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
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
			ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedColonist.health * 100);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), selectedColonist.health);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedColonist.health);
			//selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistHealth-Panel/ColonistHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.health * 100) + "%";

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMood-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedColonist.effectiveMood);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMood-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), selectedColonist.effectiveMood / 100f);
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMood-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedColonist.effectiveMood / 100f);
			//selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistMood-Panel/ColonistMoodValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.effectiveMood) + "%";

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider").GetComponent<Slider>().minValue = 0;
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider").GetComponent<Slider>().maxValue = selectedColonist.GetInventory().maxWeight;
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider").GetComponent<Slider>().value = selectedColonist.GetInventory().UsedWeight();
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryWeight-Slider/Handle Slide Area/Handle/Text").GetComponent<Text>().text = Mathf.RoundToInt((selectedColonist.GetInventory().UsedWeight() / (float)selectedColonist.GetInventory().maxWeight) * 100).ToString();

			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider").GetComponent<Slider>().minValue = 0;
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider").GetComponent<Slider>().maxValue = selectedColonist.GetInventory().maxVolume;
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider").GetComponent<Slider>().value = selectedColonist.GetInventory().UsedVolume();
			selectedColonistInformationPanel.transform.Find("ColonistStatusBars-Panel/ColonistInventorySlider-Panel/SliderSplitter-Panel/ColonistInventoryVolume-Slider/Handle Slide Area/Handle/Text").GetComponent<Text>().text = Mathf.RoundToInt((selectedColonist.GetInventory().UsedVolume() / (float)selectedColonist.GetInventory().maxVolume) * 100).ToString();

			selectedColonistInformationPanel.transform.Find("ColonistCurrentAction-Text").GetComponent<Text>().text = GameManager.jobM.GetJobDescription(selectedColonist.job);
			if (selectedColonist.storedJob != null) {
				selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = GameManager.jobM.GetJobDescription(selectedColonist.storedJob);
			} else {
				selectedColonistInformationPanel.transform.Find("ColonistStoredAction-Text").GetComponent<Text>().text = string.Empty;
			}

			int moodModifiersSum = Mathf.RoundToInt(selectedColonist.moodModifiersSum);

			int moodLength = Mathf.Abs(moodModifiersSum).ToString().Length;
			Text moodModifierAmountText = selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodModifiers-Button/MoodModifiersAmount-Text").GetComponent<Text>();
			if (moodModifiersSum > 0) {
				moodModifierAmountText.text = "+" + moodModifiersSum + "%";
				moodModifierAmountText.color = GetColour(Colours.LightGreen);
			} else if (moodModifiersSum < 0) {
				moodModifierAmountText.text = moodModifiersSum + "%";
				moodModifierAmountText.color = GetColour(Colours.LightRed);
			} else {
				moodModifierAmountText.text = moodModifiersSum + "%";
				moodModifierAmountText.color = GetColour(Colours.DarkGrey50);
			}
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodModifiers-Button").GetComponent<RectTransform>().sizeDelta = new Vector2(moodModifierButtonSizeMap[moodLength], 20);
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodValue-Text").GetComponent<RectTransform>().offsetMax = new Vector2(moodModifierValueHorizontalPositionMap[moodLength], 0);
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedColonist.effectiveMood) + "%";
			selectedColonistNeedsSkillsPanel.transform.Find("Needs-Panel/MoodValue-Text").GetComponent<Text>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedColonist.effectiveMood / 100f);

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
					MonoBehaviour.Destroy(moodModifierElement.obj);
					selectedColonistMoodModifierElements.Remove(moodModifierElement);
				}
				removeHME.Clear();
			}
		}
	}

	public class ColonistElement {

		public ColonistManager.Colonist colonist;
		public GameObject obj;

		public ColonistElement(ColonistManager.Colonist colonist, Transform transform) {
			this.colonist = colonist;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), transform, false);

			obj.GetComponent<RectTransform>().sizeDelta = new Vector2(135, obj.GetComponent<RectTransform>().sizeDelta.y);

			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });

			Update();
		}

		public void Update() {
			obj.GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), colonist.health);
		}

		public void DestroyObject() {
			MonoBehaviour.Destroy(obj);
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
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
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

		public CaravanManager.Caravan caravan;
		public GameObject obj;

		private readonly Text affiliatedColonyNameText;
		private readonly Text resourceGroupNameText;

		public CaravanElement(CaravanManager.Caravan caravan, Transform transform) {
			this.caravan = caravan;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/CaravanElement-Panel"), transform, false);

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
			obj.GetComponent<Image>().color = caravan.confirmedResourcesToTrade.Count > 0 ? GetColour(Colours.LightYellow) : GetColour(Colours.LightPurple);

			Color textColour = caravan.confirmedResourcesToTrade.Count > 0 ? GetColour(Colours.DarkGrey50) : GetColour(Colours.LightGrey220);
			affiliatedColonyNameText.color = textColour;
			resourceGroupNameText.color = textColour;

			obj.GetComponent<Button>().enabled = caravan.traders.Count > 0;
		}

		public void DestroyObject() {
			MonoBehaviour.Destroy(obj);
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
				foreach (CaravanManager.Caravan caravan in GameManager.caravanM.caravans.Where(c => c.traders[0].overTile.IsVisibleToAColonist()).OrderByDescending(c => c.confirmedResourcesToTrade.Count > 0)) {
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
		public JobManager.Job job;
		public ColonistManager.Colonist colonist;
		public GameObject obj;
		public GameObject colonistObj;

		public JobElement(JobManager.Job job, ColonistManager.Colonist colonist, Transform parent) {
			this.job = job;
			this.colonist = colonist;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/JobInfoElement-Panel"), parent, false);
			GameObject jobInfo = obj.transform.Find("Content/JobInfo").gameObject;
			Text jobInfoNameText = jobInfo.transform.Find("Name").GetComponent<Text>();

			if (job.prefab.type == ResourceManager.ObjectEnum.CreateResource) {
				jobInfo.transform.Find("Image").GetComponent<Image>().sprite = job.createResource.resource.image;
			} else {
				jobInfo.transform.Find("Image").GetComponent<Image>().sprite = job.prefab.GetBaseSpriteForVariation(job.variation);
			}

			switch (job.prefab.jobType) {
				case JobManager.JobEnum.Mine:
				case JobManager.JobEnum.Dig:
					jobInfoNameText.text = job.tile.tileType.name;
					break;
				case JobManager.JobEnum.PlantPlant:
					jobInfoNameText.text = GameManager.resourceM.GetPlantGroupByEnum(job.variation.plants.First().Key.groupType).name;
					break;
				case JobManager.JobEnum.ChopPlant:
					jobInfoNameText.text = job.tile.plant.name;
					break;
				case JobManager.JobEnum.PlantFarm:
					jobInfoNameText.text = job.prefab.name;
					break;
				case JobManager.JobEnum.HarvestFarm:
					jobInfoNameText.text = job.tile.farm.name;
					break;
				case JobManager.JobEnum.CreateResource:
					jobInfoNameText.text = job.createResource.resource.name;
					break;
				case JobManager.JobEnum.Remove:
					if (job.prefab.type == ResourceManager.ObjectEnum.RemoveRoof) {
						jobInfoNameText.text = "Roof";
					} else {
						jobInfoNameText.text = job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.name;
					}
					break;
				default:
					jobInfoNameText.text = job.prefab.name;
					break;
			}
			jobInfo.transform.Find("Type").GetComponent<Text>().text = SplitByCapitals(job.prefab.jobType.ToString());
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				GameManager.cameraM.SetCameraPosition(job.tile.obj.transform.position);
			});

			bool hasPriority = job.priority != 0;

			Text priorityText = jobInfo.transform.Find("Priority").GetComponent<Text>();

			if (hasPriority) {
				priorityText.text = job.priority.ToString();
				if (job.priority > 0) {
					priorityText.color = GetColour(Colours.DarkYellow);
				} else if (job.priority < 0) {
					priorityText.color = GetColour(Colours.DarkRed);
				}
			} else {
				priorityText.text = string.Empty;
			}

			if (job.resourcesToBuild.Count > 0) {

				obj.transform.Find("Content/RequiredResources-Panel").GetComponent<Image>().color = GetColour(Colours.WhiteAlpha64);

				foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
					GameObject resourceAmountObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/RequiredResource-Panel"), obj.transform.Find("Content/RequiredResources-Panel"), false);

					resourceAmountObj.GetComponent<Image>().color = GetColour(Colours.Clear);

					resourceAmountObj.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resourceAmount.resource.image;
					resourceAmountObj.transform.Find("ResourceName-Text").GetComponent<Text>().text = resourceAmount.resource.name;
					resourceAmountObj.transform.Find("AvailableOverRequiredValue-Text").GetComponent<Text>().text = resourceAmount.amount.ToString();
				}
			}

			if (colonist != null) {

				colonistObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"), obj.transform.Find("Content"), false);
				colonistObj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
				colonistObj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
				colonistObj.GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });
				colonistObj.GetComponent<Image>().color = GetColour(Colours.WhiteAlpha64);

				colonistObj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<LayoutElement>().minWidth - 6, colonistObj.GetComponent<RectTransform>().sizeDelta.y);
				obj.transform.Find("Content").GetComponent<VerticalLayoutGroup>().padding.bottom = 3;

				if (job.started) {
					obj.GetComponent<Image>().color = GetColour(Colours.LightGreen);

					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().minValue = 0;
					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
					obj.transform.Find("JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkGreen);
				} else {
					obj.GetComponent<Image>().color = GetColour(Colours.LightOrange);

					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().minValue = 0;
					obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().maxValue = colonist.startPathLength;
					obj.transform.Find("JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkOrange);
				}
			} else {
				obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().minValue = 0;
				obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
				obj.transform.Find("JobProgress-Slider/Fill Area/Fill").GetComponent<Image>().color = GetColour(Colours.DarkGreen);
			}

			job.jobUIElement = this;

			Update();
		}

		public void Update() {
			if (colonist != null) {
				//colonistObj.GetComponent<Image>().color = Color.Lerp(ChangeAlpha(GetColour(Colours.LightRed), 150f / 255f), ChangeAlpha(GetColour(Colours.LightGreen), 150f / 255f), colonist.health);
			}
			if (colonist != null && !job.started && colonist.startPathLength > 0 && colonist.path.Count > 0) {
				obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().value = (1 - (colonist.path.Count / (float)colonist.startPathLength)) * colonist.startPathLength;//Mathf.Lerp((1 - (colonist.path.Count / (float)colonist.startPathLength)) * colonist.startPathLength, (1 - ((colonist.path.Count - 1) / (float)colonist.startPathLength)) * colonist.startPathLength, (1 - Vector2.Distance(colonist.obj.transform.position, colonist.path[0].obj.transform.position)) + 0.5f);
			} else {
				obj.transform.Find("JobProgress-Slider").GetComponent<Slider>().value = job.colonistBuildTime - job.jobProgress;
			}
		}

		public void DestroyObjects() {
			job.jobUIElement = null;
			MonoBehaviour.Destroy(obj);
			MonoBehaviour.Destroy(colonistObj);
		}

		public void Remove() {
			GameManager.uiM.jobElements.Remove(this);
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
		if (GameManager.jobM.jobs.Count > 0 || GameManager.colonistM.colonists.Where(colonist => colonist.job != null).ToList().Count > 0) {
			RemoveJobElements();
			List<ColonistManager.Colonist> orderedColonists = GameManager.colonistM.colonists.Where(colonist => colonist.job != null).ToList();
			foreach (ColonistManager.Colonist jobColonist in orderedColonists.Where(colonist => colonist.job.started).OrderBy(colonist => colonist.job.jobProgress)) {
				jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobListPanel.transform));
			}
			foreach (ColonistManager.Colonist jobColonist in orderedColonists.Where(colonist => !colonist.job.started).OrderBy(colonist => colonist.path.Count)) {
				jobElements.Add(new JobElement(jobColonist.job, jobColonist, jobListPanel.transform));
			}
			foreach (JobManager.Job job in GameManager.jobM.jobs.Where(j => j.started).OrderBy(j => (j.jobProgress / j.colonistBuildTime))) {
				jobElements.Add(new JobElement(job, null, jobListPanel.transform));
			}
			foreach (JobManager.Job job in GameManager.jobM.jobs.Where(j => !j.started).OrderByDescending(j => j.priority)) {
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

	public void UpdateDateTimeInformation(int minute, int hour, string day, TimeManager.Season season, int year, bool isDay) {
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Speed-Text").GetComponent<Text>().text = GameManager.timeM.GetTimeModifier() > 0 ? new string('>', GameManager.timeM.GetTimeModifier()) : "-";
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Time-Text").GetComponent<Text>().text = GameManager.timeM.Get12HourTime() + ":" + (minute < 10 ? ("0" + minute) : minute.ToString()) + " " + (hour < 12 || hour > 23 ? "AM" : "PM") + " - " + (isDay ? "Day" : "Night");
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Date-Text").GetComponent<Text>().text = day + " of " + season;
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Year-Text").GetComponent<Text>().text = "Year " + year;
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
			canvas.transform.position = new Vector2(
				mousePosition.x + ((canvas.GetComponent<RectTransform>().sizeDelta.x / 2f + 10) * canvas.transform.localScale.x),
				mousePosition.y + ((canvas.GetComponent<RectTransform>().sizeDelta.y / 2f + 10) * canvas.transform.localScale.y)
			);
		}
	}

	public void InitializeSelectedContainerIndicator() {
		selectedContainerIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
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
				MonoBehaviour.Destroy(reservedResourceElement.obj);
			}
			reservedResourcesColonistElement.reservedResourceElements.Clear();
			MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
		}
		containerReservedResourcesColonistElements.Clear();
		foreach (InventoryElement inventoryElement in containerInventoryElements) {
			MonoBehaviour.Destroy(inventoryElement.obj);
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
				inventoryElement.obj.GetComponent<Image>().color = GetColour(Colours.LightGrey200);
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
		selectedTradingPostIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
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
			MonoBehaviour.Destroy(resourceTransferElement.obj);
		}
		tradingPostResourceTransferElements.Clear();
		foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in tradingPostReservedResourcesColonistElements) {
			foreach (InventoryElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
				MonoBehaviour.Destroy(reservedResourceElement.obj);
			}
			reservedResourcesColonistElement.reservedResourceElements.Clear();
			MonoBehaviour.Destroy(reservedResourcesColonistElement.obj);
		}
		tradingPostReservedResourcesColonistElements.Clear();
		foreach (ResourceTransferElement inventoryElement in tradingPostInventoryElements) {
			MonoBehaviour.Destroy(inventoryElement.obj);
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
					tradingPostResourceTransferElements.Add(
						new ResourceTransferElement(
							resource,
							selectedTradingPost,
							ResourceTransferElement.TransferType.In,
							selectedTradingPostPanel.transform.Find("AvailableResources-Panel/AvailableResources-ScrollPanel/AvailableResourcesList-Panel")
						)
					);
				}
			}

			selectedTradingPostPanel.transform.Find("AvailableResources-Panel/TransferIn-Button").GetComponent<Button>().onClick.AddListener(delegate {
				JobManager.Job job = new JobManager.Job(
					selectedTradingPost.tile,
					GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.TransferResources),
					null,
					0
				);
				foreach (ResourceTransferElement rte in tradingPostResourceTransferElements) {
					if (rte.transferAmount > 0) {
						job.resourcesToBuild.Add(new ResourceManager.ResourceAmount(rte.resource, rte.transferAmount));
					}
				}
				if (job.resourcesToBuild.Count > 0) {
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
				ResourceTransferElement inventoryElement = new ResourceTransferElement(
					ra.resource,
					selectedTradingPost,
					ResourceTransferElement.TransferType.Out,
					selectedTradingPostPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryResourcesList-Panel")
				);
				tradingPostInventoryElements.Add(inventoryElement);
			}

			selectedTradingPostPanel.transform.Find("Inventory-Panel/TransferOut-Button").GetComponent<Button>().onClick.AddListener(delegate {
				JobManager.Job job = new JobManager.Job(
					selectedTradingPost.tile,
					GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectResources),
					null,
					0
				) {
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
		public ColonistManager.ProfessionPrefab professionPrefab;
		public GameObject obj;

		public Dictionary<ColonistManager.Colonist, Button> colonistToPriorityButtons = new Dictionary<ColonistManager.Colonist, Button>();

		public ProfessionColumn(ColonistManager.ProfessionPrefab professionPrefab, Transform parent, int index) {
			this.professionPrefab = professionPrefab;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ProfessionColumn-Panel"), parent, false);

			obj.transform.Find("ProfessionName-Text").GetComponent<Text>().text = professionPrefab.name;
			if (index % 2 == 1) {
				obj.transform.Find("ProfessionName-Text").GetComponent<Text>().alignment = TextAnchor.LowerCenter;
			}
		}

		public void AddButton(ColonistManager.ProfessionInstance professionInstance) {
			GameObject buttonObj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistProfessionPriority-Button"), obj.transform, false);
			Button button = buttonObj.GetComponent<Button>();

			UpdateButton(button, professionInstance);

			button.GetComponent<HandleClickScript>().Initialize(
				delegate {
					professionInstance.DecreasePriority();
					UpdateButton(button, professionInstance);
				},
				null,
				delegate {
					professionInstance.IncreasePriority();
					UpdateButton(button, professionInstance);
				}
			);

			colonistToPriorityButtons.Add(professionInstance.colonist, button);
		}

		public void UpdateButton(Button button, ColonistManager.ProfessionInstance professionInstance) {
			int priority = professionInstance.GetPriority();

			button.transform.Find("Text").GetComponent<Text>().text = priority == 0 ? string.Empty : priority.ToString();

			button.transform.Find("Text").GetComponent<Text>().color = Color.Lerp(
				GetColour(Colours.DarkGreen),
				GetColour(Colours.DarkRed),
				(priority - 1f) / (9 - 1f)
			);
		}

		public void RemoveButton(ColonistManager.Colonist colonist) {
			if (colonistToPriorityButtons.ContainsKey(colonist)) {
				MonoBehaviour.Destroy(colonistToPriorityButtons[colonist].gameObject);
			}
			colonistToPriorityButtons.Remove(colonist);
		}
	}

	public class ColonistProfessionsRow {
		public ColonistManager.Colonist colonist;
		public GameObject obj;

		public ColonistProfessionsRow(ColonistManager.Colonist colonist, Transform parent) {
			this.colonist = colonist;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistProfessionsRow-Panel"), parent, false);

			obj.transform.Find("Colonist-Button").GetComponent<Button>().onClick.AddListener(delegate { GameManager.humanM.SetSelectedHuman(colonist); });

			obj.transform.Find("Colonist-Button/Text").GetComponent<Text>().text = colonist.name;
			obj.transform.Find("Colonist-Button/Image").GetComponent<Image>().sprite = colonist.moveSprites[0];
		}

		public void Destroy() {
			MonoBehaviour.Destroy(obj);
		}
	}

	public void CreateProfessionsMenu() {

		int index = 0;
		foreach (ColonistManager.ProfessionPrefab profession in GameManager.colonistM.professionPrefabs) {
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
			MonoBehaviour.Destroy(colonistProfessionsRowBackground);
		}
		colonistProfessionsRowBackgrounds.Clear();

		// Creation

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			colonistProfessionsRows.Add(new ColonistProfessionsRow(colonist, professionsMenu.transform.Find("ColonistsColumn-Panel")));

			colonistProfessionsRowBackgrounds.Add(MonoBehaviour.Instantiate(
				Resources.Load<GameObject>(@"UI/UIElements/ColonistProfessionsRowBackground-Panel"),
				professionsMenu.transform.Find("ColonistProfessionsRowBackgrounds-Panel"),
				false
			));

			foreach (ColonistManager.ProfessionInstance professionInstance in colonist.professions) {
				professionColumns.Find(pc => pc.professionPrefab == professionInstance.prefab).AddButton(professionInstance);
			}
		}
	}

	public void UpdateProfessionsMenu() {

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			if (colonistProfessionsRows.Find(cpr => cpr.colonist == colonist) == null) {
				SetProfessionsMenu();
			}
		}
		foreach (ColonistProfessionsRow colonistProfessionsRow in colonistProfessionsRows) {
			if (GameManager.colonistM.colonists.Find(c => c == colonistProfessionsRow.colonist) == null) {
				SetProfessionsMenu();
			}
		}
	}

	public void ToggleProfessionsMenu() {
		professionsMenu.SetActive(!professionsMenu.activeSelf);

		UpdateProfessionsMenu();
	}

	public void SetSelectedTraderMenu() {
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is CaravanManager.Trader selectedTrader) {
			CaravanManager.Caravan caravan = selectedTrader.caravan;

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
		if (GameManager.humanM.selectedHuman != null && GameManager.humanM.selectedHuman is CaravanManager.Trader selectedTrader) {

			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider").GetComponent<Slider>().value = Mathf.RoundToInt(selectedTrader.health * 100);
			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider/Fill Area/Fill").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.DarkRed), GetColour(Colours.DarkGreen), selectedTrader.health);
			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealth-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = Color.Lerp(GetColour(Colours.LightRed), GetColour(Colours.LightGreen), selectedTrader.health);
			selectedTraderMenu.transform.Find("TraderHealth-Panel/TraderHealthValue-Text").GetComponent<Text>().text = Mathf.RoundToInt(selectedTrader.health * 100) + "%";
		}
	}

	private readonly List<TradeResourceElement> tradeResourceElements = new List<TradeResourceElement>();
	private readonly List<ConfirmedTradeResourceElement> confirmedTradeResourceElements = new List<ConfirmedTradeResourceElement>();

	public void SetTradeMenuActive(bool active) {
		tradeMenu.SetActive(active);

		foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
			MonoBehaviour.Destroy(tradeResourceElement.obj);
		}
		tradeResourceElements.Clear();
		foreach (ConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
			MonoBehaviour.Destroy(confirmedTradeResourceElement.obj);
		}
		confirmedTradeResourceElements.Clear();
	}

	public void SetTradeMenu() {
		CaravanManager.Caravan caravan = GameManager.caravanM.selectedCaravan;
		if (caravan != null) {

			SetTradeMenuActive(true);

			tradeMenu.transform.Find("AffiliationCaravanName-Text").GetComponent<Text>().text = "Trade Caravan of " + caravan.location.name;
			tradeMenu.transform.Find("CaravanResourceGroup-Text").GetComponent<Text>().text = "Selling " + caravan.resourceGroup.name;
			tradeMenu.transform.Find("AffiliationDescription-Text").GetComponent<Text>().text = string.Format(
				"{0} is a {1} {2} with {3} resources in a {4} climate.",
				caravan.location.name,
				caravan.location.wealth.ToString().ToLower(),
				caravan.location.citySize.ToString().ToLower(),
				caravan.location.resourceRichness.ToString().ToLower(),
				TileManager.Biome.GetBiomeByEnum(caravan.location.biomeType).name.ToLower()
			);

			RemakeTradeResourceElements(caravan);
			RemakeConfirmedTradeResourceElements(caravan);
		} else {
			SetTradeMenuActive(false);
		}
	}

	private void RemakeTradeResourceElements(CaravanManager.Caravan caravan) {
		foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
			MonoBehaviour.Destroy(tradeResourceElement.obj);
		}
		tradeResourceElements.Clear();
		foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in caravan.GenerateTradeResourceAmounts()) {
			tradeResourceElements.Add(new TradeResourceElement(tradeResourceAmount, tradeMenu.transform.Find("TradeResources-Panel/TradeResources-ScrollPanel/TradeResourcesList-Panel")));
		}
	}

	private void RemakeConfirmedTradeResourceElements(CaravanManager.Caravan caravan) {
		foreach (ConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
			MonoBehaviour.Destroy(confirmedTradeResourceElement.obj);
		}
		confirmedTradeResourceElements.Clear();
		foreach (ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade.Where(crtt => crtt.tradeAmount > 0).OrderByDescending(crtt => crtt.tradeAmount)) {
			confirmedTradeResourceElements.Add(new ConfirmedTradeResourceElement(confirmedTradeResourceAmount, tradeMenu.transform.Find("ConfirmedTradeResources-Panel/ConfirmedTradeResources-ScrollPanel/ConfirmedTradeResourcesList-Panel")));
		}
		foreach (ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade.Where(crtt => crtt.tradeAmount < 0).OrderBy(crtt => crtt.tradeAmount)) {
			confirmedTradeResourceElements.Add(new ConfirmedTradeResourceElement(confirmedTradeResourceAmount, tradeMenu.transform.Find("ConfirmedTradeResources-Panel/ConfirmedTradeResources-ScrollPanel/ConfirmedTradeResourcesList-Panel")));
		}
	}

	private readonly List<TradeResourceElement> removedTradeResourceElements = new List<TradeResourceElement>();

	public void UpdateTradeMenu() {
		if (tradeMenu.activeSelf) {
			int caravanTradeValue = 0;
			int colonyTradeValue = 0;

			int caravanTradeAmount = 0;
			int colonyTradeAmount = 0;

			if (GameManager.caravanM.selectedCaravan.traders.Count > 0) {
				List<ResourceManager.ResourceAmount> availableResources = GameManager.resourceM.GetAvailableResourcesInTradingPostsInRegion(GameManager.caravanM.selectedCaravan.traders.Find(t => t != null).overTile.region);
				foreach (ResourceManager.ResourceAmount resourceAmount in availableResources) {
					TradeResourceElement tradeResourceElement = tradeResourceElements.Find(tre => tre.tradeResourceAmount.resource == resourceAmount.resource);
					if (tradeResourceElement != null) {
						tradeResourceElement.tradeResourceAmount.SetColonyAmount(resourceAmount.amount);
						tradeResourceElement.ValidateTradeAmountInputField();
					} else {
						tradeResourceElements.Add(new TradeResourceElement(new ResourceManager.TradeResourceAmount(resourceAmount.resource, 0, GameManager.caravanM.selectedCaravan), tradeMenu.transform.Find("TradeResources-Panel/TradeResources-ScrollPanel/TradeResourcesList-Panel")));
					}
				}
				foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
					ResourceManager.ResourceAmount resourceAmount = availableResources.Find(ra => ra.resource == tradeResourceElement.tradeResourceAmount.resource);
					if (resourceAmount != null) {
						tradeResourceElement.tradeResourceAmount.SetColonyAmount(resourceAmount.amount);
					} else {
						tradeResourceElement.tradeResourceAmount.SetColonyAmount(0);
					}
					tradeResourceElement.ValidateTradeAmountInputField();
				}
			}

			foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
				bool removed = tradeResourceElement.Update();
				if (removed) {
					removedTradeResourceElements.Add(tradeResourceElement);
					continue;
				}

				ResourceManager.TradeResourceAmount tradeResourceAmount = tradeResourceElement.tradeResourceAmount;
				int tradeAmount = tradeResourceAmount.GetTradeAmount();

				if (tradeAmount < 0) {
					colonyTradeValue += Mathf.Abs(tradeResourceAmount.resource.price * tradeAmount);
					colonyTradeAmount += Mathf.Abs(tradeAmount);
				} else if (tradeAmount > 0) {
					caravanTradeValue += Mathf.Abs(tradeResourceAmount.caravanResourcePrice * tradeAmount);
					caravanTradeAmount += Mathf.Abs(tradeAmount);
				}
			}

			foreach (TradeResourceElement tradeResourceElement in removedTradeResourceElements) {
				tradeResourceElements.Remove(tradeResourceElement);
			}
			removedTradeResourceElements.Clear();

			int totalTradeAmount = caravanTradeAmount + colonyTradeAmount;

			tradeMenu.transform.Find("CaravanTradeAmount-Text").GetComponent<Text>().text = caravanTradeAmount != 0 ? caravanTradeAmount + " resource" + (caravanTradeAmount == 1 ? string.Empty : "s") : string.Empty;
			tradeMenu.transform.Find("CaravanTradeValue-Text").GetComponent<Text>().text = caravanTradeAmount != 0 ? caravanTradeValue + " value" : string.Empty;

			tradeMenu.transform.Find("ColonyTradeAmount-Text").GetComponent<Text>().text = colonyTradeAmount != 0 ? colonyTradeAmount + " resource" + (colonyTradeAmount == 1 ? string.Empty : "s") : string.Empty;
			tradeMenu.transform.Find("ColonyTradeValue-Text").GetComponent<Text>().text = colonyTradeAmount != 0 ? colonyTradeValue + " value" : string.Empty;

			int tradeValueDifference = caravanTradeValue - colonyTradeValue;
			string tradeFairness = "Equal Trade";
			if (tradeValueDifference > 0) {
				tradeFairness = "Unfair to Caravan";
			} else if (tradeValueDifference < 0) {
				tradeFairness = "Unfair to Colony";
			}
			tradeMenu.transform.Find("TradeValueDifference-Text").GetComponent<Text>().text = totalTradeAmount != 0 ? Mathf.Abs(tradeValueDifference).ToString() : "Select Resources to Trade";
			tradeMenu.transform.Find("TradeFairness-Text").GetComponent<Text>().text = totalTradeAmount != 0 ? tradeFairness : string.Empty;

			tradeMenu.transform.Find("ConfirmTrade-Button").GetComponent<Button>().interactable = tradeValueDifference <= 0 && totalTradeAmount > 0;

			foreach (ConfirmedTradeResourceElement confirmedTradeResourceElement in confirmedTradeResourceElements) {
				confirmedTradeResourceElement.Update();
			}
		}
	}

	public void ConfirmTrade() {
		CaravanManager.Caravan caravan = GameManager.caravanM.selectedCaravan;

		if (caravan != null) {
			caravan.ConfirmTrade();
			SetTradeMenu();

			foreach (TradeResourceElement tradeResourceElement in tradeResourceElements) {
				tradeResourceElement.SetTradeAmount(0);
			}
		}
	}

	public class ObjectPrefabElement {
		public ResourceManager.ObjectPrefab prefab;
		public GameObject obj;
		public GameObject objectInstancesList;
		public List<ObjectInstanceElement> instanceElements = new List<ObjectInstanceElement>();

		public ObjectPrefabElement(ResourceManager.ObjectPrefab prefab, Transform parent) {
			this.prefab = prefab;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectPrefab-Button"), parent, false);

			//obj.transform.Find("ObjectPrefabSprite-Panel/ObjectPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite; // TODO Fix
			obj.transform.Find("ObjectPrefabName-Text").GetComponent<Text>().text = prefab.name;

			objectInstancesList = obj.transform.Find("ObjectInstancesList-ScrollPanel").gameObject;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				objectInstancesList.SetActive(!objectInstancesList.activeSelf);
				if (objectInstancesList.activeSelf) {
					objectInstancesList.transform.SetParent(GameManager.uiM.canvas.transform);
					foreach (ObjectPrefabElement objectPrefabElement in GameManager.uiM.objectPrefabElements) {
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
			MonoBehaviour.Destroy(obj);
		}

		public void RemoveObjectInstances() {
			foreach (ObjectInstanceElement instance in instanceElements) {
				MonoBehaviour.Destroy(instance.obj);
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

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectInstance-Button"), parent, false);

			obj.transform.Find("ObjectInstanceSprite-Panel/ObjectInstanceSprite-Image").GetComponent<Image>().sprite = instance.obj.GetComponent<SpriteRenderer>().sprite;
			obj.transform.Find("ObjectInstanceName-Text").GetComponent<Text>().text = instance.prefab.name;
			obj.transform.Find("TilePosition-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(instance.tile.obj.transform.position.x) + ", " + Mathf.FloorToInt(instance.tile.obj.transform.position.y) + ")"; ;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				GameManager.cameraM.SetCameraPosition(instance.obj.transform.position);
			});

			ResourceManager.Container container = GameManager.resourceM.containers.Find(findContainer => findContainer == instance);
			if (container != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.SetSelectedContainer(container);
				});
			}
			ResourceManager.TradingPost tradingPost = GameManager.resourceM.tradingPosts.Find(tp => tp == instance);
			if (tradingPost != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.SetSelectedTradingPost(tradingPost);
				});
			}
			ResourceManager.CraftingObject mto = GameManager.resourceM.craftingObjectInstances.Find(findMTO => findMTO == instance);
			if (mto != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					GameManager.uiM.SetSelectedCraftingObject(mto);
				});
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

	public enum ChangeTypeEnum { Add, Update, Remove };

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

		public InputField desiredAmountInput;
		public Text desiredAmountText;

		public bool filterActive = true; // True if the resources filter deems this element visible
		public bool amountActive; // True if the total world amount is > 0

		public ResourceElement(ResourceManager.Resource resource, Transform parent) {
			this.resource = resource;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceListResourceElement-Panel"), parent, false);

			obj.transform.Find("Name").GetComponent<Text>().text = resource.name;

			obj.transform.Find("Image").GetComponent<Image>().sprite = resource.image;

			desiredAmountInput = obj.transform.Find("DesiredAmount-Input").GetComponent<InputField>();
			desiredAmountText = desiredAmountInput.transform.Find("Text").GetComponent<Text>();
			desiredAmountInput.onEndEdit.AddListener(delegate {
				if (int.TryParse(desiredAmountInput.text, out int newDesiredAmount)) {
					if (newDesiredAmount >= 0) {
						resource.SetDesiredAmount(newDesiredAmount);
						if (newDesiredAmount == 0) {
							desiredAmountInput.text = String.Empty;
						}
					}
				} else {
					resource.SetDesiredAmount(0);
				}
			});

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
			if (resource.GetDesiredAmount() > 0) {
				if (resource.GetDesiredAmount() > resource.GetAvailableAmount()) {
					desiredAmountText.color = GetColour(Colours.LightRed);
				} else if (resource.GetDesiredAmount() <= resource.GetAvailableAmount()) {
					desiredAmountText.color = GetColour(Colours.LightGreen);
				}
			}

			availableAmountPrev = resource.GetAvailableAmount();

			if (availableAmountPrev != resource.GetAvailableAmount()) {
				if (resource.GetAvailableAmount() > 0) {
					obj.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
					obj.transform.Find("DesiredAmount-Input").GetComponent<Image>().color = GetColour(Colours.LightGrey220);
				} else {
					obj.GetComponent<Image>().color = GetColour(Colours.Grey150);
					obj.transform.Find("DesiredAmount-Input").GetComponent<Image>().color = GetColour(Colours.Grey150);
				}
			}
		}
	}

	public List<ResourceElement> clothingElements = new List<ResourceElement>();

	public void CreateClothesList() {
		foreach (ResourceElement clothingElement in clothingElements) {
			MonoBehaviour.Destroy(clothingElement.obj);
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
			MonoBehaviour.Destroy(resourceElement.obj);
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
		selectedCraftingObjectIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, Vector2.zero, Quaternion.identity);
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
				MonoBehaviour.Destroy(child.gameObject);
			}
			Transform craftableResourcesPanel = settingsPanel.transform.Find("CraftableResources-Panel/CraftableResources-ScrollPanel/CraftableResourcesList-Panel");
			foreach (Transform child in craftableResourcesPanel) {
				MonoBehaviour.Destroy(child.gameObject);
			}
			foreach (ResourceManager.CraftableResourceInstance resource in selectedCraftingObject.resources.OrderBy(r => r.priority.Get())) {
				GameObject selectedResourcePreview = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/SelectedResourcePreview-Image"), selectedResourcesPanel, false);
				selectedResourcePreview.GetComponent<Image>().sprite = resource.resource.image;

				CraftingResourceElement craftableResourceElement = new CraftingResourceElement(resource, craftableResourcesPanel);
				craftingResourceElements.Add(craftableResourceElement);
			}

			Transform selectedFuelsPanel = settingsPanel.transform.Find("SelectFuels-Button/SelectedFuels-Panel");
			foreach (Transform child in selectedFuelsPanel) {
				MonoBehaviour.Destroy(child.gameObject);
			}
			foreach (ResourceManager.PriorityResourceInstance fuel in selectedCraftingObject.fuels.OrderBy(f => f.priority.Get())) {
				GameObject selectedFuelPreview = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/SelectedResourcePreview-Image"), selectedFuelsPanel, false);
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
				selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = GetColour(Colours.LightGreen);
			} else {
				selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = GetColour(Colours.LightOrange);
			}
		} else {
			selectedCraftingObjectPanelActiveButtonText.text = "Disabled";
			if (selectedCraftingObject.resources.Any(resource => resource.enableable)) {
				selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = GetColour(Colours.LightOrange);
			} else {
				selectedCraftingObjectPanelActiveButton.GetComponent<Image>().color = GetColour(Colours.LightRed);
			}
		}
	}

	private abstract class PriorityResourceElement {

		public enum Type {
			Resource,
			Fuel
		}

		public Type type;

		public ResourceManager.Resource resource;
		public ResourceManager.CraftingObject craftingObject;

		public GameObject panel;

		public PriorityResourceElement(
			Type type,
			ResourceManager.Resource resource,
			Transform parent,
			ResourceManager.CraftingObject craftingObject
		) {
			this.type = type;
			this.resource = resource;
			this.craftingObject = craftingObject;

			panel = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/SelectPriorityResource-Panel"), parent, false);

			panel.transform.Find("ResourceImage-Image").GetComponent<Image>().sprite = resource.image;
			panel.transform.Find("ResourceName-Text").GetComponent<Text>().text = resource.name;

			panel.transform.Find("Details-Panel/EnergyValue-Text").GetComponent<Text>().text = (type == Type.Resource ? resource.craftingEnergy : resource.fuelEnergy) + " energy";

			panel.GetComponent<Button>().onClick.AddListener(delegate {
				if (type == Type.Resource) {
					craftingObject.ToggleResource(resource, 1);
				} else if (type == Type.Fuel) {
					craftingObject.ToggleFuel(resource, 1);
				}
				
				Update();

				if (type == Type.Resource) {
					UpdatePriorityButtonText(craftingObject.GetCraftableResourceFromResource(resource) == null ? 0 : 1);
				} else if (type == Type.Fuel) {
					UpdatePriorityButtonText(craftingObject.GetFuelFromFuelResource(resource) == null ? 0 : 1);
				}
			});

			UpdatePriorityButtonText(0);
			if (type == Type.Resource) {
				ResourceManager.CraftableResourceInstance craftableResource = craftingObject.GetCraftableResourceFromResource(resource);
				if (craftableResource != null) {
					UpdatePriorityButtonText(craftableResource.priority.Get());
				}
			} else if (type == Type.Fuel) {
				ResourceManager.PriorityResourceInstance fuel = craftingObject.GetFuelFromFuelResource(resource);
				if (fuel != null) {
					UpdatePriorityButtonText(fuel.priority.Get());
				}
			}

			panel.transform.Find("Priority-Button").GetComponent<HandleClickScript>().Initialize(
				delegate {
					ChangePriority(1);
				},
				null,
				delegate {
					ChangePriority(-1);
				}
			);

			Update();
		}

		public virtual void Update() {
			panel.transform.Find("Details-Panel/AvailableAmount-Text").GetComponent<Text>().text = resource.GetAvailableAmount() + " available";

			panel.transform.Find("Details-Panel/AvailableAmount-Text").GetComponent<Text>().color =
				resource.GetAvailableAmount() > 0
					? GetColour(Colours.DarkGreen)
					: GetColour(Colours.DarkRed);
		}

		public void UpdatePriorityButtonText(int priority) {
			panel.transform.Find("Priority-Button/Text").GetComponent<Text>().text = priority > 0 ? priority.ToString() : string.Empty;

			panel.transform.Find("Priority-Button/Text").GetComponent<Text>().color = Color.Lerp(
				GetColour(Colours.DarkGreen),
				GetColour(Colours.DarkRed),
				(priority - 1f) / (ResourceManager.PriorityResourceInstance.priorityMax - 1f)
			);
		}

		public abstract void ChangePriority(int amount);

		public void Remove() {
			MonoBehaviour.Destroy(panel);
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
				craftableResourceElements.Add(
					new CraftableResourceElement(
						PriorityResourceElement.Type.Resource,
						resource,
						selectedCraftingObjectSelectResourcesPanel.transform.Find("SelectResources-ScrollPanel/SelectResourcesList-Panel"),
						selectedCraftingObject
					)
				);
			}
		} else {
			SetSelectedCraftingObjectPanel();
		}
	}

	private class CraftableResourceElement : PriorityResourceElement {

		public CraftableResourceElement(
			Type type,
			ResourceManager.Resource resource,
			Transform parent,
			ResourceManager.CraftingObject craftingObject
		) : base(
			type,
			resource,
			parent,
			craftingObject
		) {
			
		}

		public override void Update() {
			base.Update();

			panel.transform.Find("ActiveIndicator-Panel").GetComponent<Image>().color =
				craftingObject.GetCraftableResourceFromResource(resource) != null
					? (resource.GetAvailableAmount() > 0
						? GetColour(Colours.LightGreen)
						: GetColour(Colours.LightOrange))
					: GetColour(Colours.Clear);
		}

		public override void ChangePriority(int amount) {

			ResourceManager.CraftableResourceInstance craftableResource = craftingObject.GetCraftableResourceFromResource(resource);
			if (craftableResource == null) {
				craftableResource = craftingObject.ToggleResource(resource, 0);
			}

			int priority = craftableResource.priority.Change(amount);

			if (priority <= craftableResource.priority.min && craftableResource != null) {
				craftingObject.ToggleResource(resource, 0);
			}

			UpdatePriorityButtonText(priority);

			Update();
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
				fuelResourceElements.Add(
					new FuelResourceElement(
						PriorityResourceElement.Type.Fuel,
						resource,
						selectedCraftingObjectSelectFuelsPanel.transform.Find("SelectFuels-ScrollPanel/SelectFuelsList-Panel"),
						selectedCraftingObject
					)
				);
			}
		} else {
			SetSelectedCraftingObjectPanel();
		}
	}

	private class FuelResourceElement : PriorityResourceElement {

		public FuelResourceElement(
			Type type,
			ResourceManager.Resource resource,
			Transform parent,
			ResourceManager.CraftingObject craftingObject
		) : base(
			type,
			resource,
			parent,
			craftingObject
		) {
			
		}

		public override void Update() {
			base.Update();

			panel.transform.Find("ActiveIndicator-Panel").GetComponent<Image>().color = 
				craftingObject.GetFuelFromFuelResource(resource) != null
					? (resource.GetAvailableAmount() > 0
						? GetColour(Colours.LightGreen)
						: GetColour(Colours.LightOrange))
					: GetColour(Colours.Clear);
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

			panel = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/Prefabs/SelectedCraftingObject/CraftableResource-Panel"), parent, false);

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

			singleRunButton.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
			maintainStockButton.GetComponent<Image>().color = GetColour(Colours.LightGrey220);
			continuousRunButton.GetComponent<Image>().color = GetColour(Colours.LightGrey220);

			createAmountPanel.gameObject.SetActive(true);

			switch (resource.creationMethod) {
				case ResourceManager.CreationMethod.SingleRun:
					singleRunButton.GetComponent<Image>().color = GetColour(Colours.DarkGrey50);
					createAmountPanel.transform.Find("CreateAmount-Text").GetComponent<Text>().text = "Create Amount";
					break;
				case ResourceManager.CreationMethod.MaintainStock:
					maintainStockButton.GetComponent<Image>().color = GetColour(Colours.DarkGrey50);
					createAmountPanel.transform.Find("CreateAmount-Text").GetComponent<Text>().text = "Maintain Amount";
					break;
				case ResourceManager.CreationMethod.ContinuousRun:
					continuousRunButton.GetComponent<Image>().color = GetColour(Colours.DarkGrey50);
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

		pauseMenu.SetActive(active);

		pauseSaveButton.GetComponent<Image>().color = GetColour(Colours.LightGrey200);

		GameManager.timeM.SetPaused(pauseMenu.activeSelf);
	}

	public void TogglePauseMenuButtons(bool state) {
		pauseMenuButtons.SetActive(state);
		pauseLabel.SetActive(pauseMenuButtons.activeSelf);
	}

	public enum UIScaleMode {
		ConstantPixelSize,
		ScaleWithScreenSize
	};

	public void SetSettingsMenuActive(bool active) {
		settingsPanel.SetActive(active);

		ToggleMainMenuButtons(settingsPanel);
		TogglePauseMenuButtons(!settingsPanel.activeSelf);
		if (settingsPanel.activeSelf) {
			GameObject resolutionSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/ResolutionSettings-Panel").gameObject;
			Slider resolutionSlider = resolutionSettingsPanel.transform.Find("Resolution-Slider").GetComponent<Slider>();
			resolutionSlider.minValue = 0;
			resolutionSlider.maxValue = Screen.resolutions.Length - 1;
			resolutionSlider.onValueChanged.AddListener(delegate {
				Resolution r = Screen.resolutions[Mathf.RoundToInt(resolutionSlider.value)];
				GameManager.persistenceM.settingsState.resolution = r;
				GameManager.persistenceM.settingsState.resolutionWidth = r.width;
				GameManager.persistenceM.settingsState.resolutionHeight = r.height;
				GameManager.persistenceM.settingsState.refreshRate = r.refreshRate;
				resolutionSettingsPanel.transform.Find("ResolutionValue-Text").GetComponent<Text>().text = GameManager.persistenceM.settingsState.resolutionWidth + " × " + GameManager.persistenceM.settingsState.resolutionHeight + " @ " + r.refreshRate + "hz";
			});
			resolutionSlider.value = Screen.resolutions.ToList().IndexOf(GameManager.persistenceM.settingsState.resolution);

			GameObject fullscreenSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/FullscreenSettings-Panel").gameObject;
			Toggle fullscreenToggle = fullscreenSettingsPanel.transform.Find("Fullscreen-Toggle").GetComponent<Toggle>();
			fullscreenToggle.onValueChanged.AddListener(delegate { GameManager.persistenceM.settingsState.fullscreen = fullscreenToggle.isOn; });
			fullscreenToggle.isOn = GameManager.persistenceM.settingsState.fullscreen;

			GameObject UIScaleModeSettingsPanel = settingsPanel.transform.Find("SettingsList-ScrollPanel/SettingsList-Panel/UIScaleModeSettings-Panel").gameObject;
			Toggle UIScaleModeToggle = UIScaleModeSettingsPanel.transform.Find("UIScaleMode-Toggle").GetComponent<Toggle>();
			UIScaleModeToggle.onValueChanged.AddListener(delegate {
				if (UIScaleModeToggle.isOn) {
					GameManager.persistenceM.settingsState.scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				} else {
					GameManager.persistenceM.settingsState.scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
				}
			});
			if (GameManager.persistenceM.settingsState.scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize) {
				UIScaleModeToggle.isOn = true;
			} else if (GameManager.persistenceM.settingsState.scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) {
				UIScaleModeToggle.isOn = false;
			}
		}
	}

	public Resolution GetResolutionByDimensions(int width, int height, int refreshRate) {
		List<Resolution> resolutions = Screen.resolutions.ToList();
		return resolutions.Find(resolution => resolution.width == width && resolution.height == height && resolution.refreshRate == refreshRate);
	}

	public void ExitToMenu() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public bool IsPointerOverUI() {
		return EventSystem.current.IsPointerOverGameObject();
	}

	public List<RaycastResult> GetUIElementsUnderPointer() {
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current) {
			pointerId = -1,
		};

		pointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, results);

		return results;
	}
}