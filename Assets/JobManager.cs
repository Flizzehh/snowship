﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JobManager:MonoBehaviour {

	private TileManager tileM;
	private ColonistManager colonistM;
	private CameraManager cameraM;
	private TimeManager timeM;
	private UIManager uiM;
	private PathManager pathM;
	private ResourceManager resourceM;

	void Awake() {
		tileM = GetComponent<TileManager>();
		colonistM = GetComponent<ColonistManager>();
		cameraM = GetComponent<CameraManager>();
		timeM = GetComponent<TimeManager>();
		uiM = GetComponent<UIManager>();
		pathM = GetComponent<PathManager>();
		resourceM = GetComponent<ResourceManager>();

		InitializeSelectionModifierFunctions();
		InitializeFinishJobFunctions();
		InitializeJobDescriptionFunctions();

		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
	}

	public List<Job> jobs = new List<Job>();

	public class Job {

		private ResourceManager resourceM;
		private TileManager tileM;
		private UIManager uiM;

		private void GetScriptReferences() {
			GameObject GM = GameObject.Find("GM");

			resourceM = GM.GetComponent<ResourceManager>();
			tileM = GM.GetComponent<TileManager>();
			uiM = GM.GetComponent<UIManager>();
		}

		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public int rotationIndex;

		public GameObject jobPreview;
		public GameObject priorityIndicator;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> resourcesToBuild = new List<ResourceManager.ResourceAmount>();

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public UIManager.JobElement jobUIElement;

		public ResourceManager.Plant plant;

		public ResourceManager.Resource createResource;
		public ResourceManager.TileObjectInstance activeTileObject;

		public int priority;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab,int rotationIndex) {

			GetScriptReferences();

			this.tile = tile;
			this.prefab = prefab;

			resourcesToBuild.AddRange(prefab.resourcesToBuild);

			if (prefab.jobType == JobTypesEnum.PlantPlant) {
				ResourceManager.PlantGroup plantGroup = resourceM.GetPlantGroupByBiome(tile.biome, true);
				if (prefab.type == ResourceManager.TileObjectPrefabsEnum.PlantAppleTree) {
					plantGroup = resourceM.GetPlantGroupByEnum(ResourceManager.PlantGroupsEnum.WideTree);
				} else if (prefab.type == ResourceManager.TileObjectPrefabsEnum.PlantBlueberryBush) {
					plantGroup = resourceM.GetPlantGroupByEnum(ResourceManager.PlantGroupsEnum.Bush);
				}
				if (plantGroup != null) {
					plant = new ResourceManager.Plant(plantGroup, tile, false, true, tileM.map.smallPlants,false,(resourcesToBuild.Count > 0 ? resourceM.GetResourceByEnum(resourceM.GetSeedToHarvestResource()[resourcesToBuild[0].resource.type]) : null),resourceM);
					tileM.map.smallPlants.Remove(plant);
					plant.obj.SetActive(false);
					resourcesToBuild.Add(new ResourceManager.ResourceAmount(plant.group.seed,1));
				}
			}

			this.rotationIndex = rotationIndex;

			jobPreview = Instantiate(resourceM.tilePrefab,tile.obj.transform,false);
			jobPreview.transform.position += (Vector3)prefab.anchorPositionOffset[rotationIndex];
			jobPreview.name = "JobPreview: " + prefab.name + " at " + jobPreview.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.baseSprite != null) {
				jPSR.sprite = prefab.baseSprite;
			}
			if (!resourceM.GetBitmaskingTileObjects().Contains(prefab.type) && prefab.bitmaskSprites.Count > 0) {
				jPSR.sprite = prefab.bitmaskSprites[rotationIndex];
			}
			jPSR.sortingOrder = 5 + prefab.layer; // Job Preview Sprite
			jPSR.color = new Color(1f,1f,1f,0.25f);

			jobProgress = prefab.timeToBuild;
			colonistBuildTime = prefab.timeToBuild;
		}

		public void SetCreateResourceData(ResourceManager.Resource createResource, ResourceManager.TileObjectInstance manufacturingTileObject) {
			this.createResource = createResource;
			resourcesToBuild.AddRange(createResource.requiredResources);
			if (manufacturingTileObject.mto.fuelResource != null) {
				resourcesToBuild.Add(new ResourceManager.ResourceAmount(manufacturingTileObject.mto.fuelResource, manufacturingTileObject.mto.fuelResourcesRequired));
			}
			activeTileObject = manufacturingTileObject;
		}

		public void SetColonist(ColonistManager.Colonist colonist, ResourceManager resourceM, ColonistManager colonistM, JobManager jobM, PathManager pathM) {
			this.colonist = colonist;
			if (prefab.jobType != JobTypesEnum.PickupResources && containerPickups != null && containerPickups.Count > 0) {
				colonist.storedJob = this;
				colonist.SetJob(new ColonistJob(colonist,new Job(containerPickups[0].container.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.PickupResources),0),null,null,jobM,pathM));
			}
		}

		public void ChangePriority(int amount) {
			priority += amount;
			if (priorityIndicator == null && jobPreview != null) {
				priorityIndicator = Instantiate(resourceM.tilePrefab, jobPreview.transform, false);
				priorityIndicator.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(@"UI/priorityIndicator");
				priorityIndicator.GetComponent<SpriteRenderer>().sortingOrder = jobPreview.GetComponent<SpriteRenderer>().sortingOrder + 1; // Priority Indicator Sprite
				if (priority == 1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = uiM.GetColour(UIManager.Colours.LightYellow);
				} else if (priority == -1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = uiM.GetColour(UIManager.Colours.LightRed);
				}
			}
			if (priority == 0) {
				Destroy(priorityIndicator);
			}
		}

		public void Remove() {
			Destroy(jobPreview);
		}
	}

	public enum JobTypesEnum {
		Build, Remove,
		ChopPlant, PlantPlant, Mine, Dig, PlantFarm, HarvestFarm,
		CreateResource, PickupResources, EmptyInventory, Cancel, IncreasePriority, DecreasePriority, CollectFood, Eat, Sleep
	};

	public Dictionary<JobTypesEnum,System.Func<Job,string>> jobDescriptionFunctions = new Dictionary<JobTypesEnum,System.Func<Job,string>>();

	void InitializeJobDescriptionFunctions() {
		jobDescriptionFunctions.Add(JobTypesEnum.Build,delegate (Job job) {
			return "Building a " + job.prefab.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Remove,delegate (Job job) {
			return "Removing a " + job.tile.GetObjectInstanceAtLayer(job.prefab.layer).prefab.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.ChopPlant,delegate (Job job) {
			return "Chopping down a " + job.tile.plant.group.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.PlantPlant,delegate (Job job) {
			return "Planting a " + job.plant.group.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Mine,delegate (Job job) {
			return "Mining " + job.tile.tileType.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Dig,delegate (Job job) {
			if (tileM.GetResourceTileTypes().Contains(job.tile.tileType.type)) {
				if (tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) {
					if (tileM.GetWaterToGroundResourceMap().ContainsKey(job.tile.tileType.type)) {
						return "Digging " + tileM.GetTileTypeByEnum(tileM.GetWaterToGroundResourceMap()[job.tile.tileType.type]).name + ".";
					} else {
						return "Digging something.";
					}
				} else {
					return "Digging " + tileM.GetTileTypeByEnum(job.tile.tileType.type).name + ".";
				}
			} else {
				return "Digging " + job.tile.biome.groundResource.name + ".";
			}
		});
		jobDescriptionFunctions.Add(JobTypesEnum.PlantFarm,delegate (Job job) {
			return "Planting a " + job.prefab.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.HarvestFarm,delegate (Job job) {
			return "Harvesting a farm of " + job.tile.farm.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.CreateResource,delegate (Job job) {
			return "Creating " + job.createResource.name + ".";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.PickupResources,delegate (Job job) {
			return "Picking up some resources.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.EmptyInventory,delegate (Job job) {
			return "Emptying their inventory.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.CollectFood,delegate (Job job) {
			return "Finding some food to eat.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Eat,delegate (Job job) {
			return "Eating.";
		});
		jobDescriptionFunctions.Add(JobTypesEnum.Sleep,delegate (Job job) {
			return "Sleeping.";
		});
	}

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

	public Dictionary<JobTypesEnum,System.Action<ColonistManager.Colonist,Job>> finishJobFunctions = new Dictionary<JobTypesEnum,System.Action<ColonistManager.Colonist,Job>>();

	void InitializeFinishJobFunctions() {
		finishJobFunctions.Add(JobTypesEnum.Build,delegate (ColonistManager.Colonist colonist, Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,-resourceAmount.amount);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.Remove,delegate (ColonistManager.Colonist colonist,Job job) {
			bool previousWalkability = job.tile.walkable;
			ResourceManager.TileObjectInstance instance = job.tile.GetObjectInstanceAtLayer(job.prefab.layer);
			foreach (ResourceManager.ResourceAmount resourceAmount in instance.prefab.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,Mathf.RoundToInt(resourceAmount.amount / 2f));
			}
			if (instance.prefab.jobType == JobTypesEnum.PlantFarm) {
				if (instance.tile.farm.growProgressSpriteIndex == 0) {
					job.colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(instance.tile.farm.seedType),1);
				}
				instance.tile.SetFarm(null);
			}
			if (instance.prefab.tileObjectPrefabSubGroup.type == ResourceManager.TileObjectPrefabSubGroupsEnum.Containers) {
				ResourceManager.Container targetContainer = resourceM.containers.Find(container => container.parentObject == instance);
				if (targetContainer != null) {
					foreach (ResourceManager.ResourceAmount resourceAmount in targetContainer.inventory.resources) {
						targetContainer.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount);
						colonist.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount);
					}
					List<ResourceManager.ReservedResources> reservedResourcesToRemove = new List<ResourceManager.ReservedResources>();
					foreach (ResourceManager.ReservedResources reservedResources in targetContainer.inventory.reservedResources) {
						foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
							colonist.inventory.ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount);
						}
						reservedResourcesToRemove.Add(reservedResources);
						reservedResources.colonist.ReturnJob();
					}
					foreach (ResourceManager.ReservedResources reservedResourceToRemove in reservedResourcesToRemove) {
						targetContainer.inventory.reservedResources.Remove(reservedResourceToRemove);
					}
					uiM.SetSelectedColonistInformation();
					uiM.SetSelectedContainerInfo();
				} else {
					//print("Target container is null but it shouldn't be...");
				}
			}
			colonist.resourceM.RemoveTileObjectInstance(instance);
			job.tile.RemoveTileObjectAtLayer(instance.prefab.layer);
			resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
			if (job.tile.walkable && !previousWalkability) {
				tileM.map.RemoveTileBrightnessEffect(job.tile);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.PlantFarm,delegate (ColonistManager.Colonist colonist,Job job) {
			finishJobFunctions[JobTypesEnum.Build](colonist,job);
		});
		finishJobFunctions.Add(JobTypesEnum.HarvestFarm,delegate (ColonistManager.Colonist colonist,Job job) {
			if (job.tile.farm != null) {
				colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(job.tile.farm.seedType), Random.Range(1, 3));
				colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(resourceM.GetFarmSeedReturnResource()[job.tile.farm.seedType]), Random.Range(1, 6));

				CreateJob(new Job(job.tile, resourceM.GetTileObjectPrefabByEnum(resourceM.GetFarmSeedsTileObject()[job.tile.farm.seedType]), 0));

				colonist.resourceM.RemoveTileObjectInstance(job.tile.farm);
				job.tile.RemoveTileObjectAtLayer(job.tile.farm.prefab.layer);
			}
			job.tile.SetFarm(null);
			resourceM.Bitmask(new List<TileManager.Tile>() { job.tile }.Concat(job.tile.surroundingTiles).ToList());
		});
		finishJobFunctions.Add(JobTypesEnum.ChopPlant,delegate (ColonistManager.Colonist colonist,Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.tile.plant.GetResources()) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,resourceAmount.amount);
			}
			job.tile.SetPlant(true,null);
		});
		finishJobFunctions.Add(JobTypesEnum.PlantPlant,delegate (ColonistManager.Colonist colonist,Job job) {
			job.plant.obj.SetActive(true);
			job.tile.SetPlant(false,job.plant);
			tileM.map.smallPlants.Add(job.plant);
			colonist.inventory.ChangeResourceAmount(job.plant.group.seed,-1);
			tileM.map.SetTileBrightness(timeM.tileBrightnessTime);
		});
		finishJobFunctions.Add(JobTypesEnum.Mine,delegate (ColonistManager.Colonist colonist,Job job) {
			colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),job.tile.tileType.type.ToString())),Random.Range(4,7));
			job.tile.SetTileType(tileM.GetTileTypeByEnum(TileManager.TileTypes.Dirt),true,true,true,false);
			tileM.map.RemoveTileBrightnessEffect(job.tile);
			foreach (ResourceManager.LightSource lightSource in resourceM.lightSources) {
				if (Vector2.Distance(job.tile.obj.transform.position, lightSource.parentTile.obj.transform.position) <= lightSource.parentObject.prefab.maxLightDistance) {
					lightSource.RemoveTileBrightnesses();
					lightSource.SetTileBrightnesses();
				}
			}
		});
		finishJobFunctions.Add(JobTypesEnum.Dig, delegate (ColonistManager.Colonist colonist, Job job) {
			job.tile.dugPreviously = true;
			if (tileM.GetResourceTileTypes().Contains(job.tile.tileType.type)) {
				if (tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) {
					if (tileM.GetWaterToGroundResourceMap().ContainsKey(job.tile.tileType.type)) {
						colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),tileM.GetTileTypeByEnum(tileM.GetWaterToGroundResourceMap()[job.tile.tileType.type]).type.ToString())),Random.Range(4,7));
					}
				} else {
					colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum((ResourceManager.ResourcesEnum)System.Enum.Parse(typeof(ResourceManager.ResourcesEnum),job.tile.tileType.type.ToString())),Random.Range(4,7));
				}
			} else {
				colonist.inventory.ChangeResourceAmount(job.tile.biome.groundResource, Random.Range(4, 7));
			}
			bool setToWater = false;
			if ((!tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) || (tileM.GetResourceTileTypes().Contains(job.tile.tileType.type))) {
				foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
					if (nTile != null && tileM.GetWaterEquivalentTileTypes().Contains(nTile.tileType.type)) {
						job.tile.SetTileType(nTile.tileType, true, true, true, true);
						setToWater = true;
						break;
					}
				}
			} else if (tileM.GetWaterEquivalentTileTypes().Contains(job.tile.tileType.type)) {
				setToWater = true;
			}
			if (setToWater) {
				foreach (TileManager.Tile nTile in job.tile.horizontalSurroundingTiles) {
					if (nTile != null && tileM.GetHoleTileTypes().Contains(nTile.tileType.type)) {
						List<TileManager.Tile> frontier = new List<TileManager.Tile>() { nTile };
						List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>() { };
						TileManager.Tile currentTile = nTile;
						while (frontier.Count > 0) {
							currentTile = frontier[0];
							frontier.RemoveAt(0);
							checkedTiles.Add(currentTile);
							currentTile.SetTileType(job.tile.tileType, true, true, true, true);
							foreach (TileManager.Tile nTile2 in currentTile.horizontalSurroundingTiles) {
								if (nTile2 != null && tileM.GetHoleTileTypes().Contains(nTile2.tileType.type) && !checkedTiles.Contains(nTile2)) {
									frontier.Add(nTile2);
								}
							}
						}
					}
				}
			} else {
				job.tile.SetTileType(job.tile.biome.holeType, true, true, true, false);
			}
		});
		finishJobFunctions.Add(JobTypesEnum.PickupResources,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			//print(containerOnTile + " " + colonist.storedJob.prefab.type.ToString() + " " + colonist.storedJob.containerPickups + " " + colonist.storedJob.containerPickups.Count);
			if (containerOnTile != null && colonist.storedJob != null) {
				ContainerPickup containerPickup = colonist.storedJob.containerPickups.Find(pickup => pickup.container == containerOnTile);
				//print(containerPickup);
				if (containerPickup != null) {
					foreach (ResourceManager.ReservedResources rr in containerPickup.container.inventory.TakeReservedResources(colonist)) {
						//print(name + " " + rr.colonist.name + " " + rr.resources.Count);
						foreach (ResourceManager.ResourceAmount ra in rr.resources) {
							colonist.inventory.ChangeResourceAmount(ra.resource,ra.amount);
							//print(name + " " + ra.resource.name + " " + ra.amount);
						}
					}
					colonist.storedJob.containerPickups.RemoveAt(0);
				}
			}
			if (colonist.storedJob != null) {
				if (colonist.storedJob.containerPickups.Count <= 0) {
					//print("Setting stored job on " + name);
					colonist.SetJob(new ColonistJob(colonist, colonist.storedJob, colonist.storedJob.colonistResources, null, colonist.jobM, pathM));
					colonist.storedJob = null;
				} else {
					//print("Setting next pickup resources job on " + name + " -- " + colonist.storedJob.containerPickups.Count + " more left");
					colonist.SetJob(new ColonistJob(colonist, new Job(colonist.storedJob.containerPickups[0].container.parentObject.tile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.PickupResources),0), colonist.storedJob.colonistResources, colonist.storedJob.containerPickups, colonist.jobM, pathM),false);
				}
			}
		});
		finishJobFunctions.Add(JobTypesEnum.CreateResource,delegate (ColonistManager.Colonist colonist,Job job) {
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesToBuild) {
				colonist.inventory.ChangeResourceAmount(resourceAmount.resource,-resourceAmount.amount);
			}
			colonist.inventory.ChangeResourceAmount(job.createResource,job.createResource.amountCreated);
			job.activeTileObject.mto.jobBacklog.Remove(job);
		});
		finishJobFunctions.Add(JobTypesEnum.EmptyInventory,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			if (containerOnTile != null) {
				List<ResourceManager.ResourceAmount> removeResourceAmounts = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount inventoryResourceAmount in colonist.inventory.resources) {
					if (inventoryResourceAmount.amount <= containerOnTile.maxAmount - containerOnTile.inventory.CountResources()) {
						containerOnTile.inventory.ChangeResourceAmount(inventoryResourceAmount.resource,inventoryResourceAmount.amount);
						removeResourceAmounts.Add(new ResourceManager.ResourceAmount(inventoryResourceAmount.resource,inventoryResourceAmount.amount));
					} else if (containerOnTile.inventory.CountResources() < containerOnTile.maxAmount) {
						int amount = containerOnTile.maxAmount - containerOnTile.inventory.CountResources();
						containerOnTile.inventory.ChangeResourceAmount(inventoryResourceAmount.resource,amount);
						removeResourceAmounts.Add(new ResourceManager.ResourceAmount(inventoryResourceAmount.resource,amount));
					} else {
						//print("No space left in container");
					}
				}
				foreach (ResourceManager.ResourceAmount removeResourceAmount in removeResourceAmounts) {
					colonist.inventory.ChangeResourceAmount(removeResourceAmount.resource,-removeResourceAmount.amount);
				}
			}
		});
		finishJobFunctions.Add(JobTypesEnum.CollectFood,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.Container containerOnTile = resourceM.containers.Find(container => container.parentObject.tile == colonist.overTile);
			if (containerOnTile != null) {
				foreach (ResourceManager.ReservedResources rr in containerOnTile.inventory.TakeReservedResources(colonist)) {
					foreach (ResourceManager.ResourceAmount ra in rr.resources) {
						colonist.inventory.ChangeResourceAmount(ra.resource,ra.amount);
					}
				}
			}
			colonist.SetJob(new ColonistJob(colonist,new Job(colonist.overTile,resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Eat),0),null,null,this,pathM));
		});
		finishJobFunctions.Add(JobTypesEnum.Eat,delegate (ColonistManager.Colonist colonist,Job job) {
			List<ResourceManager.ResourceAmount> resourcesToEat = colonist.inventory.resources.Where(r => r.resource.resourceGroup.type == ResourceManager.ResourceGroupsEnum.Foods).OrderBy(r => r.resource.nutrition).ToList();
			ColonistManager.NeedInstance foodNeed = colonist.needs.Find(need => need.prefab.type == ColonistManager.NeedsEnum.Food);
			float startingFoodNeedValue = foodNeed.value;
			foreach (ResourceManager.ResourceAmount ra in resourcesToEat) {
				bool stopEating = false;
				for (int i = 0; i < ra.amount; i++) {
					if (foodNeed.value <= 0) {
						stopEating = true;
						break;
					}
					foodNeed.value -= ra.resource.nutrition;
					colonist.inventory.ChangeResourceAmount(ra.resource, -1);
					if (ra.resource.type == ResourceManager.ResourcesEnum.Apple || ra.resource.type == ResourceManager.ResourcesEnum.BakedApple) {
						colonist.inventory.ChangeResourceAmount(resourceM.GetResourceByEnum(ResourceManager.ResourcesEnum.AppleSeed), Random.Range(1, 5));
					}
				}
				if (stopEating) {
					break;
				}
			}
			float amountEaten = startingFoodNeedValue - foodNeed.value;
			if (amountEaten >= 15 && foodNeed.value <= -10) {
				colonist.AddHappinessModifier(ColonistManager.HappinessModifiersEnum.Stuffed);
			} else if (amountEaten >= 15) {
				colonist.AddHappinessModifier(ColonistManager.HappinessModifiersEnum.Full);
			}
			if (foodNeed.value < 0) {
				foodNeed.value = 0;
			}
		});
		finishJobFunctions.Add(JobTypesEnum.Sleep,delegate (ColonistManager.Colonist colonist,Job job) {
			ResourceManager.SleepSpot targetSleepSpot = resourceM.sleepSpots.Find(sleepSpot => sleepSpot.parentObject.tile == job.tile);
			if (targetSleepSpot != null) {
				targetSleepSpot.StopSleeping();
				if (targetSleepSpot.parentObject.prefab.restComfortAmount >= 10) {
					colonist.AddHappinessModifier(ColonistManager.HappinessModifiersEnum.Rested);
				}
			}
			foreach (ResourceManager.SleepSpot sleepSpot in resourceM.sleepSpots) {
				if (sleepSpot.occupyingColonist == colonist) {
					sleepSpot.StopSleeping();
				}
			}
		});
	}

	private ResourceManager.TileObjectPrefab selectedPrefab;

	public void SetSelectedPrefab(ResourceManager.TileObjectPrefab newSelectedPrefab) {
		if (newSelectedPrefab != selectedPrefab) {
			if (newSelectedPrefab != null) {
				selectedPrefab = newSelectedPrefab;
				rotationIndex = 0;
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
				}
			} else {
				selectedPrefab = null;
			}
		}
	}

	public ResourceManager.TileObjectPrefab GetSelectedPrefab() {
		return selectedPrefab;
	}

	private GameObject selectedPrefabPreview;
	public void SelectedPrefabPreview() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		TileManager.Tile tile = tileM.map.GetTileFromPosition(mousePosition);
		selectedPrefabPreview.transform.position = tile.obj.transform.position + (Vector3)selectedPrefab.anchorPositionOffset[rotationIndex];
	}

	public void UpdateSelectedPrefabInfo() {
		if (selectedPrefab != null) {
			if (enableSelectionPreview) {
				if (!selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(true);
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
					if (selectedPrefab.canRotate) {
						selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.bitmaskSprites[rotationIndex];
					}
					uiM.SelectionSizeCanvasSetActive(false);
				}
				SelectedPrefabPreview();
				if (Input.GetKeyDown(KeyCode.R)) {
					if (selectedPrefab.canRotate) {
						rotationIndex += 1;
						if (rotationIndex >= selectedPrefab.bitmaskSprites.Count) {
							rotationIndex = 0;
						}
						selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.bitmaskSprites[rotationIndex];
					}
				}
			} else {
				if (selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(false);
				}
				uiM.SelectionSizeCanvasSetActive(true);
			}
		} else {
			selectedPrefabPreview.SetActive(false);
			uiM.SelectionSizeCanvasSetActive(false);
		}
	}

	private bool changedJobList = false;
	private int rotationIndex = 0;
	void Update() {
		if (changedJobList) {
			UpdateColonistJobs();
			uiM.SetJobElements();
			changedJobList = false;
		}
		GetJobSelectionArea();
		UpdateSelectedPrefabInfo();
	}

	public enum SelectionModifiersEnum { Outline, Walkable, OmitWalkable, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms, ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, OmitNonLivingTreeOrBushBiomes, OmitObjectInstancesOnAdditionalTiles
	};

	Dictionary<SelectionModifiersEnum, System.Func<TileManager.Tile, TileManager.Tile, ResourceManager.TileObjectPrefab, bool>> selectionModifierFunctions = new Dictionary<SelectionModifiersEnum, System.Func<TileManager.Tile, TileManager.Tile, ResourceManager.TileObjectPrefab, bool>>();
	void InitializeSelectionModifierFunctions() {
		selectionModifierFunctions.Add(SelectionModifiersEnum.Walkable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.walkable;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitWalkable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !posTile.walkable;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Buildable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.tileType.buildable;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitBuildable, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !posTile.tileType.buildable;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.StoneTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return tileM.GetStoneEquivalentTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitStoneTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !tileM.GetStoneEquivalentTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.AllWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return tileM.GetWaterEquivalentTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitAllWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !tileM.GetWaterEquivalentTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.LiquidWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return tileM.GetLiquidWaterEquivalentTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitLiquidWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !tileM.GetLiquidWaterEquivalentTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNonStoneAndWaterTypes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return (!tileM.GetWaterEquivalentTileTypes().Contains(posTile.tileType.type) && !tileM.GetStoneEquivalentTileTypes().Contains(posTile.tileType.type));
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Plants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.plant != null;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitPlants, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.plant == null;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerJobs, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			foreach (Job job in jobs) {
				if (job.prefab.layer == prefab.layer) {
					if (job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in job.prefab.multiTilePositions[job.rotationIndex]) {
						if ((tileM.map.GetTileFromPosition(job.tile.obj.transform.position + (Vector3)multiTilePosition)) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.job != null && colonist.job.prefab.layer == prefab.layer) {
					if (colonist.job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.job.prefab.multiTilePositions[colonist.job.rotationIndex]) {
						if ((tileM.map.GetTileFromPosition(colonist.job.tile.obj.transform.position + (Vector3)multiTilePosition)) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.storedJob != null && colonist.storedJob.prefab.layer == prefab.layer) {
					if (colonist.storedJob.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.storedJob.prefab.multiTilePositions[colonist.storedJob.rotationIndex]) {
						if ((tileM.map.GetTileFromPosition(colonist.storedJob.tile.obj.transform.position + (Vector3)multiTilePosition)) == posTile) {
							return false;
						}
					}
				}
			}
			return true;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerObjectInstances, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return (!posTile.objectInstances.ContainsKey(prefab.layer) || posTile.objectInstances[prefab.layer] == null);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Farms, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.farm != null;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitFarms, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.farm == null;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.ObjectsAtSameLayer, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.GetObjectInstanceAtLayer(prefab.layer) != null;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Objects, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.GetAllObjectInstances().Count > 0;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitObjects, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.GetAllObjectInstances().Count <= 0;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNonCoastWater, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			if (tileM.GetWaterEquivalentTileTypes().Contains(posTile.tileType.type)) {
				if (!(posTile.surroundingTiles.Find(t => t != null && !tileM.GetWaterEquivalentTileTypes().Contains(t.tileType.type)) != null)) {
					return false;
				}
			}
			return true;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitHoles, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !tileM.GetHoleTileTypes().Contains(posTile.tileType.type);
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitPreviousDig, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return !posTile.dugPreviously;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNonLivingTreeOrBushBiomes, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			return posTile.biome.vegetationChances.Keys.Where(groupEnum => resourceM.GetLivingTreesAndBushes().Contains(groupEnum)).ToList().Count > 0;
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitObjectInstancesOnAdditionalTiles, delegate (TileManager.Tile tile, TileManager.Tile posTile, ResourceManager.TileObjectPrefab prefab) {
			ResourceManager.TileObjectInstance tileObjectInstance = posTile.GetObjectInstanceAtLayer(prefab.layer);
			if (tileObjectInstance != null && tileObjectInstance.tile != posTile) {
				return false;
			}
			return true;
		});
	}

	private List<GameObject> selectionIndicators = new List<GameObject>();

	public TileManager.Tile firstTile;
	private bool stopSelection;

	public void StopSelection() {
		stopSelection = true;
	}

	private bool enableSelectionPreview = true;

	public void GetJobSelectionArea() {

		enableSelectionPreview = true;

		foreach (GameObject selectionIndicator in selectionIndicators) {
			Destroy(selectionIndicator);
		}
		selectionIndicators.Clear();

		if (selectedPrefab != null) {
			Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				firstTile = tileM.map.GetTileFromPosition(mousePosition);
			}
			if (firstTile != null) {
				if (stopSelection) {
					stopSelection = false;
					firstTile = null;
					return;
				}
				TileManager.Tile secondTile = tileM.map.GetTileFromPosition(mousePosition);
				if (secondTile != null) {

					enableSelectionPreview = false;

					float smallerY = Mathf.Min(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float largerY = Mathf.Max(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float smallerX = Mathf.Min(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);
					float largerX = Mathf.Max(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);

					List<TileManager.Tile> selectionArea = new List<TileManager.Tile>();

					float maxY = largerY + 1;
					float maxX = largerX + 1;

					bool addedToSelectionArea = false;
					for (float y = smallerY; y < maxY; y += (addedToSelectionArea ? selectedPrefab.dimensions[rotationIndex].y : 1)) {
						//addedToSelectionArea = false;
						for (float x = smallerX; x < maxX; x += (addedToSelectionArea ? selectedPrefab.dimensions[rotationIndex].x : 1)) {
							addedToSelectionArea = true; // default = false // Try swapping x and y values when the object is rotated vertically (i.e. rotationIndex == 1 || 3).
							TileManager.Tile tile = tileM.map.GetTileFromPosition(new Vector2(x, y));
							bool addTile = true;
							bool addOutlineTile = true;
							if (selectedPrefab.selectionModifiers.Contains(SelectionModifiersEnum.Outline)) {
								addOutlineTile = (x == smallerX || y == smallerY || x == largerX || y == largerY);
							}
							foreach (SelectionModifiersEnum selectionModifier in selectedPrefab.selectionModifiers) {
								if (selectionModifier != SelectionModifiersEnum.Outline) {
									foreach (Vector2 multiTilePosition in selectedPrefab.multiTilePositions[rotationIndex]) {
										Vector2 actualMultiTilePosition = tile.obj.transform.position + (Vector3)multiTilePosition;
										if (actualMultiTilePosition.x >= 0 && actualMultiTilePosition.x < tileM.map.mapData.mapSize && actualMultiTilePosition.y >= 0 && actualMultiTilePosition.y < tileM.map.mapData.mapSize) {
											TileManager.Tile posTile = tileM.map.GetTileFromPosition(actualMultiTilePosition);
											addTile = selectionModifierFunctions[selectionModifier](tile, posTile, selectedPrefab);
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

								GameObject selectionIndicator = Instantiate(resourceM.tilePrefab, tile.obj.transform, false);
								selectionIndicator.name = "Selection Indicator";
								SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
								sISR.sprite = Resources.Load<Sprite>(@"UI/selectionIndicator");
								sISR.sortingOrder = 20; // Selection Indicator Sprite
								selectionIndicators.Add(selectionIndicator);
							}
						}
					}

					uiM.UpdateSelectionSizePanel(smallerX - maxX,smallerY - maxY,selectionArea.Count,selectedPrefab);

					if (Input.GetMouseButtonUp(0)) {
						if (selectedPrefab.jobType == JobTypesEnum.Cancel) {
							CancelJobsInSelectionArea(selectionArea);
						} else if (selectedPrefab.jobType == JobTypesEnum.IncreasePriority) {
							ChangeJobPriorityInSelectionArea(selectionArea, 1);
						} else if (selectedPrefab.jobType == JobTypesEnum.DecreasePriority) {
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
			if (job.prefab.jobType == JobTypesEnum.CreateResource) {
				job.activeTileObject.mto.jobBacklog.Remove(job);
			}
			job.jobUIElement.Remove(uiM);
			job.Remove();
			jobs.Remove(job);
		}
		removeJobs.Clear();

		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {

			bool removeJob = false;
			bool removeStoredJob = false;

			if (colonist.job != null && selectionArea.Contains(colonist.job.tile)) {
				removeJob = true;

				if (colonist.storedJob != null && !selectionArea.Contains(colonist.storedJob.tile)) {
					removeStoredJob = true;
				}
			}

			if (removeStoredJob || (colonist.storedJob != null && selectionArea.Contains(colonist.storedJob.tile))) {
				if (colonist.storedJob.prefab.jobType == JobTypesEnum.CreateResource) {
					colonist.storedJob.activeTileObject.mto.jobBacklog.Remove(colonist.storedJob);
				}
				if (colonist.storedJob.jobUIElement != null) {
					colonist.storedJob.jobUIElement.Remove(uiM);
				} else {
					Debug.LogWarning("storedJob on Colonist " + colonist.name + " jobUIElement is null for job " + colonist.storedJob.prefab.type);
				}
				colonist.storedJob.Remove();
				colonist.storedJob = null;

				if (colonist.job != null) {
					removeJob = true;
				}
			}

			if (removeJob) {
				if (colonist.job.prefab.jobType == JobTypesEnum.CreateResource) {
					colonist.job.activeTileObject.mto.jobBacklog.Remove(colonist.job);
				}
				if (colonist.job.jobUIElement != null) {
					colonist.job.jobUIElement.Remove(uiM);
				}
				colonist.job.Remove();
				colonist.job = null;
				colonist.path.Clear();
				colonist.MoveToClosestWalkableTile(false);
			}
		}

		UpdateColonistJobs();
	}

	public void ChangeJobPriorityInSelectionArea(List<TileManager.Tile> selectionArea, int amount) {
		foreach (Job job in jobs) {
			if (selectionArea.Contains(job.tile)) {
				job.ChangePriority(amount);
			}
		}
		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
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
		uiM.SetJobElements();
	}

	Dictionary<int,ResourceManager.TileObjectPrefabsEnum> RemoveLayerMap = new Dictionary<int,ResourceManager.TileObjectPrefabsEnum>() {
		{1,ResourceManager.TileObjectPrefabsEnum.RemoveLayer1 },{2,ResourceManager.TileObjectPrefabsEnum.RemoveLayer2 }
	};

	public void CreateJobsInSelectionArea(ResourceManager.TileObjectPrefab prefab, List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			if (selectedPrefab.type == ResourceManager.TileObjectPrefabsEnum.RemoveAll) {
				foreach (ResourceManager.TileObjectInstance instance in tile.GetAllObjectInstances()) {
					if (RemoveLayerMap.ContainsKey(instance.prefab.layer) && !JobOfPrefabTypeExistsAtTile(RemoveLayerMap[instance.prefab.layer], instance.tile)) {
						ResourceManager.TileObjectPrefab selectedRemovePrefab = resourceM.GetTileObjectPrefabByEnum(RemoveLayerMap[instance.prefab.layer]);
						bool createJobAtTile = true;
						foreach (SelectionModifiersEnum selectionModifier in selectedRemovePrefab.selectionModifiers) {
							if (selectionModifier != SelectionModifiersEnum.Outline) {
								createJobAtTile = selectionModifierFunctions[selectionModifier](instance.tile, instance.tile, selectedRemovePrefab);
								if (!createJobAtTile) {
									break;
								}
							}
						}
						if (createJobAtTile) {
							CreateJob(new Job(instance.tile, selectedRemovePrefab, rotationIndex));
						}
					}
				}
			} else {
				CreateJob(new Job(tile, prefab, rotationIndex));
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

		public ContainerPickup(ResourceManager.Container container,List<ResourceManager.ResourceAmount> resourcesToPickup) {
			this.container = container;
			this.resourcesToPickup = resourcesToPickup;
		}
	}

	public List<ContainerPickup> CalculateColonistPickupContainers(ColonistManager.Colonist colonist,Job job,List<ResourceManager.ResourceAmount> resourcesToPickup) {
		List<ContainerPickup> containersToPickupFrom = new List<ContainerPickup>();
		List<ResourceManager.Container> sortedContainersByDistance = resourceM.containers.Where(container => container.parentObject.tile.region == colonist.overTile.region).OrderBy(container => pathM.RegionBlockDistance(colonist.overTile.regionBlock,container.parentObject.tile.regionBlock,true,true,false)).ToList();
		if (sortedContainersByDistance.Count > 0) {
			foreach (ResourceManager.Container container in sortedContainersByDistance) {
				List<ResourceManager.ResourceAmount> resourcesToPickupAtContainer = new List<ResourceManager.ResourceAmount>();
				foreach (ResourceManager.ResourceAmount resourceAmount in container.inventory.resources.Where(ra => resourcesToPickup.Find(pickupResource => pickupResource.resource == ra.resource) != null)) {
					ResourceManager.ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.resource == resourceAmount.resource);
					if (resourceAmount.amount >= pickupResource.amount) {
						//print("Found all of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource,pickupResource.amount));
						resourcesToPickup.Remove(pickupResource);
					} else if (resourceAmount.amount > 0 && resourceAmount.amount < pickupResource.amount) {
						//print("Found some of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
						resourcesToPickupAtContainer.Add(new ResourceManager.ResourceAmount(pickupResource.resource,resourceAmount.amount));
						pickupResource.amount -= resourceAmount.amount;
						if (pickupResource.amount <= 0) {
							resourcesToPickup.Remove(pickupResource);
						}
					} else {
						//print("Found none of resource" + pickupResource.resource.name + "(" + pickupResource.amount + ") at " + container.parentObject.tile.obj.transform.position);
					}
				}
				if (resourcesToPickupAtContainer.Count > 0) {
					containersToPickupFrom.Add(new ContainerPickup(container,resourcesToPickupAtContainer));
				}
			}
			if (containersToPickupFrom.Count > 0) {
				if (resourcesToPickup.Count <= 0) {
					return containersToPickupFrom;
				} else {
					//print("Didn't find all resources in containers. Missed " + resourcesToPickup.Count + " resources");
					return null;
				}
			} else {
				//print("Didn't find any containers which contain the resources the colonist needs");
				return null;
			}
		} else {
			//print("Didn't find any valid containers");
			return null;
		}
	}

	public KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> CalculateColonistResourcesToPickup(ColonistManager.Colonist colonist, List<ResourceManager.ResourceAmount> resourcesToFind) {
		bool colonistHasAllResources = false;
		List<ResourceManager.ResourceAmount> resourcesColonistHas = new List<ResourceManager.ResourceAmount>();
		List<ResourceManager.ResourceAmount> resourcesToPickup = new List<ResourceManager.ResourceAmount>();
		foreach (ResourceManager.ResourceAmount resourceAmount in resourcesToFind) {
			ResourceManager.ResourceAmount colonistResourceAmount = colonist.inventory.resources.Find(resource => resource.resource == resourceAmount.resource);
			if (colonistResourceAmount != null) {
				if (colonistResourceAmount.amount >= resourceAmount.amount) {
					colonistHasAllResources = true;
					//print("Found all of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
				} else if (colonistResourceAmount.amount > 0 && colonistResourceAmount.amount < resourceAmount.amount) {
					colonistHasAllResources = false;
					//print("Found some of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesColonistHas.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,colonistResourceAmount.amount));
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount - colonistResourceAmount.amount));
				} else {
					colonistHasAllResources = false;
					//print("Found none of resource " + resourceAmount.resource.name + "(" + resourceAmount.amount + ") in " + colonist.name);
					resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
				}
			} else {
				colonistHasAllResources = false;
				resourcesToPickup.Add(new ResourceManager.ResourceAmount(resourceAmount.resource,resourceAmount.amount));
			}
		}
		/*
		if (resourcesToPickup.Count > 0 && resourcesColonistHas.Count > 0) {
			print("Found " + resourcesToPickup.Count + " that " + colonist.name + " needs to pickup");
		} else if (resourcesToPickup.Count <= 0 && resourcesColonistHas.Count > 0) {
			print("Found all resources in " + colonist.name);
		} else if (resourcesToPickup.Count > 0 && resourcesColonistHas.Count <= 0) {
			print("Found no resources in " + colonist.name);
		}
		*/
		return new KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>>(colonistHasAllResources, new List<List<ResourceManager.ResourceAmount>>() { (resourcesToPickup.Count > 0 ? resourcesToPickup : null),(resourcesColonistHas.Count > 0 ? resourcesColonistHas : null) });
	}

	public float CalculateJobCost(ColonistManager.Colonist colonist,Job job, List<ContainerPickup> containerPickups) {
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
			for (int i = 0;i < containerPickups.Count;i++) {
				if (i == 0) {
					cost += pathM.RegionBlockDistance(colonist.overTile.regionBlock,containerPickups[i].container.parentObject.tile.regionBlock,true,true,true);
				} else {
					cost += pathM.RegionBlockDistance(containerPickups[i - 1].container.parentObject.tile.regionBlock,containerPickups[i].container.parentObject.tile.regionBlock,true,true,true);
				}
			}
			cost += pathM.RegionBlockDistance(job.tile.regionBlock,containerPickups[containerPickups.Count-1].container.parentObject.tile.regionBlock,true,true,true);
		} else {
			cost += pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true,true);
		}
		ColonistManager.SkillInstance skill = colonist.GetSkillFromJobType(job.prefab.jobType);
		if (skill != null) {
			ColonistManager.Profession jobTypeProfession = colonistM.professions.Find(profession => profession.primarySkill == skill.prefab);
			if (jobTypeProfession != null && jobTypeProfession == colonist.profession) {
				cost -= tileM.map.mapData.mapSize + (skill.level * 5f);
			} else {
				cost -= skill.level * 5f;
			}
		}
		if (colonist.profession.type == ColonistManager.ProfessionTypeEnum.Builder && resourceM.GetContainerTileObjectTypes().Contains(job.prefab.type)) {
			cost -= tileM.map.mapData.mapSize;
		}
		return cost;
	}

	public class ColonistJob {
		public ColonistManager.Colonist colonist;
		public Job job;

		public List<ResourceManager.ResourceAmount> colonistResources;
		public List<ContainerPickup> containerPickups;

		public float cost;

		public ColonistJob(ColonistManager.Colonist colonist,Job job,List<ResourceManager.ResourceAmount> colonistResources,List<ContainerPickup> containerPickups, JobManager jobM, PathManager pathM) {
			this.colonist = colonist;
			this.job = job;
			this.colonistResources = colonistResources;
			this.containerPickups = containerPickups;

			CalculateCost(jobM);
		}

		public void CalculateCost(JobManager jobM) {
			cost = jobM.CalculateJobCost(colonist,job,containerPickups);
		}

		public void RecalculatePickupResources(JobManager jobM) {
			KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> returnKVP = jobM.CalculateColonistResourcesToPickup(colonist,job.resourcesToBuild);
			List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
			colonistResources = returnKVP.Value[1];
			if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
				containerPickups = jobM.CalculateColonistPickupContainers(colonist,job,resourcesToPickup);
			} else {
				containerPickups = null;
			}
		}
	}

	public void UpdateColonistJobCosts(ColonistManager.Colonist colonist) {
		if (colonistJobs.ContainsKey(colonist)) {
			foreach (ColonistJob colonistJob in colonistJobs[colonist]) {
				colonistJob.CalculateCost(this);
			}
		}
	}

	public void UpdateAllColonistJobCosts() {
		foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
			UpdateColonistJobCosts(colonist);
		}
	}

	public void UpdateSingleColonistJobs(ColonistManager.Colonist colonist) {
		List<Job> sortedJobs = jobs.Where(job => (job.tile.region == colonist.overTile.region) || (job.tile.region != colonist.overTile.region && job.tile.horizontalSurroundingTiles.Find(nTile => nTile != null && nTile.region == colonist.overTile.region) != null)).OrderBy(job => CalculateJobCost(colonist,job,null)).ToList();
		List<ColonistJob> validJobs = new List<ColonistJob>();
		foreach (Job job in sortedJobs) {
			if (job.resourcesToBuild.Count > 0) {
				KeyValuePair<bool,List<List<ResourceManager.ResourceAmount>>> returnKVP = CalculateColonistResourcesToPickup(colonist,job.resourcesToBuild);
				bool colonistHasAllResources = returnKVP.Key;
				List<ResourceManager.ResourceAmount> resourcesToPickup = returnKVP.Value[0];
				List<ResourceManager.ResourceAmount> resourcesColonistHas = returnKVP.Value[1];
				if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
					List<ContainerPickup> containerPickups = CalculateColonistPickupContainers(colonist,job,resourcesToPickup);
					if (containerPickups != null) { // If all resources were found in containers
						validJobs.Add(new ColonistJob(colonist,job,resourcesColonistHas,containerPickups,this,pathM));
					} else {
						continue;
					}
				} else if (colonistHasAllResources) { // If the colonist has all resources
					validJobs.Add(new ColonistJob(colonist,job,resourcesColonistHas,null,this,pathM));
				} else {
					continue;
				}
			} else {
				validJobs.Add(new ColonistJob(colonist,job,null,null,this,pathM));
			}
		}
		if (validJobs.Count > 0) {
			validJobs = validJobs.OrderByDescending(job => job.job.priority).ThenBy(job => job.cost).ToList();
			if (colonistJobs.ContainsKey(colonist)) {
				colonistJobs[colonist] = validJobs;
			} else {
				colonistJobs.Add(colonist,validJobs);
			}
		}
	}

	private Dictionary<ColonistManager.Colonist,List<ColonistJob>> colonistJobs = new Dictionary<ColonistManager.Colonist,List<ColonistJob>>();
	public void UpdateColonistJobs() {
		colonistJobs.Clear();
		List<ColonistManager.Colonist> availableColonists = colonistM.colonists.Where(colonist => colonist.job == null && colonist.overTile.walkable).ToList();
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
		Dictionary<ColonistManager.Colonist,ColonistJob> jobsGiven = new Dictionary<ColonistManager.Colonist,ColonistJob>();
		foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> colonistKVP in colonistJobs) {
			ColonistManager.Colonist colonist = colonistKVP.Key;
			List<ColonistJob> colonistJobsList = colonistKVP.Value;
			if (colonist.job == null && !colonist.playerMoved) {
				for (int i = 0; i < colonistJobsList.Count; i++) {
					ColonistJob colonistJob = colonistJobsList[i];
					bool bestColonistForJob = true;
					foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> otherColonistKVP in colonistJobs) {
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
						jobsGiven.Add(colonist,colonistJob);
						jobs.Remove(colonistJob.job);
						foreach (KeyValuePair<ColonistManager.Colonist,List<ColonistJob>> removeKVP in colonistJobs) {
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
		foreach (KeyValuePair<ColonistManager.Colonist,ColonistJob> jobGiven in jobsGiven) {
			jobGiven.Key.SetJob(jobGiven.Value);
		}
		if (gaveJob) {
			uiM.SetJobElements();
			UpdateColonistJobs();
		}
	}

	public bool JobOfTypeExistsAtTile(JobTypesEnum jobType,TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.jobType == jobType && job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.jobType == jobType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.jobType == jobType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}

	public bool JobOfPrefabTypeExistsAtTile(ResourceManager.TileObjectPrefabsEnum prefabType,TileManager.Tile tile) {
		if (jobs.Find(job => job.prefab.type == prefabType && job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.type == prefabType && colonist.job.tile == tile) != null) {
			return true;
		}
		if (colonistM.colonists.Find(colonist => colonist.storedJob != null && colonist.storedJob.prefab.type == prefabType && colonist.storedJob.tile == tile) != null) {
			return true;
		}
		return false;
	}
}