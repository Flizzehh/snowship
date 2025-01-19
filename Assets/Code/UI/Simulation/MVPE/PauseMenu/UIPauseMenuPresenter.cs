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
			UniTask.WhenAll(GameManager.stateM.TransitionToState(EState.Simulation, ETransitionUIAction.Close));
		}

		private void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			UniTask.WhenAll(GameManager.stateM.TransitionToState(EState.Simulation, ETransitionUIAction.Close));
		}

		private void OnSaveButtonClicked() {
			try {
				UniTask.WhenAll(GameManager.persistenceM.CreateSave(GameManager.colonyM.colony));
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen));
			} catch (Exception e) {
				View.SetSaveButtonImageColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed));
				throw e.InnerException ?? e;
			}
		}

		private void OnSettingsButtonClicked() {
			UniTask.WhenAll(GameManager.uiM.OpenViewAsync<UISettings>(this, false));
		}

		private void OnExitToMenuButtonClicked() {
			UniTask.WhenAll(GameManager.stateM.TransitionToState(EState.MainMenu, ETransitionUIAction.Close));
		}

		private void OnExitToDesktopButtonClicked() {
			Application.Quit();
		}
	}
}
