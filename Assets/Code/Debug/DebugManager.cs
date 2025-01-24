using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Snowship.NCamera;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NInput;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUI;
using Snowship.NUI.Simulation.DebugConsole;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
public class DebugManager : IManager
{
	private bool debugEnabled = false;

	public event Action<string> OnConsoleOutputProduced;
	public event Action OnConsoleClearRequested;

	public void OnCreate() {

		GameManager.Get<InputManager>().InputSystemActions.Simulation.DebugMenu.performed += OnDebugMenuButtonPerformed;

		CreateCommandFunctions();
	}

	private void OnDebugMenuButtonPerformed(InputAction.CallbackContext callbackContext) {
		debugEnabled = !debugEnabled;
		if (debugEnabled) {
			GameManager.Get<UIManager>().OpenViewAsync<UIDebugConsole>().Forget();
		} else {
			GameManager.Get<UIManager>().CloseView<UIDebugConsole>();
		}
	}

	public void OnUpdate() {
		if (debugEnabled) {
			if (selectTileMouseToggle) {
				SelectTileMouseUpdate();
			}
			if (toggleSunCycleToggle) {
				ToggleSunCycleUpdate();
			}
		}
	}

	public void ParseCommandInput(string commandString) {

		Output(commandString);

		if (string.IsNullOrEmpty(commandString)) {
			return;
		}

		List<string> commandStringSplit = commandString.Split(' ').ToList();
		if (commandStringSplit.Count > 0) {
			Commands selectedCommand;
			try {
				selectedCommand = (Commands)Enum.Parse(typeof(Commands), commandStringSplit[0]);
			} catch {
				Output($"ERROR: Unknown command: {commandString}");
				return;
			}
			commandFunctions[selectedCommand](commandStringSplit.Skip(1).ToList());
		}
	}

	private void Output(string text) {
		if (string.IsNullOrEmpty(text)) {
			return;
		}
		OnConsoleOutputProduced?.Invoke(text);
	}

