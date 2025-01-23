using System;
using System.Collections.Generic;
using System.Linq;
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
		private Job job;
		public Job Job {
			get => job;
			set {
				job = value;
				OnJobChanged?.Invoke(value);
			}
		}

		private Job storedJob;
		public Job StoredJob {
			get => storedJob;
			set {
				storedJob = value;
				OnStoredJobChanged?.Invoke(value);
			}
		}
		public Job NeedJob;
		public readonly List<Job> Backlog = new();

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
		public float baseMood = 100;
		public float moodModifiersSum = 0;
		public float effectiveMood = 100;
		public readonly List<MoodModifierInstance> moodModifiers = new List<MoodModifierInstance>();

		public event Action<MoodModifierInstance> OnMoodAdded;
		public event Action<MoodModifierInstance> OnMoodRemoved;
		public event Action<float, float> OnMoodChanged;

		public Colonist(TileManager.Tile spawnTile, float startingHealth) : base(spawnTile, startingHealth) {
			obj.transform.SetParent(GameManager.resourceM.colonistParent.transform, false);

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

			GameManager.timeM.OnTimeChanged += OnTimeChanged;
		}

		private void OnTimeChanged(SimulationDateTime time) {
			UpdateNeeds();
			UpdateMoodModifiers();
			UpdateMood();
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

			if (Job != null) {
				if (!Job.started && overTile == Job.tile) {
					StartJob();
				}
				if (Job.started && overTile == Job.tile && !Mathf.Approximately(Job.jobProgress, 0)) {
					WorkJob();
				}
			}
			if (Job == null) {
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
			// GameManager.uiMOld.SetColonistElements(); // TODO Update colonist list
			// GameManager.uiMOld.SetJobElements();
			foreach (Container container in Container.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.humanM.SetSelectedHuman(null);
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
					new Job(
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
				if (need.colonist.Job == null) {
					checkNeed = true;
				} else {
					if (need.colonist.Job.prefab.group.name == "Need") {
						if (NeedUtilities.jobToNeedMap.ContainsKey(need.colonist.Job.objectPrefab.jobType)) {
							if (need.prefab.priority < NeedPrefab.GetNeedPrefabFromEnum(NeedUtilities.jobToNeedMap[need.colonist.Job.objectPrefab.jobType]).priority) {
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

		public void UpdateMoodModifiers() {
			foreach (MoodModifierGroup moodModifierGroup in MoodModifierGroup.moodModifierGroups) {
				MoodModifierUtilities.moodModifierFunctions[moodModifierGroup.type](this);
			}

			// for (int i = 0; i < moodModifiers.Count; i++) {
			// 	MoodModifierInstance moodModifier = moodModifiers[i];
			// 	//moodModifier.Update();
			// 	if (moodModifier.timer <= 0) {
			// 		RemoveMoodModifier(moodModifier.prefab.type);
			// 		i -= 1;
			// 	}
			// }
		}

		public void AddMoodModifier(MoodModifierEnum moodModifierEnum) {
			MoodModifierInstance moodToAdd = new(this, MoodModifierGroup.GetMoodModifierPrefabFromEnum(moodModifierEnum));
			MoodModifierInstance sameGroupMoodModifier = moodModifiers.Find(findMoodModifier => moodToAdd.prefab.group.type == findMoodModifier.prefab.group.type);
			if (sameGroupMoodModifier != null) {
				RemoveMoodModifier(sameGroupMoodModifier.prefab.type);
			}
			moodModifiers.Add(moodToAdd);
			OnMoodAdded?.Invoke(moodToAdd);
		}

		public void RemoveMoodModifier(MoodModifierEnum moodModifierEnum) {
			MoodModifierInstance moodToRemove = moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.type == moodModifierEnum);
			if (moodToRemove == null) {
				return;
			}
			moodModifiers.Remove(moodToRemove);
			OnMoodRemoved?.Invoke(moodToRemove);
		}

		public void UpdateMood() {

			float previousMood = effectiveMood;

			baseMood = Mathf.Clamp(Mathf.RoundToInt(100 - (needs.Sum(need => (need.GetValue() / (need.prefab.priority + 1))))), 0, 100);

			moodModifiersSum = moodModifiers.Sum(hM => hM.prefab.effectAmount);

			float targetMood = Mathf.Clamp(baseMood + moodModifiersSum, 0, 100);
			float moodChangeAmount = ((targetMood - effectiveMood) / (effectiveMood <= 0f ? 1f : effectiveMood));
			effectiveMood += moodChangeAmount /* * GameManager.timeM.Time.DeltaTime*/;
			effectiveMood = Mathf.Clamp(effectiveMood, 0, 100);

			if (Mathf.RoundToInt(previousMood) != Mathf.RoundToInt(effectiveMood)) {
				OnMoodChanged?.Invoke(effectiveMood, moodModifiersSum);
			}
		}

		public List<MoodModifierInstance> FindMoodModifierByGroupEnum(MoodModifierGroupEnum moodModifierGroupEnum, int polarity) {
			List<MoodModifierInstance> moodModifiersInGroup = moodModifiers.Where(moodModifier => moodModifier.prefab.group.type == moodModifierGroupEnum).ToList();
			if (polarity != 0) {
				moodModifiersInGroup = moodModifiersInGroup.Where(moodModifier => (moodModifier.prefab.effectAmount < 0) == (polarity < 0)).ToList();
			}
			return moodModifiersInGroup;
		}

		public MoodModifierInstance FindMoodModifierByEnum(MoodModifierEnum moodModifierEnum) {
			return moodModifiers.Find(moodModifier => moodModifier.prefab.type == moodModifierEnum);
		}

		public void SetJob(ColonistJob colonistJob, bool reserveResourcesInContainerPickups = true) {
			Job = colonistJob.job;
			Job.resourcesColonistHas = colonistJob.resourcesColonistHas;
			Job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && Job.containerPickups != null && Job.containerPickups.Count > 0) {
				foreach (ContainerPickup containerPickup in Job.containerPickups) {
					containerPickup.container.GetInventory().ReserveResources(containerPickup.resourcesToPickup, this);
				}
			}
			if (Job.transferResources != null && Job.transferResources.Count > 0) {
				Container collectContainer = (Container)Job.tile.GetAllObjectInstances().Find(oi => oi is Container);
				if (collectContainer != null) {
					collectContainer.GetInventory().ReserveResources(Job.transferResources, this);
				}
			}
			Job.SetColonist(this);
			MoveToTile(Job.tile, !Job.tile.walkable);

			if (colonistJob.job.objectPrefab.type == ObjectPrefab.ObjectEnum.Sleep) {
				Debug.Log($"Sleeping: {colonistJob.colonist.name} at {GameManager.timeM.Time.DateString} {GameManager.timeM.Time.TimeString}");
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
					new Job(
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

			// GameManager.uiMOld.SetJobElements();
		}

		public void WorkJob() {

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
				restNeed.ChangeValue(-restNeed.prefab.baseIncreaseRate * restNeed.prefab.decreaseRateMultiplier /* * GameManager.timeM.Time.DeltaTime*/);
			}

			if (Job.activeObject != null) {
				Job.activeObject.SetActiveSprite(Job, true);
			}

			Job.jobProgress -= 1 * GameManager.timeM.Time.DeltaTime;

			if (Job.jobProgress <= 0 || Mathf.Approximately(Job.jobProgress, 0)) {
				Job.jobProgress = 0;
				FinishJob();
				return;
			}

			Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (Job.colonistBuildTime - Job.jobProgress) / Job.colonistBuildTime);
		}

		public void FinishJob() {
			Job finishedJob = Job;
			Job = null;

			MonoBehaviour.Destroy(finishedJob.jobPreview);
			if (finishedJob.objectPrefab.addToTileWhenBuilt) {
				finishedJob.tile.SetObject(ObjectInstance.CreateObjectInstance(finishedJob.objectPrefab, finishedJob.variation, finishedJob.tile, finishedJob.rotationIndex, true));
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.objectPrefab.layer).obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.objectPrefab.layer).FinishCreation();
				if (finishedJob.objectPrefab.canRotate) {
					finishedJob.tile.GetObjectInstanceAtLayer(finishedJob.objectPrefab.layer).obj.GetComponent<SpriteRenderer>().sprite = finishedJob.objectPrefab.GetBitmaskSpritesForVariation(finishedJob.variation)[finishedJob.rotationIndex];
				}
			}

			SkillInstance skill = GetSkillFromJobType(finishedJob.objectPrefab.jobType);
			if (skill != null) {
				skill.AddExperience(finishedJob.objectPrefab.timeToBuild);
			}

			MoveToClosestWalkableTile(true);

			finishedJob.prefab.RunFinishJobActions(finishedJob, this);

			if (finishedJob.activeObject != null) {
				finishedJob.activeObject.SetActiveSprite(finishedJob, false);
			}

			ColonistJob.UpdateSingleColonistJobs(this);
			// GameManager.uiMOld.SetJobElements();
			// GameManager.uiMOld.UpdateSelectedColonistInformation(); // TODO Job Finished, Skill Updated
			// GameManager.uiMOld.UpdateSelectedContainerInfo();
			// GameManager.uiMOld.UpdateSelectedTradingPostInfo();
			// GameManager.uiMOld.UpdateTileInformation();

			if (StoredJob != null) {
				Update();
			}
		}

		public void ReturnJob() {
			foreach (Container container in Container.containers) {
				container.GetInventory().ReleaseReservedResources(this);
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
				GameManager.jobM.AddExistingJob(StoredJob);
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
				GameManager.jobM.AddExistingJob(Job);
				if (Job.jobPreview != null) {
					Job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.25f);
				}
				Job = null;
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

		public override string GetCurrentActionString() {
			if (Job != null) {
				return Job.prefab.GetJobDescription(Job);
			}

			return "Wandering around.";
		}

		public override string GetStoredActionString() {
			if (StoredJob != null) {
				return StoredJob.prefab.GetJobDescription(StoredJob);
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
						new Job(
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