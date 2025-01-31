using Snowship.NColonist;
using UnityEngine;
using static Snowship.NUtilities.ColourUtilities;

namespace Snowship.NUI
{
	public class UISkillElement : UIElement<UISkillElementComponent>
	{
		private readonly SkillInstance skill;

		public UISkillElement(Transform parent, SkillInstance skill) : base(parent) {
			this.skill = skill;

			Component.SetSkillName(skill.prefab.name);
			Component.SetSkillLevelText($"{skill.Level}.{Mathf.RoundToInt(skill.CurrentExperience / skill.NextLevelExperience * 100)}");

			skill.OnLevelChanged += OnSkillLevelChanged;
			skill.OnExperienceChanged += OnSkillExperienceChanged;

			OnSkillLevelChanged(skill.Level);
			OnSkillExperienceChanged(skill.CurrentExperience, skill.NextLevelExperience);
		}

		protected override void OnClose() {
			base.OnClose();

			skill.OnLevelChanged -= OnSkillLevelChanged;
			skill.OnExperienceChanged -= OnSkillExperienceChanged;
		}

		private void OnSkillLevelChanged(int level) {
			SkillInstance highestSkillInColonists = SkillInstance.GetBestColonistAtSkill(skill.prefab);
			bool thisIsHighestSkill = highestSkillInColonists.Level == skill.Level;
			Component.SetLevelSlider(
				level,
				highestSkillInColonists.Level,
				thisIsHighestSkill ? GetColour(EColour.DarkYellow) : GetColour(EColour.DarkGreen),
				thisIsHighestSkill ? GetColour(EColour.LightYellow) : GetColour(EColour.LightGreen)
			);
		}

		private void OnSkillExperienceChanged(float currentExperience, float nextLevelExperience) {
			Component.SetExperienceSlider(currentExperience, nextLevelExperience);
		}
	}
}