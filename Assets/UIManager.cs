﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class UIManager : MonoBehaviour {

	public string SplitByCapitals(string combinedString) {
		var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])",
				 RegexOptions.IgnorePatternWhitespace);
		return r.Replace(combinedString," ");
	}

	private TileManager tileM;
	private JobManager jobM;
	private ResourceManager resourceM;
	private CameraManager cameraM;
	private ColonistManager colonistM;
	private TimeManager timeM;

	public int mapSize;

	private GameObject mainMenu;

	private Vector2 mousePosition;
	private TileManager.Tile mouseOverTile;

	private GameObject tileInformation;

	private GameObject colonistListToggleButton;
	private GameObject colonistList;
	private GameObject jobListToggleButton;
	private GameObject jobList;

	private GameObject selectedColonistInformationPanel;

	private GameObject dateTimeInformationPanel;

	private GameObject selectionSizeCanvas;
	private GameObject selectionSizePanel;

	private GameObject selectedContainerIndicator;
	private GameObject selectedContainerInventoryPanel;

	private GameObject professionMenuButton;
	private GameObject professionsList;

	private GameObject objectsMenuButton;
	private GameObject objectPrefabsList;

	private GameObject cancelButton;

	void Awake() {
		tileM = GetComponent<TileManager>();
		jobM = GetComponent<JobManager>();
		resourceM = GetComponent<ResourceManager>();
		cameraM = GetComponent<CameraManager>();
		colonistM = GetComponent<ColonistManager>();
		timeM = GetComponent<TimeManager>();

		GameObject.Find("PlayButton").GetComponent<Button>().onClick.AddListener(delegate { PlayButton(); });

		GameObject.Find("MapSize-Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate { UpdateMapSizeText(); });
		GameObject.Find("MapSize-Slider").GetComponent<Slider>().value = 2;

		mainMenu = GameObject.Find("MainMenu");

		tileInformation = GameObject.Find("TileInformation-Panel");

		colonistListToggleButton = GameObject.Find("ColonistListToggle-Button");
		colonistList = GameObject.Find("ColonistList-ScrollPanel");
		colonistListToggleButton.GetComponent<Button>().onClick.AddListener(delegate { colonistList.SetActive(!colonistList.activeSelf); });
		jobListToggleButton = GameObject.Find("JobListToggle-Button");
		jobList = GameObject.Find("JobList-ScrollPanel");
		jobListToggleButton.GetComponent<Button>().onClick.AddListener(delegate { jobList.SetActive(!jobList.activeSelf); });

		selectedColonistInformationPanel = GameObject.Find("SelectedColonistInfo-Panel");

		dateTimeInformationPanel = GameObject.Find("DateTimeInformation-Panel");

		selectionSizeCanvas = GameObject.Find("SelectionSize-Canvas");
		selectionSizeCanvas.GetComponent<Canvas>().sortingOrder = 100;
		selectionSizePanel = selectionSizeCanvas.transform.Find("SelectionSize-Panel/Content-Panel").gameObject;

		selectedContainerInventoryPanel = GameObject.Find("SelectedContainerInventory-Panel");

		professionMenuButton = GameObject.Find("ProfessionsMenu-Button");
		professionsList = professionMenuButton.transform.Find("ProfessionsList-Panel").gameObject;
		professionMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SetProfessionsList(); });

		objectsMenuButton = GameObject.Find("ObjectsMenu-Button");
		objectPrefabsList = objectsMenuButton.transform.Find("ObjectPrefabsList-ScrollPanel").gameObject;
		objectsMenuButton.GetComponent<Button>().onClick.AddListener(delegate {
			ToggleObjectPrefabsList();
		});
		objectPrefabsList.SetActive(false);

		cancelButton = GameObject.Find("Cancel-Button");
		cancelButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(resourceM.GetTileObjectPrefabByEnum(ResourceManager.TileObjectPrefabsEnum.Cancel)); });

		InitializeTileInformation();
		InitializeSelectedContainerIndicator();
	}

	public ResourceManager.Container selectedContainer;
	void Update() {
		if (tileM.generated) {
			mousePosition = cameraM.cameraComponent.ScreenToWorldPoint(Input.mousePosition);
			TileManager.Tile newMouseOverTile = tileM.GetTileFromPosition(mousePosition);
			if (newMouseOverTile != mouseOverTile) {
				mouseOverTile = newMouseOverTile;
				UpdateTileInformation();
			}
			if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) {
				if (jobM.firstTile != null) {
					jobM.StopSelection();
				} else {
					jobM.SetSelectedPrefab(null);
				}
			}
			if (colonistM.selectedColonist != null) {
				UpdateSelectedColonistInformation();
			}
			if (jobElements.Count > 0) {
				UpdateJobElements();
			}
			if (colonistElements.Count > 0) {
				UpdateColonistElements();
			}
			if (selectedContainer != null) {
				if (Input.GetMouseButtonDown(1)) {
					selectedContainer = null;
					SetSelectedContainerInfo();
				}
				UpdateSelectedContainerInfo();
			}
			if (Input.GetMouseButtonDown(0)) {
				ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject.tile == newMouseOverTile);
				if (container != null) {
					selectedContainer = container;
					SetSelectedContainerInfo();
				}
			}
			if (professionsList.activeSelf) {
				foreach (ProfessionElement professionElement in professionElements) {
					professionElement.Update();
				}
			}
		}
	}

	public void PlayButton() {
		string mapSeedString = GameObject.Find("MapSeedInput-Text").GetComponent<Text>().text;
		if (string.IsNullOrEmpty(mapSeedString)) {
			mapSeedString = UnityEngine.Random.Range(0,int.MaxValue).ToString();
		}
		int mapSeed = 0;
		if (!int.TryParse(mapSeedString,out mapSeed)) {
			foreach (char c in mapSeedString) {
				mapSeed += c;
			}
		}
		tileM.Initialize(mapSize,mapSeed);
		mainMenu.SetActive(false);
	}

	public void UpdateMapSizeText() {
		mapSize = Mathf.RoundToInt(GameObject.Find("MapSize-Slider").GetComponent<Slider>().value * 50);
		GameObject.Find("MapSizeValue-Text").GetComponent<Text>().text = mapSize.ToString();
	}

	/*	Menu Structure:
	 *		Build Menu Button -> Build Menu Panel
	 *			Build Menu Group Button -> Group Panel
	 *				Build Menu Subgroup Button -> Subgroup Panel
	 *					Build Menu Prefab Button
	 *		Command Menu Button -> Command Menu Panel
	 *			Command Menu Subgroup Button -> Panel
	 *				Command Menu Prefab Button
	*/

	public void CreateMenus() {
		Dictionary<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>> menus = CreateBuildMenuButtons();

		foreach (KeyValuePair<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>> menuKVP in menus) {
			GameObject menuKVPPanel = menuKVP.Key.transform.Find("Panel").gameObject;
			menuKVP.Key.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>> otherMenuKVP in menus) {
					if (menuKVP.Key != otherMenuKVP.Key) {
						otherMenuKVP.Key.transform.Find("Panel").gameObject.SetActive(false);
						foreach (KeyValuePair<GameObject,Dictionary<GameObject,List<GameObject>>> groupKVP in otherMenuKVP.Value) {
							groupKVP.Key.SetActive(false);
							foreach (KeyValuePair<GameObject,List<GameObject>> subgroupKVP in groupKVP.Value) {
								subgroupKVP.Key.SetActive(false);
							}
						}
					}
				}
			});
			menuKVP.Key.GetComponent<Button>().onClick.AddListener(delegate { menuKVPPanel.SetActive(!menuKVPPanel.activeSelf); });
		}

		foreach (KeyValuePair<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>> menuKVP in menus) {

			menuKVP.Key.transform.Find("Panel").gameObject.SetActive(false);
			foreach (KeyValuePair<GameObject,Dictionary<GameObject,List<GameObject>>> groupKVP in menuKVP.Value) {
				groupKVP.Key.SetActive(false);
				foreach (KeyValuePair<GameObject,List<GameObject>> subgroupKVP in groupKVP.Value) {
					subgroupKVP.Key.SetActive(false);
				}
			}
		}
	}

	public Dictionary<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>> CreateBuildMenuButtons() {

		Dictionary<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>> menus = new Dictionary<GameObject,Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>>();

		GameObject buildMenuButton = GameObject.Find("BuildMenu-Button");
		GameObject buildMenuPanel = buildMenuButton.transform.Find("Panel").gameObject;

		Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>> groupPanels = new Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>();

		foreach (ResourceManager.TileObjectPrefabGroup group in resourceM.tileObjectPrefabGroups) {
			if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.None) {
				continue;
			} else if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.Command) {
				menus.Add(GameObject.Find("CommandMenu-Button"),CreateAdditionalMenuButtons(GameObject.Find("CommandMenu-Button"),group));
				continue;
			} else if (group.type == ResourceManager.TileObjectPrefabGroupsEnum.Farm) {
				menus.Add(GameObject.Find("FarmMenu-Button"),CreateAdditionalMenuButtons(GameObject.Find("FarmMenu-Button"),group));
				continue;
			}

			GameObject groupButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"),buildMenuPanel.transform,false);
			groupButton.transform.Find("Text").GetComponent<Text>().text = SplitByCapitals(group.name);
			GameObject groupPanel = groupButton.transform.Find("Panel").gameObject;
			groupPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(100,21);

			Dictionary<GameObject,List<GameObject>> subgroupPanels = new Dictionary<GameObject,List<GameObject>>();
			foreach (ResourceManager.TileObjectPrefabSubGroup subgroup in group.tileObjectPrefabSubGroups) {
				GameObject subgroupButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"),groupPanel.transform,false);
				subgroupButton.transform.Find("Text").GetComponent<Text>().text = SplitByCapitals(subgroup.name);
				GameObject subgroupPanel = subgroupButton.transform.Find("Panel").gameObject;

				List<GameObject> prefabButtons = new List<GameObject>();
				foreach (ResourceManager.TileObjectPrefab prefab in subgroup.tileObjectPrefabs) {
					GameObject prefabButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"),subgroupPanel.transform,false);
					prefabButton.transform.Find("Text").GetComponent<Text>().text = prefab.name;
					if (prefab.baseSprite != null) {
						prefabButton.transform.Find("Image").GetComponent<Image>().sprite = prefab.baseSprite;
					}
					prefabButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(prefab); });
					prefabButtons.Add(prefabButton);
				}
				subgroupPanels.Add(subgroupPanel,prefabButtons);

				subgroupButton.GetComponent<Button>().onClick.AddListener(delegate {
					foreach (KeyValuePair<GameObject,List<GameObject>> subgroupKVP in subgroupPanels) {
						if (subgroupKVP.Key != subgroupPanel) {
							subgroupKVP.Key.SetActive(false);
						}
					}
				});
				subgroupButton.GetComponent<Button>().onClick.AddListener(delegate { subgroupPanel.SetActive(!subgroupPanel.activeSelf); });
			}
			groupPanels.Add(groupPanel,subgroupPanels);

			groupButton.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject,Dictionary<GameObject,List<GameObject>>> groupKVP in groupPanels) {
					if (groupKVP.Key != groupPanel) {
						groupKVP.Key.SetActive(false);
					}
					foreach (KeyValuePair<GameObject,List<GameObject>> subgroupKVP in groupKVP.Value) {
						subgroupKVP.Key.SetActive(false);
					}
				}
			});
			groupButton.GetComponent<Button>().onClick.AddListener(delegate { groupPanel.SetActive(!groupPanel.activeSelf); });
		}

		menus.Add(buildMenuButton,groupPanels);

		return menus;
	}

	public Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>> CreateAdditionalMenuButtons(GameObject parentButton, ResourceManager.TileObjectPrefabGroup group) {

		Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>> groupPanels = new Dictionary<GameObject,Dictionary<GameObject,List<GameObject>>>();

		GameObject parentMenuButton = parentButton;
		GameObject parentMenuPanel = parentMenuButton.transform.Find("Panel").gameObject;

		Dictionary<GameObject,List<GameObject>> subgroupPanels = new Dictionary<GameObject,List<GameObject>>();
		foreach (ResourceManager.TileObjectPrefabSubGroup subgroup in group.tileObjectPrefabSubGroups) {
			GameObject subgroupButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildItem-Button-Prefab"),parentMenuPanel.transform,false);
			subgroupButton.transform.Find("Text").GetComponent<Text>().text = SplitByCapitals(subgroup.name);
			GameObject subgroupPanel = subgroupButton.transform.Find("Panel").gameObject;

			List<GameObject> prefabButtons = new List<GameObject>();
			foreach (ResourceManager.TileObjectPrefab prefab in subgroup.tileObjectPrefabs) {
				GameObject prefabButton = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/BuildObject-Button-Prefab"),subgroupPanel.transform,false);
				prefabButton.transform.Find("Text").GetComponent<Text>().text = prefab.name;
				if (prefab.baseSprite != null) {
					prefabButton.transform.Find("Image").GetComponent<Image>().sprite = prefab.baseSprite;
				}
				prefabButton.GetComponent<Button>().onClick.AddListener(delegate { jobM.SetSelectedPrefab(prefab); });
				prefabButtons.Add(prefabButton);
			}
			subgroupPanels.Add(subgroupPanel,prefabButtons);

			subgroupButton.GetComponent<Button>().onClick.AddListener(delegate {
				foreach (KeyValuePair<GameObject,List<GameObject>> subgroupKVP in subgroupPanels) {
					if (subgroupKVP.Key != subgroupPanel) {
						subgroupKVP.Key.SetActive(false);
					}
				}
			});
			subgroupButton.GetComponent<Button>().onClick.AddListener(delegate { subgroupPanel.SetActive(!subgroupPanel.activeSelf); });
		}
		groupPanels.Add(parentMenuPanel,subgroupPanels);

		return groupPanels;
	}

	public void InitializeTileInformation() {
		plantObjectElements.Add(Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"),tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage"),false));
		plantObjectElements.Add(Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"),tileInformation.transform,false));
	}

	private List<GameObject> tileObjectElements = new List<GameObject>();
	private List<GameObject> plantObjectElements = new List<GameObject>();
	public void UpdateTileInformation() {
		if (mouseOverTile != null) {
			foreach (GameObject tileObjectElement in tileObjectElements) {
				Destroy(tileObjectElement);
			}
			tileObjectElements.Clear();

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage").GetComponent<Image>().sprite = mouseOverTile.obj.GetComponent<SpriteRenderer>().sprite;

			if (mouseOverTile.plant != null) {
				foreach (GameObject plantObjectElement in plantObjectElements) {
					plantObjectElement.SetActive(true);
				}
				plantObjectElements[0].GetComponent<Image>().sprite = mouseOverTile.plant.obj.GetComponent<SpriteRenderer>().sprite;
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "Plant";
				plantObjectElements[1].transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = mouseOverTile.plant.group.name;
			} else {
				foreach (GameObject plantObjectElement in plantObjectElements) {
					plantObjectElement.SetActive(false);
				}
			}
			if (mouseOverTile.GetAllObjectInstances().Count > 0) {
				foreach (ResourceManager.TileObjectInstance tileObject in mouseOverTile.GetAllObjectInstances().OrderBy(o => o.prefab.layer).ToList()) {
					GameObject tileLayerSpriteObject = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage"),tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInfoElement-TileImage"),false);
					tileLayerSpriteObject.GetComponent<Image>().sprite = tileObject.obj.GetComponent<SpriteRenderer>().sprite;
					tileObjectElements.Add(tileLayerSpriteObject);

					GameObject tileObjectDataObject = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel"),tileInformation.transform,false);
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Label").GetComponent<Text>().text = "L" + tileObject.prefab.layer;
					tileObjectDataObject.transform.Find("TileInfo-ObjectData-Value").GetComponent<Text>().text = tileObject.prefab.name;
					tileObjectElements.Add(tileObjectDataObject);
				}
			}

			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Position").GetComponent<Text>().text = "<b>Position</b>\n(" + Mathf.FloorToInt(mouseOverTile.obj.transform.position.x) + ", " + Mathf.FloorToInt(mouseOverTile.obj.transform.position.y) + ")";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Biome").GetComponent<Text>().text = "<b><size=14>Biome</size></b>\n<size=12>" + mouseOverTile.biome.name + "</size>";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Temperature").GetComponent<Text>().text = "<b>Temperature</b>\n" + Mathf.RoundToInt(mouseOverTile.temperature) + "°C";
			tileInformation.transform.Find("TileInformation-GeneralInfo-Panel/TileInformation-Precipitation").GetComponent<Text>().text = "<b>Precipitation</b>\n" + Mathf.RoundToInt(mouseOverTile.precipitation * 100f) + "%";

			if (!tileInformation.activeSelf) {
				tileInformation.SetActive(true);
			}
		} else {
			tileInformation.SetActive(false);
		}
	}

	public class SkillElement {
		public ColonistManager.Colonist colonist;
		public ColonistManager.SkillInstance skill;
		public GameObject obj;

		public SkillElement(ColonistManager.Colonist colonist,ColonistManager.SkillInstance skill,Transform parent) {
			this.colonist = colonist;
			this.skill = skill;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/SkillInfoElement-Panel"),parent,false);

			obj.transform.Find("Name").GetComponent<Text>().text = skill.prefab.name;
			// ADD SKILL IMAGE

			Update();
		}

		public void Update() {
			obj.transform.Find("Level").GetComponent<Text>().text = "Level " + skill.level + " (+" + Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f) + "%)";
			obj.transform.Find("Experience-Slider").GetComponent<Slider>().value = Mathf.RoundToInt((skill.currentExperience / skill.nextLevelExperience) * 100f);
			if (skill.level >= 10) {
				obj.transform.Find("Experience-Slider/Fill Area/Fill").GetComponent<Image>().color = new Color(192f,57f,43f,255f) / 255f;
				obj.transform.Find("Experience-Slider/Handle Slide Area/Handle").GetComponent<Image>().color = new Color(231f,76f,60f,255f) / 255f;
			}
		}
	}

	public class InventoryElement {
		public ColonistManager.Colonist colonist;
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public InventoryElement(ColonistManager.Colonist colonist,ResourceManager.ResourceAmount resourceAmount,Transform parent, UIManager uiM) {
			this.colonist = colonist;
			this.resourceAmount = resourceAmount;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ResourceInfoElement-Panel"),parent,false);

			obj.transform.Find("Name").GetComponent<Text>().text = uiM.SplitByCapitals(resourceAmount.resource.name);
			// ADD RESOURCE IMAGE

			Update();
		}

		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
		}
	}

	public class ReservedResourcesColonistElement {
		public ColonistManager.Colonist colonist;
		public ResourceManager.ReservedResources reservedResources;
		public List<ReservedResourceElement> reservedResourceElements = new List<ReservedResourceElement>();
		public GameObject obj;

		public ReservedResourcesColonistElement(ColonistManager.Colonist colonist,ResourceManager.ReservedResources reservedResources,Transform parent) {
			this.colonist = colonist;
			this.reservedResources = reservedResources;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourcesColonistInfoElement-Panel"),parent,false);

			obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;
			obj.transform.Find("ColonistReservedCount-Text").GetComponent<Text>().text = reservedResources.resources.Count.ToString();
			obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];

			foreach (ResourceManager.ResourceAmount ra in reservedResources.resources) {
				reservedResourceElements.Add(new ReservedResourceElement(colonist,ra,parent));
			}
		}
	}

	public class ReservedResourceElement {
		public ColonistManager.Colonist colonist;
		public ResourceManager.ResourceAmount resourceAmount;
		public GameObject obj;

		public ReservedResourceElement(ColonistManager.Colonist colonist,ResourceManager.ResourceAmount resourceAmount,Transform parent) {
			this.colonist = colonist;
			this.resourceAmount = resourceAmount;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ReservedResourceInfoElement-Panel"),parent,false);

			obj.transform.Find("Name").GetComponent<Text>().text = colonist.uiM.SplitByCapitals(resourceAmount.resource.name);
			// ADD RESOURCE IMAGE

			Update();
		}

		public void Update() {
			obj.transform.Find("Amount").GetComponent<Text>().text = resourceAmount.amount.ToString();
		}
	}

	List<SkillElement> skillElements = new List<SkillElement>();
	List<InventoryElement> inventoryElements = new List<InventoryElement>();
	List<ReservedResourcesColonistElement> reservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();

	/* Called from ColonistManager.SetSelectedColonistFromInput(), ColonistManager.SetSelectedColonist(), ColonistManager.DeselectSelectedColonist(), TileManager.Initialize() */
	public void SetSelectedColonistInformation() {
		if (colonistM.selectedColonist != null) {
			selectedColonistInformationPanel.SetActive(true);

			RectTransform rightListPanel = GameObject.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			rightListPanel.offsetMin = new Vector2(rightListSize.x,310);

			selectedColonistInformationPanel.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonistM.selectedColonist.name;
			selectedColonistInformationPanel.transform.Find("ColonistBaseSprite-Image").GetComponent<Image>().sprite = colonistM.selectedColonist.moveSprites[0];

			foreach (SkillElement skillElement in skillElements) {
				Destroy(skillElement.obj);
			}
			skillElements.Clear();
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in reservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Destroy(reservedResourcesColonistElement.obj);
			}
			reservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in inventoryElements) {
				Destroy(inventoryElement.obj);
			}
			inventoryElements.Clear();

			foreach (ColonistManager.SkillInstance skill in colonistM.selectedColonist.skills) {
				skillElements.Add(new SkillElement(colonistM.selectedColonist,skill,selectedColonistInformationPanel.transform.Find("SkillsList-Panel")));
			}

			foreach (ResourceManager.ReservedResources rr in colonistM.selectedColonist.inventory.reservedResources) {
				reservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.colonist,rr,selectedColonistInformationPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryList-Panel")));
			}

			foreach (ResourceManager.ResourceAmount ra in colonistM.selectedColonist.inventory.resources) {
				inventoryElements.Add(new InventoryElement(colonistM.selectedColonist,ra,selectedColonistInformationPanel.transform.Find("Inventory-Panel/Inventory-ScrollPanel/InventoryList-Panel"),this));
			}

		} else {
			selectedColonistInformationPanel.SetActive(false);

			RectTransform rightListPanel = GameObject.Find("RightList-Panel").GetComponent<RectTransform>();
			Vector2 rightListSize = rightListPanel.offsetMin;
			rightListPanel.offsetMin = new Vector2(rightListSize.x,5);

			if (skillElements.Count > 0) {
				foreach (SkillElement skillElement in skillElements) {
					Destroy(skillElement.obj);
				}
				skillElements.Clear();
			}
			if (reservedResourcesColonistElements.Count > 0) {
				foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in reservedResourcesColonistElements) {
					foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
						Destroy(reservedResourceElement.obj);
					}
					reservedResourcesColonistElement.reservedResourceElements.Clear();
					Destroy(reservedResourcesColonistElement.obj);
				}
				reservedResourcesColonistElements.Clear();
			}
			if (inventoryElements.Count > 0) {
				foreach (InventoryElement inventoryElement in inventoryElements) {
					Destroy(inventoryElement.obj);
				}
				inventoryElements.Clear();
			}
		}
	}

	public void UpdateSelectedColonistInformation() {
		if (colonistM.selectedColonist != null) {
			selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventory-Slider").GetComponent<Slider>().maxValue = colonistM.selectedColonist.inventory.maxAmount;
			selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventory-Slider").GetComponent<Slider>().value = colonistM.selectedColonist.inventory.CountResources();
			selectedColonistInformationPanel.transform.Find("ColonistInventoryToggle-Button/ColonistInventoryValue-Text").GetComponent<Text>().text = colonistM.selectedColonist.inventory.CountResources() + "/ " + colonistM.selectedColonist.inventory.maxAmount;

			selectedColonistInformationPanel.transform.Find("ColonistAction-Text").GetComponent<Text>().text = "NO ACTION TEXT";
			selectedColonistInformationPanel.transform.Find("SkillsList-Panel/SkillsListTitle-Panel/Profession-Text").GetComponent<Text>().text = colonistM.selectedColonist.profession.name;

			foreach (SkillElement skillElement in skillElements) {
				skillElement.Update();
			}
			foreach (InventoryElement inventoryElement in inventoryElements) {
				inventoryElement.Update();
			}
		}
	}

	public class ColonistElement {

		public ColonistManager.Colonist colonist;
		public GameObject obj;

		public ColonistElement(ColonistManager.Colonist colonist,Transform transform) {
			this.colonist = colonist;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"),transform,false);

			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate { colonist.colonistM.SetSelectedColonist(colonist); });

			Update();
		}

		public void Update() {

		}

		public void DestroyObject() {
			Destroy(obj);
		}
	}

	List<ColonistElement> colonistElements = new List<ColonistElement>();

	public void RemoveColonistElements() {
		foreach (ColonistElement colonistElement in colonistElements) {
			colonistElement.DestroyObject();
		}
		colonistElements.Clear();
	}

	/* Called from ColonistManager.SpawnColonists() */
	public void SetColonistElements() {
		if (colonistList.activeSelf) {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				colonistElements.Add(new ColonistElement(colonist,colonistList.transform.Find("ColonistList-Panel")));
			}
		}
	}

	public void UpdateColonistElements() {
		foreach (ColonistElement colonistElement in colonistElements) {
			colonistElement.Update();
		}
	}

	public class JobElement {

		public JobManager.Job job;
		public ColonistManager.Colonist colonist;
		public GameObject obj;
		public GameObject colonistObj;

		public JobElement(JobManager.Job job,ColonistManager.Colonist colonist,Transform parent,UIManager uiM,CameraManager cameraM) {
			this.job = job;
			this.colonist = colonist;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/JobInfoElement-Panel"),parent,false);

			obj.transform.Find("JobInfo/Image").GetComponent<Image>().sprite = job.prefab.baseSprite;
			obj.transform.Find("JobInfo/Name").GetComponent<Text>().text = job.prefab.name;
			obj.transform.Find("JobInfo/Type").GetComponent<Text>().text = uiM.SplitByCapitals(job.prefab.jobType.ToString());
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				cameraM.SetCameraPosition(job.tile.obj.transform.position);
				cameraM.SetCameraZoom(5);
			});

			if (colonist != null) {
				obj.GetComponent<RectTransform>().sizeDelta = new Vector2(obj.GetComponent<RectTransform>().sizeDelta.x,105);
				obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().minValue = 0;
				obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().maxValue = job.colonistBuildTime;
				obj.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;

				colonistObj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"),obj.transform,false);

				colonistObj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
				colonistObj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
				colonistObj.GetComponent<Button>().onClick.AddListener(delegate { colonist.colonistM.SetSelectedColonist(colonist); });
				colonistObj.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;
				colonistObj.GetComponent<RectTransform>().sizeDelta = new Vector2(175,colonistObj.GetComponent<RectTransform>().sizeDelta.y);
				colonistObj.GetComponent<Outline>().enabled = false;
			}

			job.jobUIElement = this;

			Update();
		}

		public void Update() {
			obj.transform.Find("JobInfo/JobProgress-Slider").GetComponent<Slider>().value = job.colonistBuildTime - job.jobProgress;
		}

		public void DestroyObjects() {
			job.jobUIElement = null;
			Destroy(obj);
			Destroy(colonistObj);
		}

		public void Remove(UIManager uiM) {
			uiM.jobElements.Remove(this);
			job.jobUIElement = null;
			DestroyObjects();
		}
	}

	public List<JobElement> jobElements = new List<JobElement>();

	public void RemoveJobElements() {
		foreach (JobElement jobElement in jobElements) {
			jobElement.job.jobUIElement = null;
			jobElement.DestroyObjects();
		}
		jobElements.Clear();
	}

	/* Called from Colonist.FinishJob(), Colonist.CreateJob(), Colonist.AddExistingJob(), JobManager.GiveJobsToColonists(), TileManager.Initialize() */
	public void SetJobElements() {
		if (jobM.jobs.Count > 0 || colonistM.colonists.Where(colonist => colonist.job != null).ToList().Count > 0) {
			jobList.SetActive(true);
			RemoveJobElements();
			List<ColonistManager.Colonist> orderedColonists = colonistM.colonists.Where(colonist => colonist.job != null).OrderByDescending(colonist => 1 - (colonist.job.jobProgress / colonist.job.colonistBuildTime)).ToList();
			foreach (ColonistManager.Colonist jobColonist in orderedColonists) {
				jobElements.Add(new JobElement(jobColonist.job,jobColonist,jobList.transform.Find("JobList-Panel"),this,cameraM));
			}
			foreach (JobManager.Job job in jobM.jobs) {
				jobElements.Add(new JobElement(job,null,jobList.transform.Find("JobList-Panel"),this,cameraM));
			}
		} else {
			jobList.SetActive(false);
			if (jobElements.Count > 0) {
				RemoveJobElements();
			}
		}
	}

	public void UpdateJobElements() {
		foreach (JobElement jobElement in jobElements) {
			jobElement.Update();
		}
	}

	public void UpdateDateTimeInformation(int minute,int hour,int day,int month,int year,bool isDay) {
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Time-Text").GetComponent<Text>().text = timeM.Get12HourTime() + ":" + (minute < 10 ? ("0" + minute) : minute.ToString()) + (hour < 13 ? "AM" : "PM") + "(" + (isDay ? "D" : "N") + ")";
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Speed-Text").GetComponent<Text>().text = (timeM.timeModifier > 0 ? new string('>',timeM.timeModifier) : "-");
		dateTimeInformationPanel.transform.Find("DateTimeInformation-Date-Text").GetComponent<Text>().text = "D" + day + " M" + month + " Y" + year;
	}

	public void SelectionSizeCanvasSetActive(bool active) {
		selectionSizeCanvas.SetActive(active);
	}

	public void UpdateSelectionSizePanel(float xSize,float ySize,int selectionAreaCount,ResourceManager.TileObjectPrefab prefab) {
		int ixSize = Mathf.Abs(Mathf.FloorToInt(xSize));
		int iySize = Mathf.Abs(Mathf.FloorToInt(ySize));

		selectionSizePanel.transform.Find("Dimensions-Text").GetComponent<Text>().text = ixSize + " × " + iySize;
		selectionSizePanel.transform.Find("TotalSize-Text").GetComponent<Text>().text = (Mathf.RoundToInt(ixSize * iySize)) + " (" + selectionAreaCount + ")";
		selectionSizePanel.transform.Find("SelectedPrefabName-Text").GetComponent<Text>().text = prefab.name;
		selectionSizePanel.transform.Find("SelectedPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite;

		selectionSizeCanvas.transform.localScale = Vector2.one * 0.005f * 0.5f * cameraM.cameraComponent.orthographicSize;
		selectionSizeCanvas.transform.position = new Vector2(
			mousePosition.x + (selectionSizeCanvas.GetComponent<RectTransform>().sizeDelta.x / 2f * selectionSizeCanvas.transform.localScale.x),
			mousePosition.y + (selectionSizeCanvas.GetComponent<RectTransform>().sizeDelta.y / 2f * selectionSizeCanvas.transform.localScale.y)
		);
	}

	public void InitializeSelectedContainerIndicator() {
		selectedContainerIndicator = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),Vector2.zero,Quaternion.identity);
		SpriteRenderer sCISR = selectedContainerIndicator.GetComponent<SpriteRenderer>();
		sCISR.sprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		sCISR.sortingOrder = 20; // Selected Container Indicator Sprite
		sCISR.color = new Color(1f,1f,1f,0.75f);
		selectedContainerIndicator.transform.localScale = new Vector2(1f,1f) * 1.2f;
		selectedContainerIndicator.SetActive(false);
	}

	List<ReservedResourcesColonistElement> containerReservedResourcesColonistElements = new List<ReservedResourcesColonistElement>();
	List<InventoryElement> containerInventoryElements = new List<InventoryElement>();
	public void SetSelectedContainerInfo() {
		if (selectedContainer != null) {
			selectedContainerIndicator.SetActive(true);
			selectedContainerIndicator.transform.position = selectedContainer.parentObject.obj.transform.position;

			selectedContainerInventoryPanel.SetActive(true);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Destroy(reservedResourcesColonistElement.obj);
			}
			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				Destroy(inventoryElement.obj);
			}
			containerInventoryElements.Clear();

			int numResources = selectedContainer.inventory.CountResources();
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().minValue = 0;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().maxValue = selectedContainer.maxAmount;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-Slider").GetComponent<Slider>().value = numResources;
			selectedContainerInventoryPanel.transform.Find("SelectedContainerInventorySizeValue-Text").GetComponent<Text>().text = numResources + "/ " + selectedContainer.maxAmount;

			foreach (ResourceManager.ReservedResources rr in selectedContainer.inventory.reservedResources) {
				containerReservedResourcesColonistElements.Add(new ReservedResourcesColonistElement(rr.colonist,rr,selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel")));
			}
			foreach (ResourceManager.ResourceAmount ra in selectedContainer.inventory.resources) {
				containerInventoryElements.Add(new InventoryElement(colonistM.selectedColonist,ra,selectedContainerInventoryPanel.transform.Find("SelectedContainerInventory-ScrollPanel/InventoryList-Panel"),this));
			}
		} else {
			selectedContainerIndicator.SetActive(false);
			foreach (ReservedResourcesColonistElement reservedResourcesColonistElement in containerReservedResourcesColonistElements) {
				foreach (ReservedResourceElement reservedResourceElement in reservedResourcesColonistElement.reservedResourceElements) {
					Destroy(reservedResourceElement.obj);
				}
				reservedResourcesColonistElement.reservedResourceElements.Clear();
				Destroy(reservedResourcesColonistElement.obj);
			}
			containerReservedResourcesColonistElements.Clear();
			foreach (InventoryElement inventoryElement in containerInventoryElements) {
				Destroy(inventoryElement.obj);
			}
			containerInventoryElements.Clear();
			selectedContainerInventoryPanel.SetActive(false);
		}
	}

	public void UpdateSelectedContainerInfo() {
		foreach (InventoryElement inventoryElement in containerInventoryElements) {
			inventoryElement.Update();
		}
	}

	public void DisableAdminPanels(GameObject parentObj) {
		if (professionsList.activeSelf && parentObj != professionsList) {
			SetProfessionsList();
		}
		if (objectPrefabsList.activeSelf && parentObj != objectPrefabsList) {
			ToggleObjectPrefabsList();
		}
	}

	public class ProfessionElement {
		public ColonistManager.Profession profession;
		public GameObject obj;
		public GameObject colonistsInProfessionListObj;
		public List<GameObject> colonistsInProfessionElements = new List<GameObject>();
		public GameObject editColonistsInProfessionListObj;
		public List<GameObject> editColonistsInProfessionElements = new List<GameObject>();
		public bool lastRemoveState = false;

		public ProfessionElement(ColonistManager.Profession profession, Transform parent, UIManager uiM) {
			this.profession = profession;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ProfessionInfoElement-Panel"),parent,false);

			colonistsInProfessionListObj = obj.transform.Find("ColonistsInProfessionList-Panel").gameObject;
			obj.transform.Find("ColonistsInProfession-Button").GetComponent<Button>().onClick.AddListener(delegate {
				foreach (ProfessionElement professionElement in uiM.professionElements) {
					if (professionElement != this) {
						professionElement.colonistsInProfessionListObj.SetActive(false);
					}
					foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
						Destroy(obj);
					}
				}
				colonistsInProfessionListObj.SetActive(!colonistsInProfessionListObj.activeSelf);
				if (colonistsInProfessionListObj.activeSelf) {
					uiM.SetColonistsInProfessionList(this);
				}
			});

			editColonistsInProfessionListObj = obj.transform.Find("EditColonistsInProfessionList-Panel").gameObject;

			obj.transform.Find("ColonistsInProfession-Button/ColonistsInProfessionAmount-Text").GetComponent<Text>().text = profession.colonistsInProfession.Count.ToString();

			obj.transform.Find("ProfessionName-Text").GetComponent<Text>().text = profession.name;

			obj.transform.Find("AddColonists-Button").GetComponent<Button>().onClick.AddListener(delegate {
				foreach (ProfessionElement professionElement in uiM.professionElements) {
					if (professionElement != this) {
						professionElement.editColonistsInProfessionListObj.SetActive(false);
					}
					foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
						Destroy(obj);
					}
				}
				editColonistsInProfessionListObj.SetActive(!editColonistsInProfessionListObj.activeSelf);
				if (lastRemoveState) {
					editColonistsInProfessionListObj.SetActive(true);
				}
				if (editColonistsInProfessionListObj.activeSelf) {
					uiM.SetEditColonistsInProfessionList(this,false);
				}
				lastRemoveState = false;
			});
			if (profession.type != ColonistManager.ProfessionTypeEnum.Nothing) {
				obj.transform.Find("RemoveColonists-Button").GetComponent<Button>().onClick.AddListener(delegate {
					foreach (ProfessionElement professionElement in uiM.professionElements) {
						if (professionElement != this) {
							professionElement.editColonistsInProfessionListObj.SetActive(false);
						}
						foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
							Destroy(obj);
						}
					}
					editColonistsInProfessionListObj.SetActive(!editColonistsInProfessionListObj.activeSelf);
					if (!lastRemoveState) {
						editColonistsInProfessionListObj.SetActive(true);
					}
					if (editColonistsInProfessionListObj.activeSelf) {
						uiM.SetEditColonistsInProfessionList(this,true);
					}
					lastRemoveState = true;
				});
			} else {
				obj.transform.Find("RemoveColonists-Button").gameObject.SetActive(false);
			}
			colonistsInProfessionListObj.SetActive(false);
			editColonistsInProfessionListObj.SetActive(false);
		}

		public void Update() {
			obj.transform.Find("ColonistsInProfession-Button/ColonistsInProfessionAmount-Text").GetComponent<Text>().text = profession.colonistsInProfession.Count.ToString();
		}
	}

	public void SetColonistsInProfessionList(ProfessionElement professionElement) {
		foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
			Destroy(obj);
		}
		professionElement.colonistsInProfessionElements.Clear();
		foreach (ColonistManager.Colonist colonist in professionElement.profession.colonistsInProfession) {
			GameObject obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ColonistInfoElement-Panel"),professionElement.colonistsInProfessionListObj.transform,false);
			obj.transform.Find("BodySprite").GetComponent<Image>().sprite = colonist.moveSprites[0];
			obj.transform.Find("Name").GetComponent<Text>().text = colonist.name;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				colonistM.SetSelectedColonist(colonist);
			});
			professionElement.colonistsInProfessionElements.Add(obj);
		}
	}

	public void SetEditColonistsInProfessionList(ProfessionElement professionElement, bool remove) {
		foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
			Destroy(obj);
		}
		professionElement.editColonistsInProfessionElements.Clear();
		ColonistManager.Profession nothingProfession = colonistM.professions.Find(findProfession => findProfession.type == ColonistManager.ProfessionTypeEnum.Nothing);
		if (remove) {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.profession == professionElement.profession) {
					GameObject obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/EditColonistInProfessionInfoElement-Panel"),professionElement.editColonistsInProfessionListObj.transform,false);
					obj.GetComponent<Button>().onClick.AddListener(delegate {
						if (colonist.profession == professionElement.profession) {
							if (colonist.profession == colonist.oldProfession) {
								colonist.ChangeProfession(nothingProfession);
							} else {
								colonist.ChangeProfession(colonist.oldProfession);
							}
							obj.GetComponent<Image>().color = new Color(231f,76f,60f,255f) / 255f;
						} else {
							colonist.oldProfession = colonist.profession;
							colonist.ChangeProfession(professionElement.profession);
							obj.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;
						}
						obj.transform.Find("ColonistCurrentProfession-Text").GetComponent<Text>().text = colonist.profession.name;
					});
					obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];
					obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;
					obj.transform.Find("ColonistCurrentProfession-Text").GetComponent<Text>().text = colonist.profession.name;
					obj.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;
					professionElement.editColonistsInProfessionElements.Add(obj);
				}
			}
		} else {
			foreach (ColonistManager.Colonist colonist in colonistM.colonists) {
				if (colonist.profession != professionElement.profession) {
					GameObject obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/EditColonistInProfessionInfoElement-Panel"),professionElement.editColonistsInProfessionListObj.transform,false);
					obj.GetComponent<Button>().onClick.AddListener(delegate {
						if (colonist.profession == professionElement.profession) {
							colonist.ChangeProfession(colonist.oldProfession);
							obj.GetComponent<Image>().color = new Color(200f,200f,200f,255f) / 255f;
						} else {
							colonist.oldProfession = colonist.profession;
							colonist.ChangeProfession(professionElement.profession);
							obj.GetComponent<Image>().color = new Color(52f,152f,219f,255f) / 255f;
						}
						obj.transform.Find("ColonistCurrentProfession-Text").GetComponent<Text>().text = colonist.profession.name;
					});
					obj.transform.Find("ColonistImage").GetComponent<Image>().sprite = colonist.moveSprites[0];
					obj.transform.Find("ColonistName-Text").GetComponent<Text>().text = colonist.name;
					obj.transform.Find("ColonistCurrentProfession-Text").GetComponent<Text>().text = colonist.profession.name;
					professionElement.editColonistsInProfessionElements.Add(obj);
				}
			}
		}
	}

	private List<ProfessionElement> professionElements = new List<ProfessionElement>();
	public void InitializeProfessionsList() {
		foreach (ColonistManager.Profession profession in colonistM.professions) {
			professionElements.Add(new ProfessionElement(profession,professionsList.transform,this));
		}
		SetProfessionsList();
	}

	public void SetProfessionsList() {
		DisableAdminPanels(professionsList);
		professionsList.SetActive(!professionsList.activeSelf);
		foreach (ProfessionElement professionElement in professionElements) {
			foreach (GameObject obj in professionElement.colonistsInProfessionElements) {
				Destroy(obj);
			}
			foreach (GameObject obj in professionElement.editColonistsInProfessionElements) {
				Destroy(obj);
			}
			professionElement.colonistsInProfessionListObj.SetActive(false);
			professionElement.editColonistsInProfessionListObj.SetActive(false);
			professionElement.obj.SetActive(professionsList.activeSelf);
		}
	}

	public class ObjectPrefabElement {
		public ResourceManager.TileObjectPrefab prefab;
		public GameObject obj;
		public GameObject objectInstancesList;
		public List<ObjectInstanceElement> instanceElements = new List<ObjectInstanceElement>();

		public ObjectPrefabElement(ResourceManager.TileObjectPrefab prefab,Transform parent,CameraManager cameraM,ResourceManager resourceM,UIManager uiM) {
			this.prefab = prefab;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectPrefab-Button"),parent,false);

			obj.transform.Find("ObjectPrefabSprite-Image").GetComponent<Image>().sprite = prefab.baseSprite;
			obj.transform.Find("ObjectPrefabName-Text").GetComponent<Text>().text = prefab.name;

			objectInstancesList = obj.transform.Find("ObjectInstancesList-ScrollPanel").gameObject;
			obj.GetComponent<Button>().onClick.AddListener(delegate {
				objectInstancesList.SetActive(!objectInstancesList.activeSelf);
				if (objectInstancesList.activeSelf) {
					objectInstancesList.transform.SetParent(GameObject.Find("Canvas").transform);
					foreach (ObjectPrefabElement objectPrefabElement in uiM.objectPrefabElements) {
						if (objectPrefabElement != this) {
							objectPrefabElement.objectInstancesList.SetActive(false);
						}
					}
				} else {
					objectInstancesList.transform.SetParent(obj.transform);
				}
			});

			AddObjectInstancesList(true,cameraM,resourceM,uiM);

			Update();
		}

		public void AddObjectInstancesList(bool newList, CameraManager cameraM,ResourceManager resourceM,UIManager uiM) {
			RemoveObjectInstances();
			bool objectInstancesListState = objectInstancesList.activeSelf;
			if (newList) {
				objectInstancesListState = false;
			}
			objectInstancesList.SetActive(true);
			foreach (ResourceManager.TileObjectInstance instance in resourceM.GetTileObjectInstanceList(prefab)) {
				instanceElements.Add(new ObjectInstanceElement(instance,objectInstancesList.transform.Find("ObjectInstancesList-Panel"),cameraM,resourceM,uiM));
			}
			objectInstancesList.SetActive(objectInstancesListState);
			Update();
		}

		public void Remove() {
			RemoveObjectInstances();
			Destroy(obj);
		}

		public void RemoveObjectInstances() {
			foreach (ObjectInstanceElement instance in instanceElements) {
				Destroy(instance.obj);
			}
			instanceElements.Clear();
		}

		public void Update() {
			obj.transform.Find("ObjectPrefabAmount-Text").GetComponent<Text>().text = instanceElements.Count.ToString();
		}
	}

	public class ObjectInstanceElement {
		public ResourceManager.TileObjectInstance instance;
		public GameObject obj;

		public ObjectInstanceElement(ResourceManager.TileObjectInstance instance, Transform parent, CameraManager cameraM, ResourceManager resourceM, UIManager uiM) {
			this.instance = instance;

			obj = Instantiate(Resources.Load<GameObject>(@"UI/UIElements/ObjectInstance-Button"),parent,false);

			obj.transform.Find("ObjectInstanceSprite-Image").GetComponent<Image>().sprite = instance.obj.GetComponent<SpriteRenderer>().sprite;
			obj.transform.Find("ObjectInstanceName-Text").GetComponent<Text>().text = instance.prefab.name;
			obj.transform.Find("TilePosition-Text").GetComponent<Text>().text = "(" + Mathf.FloorToInt(instance.tile.obj.transform.position.x) + ", " + Mathf.FloorToInt(instance.tile.obj.transform.position.y) + ")"; ;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				cameraM.SetCameraPosition(instance.obj.transform.position);
				cameraM.SetCameraZoom(5);
			});

			ResourceManager.Container container = resourceM.containers.Find(findContainer => findContainer.parentObject == instance);
			if (container != null) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					uiM.selectedContainer = container;
					uiM.SetSelectedContainerInfo();
				});
			}
		}
	}

	public List<ObjectPrefabElement> objectPrefabElements = new List<ObjectPrefabElement>();

	public void ToggleObjectPrefabsList() {
		DisableAdminPanels(objectPrefabsList);
		objectPrefabsList.SetActive(!objectPrefabsList.activeSelf);
		foreach (ObjectPrefabElement objectPrefabElement in objectPrefabElements) {
			objectPrefabElement.objectInstancesList.transform.SetParent(objectPrefabElement.obj.transform);
			objectPrefabElement.objectInstancesList.SetActive(false);
		}
		if (objectPrefabElements.Count <= 0) {
			objectPrefabsList.SetActive(false);
		}
	}

	public enum ChangeTypesEnum { Add, Update, Remove };

	public void ChangeObjectPrefabElements(ChangeTypesEnum changeType,ResourceManager.TileObjectPrefab prefab) {
		if (prefab.tileObjectPrefabSubGroup.type == ResourceManager.TileObjectPrefabSubGroupsEnum.None || prefab.tileObjectPrefabSubGroup.tileObjectPrefabGroup.type == ResourceManager.TileObjectPrefabGroupsEnum.Command) {
			return;
		}
		switch (changeType) {
			case ChangeTypesEnum.Add:
				AddObjectPrefabElement(prefab);
				break;
			case ChangeTypesEnum.Update:
				UpdateObjectPrefabElement(prefab);
				break;
			case ChangeTypesEnum.Remove:
				RemoveObjectPrefabElement(prefab);
				break;
		}
	}

	private void AddObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement == null) {
			objectPrefabElements.Add(new ObjectPrefabElement(prefab,objectPrefabsList.transform.Find("ObjectPrefabsList-Panel"),cameraM,resourceM,this));
		} else {
			UpdateObjectPrefabElement(prefab);
		}
	}

	private void UpdateObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement != null) {
			objectPrefabElement.AddObjectInstancesList(false,cameraM,resourceM,this);
		}
	}

	private void RemoveObjectPrefabElement(ResourceManager.TileObjectPrefab prefab) {
		ObjectPrefabElement objectPrefabElement = objectPrefabElements.Find(element => element.prefab == prefab);
		if (objectPrefabElement != null) {
			objectPrefabElement.Remove();
			objectPrefabElements.Remove(objectPrefabElement);
		}
		if (objectPrefabElements.Count <= 0) {
			objectPrefabsList.SetActive(true);
			ToggleObjectPrefabsList();
		}
	}
}