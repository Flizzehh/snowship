using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NPersistence;
using Snowship.NUI.Generic;
using Snowship.NUI.Menu.CreatePlanet;
using Snowship.NUI.Menu.LoadColony;
using Snowship.NUI.Menu.Settings;
using UnityEngine;

namespace Snowship.NUI.Menu.MainMenu {

	[UsedImplicitly]
	public class UIMainMenuPresenter : UIPresenter<UIMainMenuView> {

		private readonly PLastSave pLastSave = new PLastSave();

		public UIMainMenuPresenter(UIMainMenuView view) : base(view) {
		}

		public override void OnCreate() {

			View.OnNewButtonClicked += OnNewButtonClicked;
			View.OnContinueButtonClicked += OnContinueButtonClicked;
			View.OnLoadButtonClicked += OnLoadButtonClicked;
			View.OnSettingsButtonClicked += OnSettingsButtonClicked;
			View.OnExitButtonClicked += OnExitButtonClicked;

			View.SetDisclaimerText($"Lumi Games (Snowship {PersistenceManager.GameVersion.text})");

			PLastSave.LastSaveProperties lastSaveProperties = pLastSave.GetLastSaveProperties();
			if (lastSaveProperties != null) {
				View.SetupContinueButton(
					pLastSave.IsLastSaveUniverseLoadable(),
					pLastSave.LoadSaveImageFromSaveDirectoryPath(lastSaveProperties.lastSaveSavePath)
				);
			} else {
				View.DisableContinueButton();
			}

			List<Sprite> backgroundImages = Resources.LoadAll<Sprite>(@"UI/Backgrounds/SingleMap").ToList();
			View.SetBackground(backgroundImages[Random.Range(0, backgroundImages.Count)]);
		}

		public override void OnClose() {
			View.OnNewButtonClicked -= OnNewButtonClicked;
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnLoadButtonClicked -= OnLoadButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitButtonClicked -= OnExitButtonClicked;
		}

		private void OnNewButtonClicked() {
			GameManager.uiM.OpenViewAsync<UICreatePlanet>(this, false).Forget();
		}

		private void OnContinueButtonClicked() {
			GameManager.persistenceM.ContinueFromMostRecentSave().Forget();
		}

		private void OnLoadButtonClicked() {
			GameManager.uiM.OpenViewAsync<UILoadColony>(this, false).Forget();
		}

		private void OnSettingsButtonClicked() {
			GameManager.uiM.OpenViewAsync<UISettings>(this, false).Forget();
		}

		private void OnExitButtonClicked() {
			Application.Quit();
		}
	}
}
