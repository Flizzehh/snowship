using Snowship.NUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel
{
	public class UINeedElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text needNameText;
		[SerializeField] private TMP_Text needValueText;

		[Header("Slider")]
		[SerializeField] private Slider needValueSlider;
		[SerializeField] private Image needValueSliderFill;
		[SerializeField] private Image needValueSliderHandle;

		public void SetNeedNameText(string name) {
			needNameText.SetText(name);
		}

		public void SetNeedValueText(int needValueRounded) {
			needValueText.SetText($"{needValueRounded}%");
		}

		public void SetNeedSlider(float needValue, float clampValue) {
			needValueSlider.value = needValue;
			needValueSliderFill.color = Color.Lerp(
				ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen),
				ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed),
				needValue / clampValue
			);
			needValueSliderHandle.color = Color.Lerp(
				ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen),
				ColourUtilities.GetColour(ColourUtilities.EColour.LightRed),
				needValue / clampValue
			);
		}
	}
}