	private void Clear() {
		OnConsoleClearRequested?.Invoke();
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	private enum Commands
	{
		help,
		clear,
		selecthuman,
		spawncolonists,
		spawncaravans,
		sethealth,
		setneed,
		setmood,
		cia,
		changeinvamt,
		listresources,
		selectcontainer,
		selecttiles,
		selecttilemouse,
		viewtileproperty,
		deselecttiles,
		deselectunwalkable,
		deselectwalkable,
		deselectunbuildable,
		deselectbuildable,
		changetiletype,
		listtiletypes,
		changetileobj,
		removetileobj,
		listtileobjs,
		changetileplant,
		removetileplant,
		listtileplants,
		campos,
		camzoom,
		togglesuncycle,
		settime,
		pause,
		viewselectedtiles,
		viewnormal,
		viewregions,
		viewheightmap,
		viewprecipitation,
		viewtemperature,
		viewbiomes,
		viewresourceveins,
		viewdrainagebasins,
		viewrivers,
		viewlargerivers,
		viewwalkspeed,
		viewregionblocks,
		viewsquareregionblocks,
		viewlightblockingtiles,
		viewshadowstarttiles,
		viewshadowsfrom,
		viewshadowsto,
		viewblockingfrom,
		viewroofs,
		viewhidden,
		listjobs,
		growfarms
	}

	private readonly Dictionary<Commands, string> commandHelpOutputs = new() {
		{ Commands.help, "help (commandName) -- without (commandName): lists all commands. with (commandName), shows syntax/description of the specified command" },
		{ Commands.clear, "clear -- clear the console" },
		{ Commands.selecthuman, "selecthuman <name> -- sets the selected human to the first human named <name>" },
		{ Commands.spawncolonists, "spawncolonists <numToSpawn> -- spawns <numToSpawn> colonists to join the colony" },
		{ Commands.spawncaravans, "spawncaravans <numToSpawn> <caravanType> <maxNumTraders> -- spawns <numToSpawn> caravans of the <caravanType> type with a maximum number of <maxNumTraders> traders" },
		{ Commands.sethealth, "sethealth <amount> -- sets the health of the selected colonist to <amount> (value between 0 (0%) -> 1 (100%))" },
		{ Commands.setneed, "setneed <name> <amount> -- sets the need named <name> on the selected colonist to <amount> (value between 0 (0%) -> 100 (100%))" },
		{ Commands.setmood, "setmood <name> -- adds the mood named <name> on the selected colonist" },
		{ Commands.cia, "cia -- shortcut to \"changeinvamt\" -- see that command for more information" },
		{ Commands.changeinvamt, "changeinvamt <resourceName> <amount> (allColonists) (allContainers) -- adds the amount of the resource named <resourceName> to <amount> to a selected colonist, container, or all colonists (allColonists = true), or all containers (allContainers = true)" },
		{ Commands.listresources, "listresources -- lists all resources" },
		{ Commands.selectcontainer, "selectcontainer <index> -- sets the selected container to the container at <index> in the list of containers" },
		{ Commands.selecttiles, "selecttiles <x(,rangeX)> <y,(rangeY)> -- without (rangeX/Y): select tile at <x> <y>. with (rangeX/Y): select all tiles between <x> -> (rangeX) <y> -> (rangeY)" },
		{ Commands.selecttilemouse, "selecttilemouse -- continuously select the tile under the mouse until this command is run again" },
		{ Commands.viewtileproperty, "viewtileproperty <variableName> -- view the value of the variable with name <variableName>" },
		{ Commands.deselecttiles, "deselecttiles -- deselect all selected tiles" },
		{ Commands.deselectunwalkable, "deselectunwalkabletiles -- deselect all tiles in the selection that are unwalkable" },
		{ Commands.deselectwalkable, "deselectwalkabletiles -- deselect all tiles in the selection that are walkable" },
		{ Commands.deselectunbuildable, "deselectunbuildable -- deselect all tiles in the selection that are unbuildable" },
		{ Commands.deselectbuildable, "deselectbuildable -- deselect all tiles in the selection that are buildable" },
		{ Commands.changetiletype, "changetiletype <tileType> -- changes the tile type of the selected tile to <tileType>" },
		{ Commands.listtiletypes, "listtiletypes -- lists all tile types" },
		{ Commands.changetileobj, "changetileobj <tileObj> <variation> (rotationIndex) -- changes the tile object of the selected tile(s) to <tileObj> <variation> with (rotationIndex) (int - value depends on obj)" },
		{ Commands.removetileobj, "removetileobj (layer) -- without (layer): removes all tile objects on the selected tile(s). with (layer): removes the tile object at (layer)" },
		{ Commands.listtileobjs, "listtileobjs -- lists all tile objs" },
		{ Commands.changetileplant, "changetileplant <plantType> <small> -- changes the tile plant of the selected tile(s) to <plantType> and size large if (small = false), otherwise small" },
		{ Commands.removetileplant, "removetileplant -- removes the tile plant of the selected tile(s)" },
		{ Commands.listtileplants, "listtileplants -- lists all tile plants" },
		{ Commands.campos, "campos <x> <y> -- sets the camera position to <x> <y>" },
		{ Commands.camzoom, "camzoom <zoom> -- sets the camera orthographic size to <zoom>" },
		{ Commands.togglesuncycle, "togglesuncycle -- toggle the day/night (sun) cycle (i.e. the changing of tile brightnesses every N in-game minutes)" },
		{ Commands.settime, "settime <time> -- sets the in-game time to <time> (time = float between 0 and 23, decimals represent minutes (0.5 = 30 minutes))" },
		{ Commands.pause, "pause -- pauses or unpauses the game" },
		{ Commands.viewselectedtiles, "viewselectedtiles -- sets the map overlay to highlight all selected tiles" },
		{ Commands.viewnormal, "viewnormal -- sets the map overlay to normal gameplay settings" },
		{ Commands.viewregions, "viewregions -- sets the map overlay to colour individual regions (region colour is random)" },
		{ Commands.viewheightmap, "viewheightmap -- sets the map overlay to colour each tile depending on its height between 0 and 1 (black = 0, white = 1)" },
		{ Commands.viewprecipitation, "viewprecipitation -- sets the map overlay to colour each tile depending on its precipitation between 0 and 1 (black = 0, white = 1)" },
		{ Commands.viewtemperature, "viewtemperature -- sets the map overlay to colour each tile depending on its temperature (darker = colder, lighter = warmer)" },
		{ Commands.viewbiomes, "viewbiomes -- sets the map overlay to colour individual biomes (colours correspond to specific biomes)" },
		{ Commands.viewresourceveins, "viewresourceveins -- sets the map overlay to show normal sprites for tiles with resources and white for other tiles" },
		{ Commands.viewdrainagebasins, "viewdrainagebasins -- sets the map overlay to colour individual drainage basins (drainage basin colour is random)" },
		{ Commands.viewrivers, "viewrivers (index) -- without (index): highlight all rivers. with (index): highlight the river at (index) in the list of rivers" },
		{ Commands.viewwalkspeed, "viewwalkspeed -- sets the map overlay to colour each tile depending on its walk speed (black = 0, white = 1)" },
		{ Commands.viewregionblocks, "viewregionblocks -- sets the map overlay to colour individual region blocks (region block colour is random)" },
		{ Commands.viewsquareregionblocks, "viewsquareregionblocks -- sets the map overlay to colour individual square region blocks (square region block colour is random)" },
		{ Commands.viewlightblockingtiles, "viewlightblockingtiles -- highlights all tiles that block light" },
		{ Commands.viewshadowstarttiles, "viewshadowstarttiles -- highlights all tiles that can start a shadow" },
		{ Commands.viewshadowsfrom, "viewshadowsfrom -- highlights all tiles that affect the brightness of the selected tile through the day (green = earlier, pink = later)" },
		{ Commands.viewshadowsto, "viewshadowsto -- highlights all tiles that the selected tile affects the brightness of through the day (green = earlier, pink = later)" },
		{ Commands.viewblockingfrom, "viewblockingfrom -- highlights all tiles that the selected tile blocks shadows from (tiles that have shadows that were cut short because this tile was in the way)" },
		{ Commands.viewroofs, "viewroofs -- highlights all tiles with a roof above it" },
		{ Commands.viewhidden, "viewhidden -- shows all tiles on the map, including ones that are hidden from the player's view" },
		{ Commands.listjobs, "listjobs (colonist) -- list all jobs and their costs for each colonist or for a specific colonist with the (colonist) argument" },
		{ Commands.growfarms, "growfarms -- instantly grow all farms on the map" }
	};

	private readonly Dictionary<Commands, Action<List<string>>> commandFunctions = new();

	private List<TileManager.Tile> selectedTiles = new();
	private bool selectTileMouseToggle;

	private void SelectTileMouseUpdate() {
		selectedTiles.Clear();
		selectedTiles.Add(GameManager.Get<UIManagerOld>().mouseOverTile);
	}

	private bool toggleSunCycleToggle;
	private float holdHour = 12;

	private void ToggleSunCycleUpdate() {
		GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(holdHour, true);
	}

	private int viewRiverAtIndex = 0;

	private void CreateCommandFunctions() {
		commandFunctions.Add(
			Commands.help,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (KeyValuePair<Commands, string> commandHelpKVP in commandHelpOutputs) {
						Output(commandHelpKVP.Value);
					}
				} else if (parameters.Count == 1) {
					Commands parameterCommand;
					try {
						parameterCommand = (Commands)Enum.Parse(typeof(Commands), parameters[0]);
					} catch {
						Output($"ERROR: Unable to get help with command: {parameters[0]}");
						return;
					}
					if ((int)parameterCommand < commandHelpOutputs.Count && (int)parameterCommand >= 0) {
						Output(commandHelpOutputs[parameterCommand]);
					} else {
						Output($"ERROR: Command index is out of range: {parameters[0]}");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(Commands.clear, delegate { Clear(); });
		commandFunctions.Add(
			Commands.selecthuman,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					HumanManager.Human selectedHuman = GameManager.Get<HumanManager>().humans.Find(human => human.name == parameters[0]);
					if (selectedHuman != null) {
						GameManager.Get<HumanManager>().SetSelectedHuman(selectedHuman);
						if (GameManager.Get<HumanManager>().selectedHuman == selectedHuman) {
							Output($"SUCCESS: Selected {GameManager.Get<HumanManager>().selectedHuman.name}.");
						}
					} else {
						Output($"ERROR: Invalid colonist name: {parameters[0]}");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.spawncolonists,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					if (int.TryParse(parameters[0], out int numberToSpawn)) {
						int oldNumColonists = Colonist.colonists.Count;
						GameManager.Get<ColonistManager>().SpawnColonists(numberToSpawn);
						if (oldNumColonists + numberToSpawn == Colonist.colonists.Count) {
							Output($"SUCCESS: Spawned {numberToSpawn} colonists.");
						} else {
							Output($"ERROR: Unable to spawn colonists. Spawned {Colonist.colonists.Count - oldNumColonists} colonists.");
						}
					} else {
						Output("ERROR: Invalid number of colonists.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.spawncaravans,
			delegate(List<string> parameters) {
				if (parameters.Count == 3) {
					if (int.TryParse(parameters[0], out int numberToSpawn)) {
						CaravanType caravanType;
						try {
							caravanType = (CaravanType)Enum.Parse(typeof(CaravanType), parameters[1]);
						} catch (ArgumentException) {
							Output($"ERROR: Unknown caravan type: {parameters[1]}.");
							return;
						}
						if (int.TryParse(parameters[2], out int maxNumTraders)) {
							int oldNumCaravans = GameManager.Get<CaravanManager>().caravans.Count;
							for (int i = 0; i < numberToSpawn; i++) {
								GameManager.Get<CaravanManager>().SpawnCaravan(caravanType, maxNumTraders);
							}
							if (oldNumCaravans + numberToSpawn == GameManager.Get<CaravanManager>().caravans.Count) {
								Output($"SUCCESS: Spawned {numberToSpawn} caravans.");
							} else {
								Output($"ERROR: Unable to spawn caravans. Spawned {GameManager.Get<CaravanManager>().caravans.Count - oldNumCaravans} caravans.");
							}
						} else {
							Output("ERROR: Unable to parse maxNumTraders as int.");
						}
					} else {
						Output("ERROR: Invalid number of colonists.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.sethealth,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					if (float.TryParse(parameters[0], out float newHealth)) {
						if (newHealth is >= 0 and <= 1) {
							if (GameManager.Get<HumanManager>().selectedHuman != null) {
								FieldInfo field = typeof(LifeManager.Life).GetField("Health", BindingFlags.NonPublic | BindingFlags.Instance);
								if (field != null) {
									field.SetValue(GameManager.Get<HumanManager>().selectedHuman, 100);
								}
								if (Mathf.Approximately(GameManager.Get<HumanManager>().selectedHuman.Health, newHealth)) {
									Output($"SUCCESS: {GameManager.Get<HumanManager>().selectedHuman.name}'s health is now {GameManager.Get<HumanManager>().selectedHuman.Health}.");
								} else {
									Output($"ERROR: Unable to change {GameManager.Get<HumanManager>().selectedHuman.name}'s health.");
								}
							} else {
								Output("ERROR: No human selected.");
							}
						} else {
							Output("ERROR: Invalid range on health value (float from 0 to 1).");
						}
					} else {
						Output("ERROR: Unable to parse health value as float.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.setneed,
			delegate(List<string> parameters) {
				if (parameters.Count == 2) {
					NeedPrefab needPrefab = NeedPrefab.needPrefabs.Find(need => need.name == parameters[0]);
					if (needPrefab != null) {
						if (GameManager.Get<HumanManager>().selectedHuman != null) {
							if (GameManager.Get<HumanManager>().selectedHuman is Colonist) {
								Colonist selectedColonist = (Colonist)GameManager.Get<HumanManager>().selectedHuman;
								NeedInstance needInstance = selectedColonist.needs.Find(need => need.prefab == needPrefab);
								if (needInstance != null) {
									if (float.TryParse(parameters[1], out float newNeedValue)) {
										if (newNeedValue is >= 0 and <= 100) {
											needInstance.SetValue(newNeedValue);
											if (Mathf.Approximately(needInstance.GetValue(), newNeedValue)) {
												Output($"SUCCESS: {needInstance.prefab.name} now has value {needInstance.GetValue()}.");
											} else {
												Output($"ERROR: Unable to change {needInstance.prefab.name} need value.");
											}
										} else {
											Output("ERROR: Invalid range on need value (float from 0 to 100).");
										}
									} else {
										Output("ERROR: Unable to parse need value as float.");
									}
								} else {
									Output("ERROR: Unable to find specified need on selected colonist.");
								}
							} else {
								Output("ERROR: Selected human is not a colonist.");
							}
						} else {
							Output("ERROR: No human selected.");
						}
					} else {
						Output("ERROR: Invalid need name.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.setmood,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					MoodModifierPrefab moodPrefab = null;
					foreach (MoodModifierGroup group in MoodModifierGroup.moodModifierGroups) {
						moodPrefab = group.prefabs.Find(mood => mood.name == parameters[0]);
						if (moodPrefab != null) {
							break;
						}
					}
					if (moodPrefab != null) {
						if (GameManager.Get<HumanManager>().selectedHuman != null) {
							if (GameManager.Get<HumanManager>().selectedHuman is Colonist) {
								Colonist selectedColonist = (Colonist)GameManager.Get<HumanManager>().selectedHuman;
								selectedColonist.Moods.AddMoodModifier(moodPrefab.type);
								Output($"SUCCESS: Added {moodPrefab.name} to {GameManager.Get<HumanManager>().selectedHuman}.");
							} else {
								Output("ERROR: Selected human is not a colonist.");
							}
						} else {
							Output("ERROR: No human selected.");
						}
					} else {
						Output("ERROR: Invalid mood name.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.cia,
			delegate(List<string> parameters) { commandFunctions[Commands.changeinvamt](parameters); }
		);
		commandFunctions.Add(
			Commands.changeinvamt,
			delegate(List<string> parameters) {
				if (parameters.Count == 2) {
					Resource resource = Resource.GetResources().Find(r => r.type.ToString() == parameters[0]);
					if (resource != null) {
						if (GameManager.Get<HumanManager>().selectedHuman != null || GameManager.Get<UIManagerOld>().selectedContainer != null) {
							if (GameManager.Get<HumanManager>().selectedHuman != null) {
								if (int.TryParse(parameters[1], out int amount)) {
									GameManager.Get<HumanManager>().selectedHuman.GetInventory().ChangeResourceAmount(resource, amount, false);
									Output($"SUCCESS: Added {amount} {resource.name} to {GameManager.Get<HumanManager>().selectedHuman}.");
								} else {
									Output("ERROR: Unable to parse resource amount as int.");
								}
							} else {
								Output("No colonist selected, skipping.");
							}
							if (GameManager.Get<UIManagerOld>().selectedContainer != null) {
								if (int.TryParse(parameters[1], out int amount)) {
									GameManager.Get<UIManagerOld>().selectedContainer.GetInventory().ChangeResourceAmount(resource, amount, false);
									Output($"SUCCESS: Added {amount} {resource.name} to container.");
								} else {
									Output("ERROR: Unable to parse resource amount as int.");
								}
							} else {
								Output("No container selected, skipping.");
							}
						} else {
							if (int.TryParse(parameters[1], out int amount)) {
								foreach (Colonist colonist in Colonist.colonists) {
									colonist.GetInventory().ChangeResourceAmount(resource, amount, false);
								}
								foreach (Container container in Container.containers) {
									container.GetInventory().ChangeResourceAmount(resource, amount, false);
								}
								Output($"SUCCESS: Added {amount} {resource.name} to all colonists and all containers.");
							} else {
								Output("ERROR: Unable to parse resource amount as int.");
							}
						}
					} else {
						Output("ERROR: Invalid resource name.");
					}
				} else if (parameters.Count == 3) {
					if (bool.TryParse(parameters[2], out bool allColonists)) {
						if (allColonists) {
							Resource resource = Resource.GetResources().Find(r => r.type.ToString() == parameters[0]);
							if (resource != null) {
								if (int.TryParse(parameters[1], out int amount)) {
									foreach (Colonist colonist in Colonist.colonists) {
										colonist.GetInventory().ChangeResourceAmount(resource, amount, false);
									}
								} else {
									Output("ERROR: Unable to parse resource amount as int.");
								}
							} else {
								Output("ERROR: Invalid resource name.");
							}
						}
					} else {
						Output("ERROR: Unable to parse allColonists parameter as bool.");
					}
				} else if (parameters.Count == 4) {
					if (bool.TryParse(parameters[2], out bool allColonists)) {
						if (allColonists) {
							Resource resource = Resource.GetResources().Find(r => r.type.ToString() == parameters[0]);
							if (resource != null) {
								if (int.TryParse(parameters[1], out int amount)) {
									foreach (Colonist colonist in Colonist.colonists) {
										colonist.GetInventory().ChangeResourceAmount(resource, amount, false);
									}
								} else {
									Output("ERROR: Unable to parse resource amount as int.");
								}
							} else {
								Output("ERROR: Invalid resource name.");
							}
						}
					} else {
						Output("ERROR: Unable to parse allColonists parameter as bool.");
					}
					if (bool.TryParse(parameters[3], out bool allContainers)) {
						if (allContainers) {
							Resource resource = Resource.GetResources().Find(r => r.type.ToString() == parameters[0]);
							if (resource != null) {
								if (int.TryParse(parameters[1], out int amount)) {
									foreach (Container container in Container.containers) {
										container.GetInventory().ChangeResourceAmount(resource, amount, false);
									}
								} else {
									Output("ERROR: Unable to parse resource amount as int.");
								}
							} else {
								Output("ERROR: Invalid resource name.");
							}
						}
					} else {
						Output("ERROR: Unable to parse allContainers parameter as bool.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.listresources,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (Resource resource in Resource.GetResources()) {
						Output(resource.type.ToString());
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.selectcontainer,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					if (int.TryParse(parameters[0], out int index)) {
						if (index >= 0 && index < Container.containers.Count) {
							GameManager.Get<UIManagerOld>().SetSelectedContainer(Container.containers[index]);
						} else {
							Output("ERROR: Container index out of range.");
						}
					} else {
						Output("ERROR: Unable to parse container index as int.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.selecttiles,
			delegate(List<string> parameters) {
				int count = selectedTiles.Count;
				if (parameters.Count == 2) {
					if (int.TryParse(parameters[0], out int startX)) {
						if (int.TryParse(parameters[1], out int startY)) {
							if (startX >= 0 && startY >= 0 && startX < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && startY < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize) {
								selectedTiles.Add(GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(startX, startY)));
							} else {
								Output("ERROR: Positions out of range.");
							}
						} else {
							if (parameters[1].Split(',').ToList().Count > 0) {
								string startYString = parameters[1].Split(',')[0];
								string endYString = parameters[1].Split(',')[1];
								if (int.TryParse(startYString, out startY) && int.TryParse(endYString, out int endY)) {
									if (startX >= 0 && startY >= 0 && endY >= 0 && startX < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && startY < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && endY < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize) {
										if (startY <= endY) {
											for (int y = startY; y <= endY; y++) {
												selectedTiles.Add(GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(startX, y)));
											}
										} else {
											Output("ERROR: Starting y position is greater than ending y position.");
										}
									} else {
										Output("ERROR: Positions out of range.");
									}
								} else {
									Output("ERROR: Unable to parse y positions.");
								}
							} else {
								Output("ERROR: Unable to parse y as single position or split into two positions by a comma.");
							}
						}
					} else {
						if (parameters[0].Split(',').ToList().Count > 0) {
							string startXString = parameters[0].Split(',')[0];
							string endXString = parameters[0].Split(',')[1];
							if (int.TryParse(startXString, out startX) && int.TryParse(endXString, out int endX)) {
								if (int.TryParse(parameters[1], out int startY)) {
									if (startX >= 0 && startY >= 0 && endX >= 0 && startX < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && startY < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && endX < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize) {
										if (startX <= endX) {
											for (int x = startX; x <= endX; x++) {
												selectedTiles.Add(GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(x, startY)));
											}
										} else {
											Output("ERROR: Starting x position is greater than ending x position.");
										}
									} else {
										Output("ERROR: Positions out of range.");
									}
								} else {
									if (parameters[1].Split(',').ToList().Count > 0) {
										string startYString = parameters[1].Split(',')[0];
										string endYString = parameters[1].Split(',')[1];
										if (int.TryParse(startYString, out startY) && int.TryParse(endYString, out int endY)) {
											if (startX >= 0 && startY >= 0 && endX >= 0 && endY >= 0 && startX < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && startY < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && endX < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize && endY < GameManager.Get<ColonyManager>().colony.map.mapData.mapSize) {
												if (startX <= endX && startY <= endY) {
													for (int y = startY; y <= endY; y++) {
														for (int x = startX; x <= endX; x++) {
															selectedTiles.Add(GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(x, y)));
														}
													}
												} else {
													Output("ERROR: Starting x or y position is greater than ending x or y position.");
												}
											} else {
												Output("ERROR: Positions out of range.");
											}
										} else {
											Output("ERROR: Unable to parse y positions.");
										}
									} else {
										Output("ERROR: Unable to parse y as single position or split into two positions by a comma.");
									}
								}
							} else {
								Output("ERROR: Unable to parse x positions.");
							}
						} else {
							Output("ERROR: Unable to parse x as single position or split into two positions by a comma.");
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				selectedTiles = selectedTiles.Distinct().ToList();
				Output($"Selected {selectedTiles.Count - count} tiles.");
			}
		);
		commandFunctions.Add(
			Commands.selecttilemouse,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					selectTileMouseToggle = !selectTileMouseToggle;
					Output($"State: {(selectTileMouseToggle ? "On" : "Off")}");
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewtileproperty,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					foreach (TileManager.Tile tile in selectedTiles) {
						Type type = tile.GetType();
						PropertyInfo propertyInfo = type.GetProperty(parameters[0]);
						if (propertyInfo != null) {
							object value = propertyInfo.GetValue(tile, null);
							if (value != null) {
								Output($"{tile.position}: \"{value}\"");
							} else {
								Output($"ERROR: Value is null on propertyInfo for {tile.position}.");
							}
						} else {
							FieldInfo fieldInfo = type.GetField(parameters[0]);
							if (fieldInfo != null) {
								object value = fieldInfo.GetValue(tile);
								if (value != null) {
									Output($"{tile.position}: \"{value}\"");
								} else {
									Output($"ERROR: Value is null on fieldInfo for {tile.position}.");
								}
							} else {
								Output($"ERROR: propertyInfo/fieldInfo is null on type for ({parameters[0]}).");
							}
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.deselecttiles,
			delegate(List<string> parameters) {
				int selectedTilesCount = selectedTiles.Count;
				if (parameters.Count == 0) {
					selectedTiles.Clear();
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Deselected {selectedTilesCount} tiles.");
			}
		);
		commandFunctions.Add(
			Commands.deselectunwalkable,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 0) {
					List<TileManager.Tile> deselectedTiles = new();
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
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Deselected {counter} unwalkable tiles.");
			}
		);
		commandFunctions.Add(
			Commands.deselectwalkable,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 0) {
					List<TileManager.Tile> deselectedTiles = new();
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
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Deselected {counter} walkable tiles.");
			}
		);
		commandFunctions.Add(
			Commands.deselectunbuildable,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 0) {
					List<TileManager.Tile> deselectedTiles = new();
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
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Deselected {counter} unbuildable tiles.");
			}
		);
		commandFunctions.Add(
			Commands.deselectbuildable,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 0) {
					List<TileManager.Tile> deselectedTiles = new();
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
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Deselected {counter} buildable tiles.");
			}
		);
		commandFunctions.Add(
			Commands.changetiletype,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 1) {
					TileManager.TileType tileType = TileManager.TileType.GetTileTypeByString(parameters[0]);
					if (tileType != null) {
						if (selectedTiles.Count > 0) {
							foreach (TileManager.Tile tile in selectedTiles) {
								bool previousWalkableState = tile.walkable;
								tile.SetTileType(tileType, false, true, true);
								if (!previousWalkableState && tile.walkable) {
									GameManager.Get<ColonyManager>().colony.map.RemoveTileBrightnessEffect(tile);
								} else if (previousWalkableState && !tile.walkable) {
									GameManager.Get<ColonyManager>().colony.map.RecalculateLighting(new List<TileManager.Tile> { tile }, true);
								}
								counter += 1;
							}
						} else {
							Output("No tiles are currently selected.");
						}
					} else {
						Output("ERROR: Unable to parse tile type.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Changed {counter} tile types.");
			}
		);
		commandFunctions.Add(
			Commands.listtiletypes,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.TileType tileType in TileManager.TileType.tileTypes) {
						Output(tileType.type.ToString());
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.changetileobj,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 3) {
					ObjectPrefab objectPrefab = ObjectPrefab.GetObjectPrefabByString(parameters[0]);
					if (objectPrefab != null) {
						// Variation doesn't need a null check because everything is designed around it working regardless of whether it's null or not
						Variation variation = objectPrefab.GetVariationFromString(parameters[1]);

						if (int.TryParse(parameters[1], out int rotation)) {
							if (rotation >= 0 && rotation < objectPrefab.GetBitmaskSpritesForVariation(variation).Count) {
								if (selectedTiles.Count > 0) {
									foreach (TileManager.Tile tile in selectedTiles) {
										if (tile.objectInstances.ContainsKey(objectPrefab.layer) && tile.objectInstances[objectPrefab.layer] != null) {
											ObjectInstance.RemoveObjectInstance(tile.objectInstances[objectPrefab.layer]);
											tile.RemoveObjectAtLayer(objectPrefab.layer);
										}
										tile.SetObject(ObjectInstance.CreateObjectInstance(objectPrefab, variation, tile, rotation, true));
										tile.GetObjectInstanceAtLayer(objectPrefab.layer).FinishCreation();
										counter += 1;
									}
								} else {
									Output("No tiles are currently selected.");
								}
							} else {
								Output("ERROR: Rotation index is out of range.");
							}
						} else {
							Output("ERROR: Unable to parse rotation index.");
						}
					} else {
						Output("ERROR: Unable to parse tile object.");
					}
				} else if (parameters.Count == 2) {
					ObjectPrefab objectPrefab = ObjectPrefab.GetObjectPrefabByString(parameters[0]);
					if (objectPrefab != null) {
						// Variation doesn't need a null check because everything is designed around it working regardless of whether it's null or not
						Variation variation = objectPrefab.GetVariationFromString(parameters[1]);

						if (selectedTiles.Count > 0) {
							foreach (TileManager.Tile tile in selectedTiles) {
								if (tile.objectInstances.ContainsKey(objectPrefab.layer) && tile.objectInstances[objectPrefab.layer] != null) {
									ObjectInstance.RemoveObjectInstance(tile.objectInstances[objectPrefab.layer]);
									tile.RemoveObjectAtLayer(objectPrefab.layer);
								}
								tile.SetObject(ObjectInstance.CreateObjectInstance(objectPrefab, variation, tile, 0, true));
								tile.GetObjectInstanceAtLayer(objectPrefab.layer).FinishCreation();
								counter += 1;
							}
						} else {
							Output("No tiles are currently selected.");
						}
					} else {
						Output("ERROR: Unable to parse tile object.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				GameManager.Get<ColonyManager>().colony.map.RemoveTileBrightnessEffect(null);
				Output($"Changed {counter} tile objects.");
			}
		);
		commandFunctions.Add(
			Commands.removetileobj,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 0) {
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							List<ObjectInstance> removeTOIs = new();
							foreach (KeyValuePair<int, ObjectInstance> toiKVP in tile.objectInstances) {
								removeTOIs.Add(toiKVP.Value);
							}
							foreach (ObjectInstance toi in removeTOIs) {
								ObjectInstance.RemoveObjectInstance(tile.objectInstances[toi.prefab.layer]);
								tile.RemoveObjectAtLayer(toi.prefab.layer);
							}
							counter += 1;
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else if (parameters.Count == 1) {
					if (int.TryParse(parameters[0], out int layer)) {
						if (selectedTiles.Count > 0) {
							foreach (TileManager.Tile tile in selectedTiles) {
								if (tile.objectInstances.ContainsKey(layer) && tile.objectInstances[layer] != null) {
									ObjectInstance.RemoveObjectInstance(tile.objectInstances[layer]);
									tile.RemoveObjectAtLayer(layer);
									counter += 1;
								}
							}
						} else {
							Output("No tiles are currently selected.");
						}
					} else {
						Output("ERROR: Unable to parse layer as int.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Removed {counter} tile objects.");
			}
		);
		commandFunctions.Add(
			Commands.listtileobjs,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (ObjectPrefab top in ObjectPrefab.GetObjectPrefabs()) {
						Output(top.type.ToString());
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.changetileplant,
			delegate(List<string> parameters) {
				int counter = 0;
				if (parameters.Count == 2) {
					PlantPrefab plantPrefab = PlantPrefab.GetPlantPrefabByString(parameters[0]);
					if (plantPrefab != null) {
						if (bool.TryParse(parameters[1], out bool small)) {
							if (selectedTiles.Count > 0) {
								foreach (TileManager.Tile tile in selectedTiles) {
									tile.SetPlant(false, new Plant(plantPrefab, tile, small, true, null));
									GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
									if (tile.plant != null) {
										counter += 1;
									}
								}
							} else {
								Output("No tiles are currently selected.");
							}
						} else {
							Output("ERROR: Unable to parse small bool.");
						}
					} else {
						Output("ERROR: Unable to parse plant type.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Changed {counter} tile plants.");
			}
		);
		commandFunctions.Add(
			Commands.removetileplant,
			delegate(List<string> parameters) {
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
						Output("No tiles are currently selected.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
				Output($"Removed {counter} tile plants.");
			}
		);
		commandFunctions.Add(
			Commands.listtileplants,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (PlantPrefab plantGroup in PlantPrefab.GetPlantPrefabs()) {
						Output(plantGroup.type.ToString());
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.campos,
			delegate(List<string> parameters) {
				if (parameters.Count == 2) {
					if (int.TryParse(parameters[0], out int camX)) {
						if (int.TryParse(parameters[1], out int camY)) {
							GameManager.Get<CameraManager>().SetCameraPosition(new Vector2(camX, camY));
						} else {
							Output("ERROR: Unable to parse camera y position as int.");
						}
					} else {
						Output("ERROR: Unable to parse camera x position as int.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.camzoom,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					if (float.TryParse(parameters[0], out float camZoom)) {
						GameManager.Get<CameraManager>().SetCameraZoom(camZoom);
					} else {
						Output("ERROR: Unable to parse camera zoom as float.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.togglesuncycle,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					holdHour = GameManager.Get<TimeManager>().Time.TileBrightnessTime;
					toggleSunCycleToggle = !toggleSunCycleToggle;
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.settime,
			delegate(List<string> parameters) {
				if (parameters.Count == 1) {
					if (float.TryParse(parameters[0], out float time)) {
						if (time is >= 0 and < 24) {
							holdHour = time;
							GameManager.Get<TimeManager>().SetTime(time);
							GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
						} else {
							Output("ERROR: Time out of range.");
						}
					} else {
						Output("ERROR: Unable to parse time as float.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.pause,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					GameManager.Get<TimeManager>().TogglePause();
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewselectedtiles,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					if (selectedTiles.Count <= 0) {
						Output("No tiles are currently selected.");
					} else {
						foreach (TileManager.Tile tile in selectedTiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = Color.blue;
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewnormal,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.sr.color = Color.white;
					}
					GameManager.Get<ColonyManager>().colony.map.Bitmasking(GameManager.Get<ColonyManager>().colony.map.tiles, true, true);
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewregions,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Map.Region region in GameManager.Get<ColonyManager>().colony.map.regions) {
						Color colour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
						foreach (TileManager.Tile tile in region.tiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = colour;
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewheightmap,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
						tile.sr.color = new Color(tile.height, tile.height, tile.height, 1f);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewprecipitation,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
						tile.sr.color = new Color(tile.GetPrecipitation(), tile.GetPrecipitation(), tile.GetPrecipitation(), 1f);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewtemperature,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
						tile.sr.color = new Color((tile.temperature + 50f) / 100f, (tile.temperature + 50f) / 100f, (tile.temperature + 50f) / 100f, 1f);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewbiomes,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
						tile.sr.color = tile.biome?.colour ?? Color.black;
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewresourceveins,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					GameManager.Get<ColonyManager>().colony.map.Bitmasking(GameManager.Get<ColonyManager>().colony.map.tiles, false, true);
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						if (tile.tileType.resourceRanges.Find(rr => TileManager.ResourceVein.resourceVeinValidTileFunctions.ContainsKey(rr.resource.type)) == null) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = Color.white;
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewdrainagebasins,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (KeyValuePair<TileManager.Map.Region, TileManager.Tile> kvp in GameManager.Get<ColonyManager>().colony.map.drainageBasins) {
						Color colour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
						foreach (TileManager.Tile tile in kvp.Key.tiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = colour;
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewrivers,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Map.River river in GameManager.Get<ColonyManager>().colony.map.rivers) {
						Color riverColour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
						foreach (TileManager.Tile tile in river.tiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = riverColour;
						}
						river.tiles[0].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
						river.endTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
						river.tiles[^1].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
						river.startTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
					}
					Output($"Showing {GameManager.Get<ColonyManager>().colony.map.rivers.Count} rivers.");
				} else if (parameters.Count == 1) {
					if (int.TryParse(parameters[0], out viewRiverAtIndex)) {
						if (viewRiverAtIndex >= 0 && viewRiverAtIndex < GameManager.Get<ColonyManager>().colony.map.rivers.Count) {
							Color riverColour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
							foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.rivers[viewRiverAtIndex].tiles) {
								tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
								tile.sr.color = riverColour;
							}
							GameManager.Get<ColonyManager>().colony.map.rivers[viewRiverAtIndex].tiles[0].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
							GameManager.Get<ColonyManager>().colony.map.rivers[viewRiverAtIndex].endTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
							GameManager.Get<ColonyManager>().colony.map.rivers[viewRiverAtIndex].tiles[^1].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
							GameManager.Get<ColonyManager>().colony.map.rivers[viewRiverAtIndex].startTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
							Output($"Showing river {viewRiverAtIndex + 1} of {GameManager.Get<ColonyManager>().colony.map.rivers.Count} rivers.");
						} else {
							Output("ERROR: River index out of range.");
						}
					} else {
						Output("ERROR: Unable to parse river index as int.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewlargerivers,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Map.River river in GameManager.Get<ColonyManager>().colony.map.largeRivers) {
						Color riverColour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
						foreach (TileManager.Tile tile in river.tiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = riverColour;
						}
						river.tiles[0].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
						river.endTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
						river.tiles[^1].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
						river.startTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
						river.centreTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightBlue);
					}
					Output($"Showing {GameManager.Get<ColonyManager>().colony.map.largeRivers.Count} large rivers.");
				} else if (parameters.Count == 1) {
					if (int.TryParse(parameters[0], out viewRiverAtIndex)) {
						if (viewRiverAtIndex >= 0 && viewRiverAtIndex < GameManager.Get<ColonyManager>().colony.map.largeRivers.Count) {
							Color riverColour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
							foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.largeRivers[viewRiverAtIndex].tiles) {
								tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
								tile.sr.color = riverColour;
							}
							GameManager.Get<ColonyManager>().colony.map.largeRivers[viewRiverAtIndex].tiles[0].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
							GameManager.Get<ColonyManager>().colony.map.largeRivers[viewRiverAtIndex].endTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
							GameManager.Get<ColonyManager>().colony.map.largeRivers[viewRiverAtIndex].tiles[^1].sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen);
							GameManager.Get<ColonyManager>().colony.map.largeRivers[viewRiverAtIndex].startTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen);
							GameManager.Get<ColonyManager>().colony.map.largeRivers[viewRiverAtIndex].centreTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.LightBlue);
							Output($"Showing river {viewRiverAtIndex + 1} of {GameManager.Get<ColonyManager>().colony.map.largeRivers.Count} large rivers.");
						} else {
							Output("ERROR: River index out of range.");
						}
					} else {
						Output("ERROR: Unable to parse river index as int.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewwalkspeed,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
						tile.sr.color = new Color(tile.walkSpeed, tile.walkSpeed, tile.walkSpeed, 1f);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewregionblocks,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Map.RegionBlock region in GameManager.Get<ColonyManager>().colony.map.regionBlocks) {
						Color colour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
						foreach (TileManager.Tile tile in region.tiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = colour;
						}
						TileManager.Tile averageTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(region.averagePosition);
						averageTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.White);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewsquareregionblocks,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Map.RegionBlock region in GameManager.Get<ColonyManager>().colony.map.squareRegionBlocks) {
						Color colour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
						foreach (TileManager.Tile tile in region.tiles) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = colour;
						}
						TileManager.Tile averageTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(region.averagePosition);
						averageTile.sr.color = ColourUtilities.GetColour(ColourUtilities.EColour.White);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewlightblockingtiles,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.SetVisible(true);
						if (tile.blocksLight) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = Color.red;
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewshadowstarttiles,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.DetermineShadowSourceTiles(GameManager.Get<ColonyManager>().colony.map.tiles)) {
						tile.SetVisible(true);
						tile.sr.color = Color.red;
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewshadowsfrom,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							foreach (KeyValuePair<int, Dictionary<TileManager.Tile, float>> shadowsFromKVP in tile.shadowsFrom) {
								foreach (KeyValuePair<TileManager.Tile, float> sTile in shadowsFromKVP.Value) {
									sTile.Key.SetColour(new Color(shadowsFromKVP.Key / 24f, 1 - shadowsFromKVP.Key / 24f, shadowsFromKVP.Key / 24f, 1f), 12);
								}
							}
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else if (parameters.Count == 1) {
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
					if (selectedTiles.Count > 0) {
						if (int.TryParse(parameters[0], out int hour)) {
							if (hour is >= 0 and <= 23) {
								foreach (TileManager.Tile tile in selectedTiles) {
									foreach (KeyValuePair<TileManager.Tile, float> sTile in tile.shadowsFrom[hour]) {
										sTile.Key.SetColour(new Color(hour / 24f, 1 - hour / 24f, hour / 24f, 1f), 12);
									}
								}
							} else {
								Output("ERROR: Hour out of range (0 [inclusive] to 23 [inclusive]).");
							}
						} else {
							Output("ERROR: Unable to parse hour as int.");
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewshadowsto,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							foreach (KeyValuePair<int, List<TileManager.Tile>> shadowsToKVP in tile.shadowsTo) {
								foreach (TileManager.Tile shadowsToTile in shadowsToKVP.Value) {
									shadowsToTile.SetColour(new Color(shadowsToKVP.Key / 24f, 1 - shadowsToKVP.Key / 24f, shadowsToKVP.Key / 24f, 1f), 12);
								}
							}
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else if (parameters.Count == 1) {
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
					if (selectedTiles.Count > 0) {
						if (int.TryParse(parameters[0], out int hour)) {
							if (hour is >= 0 and <= 23) {
								foreach (TileManager.Tile tile in selectedTiles) {
									foreach (TileManager.Tile sTile in tile.shadowsTo[hour]) {
										sTile.SetColour(new Color(hour / 24f, 1 - hour / 24f, hour / 24f, 1f), 12);
									}
								}
							} else {
								Output("ERROR: Hour out of range (0 [inclusive] to 23 [inclusive]).");
							}
						} else {
							Output("ERROR: Unable to parse hour as int.");
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewblockingfrom,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
					if (selectedTiles.Count > 0) {
						foreach (TileManager.Tile tile in selectedTiles) {
							foreach (KeyValuePair<int, List<TileManager.Tile>> blockingShadowsFromKVP in tile.blockingShadowsFrom) {
								foreach (TileManager.Tile blockingShadowsFromTile in blockingShadowsFromKVP.Value) {
									blockingShadowsFromTile.SetColour(Color.red, 12);
								}
							}
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else if (parameters.Count == 1) {
					GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
					if (selectedTiles.Count > 0) {
						if (int.TryParse(parameters[0], out int hour)) {
							if (hour is >= 0 and <= 23) {
								foreach (TileManager.Tile tile in selectedTiles) {
									foreach (TileManager.Tile blockingShadowsFromTile in tile.blockingShadowsFrom[hour]) {
										blockingShadowsFromTile.SetColour(Color.red, 12);
									}
								}
							} else {
								Output("ERROR: Hour out of range (0 [inclusive] to 23 [inclusive]).");
							}
						} else {
							Output("ERROR: Unable to parse hour as int.");
						}
					} else {
						Output("No tiles are currently selected.");
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewroofs,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.SetVisible(true);
						if (tile.HasRoof()) {
							tile.sr.sprite = GameManager.Get<ResourceManager>().whiteSquareSprite;
							tile.sr.color = Color.red;
						}
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.viewhidden,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
						tile.SetVisible(true);
					}
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.listjobs,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					foreach (Colonist colonist in Colonist.colonists) {
						Output(colonist.name);
						foreach (Job job in JobManager.GetSortedJobs(colonist)) {
							Output($"\t{job.objectPrefab.jobType} {job.objectPrefab.type} {JobManager.CalculateJobCost(colonist, job, null)}");
						}
					}
				} else if (parameters.Count == 1) {
					Output("ERROR: Not yet implemented.");
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
		commandFunctions.Add(
			Commands.growfarms,
			delegate(List<string> parameters) {
				if (parameters.Count == 0) {
					int numFarmsGrown = 0;
					foreach (Farm farm in Farm.farms) {
						farm.growTimer = farm.prefab.growthTimeDays * (SimulationDateTime.DayLengthSeconds - 1);
						farm.Update();
						numFarmsGrown += 1;
					}
					Output($"{numFarmsGrown} farms have been grown.");
				} else {
					Output("ERROR: Invalid number of parameters specified.");
				}
			}
		);
	}
}