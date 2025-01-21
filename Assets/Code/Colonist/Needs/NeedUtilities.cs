using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;

namespace Snowship.NColonist {
	public static class NeedUtilities {

		// TODO Add "relatedNeeds" variable to JobPrefab and then find a way to delete this
		public static readonly Dictionary<string, ENeed> jobToNeedMap = new Dictionary<string, ENeed> {
			{ "Sleep", ENeed.Rest },
			// { "CollectWater", ENeed.Water },
			// { "Drink", ENeed.Water },
			{ "CollectFood", ENeed.Food },
			{ "Eat", ENeed.Food }
		};

		public static readonly Dictionary<ENeed, Func<NeedInstance, float>> NeedToSpecialValueFunctionMap = new Dictionary<ENeed, Func<NeedInstance, float>> {
			{ ENeed.Rest, CalculateRestNeedSpecialValueIncrease },
			// { ENeed.Water, CalculateWaterNeedSpecialValueIncrease },
			{ ENeed.Food, CalculateFoodNeedSpecialValueIncrease }
		};

		public static readonly Dictionary<ENeed, Func<NeedInstance, bool>> NeedToReactionFunctionMap = new Dictionary<ENeed, Func<NeedInstance, bool>> {
			{ ENeed.Rest, DetermineRestNeedReaction },
			// { ENeed.Water, null },
			{ ENeed.Food, DetermineFoodNeedReaction }
		};

		public static float CalculateRestNeedSpecialValueIncrease(NeedInstance needInstance) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance positiveRestMoodModifier = needInstance.colonist.FindMoodModifierByGroupEnum(MoodModifierGroupEnum.Rest, 1).FirstOrDefault();
			if (positiveRestMoodModifier != null) {
				switch (positiveRestMoodModifier.prefab.type) {
					case MoodModifierEnum.WellRested:
						totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.8f;
						break;
					case MoodModifierEnum.Rested:
						totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.6f;
						break;
				}
			}
			if (!GameManager.timeM.Time.IsDay && positiveRestMoodModifier == null) {
				totalSpecialIncrease += needInstance.prefab.baseIncreaseRate * 2f;
			}
			return totalSpecialIncrease;
		}

		/*private static float CalculateWaterNeedSpecialValueIncrease(NeedInstance needInstance) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = needInstance.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Water);
			if (moodModifier != null) {
				if (moodModifier.prefab.type == MoodModifierEnum.Quenched) {
					totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.5f;
				}
			}
			return totalSpecialIncrease;
		}*/

