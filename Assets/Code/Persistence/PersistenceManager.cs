﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NPlanet;
using Snowship.NResource;
using Snowship.NState;
using Snowship.NUI;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PersistenceManager : IManager {

		public static readonly (int increment, string text) GameVersion = (3, "2025.1");
		public static readonly (int increment, string text) SaveVersion = (3, "2025.1");

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

		public async UniTask CreateSave(Colony colony) {
			string savesDirectoryPath = colony.directory + "/Saves";
			string dateTimeString = PersistenceUtilities.GenerateDateTimeString();
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

				string lastSaveDateTime = PersistenceUtilities.GenerateSaveDateTimeString();
				string lastSaveTimeChunk = PersistenceUtilities.GenerateDateTimeString();

				GameManager.Get<UniverseManager>().universe.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
				PUniverse.UpdateUniverseSave(GameManager.Get<UniverseManager>().universe);

				GameManager.Get<PlanetManager>().planet.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
				PPlanet.UpdatePlanetSave(GameManager.Get<PlanetManager>().planet);

				colony.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
				PColony.UpdateColonySave(GameManager.Get<ColonyManager>().colony);

				PSave.SaveSave(saveDirectoryPath, lastSaveDateTime);

				await PU.CreateScreenshot(saveDirectoryPath + "/screenshot-" + dateTimeString);

				PLastSave.UpdateLastSave(
					new PLastSave.LastSaveProperties(
						GameManager.Get<UniverseManager>().universe.directory,
						GameManager.Get<PlanetManager>().planet.directory,
						GameManager.Get<ColonyManager>().colony.directory,
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
				GameManager.Get<TileManager>().mapState = TileManager.MapState.Generating;

				GameManager.Get<UIManagerOld>().SetGameUIActive(false);

				UIEvents.UpdateLoadingScreenText("Loading Colony", string.Empty);
				yield return null;
				GameManager.Get<ColonyManager>().LoadColony(GameManager.Get<ColonyManager>().colony, false);

				if (persistenceSave.path == null) {
					throw new Exception("persistenceSave.path is null");
				}

				string saveDirectoryPath = Directory.GetParent(persistenceSave.path)?.FullName;

				loadingState = LoadingState.LoadingCamera;
				UIEvents.UpdateLoadingScreenText("Loading Camera", string.Empty);
				yield return null;
				PCamera.LoadCamera(saveDirectoryPath + "/camera.snowship");
				while (loadingState != LoadingState.LoadedCamera) {
					yield return null;
				}

				loadingState = LoadingState.LoadingTime;
				UIEvents.UpdateLoadingScreenText("Loading Time", string.Empty);
				yield return null;
				PTime.LoadTime(saveDirectoryPath + "/time.snowship");
				while (loadingState != LoadingState.LoadedTime) {
					yield return null;
				}

				loadingState = LoadingState.LoadingResources;
				UIEvents.UpdateLoadingScreenText("Loading Resources", string.Empty);
				yield return null;
				PResource.LoadResources(saveDirectoryPath + "/resources.snowship");
				while (loadingState != LoadingState.LoadedResources) {
					yield return null;
				}

				loadingState = LoadingState.LoadingMap;
				UIEvents.UpdateLoadingScreenText("Loading Original Map", string.Empty);
				yield return null;
				GameManager.Get<ColonyManager>().colony.map = new TileManager.Map() { mapData = GameManager.Get<ColonyManager>().colony.mapData };
				TileManager.Map map = GameManager.Get<ColonyManager>().colony.map;

				List<PersistenceTile> originalTiles = PMap.LoadTiles(GameManager.Get<ColonyManager>().colony.directory + "/Map/tiles.snowship");
				List<PersistenceRiver> originalRivers = PRiver.LoadRivers(GameManager.Get<ColonyManager>().colony.directory + "/Map/rivers.snowship");

				UIEvents.UpdateLoadingScreenText("Loading Modified Map", string.Empty);
				yield return null;
				List<PersistenceTile> modifiedTiles = PMap.LoadTiles(saveDirectoryPath + "/tiles.snowship");
				List<PersistenceRiver> modifiedRivers = PRiver.LoadRivers(saveDirectoryPath + "/rivers.snowship");

				UIEvents.UpdateLoadingScreenText("Applying Changes to Map", string.Empty);
				yield return null;
				PMap.ApplyLoadedTiles(originalTiles, modifiedTiles, map);
				PRiver.ApplyLoadedRivers(originalRivers, modifiedRivers, map);
				while (loadingState != LoadingState.LoadedMap) {
					yield return null;
				}

				loadingState = LoadingState.LoadingObjects;
				UIEvents.UpdateLoadingScreenText("Loading Object Data", string.Empty);
				yield return null;
				List<PObject.PersistenceObject> persistenceObjects = PObject.LoadObjects(saveDirectoryPath + "/objects.snowship");
				PObject.ApplyLoadedObjects(persistenceObjects);
				while (loadingState != LoadingState.LoadedObjects) {
					yield return null;
				}

				loadingState = LoadingState.LoadingCaravans;
				UIEvents.UpdateLoadingScreenText("Loading Caravan Data", string.Empty);
				yield return null;
				List<PCaravan.PersistenceCaravan> persistenceCaravans = PCaravan.LoadCaravans(saveDirectoryPath + "/caravans.snowship");
				PCaravan.ApplyLoadedCaravans(persistenceCaravans);
				while (loadingState != LoadingState.LoadedCaravans) {
					yield return null;
				}

				loadingState = LoadingState.LoadingJobs;
				UIEvents.UpdateLoadingScreenText("Loading Job Data", string.Empty);
				yield return null;
				List<PJob.PersistenceJob> persistenceJobs = PJob.LoadJobs(saveDirectoryPath + "/jobs.snowship");
				PJob.ApplyLoadedJobs(persistenceJobs);
				while (loadingState != LoadingState.LoadedJobs) {
					yield return null;
				}

				loadingState = LoadingState.LoadingColonists;
				UIEvents.UpdateLoadingScreenText("Loading Colonist Data", string.Empty);
				yield return null;
				List<PColonist.PersistenceColonist> persistenceColonists = PColonist.LoadColonists(saveDirectoryPath + "/colonists.snowship");
				PColonist.ApplyLoadedColonists(persistenceColonists);
				while (loadingState != LoadingState.LoadedColonists) {
					yield return null;
				}

				for (int i = 0; i < persistenceObjects.Count; i++) {
					PObject.PersistenceObject persistenceObject = persistenceObjects[i];
					ObjectInstance objectInstance = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(persistenceObject.zeroPointTilePosition.Value).objectInstances.Values.ToList().Find(o => o.prefab.type == persistenceObject.type);

					switch (objectInstance.prefab.instanceType) {
						case ObjectInstance.ObjectInstanceType.Container:
							Container container = (Container)objectInstance;
							foreach (KeyValuePair<string, List<ResourceAmount>> humanToReservedResourcesKVP in persistenceObject.persistenceInventory.reservedResources) {
								foreach (ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
									container.GetInventory().ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
								}
								container.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.Get<HumanManager>().humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
							}
							break;
						case ObjectInstance.ObjectInstanceType.CraftingObject:
							CraftingObject craftingObject = (CraftingObject)objectInstance;
							craftingObject.SetActive(persistenceObject.active.Value);
							break;
						case ObjectInstance.ObjectInstanceType.SleepSpot:
							SleepSpot sleepSpot = (SleepSpot)objectInstance;
							if (persistenceObject.occupyingColonistName != null) {
								sleepSpot.occupyingColonist = Colonist.colonists.Find(c => c.name == persistenceObject.occupyingColonistName);
							}
							break;
					}

					objectInstance.Update();
				}

				for (int i = 0; i < persistenceCaravans.Count; i++) {
					PCaravan.PersistenceCaravan persistenceCaravan = persistenceCaravans[i];
					Caravan caravan = GameManager.Get<CaravanManager>().caravans[i];

					foreach (KeyValuePair<string, List<ResourceAmount>> humanToReservedResourcesKVP in persistenceCaravan.persistenceInventory.reservedResources) {
						foreach (ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
							caravan.GetInventory().ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
						}
						caravan.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.Get<HumanManager>().humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
					}

					for (int t = 0; t < caravan.traders.Count; t++) {
						PCaravan.PersistenceTrader persistenceTrader = persistenceCaravan.persistenceTraders[t];
						Trader trader = caravan.traders[t];

						foreach (KeyValuePair<string, List<ResourceAmount>> humanToReservedResourcesKVP in persistenceTrader.persistenceHuman.persistenceInventory.reservedResources) {
							foreach (ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
								trader.GetInventory().ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
							}
							trader.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.Get<HumanManager>().humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
						}
					}
				}

				PMap.ApplyMapBitmasking(originalTiles, modifiedTiles, map);
				map.SetInitialRegionVisibility();

				loadingState = LoadingState.FinishedLoading;
				GameManager.Get<TileManager>().mapState = TileManager.MapState.Generated;
				GameManager.Get<UIManagerOld>().SetGameUIActive(true);
			} else {
				Debug.LogError("Unable to load a save without a save being selected.");
			}
		}

		public async UniTask ContinueFromMostRecentSave() {

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
			GameManager.Get<PlanetManager>().SetSelectedPlanetTile(GameManager.Get<PlanetManager>().planet.planetTiles.Find(pt => pt.tile.position == persistenceColony.planetPosition));
			PColony.ApplyLoadedColony(persistenceColony);

			await GameManager.Get<StateManager>().TransitionToState(EState.LoadToSimulation);

			PSave.PersistenceSave persistenceSave = PSave.GetPersistenceSaves().Find(ps => string.Equals(Path.GetFullPath(ps.path), Path.GetFullPath(lastSaveProperties.lastSaveSavePath + "/save.snowship"), StringComparison.OrdinalIgnoreCase));
			await GameManager.Get<PersistenceManager>().ApplyLoadedSave(persistenceSave);

			await GameManager.Get<StateManager>().TransitionToState(EState.Simulation);
		}
	}
}