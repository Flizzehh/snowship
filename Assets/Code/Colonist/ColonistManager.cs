using Snowship.NProfession;
using Snowship.NTime;
using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NColonist {

	public class ColonistManager : IManager {

		private readonly List<Colonist> deadColonists = new List<Colonist>();

		public void Update() {
			UpdateColonists();
			UpdateColonistJobs();
		}

		private void UpdateColonists() {
			foreach (Colonist colonist in Colonist.colonists) {
				colonist.Update();
				if (colonist.dead) {
					deadColonists.Add(colonist);
				}
			}
			foreach (Colonist deadColonist in deadColonists) {
				deadColonist.Die();
			}
			deadColonists.Clear();
		}

		private void UpdateColonistJobs() {
			if (!GameManager.timeM.GetPaused()) {
				GameManager.jobM.GiveJobsToColonists();
			}
		}

		public enum SkillEnum { Building, Terraforming, Farming, Forestry, Crafting, Hauling };

		public class SkillPrefab {

			public SkillEnum type;
			public string name;

			public readonly Dictionary<string, float> affectedJobTypes = new Dictionary<string, float>();

			public string relatedProfession;

			public SkillPrefab(List<string> data) {
				type = (SkillEnum)Enum.Parse(typeof(SkillEnum), data[0]);
				name = type.ToString();

				foreach (string affectedJobTypeString in data[1].Split(';')) {
					List<string> affectedJobTypeData = affectedJobTypeString.Split(',').ToList();
					affectedJobTypes.Add(affectedJobTypeData[0], float.Parse(affectedJobTypeData[1]));
				}
			}
		}

		public SkillPrefab GetSkillPrefabFromString(string skillTypeString) {
			return skillPrefabs.Find(skillPrefab => skillPrefab.type == (SkillEnum)Enum.Parse(typeof(SkillEnum), skillTypeString));
		}

		public SkillPrefab GetSkillPrefabFromEnum(SkillEnum skillEnum) {
			return skillPrefabs.Find(skillPrefab => skillPrefab.type == skillEnum);
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
				ColonistJob.UpdateColonistJobCosts(colonist);
				if (GameManager.humanM.selectedHuman == colonist) {
					GameManager.uiMOld.RemakeSelectedColonistSkills();
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
				skillPrefab.name = StringUtilities.SplitByCapitals(skillPrefab.name);
			}
		}

		public SkillInstance FindHighestSkillInstance(SkillPrefab skill) {

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
		public readonly Dictionary<string, NeedEnum> jobToNeedMap = new() {
		{ "Sleep", NeedEnum.Rest },
		{ "CollectWater", NeedEnum.Water },
		{ "Drink", NeedEnum.Water },
		{ "CollectFood", NeedEnum.Food },
		{ "Eat", NeedEnum.Food }
	};

		public readonly Dictionary<NeedEnum, Func<NeedInstance, float>> needsValueSpecialIncreases = new Dictionary<NeedEnum, Func<NeedInstance, float>>() {
		{ NeedEnum.Rest, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance positiveRestMoodModifier = need.colonist.FindMoodModifierByGroupEnum(MoodModifierGroupEnum.Rest, 1).FirstOrDefault();
			if (positiveRestMoodModifier != null) {
				switch (positiveRestMoodModifier.prefab.type) {
					case MoodModifierEnum.WellRested:
						totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.8f);
						break;
					case MoodModifierEnum.Rested:
						totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.6f);
						break;
				}
			}
			if (!GameManager.timeM.IsDay && positiveRestMoodModifier == null) {
				totalSpecialIncrease += (need.prefab.baseIncreaseRate * 2f);
			}
			return totalSpecialIncrease;
		} },
		{ NeedEnum.Water, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = need.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Water);
			if (moodModifier != null) {
				if (moodModifier.prefab.type == MoodModifierEnum.Quenched) {
					totalSpecialIncrease -= (need.prefab.baseIncreaseRate* 0.5f);
				}
			}
			return totalSpecialIncrease;
		} },
		{ NeedEnum.Food, delegate (NeedInstance need) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = need.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Food);
			if (moodModifier != null) {
				if (moodModifier.prefab.type == MoodModifierEnum.Stuffed) {
					totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.9f);
				} else if (moodModifier.prefab.type == MoodModifierEnum.Full) {
					totalSpecialIncrease -= (need.prefab.baseIncreaseRate * 0.5f);
				}
			}
			return totalSpecialIncrease;
		} }
	};

		public void CalculateNeedValue(NeedInstance need) {
			if (need.colonist.job != null && need.prefab.relatedJobs.Contains(need.colonist.job.objectPrefab.jobType)) {
				return;
			}
			float needIncreaseAmount = need.prefab.baseIncreaseRate;
			foreach (TraitInstance trait in need.colonist.traits) {
				if (need.prefab.traitsAffectingThisNeed.ContainsKey(trait.prefab.type)) {
					needIncreaseAmount *= need.prefab.traitsAffectingThisNeed[trait.prefab.type];
				}
			}
			need.ChangeValue(needIncreaseAmount + (needsValueSpecialIncreases.ContainsKey(need.prefab.type) ? needsValueSpecialIncreases[need.prefab.type](need) : 0));
		}

		public static ResourceManager.Container FindClosestResourceAmountInContainers(Colonist colonist, ResourceManager.ResourceAmount resourceAmount) {

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
						Job job = new Job(
							JobPrefab.GetJobPrefabByName("CollectFood"),
							container.tile,
							GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectFood),
							null,
							0
						);
						need.colonist.SetJob(new ColonistJob(need.colonist, job, null, null));
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
					foreach (Colonist otherColonist in Colonist.colonists) {
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
					need.colonist.SetJob(new ColonistJob(
						need.colonist,
						new Job(
							JobPrefab.GetJobPrefabByName("Sleep"),
							chosenSleepSpot.tile,
							GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Sleep),
							null,
							0
						),
						null,
						  null
					));
					return true;
				}
			}
			if (sleepAnywhere) {
				need.colonist.SetJob(new ColonistJob(
					need.colonist,
					new Job(
						JobPrefab.GetJobPrefabByName("Sleep"),
						need.colonist.overTile,
						GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Sleep),
						null,
						0),
					null,
					null
				));
				return true;
			}
			return false;
		}

		public static readonly Dictionary<NeedEnum, Func<NeedInstance, bool>> needsValueFunctions = new Dictionary<NeedEnum, Func<NeedInstance, bool>>() {
		{ NeedEnum.Food, delegate (NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.objectPrefab.jobType == "CollectFood" || need.colonist.job.objectPrefab.jobType == "Eat")) {
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
			if (need.colonist.job == null || !(need.colonist.job.objectPrefab.jobType == "CollectWater" || need.colonist.job.objectPrefab.jobType == "Drink")) {
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
			if (need.colonist.job == null || !(need.colonist.job.objectPrefab.jobType == "Sleep")) {
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
			public float decreaseRateMultiplier;

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

			public readonly Dictionary<TraitEnum, float> traitsAffectingThisNeed = new();

			public readonly List<string> relatedJobs = new List<string>();

			public NeedPrefab(
				NeedEnum type,
				float baseIncreaseRate,
				float decreaseRateMultiplier,
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
				List<string> relatedJobs
			) {
				this.type = type;
				name = StringUtilities.SplitByCapitals(type.ToString());

				this.baseIncreaseRate = baseIncreaseRate;
				this.decreaseRateMultiplier = decreaseRateMultiplier;

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
				value = Mathf.Clamp(value, 0, prefab.clampValue);
				roundedValue = Mathf.RoundToInt((value / prefab.clampValue) * 100);
				if (GameManager.humanM.selectedHuman == colonist && Mathf.RoundToInt((oldValue / prefab.clampValue) * 100) != roundedValue) {
					GameManager.uiMOld.RemakeSelectedColonistNeeds();
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
				float decreaseRateMultiplier = 0;
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
				Dictionary<TraitEnum, float> traitsAffectingThisNeed = new();
				List<string> relatedJobs = new();

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
							case "DecreaseRateMultiplier":
								decreaseRateMultiplier = float.Parse(value);
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
								if (!string.IsNullOrEmpty(StringUtilities.RemoveNonAlphanumericChars(value))) {
									foreach (string traitsAffectingThisNeedString in value.Split(',')) {
										TraitEnum traitEnum = (TraitEnum)Enum.Parse(typeof(TraitEnum), traitsAffectingThisNeedString.Split(':')[0]);
										float multiplier = float.Parse(traitsAffectingThisNeedString.Split(':')[1]);
										traitsAffectingThisNeed.Add(traitEnum, multiplier);
									}
								}
								break;
							case "RelatedJobs":
								if (!string.IsNullOrEmpty(StringUtilities.RemoveNonAlphanumericChars(value))) {
									foreach (string relatedJobString in value.Split(',')) {
										string jobTypeEnum = relatedJobString;
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

				needPrefabs.Add(new NeedPrefab(type, baseIncreaseRate, decreaseRateMultiplier, minValueAction, minValue, maxValueAction, maxValue, critValueAction, critValue, canDie, healthDecreaseRate, clampValue, priority, traitsAffectingThisNeed, relatedJobs));
			}
		}

		public static readonly Dictionary<MoodModifierGroupEnum, Action<Colonist>> moodModifierFunctions = new Dictionary<MoodModifierGroupEnum, Action<Colonist>>() {
		{ MoodModifierGroupEnum.Death, delegate (Colonist colonist) {
			// TODO Implement colonists viewing deaths and being sad
		} },
		{ MoodModifierGroupEnum.Rest, delegate (Colonist colonist) {
			NeedInstance restNeed = colonist.needs.Find(ni => ni.prefab.type == NeedEnum.Rest);
			if (restNeed.GetValue() >= restNeed.prefab.critValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Exhausted);
			} else if (restNeed.GetValue() >= restNeed.prefab.maxValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Tired);
			} else {
				colonist.RemoveMoodModifier(MoodModifierEnum.Exhausted);
				colonist.RemoveMoodModifier(MoodModifierEnum.Tired);
			}
		} },
		{ MoodModifierGroupEnum.Water, delegate (Colonist colonist) {
			NeedInstance waterNeed = colonist.needs.Find(ni => ni.prefab.type == NeedEnum.Water);
			if (waterNeed.GetValue() >= waterNeed.prefab.critValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Dehydrated);
			} else if (waterNeed.GetValue() >= waterNeed.prefab.maxValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Thirsty);
			} else {
				colonist.RemoveMoodModifier(MoodModifierEnum.Dehydrated);
				colonist.RemoveMoodModifier(MoodModifierEnum.Thirsty);
			}
		} },
		{ MoodModifierGroupEnum.Food, delegate (Colonist colonist) {
			NeedInstance foodNeed = colonist.needs.Find(ni => ni.prefab.type == NeedEnum.Food);
			if (foodNeed.GetValue() >= foodNeed.prefab.critValue) {
				colonist.AddMoodModifier(MoodModifierEnum.Starving);
			} else if (foodNeed.GetValue() >= foodNeed.prefab.maxValue) {
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
			Stuffed, Full, Hungry, Starving, AteOnTheFloor, AteWithoutATable,
			Dehydrated, Thirsty, Quenched,
			WellRested, Rested, Tired, Exhausted,
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
				name = StringUtilities.SplitByCapitals(type.ToString());

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
								name = StringUtilities.SplitByCapitals(type.ToString());
								break;
							case "EffectAmount":
								effectAmount = int.Parse(value);
								break;
							case "EffectLengthSeconds":
								infinite = StringUtilities.RemoveNonAlphanumericChars(value) == "UntilNot";
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



		public void SpawnStartColonists(int amount) {
			SpawnColonists(amount);

			Vector2 averageColonistPosition = new Vector2(0, 0);
			foreach (Colonist colonist in Colonist.colonists) {
				averageColonistPosition = new Vector2(averageColonistPosition.x + colonist.obj.transform.position.x, averageColonistPosition.y + colonist.obj.transform.position.y);
			}
			averageColonistPosition /= Colonist.colonists.Count;
			GameManager.cameraM.SetCameraPosition(averageColonistPosition);

			// TEMPORARY COLONIST TESTING STUFF
			Colonist.colonists[UnityEngine.Random.Range(0, Colonist.colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.WheatSeed), UnityEngine.Random.Range(5, 11), false);
			Colonist.colonists[UnityEngine.Random.Range(0, Colonist.colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.Potato), UnityEngine.Random.Range(5, 11), false);
			Colonist.colonists[UnityEngine.Random.Range(0, Colonist.colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.CottonSeed), UnityEngine.Random.Range(5, 11), false);
		}

		public void SpawnColonists(int amount) {
			if (amount > 0) {
				int mapSize = GameManager.colonyM.colony.map.mapData.mapSize;
				for (int i = 0; i < amount; i++) {
					List<TileManager.Tile> walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && o.buildable && Colonist.colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
					if (walkableTilesByDistanceToCentre.Count <= 0) {
						foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles.Where(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
							tile.SetTileType(tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Ground], true, true, true);
						}
						walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && Colonist.colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
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

				GameManager.uiMOld.SetColonistElements();
				GameManager.colonyM.colony.map.Bitmasking(GameManager.colonyM.colony.map.tiles, true, true);
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
			}
		}
	}
}
