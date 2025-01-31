using Snowship.NUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIMoodElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text moodNameText;
		[SerializeField] private TMP_Text moodEffectAmountText;
		[SerializeField] private TMP_Text moodTimerText;
		[SerializeField] private Image backgroundImage;

		public void SetMoodNameText(string moodName) {
			moodNameText.text = moodName;
		}

		public void SetMoodEffectAmountText(int moodEffectAmount) {
			switch (moodEffectAmount) {
				case > 0:
					moodEffectAmountText.text = $"+{moodEffectAmount}%";
					backgroundImage.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
					break;
				case < 0:
					moodEffectAmountText.text = $"{moodEffectAmount}%";
					backgroundImage.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
					break;
				default:
					moodEffectAmountText.text = $"{moodEffectAmount}%";
					backgroundImage.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey220);
					break;
			}
		}

		public void SetMoodTimerText(int moodTimer, int moodEffectLength, bool infinite) {
			moodTimerText.SetText(infinite ? "Until Not" : $"{moodTimer}m ({moodEffectLength}m)");
		}
	}
}