		public static float CalculateFoodNeedSpecialValueIncrease(NeedInstance needInstance) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = needInstance.colonist.moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.group.type == MoodModifierGroupEnum.Food);
			if (moodModifier != null) {
				if (moodModifier.prefab.type == MoodModifierEnum.Stuffed) {
					totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.9f;
				} else if (moodModifier.prefab.type == MoodModifierEnum.Full) {
					totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.5f;
				}
			}
			return totalSpecialIncrease;
		}

		public static void CalculateNeedValue(NeedInstance need) {
			if (need.colonist.job != null && need.prefab.relatedJobs.Contains(need.colonist.job.objectPrefab.jobType)) {
				return;
			}
			float needIncreaseAmount = need.prefab.baseIncreaseRate;
			foreach (TraitInstance trait in need.colonist.traits) {
				if (need.prefab.traitsAffectingThisNeed.ContainsKey(trait.prefab.type)) {
					needIncreaseAmount *= need.prefab.traitsAffectingThisNeed[trait.prefab.type];
				}
			}
			need.ChangeValue(needIncreaseAmount + (NeedToSpecialValueFunctionMap.TryGetValue(need.prefab.type, out Func<NeedInstance, float> specialValueFunction) ? specialValueFunction(need) : 0));
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

		private static KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>> FindClosestFood(Colonist colonist, float minimumNutritionRequired, bool takeFromOtherColonists, bool eatAnything) {
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
				// TODO Take from other colonists
			}
			if (resourcesPerInventory.Count > 0) {
				return resourcesPerInventory[0].Key;
			} else {
				return new KeyValuePair<ResourceManager.Inventory, List<ResourceManager.ResourceAmount>>(null, null);
			}
		}

		public static bool GetFood(NeedInstance need, bool takeFromOtherColonists, bool eatAnything) {
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
						// TODO Take food from another human
						//Human human = closestFood.Key.human;
					}
				}
			} else {
				need.colonist.SetEatJob();
				return true;
			}
			return false;
		}

		public static int FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum resourceGroup, Colonist colonist, bool worldTotal, bool includeOtherColonists) {
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

		public static bool GetSleep(NeedInstance need, bool sleepAnywhere) {
			if (GameManager.resourceM.sleepSpots.Count > 0) {
				List<ResourceManager.SleepSpot> validSleepSpots = GameManager.resourceM.sleepSpots.Where(sleepSpot => sleepSpot.occupyingColonist == null && sleepSpot.tile.region == need.colonist.overTile.region).ToList();
				if (validSleepSpots.Count > 0) {
					ResourceManager.SleepSpot chosenSleepSpot = validSleepSpots.OrderByDescending(sleepSpot => sleepSpot.prefab.restComfortAmount / (PathManager.RegionBlockDistance(need.colonist.overTile.regionBlock, sleepSpot.tile.regionBlock, true, true, false) + 1)).ToList()[0];
					chosenSleepSpot.StartSleeping(need.colonist);
					need.colonist.SetJob(
						new ColonistJob(
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
				need.colonist.SetJob(
					new ColonistJob(
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

		public static bool DetermineFoodNeedReaction(NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.objectPrefab.jobType == "CollectFood" || need.colonist.job.objectPrefab.jobType == "Eat")) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate);
					// TODO Check that this still works properly - (removed timeM.minuteChanged check before each of these 3 blocks)
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) { // true, true
						need.colonist.ReturnJob();
						return GetFood(need, false, false); // true, true - TODO use these once implemented
					}
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) { // false, true
						if (UnityEngine.Random.Range(0f, 1f) < (need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue)) {
							need.colonist.ReturnJob();
							return GetFood(need, false, false); // true, false - TODO use these once implemented
						}
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (FindAvailableResourceAmount(ResourceManager.ResourceGroupEnum.Foods, need.colonist, false, false) > 0) {
							if (UnityEngine.Random.Range(0f, 1f) < (need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue)) {
								need.colonist.ReturnJob();
								return GetFood(need, false, false);
							}
						}
					}
					return false;
				}
			}
			return false;
		}

		public static bool DetermineRestNeedReaction(NeedInstance need) {
			if (need.colonist.job == null || !(need.colonist.job.objectPrefab.jobType == "Sleep")) {
				if (need.prefab.critValueAction && need.GetValue() >= need.prefab.critValue) {
					need.colonist.ChangeHealthValue(need.prefab.healthDecreaseRate);
					need.colonist.ReturnJob();
					GetSleep(need, true);
					return false;
				}
				if (need.prefab.maxValueAction && need.GetValue() >= need.prefab.maxValue) {
					if (UnityEngine.Random.Range(0f, 1f) < (need.GetValue() - need.prefab.maxValue) / (need.prefab.critValue - need.prefab.maxValue)) {
						need.colonist.ReturnJob();
						GetSleep(need, true);
					}
					return false;
				}
				if (need.prefab.minValueAction && need.GetValue() >= need.prefab.minValue) {
					if (need.colonist.job == null) {
						if (UnityEngine.Random.Range(0f, 1f) < (need.GetValue() - need.prefab.minValue) / (need.prefab.maxValue - need.prefab.minValue)) {
							need.colonist.ReturnJob();
							GetSleep(need, false);
						}

					}
					return false;
				}
			}
			return false;
		}

	}
}
