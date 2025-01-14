using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NPersistence;
using UnityEngine;

namespace Snowship.NColony {
	public class ColonyManager : IManager {

		private readonly PColony pColony = new PColony();

		public void SetupNewColony(Colony colony, bool initialized) {
			if (!initialized) {
				this.colony = colony;

				UniTask.WhenAll(GameManager.tileM.Initialize(colony, TileManager.MapInitializeType.NewMap));
			} else {
				GameManager.colonistM.SpawnStartColonists(3);

				pColony.CreateColony(colony);
				UniTask.WhenAll(GameManager.persistenceM.CreateSave(colony));
			}
		}

		public Colony CreateColony(CreateColonyData createColonyData) {
			Colony colony = new Colony(
				createColonyData.Name,
				new TileManager.MapData(
					GameManager.planetM.planet.mapData,
					createColonyData.Seed,
					createColonyData.Size,
					true,
					false,
					0,
					0,
					createColonyData.PlanetTile.tile.temperature,
					createColonyData.PlanetTile.tile.GetPrecipitation(),
					createColonyData.PlanetTile.terrainTypeHeights,
					createColonyData.PlanetTile.surroundingPlanetTileHeightDirections,
					createColonyData.PlanetTile.isRiver,
					createColonyData.PlanetTile.surroundingPlanetTileRivers,
					false,
					GameManager.planetM.planet.primaryWindDirection,
					createColonyData.PlanetTile.tile.position
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
