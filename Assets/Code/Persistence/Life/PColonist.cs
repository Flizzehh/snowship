using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NColonist;
using Snowship.NJob;
using Snowship.NProfession;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PColonist : PersistenceHandler {

		private readonly PLife pLife = new PLife();
		private readonly PHuman pHuman = new PHuman();

		private readonly PJob pJob = new PJob();

		public enum ColonistProperty {
			Colonist,
			Life,
			Human,
			PlayerMoved,
			Job,
			StoredJob,
			BacklogJobs,
			Professions,
			Skills,
			Traits,
			Needs,
			BaseMood,
			EffectiveMood,
			MoodModifiers
		}

		public enum BacklogJobProperty {
			BacklogJob
		}

		public enum ProfessionProperty {
			Profession,
			Name,
			Priority
		}

		public enum SkillProperty {
			Skill,
			Type,
			Level,
			NextLevelExperience,
			CurrentExperience
		}

		public enum TraitProperty {
			Trait,
			Type
		}

		public enum NeedProperty {
			Need,
			Type,
			Value
		}

		public enum MoodModifierProperty {
			MoodModifier,
			Type,
			TimeRemaining
		}

		public void SaveColonists(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "colonists.snowship");

			foreach (Colonist colonist in Colonist.colonists) {
				file.WriteLine(CreateKeyValueString(ColonistProperty.Colonist, string.Empty, 0));

				pLife.WriteLifeLines(file, colonist, 1);

				pHuman.WriteHumanLines(file, colonist, 1);

				file.WriteLine(CreateKeyValueString(ColonistProperty.PlayerMoved, colonist.playerMoved, 1));

				if (colonist.job != null) {
					pJob.WriteJobLines(file, colonist.job, PJob.JobProperty.Job, 1);
				}
				if (colonist.storedJob != null) {
					pJob.WriteJobLines(file, colonist.storedJob, PJob.JobProperty.StoredJob, 1);
				}
				if (colonist.backlog.Count > 0) {
					file.WriteLine(CreateKeyValueString(ColonistProperty.BacklogJobs, string.Empty, 1));
					foreach (Job backlogJob in colonist.backlog) {
						pJob.WriteJobLines(file, backlogJob, PJob.JobProperty.BacklogJob, 2);
					}
				}

				file.WriteLine(CreateKeyValueString(ColonistProperty.Professions, string.Empty, 1));
				foreach (Profession profession in colonist.professions) {
					file.WriteLine(CreateKeyValueString(ProfessionProperty.Profession, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(ProfessionProperty.Name, profession.prefab.type, 3));
					file.WriteLine(CreateKeyValueString(ProfessionProperty.Priority, profession.GetPriority(), 3));
				}

				file.WriteLine(CreateKeyValueString(ColonistProperty.Skills, string.Empty, 1));
				foreach (SkillInstance skill in colonist.skills) {
					file.WriteLine(CreateKeyValueString(SkillProperty.Skill, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(SkillProperty.Type, skill.prefab.type, 3));
					file.WriteLine(CreateKeyValueString(SkillProperty.Level, skill.Level, 3));
					file.WriteLine(CreateKeyValueString(SkillProperty.NextLevelExperience, skill.NextLevelExperience, 3));
					file.WriteLine(CreateKeyValueString(SkillProperty.CurrentExperience, skill.CurrentExperience, 3));
				}

				if (colonist.traits.Count > 0) {
					file.WriteLine(CreateKeyValueString(ColonistProperty.Traits, string.Empty, 1));
					foreach (TraitInstance trait in colonist.traits) {
						file.WriteLine(CreateKeyValueString(TraitProperty.Trait, string.Empty, 2));

						file.WriteLine(CreateKeyValueString(TraitProperty.Type, trait.prefab.type, 3));
					}
				}

				file.WriteLine(CreateKeyValueString(ColonistProperty.Needs, string.Empty, 1));
				foreach (NeedInstance need in colonist.needs) {
					file.WriteLine(CreateKeyValueString(NeedProperty.Need, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(NeedProperty.Type, need.prefab.type, 3));
					file.WriteLine(CreateKeyValueString(NeedProperty.Value, need.GetValue(), 3));
				}

				file.WriteLine(CreateKeyValueString(ColonistProperty.BaseMood, colonist.baseMood, 1));
				file.WriteLine(CreateKeyValueString(ColonistProperty.EffectiveMood, colonist.effectiveMood, 1));

				if (colonist.moodModifiers.Count > 0) {
					file.WriteLine(CreateKeyValueString(ColonistProperty.MoodModifiers, string.Empty, 1));
					foreach (MoodModifierInstance moodModifier in colonist.moodModifiers) {
						file.WriteLine(CreateKeyValueString(MoodModifierProperty.MoodModifier, string.Empty, 2));

						file.WriteLine(CreateKeyValueString(MoodModifierProperty.Type, moodModifier.prefab.type, 3));
						file.WriteLine(CreateKeyValueString(MoodModifierProperty.TimeRemaining, moodModifier.timer, 3));
					}
				}
			}

			file.Close();
		}

		public class PersistenceColonist {

			public PLife.PersistenceLife persistenceLife;
			public PHuman.PersistenceHuman persistenceHuman;

			public bool? playerMoved;
			public PJob.PersistenceJob persistenceJob;
			public PJob.PersistenceJob persistenceStoredJob;
			public List<PJob.PersistenceJob> persistenceBacklogJobs;
			public List<PersistenceProfession> persistenceProfessions;
			public List<PersistenceSkill> persistenceSkills;
			public List<PersistenceTrait> persistenceTraits;
			public List<PersistenceNeed> persistenceNeeds;
			public float? baseMood;
			public float? effectiveMood;
			public List<PersistenceMoodModifier> persistenceMoodModifiers;

			public PersistenceColonist(
				PLife.PersistenceLife persistenceLife,
				PHuman.PersistenceHuman persistenceHuman,
				bool? playerMoved,
				PJob.PersistenceJob persistenceJob,
				PJob.PersistenceJob persistenceStoredJob,
				List<PJob.PersistenceJob> persistenceBacklogJobs,
				List<PersistenceProfession> persistenceProfessions,
				List<PersistenceSkill> persistenceSkills,
				List<PersistenceTrait> persistenceTraits,
				List<PersistenceNeed> persistenceNeeds,
				float? baseMood,
				float? effectiveMood,
				List<PersistenceMoodModifier> persistenceMoodModifiers
			) {
				this.persistenceLife = persistenceLife;
				this.persistenceHuman = persistenceHuman;
				this.playerMoved = playerMoved;
				this.persistenceJob = persistenceJob;
				this.persistenceStoredJob = persistenceStoredJob;
				this.persistenceBacklogJobs = persistenceBacklogJobs;
				this.persistenceProfessions = persistenceProfessions;
				this.persistenceSkills = persistenceSkills;
				this.persistenceTraits = persistenceTraits;
				this.persistenceNeeds = persistenceNeeds;
				this.baseMood = baseMood;
				this.effectiveMood = effectiveMood;
				this.persistenceMoodModifiers = persistenceMoodModifiers;
			}
		}

		public class PersistenceProfession {
			public string type;
			public int? priority;

			public PersistenceProfession(
				string type,
				int? priority
			) {
				this.type = type;
				this.priority = priority;
			}
		}

		public class PersistenceSkill {

			public ESkill? type;
			public int? level;
			public float? nextLevelExperience;
			public float? currentExperience;

			public PersistenceSkill(
				ESkill? type,
				int? level,
				float? nextLevelExperience,
				float? currentExperience
			) {
				this.type = type;
				this.level = level;
				this.nextLevelExperience = nextLevelExperience;
				this.currentExperience = currentExperience;
			}
		}

		public class PersistenceTrait {

			public ETrait? type;

			public PersistenceTrait(ETrait? type) {
				this.type = type;
			}
		}

		public class PersistenceNeed {

			public ENeed? type;
			public float? value;

			public PersistenceNeed(
				ENeed? type,
				float? value
			) {
				this.type = type;
				this.value = value;
			}
		}

		public class PersistenceMoodModifier {

			public MoodModifierEnum? type;
			public int? timeRemaining;

			public PersistenceMoodModifier(
				MoodModifierEnum? type,
				int? timeRemaining
			) {
				this.type = type;
				this.timeRemaining = timeRemaining;
			}
		}

		public List<PersistenceColonist> LoadColonists(string path) {
			List<PersistenceColonist> persistenceColonists = new List<PersistenceColonist>();

			List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((ColonistProperty)Enum.Parse(typeof(ColonistProperty), property.Key)) {
					case ColonistProperty.Colonist:

						List<KeyValuePair<string, object>> colonistProperties = (List<KeyValuePair<string, object>>)property.Value;

						PLife.PersistenceLife persistenceLife = null;
						PHuman.PersistenceHuman persistenceHuman = null;

						bool? playerMoved = null;
						PJob.PersistenceJob persistenceJob = null;
						PJob.PersistenceJob persistenceStoredJob = null;
						List<PJob.PersistenceJob> persistenceBacklogJobs = new List<PJob.PersistenceJob>();
						List<PersistenceProfession> persistenceProfessions = new List<PersistenceProfession>();
						List<PersistenceSkill> persistenceSkills = new List<PersistenceSkill>();
						List<PersistenceTrait> persistenceTraits = new List<PersistenceTrait>();
						List<PersistenceNeed> persistenceNeeds = new List<PersistenceNeed>();
						float? baseMood = null;
						float? effectiveMood = null;
						List<PersistenceMoodModifier> persistenceMoodModifiers = new List<PersistenceMoodModifier>();

						foreach (KeyValuePair<string, object> colonistProperty in colonistProperties) {
							switch ((ColonistProperty)Enum.Parse(typeof(ColonistProperty), colonistProperty.Key)) {
								case ColonistProperty.Life:
									persistenceLife = pLife.LoadPersistenceLife((List<KeyValuePair<string, object>>)colonistProperty.Value);
									break;
								case ColonistProperty.Human:
									persistenceHuman = pHuman.LoadPersistenceHuman((List<KeyValuePair<string, object>>)colonistProperty.Value);
									break;
								case ColonistProperty.PlayerMoved:
									playerMoved = bool.Parse((string)colonistProperty.Value);
									break;
								case ColonistProperty.Job:
									persistenceJob = pJob.LoadPersistenceJob((List<KeyValuePair<string, object>>)colonistProperty.Value);
									break;
								case ColonistProperty.StoredJob:
									persistenceStoredJob = pJob.LoadPersistenceJob((List<KeyValuePair<string, object>>)colonistProperty.Value);
									break;
								case ColonistProperty.BacklogJobs:
									foreach (KeyValuePair<string, object> backlogJobProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
										switch ((BacklogJobProperty)Enum.Parse(typeof(BacklogJobProperty), backlogJobProperty.Key)) {
											case BacklogJobProperty.BacklogJob:
												persistenceBacklogJobs.Add(pJob.LoadPersistenceJob((List<KeyValuePair<string, object>>)backlogJobProperty.Value));
												break;
										}
									}
									break;
								case ColonistProperty.Professions:
									foreach (KeyValuePair<string, object> professionProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
										switch ((ProfessionProperty)Enum.Parse(typeof(ProfessionProperty), professionProperty.Key)) {
											case ProfessionProperty.Profession:

												string professionType = null;
												int? professionPriority = null;

												foreach (KeyValuePair<string, object> professionSubProperty in (List<KeyValuePair<string, object>>)professionProperty.Value) {
													switch ((ProfessionProperty)Enum.Parse(typeof(ProfessionProperty), professionSubProperty.Key)) {
														case ProfessionProperty.Name:
															professionType = (string)professionSubProperty.Value;
															break;
														case ProfessionProperty.Priority:
															professionPriority = int.Parse((string)professionSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown profession sub property: " + professionSubProperty.Key + " " + professionSubProperty.Value);
															break;
													}
												}

												persistenceProfessions.Add(
													new PersistenceProfession(
														professionType,
														professionPriority
													));
												break;
											default:
												Debug.LogError("Unknown profession property: " + professionProperty.Key + " " + professionProperty.Value);
												break;
										}
									}
									break;
								case ColonistProperty.Skills:
									foreach (KeyValuePair<string, object> skillProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
										switch ((SkillProperty)Enum.Parse(typeof(SkillProperty), skillProperty.Key)) {
											case SkillProperty.Skill:

												ESkill? skillType = null;
												int? skillLevel = null;
												float? skillNextLevelExperience = null;
												float? skillCurrentExperience = null;

												foreach (KeyValuePair<string, object> skillSubProperty in (List<KeyValuePair<string, object>>)skillProperty.Value) {
													switch ((SkillProperty)Enum.Parse(typeof(SkillProperty), skillSubProperty.Key)) {
														case SkillProperty.Type:
															skillType = (ESkill)Enum.Parse(typeof(ESkill), (string)skillSubProperty.Value);
															break;
														case SkillProperty.Level:
															skillLevel = int.Parse((string)skillSubProperty.Value);
															break;
														case SkillProperty.NextLevelExperience:
															skillNextLevelExperience = float.Parse((string)skillSubProperty.Value);
															break;
														case SkillProperty.CurrentExperience:
															skillCurrentExperience = float.Parse((string)skillSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown skill sub property: " + skillSubProperty.Key + " " + skillSubProperty.Value);
															break;
													}
												}

												persistenceSkills.Add(
													new PersistenceSkill(
														skillType,
														skillLevel,
														skillNextLevelExperience,
														skillCurrentExperience
													));
												break;
											default:
												Debug.LogError("Unknown skill property: " + skillProperty.Key + " " + skillProperty.Value);
												break;
										}
									}
									break;
								case ColonistProperty.Traits:
									foreach (KeyValuePair<string, object> traitProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
										switch ((TraitProperty)Enum.Parse(typeof(TraitProperty), traitProperty.Key)) {
											case TraitProperty.Trait:

												ETrait? traitType = null;

												foreach (KeyValuePair<string, object> traitSubProperty in (List<KeyValuePair<string, object>>)traitProperty.Value) {
													switch ((TraitProperty)Enum.Parse(typeof(TraitProperty), traitSubProperty.Key)) {
														case TraitProperty.Type:
															traitType = (ETrait)Enum.Parse(typeof(ETrait), (string)traitSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown trait sub property: " + traitSubProperty.Key + " " + traitSubProperty.Value);
															break;
													}
												}

												persistenceTraits.Add(
													new PersistenceTrait(
														traitType
													));
												break;
											default:
												Debug.LogError("Unknown trait property: " + traitProperty.Key + " " + traitProperty.Value);
												break;
										}
									}
									break;
								case ColonistProperty.Needs:
									foreach (KeyValuePair<string, object> needProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
										switch ((NeedProperty)Enum.Parse(typeof(NeedProperty), needProperty.Key)) {
											case NeedProperty.Need:

												ENeed? needType = null;
												float? needValue = null;

												foreach (KeyValuePair<string, object> needSubProperty in (List<KeyValuePair<string, object>>)needProperty.Value) {
													switch ((NeedProperty)Enum.Parse(typeof(NeedProperty), needSubProperty.Key)) {
														case NeedProperty.Type:
															needType = (ENeed)Enum.Parse(typeof(ENeed), (string)needSubProperty.Value);
															break;
														case NeedProperty.Value:
															needValue = float.Parse((string)needSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown need sub property: " + needSubProperty.Key + " " + needSubProperty.Value);
															break;
													}
												}

												persistenceNeeds.Add(
													new PersistenceNeed(
														needType,
														needValue
													));
												break;
											default:
												Debug.LogError("Unknown need property: " + needProperty.Key + " " + needProperty.Value);
												break;
										}
									}
									break;
								case ColonistProperty.BaseMood:
									baseMood = float.Parse((string)colonistProperty.Value);
									break;
								case ColonistProperty.EffectiveMood:
									effectiveMood = float.Parse((string)colonistProperty.Value);
									break;
								case ColonistProperty.MoodModifiers:
									foreach (KeyValuePair<string, object> moodModifierProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
										switch ((MoodModifierProperty)Enum.Parse(typeof(MoodModifierProperty), moodModifierProperty.Key)) {
											case MoodModifierProperty.MoodModifier:

												MoodModifierEnum? moodModifierType = null;
												int? moodModifierTimeRemaining = null;

												foreach (KeyValuePair<string, object> moodModifierSubProperty in (List<KeyValuePair<string, object>>)moodModifierProperty.Value) {
													switch ((MoodModifierProperty)Enum.Parse(typeof(MoodModifierProperty), moodModifierSubProperty.Key)) {
														case MoodModifierProperty.Type:
															moodModifierType = (MoodModifierEnum)Enum.Parse(typeof(MoodModifierEnum), (string)moodModifierSubProperty.Value);
															break;
														case MoodModifierProperty.TimeRemaining:
															moodModifierTimeRemaining = int.Parse((string)moodModifierSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown mood modifier sub property: " + moodModifierSubProperty.Key + " " + moodModifierSubProperty.Value);
															break;
													}
												}

												persistenceMoodModifiers.Add(new PersistenceMoodModifier(moodModifierType, moodModifierTimeRemaining));
												break;
											default:
												Debug.LogError("Unknown mood modifier property: " + moodModifierProperty.Key + " " + moodModifierProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown colonist property: " + colonistProperty.Key + " " + colonistProperty.Value);
									break;
							}
						}

						persistenceColonists.Add(
							new PersistenceColonist(
								persistenceLife,
								persistenceHuman,
								playerMoved,
								persistenceJob,
								persistenceStoredJob,
								persistenceBacklogJobs,
								persistenceProfessions,
								persistenceSkills,
								persistenceTraits,
								persistenceNeeds,
								baseMood,
								effectiveMood,
								persistenceMoodModifiers
							));
						break;
					default:
						Debug.LogError("Unknown colonist property: " + property.Key + " " + property.Value);
						break;
				}
			}

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedColonists;
			return persistenceColonists;
		}

		public void ApplyLoadedColonists(List<PersistenceColonist> persistenceColonists) {
			foreach (PersistenceColonist persistenceColonist in persistenceColonists) {
				Colonist colonist = new Colonist(
					GameManager.colonyM.colony.map.GetTileFromPosition(persistenceColonist.persistenceLife.position.Value),
					persistenceColonist.persistenceLife.health.Value
				) { gender = persistenceColonist.persistenceLife.gender.Value, previousPosition = persistenceColonist.persistenceLife.previousPosition.Value, playerMoved = persistenceColonist.playerMoved.Value, };

				colonist.obj.transform.position = persistenceColonist.persistenceLife.position.Value;

				colonist.SetName(persistenceColonist.persistenceHuman.name);

				colonist.bodyIndices[HumanManager.Human.Appearance.Skin] = persistenceColonist.persistenceHuman.skinIndex.Value;
				colonist.moveSprites = GameManager.humanM.humanMoveSprites[colonist.bodyIndices[HumanManager.Human.Appearance.Skin]];
				colonist.bodyIndices[HumanManager.Human.Appearance.Hair] = persistenceColonist.persistenceHuman.hairIndex.Value;

				colonist.GetInventory().maxWeight = persistenceColonist.persistenceHuman.persistenceInventory.maxWeight.Value;
				colonist.GetInventory().maxVolume = persistenceColonist.persistenceHuman.persistenceInventory.maxVolume.Value;
				foreach (ResourceAmount resourceAmount in persistenceColonist.persistenceHuman.persistenceInventory.resources) {
					colonist.GetInventory().ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
				}

				foreach (KeyValuePair<HumanManager.Human.Appearance, Clothing> appearanceToClothingKVP in persistenceColonist.persistenceHuman.clothes) {
					colonist.GetInventory().ChangeResourceAmount(Resource.GetResourceByEnum(appearanceToClothingKVP.Value.type), 1, false);
					colonist.ChangeClothing(appearanceToClothingKVP.Key, appearanceToClothingKVP.Value);
				}

				if (persistenceColonist.persistenceStoredJob != null) {
					Job storedJob = pJob.LoadJob(persistenceColonist.persistenceStoredJob);
					colonist.storedJob = storedJob;
				}

				if (persistenceColonist.persistenceJob != null) {
					Job job = pJob.LoadJob(persistenceColonist.persistenceJob);
					colonist.SetJob(new ColonistJob(colonist, job, persistenceColonist.persistenceJob.resourcesColonistHas, job.containerPickups));
				}

				foreach (PJob.PersistenceJob persistenceBacklogJob in persistenceColonist.persistenceBacklogJobs) {
					Job backlogJob = pJob.LoadJob(persistenceBacklogJob);
					colonist.backlog.Add(backlogJob);
				}

				if (persistenceColonist.persistenceLife.pathEndPosition.HasValue) {
					colonist.MoveToTile(GameManager.colonyM.colony.map.GetTileFromPosition(persistenceColonist.persistenceLife.pathEndPosition.Value), true);
				}

				foreach (PersistenceProfession persistenceProfession in persistenceColonist.persistenceProfessions) {
					Profession profession = colonist.GetProfessionFromType(persistenceProfession.type);
					profession.SetPriority(persistenceProfession.priority.Value);
				}

				foreach (PersistenceSkill persistenceSkill in persistenceColonist.persistenceSkills) {
					SkillInstance skill = colonist.skills.Find(s => s.prefab.type == persistenceSkill.type);
					skill.Level = persistenceSkill.level.Value;
					skill.NextLevelExperience = persistenceSkill.nextLevelExperience.Value;
					skill.CurrentExperience = persistenceSkill.currentExperience.Value;
				}

				foreach (PersistenceTrait persistenceTrait in persistenceColonist.persistenceTraits) {
					TraitInstance trait = colonist.traits.Find(t => t.prefab.type == persistenceTrait.type);
					Debug.LogWarning("Load Colonist Traits: " + trait.prefab.type);
				}

				foreach (PersistenceNeed persistenceNeed in persistenceColonist.persistenceNeeds) {
					NeedInstance need = colonist.needs.Find(n => n.prefab.type == persistenceNeed.type);
					need.SetValue(persistenceNeed.value.Value);
				}

				foreach (PersistenceMoodModifier persistenceMoodModifier in persistenceColonist.persistenceMoodModifiers) {
					colonist.AddMoodModifier(persistenceMoodModifier.type.Value);
					colonist.moodModifiers.Find(hm => hm.prefab.type == persistenceMoodModifier.type.Value).timer = persistenceMoodModifier.timeRemaining.Value;
				}

				colonist.baseMood = persistenceColonist.baseMood.Value;
				colonist.effectiveMood = persistenceColonist.effectiveMood.Value;
			}

			for (int i = 0; i < persistenceColonists.Count; i++) {
				PersistenceColonist persistenceColonist = persistenceColonists[i];
				Colonist colonist = Colonist.colonists[i];

				foreach (KeyValuePair<string, List<ResourceAmount>> humanToReservedResourcesKVP in persistenceColonist.persistenceHuman.persistenceInventory.reservedResources) {
					foreach (ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
						colonist.GetInventory().ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
					}
					colonist.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
				}
			}

			//GameManager.uiMOld.SetColonistElements();
		}

	}
}
