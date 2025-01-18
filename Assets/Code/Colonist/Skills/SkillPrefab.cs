using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NColonist {

	public class SkillPrefab {

		public ESkill type;
		public string name;

		public readonly Dictionary<string, float> affectedJobTypes = new Dictionary<string, float>();

		public string relatedProfession;

		public static readonly List<SkillPrefab> skillPrefabs = new List<SkillPrefab>();

		public SkillPrefab(List<string> data) {
			type = (ESkill)Enum.Parse(typeof(ESkill), data[0]);
			name = type.ToString();

			foreach (string affectedJobTypeString in data[1].Split(';')) {
				List<string> affectedJobTypeData = affectedJobTypeString.Split(',').ToList();
				affectedJobTypes.Add(affectedJobTypeData[0], float.Parse(affectedJobTypeData[1]));
			}
		}

		public static void CreateColonistSkills() {
			List<string> stringSkills = Resources.Load<TextAsset>(@"Data/colonist-skills").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('`').ToList();
			foreach (string stringSkill in stringSkills) {
				List<string> stringSkillData = stringSkill.Split('/').ToList();
				skillPrefabs.Add(new SkillPrefab(stringSkillData));
			}
			foreach (SkillPrefab skillPrefab in skillPrefabs) {
				skillPrefab.name = StringUtilities.SplitByCapitals(skillPrefab.name);
			}
		}

		public static SkillPrefab GetSkillPrefabFromString(string skillTypeString) {
			return skillPrefabs.Find(skillPrefab => skillPrefab.type == (ESkill)Enum.Parse(typeof(ESkill), skillTypeString));
		}

		public static SkillPrefab GetSkillPrefabFromEnum(ESkill skillEnum) {
			return skillPrefabs.Find(skillPrefab => skillPrefab.type == skillEnum);
		}

	}

}
