using JetBrains.Annotations;
using Snowship.NColony;
using Snowship.NPlanet;
using Snowship.NUI.Generic;
using Snowship.NUI.Modules;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI.Menu.CreateColony {

	[UsedImplicitly]
	public class UICreateColonyPresenter : UIPresenter<UICreateColonyView> {

		private PlanetViewModule planetViewModule;
		private CreateColonyData createColonyData;

		public UICreateColonyPresenter(UICreateColonyView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;

			View.OnColonyNameChanged += OnColonyNameChanged;
			View.OnRandomizeColonyNameButtonClicked += OnRandomizeColonyNameButtonClicked;
			View.OnMapSizeSliderChanged += OnMapSizeSliderChanged;
			View.OnMapSeedChanged += OnMapSeedChanged;

			View.OnCreateColonyButtonClicked += OnCreateColonyButtonClicked;

			SetColonyName();
			SetMapSeed();
			SetMapSizeSlider();
			SetCreateColonyButtonInteractable(CanCreateColony());

			CreatePlanetViewModule();
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

		private void SetColonyName() {
			OnColonyNameChanged(ColonyManager.GetRandomColonyName());
			View.SetColonyNameInputField(createColonyData.name);
		}

		private void SetMapSeed() {
			OnMapSeedChanged($"{TileManager.Map.GetRandomMapSeed()}");
			View.SetMapSeedInputField($"{createColonyData.seed}");
		}

		private void SetMapSizeSlider() {
			View.SetupMapSizeSlider(0, ColonyManager.GetNumMapSizes() - 1, 1);
			OnMapSizeSliderChanged(1);
		}

		private bool IsColonyNameValid(string colonyName) {
			return StringUtilities.IsAlphanumericWithSpaces(colonyName);
		}

		private bool IsPlanetTileValid(PlanetTile planetTile) {
			return planetTile != null;
		}

		private bool CanCreateColony() {
			return IsColonyNameValid(createColonyData.name) && IsPlanetTileValid(createColonyData.planetTile);
		}

		private void SetCreateColonyButtonInteractable(bool interactable) {
			View.SetSaveColonyButtonInteractable(interactable);
		}

		private void OnBackButtonClicked() {
			GameManager.uiM.CloseView(this);
		}

		private void OnColonyNameChanged(string colonyName) {
			bool colonyNameValid = IsColonyNameValid(colonyName);
			createColonyData.name = colonyNameValid ? colonyName : string.Empty;
			SetCreateColonyButtonInteractable(CanCreateColony());
		}

		private void OnRandomizeColonyNameButtonClicked() {
			View.SetColonyNameInputField(ColonyManager.GetRandomColonyName());
		}

		private void OnMapSizeSliderChanged(float sliderValue) {
			createColonyData.size = ColonyManager.GetMapSizeByIndex(Mathf.FloorToInt(sliderValue));
			View.SetMapSizeText($"{createColonyData.size}");
		}

		private void OnMapSeedChanged(string mapSeed) {
			createColonyData.seed = StringUtilities.ParseSeed(mapSeed);
		}

		private void OnCreateColonyButtonClicked() {
			Colony colony = GameManager.colonyM.CreateColony(createColonyData);

			// TODO This should all be handled in the ColonyManager itself
			GameManager.colonyM.SetupNewColony(colony, false);
		}

		private void OnPlanetTileClicked(PlanetTile planetTile) {
			createColonyData.planetTile = planetTile;

			SetCreateColonyButtonInteractable(CanCreateColony());

			bool planetTileValid = IsPlanetTileValid(planetTile);
			View.SetPlanetTileState(planetTileValid);
			if (planetTileValid) {
				View.SetPlanetTileData(planetTile);
			}
		}

		private void OnColonyTileClicked(PersistenceManager.PersistenceColony persistenceColony) {
			// TODO Popup asking to load colony?
		}
	}
}
