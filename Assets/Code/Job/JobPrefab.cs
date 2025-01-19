using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using UnityEngine;

namespace Snowship.NJob {

	public class JobPrefabGroup {

		public static readonly Dictionary<string, JobPrefabGroup> jobPrefabGroups = new();

		public readonly string name;

		public readonly List<JobPrefab> jobPrefabs = new();

		public JobPrefabGroup(string name, List<JobPrefab> jobPrefabs) {
			this.name = name;
			this.jobPrefabs = jobPrefabs;

			foreach (JobPrefab jobPrefab in jobPrefabs) {
				jobPrefab.group = this;
			}
		}
	}

	public class JobPrefab {

		public static readonly Dictionary<string, JobPrefab> jobPrefabs = new();

		public JobPrefabGroup group;

		public readonly string name;

		public readonly bool returnable;

		private Func<Job, string> description {
			get {
				if (JobPrefabProvider.descriptionForJobPrefab.ContainsKey(name)) {
					return JobPrefabProvider.descriptionForJobPrefab[name];
				} else {
					Debug.LogWarning($"No description set for job with name '{name}'");
					return JobPrefabProvider.descriptionForJobPrefab["None"];
				}
			}
		}

		private Func<Job, string> jobInfoNameText {
			get {
				if (JobPrefabProvider.jobInfoNameTextForJobPrefab.ContainsKey(name)) {
					return JobPrefabProvider.jobInfoNameTextForJobPrefab[name];
				} else {
					return JobPrefabProvider.jobInfoNameTextForJobPrefab["None"];
				}
			}
		}

		private List<Action<Job>> startJobActions => JobPrefabProvider.startJobActionsForJobPrefab[name];
		private List<Action<Job>> workJobActions => JobPrefabProvider.workJobActionsForJobPrefab[name];
		private List<Action<Job, Colonist>> finishJobActions => JobPrefabProvider.finishJobActionsForJobPrefab[name];

		public JobPrefab(
			string name,
			bool returnable
		) {
			this.name = name;

			this.returnable = returnable;
		}

		public static JobPrefab GetJobPrefabByName(string name) {
			return jobPrefabs[name];
		}

		public string GetJobDescription(Job job) {
			return description.Invoke(job);
		}

		public string GetJobInfoNameText(Job job) {
			return jobInfoNameText.Invoke(job);
		}

		public void RunStartJobActions(Job job) {
			foreach (Action<Job> startJobAction in startJobActions) {
				startJobAction.Invoke(job);
			}
		}

		public void RunWorkJobActions(Job job) {
			foreach (Action<Job> workJobAction in workJobActions) {
				workJobAction.Invoke(job);
			}
		}

		public void RunFinishJobActions(Job job, Colonist colonist) {
			foreach (Action<Job, Colonist> finishJobAction in finishJobActions) {
				finishJobAction.Invoke(job, colonist);
			}
		}
	}

	internal static class JobPrefabProvider {

		public static Dictionary<string, Func<Job, string>> descriptionForJobPrefab = new() {
			{
				"None",
				delegate (Job job) {
					return "Doing something.";
				}
			},
			{
				"Build",
				delegate (Job job) {
					return $"Building a {job.objectPrefab.name}.";
				}
			},
			{
				"Remove",
				delegate (Job job) {
					if (job.objectPrefab.type == ResourceManager.ObjectEnum.RemoveRoof) {
						return $"Removing a roof.";
					} else {
						return $"Removing a {job.tile.GetObjectInstanceAtLayer(job.objectPrefab.layer).prefab.name}.";
					}
				}
			},
			{
				"ChopPlant",
				delegate (Job job) {
					return $"Chopping down a {job.tile.plant.prefab.name}.";
				}
			},
			{
				"PlantPlant",
				delegate (Job job) {
					return $"Planting a plant.";
				}
			},
			{
				"Mine",
				delegate (Job job) {
					return $"Mining {job.tile.tileType.name}.";
				}
			},
			{
				"Dig",
				delegate (Job job) {
					return $"Digging {string.Join(" and ", job.tile.tileType.resourceRanges.Select(rr => rr.resource.name).ToArray())}";
				}
			},
			{
				"Fill",
				delegate (Job job) {
					return $"Filling {job.tile.tileType.groupType.ToString().ToLower()}.";
				}
			},
			{
				"PlantFarm",
				delegate (Job job) {
					return $"Planting a {job.objectPrefab.name}.";
				}
			},
			{
				"HarvestFarm",
				delegate (Job job) {
					return $"Harvesting a farm of {job.tile.farm.name}.";
				}
			},
			{
				"CreateResource",
				delegate (Job job) {
					return $"Creating {job.createResource.resource.name}.";
				}
			},
			{
				"PickupResources",
				delegate (Job job) {
					return $"Picking up some resources.";
				}
			},
			{
				"TransferResources",
				delegate (Job job) {
					return $"Transferring resources.";
				}
			},
			{
				"CollectResources",
				delegate (Job job) {
					return $"Collecting resources.";
				}
			},
			{
				"EmptyInventory",
				delegate (Job job) {
					return $"Emptying inventory.";
				}
			},
			{
				"CollectFood",
				delegate (Job job) {
					return $"Finding some food to eat.";
				}
			},
			{
				"Eat",
				delegate (Job job) {
					return $"Eating.";
				}
			},
			{
				"CollectWater",
				delegate (Job job) {
					return $"Finding something to drink.";
				}
			},
			{
				"Drink",
				delegate (Job job) {
					return $"Drinking.";
				}
			},
			{
				"Sleep",
				delegate (Job job) {
					return $"Sleeping.";
				}
			},
			{
				"WearClothes",
				delegate (Job job) {
					return $"Wearing {job.requiredResources[0].resource.name}.";
				}
			}
		};

