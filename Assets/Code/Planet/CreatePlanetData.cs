using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;
using Range = Snowship.NUtilities.Range;

namespace Snowship.NPlanet {

	public class CreatePlanetData {

		public string Name { get; private set; }
		public int Seed;

		public int SizeIndex { get; private set; }
		public int Size { get; private set; }

		public int DistanceIndex { get; private set; }
		public float Distance { get; private set; }

		public int TemperatureRangeIndex { get; private set; }
		public int TemperatureRange { get; private set; }

		public int WindDirectionIndex { get; private set; }
		public int WindDirection { get; private set; }

		private static readonly List<int> planetSizes = new List<int> {
			10, 20, 30, 40, 50, 60, 100, 120 // Some divisors of 600 (600px = width of planet preview)
		};

		public static readonly Range PlanetDistanceIndexRange = new Range(1, 7);
		public static readonly Range PlanetTemperatureIndexRange = new Range(2, 8);

		public static readonly Range PlanetWindDirectionIndexRange = new Range(0, 8);
		private static readonly List<int> windCircularDirectionMap = new List<int> {
			0, 4, 1, 5, 2, 6, 3, 7
		};
		private static readonly List<string> windCardinalDirectionMap = new List<string> {
			"N", "NE", "E", "SE", "S", "SW", "W", "NW"
		};

		public CreatePlanetData() {
			SetDefaultValues();
		}

		public CreatePlanetData(
			string name,
			int seed,
			int size,
			float distance,
			int temperatureRange,
			int windDirection
		) {
			Name = name;
			Seed = seed;
			Size = size;
			Distance = distance;
			TemperatureRange = temperatureRange;
			WindDirection = windDirection;
		}

		private void SetDefaultValues() {

			SetName(GenerateRandomPlanetName());
			Seed = GenerateRandomPlanetSeed();

			SetSize(Mathf.FloorToInt((GetNumPlanetSizes() - 1) / 2f) + 2); // 60
			SetDistance(PlanetDistanceIndexRange.Midpoint); // 1 AU
			SetTemperatureRange(PlanetTemperatureIndexRange.Midpoint + 2); // 70°C
			SetWindDirection(PlanetWindDirectionIndexRange.RandomExclusive());
		}

		public bool SetName(string name) {
			bool validPlanetName = IsPlanetNameValid(name);
			if (validPlanetName) {
				Name = name;
			}
			return validPlanetName;
		}

		public void SetSize(int sizeIndex) {
			SizeIndex = sizeIndex;
			Size = GetPlanetSizeByIndex(SizeIndex);
		}

		public void SetDistance(int distanceIndex) {
			DistanceIndex = distanceIndex;
			Distance = CalculatePlanetDistanceByIndex(DistanceIndex);
		}

		public void SetTemperatureRange(int temperatureRangeIndex) {
			TemperatureRangeIndex = temperatureRangeIndex;
			TemperatureRange = CalculateTemperatureRangeByIndex(TemperatureRangeIndex);
		}

		public void SetWindDirection(int windDirectionIndex) {
			WindDirectionIndex = windDirectionIndex;
			WindDirection = GetWindCircularDirectionByIndex(WindDirectionIndex);
		}

		public override string ToString() {
			return $"{nameof(Name)}: {Name}, {nameof(Seed)}: {Seed}, {nameof(Size)}: {Size}, {nameof(Distance)}: {Distance}, {nameof(TemperatureRange)}: {TemperatureRange}, {nameof(WindDirection)}: {WindDirection}";
		}

		private static string GenerateRandomPlanetName() {
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string twoCharacters = new string(Enumerable.Repeat(chars, 2).Select(s => s[UnityEngine.Random.Range(0, chars.Length)]).ToArray());
			return twoCharacters + UnityEngine.Random.Range(1000, 9999);
		}

		private bool IsPlanetNameValid(string planetName) {
			return StringUtilities.IsAlphanumericWithSpaces(planetName);
		}

		public static int GenerateRandomPlanetSeed() {
			return UnityEngine.Random.Range(0, int.MaxValue);
		}

		private static int GetPlanetSizeByIndex(int index) {
			return planetSizes[index];
		}

		public static int GetNumPlanetSizes() {
			return planetSizes.Count;
		}

		private static float CalculatePlanetDistanceByIndex(int index) {
			return (float)Math.Round(0.1f * (index + 6), 1);
		}

		private static int CalculateTemperatureRangeByIndex(int index) {
			return index * 10;
		}

		private static int GetWindCircularDirectionByIndex(int index) {
			return windCircularDirectionMap[index];
		}

		public static string GetWindCardinalDirectionByIndex(int index) {
			return windCardinalDirectionMap[index];
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
	}
}
