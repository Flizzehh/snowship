﻿using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NProfession;
using Snowship.NResource;
using Snowship.NUI;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob {

	public class JobManager : IManager {
		private readonly JobRegistry jobRegistry = new();

		public void OnCreate() {

			selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
			selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
			selectedPrefabPreview.GetComponent<SpriteRenderer>().color = ColourUtilities.GetColour(ColourUtilities.EColour.WhiteAlpha128);
		}

		private bool changedJobList = false;
		private int rotationIndex = 0;

		public void OnUpdate() {
			if (changedJobList) {
				ColonistJob.UpdateColonistJobs();
				// GameManager.Get<UIManagerOld>().SetJobElements();
				changedJobList = false;
			}
			GetJobSelectionArea();
			UpdateSelectedPrefabInfo();
		}

		private SelectedPrefab selectedPrefab;

		public void SetSelectedPrefab(ObjectPrefab newPrefab, Variation newVariation) {
			if (selectedPrefab == null || selectedPrefab.prefab != newPrefab || !Variation.Equals(selectedPrefab.variation, newVariation)) {
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
			Vector2 mousePosition = GameManager.Get<CameraManager>().camera.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile tile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(mousePosition);
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
						GameManager.Get<UIManagerOld>().GetSelectionSizePanel().SetActive(false);
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
					// TODO GameManager.Get<UIManagerOld>().GetSelectionSizePanel().SetActive(true);
				}
			} else {
				selectedPrefabPreview.SetActive(false);
				// TODO GameManager.Get<UIManagerOld>().GetSelectionSizePanel().SetActive(false);
			}
		}

		public enum SelectionModifiersEnum {
			Outline, Walkable, OmitWalkable, WalkableIncludingFences, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
			Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms, Roofs, OmitRoofs, CloseToSupport,
			ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, BiomeSupportsSelectedPlants, OmitObjectInstancesOnAdditionalTiles, Fillable
		};

		private static readonly Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ObjectPrefab, Variation, bool>> selectionModifierFunctions = new Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ObjectPrefab, Variation, bool>>() {
			{
				SelectionModifiersEnum.Walkable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.walkable;
				}
			}, {
				SelectionModifiersEnum.WalkableIncludingFences, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					ObjectInstance objectInstance = posTile.GetObjectInstanceAtLayer(2);
					if (objectInstance != null && objectInstance.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Fences) {
				return true;
			}
			return posTile.walkable;
				}
			}, {
				SelectionModifiersEnum.OmitWalkable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return !posTile.walkable;
				}
			}, {
				SelectionModifiersEnum.Buildable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.buildable;
				}
			}, {
				SelectionModifiersEnum.OmitBuildable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return !posTile.buildable;
				}
			}, {
				SelectionModifiersEnum.StoneTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Stone;
				}
			}, {
				SelectionModifiersEnum.OmitStoneTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Stone;
				}
			}, {
				SelectionModifiersEnum.AllWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water;
				}
			}, {
				SelectionModifiersEnum.OmitAllWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water;
				}
			}, {
				SelectionModifiersEnum.LiquidWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater];
				}
			}, {
				SelectionModifiersEnum.OmitLiquidWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return !posTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater];
				}
			}, {
				SelectionModifiersEnum.OmitNonStoneAndWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water && posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Stone;
				}
			}, {
				SelectionModifiersEnum.Plants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.plant != null;
				}
			}, {
				SelectionModifiersEnum.OmitPlants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.plant == null;
				}
			}, {
				SelectionModifiersEnum.OmitSameLayerJobs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					foreach (JobInstance job in JobInstance.jobs) {
				if (job.objectPrefab.layer == prefab.layer) {
					if (job.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in job.objectPrefab.multiTilePositions[job.rotationIndex]) {
						if (GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(job.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (Colonist colonist in Colonist.colonists) {
				if (colonist.JobInstance != null && colonist.JobInstance.objectPrefab.layer == prefab.layer) {
					if (colonist.JobInstance.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.JobInstance.objectPrefab.multiTilePositions[colonist.JobInstance.rotationIndex]) {
						if (GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(colonist.JobInstance.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			foreach (Colonist colonist in Colonist.colonists) {
				if (colonist.StoredJobInstance != null && colonist.StoredJobInstance.objectPrefab.layer == prefab.layer) {
					if (colonist.StoredJobInstance.tile == posTile) {
						return false;
					}
					foreach (Vector2 multiTilePosition in colonist.StoredJobInstance.objectPrefab.multiTilePositions[colonist.StoredJobInstance.rotationIndex]) {
						if (GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(colonist.StoredJobInstance.tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
							return false;
						}
					}
				}
			}
			return true;
				}
			}, {
				SelectionModifiersEnum.OmitSameLayerObjectInstances, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return !posTile.objectInstances.ContainsKey(prefab.layer) || posTile.objectInstances[prefab.layer] == null;
				}
			}, {
				SelectionModifiersEnum.Farms, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.farm != null;
				}
			}, {
				SelectionModifiersEnum.OmitFarms, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.farm == null;
				}
			}, {
				SelectionModifiersEnum.Roofs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.HasRoof();
				}
			}, {
				SelectionModifiersEnum.OmitRoofs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return !posTile.HasRoof();
				}
			}, {
				SelectionModifiersEnum.CloseToSupport, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			for (int y = -5; y < 5; y++) {
				for (int x = -5; x < 5; x++) {
					TileManager.Tile supportTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(posTile.position.x + x, posTile.position.y + y));
					if (!supportTile.buildable && !supportTile.walkable) {
						return true;
					}
				}
			}
			return false;
				}
			}, {
				SelectionModifiersEnum.ObjectsAtSameLayer, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.GetObjectInstanceAtLayer(prefab.layer) != null;
				}
			}, {
				SelectionModifiersEnum.Objects, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.GetAllObjectInstances().Count > 0;
				}
			}, {
				SelectionModifiersEnum.OmitObjects, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.GetAllObjectInstances().Count <= 0;
				}
			}, {
				SelectionModifiersEnum.OmitNonCoastWater, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			if (posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
				if (!(posTile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water) != null)) {
					return false;
				}
			}
			return true;
				}
			}, {
				SelectionModifiersEnum.OmitHoles, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Hole;
				}
			}, {
				SelectionModifiersEnum.OmitPreviousDig, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return !posTile.dugPreviously;
				}
			}, {
				SelectionModifiersEnum.BiomeSupportsSelectedPlants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
			return posTile.biome.plantChances.Keys.Intersect(variation.plants.Select(plant => plant.Key.type)).Any();
				}
			}, {
				SelectionModifiersEnum.OmitObjectInstancesOnAdditionalTiles, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					ObjectInstance objectInstance = posTile.GetObjectInstanceAtLayer(prefab.layer);
			if (objectInstance != null && objectInstance.tile != posTile) {
				return false;
			}
			return true;
				}
			}, {
				SelectionModifiersEnum.Fillable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
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
				Vector2 mousePosition = GameManager.Get<CameraManager>().camera.ScreenToWorldPoint(Input.mousePosition);
				if (Input.GetMouseButtonDown(0) && !GameManager.Get<UIManagerOld>().IsPointerOverUI()) {
					firstTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(mousePosition);
				}
				if (firstTile != null) {
					if (stopSelection) {
						stopSelection = false;
						firstTile = null;
						return;
					}
					TileManager.Tile secondTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(mousePosition);
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
								TileManager.Tile tile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(x, y));
								bool addTile = true;
								bool addOutlineTile = true;
								if (selectedPrefab.prefab.selectionModifiers.Contains(SelectionModifiersEnum.Outline)) {
									addOutlineTile = (x == smallerX || y == smallerY || x == largerX || y == largerY);
								}
								foreach (SelectionModifiersEnum selectionModifier in selectedPrefab.prefab.selectionModifiers) {
									if (selectionModifier != SelectionModifiersEnum.Outline) {
										foreach (Vector2 multiTilePosition in selectedPrefab.prefab.multiTilePositions[rotationIndex]) {
											Vector2 actualMultiTilePosition = tile.obj.transform.position + (Vector3)multiTilePosition;
											if (actualMultiTilePosition.x >= 0 && actualMultiTilePosition.x < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && actualMultiTilePosition.y >= 0 && actualMultiTilePosition.y < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize) {
												TileManager.Tile posTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(actualMultiTilePosition);
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

									GameObject selectionIndicator = MonoBehaviour.Instantiate(GameManager.Get<ResourceManager>().tilePrefab, GameManager.SharedReferences.SelectionParent, false);
									selectionIndicator.transform.position = tile.obj.transform.position + (Vector3)selectedPrefab.prefab.anchorPositionOffset[rotationIndex];
									;
									selectionIndicator.name = "Selection Indicator";
									SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
									sISR.sprite = selectedPrefab.prefab.canRotate
										? selectedPrefab.prefab.GetBitmaskSpritesForVariation(selectedPrefab.variation)[rotationIndex]
										: selectedPrefab.prefab.GetBaseSpriteForVariation(selectedPrefab.variation);
									sISR.sortingOrder = 20; // Selection Indicator Sprite
									sISR.color = ColourUtilities.GetColour(ColourUtilities.EColour.WhiteAlpha64);
									selectionIndicators.Add(selectionIndicator);
								}
							}
						}

						GameManager.Get<UIManagerOld>().GetSelectionSizePanel().Update(smallerX - maxX, smallerY - maxY, selectionArea.Count);

						if (Input.GetMouseButtonUp(0)) {
							if (selectedPrefab.prefab.jobType == "Cancel") {
								CancelJobsInSelectionArea(selectionArea);
							} else if (selectedPrefab.prefab.jobType == "IncreasePriority") {
								ChangeJobPriorityInSelectionArea(selectionArea, 1);
							} else if (selectedPrefab.prefab.jobType == "DecreasePriority") {
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
			List<JobInstance> removeJobs = new();
			foreach (JobInstance job in JobInstance.jobs) {
				if (selectionArea.Contains(job.tile)) {
					removeJobs.Add(job);
				}
			}
			foreach (JobInstance job in removeJobs) {
				if (job.objectPrefab.jobType == "CreateResource") {
					job.createResource.JobInstance = null;
				}
				job.Remove();
				JobInstance.jobs.Remove(job);
			}
			removeJobs.Clear();

			foreach (Colonist colonist in Colonist.colonists) {
				if (!((colonist.JobInstance != null && selectionArea.Contains(colonist.JobInstance.tile)) || (colonist.StoredJobInstance != null && selectionArea.Contains(colonist.StoredJobInstance.tile)))) {
					continue;
				}

				if (colonist.StoredJobInstance != null) {
					if (colonist.StoredJobInstance.containerPickups != null) {
						foreach (ContainerPickup containerPickup in colonist.StoredJobInstance.containerPickups) {
							containerPickup.container.GetInventory().ReleaseReservedResources(colonist);
						}
					}
					if (colonist.StoredJobInstance.objectPrefab.jobType == "CreateResource") {
						colonist.StoredJobInstance.createResource.JobInstance = null;
					}
					colonist.StoredJobInstance.Remove();
					colonist.StoredJobInstance = null;
				}

				if (colonist.JobInstance != null) {
					if (colonist.JobInstance.containerPickups != null) {
						foreach (ContainerPickup containerPickup in colonist.JobInstance.containerPickups) {
							containerPickup.container.GetInventory().ReleaseReservedResources(colonist);
						}
					}
					if (colonist.JobInstance.objectPrefab.jobType == "CreateResource") {
						colonist.JobInstance.createResource.JobInstance = null;
					}
					colonist.JobInstance.Remove();
					colonist.JobInstance = null;
					colonist.path.Clear();
					colonist.MoveToClosestWalkableTile(false);
				}
			}

			ColonistJob.UpdateColonistJobs();
			// GameManager.Get<UIManagerOld>().SetJobElements();
		}

		public void CancelJob(JobInstance jobInstance) {
			Colonist colonist = Colonist.colonists.Find(c => c.JobInstance == jobInstance || c.StoredJobInstance == jobInstance);
			if (colonist != null) {
				if (jobInstance.containerPickups != null) {
					foreach (ContainerPickup containerPickup in jobInstance.containerPickups) {
						containerPickup.container.GetInventory().ReleaseReservedResources(colonist);
					}
				}

				if (colonist.JobInstance == jobInstance) {
					colonist.JobInstance = null;
					colonist.path.Clear();
					colonist.MoveToClosestWalkableTile(false);
				}

				if (colonist.StoredJobInstance == jobInstance) {
					colonist.StoredJobInstance = null;
				}
			}

			if (jobInstance.objectPrefab.jobType == "CreateResource") {
				jobInstance.createResource.JobInstance = null;
			}

			if (jobInstance.activeObject != null) {
				jobInstance.activeObject.SetActiveSprite(jobInstance, false);
			}

			jobInstance.Remove();
			JobInstance.jobs.Remove(jobInstance);

			ColonistJob.UpdateColonistJobs();
			// GameManager.Get<UIManagerOld>().SetJobElements();
		}

		public void ChangeJobPriorityInSelectionArea(List<TileManager.Tile> selectionArea, int amount) {
			foreach (JobInstance job in JobInstance.jobs) {
				if (selectionArea.Contains(job.tile)) {
					job.ChangePriority(amount);
				}
			}
			foreach (Colonist colonist in Colonist.colonists) {
				if (colonist.JobInstance != null) {
					if (selectionArea.Contains(colonist.JobInstance.tile)) {
						colonist.JobInstance.ChangePriority(amount);
					}
				}
				if (colonist.StoredJobInstance != null) {
					if (selectionArea.Contains(colonist.StoredJobInstance.tile)) {
						colonist.JobInstance.ChangePriority(amount);
					}
				}
			}
			ColonistJob.UpdateColonistJobs();
			ColonistJob.UpdateAllColonistJobCosts();
			// GameManager.Get<UIManagerOld>().SetJobElements();
		}

		private static readonly Dictionary<int, ObjectPrefab.ObjectEnum> removeLayerMap = new Dictionary<int, ObjectPrefab.ObjectEnum>() {
			{ 1, ObjectPrefab.ObjectEnum.RemoveFloor },
			{ 2, ObjectPrefab.ObjectEnum.RemoveObject }
	};

		public void CreateJobsInSelectionArea(SelectedPrefab selectedPrefab, List<TileManager.Tile> selectionArea) {
			foreach (TileManager.Tile tile in selectionArea) {
				if (selectedPrefab.prefab.type == ObjectPrefab.ObjectEnum.RemoveAll) {
					foreach (ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
						if (removeLayerMap.ContainsKey(objectInstance.prefab.layer) && !JobOfPrefabTypeExistsAtTile(removeLayerMap[objectInstance.prefab.layer], objectInstance.tile)) {
							ObjectPrefab selectedRemovePrefab = ObjectPrefab.GetObjectPrefabByEnum(removeLayerMap[objectInstance.prefab.layer]);
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
								CreateJob(
									new JobInstance(
									JobPrefab.GetJobPrefabByName(selectedRemovePrefab.jobType),
									objectInstance.tile,
									selectedRemovePrefab,
									null,
									rotationIndex
								));
								objectInstance.SetActive(false);
							}
						}
					}
				} else {
					CreateJob(
						new JobInstance(
						JobPrefab.GetJobPrefabByName(selectedPrefab.prefab.jobType),
						tile,
						selectedPrefab.prefab,
						selectedPrefab.variation,
						rotationIndex
					));
					foreach (ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
						objectInstance.SetActive(false);
					}
				}
			}
		}

		public void CreateJob(JobInstance newJobInstance) {
			JobInstance.jobs.Add(newJobInstance);
			changedJobList = true;
		}

		public void AddExistingJob(JobInstance existingJobInstance) {
			JobInstance.jobs.Add(existingJobInstance);
			changedJobList = true;
		}

		public List<ContainerPickup> CalculateColonistPickupContainers(Colonist colonist, List<ResourceAmount> resourcesToPickup) {
			List<ContainerPickup> containersToPickupFrom = new List<ContainerPickup>();
			List<Container> sortedContainersByDistance = Container.GetContainersInRegion(colonist.overTile.region).OrderBy(container => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, container.tile.regionBlock, true, true, false)).ToList();
			if (sortedContainersByDistance.Count > 0) {
				foreach (Container container in sortedContainersByDistance) {
					List<ResourceAmount> resourcesToPickupAtContainer = new();
					foreach (ResourceAmount resourceAmount in container.GetInventory().resources.Where(ra => resourcesToPickup.Find(pickupResource => pickupResource.Resource == ra.Resource) != null)) {
						ResourceAmount pickupResource = resourcesToPickup.Find(pR => pR.Resource == resourceAmount.Resource);
						if (resourceAmount.Amount >= pickupResource.Amount) {
							resourcesToPickupAtContainer.Add(new ResourceAmount(pickupResource.Resource, pickupResource.Amount));
							resourcesToPickup.Remove(pickupResource);
						} else if (resourceAmount.Amount > 0 && resourceAmount.Amount < pickupResource.Amount) {
							resourcesToPickupAtContainer.Add(new ResourceAmount(pickupResource.Resource, resourceAmount.Amount));
							pickupResource.Amount -= resourceAmount.Amount;
							if (pickupResource.Amount <= 0) {
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

		public KeyValuePair<bool, List<List<ResourceAmount>>> CalculateColonistResourcesToPickup(Colonist colonist, List<ResourceAmount> resourcesToFind) {
			bool colonistHasAllResources = true;
			List<ResourceAmount> resourcesColonistHas = new();
			List<ResourceAmount> resourcesToPickup = new();
			foreach (ResourceAmount resourceAmount in resourcesToFind) {
				ResourceAmount colonistResourceAmount = colonist.GetInventory().resources.Find(resource => resource.Resource == resourceAmount.Resource);
				if (colonistResourceAmount != null) {
					if (colonistResourceAmount.Amount >= resourceAmount.Amount) {
						resourcesColonistHas.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
					} else if (colonistResourceAmount.Amount > 0 && colonistResourceAmount.Amount < resourceAmount.Amount) {
						colonistHasAllResources = false;
						resourcesColonistHas.Add(new ResourceAmount(resourceAmount.Resource, colonistResourceAmount.Amount));
						resourcesToPickup.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount - colonistResourceAmount.Amount));
					} else {
						colonistHasAllResources = false;
						resourcesToPickup.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
					}
				} else {
					colonistHasAllResources = false;
					resourcesToPickup.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
				}
			}
			return new KeyValuePair<bool, List<List<ResourceAmount>>>(colonistHasAllResources, new List<List<ResourceAmount>>() { resourcesToPickup.Count > 0 ? resourcesToPickup : null, resourcesColonistHas.Count > 0 ? resourcesColonistHas : null });
		}

		public static float CalculateJobCost(Colonist colonist, JobInstance jobInstance, List<ContainerPickup> containerPickups) {
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
				cost += PathManager.RegionBlockDistance(jobInstance.tile.regionBlock, containerPickups[containerPickups.Count - 1].container.tile.regionBlock, true, true, true);
			} else {
				cost += PathManager.RegionBlockDistance(jobInstance.tile.regionBlock, colonist.overTile.regionBlock, true, true, true);
			}
			SkillInstance skill = colonist.GetSkillFromJobType(jobInstance.objectPrefab.jobType);
			if (skill != null) {
				cost -= skill.CalculateTotalSkillLevel() * 5f;
			}
			return cost;
		}

		public static List<JobInstance> GetSortedJobs(Colonist colonist) {
			return JobInstance.jobs
				.Where(job =>
					// Colonist is in the same region as the job
					(job.tile.region == colonist.overTile.region)
					// OR Colonist is NOT in the same region as the job BUT the job is in a tile neighbouring the colonist's region (e.g. for mining jobs)
					|| (job.tile.region != colonist.overTile.region && job.tile.horizontalSurroundingTiles.Find(nTile => nTile != null && nTile.region == colonist.overTile.region) != null))
				.Where(job =>
					// Job is not associated with ANY professions
					ProfessionPrefab.professionPrefabs.Find(p => p.jobs.Contains(job.prefab.name)) == null
					// OR Remove jobs that the colonist either CAN'T do, or WON'T do due to the player disabling it for them (i.e. priority == 0)
					|| colonist.professions.Find(p => p.prefab.jobs.Contains(job.objectPrefab.jobType) && p.GetPriority() != 0) != null)
				.OrderBy(job => colonist.professions.Find(p => p.prefab.jobs.Contains(job.objectPrefab.jobType)).GetPriority())
				.ThenBy(job => CalculateJobCost(colonist, job, null))
				.ToList();
		}

		public void GiveJobsToColonists() {
			bool gaveJob = false;
			Dictionary<Colonist, ColonistJob> jobsGiven = new Dictionary<Colonist, ColonistJob>();

			foreach (Colonist colonist in Colonist.colonists) {
				if (colonist.JobInstance == null && !colonist.playerMoved && colonist.Backlog.Count > 0) {
					ColonistJob backlogJob = new(colonist, colonist.Backlog[0], colonist.Backlog[0].resourcesColonistHas, colonist.Backlog[0].containerPickups);
					gaveJob = true;
					jobsGiven.Add(colonist, backlogJob);
					colonist.Backlog.Remove(backlogJob.JobInstance);
				}
			}

			foreach (KeyValuePair<Colonist, List<ColonistJob>> colonistKVP in ColonistJob.colonistJobs) {
				Colonist colonist = colonistKVP.Key;
				List<ColonistJob> colonistJobsList = colonistKVP.Value;
				if (colonist.JobInstance == null && !colonist.playerMoved && !jobsGiven.ContainsKey(colonist)) {
					for (int i = 0; i < colonistJobsList.Count; i++) {
						ColonistJob colonistJob = colonistJobsList[i];
						bool bestColonistForJob = true;
						foreach (KeyValuePair<Colonist, List<ColonistJob>> otherColonistKVP in ColonistJob.colonistJobs) {
							Colonist otherColonist = otherColonistKVP.Key;
							if (colonist != otherColonist && otherColonist.JobInstance == null) {
								ColonistJob otherColonistJob = otherColonistKVP.Value.Find(job => job.JobInstance == colonistJob.JobInstance);
								if (otherColonistJob != null && otherColonistJob.cost < colonistJob.cost) {
									bestColonistForJob = false;
									break;
								}
							}
						}
						if (bestColonistForJob) {
							gaveJob = true;
							jobsGiven.Add(colonist, colonistJob);
							JobInstance.jobs.Remove(colonistJob.JobInstance);
							foreach (KeyValuePair<Colonist, List<ColonistJob>> removeKVP in ColonistJob.colonistJobs) {
								ColonistJob jobToRemove = removeKVP.Value.Find(cJob => cJob.JobInstance == colonistJob.JobInstance);
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
			foreach (KeyValuePair<Colonist, ColonistJob> jobGiven in jobsGiven) {
				jobGiven.Key.SetJob(jobGiven.Value);
			}
			if (gaveJob) {
				// GameManager.Get<UIManagerOld>().SetJobElements();
				ColonistJob.UpdateColonistJobs();
			}
		}

		public bool JobOfTypeExistsAtTile(string jobType, TileManager.Tile tile) {
			if (JobInstance.jobs.Find(job => job.prefab.name == jobType && job.tile == tile) != null) {
				return true;
			}
			if (Colonist.colonists.Find(colonist => colonist.JobInstance != null && colonist.JobInstance.objectPrefab.jobType == jobType && colonist.JobInstance.tile == tile) != null) {
				return true;
			}
			if (Colonist.colonists.Find(colonist => colonist.StoredJobInstance != null && colonist.StoredJobInstance.objectPrefab.jobType == jobType && colonist.StoredJobInstance.tile == tile) != null) {
				return true;
			}
			return false;
		}

		public bool JobOfPrefabTypeExistsAtTile(ObjectPrefab.ObjectEnum prefabType, TileManager.Tile tile) {
			if (JobInstance.jobs.Find(job => job.objectPrefab.type == prefabType && job.tile == tile) != null) {
				return true;
			}
			if (Colonist.colonists.Find(colonist => colonist.JobInstance != null && colonist.JobInstance.objectPrefab.type == prefabType && colonist.JobInstance.tile == tile) != null) {
				return true;
			}
			if (Colonist.colonists.Find(colonist => colonist.StoredJobInstance != null && colonist.StoredJobInstance.objectPrefab.type == prefabType && colonist.StoredJobInstance.tile == tile) != null) {
				return true;
			}
			return false;
		}
	}
}