using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;
using Snowship.NProfession;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using static Snowship.NColonist.ColonistManager;

namespace Snowship.NColonist {
	public class Colonist : HumanManager.Human {

		public static readonly List<Colonist> colonists = new List<Colonist>();

		public bool playerMoved;

		// Job
		public Job job;
		public Job storedJob;
		public Job needJob;
		public readonly List<Job> backlog = new();

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
		public float moodModifiersSum = 100;
		public float effectiveMood = 100;
		public readonly List<MoodModifierInstance> moodModifiers = new List<MoodModifierInstance>();

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
			GameManager.uiMOld.SetColonistElements();
			GameManager.uiMOld.SetJobElements();
			foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.humanM.SetSelectedHuman(null);
			}
			ColonistJob.UpdateAllColonistJobCosts();
		}

		public List<ResourceManager.Container> FindValidContainersToEmptyInventory() {
			return GameManager.resourceM.GetContainersInRegion(overTile.region).Where(container => container.GetInventory().UsedWeight() < container.GetInventory().maxWeight && container.GetInventory().UsedVolume() < container.GetInventory().maxVolume).ToList();
		}

		public void EmptyInventory(List<ResourceManager.Container> validContainers) {
			if (GetInventory().UsedWeight() > 0 && GetInventory().UsedVolume() > 0 && validContainers.Count > 0) {
				ReturnJob();
				ResourceManager.Container closestContainer = validContainers.OrderBy(container => PathManager.RegionBlockDistance(container.tile.regionBlock, overTile.regionBlock, true, true, false)).ToList()[0];
				SetJob(new ColonistJob(
					this,
					new Job(
						JobPrefab.GetJobPrefabByName("EmptyInventory"),
						closestContainer.tile,
						GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.EmptyInventory),
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
				if (need.colonist.job == null) {
					checkNeed = true;
				} else {
					if (need.colonist.job.prefab.group.name == "Need") {
						if (NeedUtilities.jobToNeedMap.ContainsKey(need.colonist.job.objectPrefab.jobType)) {
							if (need.prefab.priority < NeedPrefab.GetNeedPrefabFromEnum(NeedUtilities.jobToNeedMap[need.colonist.job.objectPrefab.jobType]).priority) {
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
			MoodModifierInstance moodModifier = new MoodModifierInstance(this, MoodModifierGroup.GetMoodModifierPrefabFromEnum(moodModifierEnum));
			MoodModifierInstance sameGroupMoodModifier = moodModifiers.Find(findMoodModifier => moodModifier.prefab.group.type == findMoodModifier.prefab.group.type);
			if (sameGroupMoodModifier != null) {
				RemoveMoodModifier(sameGroupMoodModifier.prefab.type);
			}
			moodModifiers.Add(moodModifier);
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.uiMOld.RemakeSelectedColonistMoodModifiers();
			}
		}

		public void RemoveMoodModifier(MoodModifierEnum moodModifierEnum) {
			moodModifiers.Remove(moodModifiers.Find(findMoodModifier => findMoodModifier.prefab.type == moodModifierEnum));
			if (GameManager.humanM.selectedHuman == this) {
				GameManager.uiMOld.RemakeSelectedColonistMoodModifiers();
			}
		}

		public void UpdateMood() {
			baseMood = Mathf.Clamp(Mathf.RoundToInt(100 - (needs.Sum(need => (need.GetValue() / (need.prefab.priority + 1))))), 0, 100);

			moodModifiersSum = moodModifiers.Sum(hM => hM.prefab.effectAmount);

			float targetMood = Mathf.Clamp(baseMood + moodModifiersSum, 0, 100);
			float moodChangeAmount = ((targetMood - effectiveMood) / (effectiveMood <= 0f ? 1f : effectiveMood));
			effectiveMood += moodChangeAmount * GameManager.timeM.Time.DeltaTime;
			effectiveMood = Mathf.Clamp(effectiveMood, 0, 100);
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
			job = colonistJob.job;
			job.resourcesColonistHas = colonistJob.resourcesColonistHas;
			job.containerPickups = colonistJob.containerPickups;
			if (reserveResourcesInContainerPickups && (job.containerPickups != null && job.containerPickups.Count > 0)) {
				foreach (ContainerPickup containerPickup in job.containerPickups) {
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
			GameManager.uiMOld.SetJobElements();

			if (colonistJob.job.objectPrefab.type == ResourceManager.ObjectEnum.Sleep) {
				Debug.Log($"Sleeping: {colonistJob.colonist.name} at {GameManager.timeM.Time.DateString} {GameManager.timeM.Time.TimeString}");
			}
		}

		public void SetEatJob() {

			// Find a chair (ideally next to a table) for the colonist to sit at to eat
			List<ResourceManager.ObjectInstance> chairs = new List<ResourceManager.ObjectInstance>();
			foreach (ResourceManager.ObjectPrefab chairPrefab in GameManager.resourceM.GetObjectPrefabSubGroupByEnum(ResourceManager.ObjectSubGroupEnum.Chairs).prefabs) {
				List<ResourceManager.ObjectInstance> chairsFromPrefab = GameManager.resourceM.GetObjectInstancesByPrefab(chairPrefab);
				if (chairsFromPrefab != null) {
					chairs.AddRange(chairsFromPrefab);
				}
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
				new ColonistJob(
					this,
					new Job(
						JobPrefab.GetJobPrefabByName("Eat"),
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

			job.jobProgress *= (1 + (1 - GetJobSkillMultiplier(job.objectPrefab.jobType)));

			if (job.prefab.name == "Eat") {
				job.jobProgress += needs.Find(need => need.prefab.type == ENeed.Food).GetValue();
			}
			if (job.prefab.name == "Sleep") {
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				job.jobProgress += restNeed.GetValue() / restNeed.prefab.baseIncreaseRate / restNeed.prefab.decreaseRateMultiplier / SimulationDateTime.PermanentTimerMultiplier;
				job.jobProgress += Random.Range(job.jobProgress * 0.1f, job.jobProgress * 0.3f);
			}

			job.colonistBuildTime = job.jobProgress;

			GameManager.uiMOld.SetJobElements();
		}

		public void WorkJob() {

			if (job.prefab.name == "HarvestFarm" && job.tile.farm == null) {
				job.Remove();
				job = null;
				return;
			}

			if (
				job.prefab.name == "EmptyInventory" ||
				job.prefab.name == "CollectFood" ||
				job.prefab.name == "PickupResources"
			) {

				ResourceManager.Container containerOnTile = GameManager.resourceM.containers.Find(container => container.tile == job.tile);
				if (containerOnTile == null) {
					job.Remove();
					job = null;
					return;
				}
			} else if (job.prefab.name == "Sleep") { // TODO: Check that this still works - (removed timeM.minuteChanged, but added the * DeltaTime instead)
				NeedInstance restNeed = needs.Find(need => need.prefab.type == ENeed.Rest);
				restNeed.ChangeValue(-restNeed.prefab.baseIncreaseRate * restNeed.prefab.decreaseRateMultiplier * GameManager.timeM.Time.DeltaTime);
			}

			if (job.activeObject != null) {
				job.activeObject.SetActiveSprite(job, true);
			}

			job.jobProgress -= 1 * GameManager.timeM.Time.DeltaTime;

			if (job.jobProgress <= 0 || Mathf.Approximately(job.jobProgress, 0)) {
				job.jobProgress = 0;
				FinishJob();
				return;
			}

			job.jobPreview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, ((job.colonistBuildTime - job.jobProgress) / job.colonistBuildTime));
		}

		public void FinishJob() {
			Job finishedJob = job;
			job = null;

			MonoBehaviour.Destroy(finishedJob.jobPreview);
			if (finishedJob.objectPrefab.addToTileWhenBuilt) {
				finishedJob.tile.SetObject(GameManager.resourceM.CreateObjectInstance(finishedJob.objectPrefab, finishedJob.variation, finishedJob.tile, finishedJob.rotationIndex, true));
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
			GameManager.uiMOld.SetJobElements();
			GameManager.uiMOld.UpdateSelectedColonistInformation();
			GameManager.uiMOld.UpdateSelectedContainerInfo();
			GameManager.uiMOld.UpdateSelectedTradingPostInfo();
			GameManager.uiMOld.UpdateTileInformation();

			if (storedJob != null) {
				Update();
			}
		}

		public void ReturnJob() {
			foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
				container.GetInventory().ReleaseReservedResources(this);
			}
			if (storedJob != null) {
				if (NeedUtilities.jobToNeedMap.ContainsKey(storedJob.objectPrefab.jobType) || !storedJob.prefab.returnable) {
					storedJob.Remove();
					storedJob = null;
					if (job != null) {
						if (NeedUtilities.jobToNeedMap.ContainsKey(job.objectPrefab.jobType) || !job.prefab.returnable) {
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
				if (NeedUtilities.jobToNeedMap.ContainsKey(job.objectPrefab.jobType) || !job.prefab.returnable) {
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
			if (job != null) {
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
				return 1 * (-(1f / (((skill.prefab.affectedJobTypes[jobType]) * (skill.level)) + 1)) + 1);
			}
			return 1.0f;
		}

		public override void SetName(string name) {
			base.SetName(name);
			SetNameColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen100));
		}

		public override string GetCurrentActionString() {
			if (job != null) {
				return job.prefab.GetJobDescription(job);
			}

			return "Wandering around.";
		}

		public override string GetStoredActionString() {
			if (storedJob != null) {
				return storedJob.prefab.GetJobDescription(storedJob);
			}

			return string.Empty;
		}

		public override void ChangeClothing(Appearance appearance, ResourceManager.Clothing clothing) {

			if (clothing == null || GetInventory().ContainsResourceAmount(new ResourceManager.ResourceAmount(clothing, 1))) {

				base.ChangeClothing(appearance, clothing);

			} else {

				ResourceManager.Container container = NeedUtilities.FindClosestResourceAmountInContainers(this, new ResourceManager.ResourceAmount(clothing, 1));

				if (container != null) {

					ResourceManager.ResourceAmount clothingToPickup = new ResourceManager.ResourceAmount(clothing, 1);

					container.GetInventory().ReserveResources(
						new List<ResourceManager.ResourceAmount> {
							clothingToPickup
						},
						this
					);

					backlog.Add(
						new Job(
							JobPrefab.GetJobPrefabByName("WearClothes"),
							container.tile,
							GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.WearClothes),
							null,
							0
						) {
							requiredResources = new List<ResourceManager.ResourceAmount> { clothingToPickup },
							resourcesColonistHas = new List<ResourceManager.ResourceAmount>(),
							containerPickups = new List<ContainerPickup> {
								new ContainerPickup(
									container,
									new List<ResourceManager.ResourceAmount> { clothingToPickup }
								)
							}
						}
					);
				}
			}
		}
	}
}
