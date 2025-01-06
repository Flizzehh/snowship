using System;
using Snowship.NPlanet;
using Snowship.NUI.Generic;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.CreateColony {
	public class UICreateColonyView : UIView {

		[SerializeField] private Button backButton;

		[SerializeField] private GridLayoutGroup planetViewGridLayoutGroup;

		[SerializeField] private InputField colonyNameInputField;
		[SerializeField] private Button randomizeColonyNameButton;
		[SerializeField] private InputField mapSeedInputField;
		[SerializeField] private Slider mapSizeSlider;
		[SerializeField] private Text mapSizeText;

		[SerializeField] private GameObject selectedPlanetTileInfoPanel;
		[SerializeField] private Image selectedPlanetTileSpriteImage;
		[SerializeField] private Text biomeText;
		[SerializeField] private Text averageTemperatureText;
		[SerializeField] private Text averagePrecipitationText;
		[SerializeField] private Text altitudeText;
		[SerializeField] private Text positionText;

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

		public GridLayoutGroup GetPlanetViewGridLayoutGroup() {
			return planetViewGridLayoutGroup;
		}

		public void SetMapSeedInputField(string text) {
			mapSeedInputField.text = text;
		}

		public void SetupMapSizeSlider(int minValue, int maxValue, int initialValue) {
			mapSizeSlider.minValue = minValue;
			mapSizeSlider.maxValue = maxValue;
			mapSizeSlider.value = initialValue;
		}

		public void SetColonyNameInputField(string text) {
			colonyNameInputField.text = text;
		}

		public void SetMapSizeText(string text) {
			mapSizeText.text = text;
		}

		public void SetPlanetTileState(bool planetTileValid) {
			selectedPlanetTileInfoPanel.SetActive(planetTileValid);
			positionText.gameObject.SetActive(planetTileValid);
		}

		public void SetPlanetTileData(PlanetTile planetTile) {
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
