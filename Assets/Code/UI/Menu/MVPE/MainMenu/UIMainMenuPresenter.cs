using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NUI.Generic;
using Snowship.NUI.Menu.CreatePlanet;
using Snowship.NUI.Menu.LoadColony;
using Snowship.NUI.Menu.Settings;
using UnityEngine;

namespace Snowship.NUI.Menu.MainMenu {

	[UsedImplicitly]
	public class UIMainMenuPresenter : UIPresenter<UIMainMenuView> {

		public UIMainMenuPresenter(UIMainMenuView view) : base(view) {
		}

		public override void OnCreate() {

			View.OnNewButtonClicked += OnNewButtonClicked;
			View.OnContinueButtonClicked += OnContinueButtonClicked;
			View.OnLoadButtonClicked += OnLoadButtonClicked;
			View.OnSettingsButtonClicked += OnSettingsButtonClicked;
			View.OnExitButtonClicked += OnExitButtonClicked;

			View.SetDisclaimerText($"Snowship by Ryan White - rywh.itch.io/snowship\n<size=20>{PersistenceManager.gameVersion.text}</size>");

			PersistenceManager.LastSaveProperties lastSaveProperties = GameManager.persistenceM.GetLastSaveProperties();
			if (lastSaveProperties != null) {
				View.SetupContinueButton(
					GameManager.persistenceM.IsLastSaveUniverseLoadable(),
					GameManager.persistenceM.LoadSaveImageFromSaveDirectoryPath(lastSaveProperties.lastSaveSavePath)
				);
			} else {
				View.DisableContinueButton();
			}

			List<Sprite> backgroundImages = Resources.LoadAll<Sprite>(@"UI/Backgrounds/SingleMap").ToList();
			View.SetBackground(backgroundImages[Random.Range(0, backgroundImages.Count)]);

			View.Initialize(Screen.currentResolution);
		}

		public override void OnClose() {
			View.OnNewButtonClicked -= OnNewButtonClicked;
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnLoadButtonClicked -= OnLoadButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitButtonClicked -= OnExitButtonClicked;
		}

		private void OnNewButtonClicked() {
			GameManager.uiM.OpenViewAsync<UICreatePlanet>(this);
		}

		private void OnContinueButtonClicked() {
			GameManager.persistenceM.ContinueFromMostRecentSave();
		}

		private void OnLoadButtonClicked() {
			GameManager.uiM.OpenViewAsync<UILoadColony>(this);
		}

		private void OnSettingsButtonClicked() {
			GameManager.uiM.OpenViewAsync<UISettings>(this);
		}

		private void OnExitButtonClicked() {
			Application.Quit();
		}
	}
}
