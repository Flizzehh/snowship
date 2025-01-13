using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NState;
using Snowship.NUI.Generic;
using UnityEngine.InputSystem;

namespace Snowship.NUI.Simulation.SimulationUI {

	[UsedImplicitly]
	public class UISimulationPresenter : UIPresenter<UISimulationView> {

		public UISimulationPresenter(UISimulationView view) : base(view) {
		}

		public override void OnCreate() {
			OnInputSystemEnabled(GameManager.inputM.InputSystemActions);
			GameManager.inputM.OnInputSystemDisabled += OnInputSystemDisabled;
		}

		private void OnInputSystemEnabled(InputSystemActions actions) {
			actions.Simulation.Escape.performed += OnEscapePerformed;
		}

		private void OnInputSystemDisabled(InputSystemActions actions) {
			actions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			UniTask.WhenAll(GameManager.stateM.TransitionToState(EState.PauseMenu, ETransitionUIAction.Hide));
		}

		public override void OnClose() {

		}

	}
}
