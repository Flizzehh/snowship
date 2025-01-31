using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Snowship.NUtilities.ColourUtilities;

namespace Snowship.NUI
{
	[RequireComponent(typeof(Slider))]
	[DisallowMultipleComponent]
	public class UISliderHandler : MonoBehaviour
	{
		[Header("Optional")]
		[SerializeField] private TMP_Text valueText;

		private Slider slider;
		private Image fillImage;
		private Image handleImage;

		private (EColour min, EColour max) fillColours;
		private (EColour min, EColour max) handleColours;

		private void OnEnable() {
			slider = GetComponent<Slider>();
			fillImage = slider.fillRect.GetComponent<Image>();
			handleImage = slider.handleRect.GetComponent<Image>();
			slider.onValueChanged.AddListener(OnSliderValueChanged);
		}

		private void OnSliderValueChanged(float value) {
			bool fillImageColoursExist = fillColours != default;
			bool handleImageColoursExist = handleColours != default;

			if (!fillImageColoursExist && !handleImageColoursExist) {
				return;
			}

			float normalizedSliderValue = (value - slider.minValue) / (slider.maxValue - slider.minValue);
			if (fillImageColoursExist) {
				fillImage.color = Color.Lerp(GetColour(fillColours.min), GetColour(fillColours.max), normalizedSliderValue);
			}
			if (handleImageColoursExist) {
				handleImage.color = Color.Lerp(GetColour(handleColours.min), GetColour(handleColours.max), normalizedSliderValue);
			}
		}

		public void SetSliderRange(
			float min,
			float max,
			float initialValue,
			bool wholeNumbers = true
		) {
			slider.minValue = min;
			slider.maxValue = max;
			slider.wholeNumbers = wholeNumbers;
			SetValue(initialValue);
		}

		public void SetFillColours(EColour fillMinColour, EColour fillMaxColour) {
			fillColours.min = fillMinColour;
			fillColours.max = fillMaxColour;
		}

		public void SetHandleColours(EColour handleMinColour, EColour handleMaxColour) {
			handleColours.min = handleMinColour;
			handleColours.max = handleMaxColour;
		}

		public void SetValue(float value) {
			SetValue(value, value.ToString(CultureInfo.CurrentCulture));
		}

		public void SetValue(float value, string valueString) {
			slider.value = value;
			valueText?.SetText(valueString);
		}
	}
}