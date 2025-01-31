using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NTime;

namespace Snowship.NColonist {
	public static class NeedUtilities {

		// TODO Add "relatedNeeds" variable to JobPrefab and then find a way to delete this
		public static readonly Dictionary<string, ENeed> jobToNeedMap = new Dictionary<string, ENeed> {
			{ "Sleep", ENeed.Rest },
			{ "CollectFood", ENeed.Food },
			{ "Eat", ENeed.Food }
		};

		public static readonly Dictionary<ENeed, Func<NeedInstance, float>> NeedToSpecialValueFunctionMap = new Dictionary<ENeed, Func<NeedInstance, float>> {
			{ ENeed.Rest, CalculateRestNeedSpecialValueIncrease },
			{ ENeed.Food, CalculateFoodNeedSpecialValueIncrease }
		};

		public static readonly Dictionary<ENeed, Func<NeedInstance, bool>> NeedToReactionFunctionMap = new Dictionary<ENeed, Func<NeedInstance, bool>> {
			{ ENeed.Rest, DetermineRestNeedReaction },
			{ ENeed.Food, DetermineFoodNeedReaction }
		};

		public static float CalculateRestNeedSpecialValueIncrease(NeedInstance needInstance) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance positiveRestMoodModifier = needInstance.colonist.Moods.FindMoodModifierByGroupEnum(MoodModifierGroupEnum.Rest, 1).FirstOrDefault();
			if (positiveRestMoodModifier != null) {
				switch (positiveRestMoodModifier.Prefab.type) {
					case MoodModifierEnum.WellRested:
						totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.8f;
						break;
					case MoodModifierEnum.Rested:
						totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.6f;
						break;
				}
			}
			if (!GameManager.Get<TimeManager>().Time.IsDay && positiveRestMoodModifier == null) {
				totalSpecialIncrease += needInstance.prefab.baseIncreaseRate * 2f;
			}
			return totalSpecialIncrease;
		}

		public static float CalculateFoodNeedSpecialValueIncrease(NeedInstance needInstance) {
			float totalSpecialIncrease = 0;
			MoodModifierInstance moodModifier = needInstance.colonist.Moods.MoodModifiers.Find(findMoodModifier => findMoodModifier.Prefab.group.type == MoodModifierGroupEnum.Food);
			if (moodModifier != null) {
				if (moodModifier.Prefab.type == MoodModifierEnum.Stuffed) {
					totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.9f;
				} else if (moodModifier.Prefab.type == MoodModifierEnum.Full) {
					totalSpecialIncrease -= needInstance.prefab.baseIncreaseRate * 0.5f;
				}
			}
			return totalSpecialIncrease;
		}

		public static void CalculateNeedValue(NeedInstance need) {
			if (need.colonist.Job != null && need.prefab.relatedJobs.Contains(need.colonist.Job.JobName)) {
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

		public static Container FindClosestResourceAmountInContainers(Colonist colonist, ResourceAmount resourceAmount) {

			List<Container> containersWithResourceAmount = new();

			foreach (Container container in Container.GetContainersInRegion(colonist.overTile.region)) {
				if (container.Inventory.ContainsResourceAmount(resourceAmount)) {
					containersWithResourceAmount.Add(container);
				}
			}
			return containersWithResourceAmount.OrderBy(container => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, container.tile.regionBlock, true, true, false)).FirstOrDefault();
		}

		public static int FindAvailableResourceAmount(ResourceGroup.ResourceGroupEnum resourceGroup, Colonist colonist, bool worldTotal, bool includeOtherColonists) {
			if (worldTotal) {
				int total = 0;
				foreach (Resource resource in Resource.GetResources()) {
					if (resource.groupType == resourceGroup) {
						total += resource.GetWorldTotalAmount();
					}
				}
				return total;
			} else {
				int total = 0;

				int amountOnThisColonist = 0;
				foreach (ResourceAmount resourceAmount in colonist.Inventory.resources.Where(ra => ra.Resource.groupType == resourceGroup)) {
					amountOnThisColonist += resourceAmount.Amount;
				}
				total += amountOnThisColonist;

				int amountUnreservedInContainers = 0;
				foreach (Resource resource in Resource.GetResources().Where(r => r.groupType == resourceGroup)) {
					amountUnreservedInContainers += resource.GetUnreservedContainerTotalAmount();
				}
				total += amountUnreservedInContainers;

				if (includeOtherColonists) {
					int amountOnOtherColonists = 0;
					foreach (Colonist otherColonist in Colonist.colonists) {
						if (colonist != otherColonist) {
							int amountOnOtherColonist = 0;
							foreach (ResourceAmount resourceAmount in otherColonist.Inventory.resources.Where(ra => ra.Resource.groupType == resourceGroup)) {
								amountOnOtherColonist += resourceAmount.Amount;
							}
							total += amountOnOtherColonist;
						}
					}
					total += amountOnOtherColonists;
				}

				return total;
			}
		}

		public static bool DetermineFoodNeedReaction(NeedInstance need) {
			if (need.GetValue() < 50) {
				return false;
			}
			if (need.colonist.Job is { Group: "Needs", SubGroup: "Food" }) {
				return false;
			}
			need.colonist.SetJob(new CollectFoodJob(need.colonist.overTile, null, null, need.GetValue()));
			return false;
		}

		public static bool DetermineRestNeedReaction(NeedInstance need) {
			if (need.GetValue() < 50) {
				return false;
			}
			if (need.colonist.Job is { Group: "Needs", SubGroup: "Rest" }) {
				return false;
			}
			need.colonist.SetJob(new SleepJob(need.colonist.overTile));
			return false;
		}

	}
}