using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class PlanetManager : BaseManager {

	public static string GetRandomPlanetName() {
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		string twoCharacters = new string(Enumerable.Repeat(chars, 2).Select(s => s[UnityEngine.Random.Range(0, chars.Length)]).ToArray());
		return twoCharacters + UnityEngine.Random.Range(1000, 9999);
	}

	public static int GetRandomPlanetSeed() {
		return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}

	private static readonly List<int> planetSizes = new List<int>() { 10, 15, 20, 25, 30, 40, 50, 60, 75, 100, 120 }; // Some divisors of 600 (600px = width of planet preview)

	public static int GetPlanetSizeByIndex(int index) {
		return planetSizes[index];
	}

	public static int GetNumPlanetSizes() {
		return planetSizes.Count;
	}

	public static float GetPlanetDistanceByIndex(int index) {
		return (float)Math.Round(0.1f * (index + 6), 1);
	}

	public static int GetMinPlanetDistance() {
		return 1;
	}

	public static int GetMaxPlanetDistance() {
		return 7;
	}

	public static int GetTemperatureRangeByIndex(int index) {
		return index * 10;
	}

	private static readonly List<int> windCircularDirectionMap = new List<int>() { 0, 4, 1, 5, 2, 6, 3, 7 };

	public static int GetWindCircularDirectionByIndex(int index) {
		return windCircularDirectionMap[index];
	}

	private static readonly List<string> windCardinalDirectionMap = new List<string>() { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

	public static string GetWindCardinalDirectionByIndex(int index) {
		return windCardinalDirectionMap[index];
	}

	public static int GetNumWindDirections() {
		if (windCircularDirectionMap.Count != windCardinalDirectionMap.Count) {
			Debug.LogError("windCircularDirectionMap.Count != windCardinalDirectionMap.Count");
		}
		return windCircularDirectionMap.Count;
	}

	public static class StaticPlanetMapDataValues {
		public static bool actualMap = false;
		public static bool planetTemperature = true;
		public static float averageTemperature = -1;
		public static float averagePrecipitation = -1;
		public static readonly Dictionary<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeights = new Dictionary<TileManager.TileTypeGroup.TypeEnum, float> {
			{ TileManager.TileTypeGroup.TypeEnum.Water, 0.40f },
			{ TileManager.TileTypeGroup.TypeEnum.Stone, 0.75f }
		};
		public static List<int> surroundingPlanetTileHeightDirections = null;
		public static bool river = false;
		public static List<int> surroundingPlanetTileRivers = null;
		public static bool preventEdgeTouching = true;
		public static Vector2 planetTilePosition = Vector2.zero;
	}

	public static int CalculatePlanetTemperature(float distance) {

		float starMass = 1; // 1 (lower = colder)
		float albedo = 29; // 29 (higher = colder)
		float greenhouse = 0.4f; // 1 (lower = colder)

		float sigma = 5.6703f * Mathf.Pow(10, -5);
		float L = 3.846f * Mathf.Pow(10, 33) * Mathf.Pow(starMass, 3);
		float D = (distance + 0.2f) * 1.496f * Mathf.Pow(10, 13);
		float A = albedo / 100f;
		float T = greenhouse * 0.5841f;
		float X = Mathf.Sqrt((1 - A) * L / (16 * Mathf.PI * sigma));
		float T_eff = Mathf.Sqrt(X) * (1 / Mathf.Sqrt(D));
		float T_eq = (Mathf.Pow(T_eff, 4)) * (1 + (3 * T / 4));
		float T_sur = T_eq / 0.9f;
		float T_kel = Mathf.Round(Mathf.Sqrt(Mathf.Sqrt(T_sur)));
		int celsius = Mathf.RoundToInt(T_kel - 273);

		return celsius;
	}

	public Planet CreatePlanet(string name, int seed, int size, float distance, int temperatureRange, bool randomOffsets, int windDirection) {
		Planet planet = new Planet(
			name,
			new TileManager.MapData(
				null,
				seed,
				size,
				StaticPlanetMapDataValues.actualMap,
				StaticPlanetMapDataValues.planetTemperature,
				temperatureRange,
				distance,
				randomOffsets,
				StaticPlanetMapDataValues.averageTemperature,
				StaticPlanetMapDataValues.averagePrecipitation,
				StaticPlanetMapDataValues.terrainTypeHeights,
				StaticPlanetMapDataValues.surroundingPlanetTileHeightDirections,
				StaticPlanetMapDataValues.river,
				StaticPlanetMapDataValues.surroundingPlanetTileRivers,
				StaticPlanetMapDataValues.preventEdgeTouching,
				windDirection,
				StaticPlanetMapDataValues.planetTilePosition
			)
		);

		this.planet = planet;

		return planet;
	}

	public Planet planet = null;

	public void SetPlanet(Planet planet) {
		this.planet = planet;
	}

	public List<Planet> planets = new List<Planet>();

	public class Planet : TileManager.Map {

		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public List<PlanetTile> planetTiles;

		public string regenerationCode;

		public Planet(string name, TileManager.MapData mapData) : base(mapData) {

			this.name = name;

			planetTiles = new List<PlanetTile>();
			foreach (TileManager.Tile tile in tiles) {
				planetTiles.Add(new PlanetTile(this, tile));
			}

			regenerationCode = string.Format(
				"{0}{1}{2}{3}{4}{5}",
				mapData.mapSeed.ToString().PadLeft(20, '0'),
				mapData.mapSize.ToString().PadLeft(3, '0'),
				mapData.planetDistance.ToString().PadLeft(2, '0'),
				mapData.temperatureRange.ToString().PadLeft(3, '0'),
				mapData.randomOffsets ? "1" : "0",
				mapData.primaryWindDirection.ToString().PadLeft(2, '0')
			);

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

				River river = planet.RiversContainTile(tile, true).Value;
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
								foreach (River r in planet.rivers) {
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

	public Planet.PlanetTile selectedPlanetTile = null;

	public void SetSelectedPlanetTile(Planet.PlanetTile planetTile) {
		selectedPlanetTile = planetTile;
	}
}