		public static Dictionary<string, Func<Job, string>> jobInfoNameTextForJobPrefab = new() {
			{
				"None",
				delegate (Job job) {
					return job.objectPrefab.name;
				}
			},
			{
				"Mine",
				delegate (Job job) {
					return job.tile.tileType.name;
				}
			},
			{
				"Dig",
				delegate (Job job) {
					return job.tile.tileType.name;
				}
			},
			{
				"PlantPlant",
				delegate (Job job) {
					return GameManager.resourceM.GetPlantGroupByEnum(job.variation.plants.First().Key.groupType).name;
				}
			},
			{
				"ChopPlant",
				delegate (Job job) {
					return job.tile.plant.name;
				}
			},
			{
				"HarvestFarm",
				delegate (Job job) {
					return job.tile.farm.name;
				}
			},
			{
				"CreateResource",
				delegate (Job job) {
					return job.createResource.resource.name;
				}
			},
			{
				"Remove",
				delegate (Job job) {
					if (job.objectPrefab.type == ResourceManager.ObjectEnum.RemoveRoof) {
						return "Roof";
					} else {
						return job.tile.GetObjectInstanceAtLayer(job.objectPrefab.layer).prefab.name;
					}
				}
			}
		};

		public static Dictionary<string, List<Action<Job>>> startJobActionsForJobPrefab = new() {

		};

		public static Dictionary<string, List<Action<Job>>> workJobActionsForJobPrefab = new() {

		};

