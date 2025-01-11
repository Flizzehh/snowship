using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.Settings {
	public class UISettingsView : UIView {

		[Header("General")]
		[SerializeField] private Button cancelButton;
		[SerializeField] private Button applyButton;
		[SerializeField] private Button acceptButton;

		[Header("Resolution")]
		[SerializeField] private Slider resolutionSlider;
		[SerializeField] private Text resolutionText;

		[Header("Fullscreen")]
		[SerializeField] private Toggle fullscreenToggle;

		[Header("UI Scale")]
		[SerializeField] private Toggle uiScaleToggle;

		public event Action OnCancelButtonClicked;
		public event Action OnApplyButtonClicked;
		public event Action OnAcceptButtonClicked;

		public event Action<float> OnResolutionSliderChanged;
		public event Action<bool> OnFullscreenToggled;
		public event Action<bool> OnUIScaleToggled;

		public override void OnOpen() {
			cancelButton.onClick.AddListener(() => OnCancelButtonClicked?.Invoke());
			applyButton.onClick.AddListener(() => OnApplyButtonClicked?.Invoke());
			acceptButton.onClick.AddListener(() => OnAcceptButtonClicked?.Invoke());

			resolutionSlider.onValueChanged.AddListener(sliderValue => OnResolutionSliderChanged?.Invoke(sliderValue));
			fullscreenToggle.onValueChanged.AddListener(toggleValue => OnFullscreenToggled?.Invoke(toggleValue));
			uiScaleToggle.onValueChanged.AddListener(toggleValue => OnUIScaleToggled?.Invoke(toggleValue));
		}

		public override void OnClose() {

		}

		public void SetResolutionSlider(int minValue, int maxValue, int initialValue) {
			resolutionSlider.minValue = minValue;
			resolutionSlider.maxValue = maxValue;
			resolutionSlider.value = initialValue;
		}

		public void SetResolutionText(string text) {
			resolutionText.text = text;
		}

		public void SetFullscreenToggle(bool isOn) {
			fullscreenToggle.isOn = isOn;
		}

		public void SetUIScaleToggle(bool isOn) {
			uiScaleToggle.isOn = isOn;
		}

		public void SetButtonColours(
			Color cancelButtonColour,
			Color applyButtonColour,
			Color acceptButtonColour
		) {
			cancelButton.image.color = cancelButtonColour;
			applyButton.image.color = applyButtonColour;
			acceptButton.image.color = acceptButtonColour;
		}
	}
}
