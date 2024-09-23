using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.PauseMenu {
	public class UIPauseMenuView : UIView {

		[SerializeField] private Button continueButton;
		[SerializeField] private Button saveButton;
		[SerializeField] private Image saveButtonImage;
		[SerializeField] private Button settingsButton;
		[SerializeField] private Button exitToMenuButton;
		[SerializeField] private Button exitToDesktopButton;

		public event Action OnContinueButtonClicked;
		public event Action OnSaveButtonClicked;
		public event Action OnSettingsButtonClicked;
		public event Action OnExitToMenuButtonClicked;
		public event Action OnExitToDesktopButtonClicked;

		public override void OnOpen() {
			continueButton.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());
			saveButton.onClick.AddListener(() => OnSaveButtonClicked?.Invoke());
			settingsButton.onClick.AddListener(() => OnSettingsButtonClicked?.Invoke());
			exitToMenuButton.onClick.AddListener(() => OnExitToMenuButtonClicked?.Invoke());
			exitToDesktopButton.onClick.AddListener(() => OnExitToDesktopButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}

		public void SetSaveButtonImageColour(Color color) {
			saveButtonImage.color = color;
		}
	}
}
