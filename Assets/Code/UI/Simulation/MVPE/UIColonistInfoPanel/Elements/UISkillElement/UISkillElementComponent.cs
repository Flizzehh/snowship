using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UISkillElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text skillNameText;
		[SerializeField] private TMP_Text skillLevelText;

		[SerializeField] private Slider levelSlider;
		[SerializeField] private Image levelSliderFillImage;
		[SerializeField] private Image levelSliderHandleImage;
		[SerializeField] private Slider experienceSlider;

		public void SetSkillName(string skillName) {
			skillNameText.SetText(skillName);
		}

		public void SetSkillLevelText(string skillLevel) {
			skillLevelText.SetText(skillLevel);
		}

		public void SetLevelSlider(float level, float highestLevel, Color sliderFillColour, Color sliderHandleColour) {
			levelSlider.value = level / highestLevel;
			levelSliderFillImage.color = sliderFillColour;
			levelSliderHandleImage.color = sliderHandleColour;
		}

		public void SetExperienceSlider(float currentExperience, float nextLevelExperience) {
			experienceSlider.value = currentExperience / nextLevelExperience;
		}
	}
}