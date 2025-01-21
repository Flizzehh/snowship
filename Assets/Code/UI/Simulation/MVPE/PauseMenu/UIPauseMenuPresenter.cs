using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NState;
using Snowship.NUI.Menu.Settings;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.InputSystem;

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

			GameManager.inputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;
		}

		public override void OnClose() {
			View.OnContinueButtonClicked -= OnContinueButtonClicked;
			View.OnSaveButtonClicked -= OnSaveButtonClicked;
			View.OnSettingsButtonClicked -= OnSettingsButtonClicked;
			View.OnExitToMenuButtonClicked -= OnExitToMenuButtonClicked;
			View.OnExitToDesktopButtonClicked -= OnExitToDesktopButtonClicked;

			GameManager.inputM.InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private void OnContinueButtonClicked() {
			GameManager.stateM.TransitionToState(EState.Simulation, ETransitionUIAction.Close).Forget();
		}

		private void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			GameManager.stateM.TransitionToState(EState.Simulation, ETransitionUIAction.Close).Forget();
		}

		private async void OnSaveButtonClicked() {
			try {
				await GameManager.persistenceM.CreateSave(GameManager.colonyM.colony);
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen));
			} catch (Exception e) {
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed));
				throw e.InnerException ?? e;
			}
		}

		private void OnSettingsButtonClicked() {
			GameManager.uiM.OpenViewAsync<UISettings>(this, false).Forget();
		}

		private void OnExitToMenuButtonClicked() {
			GameManager.stateM.TransitionToState(EState.MainMenu, ETransitionUIAction.Close).Forget();
		}

		private void OnExitToDesktopButtonClicked() {
			Application.Quit();
		}
	}
}
