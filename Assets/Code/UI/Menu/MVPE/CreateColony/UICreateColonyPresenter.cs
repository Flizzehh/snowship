using JetBrains.Annotations;
using Snowship.NColony;
using Snowship.NPersistence;
using Snowship.NPlanet;
using Snowship.NState;
using Snowship.NUI.Generic;
using Snowship.NUI.Modules;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI.Menu.CreateColony {

	[UsedImplicitly]
	public class UICreateColonyPresenter : UIPresenter<UICreateColonyView> {

		private PlanetViewModule planetViewModule;
		private readonly CreateColonyData createColonyData = new CreateColonyData();

		public UICreateColonyPresenter(UICreateColonyView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;

			View.OnColonyNameChanged += OnColonyNameChanged;
			View.OnRandomizeColonyNameButtonClicked += OnRandomizeColonyNameButtonClicked;
			View.OnMapSizeSliderChanged += OnMapSizeSliderChanged;
			View.OnMapSeedChanged += OnMapSeedChanged;

			View.OnCreateColonyButtonClicked += OnCreateColonyButtonClicked;

			SetColonyNameInputField();
			SetMapSeedInputField();
			SetMapSizeSlider();
			SetCreateColonyButtonInteractable();

			CreatePlanetViewModule();
			CreatePlanetPreview();
			View.SetPlanetTileData(null, false);
		}

		public override void OnClose() {
			View.OnBackButtonClicked -= OnBackButtonClicked;

			View.OnColonyNameChanged -= OnColonyNameChanged;
			View.OnRandomizeColonyNameButtonClicked -= OnRandomizeColonyNameButtonClicked;
			View.OnMapSizeSliderChanged -= OnMapSizeSliderChanged;
			View.OnMapSeedChanged -= OnMapSeedChanged;

			View.OnCreateColonyButtonClicked -= OnCreateColonyButtonClicked;

			ClosePlanetViewModule();
		}

		private void CreatePlanetViewModule() {
			planetViewModule = new PlanetViewModule(View.PlanetViewGridLayoutGroup, View.PlanetTilePrefab);
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

		private void CreatePlanetPreview() {
			Planet planet = GameManager.planetM.planet;
			planetViewModule.DisplayPlanet(
				planet,
				GameManager.persistenceM.GetPersistenceColonies(),
				true
			);
		}

		private void SetColonyNameInputField() {
			View.SetColonyNameInputField($"{createColonyData.Name}");
		}

		private void SetMapSeedInputField() {
			View.SetMapSeedInputField($"{createColonyData.Seed}");
		}

		private void SetMapSizeSlider() {
			View.SetupMapSizeSlider(
				CreateColonyData.MapSizeIndexRange.Min,
				CreateColonyData.MapSizeIndexRange.Max,
				createColonyData.SizeIndex
			);
		}

		private void SetCreateColonyButtonInteractable() {
			View.SetSaveColonyButtonInteractable(createColonyData.CanCreateColony());
		}

		private void OnBackButtonClicked() {
			GameManager.uiM.GoBack(this);
		}

		private void OnColonyNameChanged(string colonyName) {
			bool validColonyName = createColonyData.SetName(colonyName);
			if (!validColonyName) {
				View.SetColonyNameInputField(string.Empty);
			}
			SetCreateColonyButtonInteractable();
		}

		private void OnRandomizeColonyNameButtonClicked() {
			View.SetColonyNameInputField(CreateColonyData.GenerateRandomColonyName());
		}

		private void OnMapSeedChanged(string mapSeed) {
			createColonyData.Seed = StringUtilities.ParseSeed(mapSeed);
		}

		private void OnMapSizeSliderChanged(float sliderValue) {
			createColonyData.SetSize(Mathf.FloorToInt(sliderValue));
			View.SetMapSizeText($"{createColonyData.Size}²");
		}

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			bool planetTileValid = createColonyData.SetPlanetTile(planetTile);
			SetCreateColonyButtonInteractable();
			View.SetPlanetTileData(planetTile, planetTileValid);
		}

		private void OnCreateColonyButtonClicked() {
			Colony colony = GameManager.colonyM.CreateColony(createColonyData);

			// TODO This should be handled in the ColonyManager itself
			GameManager.colonyM.SetupNewColony(colony, false);

			_ = GameManager.stateM.TransitionToState(EState.LoadToSimulation);
		}

		private void OnColonyTileClicked(PersistenceManager.PersistenceColony persistenceColony) {
			// TODO Popup asking to load colony?
		}
	}
}
