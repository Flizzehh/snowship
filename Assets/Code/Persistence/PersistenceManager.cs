﻿using Snowship.NJob;
using Snowship.NProfession;
using Snowship.NTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NPersistence.Save;
using Snowship.NPlanet;
using Snowship.NResource.Models;
using Snowship.NState;
using Snowship.NUtilities;
using UnityEngine;
using Time = Snowship.NTime;

namespace Snowship.NPersistence {
	public class PersistenceManager : IManager {

		private GameManager startCoroutineReference;

		public void SetStartCoroutineReference(GameManager startCoroutineReference) {
			this.startCoroutineReference = startCoroutineReference;
		}

		public static readonly (int increment, string text) gameVersion = (3, "2025.1");
		public static readonly (int increment, string text) saveVersion = (3, "2025.1");

		private readonly PersistenceHandler persistenceHandler = new PersistenceHandler();

		public PSettings PSettings { get; } = new PSettings();
		public PLastSave PLastSave { get; } = new PLastSave();
		public PUniverse PUniverse { get; } = new PUniverse();
		public PPlanet PPlanet { get; } = new PPlanet();
		public PMap PMap { get; } = new PMap();
		public PRiver PRiver { get; } = new PRiver();
		public PObject PObject { get; } = new PObject();
		public PTime PTime { get; } = new PTime();
		public PResource PResource { get; } = new PResource();

		public PColony PColony { get; } = new PColony();
		public PLife PLife { get; } = new PLife();
		public PHuman PHuman { get; } = new PHuman();
		public PInventory PInventory { get; } = new PInventory();

		public PColonist PColonist { get; } = new PColonist();
		public PJob PJob { get; } = new PJob();

		public PCaravan PCaravan { get; } = new PCaravan();

		public PCamera PCamera { get; } = new PCamera();
		public PUI PUI { get; } = new PUI();

		public PSave PSave { get; } = new PSave();

		// Game Saving

		public void CreateSave(Colony colony) {
			string savesDirectoryPath = colony.directory + "/Saves";
			string dateTimeString = PersistenceHandler.GenerateDateTimeString();
			string saveDirectoryPath = savesDirectoryPath + "/Save-" + dateTimeString;

			try {
				Directory.CreateDirectory(saveDirectoryPath);

				PCamera.SaveCamera(saveDirectoryPath);
				PCaravan.SaveCaravans(saveDirectoryPath);
				PColonist.SaveColonists(saveDirectoryPath);
				PJob.SaveJobs(saveDirectoryPath);
				PObject.SaveObjects(saveDirectoryPath);
				PResource.SaveResources(saveDirectoryPath);
				PRiver.SaveModifiedRivers(saveDirectoryPath, PRiver.LoadRivers(colony.directory + "/Map/rivers.snowship"));
				PMap.SaveModifiedTiles(saveDirectoryPath, PMap.LoadTiles(colony.directory + "/Map/tiles.snowship"));
				PTime.SaveTime(saveDirectoryPath);
				PUI.SaveUI(saveDirectoryPath);

				string lastSaveDateTime = PersistenceHandler.GenerateSaveDateTimeString();
				string lastSaveTimeChunk = PersistenceHandler.GenerateDateTimeString();

				GameManager.universeM.universe.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
				PUniverse.UpdateUniverseSave(GameManager.universeM.universe);

				GameManager.planetM.planet.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
				PPlanet.UpdatePlanetSave(GameManager.planetM.planet);

				colony.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
				PColony.UpdateColonySave(GameManager.colonyM.colony);

				PSave.SaveSave(saveDirectoryPath, lastSaveDateTime);

				startCoroutineReference.StartCoroutine(persistenceHandler.CreateScreenshot(saveDirectoryPath + "/screenshot-" + dateTimeString));

				PLastSave.UpdateLastSave(
					new PLastSave.LastSaveProperties(
						GameManager.universeM.universe.directory,
						GameManager.planetM.planet.directory,
						GameManager.colonyM.colony.directory,
						saveDirectoryPath
					));
			} catch (Exception e) {
				throw e;
			}
		}

		public enum LoadingState {
			NothingLoaded,
			LoadingCamera,
			LoadedCamera,
			LoadingTime,
			LoadedTime,
			LoadingResources,
			LoadedResources,
			LoadingMap,
			LoadedMap,
			LoadingObjects,
			LoadedObjects,
			LoadingCaravans,
			LoadedCaravans,
			LoadingJobs,
			LoadedJobs,
			LoadingColonists,
			LoadedColonists,
			LoadingUI,
			LoadedUI,
			FinishedLoading
		}

		public LoadingState loadingState;

