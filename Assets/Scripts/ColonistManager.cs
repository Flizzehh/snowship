using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColonistManager : BaseManager {

	public override void Update() {
		foreach (Colonist colonist in colonists) {
			colonist.Update();
			if (colonist.dead) {
				deadColonists.Add(colonist);
			}
		}
		foreach (Colonist deadColonist in deadColonists) {
			deadColonist.Die();
		}
		deadColonists.Clear();

		if (!GameManager.timeM.GetPaused()) {
			GameManager.jobM.GiveJobsToColonists();
		}
	}

	public enum ProfessionEnum { Building, Terraforming, Farming, Forestry, Crafting, Hauling };

	public readonly List<ProfessionPrefab> professionPrefabs = new List<ProfessionPrefab>();

	public class ProfessionPrefab {

		public ProfessionEnum type;
		public string name;

		public List<JobManager.JobEnum> jobs;

		public ProfessionPrefab(
			ProfessionEnum type,
			List<JobManager.JobEnum> jobs
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.jobs = jobs;
		}
	}

	public ProfessionPrefab GetProfessionFromEnum(ProfessionEnum type) {
		return professionPrefabs.Find(profession => profession.type == type);
	}

	public enum ProfessionPropertyEnum { Profession, Type, Jobs }

	public void CreateColonistProfessions() {
		List<KeyValuePair<string, object>> professionProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/colonist-professions").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> professionProperty in professionProperties) {
			switch ((ProfessionPropertyEnum)Enum.Parse(typeof(ProfessionPropertyEnum), professionProperty.Key)) {
				case ProfessionPropertyEnum.Profession:

					ProfessionEnum? type = null;
					List<JobManager.JobEnum> jobs = new List<JobManager.JobEnum>();

					foreach (KeyValuePair<string, object> professionSubProperty in (List<KeyValuePair<string, object>>)professionProperty.Value) {
						switch ((ProfessionPropertyEnum)Enum.Parse(typeof(ProfessionPropertyEnum), professionSubProperty.Key)) {
							case ProfessionPropertyEnum.Type:
								type = (ProfessionEnum)Enum.Parse(typeof(ProfessionEnum), (string)professionSubProperty.Value);
								break;
							case ProfessionPropertyEnum.Jobs:
								foreach (string jobString in ((string)professionSubProperty.Value).Split(',')) {
									jobs.Add((JobManager.JobEnum)Enum.Parse(typeof(JobManager.JobEnum), jobString));
								}
								break;
							default:
								Debug.LogError("Unknown profession sub property: " + professionSubProperty.Key + " " + professionSubProperty.Value);
								break;
						}

					}

					ProfessionPrefab profession = new ProfessionPrefab(
						type.Value,
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

	public class ProfessionInstance {
		public readonly ProfessionPrefab prefab;
		public readonly Colonist colonist;

		private int priority;

		public ProfessionInstance(
			ProfessionPrefab prefab,
			Colonist colonist,
			int priority
		) {
			this.prefab = prefab;
			this.colonist = colonist;
			this.priority = priority;
		}

		public int GetPriority() {
			return priority;
		}

		public void SetPriority(int priority) {
			if (priority > 9) {
				priority = 0;
			}

			if (priority < 0) {
				priority = 9;
			}

			this.priority = priority;

			GameManager.jobM.UpdateSingleColonistJobs(colonist);
		}

		public void IncreasePriority() {
			SetPriority(priority + 1);
		}

		public void DecreasePriority() {
			SetPriority(priority - 1);
		}
	}

	public enum SkillEnum { Building, Mining, Digging, Farming, Forestry, Crafting };

	public class SkillPrefab {

		public SkillEnum type;
		public string name;

		public readonly Dictionary<JobManager.JobEnum, float> affectedJobTypes = new Dictionary<JobManager.JobEnum, float>();

		public SkillPrefab(List<string> data) {
			type = (SkillEnum)Enum.Parse(typeof(SkillEnum), data[0]);
			name = type.ToString();

			foreach (string affectedJobTypeString in data[1].Split(';')) {
				List<string> affectedJobTypeData = affectedJobTypeString.Split(',').ToList();
				affectedJobTypes.Add((JobManager.JobEnum)Enum.Parse(typeof(JobManager.JobEnum), affectedJobTypeData[0]), float.Parse(affectedJobTypeData[1]));
			}
		}
	}

	public SkillPrefab GetSkillPrefabFromString(string skillTypeString) {
		return skillPrefabs.Find(skillPrefab => skillPrefab.type == (SkillEnum)Enum.Parse(typeof(SkillEnum), skillTypeString));
	}

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
			nextLevelExperience = 100 + (10 * level);
			AddExperience(0);
		}

		public void AddExperience(float amount) {
			currentExperience += amount;
			while (currentExperience >= nextLevelExperience) {
				level += 1;
				currentExperience -= nextLevelExperience;
				nextLevelExperience = 100 + (10 * level);
			}
			GameManager.jobM.UpdateColonistJobCosts(colonist);
			if (GameManager.humanM.selectedHuman == colonist) {
				GameManager.uiM.RemakeSelectedColonistSkills();
			}
		}

		public float CalculateTotalSkillLevel() {
			return level + (currentExperience / nextLevelExperience);
		}
	}

	public readonly List<SkillPrefab> skillPrefabs = new List<SkillPrefab>();

	public void CreateColonistSkills() {
		List<string> stringSkills = Resources.Load<TextAsset>(@"Data/colonist-skills").text.Replace("\n", string.Empty).Replace("\t", string.Empty).Split('`').ToList();
		foreach (string stringSkill in stringSkills) {
			List<string> stringSkillData = stringSkill.Split('/').ToList();
			skillPrefabs.Add(new SkillPrefab(stringSkillData));
		}
		foreach (SkillPrefab skillPrefab in skillPrefabs) {
			skillPrefab.name = UIManager.SplitByCapitals(skillPrefab.name);
		}
	}

	public enum TraitEnum {
		Lazy, Workaholic,
		Alcoholic,
		Antisocial, Socialite,
		Attractive, Unattractive,
		Dieter, Overeater
	};

	public readonly List<TraitPrefab> traitPrefabs = new List<TraitPrefab>();

	public class TraitPrefab {

		public TraitEnum type;
		public string name;

		public float effectAmount;

		public TraitPrefab() {

		}
	}

	public TraitPrefab GetTraitPrefabFromString(string traitTypeString) {
		return traitPrefabs.Find(traitPrefab => traitPrefab.type == (TraitEnum)Enum.Parse(typeof(TraitEnum), traitTypeString));
	}

	public class TraitInstance {

		public Colonist colonist;
		public TraitPrefab prefab;

		public TraitInstance(Colonist colonist, TraitPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}
	}

	public enum NeedEnum { Rest, Water, Food, Temperature, Shelter, Clothing, Safety, Social, Esteem, Relaxation };

	// TODO Add "relatedNeeds" variable to JobPrefab and then find a way to delete this
	private readonly List<JobManager.JobEnum> needRelatedJobs = new List<JobManager.JobEnum>() {
		JobManager.JobEnum.CollectFood, JobManager.JobEnum.Eat,
		JobManager.JobEnum.CollectWater, JobManager.JobEnum.Drink,
		JobManager.JobEnum.Sleep
	};

	// TODO Add "relatedNeeds" variable to JobPrefab and then find a way to delete this
	private readonly Dictionary<JobManager.JobEnum, NeedEnum> jobToNeedMap = new Dictionary<JobManager.JobEnum, NeedEnum>() {
		{ JobManager.JobEnum.Sleep, NeedEnum.Rest },
		{ JobManager.JobEnum.CollectWater, NeedEnum.Water },
		{ JobManager.JobEnum.Drink, NeedEnum.Water },
		{ JobManager.JobEnum.CollectFood, NeedEnum.Food },
		{ JobManager.JobEnum.Eat, NeedEnum.Food }
	};

	public readonly Dictionary<NeedEnum, Func<NeedInstance, float>> needsValueSpecialIncreases = new Dictionary<NeedEnum, Func<NeedInstance, float>>();

	public void InitializeNeedsValueSpecialIncreases() {
		needsValueSpecialIncreases.Add(NeedEnum.Rest, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			if (!GameManager.timeM.isDay) {
				totalSpecialIncrease += 0.05f;
			}
			MoodModifierInstance moodModifier = need.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Rest);
			if (moodModifier.prefab.type == MoodModifierEnum.Rested) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.8f);
			}
			return totalSpecialIncrease;
		});
		needsValueSpecialIncreases.Add(NeedEnum.Water, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = need.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Water);
			if (moodModifier.prefab.type == MoodModifierEnum.Quenched) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.5f);
			}
			return totalSpecialIncrease;
		});
		needsValueSpecialIncreases.Add(NeedEnum.Food, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = need.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Food);
			if (moodModifier.prefab.type == MoodModifierEnum.Stuffed) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.9f);
			} else if (moodModifier.prefab.type == MoodModifierEnum.Full) {
				totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.5f);
			}
			return totalSpecialIncrease;
		});
	}

	public void CalculateNeedValue(NeedInstance need) {
		if (need.colonist.job != null && need.prefab.relatedJobs.Contains(need.colonist.job.prefab.jobType)) {
			return;
		}
		float needIncreaseAmount = need.prefab.baseIncreaseRate;
		foreach (TraitInstance trait in need.colonist.traits) {
			if (need.prefab.traitsAffectingThisNeed.ContainsKey(trait.prefab.type)) {
				needIncreaseAmount *= need.prefab.traitsAffectingThisNeed[trait.prefab.type];
			}
		}
		need.ChangeValue((needIncreaseAmount + (needsValueSpecialIncreases.ContainsKey(need.prefab.type) ? needsValueSpecialIncreases[need.prefab.type](need) : 0)) * GameManager.timeM.deltaTime);
	}

	static ResourceManager.Container FindClosestResourceAmountInContainers(Colonist colonist, ResourceManager.ResourceAmount resourceAmount) {
		
		List<ResourceManager.Container> containersWithResourceAmount = new List<ResourceManager.Container>();

		foreach (ResourceManager.Container container in GameManager.resourceM.GetContainersInRegion(colonist.overTile.region)) {
			if (container.GetInventory().ContainsResourceAmount(resourceAmount)) {
				containersWithResourceAmount.Add(container);
			}
		}
		return containersWithResourceAmount.OrderBy(container => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, container.tile.regionBlock, true, true, false)).FirstOrDefault();
	}

	KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> FindClosestFood(Colonist colonist, float minimumNutritionRequired, bool takeFromOtherColonists, bool eatAnything) {
		List<KeyValuePair<KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>, int>> resourcesPerInventory = new List<KeyValuePair<KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>, int>>();
		int totalNutrition = 0;
		foreach (ResourceManager.Container container in GameManager.resourceM.GetContainersInRegion(colonist.overTile.region).OrderBy(c => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, c.tile.regionBlock, true, true, false))) {
			List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
			foreach (ResourceManager.ResourceAmount ra in container.GetInventory().resources.Where(ra => ra.resource.classes.Contains(ResourceManager.ResourceClassEnum.Food)).OrderBy(ra => ((ResourceManager.Food)ra.resource).nutrition).ToList()) {
				int numReserved = 0;
				for (int i = 0; i < ra.amount; i++) {
					numReserved += 1;
					totalNutrition += ((ResourceManager.Food)ra.resource).nutrition;
					if (totalNutrition >= minimumNutritionRequired) {
						break;
					}
				}
				resourcesToReserve.Add(new ResourceManager.ResourceAmount(ra.resource, numReserved));
				if (totalNutrition >= minimumNutritionRequired) {
					break;
				}
			}
			if (totalNutrition >= minimumNutritionRequired) {
				resourcesPerInventory.Add(new KeyValuePair<KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>, int>(new KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>(container.GetInventory(), resourcesToReserve), totalNutrition));
				break;
			}
		}
		if (takeFromOtherColonists) {
			//print("Take from other colonists.");
		}
		if (resourcesPerInventory.Count > 0) {
			return resourcesPerInventory[0].Key;
		} else {
			return new KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>(null, null);
		}
	}

	public bool GetFood(NeedInstance need, bool takeFromOtherColonists, bool eatAnything) {
		if (need.colonist.GetInventory().resources.Find(ra => ra.resource.groupType == ResourceManager.ResourceGroupEnum.Foods) == null) {
			KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> closestFood = FindClosestFood(need.colonist, need.GetValue(), takeFromOtherColonists, eatAnything);

			List<ResourceManager.ResourceAmount> resourcesToReserve = closestFood.Value;
			if (closestFood.Key != null) {
				if (closestFood.Key.parent is ResourceManager.Container container) {
					container.GetInventory().ReserveResources(resourcesToReserve, need.colonist);
					JobManager.Job job = new JobManager.Job(container.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectFood), null, 0);
					need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, job, null, null));
					return true;
				} else if (closestFood.Key.parent is HumanManager.Human) {
					// TODO
					//Human human = closestFood.Key.human;
					//print("Take from other human.");
				}
			}
		} else {
			need.colonist.SetEatJob();
			return true;
		}
		return false;
	}

	public int FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum resourceGroup, Colonist colonist, bool worldTotal, bool includeOtherColonists) {
		if (worldTotal) {
			int total = 0;
			foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources()) {
				if (resource.groupType == resourceGroup) {
					total += resource.GetWorldTotalAmount();
				}
			}
			return total;
		} else {
			int total = 0;

			int amountOnThisColonist = 0;
			foreach (ResourceManager.ResourceAmount resourceAmount in colonist.GetInventory().resources.Where(ra => ra.resource.groupType == resourceGroup)) {
				amountOnThisColonist += resourceAmount.amount;
			}
			total += amountOnThisColonist;

			int amountUnreservedInContainers = 0;
			foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources().Where(r => r.groupType == resourceGroup)) {
				amountUnreservedInContainers += resource.GetUnreservedContainerTotalAmount();
			}
			total += amountUnreservedInContainers;

			if (includeOtherColonists) {
				int amountOnOtherColonists = 0;
				foreach (Colonist otherColonist in colonists) {
					if (colonist != otherColonist) {
						int amountOnOtherColonist = 0;
						foreach (ResourceManager.ResourceAmount resourceAmount in otherColonist.GetInventory().resources.Where(ra => ra.resource.groupType == resourceGroup)) {
							amountOnOtherColonist += resourceAmount.amount;
						}
						total += amountOnOtherColonist;
					}
				}
				total += amountOnOtherColonists;
			}

			return total;
		}
	}

	public bool GetSleep(NeedInstance need, bool sleepAnywhere) {
		if (GameManager.resourceM.sleepSpots.Count > 0) {
			List<ResourceManager.SleepSpot> validSleepSpots = GameManager.resourceM.sleepSpots.Where(sleepSpot => sleepSpot.occupyingColonist == null && sleepSpot.tile.region == need.colonist.overTile.region).ToList();
			if (validSleepSpots.Count > 0) {
				ResourceManager.SleepSpot chosenSleepSpot = validSleepSpots.OrderByDescending(sleepSpot => sleepSpot.prefab.restComfortAmount / (PathManager.RegionBlockDistance(need.colonist.overTile.regionBlock, sleepSpot.tile.regionBlock, true, true, false) + 1)).ToList()[0];
				chosenSleepSpot.StartSleeping(need.colonist);
				need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(chosenSleepSpot.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Sleep), null, 0), null, null));
				return true;
			}
		}
		if (sleepAnywhere) {
			need.colonist.SetJob(new JobManager.ColonistJob(need.colonist, new JobManager.Job(need.colonist.overTile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Sleep), null, 0), null, null));
			return true;
		}
		return false;
	}

	public static readonly Dictionary<NeedEnum, Func<NeedInstance, bool>> needsValueFunctions = new Dictionary<NeedEnum, Func<NeedInstance, bool>>() {
		{ NeedEnum.Food, delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobEnum.CollectFood || need.colonist.job.prefab.jobType == JobManager.JobEnum.Eat)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * GameManager.timeM.deltaTime);
					if (GameManager.colonistM.FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) { // true, true
						if (GameManager.timeM.minuteChanged) {
							need.colonist.ReturnJob();
							return GameManager.colonistM.GetFood(need, false, false); // true, true - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (GameManager.colonistM.FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) { // false, true
						if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue))) {
							need.colonist.ReturnJob();
							return GameManager.colonistM.GetFood(need, false, false); // true, false - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (GameManager.colonistM.FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) {
							if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue))) {
								need.colonist.ReturnJob();
								return GameManager.colonistM.GetFood(need, false, false);
							}
						}
					}
					return false;
				}
			}
			return false;
		} },
		{ NeedEnum.Water, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobEnum.CollectWater || need.colonist.job.prefab.jobType == JobManager.JobEnum.Drink)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * GameManager.timeM.deltaTime);
					// TODO Find Water Here (Maximum Priority)
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					// TODO Find Water Here (High Priority)
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						// TODO Find Water Here (Low Priority)
					}
					return false;
				}
			}
			return false;
		} },
		{ NeedEnum.Rest, delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.prefab.jobType == JobManager.JobEnum.Sleep)) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate * GameManager.timeM.deltaTime);
					if (GameManager.timeM.minuteChanged) {
						need.colonist.ReturnJob();
						GameManager.colonistM.GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue))) {
						need.colonist.ReturnJob();
						GameManager.colonistM.GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (GameManager.timeM.minuteChanged && UnityEngine.Random.Range(0f, 1f) < ((need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue))) {
							need.colonist.ReturnJob();
							GameManager.colonistM.GetSleep(need, false);
						}

					}
					return false;
				}
			}
			return false;
		} },
		{ NeedEnum.Clothing, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedEnum.Shelter, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedEnum.Temperature, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedEnum.Safety, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedEnum.Social, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedEnum.Esteem, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} },
		{ NeedEnum.Relaxation, delegate (NeedInstance need) {
			need.SetValue(0); // Value set to 0 while need not being used
			return false;
		} }
	};

	/*

	1440 seconds per in-game day
	@ 0.01 BRI ... 14.4 points per in-game day
	@ 0.05 BRI ... 72 points per in-game day
	@ 0.1 BRI ... 144 points per in-game day

	Convert days to points between 0-100
	(days * 1440) * BRI = pointsAfterDays

	Convert points between 0-100 to days
	(pointsAfterDays / BRI) / 1440 = days

	0	NeedName/
	1	0.01/		BRI			Base rate of increase per second while conditions met
	2	false/		AMinVB		Whether there is any action taken at MinV
	3	0/			MinV		No action
	4	false/		AMaxVB		Whether there is any action taken at MaxV
	5	0/			MaxV		No action
	6	false/		ACVB		Whether there is any action taken at CV
	7	0/			CV			Value until they begin dying from the need not being fulfilled
	8	false/		DB			Whether they can die from this need not being fulfilled
	9	0.0/		DR			Base rate of health loss due to the need not being fulfilled
	10	100/		ClampV		Value above which the food value will be clamped
	11	0/			Priority	The priority of fulfilling the requirements of this need over others
	12	2/			Number of Affected Traits
	13	TraitName,TraitName/	Names of the affected traits
	14	1.1,1.1`	Multiplier of the BRI for each trait

	*/

	public readonly List<NeedPrefab> needPrefabs = new List<NeedPrefab>();

	public class NeedPrefab {

		public NeedEnum type;
		public string name;

		public float baseIncreaseRate;

		public bool minValueAction;
		public float minValue;

		public bool maxValueAction;
		public float maxValue;

		public bool critValueAction;
		public float critValue;

		public bool canDie;
		public float healthDecreaseRate;

		public int clampValue;

		public int priority;

		public readonly Dictionary<TraitEnum, float> traitsAffectingThisNeed = new Dictionary<TraitEnum, float>();

		public readonly List<JobManager.JobEnum> relatedJobs = new List<JobManager.JobEnum>();

		public NeedPrefab(
			NeedEnum type,
			float baseIncreaseRate,
			bool minValueAction,
			float minValue,
			bool maxValueAction,
			float maxValue,
			bool critValueAction,
			float critValue,
			bool canDie,
			float healthDecreaseRate,
			int clampValue,
			int priority,
			Dictionary<TraitEnum, float> traitsAffectingThisNeed,
			List<JobManager.JobEnum> relatedJobs
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.baseIncreaseRate = baseIncreaseRate;

			this.minValueAction = minValueAction;
			this.minValue = minValue;

			this.maxValueAction = maxValueAction;
			this.maxValue = maxValue;

			this.critValueAction = critValueAction;
			this.critValue = critValue;

			this.canDie = canDie;
			this.healthDecreaseRate = healthDecreaseRate;

			this.clampValue = clampValue;

			this.priority = priority;

			this.traitsAffectingThisNeed = traitsAffectingThisNeed;

			this.relatedJobs = relatedJobs;
		}
	}

	public NeedPrefab GetNeedPrefabFromEnum(NeedEnum needsEnumValue) {
		return needPrefabs.Find(needPrefab => needPrefab.type == needsEnumValue);
	}

	public NeedPrefab GetNeedPrefabFromString(string needTypeString) {
		return needPrefabs.Find(needPrefab => needPrefab.type == (NeedEnum)Enum.Parse(typeof(NeedEnum), needTypeString));
	}

	public class NeedInstance {

		public Colonist colonist;
		public NeedPrefab prefab;
		private float value = 0;
		private int roundedValue = 0;

		public NeedInstance(Colonist colonist, NeedPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}

		public float GetValue() {
			return value;
		}

		public void SetValue(float newValue) {
			float oldValue = value;
			value = newValue;
			Mathf.Clamp(value, 0, prefab.clampValue);
			roundedValue = Mathf.RoundToInt((value / prefab.clampValue) * 100);
			if (GameManager.humanM.selectedHuman == colonist && Mathf.RoundToInt((oldValue / prefab.clampValue) * 100) != roundedValue) {
				GameManager.uiM.RemakeSelectedColonistNeeds();
			}
		}

		public void ChangeValue(float amount) {
			SetValue(value + amount);
		}

		public int GetRoundedValue() {
			return roundedValue;
		}
	}

	public void CreateColonistNeeds() {
		List<string> needDataStringList = Resources.Load<TextAsset>(@"Data/colonist-needs").text.Replace("\t", string.Empty).Split(new string[] { "<Need>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string singleNeedDataString in needDataStringList) {

			NeedEnum type = NeedEnum.Rest;
			float baseIncreaseRate = 0;
			bool minValueAction = false;
			float minValue = 0;
			bool maxValueAction = false;
			float maxValue = 0;
			bool critValueAction = false;
			float critValue = 0;
			bool canDie = false;
			float healthDecreaseRate = 0;
			int clampValue = 0;
			int priority = 0;
			Dictionary<TraitEnum, float> traitsAffectingThisNeed = new Dictionary<TraitEnum, float>();
			List<JobManager.JobEnum> relatedJobs = new List<JobManager.JobEnum>();

			List<string> singleNeedDataLineStringList = singleNeedDataString.Split('\n').ToList();
			foreach (string singleNeedDataLineString in singleNeedDataLineStringList.Skip(1)) {
				if (!string.IsNullOrEmpty(singleNeedDataLineString)) {

					string label = singleNeedDataLineString.Split('>')[0].Replace("<", string.Empty);
					string value = singleNeedDataLineString.Split('>')[1];

					switch (label) {
						case "Type":
							type = (NeedEnum)Enum.Parse(typeof(NeedEnum), value);
							break;
						case "BaseIncreaseRate":
							baseIncreaseRate = float.Parse(value);
							break;
						case "MinValueAction":
							minValueAction = bool.Parse(value);
							break;
						case "MinValue":
							minValue = float.Parse(value);
							break;
						case "MaxValueAction":
							maxValueAction = bool.Parse(value);
							break;
						case "MaxValue":
							maxValue = float.Parse(value);
							break;
						case "CritValueAction":
							critValueAction = bool.Parse(value);
							break;
						case "CritValue":
							critValue = float.Parse(value);
							break;
						case "CanDie":
							canDie = bool.Parse(value);
							break;
						case "HealthDecreaseRate":
							healthDecreaseRate = float.Parse(value);
							break;
						case "ClampValue":
							clampValue = int.Parse(value);
							break;
						case "Priority":
							priority = int.Parse(value);
							break;
						case "TraitsAffectingThisNeed":
							if (!string.IsNullOrEmpty(UIManager.RemoveNonAlphanumericChars(value))) {
								foreach (string traitsAffectingThisNeedString in value.Split(',')) {
									TraitEnum traitEnum = (TraitEnum)Enum.Parse(typeof(TraitEnum), traitsAffectingThisNeedString.Split(':')[0]);
									float multiplier = float.Parse(traitsAffectingThisNeedString.Split(':')[1]);
									traitsAffectingThisNeed.Add(traitEnum, multiplier);
								}
							}
							break;
						case "RelatedJobs":
							if (!string.IsNullOrEmpty(UIManager.RemoveNonAlphanumericChars(value))) {
								foreach (string relatedJobString in value.Split(',')) {
									JobManager.JobEnum jobTypeEnum = (JobManager.JobEnum)Enum.Parse(typeof(JobManager.JobEnum), relatedJobString);
									relatedJobs.Add(jobTypeEnum);
								}
							}
							break;
						default:
							MonoBehaviour.print("Unknown need label: \"" + singleNeedDataLineString + "\"");
							break;
					}
				}
			}

			needPrefabs.Add(new NeedPrefab(type, baseIncreaseRate, minValueAction, minValue, maxValueAction, maxValue, critValueAction, critValue, canDie, healthDecreaseRate, clampValue, priority, traitsAffectingThisNeed, relatedJobs));
		}
	}

	public static readonly Dictionary<MoodModifierGroupEnum, Action<Colonist>> moodModifierFunctions = new Dictionary<MoodModifierGroupEnum, Action<Colonist>>() {
		{ MoodModifierGroupEnum.Death, delegate (Colonist colonist) {
			// TODO Implement colonists viewing deaths and being sad
		} },
		{ MoodModifierGroupEnum.Rest, delegate (Colonist colonist) {
			NeedInstance restNeed = colonist.needs.Find(ni => ni.prefab.type == NeedEnum.Rest);
			if (restNeed.GetValue() >= restNeed.prefab.maxValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Exhausted);
			} else if (restNeed.GetValue() >= restNeed.prefab.minValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Tired);
			} else {
				colonist.RemoveMoodModifier(MoodModifierEnum.Exhausted);
				colonist.RemoveMoodModifier(MoodModifierEnum.Tired);
			}
		} },
		{ MoodModifierGroupEnum.Water, delegate (Colonist colonist) {
			NeedInstance waterNeed = colonist.needs.Find(ni => ni.prefab.type == NeedEnum.Water);
			if (waterNeed.GetValue() >= waterNeed.prefab.maxValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Dehydrated);
			} else if (waterNeed.GetValue() >= waterNeed.prefab.minValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Thirsty);
			} else {
				colonist.RemoveMoodModifier(MoodModifierEnum.Dehydrated);
				colonist.RemoveMoodModifier(MoodModifierEnum.Thirsty);
			}
		} },
		{ MoodModifierGroupEnum.Food, delegate (Colonist colonist) {
			NeedInstance foodNeed = colonist.needs.Find(ni => ni.prefab.type == NeedEnum.Food);
			if (foodNeed.GetValue() >= foodNeed.prefab.maxValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Starving);
			} else if (foodNeed.GetValue() >= foodNeed.prefab.minValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Hungry);
			} else {
				colonist.RemoveMoodModifier(MoodModifierEnum.Starving);
				colonist.RemoveMoodModifier(MoodModifierEnum.Hungry);
			}
		} },
		{ MoodModifierGroupEnum.Inventory, delegate (Colonist colonist) {
			if (colonist.GetInventory().UsedWeight() > colonist.GetInventory().maxWeight) {
				colonist.AddMoodModifier(MoodModifierEnum.Overencumbered);
			} else {
				colonist.RemoveMoodModifier(MoodModifierEnum.Overencumbered);
			}
		} }
	};

	public void CreateMoodModifiers() {
		List<string> stringMoodModifierGroups = Resources.Load<TextAsset>(@"Data/mood-modifiers").text.Replace("\t", string.Empty).Split(new string[] { "<MoodModifierGroup>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		foreach (string stringMoodModifierGroup in stringMoodModifierGroups) {
			MoodModifierGroup moodModifierGroup = new MoodModifierGroup(stringMoodModifierGroup);
			moodModifierGroups.Add(moodModifierGroup);
			moodModifierPrefabs.AddRange(moodModifierGroup.prefabs);
		}
	}

	public enum MoodModifierGroupEnum {
		Death,
		Food,
		Water,
		Rest,
		Inventory
	};
	public enum MoodModifierEnum {
		WitnessDeath,
		Stuffed, Full, Hungry, Starving, AteOnFloor, AteWithoutTable,
		Dehydrated, Thirsty, Quenched,
		Rested, Tired, Exhausted,
		Overencumbered
	};

	public readonly List<MoodModifierGroup> moodModifierGroups = new List<MoodModifierGroup>();

	public class MoodModifierGroup {
		public MoodModifierGroupEnum type;
		public string name;

		public readonly List<MoodModifierPrefab> prefabs = new List<MoodModifierPrefab>();

		public MoodModifierGroup(string stringMoodModifierGroup) {
			List<string> stringMoodModifiers = stringMoodModifierGroup.Split(new string[] { "<MoodModifier>" }, StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (MoodModifierGroupEnum)Enum.Parse(typeof(MoodModifierGroupEnum), stringMoodModifiers[0]);
			name = UIManager.SplitByCapitals(type.ToString());

			foreach (string stringMoodModifier in stringMoodModifiers.Skip(1)) {
				prefabs.Add(new MoodModifierPrefab(stringMoodModifier, this));
			}
		}
	}

	public MoodModifierGroup GetMoodModifierGroupFromEnum(MoodModifierGroupEnum moodModifierGroupEnum) {
		return moodModifierGroups.Find(mmiGroup => mmiGroup.type == moodModifierGroupEnum);
	}

	public readonly List<MoodModifierPrefab> moodModifierPrefabs = new List<MoodModifierPrefab>();

	public class MoodModifierPrefab {

		public MoodModifierEnum type;
		public string name = string.Empty;

		public MoodModifierGroup group = null;

		public int effectAmount = 0;

		public int effectLengthSeconds = 0;

		public bool infinite = false;

		public MoodModifierPrefab(string stringMoodModifier, MoodModifierGroup group) {
			this.group = group;

			List<string> stringMoodModifierList = stringMoodModifier.Split('\n').ToList();
			foreach (string stringMoodModifierSingle in stringMoodModifierList.Skip(1)) {

				if (!string.IsNullOrEmpty(stringMoodModifierSingle)) {

					string label = stringMoodModifierSingle.Split('>')[0].Replace("<", string.Empty);
					string value = stringMoodModifierSingle.Split('>')[1];

					switch (label) {
						case "Type":
							type = (MoodModifierEnum)Enum.Parse(typeof(MoodModifierEnum), value);
							name = UIManager.SplitByCapitals(type.ToString());
							break;
						case "EffectAmount":
							effectAmount = int.Parse(value);
							break;
						case "EffectLengthSeconds":
							infinite = UIManager.RemoveNonAlphanumericChars(value) == "UntilNot";
							if (infinite) {
								effectLengthSeconds = int.MaxValue;
							} else {
								effectLengthSeconds = int.Parse(value);
							}
							break;
						default:
							MonoBehaviour.print("Unknown mood modifier label: \"" + stringMoodModifierSingle + "\"");
							break;
					}
				}
			}

			if (string.IsNullOrEmpty(name) || effectAmount == 0 || effectLengthSeconds == 0) {
				MonoBehaviour.print("Potential issue parsing mood modifier: " + stringMoodModifier);
			}
		}
	}

	public MoodModifierPrefab GetMoodModifierPrefabFromEnum(MoodModifierEnum moodModifierEnum) {
		return moodModifierPrefabs.Find(moodModifierPrefab => moodModifierPrefab.type == moodModifierEnum);
	}

	public MoodModifierPrefab GetMoodModifierPrefabFromString(string moodModifierTypeString) {
		return moodModifierPrefabs.Find(moodModifierPrefab => moodModifierPrefab.type == (MoodModifierEnum)Enum.Parse(typeof(MoodModifierEnum), moodModifierTypeString));
	}

	public class MoodModifierInstance {
		public Colonist colonist;
		public MoodModifierPrefab prefab;

		public float timer = 0;

		public MoodModifierInstance(Colonist colonist, MoodModifierPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;

			timer = prefab.effectLengthSeconds;
		}

		public void Update() {
			timer -= 1 * GameManager.timeM.deltaTime;
		}
	}

	public readonly List<Colonist> colonists = new List<Colonist>();

	private readonly List<Colonist> deadColonists = new List<Colonist>();

	public class Colonist : HumanManager.Human {

		public bool playerMoved;

		// Job
		public JobManager.Job job;
		public JobManager.Job storedJob;
		public JobManager.Job needJob;
		public readonly List<JobManager.Job> backlog = new List<JobManager.Job>();

		// Professions
		public readonly List<ProfessionInstance> professions = new List<ProfessionInstance>();

		// Skills
		public readonly List<SkillInstance> skills = new List<SkillInstance>();

		// Traits
		public readonly List<TraitInstance> traits = new List<TraitInstance>();

		// Needs
		public readonly List<NeedInstance> needs = new List<NeedInstance>();

		// Mood
		public float baseMood = 100;
		public float moodModifiersSum = 100;
		public float effectiveMood = 100;
		public readonly List<MoodModifierInstance> moodModifiers = new List<MoodModifierInstance>();

		public Colonist(TileManager.Tile spawnTile, float startingHealth) : base(spawnTile, startingHealth) {
			obj.transform.SetParent(GameManager.resourceM.colonistParent.transform, false);

			foreach (ProfessionPrefab professionPrefab in GameManager.colonistM.professionPrefabs) {
				professions.Add(new ProfessionInstance(
						professionPrefab, 
						this, 
						Mathf.RoundToInt(GameManager.colonistM.professionPrefabs.Count / 2f)
				));
			}

			foreach (SkillPrefab skillPrefab in GameManager.colonistM.skillPrefabs) {
				skills.Add(new SkillInstance(
						this, 
						skillPrefab, 
						true, 
						0
				));
			}

			foreach (NeedPrefab needPrefab in GameManager.colonistM.needPrefabs) {
				needs.Add(new NeedInstance(
						this, 
						needPrefab
				));
			}
			needs = needs.OrderBy(need => need.prefab.priority).ToList();

			GameManager.colonistM.colonists.Add(this);
		}

		public override void Update() {
			moveSpeedMultiplier = (-((GetInventory().UsedWeight() - GetInventory().maxWeight) / (float)GetInventory().maxWeight)) + 1;
			moveSpeedMultiplier = Mathf.Clamp(moveSpeedMultiplier, 0.1f, 1f);

			if (playerMoved && path.Count <= 0) {
				playerMoved = false;
			}

			base.Update();

			if (dead) {
				return;
			}

			UpdateNeeds();
			UpdateMoodModifiers();
			UpdateMood();

			if (overTileChanged) {
				GameManager.jobM.UpdateColonistJobCosts(this);
			}

			if (job != null) {
				if (!job.started && overTile == job.tile) {
					StartJob();
				}
				if (job.started && overTile == job.tile && !Mathf.Approximately(job.jobProgress, 0)) {
					WorkJob();
				}
			}
			if (job == null) {
				if (path.Count <= 0) {
					List<ResourceManager.Container> validEmptyInventoryContainers = FindValidContainersToEmptyInventory();
					int inventoryWeight = GetInventory().UsedWeight();
					int inventoryVolume = GetInventory().UsedVolume();
					if (validEmptyInventoryContainers.Count > 0 
							&& (((inventoryWeight >= GetInventory().maxWeight) || (inventoryVolume >= GetInventory().maxVolume)) 
								|| ((inventoryWeight > 0 || inventoryVolume > 0) && GameManager.jobM.GetColonistJobsCountForColonist(this) <= 0))
					) {
						EmptyInventory(validEmptyInventoryContainers);
					} else {
						Wander(null, 0);
					}
				} else {
					wanderTimer = UnityEngine.Random.Range(10f, 20f);
				}
			}
		}

		public override void Die() {
			base.Die();

			ReturnJob();
			GameManager.colonistM.colonists.Remove(this);
			GameManager.uiM.SetColonistElements();
			GameManager.uiM.SetJobElements();
			foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.humanM.SetSelectedHuman(null);
			}
			GameManager.jobM.UpdateAllColonistJobCosts();
		}

		public List<ResourceManager.Container> FindValidContainersToEmptyInventory() {
			return GameManager.resourceM.GetContainersInRegion(overTile.region).Where(container => container.GetInventory().UsedWeight() < container.GetInventory().maxWeight && container.GetInventory().UsedVolume() < container.GetInventory().maxVolume).ToList();
		}

		public void EmptyInventory(List<ResourceManager.Container> validContainers) {
			if (GetInventory().UsedWeight() > 0 && GetInventory().UsedVolume() > 0 && validContainers.Count > 0) {
				ReturnJob();
				ResourceManager.Container closestContainer = validContainers.OrderBy(container => PathManager.RegionBlockDistance(container.tile.regionBlock, overTile.regionBlock, true, true, false)).ToList()[0];
				SetJob(new JobManager.ColonistJob(this, new JobManager.Job(closestContainer.tile, GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.EmptyInventory), null, 0), null, null));
			}
		}

		private void UpdateNeeds() {
			foreach (NeedInstance need in needs) {
				GameManager.colonistM.CalculateNeedValue(need);
				bool checkNeed = false;
				if (need.colonist.job == null) {
					checkNeed = true;
				} else {
					if (GameManager.colonistM.needRelatedJobs.Contains(need.colonist.job.prefab.jobType)) {
						if (GameManager.colonistM.jobToNeedMap.ContainsKey(need.colonist.job.prefab.jobType)) {
							if (need.prefab.priority < GameManager.colonistM.GetNeedPrefabFromEnum(GameManager.colonistM.jobToNeedMap[need.colonist.job.prefab.jobType]).priority) {
								checkNeed = true;
							}
						} else {
							checkNeed = true;
						}
					} else {
						checkNeed = true;
					}
				}
				if (checkNeed) {
					if (needsValueFunctions[need.prefab.type](need)) {
						break;
					}
				}
			}
		}

		public void UpdateMoodModifiers() {
			foreach (MoodModifierGroup moodModifierGroup in GameManager.colonistM.moodModifierGroups) {
				moodModifierFunctions[moodModifierGroup.type](this);
			}

			for (int i = 0; i < moodModifiers.Count; i++) {
				MoodModifierInstance moodModifier = moodModifiers[i];
				moodModifier.Update();
				if (moodModifier.timer <= 0) {
					RemoveMoodModifier(moodModifier.prefab.type);
					i -= 1;
				}
			}
		}

		public void AddMoodModifier(MoodModifierEnum moodModifierEnum) {
			MoodModifierInstance moodModifier = new MoodModifierInstance(this, GameManager.colonistM.GetMoodModifierPrefabFromEnum(moodModifierEnum));
			MoodModifierInstance sameGroupMoodModifier = moodModifiers.Find(findMoodModifier => moodModifier.prefab.group.type == findMoodModifier.prefab.group.type);
			if (sameGroupMoodModifier != null) {
				RemoveMoodModifier(sameGroupMoodModifier.prefab.type);
			}
			moodModifiers.Add(moodModifier);
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.uiM.RemakeSelectedColonistMoodModifiers();
			}
		}

		public void RemoveMoodModifier(MoodModifierEnum moodModifierEnum) {
			moodModifiers.Remove(moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.type == moodModifierEnum));
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.uiM.RemakeSelectedColonistMoodModifiers();
			}
		}

		public void UpdateMood() {
			baseMood = Mathf.Clamp(Mathf.RoundToInt(100 - (needs.Sum(need => (need.GetValue() / (need.prefab.priority + 1))))), 0, 100);

			moodModifiersSum = moodModifiers.Sum(hM => hM.prefab.effectAmount);

			float targetMood = Mathf.Clamp(baseMood + moodModifiersSum, 0, 100);
			float moodChangeAmount = ((targetMood - effectiveMood) / (effectiveMood <= 0f ? 1f : effectiveMood));
			effectiveMood += moodChangeAmount * GameManager.timeM.deltaTime;
			effectiveMood = Mathf.Clamp(effectiveMood, 0, 100);
		}

		public void SetJob(JobManager.ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			job = colonistJob.job;
			job.resourcesColonistHas = colonistJob.resourcesColonistHas;
			job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && (job.containerPickups != null && job.containerPickups.Count > 0)) {
				foreach (JobManager.ContainerPickup containerPickup in job.containerPickups) {
					containerPickup.container.GetInventory().ReserveResources(containerPickup.resourcesToPickup, this);
				}
			}
			if (job.transferResources != null && job.transferResources.Count > 0) {
				ResourceManager.Container collectContainer = (ResourceManager.Container)job.tile.GetAllObjectInstances().Find(oi => oi is ResourceManager.Container);
				if (collectContainer != null) {
					collectContainer.GetInventory().ReserveResources(job.transferResources, this);
				}
			}
			job.SetColonist(this);
			MoveToTile(job.tile, !job.tile.walkable);
			GameManager.uiM.SetJobElements();
		}

		public void SetEatJob() {

			// Find a chair (ideally next to a table) for the colonist to sit at to eat
			List<ResourceManager.ObjectInstance> chairs = new List<ResourceManager.ObjectInstance>();
			foreach (ResourceManager.ObjectPrefab chairPrefab in GameManager.resourceM.GetObjectPrefabSubGroupByEnum(ResourceManager.ObjectSubGroupEnum.Chairs).prefabs) {
				chairs.AddRange(GameManager.resourceM.GetObjectInstancesByPrefab(chairPrefab));
			}
			chairs.Select(chair => chair.tile.region == overTile.region);
			chairs.OrderBy(chair => PathManager.RegionBlockDistance(overTile.regionBlock, chair.tile.regionBlock, true, true, false));
			chairs.OrderByDescending(chair => chair.tile.surroundingTiles.Find(surroundingTile => {
				ResourceManager.ObjectInstance tableNextToChair = surroundingTile.GetObjectInstanceAtLayer(2);
				if (tableNextToChair != null) {
						return tableNextToChair.prefab.subGroupType == ResourceManager.ObjectSubGroupEnum.Tables;
				}
				return false;
			}) != null);

			SetJob(
				new JobManager.ColonistJob(
					this, 
					new JobManager.Job(
						chairs.Count > 0 ? chairs[0].tile : overTile,
						GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Eat), 
						null, 
						0
					), 
					null, 
					null
				)
			);
		}

		public void StartJob() {
			job.started = true;

			job.jobProgress *= (1 + (1 - GetJobSkillMultiplier(job.prefab.jobType)));

			if (job.prefab.jobType == JobManager.JobEnum.Eat) {
				job.jobProgress += needs.Find(need => need.prefab.type == NeedEnum.Food).GetValue();
			}
			if (job.prefab.jobType == JobManager.JobEnum.Sleep) {
				job.jobProgress += 20f * (needs.Find(need => need.prefab.type == NeedEnum.Rest).GetValue());
			}

			job.colonistBuildTime = job.jobProgress;

			GameManager.uiM.SetJobElements();
		}

		public void WorkJob() {

			if (job.prefab.jobType == JobManager.JobEnum.HarvestFarm && job.tile.farm == null) {
				job.Remove();
				job = null;
				return;
			} else if (
				job.prefab.jobType == JobManager.JobEnum.EmptyInventory ||
				job.prefab.jobType == JobManager.JobEnum.CollectFood ||
				job.prefab.jobType == JobManager.JobEnum.PickupResources) {

				ResourceManager.Container containerOnTile = GameManager.resourceM.containers.Find(container => container.tile == job.tile);
				if (containerOnTile == null) {
					job.Remove();
					job = null;
					return;
				}
			} else if (job.prefab.jobType == JobManager.JobEnum.Sleep) {
				float currentRestValue = needs.Find(need => need.prefab.type == NeedEnum.Rest).GetValue();
				float originalRestValue = currentRestValue / (job.jobProgress / job.colonistBuildTime);
				needs.Find(need => need.prefab.type == NeedEnum.Rest).SetValue(originalRestValue * ((job.jobProgress - 1f * GameManager.timeM.deltaTime) / job.colonistBuildTime));
			}

			if (job.activeObject != null) {
				job.activeObject.SetActiveSprite(job, true);
			}

			job.jobProgress -= 1 * GameManager.timeM.deltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress, 0)) {
				job.jobProgress = 0;
				FinishJob();
				return;
			}

			job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, ((job.colonistBuildTime - job.jobProgress) / job.colonistBuildTime));
		}

		public void FinishJob() {
			JobManager.Job finishedJob = job;
			job = null;

			MonoBehaviour.Destroy(finishedJob.jobPreview);
			if (finishedJob.prefab.addToTileWhenBuilt) {
				finishedJob.tile.SetObject(GameManager.resourceM.CreateObjectInstance(finishedJob.prefab, finishedJob.variation, finishedJob.tile, finishedJob.rotationIndex, true));
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).FinishCreation();
				if (finishedJob.prefab.canRotate) {
					finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.prefab.layer).obj.GetComponent<SpriteRenderer>().sprite = finishedJob.prefab.GetBitmaskSpritesForVariation(finishedJob.variation)[finishedJob.rotationIndex];
				}
			}

			SkillInstance skill = GetSkillFromJobType(finishedJob.prefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJob.prefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			JobManager.finishJobFunctions[finishedJob.prefab.jobType](this, finishedJob);

			if (finishedJob.activeObject != null) {
				finishedJob.activeObject.SetActiveSprite(finishedJob, false);
			}

			GameManager.jobM.UpdateSingleColonistJobs(this);
			GameManager.uiM.SetJobElements();
			GameManager.uiM.UpdateSelectedColonistInformation();
			GameManager.uiM.UpdateSelectedContainerInfo();
			GameManager.uiM.UpdateSelectedTradingPostInfo();
			GameManager.uiM.UpdateTileInformation();

			if (storedJob != null) {
				Update();
			}
		}

		public void ReturnJob() {
			foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (storedJob != null) {
				if (GameManager.colonistM.jobToNeedMap.ContainsKey(storedJob.prefab.jobType) || JobManager.nonReturnableJobs.Contains(storedJob.prefab.jobType)) {
					storedJob.Remove();
					storedJob = null;
					if (job != null) {
						if (GameManager.colonistM.jobToNeedMap.ContainsKey(job.prefab.jobType) || JobManager.nonReturnableJobs.Contains(job.prefab.jobType)) {
							job.Remove();
						}
						job = null;
					}
					return;
				}
				GameManager.jobM.AddExistingJob(storedJob);
				if (storedJob.jobUIElement != null) {
					storedJob.jobUIElement.Remove();
				}
				if (storedJob.jobPreview != null) {
					storedJob.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				storedJob = null;
				if (job != null) {
					if (job.jobUIElement != null) {
						job.jobUIElement.Remove();
					}
					if (job.jobPreview != null) {
						job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
					}
					job = null;
				}
			} else if (job != null) {
				if (GameManager.colonistM.jobToNeedMap.ContainsKey(job.prefab.jobType) || JobManager.nonReturnableJobs.Contains(job.prefab.jobType)) {
					job.Remove();
					if (job.jobUIElement != null) {
						job.jobUIElement.Remove();
					}
					job = null;
					return;
				}
				GameManager.jobM.AddExistingJob(job);
				if (job.jobUIElement != null) {
					job.jobUIElement.Remove();
				}
				if (job.jobPreview != null) {
					job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				job = null;
			}
		}

		public void MoveToClosestWalkableTile(bool careIfOvertileIsWalkable) {
			if (!careIfOvertileIsWalkable || !overTile.walkable) {
				List<TileManager.Tile> walkableSurroundingTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable).ToList();
				if (walkableSurroundingTiles.Count > 0) {
					MoveToTile(walkableSurroundingTiles[UnityEngine.Random.Range(0, walkableSurroundingTiles.Count)], false);
				} else {
					walkableSurroundingTiles.Clear();
					List<TileManager.Tile> potentialWalkableSurroundingTiles = new List<TileManager.Tile>();
					foreach (TileManager.Map.RegionBlock regionBlock in overTile.regionBlock.horizontalSurroundingRegionBlocks) {
						if (regionBlock.tileType.walkable) {
							potentialWalkableSurroundingTiles.AddRange(regionBlock.tiles);
						}
					}
					walkableSurroundingTiles = potentialWalkableSurroundingTiles.Where(tile => tile.surroundingTiles.Find(nTile => !nTile.walkable && nTile.regionBlock == overTile.regionBlock) != null).ToList();
					if (walkableSurroundingTiles.Count > 0) {
						walkableSurroundingTiles = walkableSurroundingTiles.OrderBy(tile => Vector2.Distance(tile.obj.transform.position, overTile.obj.transform.position)).ToList();
						MoveToTile(walkableSurroundingTiles[0], false);
					} else {
						List<TileManager.Tile> validTiles = GameManager.colonyM.colony.map.tiles.Where(tile => tile.walkable).OrderBy(tile => Vector2.Distance(tile.obj.transform.position, overTile.obj.transform.position)).ToList();
						if (validTiles.Count > 0) {
							MoveToTile(validTiles[0], false);
						}
					}
				}
			}
		}

		public void PlayerMoveToTile(TileManager.Tile tile) {
			playerMoved = true;
			ReturnJob();
			if (job != null) {
				MoveToTile(tile, false);
			} else {
				MoveToTile(tile, false);
			}
		}

		public ProfessionInstance GetProfessionFromEnum(ProfessionEnum type) {
			return professions.Find(p => p.prefab.type == type);
		}

		public SkillInstance GetSkillFromEnum(SkillEnum type) {
			return skills.Find(s => s.prefab.type == type);
		}

		public SkillInstance GetSkillFromJobType(JobManager.JobEnum jobType) {
			return skills.Find(s => s.prefab.affectedJobTypes.ContainsKey(jobType));
		}

		public float GetJobSkillMultiplier(JobManager.JobEnum jobType) {
			SkillInstance skill = GetSkillFromJobType(jobType);
			if (skill != null) {
				return 1 * (-(1f / (((skill.prefab.affectedJobTypes[jobType]) * (skill.level)) + 1)) + 1);
			}
			return 1.0f;
		}

		public override void SetName(string name) {
			base.SetName(name);
			SetNameColour(UIManager.GetColour(UIManager.Colours.LightGreen100));
		}

		public override void ChangeClothing(Appearance appearance, ResourceManager.Clothing clothing) {

			if (clothing == null || GetInventory().ContainsResourceAmount(new ResourceManager.ResourceAmount(clothing, 1))) {

				base.ChangeClothing(appearance, clothing);
			
			} else {

				ResourceManager.Container container = FindClosestResourceAmountInContainers(this, new ResourceManager.ResourceAmount(clothing, 1));

				if (container != null) {

					ResourceManager.ResourceAmount clothingToPickup = new ResourceManager.ResourceAmount(clothing, 1);

					container.GetInventory().ReserveResources(
						new List<ResourceManager.ResourceAmount>() {
							clothingToPickup
						},
						this
					);

					backlog.Add(
						new JobManager.Job(
							container.tile,
							GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.WearClothes),
							null,
							0
						) {
							resourcesToBuild = new List<ResourceManager.ResourceAmount>() { clothingToPickup },
							resourcesColonistHas = new List<ResourceManager.ResourceAmount>(),
							containerPickups = new List<JobManager.ContainerPickup>() {
								new JobManager.ContainerPickup(
									container,
									new List<ResourceManager.ResourceAmount>() { clothingToPickup }
								)
							}
						}
					);
				}
			}
		}
	}

	public void SpawnStartColonists(int amount) {
		SpawnColonists(amount);

		Vector2 averageColonistPosition = new Vector2(0, 0);
		foreach (Colonist colonist in colonists) {
			averageColonistPosition = new Vector2(averageColonistPosition.x + colonist.obj.transform.position.x, averageColonistPosition.y + colonist.obj.transform.position.y);
		}
		averageColonistPosition /= colonists.Count;
		GameManager.cameraM.SetCameraPosition(averageColonistPosition);

		// TEMPORARY COLONIST TESTING STUFF
		colonists[UnityEngine.Random.Range(0, colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.WheatSeed), UnityEngine.Random.Range(5, 11), false);
		colonists[UnityEngine.Random.Range(0, colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.Potato), UnityEngine.Random.Range(5, 11), false);
		colonists[UnityEngine.Random.Range(0, colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.CottonSeed), UnityEngine.Random.Range(5, 11), false);
	}

	public void SpawnColonists(int amount) {
		if (amount > 0) {
			int mapSize = GameManager.colonyM.colony.map.mapData.mapSize;
			for (int i = 0; i < amount; i++) {
				List<TileManager.Tile> walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && o.buildable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
				if (walkableTilesByDistanceToCentre.Count <= 0) {
					foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles.Where(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
						tile.SetTileType(tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Ground], true, true, true);
					}
					walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
				}

				List<TileManager.Tile> validSpawnTiles = new List<TileManager.Tile>();
				TileManager.Tile currentTile = walkableTilesByDistanceToCentre[0];
				float minimumDistance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
				foreach (TileManager.Tile tile in walkableTilesByDistanceToCentre) {
					float distance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
					if (distance < minimumDistance) {
						currentTile = tile;
						minimumDistance = distance;
					}
				}
				List<TileManager.Tile> frontier = new List<TileManager.Tile>() { currentTile };
				List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>();
				while (frontier.Count > 0) {
					currentTile = frontier[0];
					frontier.RemoveAt(0);
					checkedTiles.Add(currentTile);
					validSpawnTiles.Add(currentTile);
					if (validSpawnTiles.Count > 100) {
						break;
					}
					foreach (TileManager.Tile nTile in currentTile.horizontalSurroundingTiles) {
						if (walkableTilesByDistanceToCentre.Contains(nTile) && !checkedTiles.Contains(nTile)) {
							frontier.Add(nTile);
						}
					}
				}
				TileManager.Tile colonistSpawnTile = validSpawnTiles.Count >= amount ? validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)] : walkableTilesByDistanceToCentre[UnityEngine.Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

				new Colonist(colonistSpawnTile, 1);
			}

			GameManager.uiM.SetColonistElements();
			GameManager.colonyM.colony.map.Bitmasking(GameManager.colonyM.colony.map.tiles, true, true);
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
		}
	}
}