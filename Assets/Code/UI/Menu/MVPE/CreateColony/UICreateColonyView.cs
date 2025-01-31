using System;
using Snowship.NPlanet;
using Snowship.NUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UICreateColonyView : UIView {

		[Header("General")]
		[SerializeField] private Button backButton;

		[SerializeField] private GridLayoutGroup planetViewGridLayoutGroup;
		public GridLayoutGroup PlanetViewGridLayoutGroup => planetViewGridLayoutGroup;
		[SerializeField] private GameObject planetTilePrefab;
		public GameObject PlanetTilePrefab => planetTilePrefab;

		[Header("Colony Properties")]
		[SerializeField] private TMP_InputField colonyNameInputField;
		[SerializeField] private Button randomizeColonyNameButton;
		[SerializeField] private TMP_InputField mapSeedInputField;
		[SerializeField] private Slider mapSizeSlider;
		[SerializeField] private TMP_Text mapSizeText;

		[Header("Selected Planet Tile Info")]
		[SerializeField] private GameObject selectedPlanetTileInfoPanel;
		[SerializeField] private Image selectedPlanetTileSpriteImage;
		[SerializeField] private TMP_Text biomeText;
		[SerializeField] private TMP_Text averageTemperatureText;
		[SerializeField] private TMP_Text averagePrecipitationText;
		[SerializeField] private TMP_Text altitudeText;
		[SerializeField] private TMP_Text positionText;

		[SerializeField] private Button createColonyButton;
		[SerializeField] private Image createColonyButtonImage;

		public event Action OnBackButtonClicked;

		public event Action<string> OnColonyNameChanged;
		public event Action OnRandomizeColonyNameButtonClicked;
		public event Action<float> OnMapSizeSliderChanged;
		public event Action<string> OnMapSeedChanged;

		public event Action OnCreateColonyButtonClicked;

		public override void OnOpen() {
			backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());

			colonyNameInputField.onValueChanged.AddListener(colonyName => OnColonyNameChanged?.Invoke(colonyName));
			randomizeColonyNameButton.onClick.AddListener(() => OnRandomizeColonyNameButtonClicked?.Invoke());
			mapSeedInputField.onValueChanged.AddListener(mapSeed => OnMapSeedChanged?.Invoke(mapSeed));
			mapSizeSlider.onValueChanged.AddListener(sliderValue => OnMapSizeSliderChanged?.Invoke(sliderValue));

			createColonyButton.onClick.AddListener(() => OnCreateColonyButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}

		public void SetColonyNameInputField(string text) {
			colonyNameInputField.SetTextWithoutNotify(text);
		}

		public void SetMapSeedInputField(string text) {
			mapSeedInputField.SetTextWithoutNotify(text);
		}

		public void SetupMapSizeSlider(int minValue, int maxValue, int initialValue) {
			mapSizeSlider.minValue = minValue;
			mapSizeSlider.maxValue = maxValue;
			mapSizeSlider.value = initialValue;
		}

		public void SetMapSizeText(string text) {
			mapSizeText.text = text;
		}

		public void SetPlanetTileData(PlanetTile planetTile, bool planetTileValid) {

			selectedPlanetTileInfoPanel.SetActive(planetTileValid);
			positionText.gameObject.SetActive(planetTileValid);

			if (!planetTileValid) {
				return;
			}

			selectedPlanetTileSpriteImage.sprite = planetTile.sprite;
			biomeText.text = planetTile.tile.biome.name;
			averageTemperatureText.text = $"{Mathf.RoundToInt(planetTile.tile.temperature)}°C";
			averagePrecipitationText.text = $"{Mathf.RoundToInt(planetTile.tile.GetPrecipitation() * 100f)}%";
			altitudeText.text = planetTile.altitude;
			positionText.text = $"({Mathf.FloorToInt(planetTile.tile.position.x)}, {Mathf.FloorToInt(planetTile.tile.position.y)})";
		}

		public void SetSaveColonyButtonInteractable(bool interactable) {
			createColonyButton.interactable = interactable;
			createColonyButtonImage.color = interactable
				? ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220)
				: ColourUtilities.GetColour(ColourUtilities.EColour.Grey120);
		}


	}
}