		public static Dictionary<string, List<Action<Job, Colonist>>> finishJobActionsForJobPrefab = new() {
			{
				"Build",
				new() {
					delegate (Job job, Colonist colonist) {
						foreach (ResourceManager.ResourceAmount resourceAmount in job.requiredResources) {
							colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
						}
						if (job.objectPrefab.subGroupType == ResourceManager.ObjectSubGroupEnum.Roofs) {
							job.tile.SetRoof(true);
						}
					}
				}
			},
			{
				"Remove",
				new() {
					delegate (Job job, Colonist colonist) {
						bool previousWalkability = job.tile.walkable;
						ResourceManager.ObjectInstance instance = job.tile.GetObjectInstanceAtLayer(job.objectPrefab.layer);
						if (instance != null) {
							foreach (ResourceManager.ResourceAmount resourceAmount in instance.prefab.commonResources) {
								colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, Mathf.RoundToInt(resourceAmount.amount), false);
							}
							if (instance.variation != null) {
								foreach (ResourceManager.ResourceAmount resourceAmount in instance.variation.uniqueResources) {
									colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, Mathf.RoundToInt(resourceAmount.amount), false);
								}
							}
							if (instance is ResourceManager.Farm farm) {
								if (farm.growProgressSpriteIndex == 0) {
									job.colonist.GetInventory().ChangeResourceAmount(farm.prefab.seedResource, 1, false);
								}
							} else if (instance is ResourceManager.IInventory inventory) {
								List<ResourceManager.ResourceAmount> nonReservedResourcesToRemove = new List<ResourceManager.ResourceAmount>();
								foreach (ResourceManager.ResourceAmount resourceAmount in inventory.GetInventory().resources) {
									nonReservedResourcesToRemove.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
									colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
								}
								foreach (ResourceManager.ResourceAmount resourceAmount in nonReservedResourcesToRemove) {
									inventory.GetInventory().ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
								}
								List<ResourceManager.ReservedResources> reservedResourcesToRemove = new List<ResourceManager.ReservedResources>();
								foreach (ResourceManager.ReservedResources reservedResources in inventory.GetInventory().reservedResources) {
									foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
										colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
									}
									reservedResourcesToRemove.Add(reservedResources);
									if (reservedResources.human is Colonist human) {
										human.ReturnJob();
									}
								}
								foreach (ResourceManager.ReservedResources reservedResourceToRemove in reservedResourcesToRemove) {
									inventory.GetInventory().reservedResources.Remove(reservedResourceToRemove);
								}
								// GameManager.uiMOld.SetSelectedColonistInformation(true); // TODO Inventory Updated
								GameManager.uiMOld.SetSelectedContainerInfo();
								GameManager.uiMOld.UpdateSelectedTradingPostInfo();
							} else if (instance is ResourceManager.CraftingObject craftingObject) {
								foreach (Job removeJob in craftingObject.resources.Where(resource => resource.job != null).Select(resource => resource.job)) {
									GameManager.jobM.CancelJob(removeJob);
								}
							} else if (instance is ResourceManager.SleepSpot sleepSpot) {
								if (sleepSpot.occupyingColonist != null) {
									sleepSpot.occupyingColonist.ReturnJob();
								}
							}
							GameManager.resourceM.RemoveObjectInstance(instance);
							job.tile.RemoveObjectAtLayer(instance.prefab.layer);
						} else {
							switch (job.objectPrefab.type) {
								case ResourceManager.ObjectEnum.RemoveRoof:
									job.tile.SetRoof(false);
									foreach (ResourceManager.ResourceAmount resourceAmount in GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.Roof).commonResources) {
										colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, Mathf.RoundToInt(resourceAmount.amount), false);
									}
									break;
								default:
									Debug.LogError("Object instance is null and no alternative removal type is known.");
									break;
							}
						}

