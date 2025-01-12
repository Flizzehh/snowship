using System;

namespace Snowship.NInput {
	public class InputManager : IManager {

		private InputSystemActions inputSystemActions;

		public event Action<InputSystemActions> OnInputSystemEnabled;
		public event Action<InputSystemActions> OnInputSystemDisabled;

		public void Awake() {
			inputSystemActions = new InputSystemActions();

			EnableInputSystem();
		}

		private void EnableInputSystem() {
			inputSystemActions.Enable();
			OnInputSystemEnabled?.Invoke(inputSystemActions);
		}

		private void DisableInputSystem() {
			inputSystemActions.Disable();
			OnInputSystemDisabled?.Invoke(inputSystemActions);
		}

	}
}
