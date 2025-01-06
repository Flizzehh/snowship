using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Snowship.NPlanet {
	public class PlanetManager : BaseManager {

		public Planet planet;
		public PlanetTile selectedPlanetTile;
		private readonly PlanetMapDataValues planetMapDataValues = new PlanetMapDataValues();

		public void SetPlanet(Planet planet) {
			this.planet = planet;
		}

		public void SetSelectedPlanetTile(PlanetTile planetTile) {
			selectedPlanetTile = planetTile;
		}

		public string GetRandomPlanetName() {
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string twoCharacters = new string(Enumerable.Repeat(chars, 2).Select(s => s[UnityEngine.Random.Range(0, chars.Length)]).ToArray());
			return twoCharacters + UnityEngine.Random.Range(1000, 9999);
		}

		public int GetRandomPlanetSeed() {
			return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}

		private readonly List<int> planetSizes = new List<int> { 10, 15, 20, 25, 30, 40, 50, 60, 75, 100, 120 }; // Some divisors of 600 (600px = width of planet preview)

		public int GetPlanetSizeByIndex(int index) {
			return planetSizes[index];
		}

		public int GetNumPlanetSizes() {
			return planetSizes.Count;
		}

		public float GetPlanetDistanceByIndex(int index) {
			return (float)Math.Round(0.1f * (index + 6), 1);
		}

		public int GetMinPlanetDistance() {
			return 1;
		}

		public int GetMaxPlanetDistance() {
			return 7;
		}

		public int GetTemperatureRangeByIndex(int index) {
			return index * 10;
		}

		private readonly List<int> windCircularDirectionMap = new List<int> { 0, 4, 1, 5, 2, 6, 3, 7 };

		public int GetWindCircularDirectionByIndex(int index) {
			return windCircularDirectionMap[index];
		}

		private readonly List<string> windCardinalDirectionMap = new List<string> { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

		public string GetWindCardinalDirectionByIndex(int index) {
			return windCardinalDirectionMap[index];
		}

		public int GetNumWindDirections() {
			if (windCircularDirectionMap.Count != windCardinalDirectionMap.Count) {
				Debug.LogError("windCircularDirectionMap.Count != windCardinalDirectionMap.Count");
			}

			return windCircularDirectionMap.Count;
		}

		private class PlanetMapDataValues {
			public const bool ActualMap = false;
			public const bool PlanetTemperature = true;
			public const float AverageTemperature = -1;
			public const float AveragePrecipitation = -1;
			public readonly Dictionary<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeights = new Dictionary<TileManager.TileTypeGroup.TypeEnum, float> {
				{ TileManager.TileTypeGroup.TypeEnum.Water, 0.40f },
				{ TileManager.TileTypeGroup.TypeEnum.Stone, 0.75f }
			};
			public readonly List<int> surroundingPlanetTileHeightDirections = null;
			public const bool River = false;
			public readonly List<int> surroundingPlanetTileRivers = null;
			public const bool PreventEdgeTouching = true;
			public readonly Vector2 planetTilePosition = Vector2.zero;
		}

		public static int CalculatePlanetTemperature(float distance) {

			const float starMass = 1; // 1 (lower = colder)
			const float albedo = 29; // 29 (higher = colder)
			const float greenhouse = 0.4f; // 1 (lower = colder)

			float sigma = 5.6703f * Mathf.Pow(10, -5);
			float starLuminosity = 3.846f * Mathf.Pow(10, 33) * Mathf.Pow(starMass, 3);
			float starDistance = (distance + 0.2f) * 1.496f * Mathf.Pow(10, 13);
			const float albedoPercentage = albedo / 100f;
			const float t = greenhouse * 0.5841f;
			float x = Mathf.Sqrt((1 - albedoPercentage) * starLuminosity / (16 * Mathf.PI * sigma));
			float tEff = Mathf.Sqrt(x) * (1 / Mathf.Sqrt(starDistance));
			float tEq = (Mathf.Pow(tEff, 4)) * (1 + (3 * t / 4));
			float tSur = tEq / 0.9f;
			float tKelvin = Mathf.Round(Mathf.Sqrt(Mathf.Sqrt(tSur)));
			int celsius = Mathf.RoundToInt(tKelvin - 273);

			return celsius;
		}

		public Planet CreatePlanet(CreatePlanetData createPlanetData) {
			Planet planet = new Planet(
				createPlanetData.name,
				new TileManager.MapData(
					null,
					createPlanetData.seed,
					createPlanetData.size,
					PlanetMapDataValues.ActualMap,
					PlanetMapDataValues.PlanetTemperature,
					createPlanetData.temperatureRange,
					createPlanetData.distance,
					createPlanetData.randomOffsets,
					PlanetMapDataValues.AverageTemperature,
					PlanetMapDataValues.AveragePrecipitation,
					planetMapDataValues.terrainTypeHeights,
					planetMapDataValues.surroundingPlanetTileHeightDirections,
					PlanetMapDataValues.River,
					planetMapDataValues.surroundingPlanetTileRivers,
					PlanetMapDataValues.PreventEdgeTouching,
					createPlanetData.windDirection,
					planetMapDataValues.planetTilePosition
				)
			);

			this.planet = planet;

			return planet;
		}


	}
}