		public IEnumerator ApplyLoadedSave(PSave.PersistenceSave persistenceSave) {
			loadingState = LoadingState.NothingLoaded;
			if (persistenceSave != null) {
				GameManager.tileM.mapState = TileManager.MapState.Generating;

				GameManager.uiMOld.SetLoadingScreenActive(true);
				GameManager.uiMOld.SetGameUIActive(false);

				GameManager.uiMOld.UpdateLoadingStateText("Loading Colony", string.Empty);
				yield return null;
				GameManager.colonyM.LoadColony(GameManager.colonyM.colony, false);

				if (persistenceSave.path == null) {
					throw new Exception("persistenceSave.path is null");
				}

				string saveDirectoryPath = Directory.GetParent(persistenceSave.path)?.FullName;

				GameManager.timeM.SetPaused(true);

				loadingState = LoadingState.LoadingCamera;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Camera", string.Empty);
				yield return null;
				PCamera.LoadCamera(saveDirectoryPath + "/camera.snowship");
				while (loadingState != LoadingState.LoadedCamera) {
					yield return null;
				}

				loadingState = LoadingState.LoadingTime;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Time", string.Empty);
				yield return null;
				PTime.LoadTime(saveDirectoryPath + "/time.snowship");
				while (loadingState != LoadingState.LoadedTime) {
					yield return null;
				}

				loadingState = LoadingState.LoadingResources;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Resources", string.Empty);
				yield return null;
				PResource.LoadResources(saveDirectoryPath + "/resources.snowship");
				while (loadingState != LoadingState.LoadedResources) {
					yield return null;
				}

				loadingState = LoadingState.LoadingMap;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Original Map", string.Empty);
				yield return null;
				GameManager.colonyM.colony.map = new TileManager.Map() { mapData = GameManager.colonyM.colony.mapData };
				TileManager.Map map = GameManager.colonyM.colony.map;

				List<PersistenceTile> originalTiles = PMap.LoadTiles(GameManager.colonyM.colony.directory + "/Map/tiles.snowship");
				List<PersistenceRiver> originalRivers = PRiver.LoadRivers(GameManager.colonyM.colony.directory + "/Map/rivers.snowship");

				GameManager.uiMOld.UpdateLoadingStateText("Loading Modified Map", string.Empty);
				yield return null;
				List<PersistenceTile> modifiedTiles = PMap.LoadTiles(saveDirectoryPath + "/tiles.snowship");
				List<PersistenceRiver> modifiedRivers = PRiver.LoadRivers(saveDirectoryPath + "/rivers.snowship");

				GameManager.uiMOld.UpdateLoadingStateText("Applying Changes to Map", string.Empty);
				yield return null;
				PMap.ApplyLoadedTiles(originalTiles, modifiedTiles, map);
				PRiver.ApplyLoadedRivers(originalRivers, modifiedRivers, map);
				while (loadingState != LoadingState.LoadedMap) {
					yield return null;
				}

				loadingState = LoadingState.LoadingObjects;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Object Data", string.Empty);
				yield return null;
				List<PObject.PersistenceObject> persistenceObjects = PObject.LoadObjects(saveDirectoryPath + "/objects.snowship");
				PObject.ApplyLoadedObjects(persistenceObjects);
				while (loadingState != LoadingState.LoadedObjects) {
					yield return null;
				}

				loadingState = LoadingState.LoadingCaravans;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Caravan Data", string.Empty);
				yield return null;
				List<PCaravan.PersistenceCaravan> persistenceCaravans = PCaravan.LoadCaravans(saveDirectoryPath + "/caravans.snowship");
				PCaravan.ApplyLoadedCaravans(persistenceCaravans);
				while (loadingState != LoadingState.LoadedCaravans) {
					yield return null;
				}

				loadingState = LoadingState.LoadingJobs;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Job Data", string.Empty);
				yield return null;
				List<PJob.PersistenceJob> persistenceJobs = PJob.LoadJobs(saveDirectoryPath + "/jobs.snowship");
				PJob.ApplyLoadedJobs(persistenceJobs);
				while (loadingState != LoadingState.LoadedJobs) {
					yield return null;
				}

				loadingState = LoadingState.LoadingColonists;
				GameManager.uiMOld.UpdateLoadingStateText("Loading Colonist Data", string.Empty);
				yield return null;
				List<PColonist.PersistenceColonist> persistenceColonists = PColonist.LoadColonists(saveDirectoryPath + "/colonists.snowship");
				PColonist.ApplyLoadedColonists(persistenceColonists);
				while (loadingState != LoadingState.LoadedColonists) {
					yield return null;
				}

				for (int i = 0; i < persistenceObjects.Count; i++) {
					PObject.PersistenceObject persistenceObject = persistenceObjects[i];
					ResourceManager.ObjectInstance objectInstance = GameManager.colonyM.colony.map.GetTileFromPosition(persistenceObject.zeroPointTilePosition.Value).objectInstances.Values.ToList().Find(o => o.prefab.type == persistenceObject.type);

					switch (objectInstance.prefab.instanceType) {
						case ResourceManager.ObjectInstanceType.Container:
							ResourceManager.Container container = (ResourceManager.Container)objectInstance;
							foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceObject.persistenceInventory.reservedResources) {
								foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
									container.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
								}
								container.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
							}
							break;
						case ResourceManager.ObjectInstanceType.CraftingObject:
							ResourceManager.CraftingObject craftingObject = (ResourceManager.CraftingObject)objectInstance;
							craftingObject.SetActive(persistenceObject.active.Value);
							break;
						case ResourceManager.ObjectInstanceType.SleepSpot:
							ResourceManager.SleepSpot sleepSpot = (ResourceManager.SleepSpot)objectInstance;
							if (persistenceObject.occupyingColonistName != null) {
								sleepSpot.occupyingColonist = Colonist.colonists.Find(c => c.name == persistenceObject.occupyingColonistName);
							}
							break;
					}

					objectInstance.Update();
				}

