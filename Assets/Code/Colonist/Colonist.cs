using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColony;
using Snowship.NJob;
using Snowship.NProfession;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NColonist {
	public class Colonist : HumanManager.Human {

		public static readonly List<Colonist> colonists = new List<Colonist>();

		public bool playerMoved;

		// Job
		private JobInstance jobInstance;
		public JobInstance JobInstance {
			get => jobInstance;
			set {
				jobInstance = value;
				OnJobChanged?.Invoke(value);
			}
		}

		private JobInstance storedJobInstance;
		public JobInstance StoredJobInstance {
			get => storedJobInstance;
			set {
				storedJobInstance = value;
				OnStoredJobChanged?.Invoke(value);
			}
		}
		public JobInstance NeedJobInstance;
		public readonly List<JobInstance> Backlog = new();

		public event Action<JobInstance> OnJobChanged;
		public event Action<JobInstance> OnStoredJobChanged;

		// Professions
		public readonly List<Profession> professions = new List<Profession>();

		// Skills
		public readonly List<SkillInstance> skills = new List<SkillInstance>();

		// Traits
		public readonly List<TraitInstance> traits = new List<TraitInstance>();

		// Needs
		public readonly List<NeedInstance> needs = new List<NeedInstance>();

		// Mood
		public Moods Moods;

		public Colonist(TileManager.Tile spawnTile, float startingHealth) : base(spawnTile, startingHealth) {
			obj.transform.SetParent(GameManager.SharedReferences.LifeParent, false);

			Moods = new Moods(this);

			foreach (ProfessionPrefab professionPrefab in ProfessionPrefab.professionPrefabs) {
				professions.Add(
					new Profession(
						professionPrefab,
						this,
						Mathf.RoundToInt(ProfessionPrefab.professionPrefabs.Count / 2f)
					));
			}

			foreach (SkillPrefab skillPrefab in SkillPrefab.skillPrefabs) {
				skills.Add(
					new SkillInstance(
						this,
						skillPrefab,
						true,
						0
					));
			}

			foreach (NeedPrefab needPrefab in NeedPrefab.needPrefabs) {
				needs.Add(
					new NeedInstance(
						this,
						needPrefab
					));
			}
			needs = needs.OrderBy(need => need.prefab.priority).ToList();

			colonists.Add(this);

			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
		}

		protected Colonist() {

		}

		private void OnTimeChanged(SimulationDateTime time) {
			UpdateNeeds();
		}

		public float CalculateNeedSumWeightedByPriority() {
			return needs.Sum(need => need.GetValue() / (need.prefab.priority + 1));
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

			if (overTileChanged) {
				ColonistJob.UpdateColonistJobCosts(this);
			}

			if (JobInstance != null) {
				if (!JobInstance.started && overTile == JobInstance.tile) {
					StartJob();
				}
				if (JobInstance.started && overTile == JobInstance.tile && !Mathf.Approximately(JobInstance.jobProgress, 0)) {
					WorkJob();
				}
			}
			if (JobInstance == null) {
				if (path.Count <= 0) {
					List<Container> validEmptyInventoryContainers = FindValidContainersToEmptyInventory();
					int inventoryWeight = GetInventory().UsedWeight();
					int inventoryVolume = GetInventory().UsedVolume();
					if (validEmptyInventoryContainers.Count > 0
						&& (inventoryWeight >= GetInventory().maxWeight || inventoryVolume >= GetInventory().maxVolume
							|| ((inventoryWeight > 0 || inventoryVolume > 0) && ColonistJob.GetColonistJobsCountForColonist(this) <= 0))
					) {
						EmptyInventory(validEmptyInventoryContainers);
					} else {
						Wander(null, 0);
					}
				} else {
					wanderTimer = Random.Range(10f, 20f);
				}
			}
		}

		public override void Die() {
			base.Die();

			ReturnJob();
			colonists.Remove(this);
			// GameManager.Get<UIManagerOld>().SetColonistElements(); // TODO Update colonist list
			// GameManager.Get<UIManagerOld>().SetJobElements();
			foreach (Container container in Container.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (GameManager.Get<HumanManager>().selectedHuman == this) {
				GameManager.Get<HumanManager>().SetSelectedHuman(null);
			}
			ColonistJob.UpdateAllColonistJobCosts();
		}

		public List<Container> FindValidContainersToEmptyInventory() {
			return Container.GetContainersInRegion(overTile.region).Where(container => container.GetInventory().UsedWeight() < container.GetInventory().maxWeight && container.GetInventory().UsedVolume() < container.GetInventory().maxVolume).ToList();
		}

		public void EmptyInventory(List<Container> validContainers) {
			if (GetInventory().UsedWeight() > 0 && GetInventory().UsedVolume() > 0 && validContainers.Count > 0) {
				ReturnJob();
				Container closestContainer = validContainers.OrderBy(container => PathManager.RegionBlockDistance(container.tile.regionBlock, overTile.regionBlock, true, true, false)).ToList()[0];
				SetJob(new ColonistJob(
					this,
					new JobInstance(
						JobPrefab.GetJobPrefabByName("EmptyInventory"),
						closestContainer.tile,
						ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.EmptyInventory),
						null,
						0
					),
					null,
					null
				));
			}
		}

		private void UpdateNeeds() {
			foreach (NeedInstance need in needs) {
				NeedUtilities.CalculateNeedValue(need);
				bool checkNeed = false;
				if (need.colonist.JobInstance == null) {
					checkNeed = true;
				} else {
					if (need.colonist.JobInstance.prefab.group.name == "Need") {
						if (NeedUtilities.jobToNeedMap.ContainsKey(need.colonist.JobInstance.objectPrefab.jobType)) {
							if (need.prefab.priority < NeedPrefab.GetNeedPrefabFromEnum(NeedUtilities.jobToNeedMap[need.colonist.JobInstance.objectPrefab.jobType]).priority) {
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
					if (NeedUtilities.NeedToReactionFunctionMap[need.prefab.type](need)) {
						break;
					}
				}
			}
		}



		public void SetJob(ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			JobInstance = colonistJob.JobInstance;
			JobInstance.resourcesColonistHas = colonistJob.resourcesColonistHas;
			JobInstance.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && JobInstance.containerPickups != null && JobInstance.containerPickups.Count > 0) {
				foreach (ContainerPickup containerPickup in JobInstance.containerPickups) {
					containerPickup.container.GetInventory().ReserveResources(containerPickup.resourcesToPickup, this);
				}
			}
			if (JobInstance.transferResources != null && JobInstance.transferResources.Count > 0) {
				Container collectContainer = (Container)JobInstance.tile.GetAllObjectInstances().Find(oi => oi is Container);
				if (collectContainer != null) {
					collectContainer.GetInventory().ReserveResources(JobInstance.transferResources, this);
				}
			}
			JobInstance.SetColonist(this);
			MoveToTile(JobInstance.tile, !JobInstance.tile.walkable);

			if (colonistJob.JobInstance.objectPrefab.type == ObjectPrefab.ObjectEnum.Sleep) {
				Debug.Log($"Sleeping: {colonistJob.colonist.name} at {GameManager.Get<TimeManager>().Time.DateString} {GameManager.Get<TimeManager>().Time.TimeString}");
			}
		}

		public void SetEatJob() {

			// Find a chair (ideally next to a table) for the colonist to sit at to eat
			List<ObjectInstance> chairs = new();
			foreach (ObjectPrefab chairPrefab in ObjectPrefabSubGroup.GetObjectPrefabSubGroupByEnum(ObjectPrefabSubGroup.ObjectSubGroupEnum.Chairs).prefabs) {
				List<ObjectInstance> chairsFromPrefab = ObjectInstance.GetObjectInstancesByPrefab(chairPrefab);
				if (chairsFromPrefab != null) {
					chairs.AddRange(chairsFromPrefab);
				}
			}
			chairs.Select(chair => chair.tile.region == overTile.region);
			chairs.OrderBy(chair => PathManager.RegionBlockDistance(overTile.regionBlock, chair.tile.regionBlock, true, true, false));
			chairs.OrderByDescending(chair => chair.tile.surroundingTiles.Find(surroundingTile => {
				ObjectInstance tableNextToChair = surroundingTile.GetObjectInstanceAtLayer(2);
				if (tableNextToChair != null) {
					return tableNextToChair.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Tables;
				}
				return false;
			}) != null);

			SetJob(
				new ColonistJob(
					this,
					new JobInstance(
						JobPrefab.GetJobPrefabByName("Eat"),
						chairs.Count > 0 ? chairs[0].tile : overTile,
						ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.Eat),
						null,
						0
					),
					null,
					null
				)
			);
		}

		public void StartJob() {
			JobInstance.started = true;

			JobInstance.jobProgress *= 1 + (1 - GetJobSkillMultiplier(JobInstance.objectPrefab.jobType));

			if (JobInstance.prefab.name == "Eat") {
				JobInstance.jobProgress += needs.Find(need => need.prefab.type == ENeed.Food).GetValue();
			}
			if (JobInstance.prefab.name == "Sleep") {
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				JobInstance.jobProgress += restNeed.GetValue() / restNeed.prefab.baseIncreaseRate / restNeed.prefab.decreaseRateMultiplier / SimulationDateTime.PermanentTimerMultiplier;
				JobInstance.jobProgress += Random.Range(JobInstance.jobProgress * 0.1f, JobInstance.jobProgress * 0.3f);
			}

			JobInstance.colonistBuildTime = JobInstance.jobProgress;

			// GameManager.Get<UIManagerOld>().SetJobElements();
		}

		public void WorkJob() {

			if (JobInstance.prefab.name == "HarvestFarm" && JobInstance.tile.farm == null) {
				JobInstance.Remove();
				JobInstance = null;
				return;
			}

			if (
				JobInstance.prefab.name == "EmptyInventory" ||
				JobInstance.prefab.name == "CollectFood" ||
				JobInstance.prefab.name == "PickupResources"
			) {

				Container containerOnTile = Container.containers.Find(container => container.tile == JobInstance.tile);
				if (containerOnTile == null) {
					JobInstance.Remove();
					JobInstance = null;
					return;
				}
			} else if (JobInstance.prefab.name == "Sleep") { // TODO: Check that this still works - (removed timeM.minuteChanged, but added the * DeltaTime instead)
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				restNeed.ChangeValue(-restNeed.prefab.baseIncreaseRate * restNeed.prefab.decreaseRateMultiplier /* * GameManager.Get<TimeManager>().Time.DeltaTime*/);
			}

			if (JobInstance.activeObject != null) {
				JobInstance.activeObject.SetActiveSprite(JobInstance, true);
			}

			JobInstance.jobProgress -= 1 * GameManager.Get<TimeManager>().Time.DeltaTime;

			if (JobInstance.jobProgress <= 0 || Mathf.Approximately(JobInstance.jobProgress, 0)) {
				JobInstance.jobProgress = 0;
				FinishJob();
				return;
			}

			JobInstance.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (JobInstance.colonistBuildTime - JobInstance.jobProgress) / JobInstance.colonistBuildTime);
		}

		public void FinishJob() {
			JobInstance finishedJobInstance = JobInstance;
			JobInstance = null;

			MonoBehaviour.Destroy(finishedJobInstance.jobPreview);
			if (finishedJobInstance.objectPrefab.addToTileWhenBuilt) {
				finishedJobInstance.tile.SetObject(ObjectInstance.CreateObjectInstance(finishedJobInstance.objectPrefab, finishedJobInstance.variation, finishedJobInstance.tile, finishedJobInstance.rotationIndex, true));
				finishedJobInstance.tile.GetObjectInstanceAtLayer(finishedJobInstance.objectPrefab.layer).obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				finishedJobInstance.tile.GetObjectInstanceAtLayer(finishedJobInstance.objectPrefab.layer).FinishCreation();
				if (finishedJobInstance.objectPrefab.canRotate) {
					finishedJobInstance.tile.GetObjectInstanceAtLayer(finishedJobInstance.objectPrefab.layer).obj.GetComponent<SpriteRenderer>().sprite = finishedJobInstance.objectPrefab.GetBitmaskSpritesForVariation(finishedJobInstance.variation)[finishedJobInstance.rotationIndex];
				}
			}

			SkillInstance skill = GetSkillFromJobType(finishedJobInstance.objectPrefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJobInstance.objectPrefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			// TODO finishedJobInstance.prefab.RunFinishJobActions(finishedJobInstance, this);

			if (finishedJobInstance.activeObject != null) {
				finishedJobInstance.activeObject.SetActiveSprite(finishedJobInstance, false);
			}

			ColonistJob.UpdateSingleColonistJobs(this);
			// GameManager.Get<UIManagerOld>().SetJobElements();
			// GameManager.Get<UIManagerOld>().UpdateSelectedColonistInformation(); // TODO Job Finished, Skill Updated
			// GameManager.Get<UIManagerOld>().UpdateSelectedContainerInfo();
			// GameManager.Get<UIManagerOld>().UpdateSelectedTradingPostInfo();
			// GameManager.Get<UIManagerOld>().UpdateTileInformation();

			if (StoredJobInstance != null) {
				Update();
			}
		}

		public void ReturnJob() {
			foreach (Container container in Container.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (StoredJobInstance != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(StoredJobInstance.objectPrefab.jobType) || !StoredJobInstance.prefab.returnable) {
					StoredJobInstance.Remove();
					StoredJobInstance = null;
					if (JobInstance != null) {
						if (NeedUtilities.jobToNeedMap.ContainsKey(JobInstance.objectPrefab.jobType) || !JobInstance.prefab.returnable) {
							JobInstance.Remove();
						}
						JobInstance = null;
					}
					return;
				}
				GameManager.Get<JobManager>().AddExistingJob(StoredJobInstance);
				if (StoredJobInstance.jobPreview != null) {
					StoredJobInstance.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				StoredJobInstance = null;
				if (JobInstance != null) {
					if (JobInstance.jobPreview != null) {
						JobInstance.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
					}
					JobInstance = null;
				}
			} else if (JobInstance != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(JobInstance.objectPrefab.jobType) || !JobInstance.prefab.returnable) {
					JobInstance.Remove();
					JobInstance = null;
					return;
				}
				GameManager.Get<JobManager>().AddExistingJob(JobInstance);
				if (JobInstance.jobPreview != null) {
					JobInstance.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				JobInstance = null;
			}
		}

		public void MoveToClosestWalkableTile(bool careIfOvertileIsWalkable) {
			if (!careIfOvertileIsWalkable || !overTile.walkable) {
				List<TileManager.Tile> walkableSurroundingTiles = overTile.surroundingTiles.Where(tile => tile != null && tile.walkable).ToList();
				if (walkableSurroundingTiles.Count > 0) {
					MoveToTile(walkableSurroundingTiles[Random.Range(0, walkableSurroundingTiles.Count)], false);
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
						List<TileManager.Tile> validTiles = GameManager.Get<ColonyManager>().colony.map.tiles.Where(tile => tile.walkable).OrderBy(tile => Vector2.Distance(tile.obj.transform.position, overTile.obj.transform.position)).ToList();
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
			if (JobInstance != null) {
				MoveToTile(tile, false);
			} else {
				MoveToTile(tile, false);
			}
		}

		public Profession GetProfessionFromType(string type) {
			return professions.Find(p => p.prefab.type == type);
		}

		public SkillInstance GetSkillFromEnum(ESkill type) {
			return skills.Find(s => s.prefab.type == type);
		}

		public SkillInstance GetSkillFromJobType(string jobType) {
			return skills.Find(s => s.prefab.affectedJobTypes.ContainsKey(jobType));
		}

		public float GetJobSkillMultiplier(string jobType) {
			SkillInstance skill = GetSkillFromJobType(jobType);
			if (skill != null) {
				return 1 * (-(1f / (skill.prefab.affectedJobTypes[jobType] * skill.Level + 1)) + 1);
			}
			return 1.0f;
		}

		public override void SetName(string name) {
			base.SetName(name);
			SetNameColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen100));
		}

		public override string GetCurrentActionString() {
			if (JobInstance != null) {
				// TODO return JobInstance.prefab.GetJobDescription(JobInstance);
			}

			return "Wandering around.";
		}

		public override string GetStoredActionString() {
			if (StoredJobInstance != null) {
				// TODO return StoredJobInstance.prefab.GetJobDescription(StoredJobInstance);
			}

			return string.Empty;
		}

		public override void ChangeClothing(Appearance appearance, Clothing clothing) {

			if (clothing == null || GetInventory().resources.Find(ra => ra.Resource == clothing) != null) {

				base.ChangeClothing(appearance, clothing);

			} else {

				Container container = NeedUtilities.FindClosestResourceAmountInContainers(this, new ResourceAmount(clothing, 1));

				if (container != null) {

					ResourceAmount clothingToPickup = new(clothing, 1);

					container.GetInventory().ReserveResources(
						new List<ResourceAmount> {
							clothingToPickup
						},
						this
					);

					Backlog.Add(
						new JobInstance(
							JobPrefab.GetJobPrefabByName("WearClothes"),
							container.tile,
							ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.WearClothes),
							null,
							0
						) {
							requiredResources = new List<ResourceAmount> { clothingToPickup },
							resourcesColonistHas = new List<ResourceAmount>(),
							containerPickups = new List<ContainerPickup> {
								new ContainerPickup(
									container,
									new List<ResourceAmount> { clothingToPickup }
								)
							}
						}
					);
				}
			}
		}
	}
}