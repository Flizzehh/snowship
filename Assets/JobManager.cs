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

	void Awake() {
		tileM = GetComponent<TileManager>();
		colonistM = GetComponent<ColonistManager>();
		cameraM = GetComponent<CameraManager>();
		timeM = GetComponent<TimeManager>();
		uiM = GetComponent<UIManager>();
		pathM = GetComponent<PathManager>();

		InitializeSelectionModifierFunctions();

		selectedPrefabPreview = GameObject.Find("SelectedPrefabPreview");
		selectedPrefabPreview.GetComponent<SpriteRenderer>().sortingOrder = 50;
	}

	public List<Job> jobs = new List<Job>();

	public class Job {
		public TileManager.Tile tile;
		public ResourceManager.TileObjectPrefab prefab;
		public ColonistManager.Colonist colonist;

		public GameObject jobPreview;

		public bool accessible;

		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public Job(TileManager.Tile tile,ResourceManager.TileObjectPrefab prefab,ColonistManager colonistM) {
			this.tile = tile;
			this.prefab = prefab;

			jobPreview = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
			jobPreview.name = "JobPreview: " + prefab.name + " at " + tile.obj.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (prefab.baseSprite != null) {
				jPSR.sprite = prefab.baseSprite;
				jPSR.sortingOrder = 2 + prefab.layer; // Job Preview Sprite
			}
			jPSR.color = new Color(1f,1f,1f,0.25f);

			jobProgress = prefab.timeToBuild;
			colonistBuildTime = prefab.timeToBuild;

			accessible = false;
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.overTile.region == tile.region) {
					accessible = true;
					break;
				}
			}
		}

		public void SetColonist(ColonistManager.Colonist colonist) {
			this.colonist = colonist;
		}
	}

	ResourceManager.TileObjectPrefab selectedPrefab;

	public void SetSelectedPrefab(ResourceManager.TileObjectPrefab newSelectedPrefab) {
		if (newSelectedPrefab != selectedPrefab) {
			if (newSelectedPrefab != null) {
				selectedPrefab = newSelectedPrefab;
			} else {
				selectedPrefab = null;
			}
		}
	}

	private GameObject selectedPrefabPreview;
	private GameObject selectionSizePanel;
	public void SelectedPrefabPreview() {
		Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
		TileManager.Tile tile = tileM.GetTileFromPosition(mousePosition);
		selectedPrefabPreview.transform.position = tile.obj.transform.position;
	}

	void Update() {
		GetJobSelectionArea();
		if (timeM.timeModifier > 0) {
			GiveJobsToColonists();
		}
		if (selectedPrefab != null) {
			if (enableSelectionPreview) {
				if (!selectedPrefabPreview.activeSelf) {
					selectedPrefabPreview.SetActive(true);
					selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
					uiM.SelectionSizeCanvasSetActive(false);
				}
				SelectedPrefabPreview();
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
		/*
		if (enableSelectionPreview && selectedPrefab != null) {
			print("Not selecting area");
			if (!selectedPrefabPreview.activeSelf) {
				selectedPrefabPreview.SetActive(false);
				selectedPrefabPreview.GetComponent<SpriteRenderer>().sprite = selectedPrefab.baseSprite;
				selectionSizePanel.SetActive(true);
			}
			SelectedPrefabPreview();
		} else if (selectedPrefabPreview.activeSelf) {
			print("Selecting area");
			selectedPrefabPreview.SetActive(true);
			selectionSizePanel.SetActive(false);
		} else {

		}
		*/
	}

	public enum JobTypesEnum { Build, Remove, Mine, PlantFarm, HarvestFarm };

	public enum SelectionModifiersEnum { Outline, Walkable, OmitWalkable, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
		Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances
	};
	Dictionary<SelectionModifiersEnum,System.Action<TileManager.Tile,List<TileManager.Tile>>> selectionModifierFunctions = new Dictionary<SelectionModifiersEnum,System.Action<TileManager.Tile,List<TileManager.Tile>>>();

	void InitializeSelectionModifierFunctions() {
		selectionModifierFunctions.Add(SelectionModifiersEnum.Walkable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tile.walkable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitWalkable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.walkable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Buildable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tile.tileType.buildable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitBuildable,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.tileType.buildable) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.StoneTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitStoneTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.AllWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitAllWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.LiquidWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (!tileM.GetLiquidWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitLiquidWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetLiquidWaterEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitNonStoneAndWaterTypes,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tileM.GetWaterEquivalentTileTypes().Contains(tile.tileType.type) || tileM.GetStoneEquivalentTileTypes().Contains(tile.tileType.type)) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.Plants,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.plant == null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitPlants,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.plant != null) { removeTiles.Add(tile); }
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerJobs,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (jobs.Find(job => job.prefab.layer == selectedPrefab.layer && job.tile == tile) != null) {
				removeTiles.Add(tile);
				return;
			}
			if (colonistM.colonists.Find(colonist => colonist.job != null && colonist.job.prefab.layer == selectedPrefab.layer && colonist.job.tile == tile) != null) {
				removeTiles.Add(tile);
				return;
			}
		});
		selectionModifierFunctions.Add(SelectionModifiersEnum.OmitSameLayerObjectInstances,delegate (TileManager.Tile tile,List<TileManager.Tile> removeTiles) {
			if (tile.objectInstances.ContainsKey(selectedPrefab.layer) && tile.objectInstances[selectedPrefab.layer] != null) { removeTiles.Add(tile); }
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

		if (selectedPrefab != null) {
			Vector2 mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				firstTile = tileM.GetTileFromPosition(mousePosition);
			}
			if (firstTile != null) {
				if (stopSelection) {
					stopSelection = false;
					firstTile = null;
					return;
				}
				TileManager.Tile secondTile = tileM.GetTileFromPosition(mousePosition);
				if (secondTile != null) {

					enableSelectionPreview = false;

					float smallerY = Mathf.Min(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float largerY = Mathf.Max(firstTile.obj.transform.position.y,secondTile.obj.transform.position.y);
					float smallerX = Mathf.Min(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);
					float largerX = Mathf.Max(firstTile.obj.transform.position.x,secondTile.obj.transform.position.x);

					List<TileManager.Tile> selectionArea = new List<TileManager.Tile>();

					float maxY = ((largerY - smallerY) + smallerY + 1);
					float maxX = ((largerX - smallerX) + smallerX + 1);

					uiM.UpdateSelectionSizePanel(smallerX - maxX,smallerY - maxY,selectedPrefab);

					for (float y = smallerY; y < maxY; y++) {
						for (float x = smallerX; x < maxX; x++) {
							TileManager.Tile tile = tileM.GetTileFromPosition(new Vector2(x,y));
							if (selectedPrefab.selectionModifiers.Contains(SelectionModifiersEnum.Outline)) {
								if (x == smallerX || y == smallerY || x == ((largerX - smallerX) + smallerX) || y == ((largerY - smallerY) + smallerY)) {
									selectionArea.Add(tile);
								}
							} else {
								selectionArea.Add(tile);
							}
						}
					}

					foreach (SelectionModifiersEnum selectionModifier in selectedPrefab.selectionModifiers) {
						List<TileManager.Tile> removeTiles = new List<TileManager.Tile>();
						if (selectionModifier == SelectionModifiersEnum.Outline) {
							continue;
						} else {
							foreach (TileManager.Tile tile in selectionArea) {
								selectionModifierFunctions[selectionModifier].Invoke(tile,removeTiles);
							}
						}
						RemoveTilesFromList(selectionArea,removeTiles);
						removeTiles.Clear();
					}

					foreach (TileManager.Tile tile in selectionArea) {
						GameObject selectionIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),tile.obj.transform,false);
						SpriteRenderer sISR = selectionIndicator.GetComponent<SpriteRenderer>();
						sISR.sprite = Resources.Load<Sprite>(@"UI/selectionIndicator");
						//sISR.color = new Color(241f,196f,15f,255f) / 255f; // Yellow
						//sISR.color = new Color(231f,76f,60f,255f) / 255f; // Red
						sISR.sortingOrder = 20; // Selection Indicator Sprite
						selectionIndicators.Add(selectionIndicator);
					}

					if (Input.GetMouseButtonUp(0)) {
						CreateJobsInSelectionArea(selectedPrefab,selectionArea);
						firstTile = null;
					}
				}
			}
		}
	}

	public void RemoveTilesFromList(List<TileManager.Tile> listToModify,List<TileManager.Tile> removeList) {
		foreach (TileManager.Tile tile in removeList) {
			listToModify.Remove(tile);
		}
		removeList.Clear();
	}

	public void CreateJobsInSelectionArea(ResourceManager.TileObjectPrefab prefab, List<TileManager.Tile> selectionArea) {
		foreach (TileManager.Tile tile in selectionArea) {
			CreateJob(new Job(tile,prefab,colonistM));
		}
	}

	public void CreateJob(Job newJob) {
		jobs.Add(newJob);
		uiM.SetJobElements();
	}

	public void AddExistingJob(Job existingJob) {
		jobs.Add(existingJob);
		uiM.SetJobElements();
	}

	public void GiveJobsToColonists() {

		if (jobs.Count > 0) {
			bool updateJobListUI = false;
			for (int i = 0; i < jobs.Count; i++) {
				Job job = jobs[i];
				List<ColonistManager.Colonist> availableColonists = colonistM.colonists.Where(colonist => colonist.job == null && job.tile.region == colonist.overTile.region).ToList();
				if (availableColonists.Count > 0) {
					List<ColonistManager.Colonist> sortedColonists = availableColonists.OrderBy(colonist => pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true)).ToList();
					foreach (ColonistManager.Colonist colonist in sortedColonists) {
						colonist.SetJob(job);
						jobs.RemoveAt(i);
						i -= 1;//(i - 1 >= 0 ? i - 1 : 0);\
						updateJobListUI = true;
						break;
					}
				}
			}
			if (updateJobListUI) {
				uiM.SetJobElements();
			}
		}
		/*
		if (availableColonists.Count > 0) {
			bool gaveJob = false;
			foreach (ColonistManager.Colonist colonist in availableColonists) {
				List<Job> sortedJobs = jobs.Where(job => (job.tile.surroundingTiles.Find(tile => tile != null && tile.region == colonist.overTile.region) != null) || (job.tile.region == colonist.overTile.region)).OrderBy(job => pathM.RegionBlockDistance(job.tile.regionBlock,colonist.overTile.regionBlock,true,true)).ToList();
				if (sortedJobs.Count > 0) {
					if (availableColonists.OrderBy(c => pathM.RegionBlockDistance(c.overTile.regionBlock,sortedJobs[0].tile.regionBlock,true,true)).ToList()[0] == colonist) {
						colonist.SetJob(sortedJobs[0]);
						jobs.Remove(sortedJobs[0]);
						gaveJob = true;
					} else {
						continue;
					}
				}
			}
			if (gaveJob) {
				uiM.SetJobList();
			}
		}
		*/
	}
}
