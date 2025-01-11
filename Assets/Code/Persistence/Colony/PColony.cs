using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowship.NColony;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PColony : PersistenceHandler {

		private readonly PMap pMap = new PMap();
		private readonly PRiver pRiver = new PRiver();

		public enum ColonyProperty {
			LastSaveDateTime,
			LastSaveTimeChunk,
			Name,
			PlanetPosition,
			Seed,
			Size,
			AverageTemperature,
			AveragePrecipitation,
			TerrainTypeHeights,
			SurroundingPlanetTileHeights,
			OnRiver,
			SurroundingPlanetTileRivers
		}

		public void SaveColony(StreamWriter file, Colony colony) {
			file.WriteLine(CreateKeyValueString(ColonyProperty.LastSaveDateTime, colony.lastSaveDateTime, 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.LastSaveTimeChunk, colony.lastSaveTimeChunk, 0));

			file.WriteLine(CreateKeyValueString(ColonyProperty.Name, colony.name, 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.PlanetPosition, FormatVector2ToString(colony.map.mapData.planetTilePosition), 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.Seed, colony.map.mapData.mapSeed, 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.Size, colony.map.mapData.mapSize, 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.AverageTemperature, colony.map.mapData.averageTemperature, 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.AveragePrecipitation, colony.map.mapData.averagePrecipitation, 0));

			file.WriteLine(CreateKeyValueString(ColonyProperty.TerrainTypeHeights, string.Empty, 0));
			foreach (KeyValuePair<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeight in colony.map.mapData.terrainTypeHeights) {
				file.WriteLine(CreateKeyValueString(terrainTypeHeight.Key, terrainTypeHeight.Value, 1));
			}

			file.WriteLine(CreateKeyValueString(ColonyProperty.SurroundingPlanetTileHeights, string.Join(",", colony.map.mapData.surroundingPlanetTileHeightDirections.Select(i => i.ToString()).ToArray()), 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.OnRiver, colony.map.mapData.isRiver, 0));
			file.WriteLine(CreateKeyValueString(ColonyProperty.SurroundingPlanetTileRivers, string.Join(",", colony.map.mapData.surroundingPlanetTileRivers.Select(i => i.ToString()).ToArray()), 0));
		}

		public List<PersistenceColony> GetPersistenceColonies() {
			List<PersistenceColony> persistenceColonies = new List<PersistenceColony>();
			string coloniesPath = GameManager.planetM.planet.directory + "/Colonies";
			if (Directory.Exists(coloniesPath)) {
				foreach (string colonyDirectoryPath in Directory.GetDirectories(coloniesPath)) {
					PersistenceColony persistenceColony = LoadColony(colonyDirectoryPath + "/colony.snowship");

					string savesDirectoryPath = colonyDirectoryPath + "/Saves";
					List<string> saveDirectories = Directory.GetDirectories(savesDirectoryPath).OrderByDescending(sd => sd).ToList();
					if (saveDirectories.Count > 0) {
						string screenshotPath = Directory.GetFiles(saveDirectories[0]).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
						if (screenshotPath != null) {
							persistenceColony.lastSaveImage = LoadSpriteFromImageFile(screenshotPath);
						}
					}
					persistenceColonies.Add(persistenceColony);
				}
			}
			persistenceColonies = persistenceColonies.OrderByDescending(pc => pc.lastSaveTimeChunk).ToList();
			return persistenceColonies;
		}

		public PersistenceColony LoadColony(string path) {

			PersistenceColony persistenceColony = new PersistenceColony(path);

			foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
				switch ((ColonyProperty)Enum.Parse(typeof(ColonyProperty), property.Key)) {
					case ColonyProperty.LastSaveDateTime:
						persistenceColony.lastSaveDateTime = (string)property.Value;
						break;
					case ColonyProperty.LastSaveTimeChunk:
						persistenceColony.lastSaveTimeChunk = (string)property.Value;
						break;
					case ColonyProperty.Name:
						persistenceColony.name = (string)property.Value;
						break;
					case ColonyProperty.PlanetPosition:
						persistenceColony.planetPosition = new Vector2(float.Parse(((string)property.Value).Split(',')[0]), float.Parse(((string)property.Value).Split(',')[1]));
						break;
					case ColonyProperty.Seed:
						persistenceColony.seed = int.Parse((string)property.Value);
						break;
					case ColonyProperty.Size:
						persistenceColony.size = int.Parse((string)property.Value);
						break;
					case ColonyProperty.AverageTemperature:
						persistenceColony.averageTemperature = float.Parse((string)property.Value);
						break;
					case ColonyProperty.AveragePrecipitation:
						persistenceColony.averagePrecipitation = float.Parse((string)property.Value);
						break;
					case ColonyProperty.TerrainTypeHeights:
						foreach (KeyValuePair<string, object> terrainTypeHeightProperty in (List<KeyValuePair<string, object>>)property.Value) {
							TileManager.TileTypeGroup.TypeEnum terrainTypeHeightPropertyKey = (TileManager.TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileManager.TileTypeGroup.TypeEnum), terrainTypeHeightProperty.Key);
							persistenceColony.terrainTypeHeights.Add(terrainTypeHeightPropertyKey, float.Parse((string)terrainTypeHeightProperty.Value));
						}
						break;
					case ColonyProperty.SurroundingPlanetTileHeights:
						foreach (string height in ((string)property.Value).Split(',')) {
							persistenceColony.surroundingPlanetTileHeights.Add(int.Parse(height));
						}
						break;
					case ColonyProperty.OnRiver:
						persistenceColony.onRiver = bool.Parse((string)property.Value);
						break;
					case ColonyProperty.SurroundingPlanetTileRivers:
						foreach (string riverIndex in ((string)property.Value).Split(',')) {
							persistenceColony.surroundingPlanetTileRivers.Add(int.Parse(riverIndex));
						}
						break;
					default:
						Debug.LogError("Unknown colony property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return persistenceColony;
		}

		public Colony ApplyLoadedColony(PersistenceColony persistenceColony) {
			Colony colony = GameManager.colonyM.CreateColony(
				new CreateColonyData(
					persistenceColony.name,
					persistenceColony.seed,
					persistenceColony.size,
					GameManager.planetM.selectedPlanetTile
				));
			colony.SetLastSaveDateTime(persistenceColony.lastSaveDateTime, persistenceColony.lastSaveTimeChunk);
			colony.SetDirectory(Directory.GetParent(persistenceColony.path).FullName);

			GameManager.colonyM.SetColony(colony);

			return colony;
		}

		public void CreateColony(Colony colony) {
			if (colony == null) {
				Debug.LogError("Colony to be saved is null.");
				return;
			}

			if (GameManager.planetM.planet == null) {
				Debug.LogError("Planet to save the colony to is null.");
				return;
			}

			if (string.IsNullOrEmpty(GameManager.planetM.planet.directory)) {
				Debug.LogError("Planet directory is null or empty.");
				return;
			}

			string coloniesDirectoryPath = GameManager.planetM.planet.directory + "/Colonies";
			string dateTimeString = GenerateDateTimeString();
			string colonyDirectoryPath = coloniesDirectoryPath + "/Colony-" + dateTimeString;
			Directory.CreateDirectory(colonyDirectoryPath);
			colony.SetDirectory(colonyDirectoryPath);

			UpdateColonySave(colony);

			string mapDirectoryPath = colonyDirectoryPath + "/Map";
			Directory.CreateDirectory(mapDirectoryPath);

			StreamWriter tilesFile = CreateFileAtDirectory(mapDirectoryPath, "tiles.snowship");
			pMap.SaveOriginalTiles(tilesFile);
			tilesFile.Close();

			StreamWriter riversFile = CreateFileAtDirectory(mapDirectoryPath, "rivers.snowship");
			pRiver.SaveOriginalRivers(riversFile);
			riversFile.Close();

			string savesDirectoryPath = colonyDirectoryPath + "/Saves";
			Directory.CreateDirectory(savesDirectoryPath);
		}

		public void UpdateColonySave(Colony colony) {
			if (colony == null) {
				Debug.LogError("Colony to be saved is null.");
				return;
			}

			if (GameManager.planetM.planet == null) {
				Debug.LogError("Planet to save the colony to is null.");
				return;
			}

			if (string.IsNullOrEmpty(GameManager.planetM.planet.directory)) {
				Debug.LogError("Planet directory is null or empty.");
				return;
			}

			string colonyFilePath = colony.directory + "/colony.snowship";
			if (File.Exists(colonyFilePath)) {
				File.WriteAllText(colonyFilePath, string.Empty);
			} else {
				CreateFileAtDirectory(colony.directory, "colony.snowship").Close();
			}
			StreamWriter colonyFile = new StreamWriter(colonyFilePath);
			SaveColony(colonyFile, colony);
			colonyFile.Close();
		}

	}
}
