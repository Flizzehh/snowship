using System;
using TMPro;
using UnityEngine.EventSystems;

namespace Snowship.NInput {
	public class InputManager : IManager {

		private InputSystemActions inputSystemActions;
		public InputSystemActions InputSystemActions {
			get => inputSystemActions ??= new InputSystemActions();
			private set => inputSystemActions = value;
		}

		public event Action<InputSystemActions> OnInputSystemEnabled;
		public event Action<InputSystemActions> OnInputSystemDisabled;

		public void OnCreate() {
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

		public bool IsPlayerTyping() {
			return EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
		}
	}
}
