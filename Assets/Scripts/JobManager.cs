using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobManager : BaseManager {

	public override void Awake() {
		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
		selectedPrefabPreview.GetComponent<SpriteRenderer>().color = UIManager.GetColour(UIManager.Colours.WhiteAlpha128);
	}

	private bool changedJobList = false;
	private int rotationIndex = 0;

	public override void Update() {
		if (changedJobList) {
			UpdateColonistJobs();
			GameManager.uiM.SetJobElements();
			changedJobList = false;
		}
		GetJobSelectionArea();
		UpdateSelectedPrefabInfo();
	}

	public List<Job> jobs = new List<Job>();

	public class Job {

		public TileManager.Tile tile;
		public ResourceManager.ObjectPrefab prefab;
		public ResourceManager.Variation variation;
		public ColonistManager.Colonist colonist;

		public int rotationIndex;

		public GameObject jobPreview;
		public GameObject priorityIndicator;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> resourcesToBuild = new List<ResourceManager.ResourceAmount>();

		public List<ResourceManager.ResourceAmount> resourcesColonistHas;
		public List<ContainerPickup> containerPickups;

		public UIManager.JobElement jobUIElement;

		public ResourceManager.CraftableResourceInstance createResource;
		public ResourceManager.ObjectInstance activeObject;

		public int priority;

		public List<ResourceManager.ResourceAmount> transferResources;

		public Job(TileManager.Tile tile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation, int rotationIndex) {

			this.tile = tile;
			this.prefab = prefab;
			this.variation = variation;

			resourcesToBuild.AddRange(prefab.commonResources);
			if (variation != null) {
				resourcesToBuild.AddRange(variation.uniqueResources);
			}

			this.rotationIndex = rotationIndex;

			jobPreview = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, GameManager.resourceM.jobParent.transform, false);
			jobPreview.transform.position = tile.obj.transform.position + (Vector3)prefab.anchorPositionOffset[rotationIndex];
			jobPreview.name = "JobPreview: " + prefab.name + " at " + jobPreview.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.GetBaseSpriteForVariation(variation) != null) {
				jPSR.sprite = prefab.GetBaseSpriteForVariation(variation);
			}
			if (!prefab.bitmasking && prefab.GetBitmaskSpritesForVariation(variation).Count > 0) {
				jPSR.sprite = prefab.GetBitmaskSpritesForVariation(variation)[rotationIndex];
			}
			jPSR.sortingOrder = 5 + prefab.layer; // Job Preview Sprite
			jPSR.color = UIManager.GetColour(UIManager.Colours.WhiteAlpha128);

			jobProgress = prefab.timeToBuild;
			colonistBuildTime = prefab.timeToBuild;
		}

		public void SetCreateResourceData(ResourceManager.CraftableResourceInstance resource, bool addToResourcesToBuild = true) {
			createResource = resource;

			jobProgress += createResource.resource.craftingTime;
			colonistBuildTime += createResource.resource.craftingTime;
			
			if (addToResourcesToBuild) {
				resourcesToBuild.AddRange(createResource.resource.craftingResources);
				if (resource.resource.craftingEnergy != 0) {
					resourcesToBuild.AddRange(resource.fuelAmounts);
				}
			}
			activeObject = resource.craftingObject;
			jobPreview.GetComponent<SpriteRenderer>().sprite = resource.resource.image;
		}

		public void SetColonist(ColonistManager.Colonist colonist) {
			this.colonist = colonist;
			if (prefab.jobType != JobEnum.PickupResources && containerPickups != null && containerPickups.Count > 0) {
				colonist.storedJob = this;
				colonist.SetJob(
					new ColonistJob(
						colonist, 
						new Job(
							containerPickups[0].container.tile, 
							GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.PickupResources), 
							null, 
							0), 
						null, 
						null
					)
				);
			}
		}

		public void ChangePriority(int amount) {
			priority += amount;
			if (priorityIndicator == null && jobPreview != null) {
				priorityIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, jobPreview.transform, false);
				priorityIndicator.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(@"UI/priorityIndicator");
				priorityIndicator.GetComponent<SpriteRenderer>().sortingOrder = jobPreview.GetComponent<SpriteRenderer>().sortingOrder + 1; // Priority Indicator Sprite
				if (priority == 1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = UIManager.GetColour(UIManager.Colours.LightYellow);
				} else if (priority == -1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = UIManager.GetColour(UIManager.Colours.LightRed);
				}
			}
			if (priority == 0) {
				MonoBehaviour.Destroy(priorityIndicator);
			}
		}

		public void Remove() {
			if (jobUIElement != null) {
				jobUIElement.Remove();
			}
			MonoBehaviour.Destroy(jobPreview);
		}
	}

	public enum JobEnum {
		Build, Remove,
		ChopPlant, PlantPlant, Mine, Dig, Fill, PlantFarm, HarvestFarm,
		CreateResource, PickupResources, TransferResources, CollectResources, EmptyInventory, Cancel, IncreasePriority, DecreasePriority,
		CollectFood, Eat, CollectWater, Drink, Sleep, WearClothes
	};

	public static readonly List<JobEnum> nonReturnableJobs = new List<JobEnum>() {
		JobEnum.PickupResources, JobEnum.EmptyInventory, JobEnum.Cancel, JobEnum.IncreasePriority, JobEnum.DecreasePriority,
		JobEnum.CollectFood, JobEnum.Eat, JobEnum.CollectWater, JobEnum.Drink, JobEnum.Sleep, JobEnum.WearClothes
	};

	private static readonly Dictionary<JobEnum, Func<Job, string>> jobDescriptionFunctions = new Dictionary<JobEnum, Func<Job, string>>() {
		{ JobEnum.Build, delegate (Job job) {
			return $"Building a {job.prefab.name}.";
		} },
		{ JobEnum.Remove, delegate (Job job) {
			if (job.prefab.type == ResourceManager.ObjectEnum.RemoveRoof) {
				return $"Removing a roof.";
			} else {
				return $"Removing a {job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.name}.";
			}
		} },
		{ JobEnum.ChopPlant, delegate (Job job) {
			return $"Chopping down a {job.tile.plant.prefab.name}.";
		} },
		{ JobEnum.PlantPlant, delegate (Job job) {
			return $"Planting a plant.";
		} },
		{ JobEnum.Mine, delegate (Job job) {
			return $"Mining {job.tile.tileType.name}.";
		} },
		{ JobEnum.Dig, delegate (Job job) {
			return $"Digging {string.Join(" and ", job.tile.tileType.resourceRanges.Select(rr => rr.resource.name).ToArray())}";
		} },
		{ JobEnum.Fill, delegate (Job job) {
			return $"Filling {job.tile.tileType.groupType.ToString().ToLower()}.";
		} },
		{ JobEnum.PlantFarm, delegate (Job job) {
			return $"Planting a {job.prefab.name}.";
		} },
		{ JobEnum.HarvestFarm, delegate (Job job) {
			return $"Harvesting a farm of {job.tile.farm.name}.";
		} },
		{ JobEnum.CreateResource, delegate (Job job) {
			return $"Creating {job.createResource.resource.name}.";
		} },
		{ JobEnum.PickupResources, delegate (Job job) {
			return $"Picking up some resources.";
		} },
		{ JobEnum.TransferResources, delegate (Job job) {
			return $"Transferring resources.";
		} },
		{ JobEnum.CollectResources, delegate (Job job) {
			return $"Collecting resources.";
		} },
		{ JobEnum.EmptyInventory, delegate (Job job) {
			return $"Emptying inventory.";
		} },
		{ JobEnum.CollectFood, delegate (Job job) {
			return $"Finding some food to eat.";
		} },
		{ JobEnum.Eat, delegate (Job job) {
			return $"Eating.";
		} },
		{ JobEnum.CollectWater, delegate (Job job) {
			return $"Finding something to drink.";
		} },
		{ JobEnum.Drink, delegate (Job job) {
			return $"Drinking.";
		} },
		{ JobEnum.Sleep, delegate (Job job) {
			return $"Sleeping.";
		} },
		{ JobEnum.WearClothes, delegate (Job job) {
			return $"Wearing {job.resourcesToBuild[0].resource.name}.";
		} }
	};

	public string GetJobDescription(Job job) {
		if (job != null) {
			if (jobDescriptionFunctions.ContainsKey(job.prefab.jobType)) {
				return jobDescriptionFunctions[job.prefab.jobType](job);
			} else {
				return "Doing something.";
			}
		} else {
			return "Wandering around.";
		}
	}

	public static readonly Dictionary<JobEnum, Action<ColonistManager.Colonist, Job>> finishJobFunctions = new Dictionary<JobEnum, Action<ColonistManager.Colonist, Job>>() {
		{ JobEnum.Build, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
			}
			if (job.prefab.subGroupType == ResourceManager.ObjectSubGroupEnum.Roofs) {
				job.tile.SetRoof(true);
			}
		} },
		{ JobEnum.Remove, delegate (ColonistManager.Colonist colonist, Job job) {
			bool previousWalkability = job.tile.walkable;
			ResourceManager.ObjectInstance instance = job.tile.GetObjectInstanceAtLayer(job.prefab.layer);
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
				} else if (instance is ResourceManager.Container container) {
					List<ResourceManager.ResourceAmount> nonReservedResourcesToRemove = new List<ResourceManager.ResourceAmount>();
					foreach (ResourceManager.ResourceAmount resourceAmount in container.GetInventory().resources) {
						nonReservedResourcesToRemove.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
						colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
					}
					foreach (ResourceManager.ResourceAmount resourceAmount in nonReservedResourcesToRemove) {
						container.GetInventory().ChangeResourceAmount(resourceAmount.resource, -resourceAmount.amount, false);
					}
					List<ResourceManager.ReservedResources> reservedResourcesToRemove = new List<ResourceManager.ReservedResources>();
					foreach (ResourceManager.ReservedResources reservedResources in container.GetInventory().reservedResources) {
						foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
							colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
						}
						reservedResourcesToRemove.Add(reservedResources);
						if (reservedResources.human is ColonistManager.Colonist human) {
							human.ReturnJob();
						}
					}
					foreach (ResourceManager.ReservedResources reservedResourceToRemove in reservedResourcesToRemove) {
						container.GetInventory().reservedResources.Remove(reservedResourceToRemove);
					}
					GameManager.uiM.SetSelectedColonistInformation(true);
					GameManager.uiM.SetSelectedContainerInfo();
					GameManager.uiM.UpdateSelectedTradingPostInfo();
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
				switch (job.prefab.type) {
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
		} },
		{ JobEnum.PlantFarm, delegate (ColonistManager.Colonist colonist, Job job) {
			JobManager.finishJobFunctions[JobEnum.Build](colonist, job);
			if (job.tile.tileType.classes[TileManager.TileType.ClassEnum.Dirt]) {
				job.tile.SetTileType(TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Mud), false, true, false);
			}
		} },
		{ JobEnum.HarvestFarm, delegate (ColonistManager.Colonist colonist, Job job) {
			if (job.tile.farm != null) {
				colonist.GetInventory().ChangeResourceAmount(job.tile.farm.prefab.seedResource, UnityEngine.Random.Range(1, 3), false);
				foreach (ResourceManager.ResourceRange harvestResourceRange in job.tile.farm.prefab.harvestResources) {
					colonist.GetInventory().ChangeResourceAmount(harvestResourceRange.resource, UnityEngine.Random.Range(harvestResourceRange.min, harvestResourceRange.max), false);
				}

				GameManager.jobM.CreateJob(new Job(job.tile, job.tile.farm.prefab, job.tile.farm.variation, 0));

				int layer = job.tile.farm.prefab.layer; // Required because RemoveObjectInstance sets job.tile.farm = null but must happen before RemoveObjectAtLayer
				GameManager.resourceM.RemoveObjectInstance(job.tile.farm);
				job.tile.RemoveObjectAtLayer(layer);
			}
			GameManager.resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
		} },
		{ JobEnum.ChopPlant, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceRange resourceRange in job.tile.plant.prefab.returnResources) {
				colonist.GetInventory().ChangeResourceAmount(resourceRange.resource, Mathf.CeilToInt(UnityEngine.Random.Range(resourceRange.min, resourceRange.max + 1) / (job.tile.plant.small ? 2f : 1f)), false);
			}
			if (job.tile.plant.harvestResource != null) {
				ResourceManager.ResourceRange harvestResourceRange = job.tile.plant.prefab.harvestResources.Find(rr => rr.resource == job.tile.plant.harvestResource);
				colonist.GetInventory().ChangeResourceAmount(job.tile.plant.harvestResource, Mathf.CeilToInt(UnityEngine.Random.Range(harvestResourceRange.min, harvestResourceRange.max + 1) / (job.tile.plant.small ? 2f : 1f)), false);
			}
			job.tile.SetPlant(true, null);
		} },
		{ JobEnum.PlantPlant, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount ra in job.resourcesToBuild) {
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
			Debug.Log(chosenPlantPrefab.name);
			job.tile.SetPlant(false, new ResourceManager.Plant(chosenPlantPrefab, job.tile, true, false, job.variation.plants[chosenPlantPrefab]));
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
		} },
		{ JobEnum.Mine, delegate (ColonistManager.Colonist colonist, Job job) {
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
		} },
		{ JobEnum.Dig, delegate (ColonistManager.Colonist colonist, Job job) {
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
		} },
		{ JobEnum.Fill, delegate (ColonistManager.Colonist colonist, Job job) {
			TileManager.TileType fillType = TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Dirt);
			job.tile.dugPreviously = false;
			foreach (ResourceManager.ResourceRange resourceRange in fillType.resourceRanges) {
				colonist.GetInventory().ChangeResourceAmount(resourceRange.resource, -(resourceRange.max + 1), true);
			}
			job.tile.SetTileType(fillType, false, true, true);
		} },
		{ JobEnum.CreateResource, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
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
		} },
		{ JobEnum.PickupResources, delegate (ColonistManager.Colonist colonist, Job job) {
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
		} },
		{ JobEnum.TransferResources, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				ResourceManager.Inventory.TransferResourcesBetweenInventories(colonist.GetInventory(), container.GetInventory(), job.resourcesToBuild, true);
			}
		} },
		{ JobEnum.CollectResources, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				foreach (ResourceManager.ReservedResources rr in container.GetInventory().TakeReservedResources(colonist)) {
					foreach (ResourceManager.ResourceAmount ra in rr.resources) {
						colonist.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);
					}
				}
			}
		} },
		{ JobEnum.EmptyInventory, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				ResourceManager.Inventory.TransferResourcesBetweenInventories(
					colonist.GetInventory(), // fromInventory
					container.GetInventory(), // toInventory
					colonist.GetInventory().resources, // resourceAmounts
					true // limitToMaxAmount
				);
			}
		} },
		{ JobEnum.CollectFood, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.Container container = GameManager.resourceM.GetContainerOrChildOnTile(colonist.overTile);
			if (container != null) {
				foreach (ResourceManager.ReservedResources rr in container.GetInventory().TakeReservedResources(colonist)) {
					foreach (ResourceManager.ResourceAmount ra in rr.resources) {
						colonist.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);
					}
				}
			}
			colonist.SetEatJob();
		} },
		{ JobEnum.Eat, delegate (ColonistManager.Colonist colonist, Job job) {
			
			List<ResourceManager.ResourceAmount> resourcesToEat = colonist.GetInventory().resources.Where(r => r.resource.classes.Contains(ResourceManager.ResourceClassEnum.Food)).OrderBy(r => ((ResourceManager.Food)r.resource).nutrition).ToList();
			ColonistManager.NeedInstance foodNeed = colonist.needs.Find(need => need.prefab.type == ColonistManager.NeedEnum.Food);

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
				colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.Stuffed);
			} else if (amountEaten >= 15) {
				colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.Full);
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
					colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.AteWithoutATable);
				}
			} else {
				colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.AteOnTheFloor);
			}
		} },
		{ JobEnum.Sleep, delegate (ColonistManager.Colonist colonist, Job job) {
			ResourceManager.SleepSpot targetSleepSpot = GameManager.resourceM.sleepSpots.Find(sleepSpot => sleepSpot.tile == job.tile);
			if (targetSleepSpot != null) {
				targetSleepSpot.StopSleeping();
				if (targetSleepSpot.prefab.restComfortAmount >= 10) {
					colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.WellRested);
				} else {
					colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.Rested);
				}
			} else {
				colonist.AddMoodModifier(ColonistManager.MoodModifierEnum.Rested);
			}
			foreach (ResourceManager.SleepSpot sleepSpot in GameManager.resourceM.sleepSpots) {
				if (sleepSpot.occupyingColonist == colonist) {
					sleepSpot.StopSleeping();
				}
			}
		} },
		{ JobEnum.WearClothes, delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild.Where(ra => ra.resource.classes.Contains(ResourceManager.ResourceClassEnum.Clothing))) {
				ResourceManager.Clothing clothing = (ResourceManager.Clothing)resourceAmount.resource;
				colonist.ChangeClothing(clothing.prefab.appearance, clothing);
			}
		} }
	};

	private SelectedPrefab selectedPrefab;

	public class SelectedPrefab {
		public readonly ResourceManager.ObjectPrefab prefab;
		public readonly ResourceManager.Variation variation;

		public SelectedPrefab(
			// Update the Equals method below whenever adding/removing parameters
			ResourceManager.ObjectPrefab prefab,
			ResourceManager.Variation variation
		) {
			this.prefab = prefab;
			this.variation = variation;
		}
	}

	public void SetSelectedPrefab(ResourceManager.ObjectPrefab newPrefab, ResourceManager.Variation newVariation) {
		if (selectedPrefab == null || selectedPrefab.prefab != newPrefab || !ResourceManager.Variation.Equals(selectedPrefab.variation, newVariation)) {
			if (newPrefab != null) {
				selectedPrefab = new SelectedPrefab(newPrefab, newVariation);
				rotationIndex = 0;
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.prefab.GetBaseSpriteForVariation(selectedPrefab.variation);
				}
			} else {
				selectedPrefab = null;
			}
		}
	}

	public SelectedPrefab GetSelectedPrefab() {
		return selectedPrefab;
	}

	private GameObject selectedPrefabPreview;
	public void SelectedPrefabPreview() {
		Vector2 mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		TileManager.Tile tile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
		selectedPrefabPreview.transform.position = tile.obj.transform.position + (Vector3)selectedPrefab.prefab.anchorPositionOffset[rotationIndex];
	}

	public void UpdateSelectedPrefabInfo() {
		if (selectedPrefab != null) {
			if (enableSelectionPreview) {
				if (!selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(true);
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.prefab.GetBaseSpriteForVariation(selectedPrefab.variation);
					if (selectedPrefab.prefab.canRotate) {
						selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.prefab.GetBitmaskSpritesForVariation(selectedPrefab.variation)[rotationIndex];
					}
					GameManager.uiM.GetSelectionSizePanel().SetActive(false);
				}
				SelectedPrefabPreview();
				if (Input.GetKeyDown(KeyCode.R)) {
					if (selectedPrefab.prefab.canRotate) {
						rotationIndex += 1;
						if (rotationIndex >= selectedPrefab.prefab.GetBitmaskSpritesForVariation(selectedPrefab.variation).Count) {
							rotationIndex = 0;
						}
						selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.prefab.GetBitmaskSpritesForVariation(selectedPrefab.variation)[rotationIndex];
					}
				}
			} else {
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(false);
				}
				GameManager.uiM.GetSelectionSizePanel().SetActive(true);
			}
		} else {
			selectedPrefabPreview.SetActive(false);
			GameManager.uiM.GetSelectionSizePanel().SetActive(false);
		}
	}

	public enum SelectionModifiersEnum {
		Outline, Walkable, OmitWalkable, WalkableIncludingFences, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms, Roofs, OmitRoofs, CloseToSupport,
		ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, BiomeSupportsSelectedPlants, OmitObjectInstancesOnAdditionalTiles, Fillable
	};

	private static readonly Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ResourceManager.ObjectPrefab, ResourceManager.Variation, bool>> selectionModifierFunctions = new Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ResourceManager.ObjectPrefab, ResourceManager.Variation, bool>>() {
		{ SelectionModifiersEnum.Walkable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.walkable;
		} },
		{ SelectionModifiersEnum.WalkableIncludingFences, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			ResourceManager.ObjectInstance objectInstance = posTile.GetObjectInstanceAtLayer(2);
			if (objectInstance != null && objectInstance.prefab.subGroupType == ResourceManager.ObjectSubGroupEnum.Fences) {
				return true;
			}
			return posTile.walkable;
		} },
		{ SelectionModifiersEnum.OmitWalkable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return !posTile.walkable;
		} },
		{ SelectionModifiersEnum.Buildable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.buildable;
		} },
		{ SelectionModifiersEnum.OmitBuildable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return !posTile.buildable;
		} },
		{ SelectionModifiersEnum.StoneTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Stone;
		} },
		{ SelectionModifiersEnum.OmitStoneTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Stone;
		} },
		{ SelectionModifiersEnum.AllWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water;
		} },
		{ SelectionModifiersEnum.OmitAllWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water;
		} },
		{ SelectionModifiersEnum.LiquidWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater];
		} },
		{ SelectionModifiersEnum.OmitLiquidWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return !posTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater];
		} },
		{ SelectionModifiersEnum.OmitNonStoneAndWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water && posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Stone;
		} },
		{ SelectionModifiersEnum.Plants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.plant != null;
		} },
		{ SelectionModifiersEnum.OmitPlants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.plant == null;
		} },
		{ SelectionModifiersEnum.OmitSameLayerJobs, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			foreach (Job job in GameManager.jobM.jobs) {
				if (job.prefab.layer == prefab.layer) {
					if (job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in job.prefab.multiTilePositions[job.rotationIndex]) {
						if (GameManager.colonyM.colony.map.GetTileFromPosition(job.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
				if (colonist.job != null && colonist.job.prefab.layer == prefab.layer) {
					if (colonist.job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.job.prefab.multiTilePositions[colonist.job.rotationIndex]) {
						if (GameManager.colonyM.colony.map.GetTileFromPosition(colonist.job.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
				if (colonist.storedJob != null && colonist.storedJob.prefab.layer == prefab.layer) {
					if (colonist.storedJob.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.storedJob.prefab.multiTilePositions[colonist.storedJob.rotationIndex]) {
						if (GameManager.colonyM.colony.map.GetTileFromPosition(colonist.storedJob.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			return true;
		} },
		{ SelectionModifiersEnum.OmitSameLayerObjectInstances, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return !posTile.objectInstances.ContainsKey(prefab.layer) || posTile.objectInstances[prefab.layer] == null;
		} },
		{ SelectionModifiersEnum.Farms, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.farm != null;
		} },
		{ SelectionModifiersEnum.OmitFarms, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.farm == null;
		} },
		{ SelectionModifiersEnum.Roofs, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.HasRoof();
		} },
		{ SelectionModifiersEnum.OmitRoofs, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return !posTile.HasRoof();
		} },
		{ SelectionModifiersEnum.CloseToSupport, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			for (int y = -5; y < 5; y++) {
				for (int x = -5; x < 5; x++) {
					TileManager.Tile supportTile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(posTile.position.x + x, posTile.position.y + y));
					if (!supportTile.buildable && !supportTile.walkable) {
						return true;
					}
				}
			}
			return false;
		} },
		{ SelectionModifiersEnum.ObjectsAtSameLayer, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.GetObjectInstanceAtLayer(prefab.layer) != null;
		} },
		{ SelectionModifiersEnum.Objects, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.GetAllObjectInstances().Count > 0;
		} },
		{ SelectionModifiersEnum.OmitObjects, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.GetAllObjectInstances().Count <= 0;
		} },
		{ SelectionModifiersEnum.OmitNonCoastWater, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			if (posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
				if (!(posTile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water) != null)) {
					return false;
				}
			}
			return true;
		} },
		{ SelectionModifiersEnum.OmitHoles, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Hole;
		} },
		{ SelectionModifiersEnum.OmitPreviousDig, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return !posTile.dugPreviously;
		} },
		{ SelectionModifiersEnum.BiomeSupportsSelectedPlants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.biome.plantChances.Keys.Intersect(variation.plants.Select(plant => plant.Key.type)).Any();
		} },
		{ SelectionModifiersEnum.OmitObjectInstancesOnAdditionalTiles, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			ResourceManager.ObjectInstance objectInstance = posTile.GetObjectInstanceAtLayer(prefab.layer);
			if (objectInstance != null && objectInstance.tile != posTile) {
				return false;
			}
			return true;
		} },
		{ SelectionModifiersEnum.Fillable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.ObjectPrefab prefab, ResourceManager.Variation variation) {
			return posTile.dugPreviously || (posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Hole || (posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water && selectionModifierFunctions[SelectionModifiersEnum.OmitNonCoastWater](tile, posTile, prefab, variation)));
		} },
	};

	private readonly List<GameObject> selectionIndicators = new List<GameObject>();

	public TileManager.Tile firstTile;
	private bool stopSelection;

	public void StopSelection() {
		stopSelection = true;
	}

	private bool enableSelectionPreview = true;

	public void GetJobSelectionArea() {

		enableSelectionPreview = true;

		foreach (GameObject selectionIndicator in selectionIndicators) {
			MonoBehaviour.Destroy(selectionIndicator);
		}
		selectionIndicators.Clear();

		if (selectedPrefab != null) {
			Vector2 mousePosition = GameManager.cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			if (Input.GetMouseButtonDown(0) && !GameManager.uiM.IsPointerOverUI()) {
				firstTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
			}
			if (firstTile != null) {
				if (stopSelection) {
					stopSelection = false;
					firstTile = null;
					return;
				}
				TileManager.Tile secondTile = GameManager.colonyM.colony.map.GetTileFromPosition(mousePosition);
				if (secondTile != null) {

					enableSelectionPreview = false;

					float smallerY = Mathf.Min(firstTile.obj.transform.position.y, secondTile.obj.transform.position.y);
					float largerY = Mathf.Max(firstTile.obj.transform.position.y, secondTile.obj.transform.position.y);
					float smallerX = Mathf.Min(firstTile.obj.transform.position.x, secondTile.obj.transform.position.x);
					float largerX = Mathf.Max(firstTile.obj.transform.position.x, secondTile.obj.transform.position.x);

					List<TileManager.Tile> selectionArea = new List<TileManager.Tile>();

					float maxY = largerY + 1;
					float maxX = largerX + 1;

					bool addedToSelectionArea = false;
					for (float y = smallerY; y < maxY; y += (addedToSelectionArea ? selectedPrefab.prefab.dimensions[rotationIndex].y : 1)) {
						for (float x = smallerX; x < maxX; x += (addedToSelectionArea ? selectedPrefab.prefab.dimensions[rotationIndex].x : 1)) {
							addedToSelectionArea = true; // default = false // Try swapping x and y values when the object is rotated vertically (i.e. rotationIndex == 1 || 3).
							TileManager.Tile tile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(x, y));
							bool addTile = true;
							bool addOutlineTile = true;
							if (selectedPrefab.prefab.selectionModifiers.Contains(SelectionModifiersEnum.Outline)) {
								addOutlineTile = (x == smallerX || y == smallerY || x == largerX || y == largerY);
							}
							foreach (SelectionModifiersEnum selectionModifier in selectedPrefab.prefab.selectionModifiers) {
								if (selectionModifier != SelectionModifiersEnum.Outline) {
									foreach (Vector2 multiTilePosition in selectedPrefab.prefab.multiTilePositions[rotationIndex]) {
										Vector2 actualMultiTilePosition = tile.obj.transform.position + (Vector3)multiTilePosition;
										if (actualMultiTilePosition.x >= 0 && actualMultiTilePosition.x < GameManager.colonyM.colony.map.mapData.mapSize && actualMultiTilePosition.y >= 0 && actualMultiTilePosition.y < GameManager.colonyM.colony.map.mapData.mapSize) {
											TileManager.Tile posTile = GameManager.colonyM.colony.map.GetTileFromPosition(actualMultiTilePosition);
											addTile = selectionModifierFunctions[selectionModifier](tile, posTile, selectedPrefab.prefab, selectedPrefab.variation);
											if (!addTile) {
												break;
											}
										} else {
											addTile = false;
											break;
										}
									}
									if (!addTile) {
										break;
									}
								}
							}
							if (addTile && addOutlineTile) {
								selectionArea.Add(tile);
								addedToSelectionArea = true;

								GameObject selectionIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, GameManager.resourceM.selectionParent.transform, false);
								selectionIndicator.transform.position = tile.obj.transform.position + (Vector3)selectedPrefab.prefab.anchorPositionOffset[rotationIndex]; ;
								selectionIndicator.name = "Selection Indicator";
								SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
								sISR.sprite = selectedPrefab.prefab.canRotate 
									? selectedPrefab.prefab.GetBitmaskSpritesForVariation(selectedPrefab.variation)[rotationIndex] 
									: selectedPrefab.prefab.GetBaseSpriteForVariation(selectedPrefab.variation);
								sISR.sortingOrder = 20; // Selection Indicator Sprite
								sISR.color = UIManager.GetColour(UIManager.Colours.WhiteAlpha64);
								selectionIndicators.Add(selectionIndicator);
							}
						}
					}

					GameManager.uiM.GetSelectionSizePanel().Update(smallerX - maxX, smallerY - maxY, selectionArea.Count);

					if (Input.GetMouseButtonUp(0)) {
						if (selectedPrefab.prefab.jobType == JobEnum.Cancel) {
							CancelJobsInSelectionArea(selectionArea);
						} else if (selectedPrefab.prefab.jobType == JobEnum.IncreasePriority) {
							ChangeJobPriorityInSelectionArea(selectionArea, 1);
						} else if (selectedPrefab.prefab.jobType == JobEnum.DecreasePriority) {
							ChangeJobPriorityInSelectionArea(selectionArea, -1);
						} else {
							CreateJobsInSelectionArea(selectedPrefab, selectionArea);
						}
						firstTile = null;
					}
				}
			}
		}
	}

	public void CancelJobsInSelectionArea(List<TileManager.Tile> selectionArea) {
		List<Job> removeJobs = new List<Job>();
		foreach (Job job in jobs) {
			if (selectionArea.Contains(job.tile)) {
				removeJobs.Add(job);
			}
		}
		foreach (Job job in removeJobs) {
			if (job.prefab.jobType == JobEnum.CreateResource) {
				job.createResource.job = null;
			}
			job.Remove();
			jobs.Remove(job);
		}
		removeJobs.Clear();

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			if (!((colonist.job != null && selectionArea.Contains(colonist.job.tile)) || (colonist.storedJob != null && selectionArea.Contains(colonist.storedJob.tile)))) {
				continue;
			}

			if (colonist.storedJob != null) {
				if (colonist.storedJob.containerPickups != null) {
					foreach (ContainerPickup containerPickup in colonist.storedJob.containerPickups) {
						containerPickup.container.GetInventory().ReleaseReservedResources(colonist);
					}
				}
				if (colonist.storedJob.prefab.jobType == JobEnum.CreateResource) {
					colonist.storedJob.createResource.job = null;
				}
				colonist.storedJob.Remove();
				colonist.storedJob = null;
			}

			if (colonist.job != null) {
				if (colonist.job.containerPickups != null) {
					foreach (ContainerPickup containerPickup in colonist.job.containerPickups) {
						containerPickup.container.GetInventory().ReleaseReservedResources(colonist);
					}
				}
				if (colonist.job.prefab.jobType == JobEnum.CreateResource) {
					colonist.job.createResource.job = null;
				}
				colonist.job.Remove();
				colonist.job = null;
				colonist.path.Clear();
				colonist.MoveToClosestWalkableTile(false);
			}
		}

		UpdateColonistJobs();
		GameManager.uiM.SetJobElements();
	}

	public void CancelJob(Job job) {
		ColonistManager.Colonist colonist = GameManager.colonistM.colonists.Find(c => c.job == job || c.storedJob == job);
		if (colonist != null) {
			if (job.containerPickups != null) {
				foreach (ContainerPickup containerPickup in job.containerPickups) {
					containerPickup.container.GetInventory().ReleaseReservedResources(colonist);
				}
			}

			if (colonist.job == job) {
				colonist.job = null;
				colonist.path.Clear();
				colonist.MoveToClosestWalkableTile(false);
			}

			if (colonist.storedJob == job) {
				colonist.storedJob = null;
			}
		}

		if (job.prefab.jobType == JobEnum.CreateResource) {
			job.createResource.job = null;
		}

		if (job.activeObject != null) {
			job.activeObject.SetActiveSprite(job, false);
		}

		job.Remove();
		jobs.Remove(job);

		UpdateColonistJobs();
		GameManager.uiM.SetJobElements();
	}

	public void ChangeJobPriorityInSelectionArea(List<TileManager.Tile> selectionArea, int amount) {
		foreach (Job job in jobs) {
			if (selectionArea.Contains(job.tile)) {
				job.ChangePriority(amount);
			}
		}
		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			if (colonist.job != null) {
				if (selectionArea.Contains(colonist.job.tile)) {
					colonist.job.ChangePriority(amount);
				}
			}
			if (colonist.storedJob != null) {
				if (selectionArea.Contains(colonist.storedJob.tile)) {
					colonist.job.ChangePriority(amount);
				}
			}
		}
		UpdateColonistJobs();
		UpdateAllColonistJobCosts();
		GameManager.uiM.SetJobElements();
	}

	private static readonly Dictionary<int, ResourceManager.ObjectEnum> removeLayerMap = new Dictionary<int, ResourceManager.ObjectEnum>() {
		{ 1, ResourceManager.ObjectEnum.RemoveFloor },
		{ 2, ResourceManager.ObjectEnum.RemoveObject }
	};

	public void CreateJobsInSelectionArea(SelectedPrefab selectedPrefab, List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			if (selectedPrefab.prefab.type == ResourceManager.ObjectEnum.RemoveAll) {
				foreach (ResourceManager.ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
					if (removeLayerMap.ContainsKey(objectInstance.prefab.layer) && !JobOfPrefabTypeExistsAtTile(removeLayerMap[objectInstance.prefab.layer], objectInstance.tile)) {
						ResourceManager.ObjectPrefab selectedRemovePrefab = GameManager.resourceM.GetObjectPrefabByEnum(removeLayerMap[objectInstance.prefab.layer]);
						bool createJobAtTile = true;
						foreach (SelectionModifiersEnum selectionModifier in selectedRemovePrefab.selectionModifiers) {
							if (selectionModifier != SelectionModifiersEnum.Outline) {
								createJobAtTile = selectionModifierFunctions[selectionModifier](objectInstance.tile, objectInstance.tile, selectedRemovePrefab, null);
								if (!createJobAtTile) {
									break;
								}
							}
						}
						if (createJobAtTile) {
							CreateJob(new Job(objectInstance.tile, selectedRemovePrefab, null, rotationIndex));
							objectInstance.SetActive(false);
						}
					}
				}
			} else {
				CreateJob(new Job(tile, selectedPrefab.prefab, selectedPrefab.variation, rotationIndex));
				foreach (ResourceManager.ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
					objectInstance.SetActive(false);
				}
			}
		}
	}

	public void CreateJob(Job newJob) {
		jobs.Add(newJob);
		changedJobList = true;
	}

	public void AddExistingJob(Job existingJob) {
		jobs.Add(existingJob);
		changedJobList = true;
	}

	public class ContainerPickup {
		public ResourceManager.Container container;
		public List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();

		public ContainerPickup(ResourceManager.Container container, List<ResourceManager.ResourceAmount> resourcesToPickup) {
			this.container = container;
			this.resourcesToPickup = resourcesToPickup;
		}
	}

	public List<ContainerPickup> CalculateColonistPickupContainers(ColonistManager.Colonist colonist, List<ResourceManager.ResourceAmount> resourcesToPickup) {
		List<ContainerPickup> containersToPickupFrom = new List<ContainerPickup>();
		List<ResourceManager.Container> sortedContainersByDistance = GameManager.resourceM.GetContainersInRegion(colonist.overTile.region).OrderBy(container => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, container.tile.regionBlock, true, true, false)).ToList();
		if (sortedContainersByDistance.Count > 0) {
			foreach (ResourceManager.Container container in sortedContainersByDistance) {
				List<ResourceManager.ResourceAmount> resourcesToPickupAtContainer = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount resourceAmount in container.GetInventory().resources.Where(ra => resourcesToPickup.Find(pickupResource => pickupResource.resource == ra.resource) != null)) {
					ResourceManager.ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.resource == resourceAmount.resource);
					if (resourceAmount.amount >= pickupResource.amount) {
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource, pickupResource.amount));
						resourcesToPickup.Remove(pickupResource);
					} else if (resourceAmount.amount > 0 && resourceAmount.amount < pickupResource.amount) {
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource, resourceAmount.amount));
						pickupResource.amount -= resourceAmount.amount;
						if (pickupResource.amount <= 0) {
							resourcesToPickup.Remove(pickupResource);
						}
					} else {
					}
				}
				if (resourcesToPickupAtContainer.Count > 0) {
					containersToPickupFrom.Add(new ContainerPickup(container, resourcesToPickupAtContainer));
				}
			}
			if (containersToPickupFrom.Count > 0) {
				if (resourcesToPickup.Count <= 0) {
					return containersToPickupFrom;
				} else {
					return null;
				}
			} else {
				return null;
			}
		} else {
			return null;
		}
	}

	public KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>> CalculateColonistResourcesToPickup(ColonistManager.Colonist colonist, List<ResourceManager.ResourceAmount> resourcesToFind) {
		bool colonistHasAllResources = true;
		List<ResourceManager.ResourceAmount> resourcesColonistHas = new List<ResourceManager.ResourceAmount>();
		List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();
		foreach (ResourceManager.ResourceAmount resourceAmount in resourcesToFind) {
			ResourceManager.ResourceAmount colonistResourceAmount = colonist.GetInventory().resources.Find(resource => resource.resource == resourceAmount.resource);
			if (colonistResourceAmount != null) {
				if (colonistResourceAmount.amount >= resourceAmount.amount) {
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
				} else if (colonistResourceAmount.amount > 0 && colonistResourceAmount.amount < resourceAmount.amount) {
					colonistHasAllResources = false;
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, colonistResourceAmount.amount));
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount - colonistResourceAmount.amount));
				} else {
					colonistHasAllResources = false;
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
				}
			} else {
				colonistHasAllResources = false;
				resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource, resourceAmount.amount));
			}
		}
		return new KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>>(colonistHasAllResources, new List<List<ResourceManager.ResourceAmount>>() { (resourcesToPickup.Count > 0 ? resourcesToPickup : null), (resourcesColonistHas.Count > 0 ? resourcesColonistHas : null) });
	}

	public float CalculateJobCost(ColonistManager.Colonist colonist, Job job, List<ContainerPickup> containerPickups) {
		/* The cost of a job is determined using:
				- the amount of resources the colonist has
					- sometimes the amount of resources containers have
				- the distance of the colonist to the job
					- sometimes the distance between the colonist position and the job following the path of pickups
				- the skill of the colonist
			The cost should be updated whenever:
				- an inventory is changed:
					- all colonists if any container's inventory is changed
					- single colonist is the colonist's inventory is changed
				- the colonist moves
				- the skill of the colonist changes
		*/
		float cost = 0;
		if (containerPickups != null) {
			for (int i = 0; i < containerPickups.Count; i++) {
				if (i == 0) {
					cost += PathManager.RegionBlockDistance(colonist.overTile.regionBlock, containerPickups[i].container.tile.regionBlock, true, true, true);
				} else {
					cost += PathManager.RegionBlockDistance(containerPickups[i - 1].container.tile.regionBlock, containerPickups[i].container.tile.regionBlock, true, true, true);
				}
			}
			cost += PathManager.RegionBlockDistance(job.tile.regionBlock, containerPickups[containerPickups.Count - 1].container.tile.regionBlock, true, true, true);
		} else {
			cost += PathManager.RegionBlockDistance(job.tile.regionBlock, colonist.overTile.regionBlock, true, true, true);
		}
		ColonistManager.SkillInstance skill = colonist.GetSkillFromJobType(job.prefab.jobType);
		if (skill != null) {
			cost -= skill.CalculateTotalSkillLevel() * 5f;
		}
		return cost;
	}

	public class ColonistJob {
		public ColonistManager.Colonist colonist;
		public Job job;

		public List<ResourceManager.ResourceAmount> resourcesColonistHas;
		public List<ContainerPickup> containerPickups;

		public float cost;

		public ColonistJob(ColonistManager.Colonist colonist, Job job, List<ResourceManager.ResourceAmount> resourcesColonistHas, List<ContainerPickup> containerPickups) {
			this.colonist = colonist;
			this.job = job;
			this.resourcesColonistHas = resourcesColonistHas;
			this.containerPickups = containerPickups;

			CalculateCost();
		}

		public void CalculateCost() {
			cost = GameManager.jobM.CalculateJobCost(colonist, job, containerPickups);
		}

		public void RecalculatePickupResources() {
			KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>> returnKVP = GameManager.jobM.CalculateColonistResourcesToPickup(colonist, job.resourcesToBuild);
			List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
			resourcesColonistHas = returnKVP.Value[1];
			if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
				containerPickups = GameManager.jobM.CalculateColonistPickupContainers(colonist, resourcesToPickup);
			} else {
				containerPickups = null;
			}
		}
	}

	public void UpdateColonistJobCosts(ColonistManager.Colonist colonist) {
		if (colonistJobs.ContainsKey(colonist)) {
			foreach (ColonistJob colonistJob in colonistJobs[colonist]) {
				colonistJob.CalculateCost();
			}
		}
	}

	public void UpdateAllColonistJobCosts() {
		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			UpdateColonistJobCosts(colonist);
		}
	}

	public List<Job> GetSortedJobs(ColonistManager.Colonist colonist) {
		return jobs
			.Where(job =>
				// Colonist is in the same region as the job
				(job.tile.region == colonist.overTile.region)
				// OR Colonist is NOT in the same region as the job BUT the job is in a tile neighbouring the colonist's region (e.g. for mining jobs)
				|| (job.tile.region != colonist.overTile.region && job.tile.horizontalSurroundingTiles.Find(nTile => nTile != null && nTile.region == colonist.overTile.region) != null))
			.Where(job =>
				// Job is not associated with ANY professions
				GameManager.colonistM.professionPrefabs.Find(p => p.jobs.Contains(job.prefab.jobType)) == null
				// OR Remove jobs that the colonist either CAN'T do, or WON'T do due to the player disabling it for them (i.e. priority == 0)
				|| colonist.professions.Find(p => p.prefab.jobs.Contains(job.prefab.jobType) && p.GetPriority() != 0) != null)
			.OrderBy(job => colonist.professions.Find(p => p.prefab.jobs.Contains(job.prefab.jobType)).GetPriority())
			.ThenBy(job => CalculateJobCost(colonist, job, null))
			.ToList();
	}

	public void UpdateSingleColonistJobs(ColonistManager.Colonist colonist) {
		List<Job> sortedJobs = GetSortedJobs(colonist);

		List<ColonistJob> validJobs = new List<ColonistJob>();

		foreach (Job job in sortedJobs) {

			if (job.resourcesToBuild.Count > 0) {

				KeyValuePair<bool, List<List<ResourceManager.ResourceAmount>>> returnKVP = CalculateColonistResourcesToPickup(colonist, job.resourcesToBuild);
				bool colonistHasAllResources = returnKVP.Key;
				List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
				List<ResourceManager.ResourceAmount> resourcesColonistHas = returnKVP.Value[1];

				if (resourcesToPickup != null) { // If there are resources the colonist doesn't have

					List<ContainerPickup> containerPickups = CalculateColonistPickupContainers(colonist, resourcesToPickup);
					if (containerPickups != null) { // If all resources were found in containers

						validJobs.Add(new ColonistJob(colonist, job, resourcesColonistHas, containerPickups));

					} else {

						continue;

					}

				} else if (colonistHasAllResources) { // If the colonist has all resources

					validJobs.Add(new ColonistJob(colonist, job, resourcesColonistHas, null));

				} else {

					continue;

				}
			} else {

				validJobs.Add(new ColonistJob(colonist, job, null, null));

			}
		}
		if (validJobs.Count > 0) {

			//validJobs = validJobs.OrderByDescending(job => job.job.priority).ThenBy(job => job.cost).ToList();

			if (colonistJobs.ContainsKey(colonist)) {

				colonistJobs[colonist] = validJobs;

			} else {

				colonistJobs.Add(colonist, validJobs);

			}
		}
	}

	private readonly Dictionary<ColonistManager.Colonist, List<ColonistJob>> colonistJobs = new Dictionary<ColonistManager.Colonist, List<ColonistJob>>();

	public void UpdateColonistJobs() {
		colonistJobs.Clear();
		List<ColonistManager.Colonist> availableColonists = GameManager.colonistM.colonists.Where(colonist => colonist.job == null && colonist.overTile.walkable).ToList();
		foreach (ColonistManager.Colonist colonist in availableColonists) {
			UpdateSingleColonistJobs(colonist);
		}
	}

	public int GetColonistJobsCountForColonist(ColonistManager.Colonist colonist) {
		if (colonistJobs.ContainsKey(colonist)) {
			return colonistJobs[colonist].Count;
		}
		return 0;
	}

	public void GiveJobsToColonists() {
		bool gaveJob = false;
		Dictionary<ColonistManager.Colonist, ColonistJob> jobsGiven = new Dictionary<ColonistManager.Colonist, ColonistJob>();

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			if (colonist.job == null && !colonist.playerMoved && colonist.backlog.Count > 0) {
				ColonistJob backlogJob = new ColonistJob(colonist, colonist.backlog[0], colonist.backlog[0].resourcesColonistHas, colonist.backlog[0].containerPickups);
				gaveJob = true;
				jobsGiven.Add(colonist, backlogJob);
				colonist.backlog.Remove(backlogJob.job);
			}
		}

		foreach (KeyValuePair<ColonistManager.Colonist, List<ColonistJob>> colonistKVP in colonistJobs) {
			ColonistManager.Colonist colonist = colonistKVP.Key;
			List<ColonistJob> colonistJobsList = colonistKVP.Value;
			if (colonist.job == null && !colonist.playerMoved && !jobsGiven.ContainsKey(colonist)) {
				for (int i = 0; i < colonistJobsList.Count; i++) {
					ColonistJob colonistJob = colonistJobsList[i];
					bool bestColonistForJob = true;
					foreach (KeyValuePair<ColonistManager.Colonist, List<ColonistJob>> otherColonistKVP in colonistJobs) {
						ColonistManager.Colonist otherColonist = otherColonistKVP.Key;
						if (colonist != otherColonist && otherColonist.job == null) {
							ColonistJob otherColonistJob = otherColonistKVP.Value.Find(job => job.job == colonistJob.job);
							if (otherColonistJob != null && otherColonistJob.cost < colonistJob.cost) {
								bestColonistForJob = false;
								break;
							}
						}
					}
					if (bestColonistForJob) {
						gaveJob = true;
						jobsGiven.Add(colonist, colonistJob);
						jobs.Remove(colonistJob.job);
						foreach (KeyValuePair<ColonistManager.Colonist, List<ColonistJob>> removeKVP in colonistJobs) {
							ColonistJob jobToRemove = removeKVP.Value.Find(cJob => cJob.job == colonistJob.job);
							if (jobToRemove != null) {
								removeKVP.Value.Remove(jobToRemove);
							}
						}
						i -= 1;
						break;
					}
				}
			}
		}
		foreach (KeyValuePair<ColonistManager.Colonist, ColonistJob> jobGiven in jobsGiven) {
			jobGiven.Key.SetJob(jobGiven.Value);
		}
		if (gaveJob) {
			GameManager.uiM.SetJobElements();
			UpdateColonistJobs();
		}
	}

	public bool JobOfTypeExistsAtTile(JobEnum jobType, TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.jobType == jobType && job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.jobType == jobType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.jobType == jobType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}

	public bool JobOfPrefabTypeExistsAtTile(ResourceManager.ObjectEnum prefabType, TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.type == prefabType && job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.type == prefabType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (GameManager.colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.type == prefabType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}
}