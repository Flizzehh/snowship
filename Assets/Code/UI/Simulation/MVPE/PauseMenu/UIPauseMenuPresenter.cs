using System;
using JetBrains.Annotations;
using Snowship.NUI.Generic;
using Snowship.NUI.Menu.Settings;
using Snowship.NUtilities;

namespace Snowship.NUI.Menu.PauseMenu {

	[UsedImplicitly]
	public class UIPauseMenuPresenter : UIPresenter<UIPauseMenuView> {

		public UIPauseMenuPresenter(UIPauseMenuView view) : base(view) {
		}

		public override void OnCreate() {

			View.OnContinueButtonClicked += OnContinueButtonClicked;
			View.OnSaveButtonClicked += OnSaveButtonClicked;
			View.OnSettingsButtonClicked += OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked += OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked += OnExitToDesktopButtonClicked;
		}

		public override void OnClose() {
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnSaveButtonClicked -= OnSaveButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked -= OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked -= OnExitToDesktopButtonClicked;
		}

		private void OnContinueButtonClicked() {
			GameManager.uiM.CloseView(this);
		}

		private void OnSaveButtonClicked() {
			try {
				GameManager.persistenceM.CreateSave(GameManager.colonyM.colony);
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen));
			} catch (Exception e) {
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed));
				throw e.InnerException ?? e;
			}
		}

		private void OnSettingsButtonClicked() {
			GameManager.uiM.OpenViewAsync<UISettings>(this);
		}

		private void OnExitToMenuButtonClicked() {

		}

		private void OnExitToDesktopButtonClicked() {

		}
	}
}
