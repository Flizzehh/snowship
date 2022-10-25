using Snowship.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : BaseManager {

	private GameObject debugPanel;
	private GameObject debugInputObj;
	public InputField debugInput;

	private GameObject debugConsole;
	private GameObject debugConsoleList;

	public override void Awake() {
		debugPanel = GameObject.Find("Debug-Panel");

		debugInputObj = debugPanel.transform.Find("DebugCommand-Input").gameObject;
		debugInput = debugInputObj.GetComponent<InputField>();
		debugInput.onEndEdit.AddListener(delegate {
			ParseCommandInput();
			SelectDebugInput();
		});

		debugConsole = debugPanel.transform.Find("DebugCommandsList-ScrollPanel").gameObject;
		debugConsoleList = debugConsole.transform.Find("DebugCommandsList-Panel").gameObject;

		debugPanel.SetActive(false);

		CreateCommandFunctions();
	}

	public bool debugMode;

	public override void Update() {
		if (GameManager.tileM.mapState == TileManager.MapState.Generated && (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote))) {
			debugMode = !debugMode;
			ToggleDebugUI();
			SelectDebugInput();
		}
		if (debugMode) {
			if (selectTileMouseToggle) {
				SelectTileMouseUpdate();
			}
			if (toggleSunCycleToggle) {
				ToggleSunCycleUpdate();
			}
		}
	}

	public void SelectDebugInput() {
		if (debugMode) {
			if (!UnityEngine.EventSystems.EventSystem.current.alreadySelecting) {
				UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(debugInput.gameObject, null);
				debugInput.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
			}
		}
	}

	private void ToggleDebugUI() {
		debugPanel.SetActive(debugMode);
	}

	private enum Commands {
		help,                   // help (commandName)						-- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command
		clear,                  // clear									-- clear the console
		selecthuman,            // selecthuman <name>						-- sets the selected human to the first human named <name>
		spawncolonists,         // spawncolonists <numToSpawn>				-- spawns <numToSpawn> colonists to join the colony
		spawncaravans,          // spawncaravans <numToSpawn> <caravanType> <maxNumTraders> 
								//											-- spawns <numToSpawn> caravans of the <caravanType> type with a maximum number of <maxNumTraders> traders
		sethealth,              // sethealth <amount>						-- sets the health of the selected colonist to <amount>
		setneed,                // setneed <name> <amount>					-- sets the need named <name> on the selected colonist to <amount>
		cia,                    // changeinvamt								-- shortcut to "changinvamt"
		changeinvamt,           // changeinvamt || cia <resourceName> <amount> (allColonists) (allContainers)
								//											-- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container,
								//											or all colonists (allColonists = true), or all containers (allContainers = true)
		listresources,          // listresources							-- lists all resources
		selectcontainer,        // selectcontainer <index>					-- sets the selected container to the container at <index> in the list of containers
		selecttiles,            // selecttiles <x(,rangeX)> <y,(rangeY)>	-- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)
		selecttilemouse,        // selecttilemouse							-- toggle continous selection of the tile at the mouse position
		viewtileproperty,       // viewtileproperty <variableName>			-- view the value of the variable with name <variableName>
		deselecttiles,          // deselecttiles							-- deselect all selected tiles
		deselectunwalkable,     // deselectunwalkabletiles					-- deselect all tiles in the selection that are unwalkable
		deselectwalkable,       // deselectwalkabletiles					-- deselect all tiles in the selection that are walkable
		deselectunbuildable,    // deselectunbuildable						-- deselect all tiles in the selection that are unbuildable
		deselectbuildable,      // deselectbuildable						-- deselect all tiles in the selection that are buildable
		changetiletype,         // changetiletype <tileType>				-- changes the tile type of the selected tile(s) to <tileType>
		listtiletypes,          // listtiletypes							-- lists all tile types
		changetileobj,          // changetileobj <tileObj> <variation> (rotationIndex)
								//											-- changes the tile object of the selected tile(s) to <tileObj> <variation> with (rotationIndex) (int - value depends on obj)
		removetileobj,          // removetileobj (layer)					-- without (layer): removes all tile objects on the selected tile(s). with (layer): removes the tile object at (layer)
		listtileobjs,           // listtileobjs								-- lists all tile objs
		changetileplant,        // changetileplant <plantType> <small>		-- changes the tile plant of the selected tile(s) to <plantType> and size large if (small = false), otherwise small
		removetileplant,        // removetileplant							-- removes the tile plant of the selected tile(s)
		listtileplants,         // listtileplants							-- lists all tile plants
		campos,                 // campos <x> <y>							-- sets the camera position to <x> <y>
		camzoom,                // camzoom <zoom>							-- sets the camera orthographic size to <zoom>
		togglesuncycle,         // togglesuncycle							-- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)
		settime,                // settime <time>							-- sets the in-game time to <time> (time = float between 0 and 23, decimals represent minutes (0.5 = 30 minutes))
		pause,                  // pause									-- pauses or unpauses the game
		viewselectedtiles,      // viewselectedtiles						-- sets the map overlay to highlight all selected tiles
		viewnormal,             // viewnormal								-- sets the map overlay to normal gameplay settings
		viewregions,            // viewregions								-- sets the map overlay to colour individual regions (region colour is random)
		viewheightmap,          // viewheightmap							-- sets the map overlay to colour each tile depending on its height between 0 and 1 (black = 0, white = 1)
		viewprecipitation,      // viewprecipitation						-- sets the map overlay to colour each tile depending on its precipitation between 0 and 1 (black = 0, white = 1)
		viewtemperature,        // viewtemperature							-- sets the map overlay to colour each tile depending on its temperature (darker = colder, lighter = warmer)
		viewbiomes,             // viewbiomes								-- sets the map overlay to colour individual biomes (colours correspond to specific biomes)
		viewresourceveins,		// viewresourceveins						-- sets the map overlay to show normal sprites for tiles with resources and white for other tiles
		viewdrainagebasins,     // viewdrainagebasins						-- sets the map overlay to colour individual drainage basins (drainage basin colour is random)
		viewrivers,             // viewrivers (index)						-- without (index): highlight all rivers. with (index): highlight the river at (index) in the list of rivers
		viewlargerivers,        // viewlargerivers (index)					-- without (index): highlight all large rivers. with (index): highlight the large river at (index) in the list of large rivers
		viewwalkspeed,          // viewwalkspeed							-- sets the map overlay to colour each tile depending on its walk speed (black = 0, white = 1)
		viewregionblocks,       // viewregionblocks							-- sets the map overlay to colour individual region blocks (region block colour is random)
		viewsquareregionblocks, // viewsquareregionblocks					-- sets the map overlay to colour invididual square region blocks (square region block colour is random)
		viewlightblockingtiles, // viewlightblockingtiles					-- higlights all tiles that block light
		viewshadowstarttiles,   // viewshadowstarttiles						-- higlights all tiles that can start a shadow
		viewshadowsfrom,        // viewshadowsfrom							-- highlights all tiles that affect the brightness of the selected tile through the day (green = earlier, pink = later)
		viewshadowsto,          // viewshadowsto							-- highlights all tiles that the selected tile affects the brightness of through the day (green = earlier, pink = later)
		viewblockingfrom,       // viewblockingfrom							-- highlights all tiles that the selected tile blocks shadows from (tiles that have shadows that were cut short because this tile was in the way)
		viewroofs,              // viewroofs								-- higlights all tiles with a roof above it
		viewhidden,				// viewhidden								-- shows all tiles on the map, including ones that are hidden from the player's view
		listjobs,				// listjobs (colonist)						-- list all jobs and their costs for each colonist or for a specific colonist with the (colonist) argument
		growfarms,				// growfarms								-- instantly grow all farms on the map
	};

	private readonly Dictionary<Commands, string> commandHelpOutputs = new Dictionary<Commands, string>() {
		{Commands.help,"help (commandName) -- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command" },
		{Commands.clear,"clear -- clear the console" },
		{Commands.selecthuman,"selecthuman <name> -- sets the selected human to the first human named <name>" },
		{Commands.spawncolonists,"spawncolonists <numToSpawn> -- spawns <numToSpawn> colonists to join the colony" },
		{Commands.spawncaravans,"spawncaravans <numToSpawn> <caravanType> <maxNumTraders> -- spawns <numToSpawn> caravans of the <caravanType> type with a maximum number of <maxNumTraders> traders" },
		{Commands.sethealth,"sethealth <amount> -- sets the health of the selected colonist to <amount> (value between 0 (0%) -> 1 (100%))" },
		{Commands.setneed,"setneed <name> <amount> -- sets the need named <name> on the selected colonist to <amount> (value between 0 (0%) -> 100 (100%))" },
		{Commands.cia,"cia -- shortcut to \"changeinvamt\" -- see that command for more information" },
		{Commands.changeinvamt,"changeinvamt <resourceName> <amount> (allColonists) (allContainers) -- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container, or all colonists (allColonists = true), or all containers (allContainers = true)" },
		{Commands.listresources,"listresources -- lists all resources" },
		{Commands.selectcontainer,"selectcontainer <index> -- sets the selected container to the container at <index> in the list of containers" },
		{Commands.selecttiles,"selecttiles <x(,rangeX)> <y,(rangeY)> -- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)" },
		{Commands.selecttilemouse,"selecttilemouse -- continuously select the tile under the mouse until this command is run again" },
		{Commands.viewtileproperty,"viewtileproperty <variableName> -- view the value of the variable with name <variableName>" },
		{Commands.deselecttiles,"deselecttiles -- deselect all selected tiles" },
		{Commands.deselectunwalkable,"deselectunwalkabletiles -- deselect all tiles in the selection that are unwalkable" },
		{Commands.deselectwalkable,"deselectwalkabletiles -- deselect all tiles in the selection that are walkable" },
		{Commands.deselectunbuildable,"deselectunbuildable -- deselect all tiles in the selection that are unbuildable" },
		{Commands.deselectbuildable,"deselectbuildable -- deselect all tiles in the selection that are buildable" },
		{Commands.changetiletype,"changetiletype <tileType> -- changes the tile type of the selected tile to <tileType>" },
		{Commands.listtiletypes,"listtiletypes -- lists all tile types" },
		{Commands.changetileobj,"changetileobj <tileObj> <variation> (rotationIndex) -- changes the tile object of the selected tile(s) to <tileObj> <variation> with (rotationIndex) (int - value depends on obj)" },
		{Commands.removetileobj,"removetileobj (layer) -- without (layer): removes all tile objects on the selected tile(s). with (layer): removes the tile object at (layer)" },
		{Commands.listtileobjs,"listtileobjs -- lists all tile objs" },
		{Commands.changetileplant,"changetileplant <plantType> <small> -- changes the tile plant of the selected tile(s) to <plantType> and size large if (small = false), otherwise small" },
		{Commands.removetileplant,"removetileplant -- removes the tile plant of the selected tile(s)" },
		{Commands.listtileplants,"listtileplants -- lists all tile plants" },
		{Commands.campos,"campos <x> <y> -- sets the camera position to <x> <y>" },
		{Commands.camzoom,"camzoom <zoom> -- sets the camera orthographic size to <zoom>" },
		{Commands.togglesuncycle,"togglesuncycle -- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)" },
		{Commands.settime,"settime <time> -- sets the in-game time to <time> (time = float between 0 and 23, decimals represent minutes (0.5 = 30 minutes))" },
		{Commands.pause,"pause -- pauses or unpauses the game" },
		{Commands.viewselectedtiles,"viewselectedtiles -- sets the map overlay to highlight all selected tiles" },
		{Commands.viewnormal,"viewnormal -- sets the map overlay to normal gameplay settings" },
		{Commands.viewregions,"viewregions -- sets the map overlay to colour individual regions (region colour is random)" },
		{Commands.viewheightmap,"viewheightmap -- sets the map overlay to colour each tile depending on its height between 0 and 1 (black = 0, white = 1)" },
		{Commands.viewprecipitation,"viewprecipitation -- sets the map overlay to colour each tile depending on its precipitation between 0 and 1 (black = 0, white = 1)" },
		{Commands.viewtemperature,"viewtemperature -- sets the map overlay to colour each tile depending on its temperature (darker = colder, lighter = warmer)" },
		{Commands.viewbiomes,"viewbiomes -- sets the map overlay to colour individual biomes (colours correspond to specific biomes)" },
		{Commands.viewresourceveins,"viewresourceveins -- sets the map overlay to show normal sprites for tiles with resources and white for other tiles" },
		{Commands.viewdrainagebasins,"viewdrainagebasins -- sets the map overlay to colour individual drainage basins (drainage basin colour is random)" },
		{Commands.viewrivers,"viewrivers (index) -- without (index): highlight all rivers. with (index): highlight the river at (index) in the list of rivers" },
		{Commands.viewwalkspeed,"viewwalkspeed -- sets the map overlay to colour each tile depending on its walk speed (black = 0, white = 1)" },
		{Commands.viewregionblocks,"viewregionblocks -- sets the map overlay to colour individual region blocks (region block colour is random)" },
		{Commands.viewsquareregionblocks,"viewsquareregionblocks -- sets the map overlay to colour invididual square region blocks (square region block colour is random)" },
		{Commands.viewlightblockingtiles,"viewlightblockingtiles -- higlights all tiles that block light" },
		{Commands.viewshadowstarttiles,"viewshadowstarttiles -- higlights all tiles that can start a shadow" },
		{Commands.viewshadowsfrom,"viewshadowsfrom -- highlights all tiles that affect the brightness of the selected tile through the day (green = earlier, pink = later)" },
		{Commands.viewshadowsto,"viewshadowsto -- highlights all tiles that the selected tile affects the brightness of through the day (green = earlier, pink = later)" },
		{Commands.viewblockingfrom,"viewblockingfrom -- highlights all tiles that the selected tile blocks shadows from (tiles that have shadows that were cut short because this tile was in the way)" },
		{Commands.viewroofs,"viewroofs -- higlights all tiles with a roof above it" },
		{Commands.viewhidden, "viewhidden -- shows all tiles on the map, including ones that are hidden from the player's view" },
		{Commands.listjobs,"listjobs (colonist) -- list all jobs and their costs for each colonist or for a specific colonist with the (colonist) argument" },
		{Commands.growfarms,"growfarms -- instantly grow all farms on the map" },
	};

	public void ParseCommandInput() {
		string commandString = debugInput.text;
		OutputToConsole(commandString);

		if (string.IsNullOrEmpty(commandString)) {
			return;
		}

		debugInput.text = string.Empty;

		List<string> commandStringSplit = commandString.Split(' ').ToList();
		if (commandStringSplit.Count > 0) {
			Commands selectedCommand;
			try {
				selectedCommand = (Commands)Enum.Parse(typeof(Commands), commandStringSplit[0]);
			} catch {
				OutputToConsole("ERROR: Unknown command: " + commandString);
				return;
			}
			commandFunctions[selectedCommand](selectedCommand, commandStringSplit.Skip(1).ToList());
		}
	}

	private static readonly float charsPerLine = 70;
	private static readonly int textBoxSizePerLine = 17;

	private void OutputToConsole(string outputString) {
		if (!string.IsNullOrEmpty(outputString)) {
			GameObject outputTextBox = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/CommandOutputText-Panel"), debugConsoleList.transform, false);

			int lines = Mathf.CeilToInt(outputString.Length / charsPerLine);

			outputTextBox.GetComponent<LayoutElement>().minHeight = lines * textBoxSizePerLine;
			outputTextBox.GetComponent<RectTransform>().sizeDelta = new Vector2(outputTextBox.GetComponent<RectTransform>().sizeDelta.x, lines * textBoxSizePerLine);

			outputTextBox.transform.Find("Text").GetComponent<Text>().text = outputString;
		}
	}

	private readonly Dictionary<Commands, Action<Commands, List<string>>> commandFunctions = new Dictionary<Commands, Action<Commands, List<string>>>();

	private List<TileManager.Tile> selectedTiles = new List<TileManager.Tile>();
	private bool selectTileMouseToggle;

	private void SelectTileMouseUpdate() {
		selectedTiles.Clear();
		selectedTiles.Add(GameManager.uiM.mouseOverTile);
	}

	private bool toggleSunCycleToggle;
	private float holdHour = 12;

	public void ToggleSunCycleUpdate() {
		GameManager.colonyM.colony.map.SetTileBrightness(holdHour, true);
	}

	private int viewRiverAtIndex = 0;

	private void CreateCommandFunctions() {
		commandFunctions.Add(Commands.help, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (KeyValuePair<Commands, string> commandHelpKVP in commandHelpOutputs) {
					OutputToConsole(commandHelpKVP.Value);
				}
			} else if (parameters.Count == 1) {
				Commands parameterCommand;
				try {
					parameterCommand = (Commands)Enum.Parse(typeof(Commands), parameters[0]);
				} catch {
					OutputToConsole("ERROR: Unable to get help with command: " + parameters[0]);
					return;
				}
				if ((int)parameterCommand < commandHelpOutputs.Count && (int)parameterCommand >= 0) {
					OutputToConsole(commandHelpOutputs[parameterCommand]);
				} else {
					OutputToConsole("ERROR: Command index is out of range: " + parameters[0]);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.clear, delegate (Commands selectedCommand, List<string> parameters) {
			foreach (Transform child in debugConsoleList.transform) {
				MonoBehaviour.Destroy(child.gameObject);
			}
			debugConsoleList.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
			debugConsole.transform.Find("Scrollbar").GetComponent<Scrollbar>().value = 0;
		});
		commandFunctions.Add(Commands.selecthuman, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				HumanManager.Human selectedHuman = GameManager.humanM.humans.Find(human => human.name == parameters[0]);
				if (selectedHuman != null) {
					GameManager.humanM.SetSelectedHuman(selectedHuman);
					if (GameManager.humanM.selectedHuman == selectedHuman) {
						OutputToConsole("SUCCESS: Selected " + GameManager.humanM.selectedHuman.name + ".");
					}
				} else {
					OutputToConsole("ERROR: Invalid colonist name: " + parameters[0]);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.spawncolonists, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				int numberToSpawn = 1;
				if (int.TryParse(parameters[0], out numberToSpawn)) {
					int oldNumColonists = GameManager.colonistM.colonists.Count;
					GameManager.colonistM.SpawnColonists(numberToSpawn);
					if (oldNumColonists + numberToSpawn == GameManager.colonistM.colonists.Count) {
						OutputToConsole("SUCCESS: Spawned " + numberToSpawn + " colonists.");
					} else {
						OutputToConsole("ERROR: Unable to spawn colonists. Spawned " + (GameManager.colonistM.colonists.Count - oldNumColonists) + " colonists.");
					}
				} else {
					OutputToConsole("ERROR: Invalid number of colonists.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.spawncaravans, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 3) {
				int numberToSpawn = 1;
				if (int.TryParse(parameters[0], out numberToSpawn)) {
					CaravanManager.CaravanTypeEnum caravanType = CaravanManager.CaravanTypeEnum.Foot;
					try {
						caravanType = (CaravanManager.CaravanTypeEnum)Enum.Parse(typeof(CaravanManager.CaravanTypeEnum), parameters[1]);
					} catch (ArgumentException) {
						OutputToConsole("ERROR: Unknown caravan type: " + parameters[1] + ".");
						return;
					}
					int maxNumTraders = 4;
					if (int.TryParse(parameters[2], out maxNumTraders)) {
						int oldNumCaravans = GameManager.caravanM.caravans.Count;
						for (int i = 0; i < numberToSpawn; i++) {
							GameManager.caravanM.SpawnCaravan(caravanType, maxNumTraders);
						}
						if (oldNumCaravans + numberToSpawn == GameManager.caravanM.caravans.Count) {
							OutputToConsole("SUCCESS: Spawned " + numberToSpawn + " caravans.");
						} else {
							OutputToConsole("ERROR: Unable to spawn caravans. Spawned " + (GameManager.caravanM.caravans.Count - oldNumCaravans) + " caravans.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse maxNumTraders as int.");
					}
				} else {
					OutputToConsole("ERROR: Invalid number of colonists.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.sethealth, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				float newHealth = 0;
				if (float.TryParse(parameters[0], out newHealth)) {
					if (newHealth >= 0 && newHealth <= 1) {
						if (GameManager.humanM.selectedHuman != null) {
							GameManager.humanM.selectedHuman.health = newHealth;
							if (GameManager.humanM.selectedHuman.health == newHealth) {
								OutputToConsole("SUCCESS: " + GameManager.humanM.selectedHuman.name + "'s health is now " + GameManager.humanM.selectedHuman.health + ".");
							} else {
								OutputToConsole("ERROR: Unable to change " + GameManager.humanM.selectedHuman.name + "'s health.");
							}
						} else {
							OutputToConsole("ERROR: No human selected.");
						}
					} else {
						OutputToConsole("ERROR: Invalid range on health value (float from 0 to 1).");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse health value as float.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.setneed, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				ColonistManager.NeedPrefab needPrefab = GameManager.colonistM.needPrefabs.Find(need => need.name == parameters[0]);
				if (needPrefab != null) {
					if (GameManager.humanM.selectedHuman != null) {
						if (GameManager.humanM.selectedHuman is ColonistManager.Colonist) {
							ColonistManager.Colonist selectedColonist = (ColonistManager.Colonist)GameManager.humanM.selectedHuman;
							ColonistManager.NeedInstance needInstance = selectedColonist.needs.Find(need => need.prefab == needPrefab);
							if (needInstance != null) {
								float newNeedValue = 0;
								if (float.TryParse(parameters[1], out newNeedValue)) {
									if (newNeedValue >= 0 && newNeedValue <= 100) {
										needInstance.SetValue(newNeedValue);
										if (needInstance.GetValue() == newNeedValue) {
											OutputToConsole("SUCCESS: " + needInstance.prefab.name + " now has value " + needInstance.GetValue() + ".");
										} else {
											OutputToConsole("ERROR: Unable to change " + needInstance.prefab.name + " need value.");
										}
									} else {
										OutputToConsole("ERROR: Invalid range on need value (float from 0 to 100).");
									}
								} else {
									OutputToConsole("ERROR: Unable to parse need value as float.");
								}
							} else {
								OutputToConsole("ERROR: Unable to find specified need on selected colonist.");
							}
						} else {
							OutputToConsole("ERROR: Selected human is not a colonist.");
						}
					} else {
						OutputToConsole("ERROR: No human selected.");
					}
				} else {
					OutputToConsole("ERROR: Invalid need name.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.cia, delegate (Commands selectedCommand, List<string> parameters) {
			commandFunctions[Commands.changeinvamt](Commands.changeinvamt, parameters);
		});
		commandFunctions.Add(Commands.changeinvamt, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				ResourceManager.Resource resource = GameManager.resourceM.GetResources().Find(r => r.type.ToString() == parameters[0]);
				if (resource != null) {
					if (GameManager.humanM.selectedHuman != null || GameManager.uiM.selectedContainer != null) {
						if (GameManager.humanM.selectedHuman != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								GameManager.humanM.selectedHuman.GetInventory().ChangeResourceAmount(resource, amount, false);
								OutputToConsole("SUCCESS: Added " + amount + " " + resource.name + " to " + GameManager.humanM.selectedHuman + ".");
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("No colonist selected, skipping.");
						}
						if (GameManager.uiM.selectedContainer != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								GameManager.uiM.selectedContainer.GetInventory().ChangeResourceAmount(resource, amount, false);
								OutputToConsole("SUCCESS: Added " + amount + " " + resource.name + " to container.");
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("No container selected, skipping.");
						}
					} else {
						int amount = 0;
						if (int.TryParse(parameters[1], out amount)) {
							foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
								colonist.GetInventory().ChangeResourceAmount(resource, amount, false);
							}
							foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
								container.GetInventory().ChangeResourceAmount(resource, amount, false);
							}
							OutputToConsole("SUCCESS: Added " + amount + " " + resource.name + " to all colonists and all containers.");
						} else {
							OutputToConsole("ERROR: Unable to parse resource amount as int.");
						}
					}
				} else {
					OutputToConsole("ERROR: Invalid resource name.");
				}
			} else if (parameters.Count == 3) {
				bool allColonists = false;
				if (bool.TryParse(parameters[2], out allColonists)) {
					if (allColonists) {
						ResourceManager.Resource resource = GameManager.resourceM.GetResources().Find(r => r.type.ToString() == parameters[0]);
						if (resource != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
									colonist.GetInventory().ChangeResourceAmount(resource, amount, false);
								}
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("ERROR: Invalid resource name.");
						}
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allColonists parameter as bool.");
				}
			} else if (parameters.Count == 4) {
				bool allColonists = false;
				if (bool.TryParse(parameters[2], out allColonists)) {
					if (allColonists) {
						ResourceManager.Resource resource = GameManager.resourceM.GetResources().Find(r => r.type.ToString() == parameters[0]);
						if (resource != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
									colonist.GetInventory().ChangeResourceAmount(resource, amount, false);
								}
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("ERROR: Invalid resource name.");
						}
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allColonists parameter as bool.");
				}
				bool allContainers = false;
				if (bool.TryParse(parameters[3], out allContainers)) {
					if (allContainers) {
						ResourceManager.Resource resource = GameManager.resourceM.GetResources().Find(r => r.type.ToString() == parameters[0]);
						if (resource != null) {
							int amount = 0;
							if (int.TryParse(parameters[1], out amount)) {
								foreach (ResourceManager.Container container in GameManager.resourceM.containers) {
									container.GetInventory().ChangeResourceAmount(resource, amount, false);
								}
							} else {
								OutputToConsole("ERROR: Unable to parse resource amount as int.");
							}
						} else {
							OutputToConsole("ERROR: Invalid resource name.");
						}
					}
				} else {
					OutputToConsole("ERROR: Unable to parse allContainers parameter as bool.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.listresources, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources()) {
					OutputToConsole(resource.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.selectcontainer, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				int index = -1;
				if (int.TryParse(parameters[0], out index)) {
					if (index >= 0 && index < GameManager.resourceM.containers.Count) {
						GameManager.uiM.SetSelectedContainer(GameManager.resourceM.containers[index]);
					} else {
						OutputToConsole("ERROR: Container index out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse container index as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.selecttiles, delegate (Commands selectedCommand, List<string> parameters) {
			int count = selectedTiles.Count;
			if (parameters.Count == 2) {
				int startX = 0;
				if (int.TryParse(parameters[0], out startX)) {
					int startY = 0;
					if (int.TryParse(parameters[1], out startY)) {
						if (startX >= 0 && startY >= 0 && startX < GameManager.colonyM.colony.map.mapData.mapSize && startY < GameManager.colonyM.colony.map.mapData.mapSize) {
							selectedTiles.Add(GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(startX, startY)));
						} else {
							OutputToConsole("ERROR: Positions out of range.");
						}
					} else {
						if (parameters[1].Split(',').ToList().Count > 0) {
							string startYString = parameters[1].Split(',')[0];
							string endYString = parameters[1].Split(',')[1];
							int endY = 0;
							if (int.TryParse(startYString, out startY) && int.TryParse(endYString, out endY)) {
								if (startX >= 0 && startY >= 0 && endY >= 0 && startX < GameManager.colonyM.colony.map.mapData.mapSize && startY < GameManager.colonyM.colony.map.mapData.mapSize && endY < GameManager.colonyM.colony.map.mapData.mapSize) {
									if (startY <= endY) {
										for (int y = startY; y <= endY; y++) {
											selectedTiles.Add(GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(startX, y)));
										}
									} else {
										OutputToConsole("ERROR: Starting y position is greater than ending y position.");
									}
								} else {
									OutputToConsole("ERROR: Positions out of range.");
								}
							} else {
								OutputToConsole("ERROR: Unable to parse y positions.");
							}
						} else {
							OutputToConsole("ERROR: Unable to parse y as single position or split into two positions by a comma.");
						}
					}
				} else {
					if (parameters[0].Split(',').ToList().Count > 0) {
						string startXString = parameters[0].Split(',')[0];
						string endXString = parameters[0].Split(',')[1];
						int endX = 0;
						if (int.TryParse(startXString, out startX) && int.TryParse(endXString, out endX)) {
							int startY = 0;
							if (int.TryParse(parameters[1], out startY)) {
								if (startX >= 0 && startY >= 0 && endX >= 0 && startX < GameManager.colonyM.colony.map.mapData.mapSize && startY < GameManager.colonyM.colony.map.mapData.mapSize && endX < GameManager.colonyM.colony.map.mapData.mapSize) {
									if (startX <= endX) {
										for (int x = startX; x <= endX; x++) {
											selectedTiles.Add(GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(x, startY)));
										}
									} else {
										OutputToConsole("ERROR: Starting x position is greater than ending x position.");
									}
								} else {
									OutputToConsole("ERROR: Positions out of range.");
								}
							} else {
								if (parameters[1].Split(',').ToList().Count > 0) {
									string startYString = parameters[1].Split(',')[0];
									string endYString = parameters[1].Split(',')[1];
									int endY = 0;
									if (int.TryParse(startYString, out startY) && int.TryParse(endYString, out endY)) {
										if (startX >= 0 && startY >= 0 && endX >= 0 && endY >= 0 && startX < GameManager.colonyM.colony.map.mapData.mapSize && startY < GameManager.colonyM.colony.map.mapData.mapSize && endX < GameManager.colonyM.colony.map.mapData.mapSize && endY < GameManager.colonyM.colony.map.mapData.mapSize) {
											if (startX <= endX && startY <= endY) {
												for (int y = startY; y <= endY; y++) {
													for (int x = startX; x <= endX; x++) {
														selectedTiles.Add(GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(x, y)));
													}
												}
											} else {
												OutputToConsole("ERROR: Starting x or y position is greater than ending x or y position.");
											}
										} else {
											OutputToConsole("ERROR: Positions out of range.");
										}
									} else {
										OutputToConsole("ERROR: Unable to parse y positions.");
									}
								} else {
									OutputToConsole("ERROR: Unable to parse y as single position or split into two positions by a comma.");
								}
							}
						} else {
							OutputToConsole("ERROR: Unable to parse x positions.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse x as single position or split into two positions by a comma.");
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			selectedTiles = selectedTiles.Distinct().ToList();
			OutputToConsole("Selected " + (selectedTiles.Count - count) + " tiles.");
		});
		commandFunctions.Add(Commands.selecttilemouse, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				selectTileMouseToggle = !selectTileMouseToggle;
				OutputToConsole("State: " + (selectTileMouseToggle ? "On" : "Off"));
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewtileproperty, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				foreach (TileManager.Tile tile in selectedTiles) {
					Type type = tile.GetType();
					if (type != null) {
						System.Reflection.PropertyInfo propertyInfo = type.GetProperty(parameters[0]);
						if (propertyInfo != null) {
							object value = propertyInfo.GetValue(tile, null);
							if (value != null) {
								OutputToConsole(tile.position + ": \"" + value.ToString() + "\"");
							} else {
								OutputToConsole("ERROR: Value is null on propertyInfo for " + tile.position + ".");
							}
						} else {
							System.Reflection.FieldInfo fieldInfo = type.GetField(parameters[0]);
							if (fieldInfo != null) {
								object value = fieldInfo.GetValue(tile);
								if (value != null) {
									OutputToConsole(tile.position + ": \"" + value.ToString() + "\"");
								} else {
									OutputToConsole("ERROR: Value is null on fieldInfo for " + tile.position + ".");
								}
							} else {
								OutputToConsole("ERROR: propertyInfo/fieldInfo is null on type for (" + parameters[0] + ").");
							}
						}
					} else {
						OutputToConsole("ERROR: type is null on tile.");
					}
					//OutputToConsole(tile.GetType().GetProperty(parameters[0]).GetValue(tile, null).ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.deselecttiles, delegate (Commands selectedCommand, List<string> parameters) {
			int selectedTilesCount = selectedTiles.Count;
			if (parameters.Count == 0) {
				selectedTiles.Clear();
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + selectedTilesCount + " tiles.");
		});
		commandFunctions.Add(Commands.deselectunwalkable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (!tile.walkable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " unwalkable tiles.");
		});
		commandFunctions.Add(Commands.deselectwalkable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (tile.walkable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " walkable tiles.");
		});
		commandFunctions.Add(Commands.deselectunbuildable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (!tile.buildable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " unbuildable tiles.");
		});
		commandFunctions.Add(Commands.deselectbuildable, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				List<TileManager.Tile> deselectedTiles = new List<TileManager.Tile>();
				foreach (TileManager.Tile tile in selectedTiles) {
					if (tile.buildable) {
						deselectedTiles.Add(tile);
					}
				}
				foreach (TileManager.Tile tile in deselectedTiles) {
					selectedTiles.Remove(tile);
					counter += 1;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Deselected " + counter + " buildable tiles.");
		});
		commandFunctions.Add(Commands.changetiletype, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 1) {
				TileManager.TileType tileType = TileManager.TileType.GetTileTypeByString(parameters[0]);
				if (tileType != null) {
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							bool previousWalkableState = tile.walkable;
							tile.SetTileType(tileType, false, true, true);
							if (!previousWalkableState && tile.walkable) {
								GameManager.colonyM.colony.map.RemoveTileBrightnessEffect(tile);
							} else if (previousWalkableState && !tile.walkable) {
								GameManager.colonyM.colony.map.RecalculateLighting(new List<TileManager.Tile>() { tile }, true);
							}
							counter += 1;
						}
					} else {
						OutputToConsole("No tiles are currently selected.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse tile type.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Changed " + counter + " tile types.");
		});
		commandFunctions.Add(Commands.listtiletypes, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.TileType tileType in TileManager.TileType.tileTypes) {
					OutputToConsole(tileType.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.changetileobj, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 3) {
				ResourceManager.ObjectPrefab objectPrefab = GameManager.resourceM.GetObjectPrefabByString(parameters[0]);
				if (objectPrefab != null) {
					// Variation doesn't need a null check because everything is designed around it working regardless of whether it's null or not
					ResourceManager.Variation variation = objectPrefab.GetVariationFromString(parameters[1]);

					int rotation = 0;
					if (int.TryParse(parameters[1], out rotation)) {
						if (rotation >= 0 && rotation < objectPrefab.GetBitmaskSpritesForVariation(variation).Count) {
							if (selectedTiles.Count > 0) {
								foreach (TileManager.Tile tile in selectedTiles) {
									if (tile.objectInstances.ContainsKey(objectPrefab.layer) && tile.objectInstances[objectPrefab.layer] != null) {
										GameManager.resourceM.RemoveObjectInstance(tile.objectInstances[objectPrefab.layer]);
										tile.RemoveObjectAtLayer(objectPrefab.layer);
									}
									tile.SetObject(GameManager.resourceM.CreateObjectInstance(objectPrefab, variation, tile, rotation, true));
									tile.GetObjectInstanceAtLayer(objectPrefab.layer).FinishCreation();
									counter += 1;
								}
							} else {
								OutputToConsole("No tiles are currently selected.");
							}
						} else {
							OutputToConsole("ERROR: Rotation index is out of range.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse rotation index.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse tile object.");
				}
			} else if (parameters.Count == 2) {
				ResourceManager.ObjectPrefab objectPrefab = GameManager.resourceM.GetObjectPrefabByString(parameters[0]);
				if (objectPrefab != null) {
					// Variation doesn't need a null check because everything is designed around it working regardless of whether it's null or not
					ResourceManager.Variation variation = objectPrefab.GetVariationFromString(parameters[1]);

					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							if (tile.objectInstances.ContainsKey(objectPrefab.layer) && tile.objectInstances[objectPrefab.layer] != null) {
								GameManager.resourceM.RemoveObjectInstance(tile.objectInstances[objectPrefab.layer]);
								tile.RemoveObjectAtLayer(objectPrefab.layer);
							}
							tile.SetObject(GameManager.resourceM.CreateObjectInstance(objectPrefab, variation, tile, 0, true));
							tile.GetObjectInstanceAtLayer(objectPrefab.layer).FinishCreation();
							counter += 1;
						}
					} else {
						OutputToConsole("No tiles are currently selected.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse tile object.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			GameManager.colonyM.colony.map.RemoveTileBrightnessEffect(null);
			OutputToConsole("Changed " + counter + " tile objects.");
		});
		commandFunctions.Add(Commands.removetileobj, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						List<ResourceManager.ObjectInstance> removeTOIs = new List<ResourceManager.ObjectInstance>();
						foreach (KeyValuePair<int, ResourceManager.ObjectInstance> toiKVP in tile.objectInstances) {
							removeTOIs.Add(toiKVP.Value);
						}
						foreach (ResourceManager.ObjectInstance toi in removeTOIs) {
							GameManager.resourceM.RemoveObjectInstance(tile.objectInstances[toi.prefab.layer]);
							tile.RemoveObjectAtLayer(toi.prefab.layer);
						}
						counter += 1;
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else if (parameters.Count == 1) {
				int layer = 0;
				if (int.TryParse(parameters[0], out layer)) {
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							if (tile.objectInstances.ContainsKey(layer) && tile.objectInstances[layer] != null) {
								GameManager.resourceM.RemoveObjectInstance(tile.objectInstances[layer]);
								tile.RemoveObjectAtLayer(layer);
								counter += 1;
							}
						}
					} else {
						OutputToConsole("No tiles are currently selected.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse layer as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Removed " + counter + " tile objects.");
		});
		commandFunctions.Add(Commands.listtileobjs, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ResourceManager.ObjectPrefab top in GameManager.resourceM.GetObjectPrefabs()) {
					OutputToConsole(top.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.changetileplant, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 2) {
				ResourceManager.PlantPrefab plantPrefab = GameManager.resourceM.GetPlantPrefabByString(parameters[0]);
				if (plantPrefab != null) {
					bool small = false;
					if (bool.TryParse(parameters[1], out small)) {
						if (selectedTiles.Count > 0) {
							foreach (TileManager.Tile tile in selectedTiles) {
								tile.SetPlant(false, new ResourceManager.Plant(plantPrefab, tile, small, true, null));
								GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
								if (tile.plant != null) {
									counter += 1;
								}
							}
						} else {
							OutputToConsole("No tiles are currently selected.");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse small bool.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse plant type.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Changed " + counter + " tile plants.");
		});
		commandFunctions.Add(Commands.removetileplant, delegate (Commands selectedCommand, List<string> parameters) {
			int counter = 0;
			if (parameters.Count == 0) {
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						tile.SetPlant(true, null);
						if (tile.plant == null) {
							counter += 1;
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
			OutputToConsole("Removed " + counter + " tile plants.");
		});
		commandFunctions.Add(Commands.listtileplants, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ResourceManager.PlantPrefab plantGroup in GameManager.resourceM.GetPlantPrefabs()) {
					OutputToConsole(plantGroup.type.ToString());
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.campos, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 2) {
				int camX = 0;
				if (int.TryParse(parameters[0], out camX)) {
					int camY = 0;
					if (int.TryParse(parameters[1], out camY)) {
						GameManager.cameraM.SetCameraPosition(new Vector2(camX, camY));
					} else {
						OutputToConsole("ERROR: Unable to parse camera y position as int.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse camera x position as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.camzoom, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				float camZoom = 0;
				if (float.TryParse(parameters[0], out camZoom)) {
					GameManager.cameraM.SetCameraZoom(camZoom);
				} else {
					OutputToConsole("ERROR: Unable to parse camera zoom as float.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.togglesuncycle, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				holdHour = GameManager.timeM.tileBrightnessTime;
				toggleSunCycleToggle = !toggleSunCycleToggle;
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.settime, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 1) {
				float time = 0;
				if (float.TryParse(parameters[0], out time)) {
					if (time >= 0 && time < 24) {
						holdHour = time;
						GameManager.timeM.SetTime(time);
						GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
					} else {
						OutputToConsole("ERROR: Time out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse time as float.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.pause, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				GameManager.timeM.TogglePause();
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewselectedtiles, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				if (selectedTiles.Count <= 0) {
					OutputToConsole("No tiles are currently selected.");
				} else {
					foreach (TileManager.Tile tile in selectedTiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = Color.blue;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewnormal, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.sr.color = Color.white;
				}
				GameManager.colonyM.colony.map.Bitmasking(GameManager.colonyM.colony.map.tiles, true, true);
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewregions, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.Region region in GameManager.colonyM.colony.map.regions) {
					Color colour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
					foreach (TileManager.Tile tile in region.tiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = colour;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewheightmap, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
					tile.sr.color = new Color(tile.height, tile.height, tile.height, 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewprecipitation, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
					tile.sr.color = new Color(tile.GetPrecipitation(), tile.GetPrecipitation(), tile.GetPrecipitation(), 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewtemperature, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
					tile.sr.color = new Color((tile.temperature + 50f) / 100f, (tile.temperature + 50f) / 100f, (tile.temperature + 50f) / 100f, 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewbiomes, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
					if (tile.biome != null) {
						tile.sr.color = tile.biome.colour;
					} else {
						tile.sr.color = Color.black;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewresourceveins, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				GameManager.colonyM.colony.map.Bitmasking(GameManager.colonyM.colony.map.tiles, false, true);
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					if (tile.tileType.resourceRanges.Find(rr => TileManager.ResourceVein.resourceVeinValidTileFunctions.ContainsKey(rr.resource.type)) == null) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = Color.white;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewdrainagebasins, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (KeyValuePair<TileManager.Map.Region, TileManager.Tile> kvp in GameManager.colonyM.colony.map.drainageBasins) {
					Color colour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
					foreach (TileManager.Tile tile in kvp.Key.tiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = colour;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewrivers, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.rivers) {
					Color riverColour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
					foreach (TileManager.Tile tile in river.tiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = riverColour;
					}
					river.tiles[0].sr.color = UIManager.GetColour(UIManager.Colours.DarkRed);
					river.endTile.sr.color = UIManager.GetColour(UIManager.Colours.LightRed);
					river.tiles[river.tiles.Count - 1].sr.color = UIManager.GetColour(UIManager.Colours.DarkGreen);
					river.startTile.sr.color = UIManager.GetColour(UIManager.Colours.LightGreen);
				}
				OutputToConsole("Showing " + GameManager.colonyM.colony.map.rivers.Count + " rivers.");
			} else if (parameters.Count == 1) {
				if (int.TryParse(parameters[0], out viewRiverAtIndex)) {
					if (viewRiverAtIndex >= 0 && viewRiverAtIndex < GameManager.colonyM.colony.map.rivers.Count) {
						Color riverColour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
						foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.rivers[viewRiverAtIndex].tiles) {
							tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
							tile.sr.color = riverColour;
						}
						GameManager.colonyM.colony.map.rivers[viewRiverAtIndex].tiles[0].sr.color = UIManager.GetColour(UIManager.Colours.DarkRed);
						GameManager.colonyM.colony.map.rivers[viewRiverAtIndex].endTile.sr.color = UIManager.GetColour(UIManager.Colours.LightRed);
						GameManager.colonyM.colony.map.rivers[viewRiverAtIndex].tiles[GameManager.colonyM.colony.map.rivers[viewRiverAtIndex].tiles.Count - 1].sr.color = UIManager.GetColour(UIManager.Colours.DarkGreen);
						GameManager.colonyM.colony.map.rivers[viewRiverAtIndex].startTile.sr.color = UIManager.GetColour(UIManager.Colours.LightGreen);
						OutputToConsole("Showing river " + (viewRiverAtIndex + 1) + " of " + GameManager.colonyM.colony.map.rivers.Count + " rivers.");
					} else {
						OutputToConsole("ERROR: River index out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse river index as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewlargerivers, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.largeRivers) {
					Color riverColour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
					foreach (TileManager.Tile tile in river.tiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = riverColour;
					}
					river.tiles[0].sr.color = UIManager.GetColour(UIManager.Colours.DarkRed);
					river.endTile.sr.color = UIManager.GetColour(UIManager.Colours.LightRed);
					river.tiles[river.tiles.Count - 1].sr.color = UIManager.GetColour(UIManager.Colours.DarkGreen);
					river.startTile.sr.color = UIManager.GetColour(UIManager.Colours.LightGreen);
					river.centreTile.sr.color = UIManager.GetColour(UIManager.Colours.LightBlue);
				}
				OutputToConsole("Showing " + GameManager.colonyM.colony.map.largeRivers.Count + " large rivers.");
			} else if (parameters.Count == 1) {
				if (int.TryParse(parameters[0], out viewRiverAtIndex)) {
					if (viewRiverAtIndex >= 0 && viewRiverAtIndex < GameManager.colonyM.colony.map.largeRivers.Count) {
						Color riverColour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
						foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].tiles) {
							tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
							tile.sr.color = riverColour;
						}
						GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].tiles[0].sr.color = UIManager.GetColour(UIManager.Colours.DarkRed);
						GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].endTile.sr.color = UIManager.GetColour(UIManager.Colours.LightRed);
						GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].tiles[GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].tiles.Count - 1].sr.color = UIManager.GetColour(UIManager.Colours.DarkGreen);
						GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].startTile.sr.color = UIManager.GetColour(UIManager.Colours.LightGreen);
						GameManager.colonyM.colony.map.largeRivers[viewRiverAtIndex].centreTile.sr.color = UIManager.GetColour(UIManager.Colours.LightBlue);
						OutputToConsole("Showing river " + (viewRiverAtIndex + 1) + " of " + GameManager.colonyM.colony.map.largeRivers.Count + " large rivers.");
					} else {
						OutputToConsole("ERROR: River index out of range.");
					}
				} else {
					OutputToConsole("ERROR: Unable to parse river index as int.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewwalkspeed, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
					tile.sr.color = new Color(tile.walkSpeed, tile.walkSpeed, tile.walkSpeed, 1f);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewregionblocks, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.RegionBlock region in GameManager.colonyM.colony.map.regionBlocks) {
					Color colour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
					foreach (TileManager.Tile tile in region.tiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = colour;
					}
					TileManager.Tile averageTile = GameManager.colonyM.colony.map.GetTileFromPosition(region.averagePosition);
					averageTile.sr.color = UIManager.GetColour(UIManager.Colours.White);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewsquareregionblocks, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Map.RegionBlock region in GameManager.colonyM.colony.map.squareRegionBlocks) {
					Color colour = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
					foreach (TileManager.Tile tile in region.tiles) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = colour;
					}
					TileManager.Tile averageTile = GameManager.colonyM.colony.map.GetTileFromPosition(region.averagePosition);
					averageTile.sr.color = UIManager.GetColour(UIManager.Colours.White);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewlightblockingtiles, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.SetVisible(true);
					if (tile.blocksLight) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = Color.red;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewshadowstarttiles, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.DetermineShadowSourceTiles(GameManager.colonyM.colony.map.tiles)) {
					tile.SetVisible(true);
					tile.sr.color = Color.red;
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewshadowsfrom, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						foreach (KeyValuePair<int, Dictionary<TileManager.Tile, float>> shadowsFromKVP in tile.shadowsFrom) {
							foreach (KeyValuePair<TileManager.Tile, float> sTile in shadowsFromKVP.Value) {
								sTile.Key.SetColour(new Color(shadowsFromKVP.Key / 24f, 1 - (shadowsFromKVP.Key / 24f), shadowsFromKVP.Key / 24f, 1f), 12);
							}
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else if (parameters.Count == 1) {
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
				if (selectedTiles.Count > 0) {
					int hour = -1;
					if (int.TryParse(parameters[0], out hour)) {
						if (hour >= 0 && hour <= 23) {
							foreach (TileManager.Tile tile in selectedTiles) {
								foreach (KeyValuePair<TileManager.Tile, float> sTile in tile.shadowsFrom[hour]) {
									sTile.Key.SetColour(new Color(hour / 24f, 1 - (hour / 24f), hour / 24f, 1f), 12);
								}
							}
						} else {
							OutputToConsole("ERROR: Hour out of range (0 [inclusive] to 23 [inclusive]).");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse hour as int.");
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewshadowsto, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						foreach (KeyValuePair<int, List<TileManager.Tile>> shadowsToKVP in tile.shadowsTo) {
							foreach (TileManager.Tile shadowsToTile in shadowsToKVP.Value) {
								shadowsToTile.SetColour(new Color(shadowsToKVP.Key / 24f, 1 - (shadowsToKVP.Key / 24f), shadowsToKVP.Key / 24f, 1f), 12);
							}
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else if (parameters.Count == 1) {
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
				if (selectedTiles.Count > 0) {
					int hour = -1;
					if (int.TryParse(parameters[0], out hour)) {
						if (hour >= 0 && hour <= 23) {
							foreach (TileManager.Tile tile in selectedTiles) {
								foreach (TileManager.Tile sTile in tile.shadowsTo[hour]) {
									sTile.SetColour(new Color(hour / 24f, 1 - (hour / 24f), hour / 24f, 1f), 12);
								}
							}
						} else {
							OutputToConsole("ERROR: Hour out of range (0 [inclusive] to 23 [inclusive]).");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse hour as int.");
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewblockingfrom, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
				if (selectedTiles.Count > 0) {
					foreach (TileManager.Tile tile in selectedTiles) {
						foreach (KeyValuePair<int, List<TileManager.Tile>> blockingShadowsFromKVP in tile.blockingShadowsFrom) {
							foreach (TileManager.Tile blockingShadowsFromTile in blockingShadowsFromKVP.Value) {
								blockingShadowsFromTile.SetColour(Color.red, 12);
							}
						}
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else if (parameters.Count == 1) {
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
				if (selectedTiles.Count > 0) {
					int hour = -1;
					if (int.TryParse(parameters[0], out hour)) {
						if (hour >= 0 && hour <= 23) {
							foreach (TileManager.Tile tile in selectedTiles) {
								foreach (TileManager.Tile blockingShadowsFromTile in tile.blockingShadowsFrom[hour]) {
									blockingShadowsFromTile.SetColour(Color.red, 12);
								}
							}
						} else {
							OutputToConsole("ERROR: Hour out of range (0 [inclusive] to 23 [inclusive]).");
						}
					} else {
						OutputToConsole("ERROR: Unable to parse hour as int.");
					}
				} else {
					OutputToConsole("No tiles are currently selected.");
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewroofs, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.SetVisible(true);
					if (tile.HasRoof()) {
						tile.sr.sprite = GameManager.resourceM.whiteSquareSprite;
						tile.sr.color = Color.red;
					}
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.viewhidden, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
					tile.SetVisible(true);
				}
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.listjobs, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
					OutputToConsole(colonist.name);
					foreach (Job job in JobManager.GetSortedJobs(colonist)) {
						OutputToConsole("\t" + job.objectPrefab.jobType + " " + job.objectPrefab.type + " " + JobManager.CalculateJobCost(colonist, job, null));
					}
				}
			} else if (parameters.Count == 1) {
				OutputToConsole("ERROR: Not yet implemented.");
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
		commandFunctions.Add(Commands.growfarms, delegate (Commands selectedCommand, List<string> parameters) {
			if (parameters.Count == 0) {
				int numFarmsGrown = 0;
				foreach (ResourceManager.Farm farm in GameManager.resourceM.farms) {
					farm.growTimer = farm.prefab.growthTimeDays * (TimeManager.dayLengthSeconds - 1);
					farm.Update();
					numFarmsGrown += 1;
				}
				OutputToConsole($"{numFarmsGrown} farms have been grown.");
			} else {
				OutputToConsole("ERROR: Invalid number of parameters specified.");
			}
		});
	}
}