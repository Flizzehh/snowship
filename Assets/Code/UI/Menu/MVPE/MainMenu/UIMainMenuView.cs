using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.MainMenu {
	public class UIMainMenuView : UIView {

		private const float MovementMultiplier = 25f;

		[Header("General")]

		[SerializeField] private GameObject mainMenu;
		[SerializeField] private Text disclaimerText;

		[Header("Buttons")]

		[SerializeField] private Button newButton;

		[SerializeField] private Button continueButton;
		[SerializeField] private Image continuePreviewImage;

		[SerializeField] private Button loadButton;
		[SerializeField] private Button settingsButton;
		[SerializeField] private Button exitButton;

		[Header("Background")]

		[SerializeField] private Image mainMenuBackgroundImage;
		[SerializeField] private RectTransform mainMenuBackgroundRectTransform;

		[SerializeField] private Image snowshipLogo;
		[SerializeField] private RectTransform snowshipLogoRectTransform;

		[SerializeField] private GameObject darkBackground;

		private Vector2 originalBackgroundPosition;

		private Resolution screenResolution;

		public event Action OnNewButtonClicked;
		public event Action OnContinueButtonClicked;
		public event Action OnLoadButtonClicked;
		public event Action OnSettingsButtonClicked;
		public event Action OnExitButtonClicked;

		public override void OnOpen() {
			newButton.onClick.AddListener(() => OnNewButtonClicked?.Invoke());
			continueButton.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());
			loadButton.onClick.AddListener(() => OnLoadButtonClicked?.Invoke());
			settingsButton.onClick.AddListener(() => OnSettingsButtonClicked?.Invoke());
			exitButton.onClick.AddListener(() => OnExitButtonClicked?.Invoke());
		}

		public void Initialize(Resolution screenResolution) {
			this.screenResolution = screenResolution;
		}

		public void Update() {
			UpdateMainMenuBackground();
		}

		public override void OnClose() {

		}

		public void SetDisclaimerText(string text) {
			disclaimerText.text = text;
		}

		public void SetupContinueButton(bool interactable, Sprite previewImage) {
			continueButton.interactable = interactable;
			continueButton.GetComponent<HoverToggleScript>().Initialize(continuePreviewImage.gameObject, false, null);
			continuePreviewImage.sprite = previewImage;
		}

		public void SetBackground(Sprite backgroundImage) {
			mainMenuBackgroundImage.sprite = backgroundImage;
		}

		/*private void SetMainMenuBackground(bool randomBackgroundImage, Resolution screenResolution) {

			float targetNewSize = Mathf.Max(screenResolution.width, screenResolution.height);

			Vector2 menuBackgroundSize = mainMenuBackgroundImage.sprite.rect.size;
			float menuBackgroundTargetSize = Mathf.Max(menuBackgroundSize.x, menuBackgroundSize.y);
			float menuBackgroundRatio = menuBackgroundTargetSize / targetNewSize;
			Vector2 newMenuBackgroundSize = menuBackgroundSize / menuBackgroundRatio;
			mainMenuBackgroundRectTransform.sizeDelta = newMenuBackgroundSize * 1.5f;
			originalBackgroundPosition = mainMenuBackgroundRectTransform.position;

			Vector2 logoSize = snowshipLogo.sprite.rect.size;
			float logoTargetSize = Mathf.Max(logoSize.x, logoSize.y);
			float logoRatio = logoTargetSize / targetNewSize;
			Vector2 newLogoSize = logoSize / logoRatio;
			snowshipLogoRectTransform.sizeDelta = newLogoSize * 1.05f;
		}*/

		private void UpdateMainMenuBackground() {
			mainMenuBackgroundRectTransform.anchoredPosition = originalBackgroundPosition
				+ new Vector2(
					-Input.mousePosition.x / (screenResolution.width / MovementMultiplier),
					-Input.mousePosition.y / (screenResolution.height / MovementMultiplier))
				+ new Vector2(
					screenResolution.width,
					screenResolution.height
				) / MovementMultiplier / 2;
		}
	}
}
