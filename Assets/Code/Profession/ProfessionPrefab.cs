using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using UnityEngine;

namespace Snowship.NProfession {

	public class ProfessionPrefab {

		public static readonly List<ProfessionPrefab> professionPrefabs = new List<ProfessionPrefab>();

		public string type;
		public string name;

		public static readonly int maxPriority = 9;

		public List<string> jobs;

		public ColonistManager.SkillEnum relatedSkill;

		public ProfessionPrefab(
			string type,
			List<string> jobs
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.jobs = jobs;

			relatedSkill = (ColonistManager.SkillEnum)Enum.Parse(
				typeof(ColonistManager.SkillEnum),
				Enum.GetNames(typeof(ColonistManager.SkillEnum)).ToList().Find(skillEnumString => type.ToString() == skillEnumString)
			);
			GameManager.colonistM.GetSkillPrefabFromEnum(relatedSkill).relatedProfession = type;
		}

		public static void CreateProfessionPrefabs() {

			if (professionPrefabs.Count > 0) {
				Debug.LogError("Calling CreateProfessionPrefabs when professionPrefabs list already contains elements.");
			}

			List<KeyValuePair<string, object>> professionProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/colonist-professions").text.Split('\n').ToList());

			foreach (KeyValuePair<string, object> professionProperty in professionProperties) {
				switch (professionProperty.Key) {
					case "Profession":

						string type = null;
						List<string> jobs = new();

						foreach (KeyValuePair<string, object> professionSubProperty in (List<KeyValuePair<string, object>>)professionProperty.Value) {
							switch (professionSubProperty.Key) {
								case "Type":
									type = (string)professionSubProperty.Value;
									break;
								case "Jobs":
									foreach (string jobString in ((string)professionSubProperty.Value).Split(',')) {
										jobs.Add(jobString);
									}
									break;
								default:
									Debug.LogError("Unknown profession sub property: " + professionSubProperty.Key + " " + professionSubProperty.Value);
									break;
							}

						}

						ProfessionPrefab profession = new ProfessionPrefab(
							type,
							jobs
						);
						professionPrefabs.Add(profession);

						break;
					default:
						Debug.LogError("Unknown profession property: " + professionProperty.Key + " " + professionProperty.Value);
						break;
				}
			}
		}

		public static ProfessionPrefab GetProfessionFromType(string type) {
			return professionPrefabs.Find(profession => profession.type == type);
		}
	}
}