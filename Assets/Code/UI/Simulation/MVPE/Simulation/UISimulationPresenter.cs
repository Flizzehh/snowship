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
			GameManager.inputM.InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;
		}

		public override void OnClose() {
			GameManager.inputM.InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			UniTask.WhenAll(GameManager.stateM.TransitionToState(EState.PauseMenu, ETransitionUIAction.Hide));
		}

	}
}
