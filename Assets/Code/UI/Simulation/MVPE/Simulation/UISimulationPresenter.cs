using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NPersistence;
using Snowship.NState;
using Snowship.NUI.Generic;
using UnityEngine.InputSystem;

namespace Snowship.NUI.Simulation.SimulationUI {

	[UsedImplicitly]
	public class UISimulationPresenter : UIPresenter<UISimulationView> {

		public UISimulationPresenter(UISimulationView view) : base(view) {
		}

		public override void OnCreate() {
			GameManager.inputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;

			View.SetDisclaimerText($"Lumi Games (Snowship {PersistenceManager.GameVersion.text})");

			SetupChildUIs().Forget();

			GameManager.humanM.OnHumanSelected += OnHumanSelected;
		}

		public override void OnClose() {
			GameManager.inputM.InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			// TODO Once setup, de-select items from SelectionManager in a FIFO format, and if none remain, then open the PauseMenu
			GameManager.stateM.TransitionToState(EState.PauseMenu, ETransitionUIAction.Hide).Forget();
		}

		private async UniTask SetupChildUIs() {
			await GameManager.uiM.OpenViewAsync<UIDateTime.UIDateTime>(this);
		}

		private void OnHumanSelected(HumanManager.Human selectedHuman) {
			GameManager.uiM.CloseView<UIColonistInfoPanel.UIColonistInfoPanel>();
			if (selectedHuman is Colonist) {
				GameManager.uiM.OpenViewAsync<UIColonistInfoPanel.UIColonistInfoPanel>(this).Forget();
			}
		}

	}
}