				for (int i = 0; i < persistenceCaravans.Count; i++) {
					PCaravan.PersistenceCaravan persistenceCaravan = persistenceCaravans[i];
					Caravan caravan = GameManager.caravanM.caravans[i];

					foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceCaravan.persistenceInventory.reservedResources) {
						foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
							caravan.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
						}
						caravan.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
					}

					for (int t = 0; t < caravan.traders.Count; t++) {
						PCaravan.PersistenceTrader persistenceTrader = persistenceCaravan.persistenceTraders[t];
						Trader trader = caravan.traders[t];

						foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceTrader.persistenceHuman.persistenceInventory.reservedResources) {
							foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
								trader.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
							}
							trader.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
						}
					}
				}

				PMap.ApplyMapBitmasking(originalTiles, modifiedTiles, map);
				map.SetInitialRegionVisibility();

				loadingState = LoadingState.FinishedLoading;
				GameManager.tileM.mapState = TileManager.MapState.Generated;
				GameManager.uiMOld.SetGameUIActive(true);
				GameManager.uiMOld.SetLoadingScreenActive(false);
			} else {
				Debug.LogError("Unable to load a save without a save being selected.");
			}
		}

		public void ContinueFromMostRecentSave() {

			_ = GameManager.stateM.TransitionToState(EState.LoadToSimulation);

			PLastSave.LastSaveProperties lastSaveProperties = PLastSave.GetLastSaveProperties();

			PersistenceUniverse persistenceUniverse = PUniverse.GetPersistenceUniverses().Find(pu => string.Equals(Path.GetFullPath(pu.path), Path.GetFullPath(lastSaveProperties.lastSaveUniversePath), StringComparison.OrdinalIgnoreCase));

			if (!PUniverse.IsUniverseLoadable(persistenceUniverse)) {
				return;
			}

			PUniverse.ApplyLoadedConfiguration(persistenceUniverse);
			PUniverse.ApplyLoadedUniverse(persistenceUniverse);

			PersistencePlanet persistencePlanet = PPlanet.GetPersistencePlanets().Find(pp => string.Equals(Path.GetFullPath(pp.path), Path.GetFullPath(lastSaveProperties.lastSavePlanetPath + "/planet.snowship"), StringComparison.OrdinalIgnoreCase));
			PPlanet.ApplyLoadedPlanet(persistencePlanet);

			PersistenceColony persistenceColony = PColony.GetPersistenceColonies().Find(pc => string.Equals(Path.GetFullPath(pc.path), Path.GetFullPath(lastSaveProperties.lastSaveColonyPath + "/colony.snowship"), StringComparison.OrdinalIgnoreCase));
			GameManager.planetM.SetSelectedPlanetTile(GameManager.planetM.planet.planetTiles.Find(pt => pt.tile.position == persistenceColony.planetPosition));
			PColony.ApplyLoadedColony(persistenceColony);

			PSave.PersistenceSave persistenceSave = PSave.GetPersistenceSaves().Find(ps => string.Equals(Path.GetFullPath(ps.path), Path.GetFullPath(lastSaveProperties.lastSaveSavePath + "/save.snowship"), StringComparison.OrdinalIgnoreCase));
			startCoroutineReference.StartCoroutine(GameManager.persistenceM.ApplyLoadedSave(persistenceSave));

			_ = GameManager.stateM.TransitionToState(EState.Simulation);
		}
	}
}
