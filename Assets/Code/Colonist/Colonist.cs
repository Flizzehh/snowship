using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NProfession;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NColonist {
	public class Colonist : Human
	{
		public static readonly List<Colonist> colonists = new List<Colonist>();

		public bool playerMoved;

		// Job
		public Job Job { get; private set; }

		public readonly Queue<Job> Backlog = new();

		public event Action<Job> OnJobChanged;
		public event Action<Job> OnStoredJobChanged;

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
			moveSpeedMultiplier = -((Inventory.UsedWeight() - Inventory.maxWeight) / (float)Inventory.maxWeight) + 1;
			moveSpeedMultiplier = Mathf.Clamp(moveSpeedMultiplier, 0.1f, 1f);

			if (playerMoved && path.Count <= 0) {
				playerMoved = false;
			}

			base.Update();

			if (dead) {
				return;
			}

			// TODO Create event for OnTileChanged or something
			// if (overTileChanged) {
			// 	ColonistJob.UpdateColonistJobCosts(this);
			// }

			// if (Job != null) {
			// 	if (!Job.Started && overTile == Job.Tile) {
			// 		StartJob();
			// 	}
			// 	if (Job.Started && overTile == Job.Tile && !Mathf.Approximately(Job.Progress, 0)) {
			// 		WorkJob();
			// 	}
			// }
			if (Job == null) {
				if (path.Count <= 0) {
					Wander(null, 0);
				} else {
					WanderTimer = Random.Range(10f, 20f);
				}
			}
		}

		public override void Die() {
			base.Die();

			Job?.ChangeJobState(EJobState.Returned);
			colonists.Remove(this);
			foreach (Container container in Container.containers) {
				container.Inventory.ReleaseReservedResources(this);
			}
			if (GameManager.Get<HumanManager>().selectedHuman == this) {
				GameManager.Get<HumanManager>().SetSelectedHuman(null);
			}
			// ColonistJob.UpdateAllColonistJobCosts();
		}

		public List<Container> FindValidContainersToEmptyInventory() {
			return Container.GetContainersInRegion(overTile.region).Where(container => container.Inventory.UsedWeight() < container.Inventory.maxWeight && container.Inventory.UsedVolume() < container.Inventory.maxVolume).ToList();
		}

		public void EmptyInventory(List<Container> validContainers) {
			if (Inventory.UsedWeight() > 0 && Inventory.UsedVolume() > 0 && validContainers.Count > 0) {
				Job?.ChangeJobState(EJobState.Returned);
				Container closestContainer = validContainers.OrderBy(container => PathManager.RegionBlockDistance(container.tile.regionBlock, overTile.regionBlock, true, true, false)).ToList()[0];
				SetJob(new EmptyInventoryJob(closestContainer));
			}
		}

		private void UpdateNeeds() {
			foreach (NeedInstance need in needs) {
				NeedUtilities.CalculateNeedValue(need);
				bool checkNeed = false;
				if (need.colonist.Job == null) {
					checkNeed = true;
				} else {
					if (need.colonist.Job.Group == "Needs") {
						if (NeedUtilities.jobToNeedMap.ContainsKey(need.colonist.Job.JobName)) {
							if (need.prefab.priority < NeedPrefab.GetNeedPrefabFromEnum(NeedUtilities.jobToNeedMap[need.colonist.Job.JobName]).priority) {
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

		public void SetJob(Job job) {

			Job?.Close(); // Close existing Job

			Job = job;

			Job.AssignWorker(this);
			MoveToTile(Job.Tile, !Job.Tile.walkable);
			OnJobChanged?.Invoke(Job);
		}

		/*public void SetJob(ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			Job = colonistJob;
			Job.resourcesColonistHas = colonistJob.resourcesColonistHas;
			Job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && Job.containerPickups != null && Job.containerPickups.Count > 0) {
				foreach (ContainerPickup containerPickup in Job.containerPickups) {
					containerPickup.container.Inventory.ReserveResources(containerPickup.resourcesToPickup, this);
				}
			}
			if (Job.transferResources != null && Job.transferResources.Count > 0) {
				Container collectContainer = (Container)Job.Job.Tile.GetAllObjectInstances().Find(oi => oi is Container);
				if (collectContainer != null) {
					collectContainer.Inventory.ReserveResources(Job.transferResources, this);
				}
			}
			Job.SetColonist(this);
			MoveToTile(Job.tile, !Job.tile.walkable);

			if (colonistJob.Job.objectPrefab.type == ObjectPrefab.ObjectEnum.Sleep) {
				Debug.Log($"Sleeping: {colonistJob.Colonist.Name} at {GameManager.Get<TimeManager>().Time.DateString} {GameManager.Get<TimeManager>().Time.TimeString}");
			}
		}*/

		/*public void SetEatJob() {

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
		}*/

		/*public void StartJob() {
			Job.started = true;

			Job.jobProgress *= 1 + (1 - GetJobSkillMultiplier(Job.objectPrefab.jobType));

			if (Job.prefab.name == "Eat") {
				Job.jobProgress += needs.Find(need => need.prefab.type == ENeed.Food).GetValue();
			}
			if (Job.prefab.name == "Sleep") {
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				Job.jobProgress += restNeed.GetValue() / restNeed.prefab.baseIncreaseRate / restNeed.prefab.decreaseRateMultiplier / SimulationDateTime.PermanentTimerMultiplier;
				Job.jobProgress += Random.Range(Job.jobProgress * 0.1f, Job.jobProgress * 0.3f);
			}

			Job.colonistBuildTime = Job.jobProgress;

			// GameManager.Get<UIManagerOld>().SetJobElements();
		}*/

		/*public void WorkJob() {

			if (Job.prefab.name == "HarvestFarm" && Job.tile.farm == null) {
				Job.Remove();
				Job = null;
				return;
			}

			if (
				Job.prefab.name == "EmptyInventory" ||
				Job.prefab.name == "CollectFood" ||
				Job.prefab.name == "PickupResources"
			) {

				Container containerOnTile = Container.containers.Find(container => container.tile == Job.tile);
				if (containerOnTile == null) {
					Job.Remove();
					Job = null;
					return;
				}
			} else if (Job.prefab.name == "Sleep") { // TODO: Check that this still works - (removed timeM.minuteChanged, but added the * DeltaTime instead)
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				restNeed.ChangeValue(-restNeed.prefab.baseIncreaseRate * restNeed.prefab.decreaseRateMultiplier /* * GameManager.Get<TimeManager>().Time.DeltaTime#1#);
			}

			if (Job.activeObject != null) {
				Job.activeObject.SetActiveSprite(Job, true);
			}

			Job.jobProgress -= 1 * GameManager.Get<TimeManager>().Time.DeltaTime;

			if (Job.jobProgress <= 0 || Mathf.Approximately(Job.jobProgress, 0)) {
				Job.jobProgress = 0;
				FinishJob();
				return;
			}

			Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (Job.colonistBuildTime - Job.jobProgress) / Job.colonistBuildTime);
		}*/

		/*public void FinishJob() {
			Job finishedJob = Job;
			Job = null;

			MonoBehaviour.Destroy(finishedJob.jobPreview);

			SkillInstance skill = GetSkillFromJobType(finishedJob.objectPrefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJob.objectPrefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			// TODO finishedJob.prefab.RunFinishJobActions(finishedJob, this);

			if (finishedJob.activeObject != null) {
				finishedJob.activeObject.SetActiveSprite(finishedJob, false);
			}

			// ColonistJob.UpdateSingleColonistJobs(this);
			// GameManager.Get<UIManagerOld>().SetJobElements();
			// GameManager.Get<UIManagerOld>().UpdateSelectedColonistInformation(); // TODO Job Finished, Skill Updated
			// GameManager.Get<UIManagerOld>().UpdateSelectedContainerInfo();
			// GameManager.Get<UIManagerOld>().UpdateSelectedTradingPostInfo();
			// GameManager.Get<UIManagerOld>().UpdateTileInformation();

			// if (StoredJob != null) {
			// 	Update();
			// }
		}*/

		/*public void ReturnJob() {
			foreach (Container container in Container.containers) {
				container.Inventory.ReleaseReservedResources(this);
			}
			if (StoredJob != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(StoredJob.objectPrefab.jobType) || !StoredJob.prefab.returnable) {
					StoredJob.Remove();
					StoredJob = null;
					if (Job != null) {
						if (NeedUtilities.jobToNeedMap.ContainsKey(Job.objectPrefab.jobType) || !Job.prefab.returnable) {
							Job.Remove();
						}
						Job = null;
					}
					return;
				}
				GameManager.Get<JobManager>().AddExistingJob(StoredJob);
				if (StoredJob.jobPreview != null) {
					StoredJob.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				StoredJob = null;
				if (Job != null) {
					if (Job.jobPreview != null) {
						Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
					}
					Job = null;
				}
			} else if (Job != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(Job.objectPrefab.jobType) || !Job.prefab.returnable) {
					Job.Remove();
					Job = null;
					return;
				}
				GameManager.Get<JobManager>().AddExistingJob(Job);
				if (Job.jobPreview != null) {
					Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				Job = null;
			}
		}*/

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
			Job?.ChangeJobState(EJobState.Returned);
			if (Job != null) {
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

		public override void ChangeClothing(BodySection bodySection, Clothing clothing) {
			if (clothing == null || Inventory.resources.Find(ra => ra.Resource == clothing) != null) {
				base.ChangeClothing(bodySection, clothing);
			} else {
				Container container = NeedUtilities.FindClosestResourceAmountInContainers(this, new ResourceAmount(clothing, 1));
				if (container == null) {
					return;
				}

				ResourceAmount clothingToPickup = new(clothing, 1);
				container.Inventory.ReserveResources(new List<ResourceAmount> { clothingToPickup }, this);
				SetJob(new WearClothesJob(container.tile, container, clothing));
			}
		}
	}
}