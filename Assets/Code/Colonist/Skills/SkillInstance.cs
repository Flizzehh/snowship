using System.Linq;
using Snowship.NJob;

namespace Snowship.NColonist {
	public class SkillInstance {

		public Colonist colonist;
		public SkillPrefab prefab;

		public int level;
		public float currentExperience;
		public float nextLevelExperience;

		public SkillInstance(Colonist colonist, SkillPrefab prefab, bool randomStartingLevel, int startingLevel) {
			this.colonist = colonist;
			this.prefab = prefab;

			if (randomStartingLevel) {
				//level = UnityEngine.Random.Range((colonist.profession.primarySkill != null && colonist.profession.primarySkill.type == prefab.type ? Mathf.RoundToInt(colonist.profession.skillRandomMaxValues[prefab] / 2f) : 0), colonist.profession.skillRandomMaxValues[prefab]);
				level = UnityEngine.Random.Range(0, 7);
			} else {
				level = startingLevel;
			}

			currentExperience = UnityEngine.Random.Range(0, 100);
			nextLevelExperience = 100 + 10 * level;
			AddExperience(0);
		}

		public void AddExperience(float amount) {
			currentExperience += amount;
			while (currentExperience >= nextLevelExperience) {
				level += 1;
				currentExperience -= nextLevelExperience;
				nextLevelExperience = 100 + 10 * level;
			}
			ColonistJob.UpdateColonistJobCosts(colonist);
			if (GameManager.humanM.selectedHuman == colonist) {
				// GameManager.uiMOld.RemakeSelectedColonistSkills(); // TODO Skill Value Updated
			}
		}

		public float CalculateTotalSkillLevel() {
			return level + currentExperience / nextLevelExperience;
		}

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
