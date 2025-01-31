using System;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
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
			continueButton.onClick.RemoveListener(() => OnContinueButtonClicked?.Invoke());
			saveButton.onClick.RemoveListener(() => OnSaveButtonClicked?.Invoke());
			settingsButton.onClick.RemoveListener(() => OnSettingsButtonClicked?.Invoke());
			exitToMenuButton.onClick.RemoveListener(() => OnExitToMenuButtonClicked?.Invoke());
			exitToDesktopButton.onClick.RemoveListener(() => OnExitToDesktopButtonClicked?.Invoke());
		}

		public void SetSaveButtonImageColour(Color color) {
			saveButtonImage.color = color;
		}
	}
}