						GameManager.resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
						if (job.tile.walkable && !previousWalkability) {
							GameManager.colonyM.colony.map.RemoveTileBrightnessEffect(job.tile);
						}
					}
				}
			},
			{
				"PlantFarm",
				new() {
					delegate (Job job, Colonist colonist) {
						finishJobActionsForJobPrefab["Build"].ForEach(a => a.Invoke(job, colonist));
						if (job.tile.tileType.classes[TileManager.TileType.ClassEnum.Dirt]) {
							job.tile.SetTileType(TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Mud), false, true, false);
						}
					}
				}
			},
			{
				"HarvestFarm",
				new() {
					delegate (Job job, Colonist colonist) {
						if (job.tile.farm != null) {
							colonist.GetInventory().ChangeResourceAmount(job.tile.farm.prefab.seedResource, UnityEngine.Random.Range(1, 3), false);
							foreach (ResourceManager.ResourceRange harvestResourceRange in job.tile.farm.prefab.harvestResources) {
								colonist.GetInventory().ChangeResourceAmount(harvestResourceRange.resource, UnityEngine.Random.Range(harvestResourceRange.min, harvestResourceRange.max), false);
							}

							GameManager.jobM.CreateJob(new Job(
								JobPrefab.GetJobPrefabByName("PlantFarm"),
								job.tile,
								job.tile.farm.prefab,
								job.tile.farm.variation,
								  0
							));

							int layer = job.tile.farm.prefab.layer; // Required because RemoveObjectInstance sets job.tile.farm = null but must happen before RemoveObjectAtLayer
							GameManager.resourceM.RemoveObjectInstance(job.tile.farm);
							job.tile.RemoveObjectAtLayer(layer);
						}
						GameManager.resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
					}
				}
			},
			{
				"ChopPlant",
				new() {
					delegate (Job job, Colonist colonist) {
						foreach (ResourceManager.ResourceRange resourceRange in job.tile.plant.prefab.returnResources) {
							colonist.GetInventory().ChangeResourceAmount(resourceRange.resource, Mathf.CeilToInt(UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1) / (job.tile.plant.small ? 2f : 1f)), false);
						}
						if (job.tile.plant.harvestResource != null) {
							ResourceManager.ResourceRange harvestResourceRange = job.tile.plant.prefab.harvestResources.Find(rr => rr.resource == job.tile.plant.harvestResource);
							colonist.GetInventory().ChangeResourceAmount(job.tile.plant.harvestResource, Mathf.CeilToInt(UnityEngine.Random.Range(harvestResourceRange.min, harvestResourceRange.max + 1) / (job.tile.plant.small ? 2f : 1f)), false);
						}
						job.tile.SetPlant(true, null);
					}
				}
			},
			{
				"PlantPlant",
				new() {
					delegate (Job job, Colonist colonist) {
						foreach (ResourceManager.ResourceAmount ra in job.requiredResources) {
							colonist.GetInventory().ChangeResourceAmount(ra.resource, -ra.amount, false);
						}
						Dictionary<ResourceManager.PlantEnum, float> plantChances = job.tile.biome.plantChances
							.Where(plantChance => job.variation.plants
								.Select(plant => plant.Key.type)
								.Contains(plantChance.Key))
							.ToDictionary(p => p.Key, p => p.Value);
						float totalPlantChanceWeight = plantChances.Sum(plantChance => plantChance.Value);
						float randomRangeValue = UnityEngine.Random.Range(0, totalPlantChanceWeight);
						float iterativeTotal = 0;
						ResourceManager.PlantEnum chosenPlantEnum = plantChances.First().Key;
						foreach (KeyValuePair<ResourceManager.PlantEnum, float> plantChanceKVP in plantChances) {
							if (randomRangeValue >= iterativeTotal && randomRangeValue <= iterativeTotal + plantChanceKVP.Value) {
								chosenPlantEnum = plantChanceKVP.Key;
								break;
							} else {
								iterativeTotal += plantChanceKVP.Value;
							}
						}
						ResourceManager.PlantPrefab chosenPlantPrefab = GameManager.resourceM.GetPlantPrefabByEnum(chosenPlantEnum);

						job.tile.SetPlant(false, new ResourceManager.Plant(chosenPlantPrefab, job.tile, true, false, job.variation.plants[chosenPlantPrefab]));
						GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.Time.TileBrightnessTime, true);
					}
				}
			},
			{
				"Mine",
				new() {
					delegate (Job job, Colonist colonist) {
						foreach (ResourceManager.ResourceRange resourceRange in job.tile.tileType.resourceRanges) {
							colonist.GetInventory().ChangeResourceAmount(resourceRange.resource, UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1), false);
						}
						if (job.tile.HasRoof()) {
							job.tile.SetTileType(TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Dirt), false, true, true);
						} else {
							job.tile.SetTileType(job.tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Ground], false, true, true);
						}

						GameManager.colonyM.colony.map.RemoveTileBrightnessEffect(job.tile);

						foreach (ResourceManager.LightSource lightSource in GameManager.resourceM.lightSources) {
							if (Vector2.Distance(job.tile.obj.transform.position, lightSource.obj.transform.position) <= lightSource.prefab.maxLightDistance) {
								lightSource.SetTileBrightnesses();
							}
						}
					}
				}
			},
			{
				"Dig",
				new() {
					delegate (Job job, Colonist colonist) {
						job.tile.dugPreviously = true;
						foreach (ResourceManager.ResourceRange resourceRange in job.tile.tileType.resourceRanges) {
							colonist.GetInventory().ChangeResourceAmount(resourceRange.resource, UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1), false);
						}
						bool setToWater = job.tile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water;
						if (!setToWater) {
							foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
								if (nTile != null && nTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
									setToWater = true;
									break;
								}
							}
						}
						if (setToWater) {
							job.tile.SetTileType(job.tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Water], false, true, true);
							foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
								if (nTile != null && nTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Hole) {
									List<TileManager.Tile> frontier = new List<TileManager.Tile>() { nTile };
									List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>() { };
									TileManager.Tile currentTile = nTile;
									while (frontier.Count > 0) {
										currentTile = frontier[0];
										frontier.RemoveAt(0);
										checkedTiles.Add(currentTile);
										currentTile.SetTileType(currentTile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Water], true, true, true);
										foreach (TileManager.Tile nTile2 in currentTile.horizontalSurroundingTiles) {
											if (nTile2 != null && nTile2.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Hole && !checkedTiles.Contains(nTile2)) {
												frontier.Add(nTile2);
											}
										}
									}
								}
							}
						} else {
							job.tile.SetTileType(job.tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Hole], false, true, true);
						}
					}
				}
			},
			{
				"Fill",
				new() {
					delegate (Job job, Colonist colonist) {
						TileManager.TileType fillType = TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Dirt);
						job.tile.dugPreviously = false;
						foreach (ResourceManager.ResourceRange resourceRange in fillType.resourceRanges) {
							colonist.GetInventory().ChangeResourceAmount(resourceRange.resource, -(resourceRange.max + 1), true);
						}
						job.tile.SetTileType(fillType, false, true, true);
					}
				}
			},
			{
				"CreateResource",
				new() {
					delegate (Job job, Colonist colonist) {
						foreach (ResourceManager.ResourceAmount resourceAmount in job.requiredResources) {
							colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
						}
						colonist.GetInventory().ChangeResourceAmount(job.createResource.resource, job.createResource.resource.amountCreated, false);

						switch (job.createResource.creationMethod) {
							case ResourceManager.CreationMethod.SingleRun:
								job.createResource.SetRemainingAmount(job.createResource.GetRemainingAmount() - job.createResource.resource.amountCreated);
								break;
							case ResourceManager.CreationMethod.MaintainStock:
								job.createResource.SetRemainingAmount(job.createResource.GetTargetAmount() - job.createResource.resource.GetAvailableAmount());
								break;
							case ResourceManager.CreationMethod.ContinuousRun:
								job.createResource.SetRemainingAmount(0);
								break;
						}
						job.createResource.job = null;
					}
				}
			},
			{
				"PickupResources",
				new() {
					delegate (Job job, Colonist colonist) {
						ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
						if (container != null && colonist.storedJob != null) {
							ContainerPickup containerPickup = colonist.storedJob.containerPickups.Find(pickup => pickup.container == container);
							if (containerPickup != null) {
								foreach (ResourceManager.ReservedResources rr in containerPickup.container.GetInventory().TakeReservedResources(colonist, containerPickup.resourcesToPickup)) {
									foreach (ResourceManager.ResourceAmount ra in rr.resources) {
										if (containerPickup.resourcesToPickup.Find(rtp => rtp.resource == ra.resource) != null) {
											colonist.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);
										}
									}
								}
								colonist.storedJob.containerPickups.RemoveAt(0);
							}
						}
						if (colonist.storedJob != null) {
							if (colonist.storedJob.containerPickups.Count <= 0) {
								colonist.SetJob(new ColonistJob(colonist, colonist.storedJob, colonist.storedJob.resourcesColonistHas, null));
								colonist.storedJob = null;
							} else {
								colonist.SetJob(
									new ColonistJob(
										colonist,
										new Job(
											JobPrefab.GetJobPrefabByName("PickupResources"),
											colonist.storedJob.containerPickups[0].container.tile,
											GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.PickupResources),
											null,
											0),
										colonist.storedJob.resourcesColonistHas,
										colonist.storedJob.containerPickups),
									false
								);
							}
						}
					}
				}
			},
			{
				"TransferResources",
				new() {
					delegate (Job job, Colonist colonist) {
						ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
						if (container != null) {
							ResourceManager.Inventory.TransferResourcesBetweenInventories(colonist.GetInventory(), container.GetInventory(), job.requiredResources, true);
						}
					}
				}
			},
			{
				"CollectResources",
				new() {
					delegate (Job job, Colonist colonist) {
						ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
						if (container != null) {
							foreach (ResourceManager.ReservedResources rr in container.GetInventory().TakeReservedResources(colonist)) {
								foreach (ResourceManager.ResourceAmount ra in rr.resources) {
									colonist.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);
								}
							}
						}
					}
				}
			},
			{
				"EmptyInventory",
				new() {
					delegate (Job job, Colonist colonist) {
						ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
						if (container != null) {
							ResourceManager.Inventory.TransferResourcesBetweenInventories(
								colonist.GetInventory(), // fromInventory
								container.GetInventory(), // toInventory
								colonist.GetInventory().resources, // resourceAmounts
								true // limitToMaxAmount
							);
						}
					}
				}
			},
			{
				"CollectFood",
				new() {
					delegate (Job job, Colonist colonist) {
						ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
						if (container != null) {
							foreach (ResourceManager.ReservedResources rr in container.GetInventory().TakeReservedResources(colonist)) {
								foreach (ResourceManager.ResourceAmount ra in rr.resources) {
									colonist.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);
								}
							}
						}
						colonist.SetEatJob();
					}
				}
			},
			{
				"Eat",
				new() {
					delegate (Job job, Colonist colonist) {

						List<ResourceManager.ResourceAmount> resourcesToEat = colonist.GetInventory().resources.Where(r => r.resource.classes.Contains(ResourceManager.ResourceClassEnum.Food)).OrderBy(r => ((ResourceManager.Food)r.resource).nutrition).ToList();
						NeedInstance foodNeed = colonist.needs.Find(need => need.prefab.type == ENeed.Food);

						float startingFoodNeedValue = foodNeed.GetValue();
						foreach (ResourceManager.ResourceAmount ra in resourcesToEat) {
							bool stopEating = false;
							for (int i = 0; i < ra.amount; i++) {
								if (foodNeed.GetValue() <= 0) {
									stopEating = true;
									break;
								}
								foodNeed.ChangeValue(-((ResourceManager.Food)ra.resource).nutrition);
								colonist.GetInventory().ChangeResourceAmount(ra.resource, -1, false);
								if (ra.resource.type == ResourceManager.ResourceEnum.Apple || ra.resource.type == ResourceManager.ResourceEnum.BakedApple) {
									colonist.GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.AppleSeed), UnityEngine.Random.Range(1, 5), false);
								}
							}
							if (stopEating) {
								break;
							}
						}

						float amountEaten = startingFoodNeedValue - foodNeed.GetValue();
						if (amountEaten >= 15 && foodNeed.GetValue() <= -10) {
							colonist.AddMoodModifier(MoodModifierEnum.Stuffed);
						} else if (amountEaten >= 15) {
							colonist.AddMoodModifier(MoodModifierEnum.Full);
						}

						if (foodNeed.GetValue() < 0) {
							foodNeed.SetValue(0);
						}

						ResourceManager.ObjectInstance objectOnTile = colonist.overTile.GetObjectInstanceAtLayer(2);
						if (objectOnTile != null && objectOnTile.prefab.subGroupType == ResourceManager.ObjectSubGroupEnum.Chairs) {
							if (objectOnTile.tile.surroundingTiles.Find(tile => {
								ResourceManager.ObjectInstance tableNextToChair = tile.GetObjectInstanceAtLayer(2);
								if (tableNextToChair != null) {
									return tableNextToChair.prefab.subGroupType == ResourceManager.ObjectSubGroupEnum.Tables;
								}
								return false;
							}) == null) {
								colonist.AddMoodModifier(MoodModifierEnum.AteWithoutATable);
							}
						} else {
							colonist.AddMoodModifier(MoodModifierEnum.AteOnTheFloor);
						}
					}
				}
			},
			{
				"Sleep",
				new() {
					delegate (Job job, Colonist colonist) {
						ResourceManager.SleepSpot targetSleepSpot = GameManager.resourceM.sleepSpots.Find(sleepSpot => sleepSpot.tile == job.tile);
						if (targetSleepSpot != null) {
							targetSleepSpot.StopSleeping();
							if (targetSleepSpot.prefab.restComfortAmount >= 10) {
								colonist.AddMoodModifier(MoodModifierEnum.WellRested);
							} else {
								colonist.AddMoodModifier(MoodModifierEnum.Rested);
							}
						} else {
							colonist.AddMoodModifier(MoodModifierEnum.Rested);
						}
						foreach (ResourceManager.SleepSpot sleepSpot in GameManager.resourceM.sleepSpots) {
							if (sleepSpot.occupyingColonist == colonist) {
								sleepSpot.StopSleeping();
							}
						}
					}
				}
			},
			{
				"WearClothes",
				new() {
					delegate (Job job, Colonist colonist) {
						foreach (ResourceManager.ResourceAmount resourceAmount in job.requiredResources.Where(ra => ra.resource.classes.Contains(ResourceManager.ResourceClassEnum.Clothing))) {
							ResourceManager.Clothing clothing = (ResourceManager.Clothing)resourceAmount.resource;
							colonist.ChangeClothing(clothing.prefab.appearance, clothing);
						}
					}
				}
			}
		};
	}
}
