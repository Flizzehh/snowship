using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NJob;
using Snowship.NPersistence;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NProfession {

	public class ProfessionPrefab {

		public static readonly List<ProfessionPrefab> professionPrefabs = new List<ProfessionPrefab>();

		public string type;
		public string name;

		public static readonly int maxPriority = 9;

		public List<JobDefinition> jobs;

		public ESkill relatedSkill;

		public ProfessionPrefab(
			string type,
			List<JobDefinition> jobs
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.jobs = jobs;

			relatedSkill = (ESkill)Enum.Parse(
				typeof(ESkill),
				Enum.GetNames(typeof(ESkill)).ToList().Find(skillEnumString => type.ToString() == skillEnumString)
			);
			SkillPrefab.GetSkillPrefabFromEnum(relatedSkill).relatedProfession = type;
		}

		public static void CreateProfessionPrefabs() {

			if (professionPrefabs.Count > 0) {
				Debug.LogError("Calling CreateProfessionPrefabs when professionPrefabs list already contains elements.");
			}

			List<KeyValuePair<string, object>> professionProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/colonist-professions").text.Split('\n').ToList());

			foreach (KeyValuePair<string, object> professionProperty in professionProperties) {
				switch (professionProperty.Key) {
					case "Profession":

						string type = null;
						List<JobDefinition> jobs = new();

						foreach (KeyValuePair<string, object> professionSubProperty in (List<KeyValuePair<string, object>>)professionProperty.Value) {
							switch (professionSubProperty.Key) {
								case "Type":
									type = (string)professionSubProperty.Value;
									break;
								case "Jobs":
									foreach (string jobString in ((string)professionSubProperty.Value).Split(',')) {
										jobs.Add(GameManager.Get<JobManager>().JobRegistry.GetJobTypeFromName(jobString));
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