using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NPlanet {
	public class PlanetTile {
		public Planet planet;
		public TileManager.Tile tile;

		public Sprite sprite;

		public float equatorOffset;
		public bool isRiver;
		public Dictionary<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeights;
		public List<int> surroundingPlanetTileHeightDirections = new List<int>();
		public List<int> surroundingPlanetTileRivers = new List<int>();

		public string altitude;

		public PlanetTile(Planet planet, TileManager.Tile tile) {
			this.planet = planet;
			this.tile = tile;

			sprite = tile.sr.sprite;

			// Setup PlanetTile-specific Information
			equatorOffset = ((tile.position.y - (planet.mapData.mapSize / 2f)) * 2) / planet.mapData.mapSize;

			TileManager.Map.River river = planet.RiversContainTile(tile, true).Value;
			isRiver = river != null;

			foreach (TileManager.Tile sTile in tile.horizontalSurroundingTiles) {
				if (sTile != null) {
					if (planet.rivers.Find(r => r.tiles.Contains(sTile)) == null) {
						if (sTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
							surroundingPlanetTileHeightDirections.Add(-2);
						} else if (sTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Stone) {
							surroundingPlanetTileHeightDirections.Add(5);
						} else {
							surroundingPlanetTileHeightDirections.Add(0);
						}
					} else {
						surroundingPlanetTileHeightDirections.Add(0);
					}

					if (isRiver) {
						int nTileRiverIndex = river.tiles.IndexOf(sTile);
						if (nTileRiverIndex == -1) {
							foreach (TileManager.Map.River r in planet.rivers) {
								if (r != river) {
									if (r.tiles.Contains(sTile)) {
										nTileRiverIndex = r.tiles.IndexOf(sTile);
									}
								}
							}
						}

						if (nTileRiverIndex == -1) {
							if (river.startTile == tile && sTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Stone) {
								nTileRiverIndex = 0;
							} else if (river.endTile == tile && sTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
								nTileRiverIndex = int.MaxValue;
							}
						}

						surroundingPlanetTileRivers.Add(nTileRiverIndex);
					} else {
						surroundingPlanetTileRivers.Add(-1);
					}
				} else {
					surroundingPlanetTileHeightDirections.Add(0);
					surroundingPlanetTileRivers.Add(-1);
				}
			}

			terrainTypeHeights = new Dictionary<TileManager.TileTypeGroup.TypeEnum, float>() {
				{ TileManager.TileTypeGroup.TypeEnum.Water, 0.40f * tile.GetPrecipitation() * (1 - tile.height) },
				{ TileManager.TileTypeGroup.TypeEnum.Stone, 0.75f * (1 - (tile.height - (1 - 0.75f))) }
			};

			altitude = Mathf.RoundToInt((tile.height - terrainTypeHeights[TileManager.TileTypeGroup.TypeEnum.Water]) * 5000f) + "m";

			// Remove Tile-specific Information
			MonoBehaviour.Destroy(tile.obj);
			tile.obj = null;
			tile.sr = null;
		}
	}
}
