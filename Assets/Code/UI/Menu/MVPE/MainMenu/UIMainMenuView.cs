using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.MainMenu {
	public class UIMainMenuView : UIView {

		[Header("General")]

		[SerializeField] private GameObject contentParent;

		[SerializeField] private TMP_Text disclaimerText;

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

		// TODO Background movement
		/*[SerializeField] private float movementMultiplier = 25f;
		private Vector2 originalBackgroundPosition;
		private Resolution screenResolution;*/

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

			/*originalBackgroundPosition = mainMenuBackgroundImage.rectTransform.pivot;
			screenResolution = Screen.currentResolution;*/
		}

		public override void SetActive(bool active) {
			contentParent.SetActive(active);
			darkBackground.SetActive(!active);
		}

		/*public void Update() {
			UpdateMainMenuBackground(); // TODO Fix because this doesn't work with the Aspect Ratio Fitter component, maybe try Pivot but it uses a 0 - 1 range
		}*/

		public override void OnClose() {
		}

		public void SetDisclaimerText(string text) {
			disclaimerText.text = text;
		}

		public void DisableContinueButton() {
			continueButton.interactable = false;
			continuePreviewImage.gameObject.SetActive(false);
		}

		public void SetupContinueButton(bool interactable, Sprite previewImage) {
			continueButton.interactable = interactable;
			continueButton.GetComponent<HoverToggleScript>().Initialize(continuePreviewImage.gameObject, false, null);
			continuePreviewImage.sprite = previewImage;
		}

		public void SetBackground(Sprite backgroundImage) {
			mainMenuBackgroundImage.sprite = backgroundImage;
			darkBackground.SetActive(false);
		}

		/*private void UpdateMainMenuBackground() {
			mainMenuBackgroundRectTransform.anchoredPosition = originalBackgroundPosition
				+ new Vector2(
					-Input.mousePosition.x / (screenResolution.width / movementMultiplier),
					-Input.mousePosition.y / (screenResolution.height / movementMultiplier))
				+ new Vector2(
					screenResolution.width,
					screenResolution.height
				) / movementMultiplier / 2f;
		}*/
	}
}
