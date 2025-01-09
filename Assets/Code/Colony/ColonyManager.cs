using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NColony {
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

				GameManager.uiMOld.SetLoadingScreenActive(false);
			}
		}

		public Colony CreateColony(CreateColonyData createColonyData) {
			Colony colony = new Colony(
				createColonyData.name,
				new TileManager.MapData(
					GameManager.planetM.planet.mapData,
					createColonyData.seed,
					createColonyData.size,
					true,
					false,
					0,
					0,
					createColonyData.planetTile.tile.temperature,
					createColonyData.planetTile.tile.GetPrecipitation(),
					createColonyData.planetTile.terrainTypeHeights,
					createColonyData.planetTile.surroundingPlanetTileHeightDirections,
					createColonyData.planetTile.isRiver,
					createColonyData.planetTile.surroundingPlanetTileRivers,
					false,
					GameManager.planetM.planet.primaryWindDirection,
					createColonyData.planetTile.tile.position
				)
			);
			return colony;
		}

		public void LoadColony(Colony colony, bool initialized) {
			if (!initialized) {
				this.colony = colony;

				Random.InitState(colony.mapData.mapSeed);

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
	}
}
