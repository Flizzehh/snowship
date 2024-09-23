using JetBrains.Annotations;
using Snowship.NPlanet;
using Snowship.NUI.Generic;
using Snowship.NUI.Modules;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI.Menu.CreatePlanet {

	[UsedImplicitly]
	public class UICreatePlanetPresenter : UIPresenter<UICreatePlanetView> {

		private PlanetViewModule planetViewModule;
		private CreatePlanetData createPlanetData;

		public UICreatePlanetPresenter(UICreatePlanetView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;

			View.OnPlanetNameChanged += OnPlanetNameChanged;
			View.OnPlanetSeedChanged += OnPlanetSeedChanged;

			View.OnPlanetSizeSliderChanged += OnPlanetSizeSliderChanged;
			View.OnPlanetDistanceSliderChanged += OnPlanetDistanceSliderChanged;
			View.OnTemperatureRangeSliderChanged += OnTemperatureRangeSliderChanged;
			View.OnRandomOffsetsValueChanged += OnRandomOffsetsValueChanged;
			View.OnWindDirectionSliderChanged += OnWindDirectionSliderChanged;

			View.OnRefreshPlanetButtonClicked += OnRefreshPlanetButtonClicked;
			View.OnRandomizePlanetButtonClicked += OnRandomizePlanetButtonClicked;
			View.OnCreatePlanetButtonClicked += OnCreatePlanetButtonClicked;

			SetPlanetNameInputField();
			SetPlanetSeedInputField();
			SetPlanetSizeSlider();
			SetPlanetDistanceSlider();
			SetTemperatureRangeSlider();
			SetRandomOffsetsToggle();
			SetWindDirectionSlider();

			CreatePlanetViewModule();
		}

		public override void OnClose() {

			View.OnBackButtonClicked -= OnBackButtonClicked;

			View.OnPlanetNameChanged -= OnPlanetNameChanged;
			View.OnPlanetSeedChanged -= OnPlanetSeedChanged;

			View.OnPlanetSizeSliderChanged -= OnPlanetSizeSliderChanged;
			View.OnPlanetDistanceSliderChanged -= OnPlanetDistanceSliderChanged;
			View.OnTemperatureRangeSliderChanged -= OnTemperatureRangeSliderChanged;
			View.OnRandomOffsetsValueChanged -= OnRandomOffsetsValueChanged;
			View.OnWindDirectionSliderChanged -= OnWindDirectionSliderChanged;

			View.OnRefreshPlanetButtonClicked -= OnRefreshPlanetButtonClicked;
			View.OnRandomizePlanetButtonClicked -= OnRandomizePlanetButtonClicked;
			View.OnCreatePlanetButtonClicked -= OnCreatePlanetButtonClicked;

			ClosePlanetViewModule();
		}

		private void CreatePlanetViewModule() {
			planetViewModule = new PlanetViewModule(View.GetPlanetViewGridLayoutGroup());
			planetViewModule.DisplayPlanet(
				GameManager.planetM.planet,
				GameManager.persistenceM.GetPersistenceColonies(),
				true
			);

			planetViewModule.OnPlanetTileClicked += OnPlanetTileClicked;
			planetViewModule.OnColonyTileClicked += OnColonyTileClicked;
		}

		private void ClosePlanetViewModule() {
			planetViewModule.OnPlanetTileClicked -= OnPlanetTileClicked;
			planetViewModule.OnColonyTileClicked -= OnColonyTileClicked;

			planetViewModule.DestroyPlanet();
		}

		private void SetPlanetNameInputField() {
			View.SetPlanetNameInputField($"{GameManager.planetM.GetRandomPlanetName()}");
		}

		private void SetPlanetSeedInputField() {
			View.SetPlanetSeedInputField($"{GameManager.planetM.GetRandomPlanetSeed()}");
		}

		private void SetPlanetSizeSlider() {
			const int planetSizeSliderMin = 0;
			int planetSizeSliderMax = GameManager.planetM.GetNumPlanetSizes() - 1;
			View.SetPlanetSizeSlider(
				planetSizeSliderMin,
				planetSizeSliderMax,
				Mathf.FloorToInt((planetSizeSliderMin + planetSizeSliderMax) / 2f) + 2 // 60
			);
		}

		private void SetPlanetDistanceSlider() {
			int planetDistanceSliderMin = GameManager.planetM.GetMinPlanetDistance();
			int planetDistanceSliderMax = GameManager.planetM.GetMaxPlanetDistance();
			View.SetPlanetDistanceSlider(
				planetDistanceSliderMin,
				planetDistanceSliderMax,
				Mathf.FloorToInt((planetDistanceSliderMin + planetDistanceSliderMax) / 2f) // 1 AU
			);
		}

		private void SetTemperatureRangeSlider() {
			const int temperatureRangeSliderMin = 0;
			const int temperatureRangeSliderMax = 10;
			View.SetTemperatureRangeSlider(
				temperatureRangeSliderMin,
				temperatureRangeSliderMax,
				Mathf.FloorToInt((temperatureRangeSliderMin + temperatureRangeSliderMax) / 2f) + 2 // 70°C
			);
		}

		private void SetRandomOffsetsToggle() {
			View.SetRandomOffsetsToggle(true);
		}

		private void SetWindDirectionSlider() {
			const int windDirectionSliderMin = 0;
			int windDirectionSliderMax = GameManager.planetM.GetNumWindDirections() - 1;
			View.SetWindDirectionSlider(
				windDirectionSliderMin,
				windDirectionSliderMax,
				Random.Range(windDirectionSliderMin, windDirectionSliderMax)
			);
		}

		private bool IsPlanetNameValid(string planetName) {
			return StringUtilities.IsAlphanumericWithSpaces(planetName);
		}

		private void SetCreatePlanetButtonInteractable(bool interactable) {
			View.SetCreatePlanetButtonInteractable(interactable);
		}

		private void OnBackButtonClicked() {
			GameManager.uiM.CloseView(this);
		}

		private void OnPlanetNameChanged(string planetName) {
			bool validPlanetName = IsPlanetNameValid(planetName);
			SetCreatePlanetButtonInteractable(validPlanetName);
			if (validPlanetName) {
				createPlanetData.name = planetName;
			}
		}

		private void OnPlanetSeedChanged(string planetSeed) {
			createPlanetData.seed = StringUtilities.ParseSeed(planetSeed);
		}

		private void OnPlanetSizeSliderChanged(float sliderValue) {
			createPlanetData.size = GameManager.planetM.GetPlanetSizeByIndex(Mathf.FloorToInt(sliderValue));
			View.SetPlanetSizeText($"{createPlanetData.size}");
		}

		private void OnPlanetDistanceSliderChanged(float sliderValue) {
			createPlanetData.distance = GameManager.planetM.GetPlanetDistanceByIndex(Mathf.FloorToInt(sliderValue));
			View.SetPlanetDistanceText($"{createPlanetData.distance} AU");
		}

		private void OnTemperatureRangeSliderChanged(float sliderValue) {
			createPlanetData.temperatureRange = GameManager.planetM.GetTemperatureRangeByIndex(Mathf.FloorToInt(sliderValue));
			View.SetTemperatureRangeText($"{createPlanetData.temperatureRange}°C");
		}

		private void OnRandomOffsetsValueChanged(bool toggleValue) {
			createPlanetData.randomOffsets = toggleValue;
		}

		private void OnWindDirectionSliderChanged(float sliderValue) {
			int sliderValueInt = Mathf.FloorToInt(sliderValue);
			createPlanetData.windDirection = GameManager.planetM.GetWindCircularDirectionByIndex(sliderValueInt);
			View.SetWindDirectionText($"{GameManager.planetM.GetWindCardinalDirectionByIndex(sliderValueInt)}");
		}

		private void OnRefreshPlanetButtonClicked() {
			CreatePlanetPreview();
		}

		private void OnRandomizePlanetButtonClicked() {
			RandomizePlanetSettings();
		}

		private void OnCreatePlanetButtonClicked() {
			GameManager.persistenceM.CreatePlanet(GameManager.planetM.planet);
		}

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			GameManager.planetM.SetSelectedPlanetTile(planetTile);
			View.SetPlanetTileData(planetTile);
		}

		private void OnColonyTileClicked(PersistenceManager.PersistenceColony persistenceColony) {
			Debug.LogError("Clicked loaded colony on new planet, this should not be possible!");
		}

		private void CreatePlanetPreview() {
			Planet planet = GameManager.planetM.CreatePlanet(createPlanetData);
			planetViewModule.DisplayPlanet(planet, null, false);
		}

		private void RandomizePlanetSettings() {
			View.SetPlanetSeedInputField($"{Random.Range(int.MinValue, int.MaxValue)}");
			CreatePlanetPreview();
		}
	}
}
