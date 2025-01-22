using System;
using System.Linq;
using Snowship.NJob;

namespace Snowship.NColonist {
	public class SkillInstance {
		public readonly Colonist colonist;
		public readonly SkillPrefab prefab;

		public int Level { get; set; }
		public float CurrentExperience { get; set; }
		public float NextLevelExperience { get; set; }

		public event Action<int> OnLevelChanged;
		public event Action<float, float> OnExperienceChanged;

		public SkillInstance(Colonist colonist, SkillPrefab prefab, bool randomStartingLevel, int startingLevel) {
			this.colonist = colonist;
			this.prefab = prefab;

			if (randomStartingLevel) {
				//level = UnityEngine.Random.Range((colonist.profession.primarySkill != null && colonist.profession.primarySkill.type == prefab.type ? Mathf.RoundToInt(colonist.profession.skillRandomMaxValues[prefab] / 2f) : 0), colonist.profession.skillRandomMaxValues[prefab]);
				Level = UnityEngine.Random.Range(0, 7);
			} else {
				Level = startingLevel;
			}

			CurrentExperience = UnityEngine.Random.Range(0, 100);
			NextLevelExperience = CalculateNextLevelExperience();
		}

		public void AddExperience(float amount) {
			CurrentExperience += amount;
			while (CurrentExperience >= NextLevelExperience) {
				Level += 1;
				CurrentExperience -= NextLevelExperience;
				NextLevelExperience = CalculateNextLevelExperience();
				OnLevelChanged?.Invoke(Level);
			}
			OnExperienceChanged?.Invoke(CurrentExperience, NextLevelExperience);
			ColonistJob.UpdateColonistJobCosts(colonist);
		}

		private int CalculateNextLevelExperience() {
			return 100 + 10 * Level;
		}

		public float CalculateTotalSkillLevel() {
			return Level + CurrentExperience / NextLevelExperience;
		}

		// TODO Make not static
		public static SkillInstance GetBestColonistAtSkill(SkillPrefab skill) {

			Colonist firstColonist = Colonist.colonists.FirstOrDefault();
			if (firstColonist == null) {
				return null;
			}
			SkillInstance highestSkillInstance = firstColonist.skills.Find(findSkill => findSkill.prefab == skill);
			float highestSkillValue = highestSkillInstance.CalculateTotalSkillLevel();

			foreach (Colonist otherColonist in Colonist.colonists.Skip(1)) {
				SkillInstance otherColonistSkillInstance = otherColonist.skills.Find(findSkill => findSkill.prefab == skill);
				float otherColonistSkillValue = otherColonistSkillInstance.CalculateTotalSkillLevel();
				if (otherColonistSkillValue > highestSkillValue) {
					highestSkillValue = otherColonistSkillValue;
					highestSkillInstance = otherColonistSkillInstance;
				}
			}

			return highestSkillInstance;
		}

	}
}
