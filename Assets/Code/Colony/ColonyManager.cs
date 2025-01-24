using Cysharp.Threading.Tasks;
using Snowship.NColonist;
using Snowship.NPersistence;
using Snowship.NPlanet;
using UnityEngine;

namespace Snowship.NColony {
	public class ColonyManager : IManager {

		private readonly PColony pColony = new PColony();

		public void SetupNewColony(Colony colony, bool initialized) {
			if (!initialized) {
				this.colony = colony;

				GameManager.Get<TileManager>().Initialize(colony, TileManager.MapInitializeType.NewMap).Forget();
			} else {
				GameManager.Get<ColonistManager>().SpawnStartColonists(3);

				pColony.CreateColony(colony);
				GameManager.Get<PersistenceManager>().CreateSave(colony).Forget();
			}
		}

		public Colony CreateColony(CreateColonyData createColonyData) {
			Colony colony = new Colony(
				createColonyData.Name,
				new TileManager.MapData(
					GameManager.Get<PlanetManager>().planet.mapData,
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
					GameManager.Get<PlanetManager>().planet.primaryWindDirection,
					createColonyData.PlanetTile.tile.position
				)
			);
			return colony;
		}

		public void LoadColony(Colony colony, bool initialized) {
			if (!initialized) {
				this.colony = colony;

				Random.InitState(colony.mapData.mapSeed);

				//GameManager.Get<TileManager>().Initialize(colony, TileManager.MapInitializeType.LoadMap);
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