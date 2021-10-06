using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonyManager : BaseManager {

	private static readonly List<int> mapSizes = new List<int>() { 50, 100, 150, 200 };

	public static int GetNumMapSizes() {
		return mapSizes.Count;
	}

	public static int GetMapSizeByIndex(int index) {
		return mapSizes[index];
	}

	public static string GetRandomColonyName() {
		return GameManager.resourceM.GetRandomLocationName();
	}

	public void SetupNewColony(Colony colony, bool initialized) {
		if (!initialized) {
			this.colony = colony;

			GameManager.tileM.Initialize(colony, TileManager.MapInitializeType.NewMap);
		} else {
			GameManager.colonistM.SpawnStartColonists(3);

			GameManager.persistenceM.CreateColony(colony);
			GameManager.persistenceM.CreateSave(colony);

			GameManager.uiM.SetLoadingScreenActive(false);
		}
	}

	public Colony CreateColony(
		string name,
		Vector2 planetPosition,
		int seed, 
		int size, 
		float averageTemperature, 
		float averagePrecipitation, 
		Dictionary<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeights,
		List<int> surroundingPlanetTileHeights,
		bool onRiver,
		List<int> surroundingPlanetTileRivers
	) {
		Colony colony = new Colony(
			name,
			new TileManager.MapData(
				GameManager.planetM.planet.mapData,
				seed,
				size,
				true,
				false,
				0,
				0,
				false,
				averageTemperature,
				averagePrecipitation,
				terrainTypeHeights,
				surroundingPlanetTileHeights,
				onRiver,
				surroundingPlanetTileRivers,
				false,
				GameManager.planetM.planet.primaryWindDirection,
				planetPosition
			)
		);
		return colony;
	}

	public void LoadColony(Colony colony, bool initialized) {
		if (!initialized) {
			this.colony = colony;

			UnityEngine.Random.InitState(colony.mapData.mapSeed);

			//GameManager.tileM.Initialize(colony, TileManager.MapInitializeType.LoadMap);
		}
	}

	public Colony colony;

	public void SetColony(Colony colony) {
		if (this.colony != null && this.colony.map != null) {
			foreach (TileManager.Tile tile in this.colony.map.tiles) {
				MonoBehaviour.Destroy(tile.obj);
			}
		}

		this.colony = colony;
	}

	public class Colony {
		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;

		public TileManager.MapData mapData;
		public TileManager.Map map;

		public Colony(string name, TileManager.MapData mapData) {
			this.name = name;
			this.mapData = mapData;

			lastSaveDateTime = PersistenceManager.GenerateSaveDateTimeString();
			lastSaveTimeChunk = PersistenceManager.GenerateDateTimeString();
		}

		public void SetDirectory(string directory) {
			this.directory = directory;
		}

		public void SetLastSaveDateTime(string lastSaveDateTime, string lastSaveTimeChunk) {
			this.lastSaveDateTime = lastSaveDateTime;
			this.lastSaveTimeChunk = lastSaveTimeChunk;
		}
	}
}
