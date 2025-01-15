using System;
using System.Collections.Generic;
using Snowship.NUI.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.DebugConsole {
	public class UIDebugConsoleView : UIView {

		[SerializeField] private TMP_InputField debugInputField;
		[SerializeField] private ScrollRect debugConsoleScrollRect;
		[SerializeField] private VerticalLayoutGroup debugConsoleVerticalLayoutGroup;
		[SerializeField] private RectTransform debugConsoleRect;

		public event Action<string> OnDebugCommandSent;

		private readonly List<UIDebugConsoleOutputTextbox> outputTextboxes = new();

		public override void OnOpen() {
			debugInputField.onEndEdit.AddListener(OnDebugInputFieldEndEdit);
		}

		public override void OnClose() {
			debugInputField.onEndEdit.RemoveListener(OnDebugInputFieldEndEdit);

			ClearConsole();
		}

		public void SelectDebugInputField() {
			if (UnityEngine.EventSystems.EventSystem.current.alreadySelecting) {
				return;
			}

			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(gameObject, null);
			debugInputField.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
		}

		private void OnDebugInputFieldEndEdit(string text) {
			if (string.IsNullOrEmpty(text)) {
				return;
			}

			OnDebugCommandSent?.Invoke(text);
			debugInputField.SetTextWithoutNotify(string.Empty);
		}

		public void OutputToConsole(string text) {
			outputTextboxes.Add(
				new UIDebugConsoleOutputTextbox(
					debugConsoleVerticalLayoutGroup.transform,
					text
				)
			);
		}

		public void ClearConsole() {
			foreach (UIDebugConsoleOutputTextbox outputTextbox in outputTextboxes) {
				outputTextbox.Close();
			}
			outputTextboxes.Clear();

			//debugConsoleRect.anchoredPosition = new Vector2(10, 0);
			debugConsoleScrollRect.verticalScrollbar.value = 0;
		}
	}
}
