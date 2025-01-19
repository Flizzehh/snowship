using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Snowship.NPlanet;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PPlanet : PersistenceHandler {

		public enum PlanetProperty {
			LastSaveDateTime,
			LastSaveTimeChunk,
			Name,
			Seed,
			Size,
			SunDistance,
			TempRange,
			RandomOffsets,
			WindDirection
		}

		public void SavePlanet(StreamWriter file, Planet planet) {
			file.WriteLine(CreateKeyValueString(PlanetProperty.LastSaveDateTime, planet.lastSaveDateTime, 0));
			file.WriteLine(CreateKeyValueString(PlanetProperty.LastSaveTimeChunk, planet.lastSaveTimeChunk, 0));

			file.WriteLine(CreateKeyValueString(PlanetProperty.Name, planet.name, 0));
			file.WriteLine(CreateKeyValueString(PlanetProperty.Seed, planet.mapData.mapSeed, 0));
			file.WriteLine(CreateKeyValueString(PlanetProperty.Size, planet.mapData.mapSize, 0));
			file.WriteLine(CreateKeyValueString(PlanetProperty.SunDistance, planet.mapData.planetDistance, 0));
			file.WriteLine(CreateKeyValueString(PlanetProperty.TempRange, planet.mapData.temperatureRange, 0));
			file.WriteLine(CreateKeyValueString(PlanetProperty.WindDirection, planet.mapData.primaryWindDirection, 0));
		}

		public List<PersistencePlanet> GetPersistencePlanets() {
			List<PersistencePlanet> persistencePlanets = new List<PersistencePlanet>();
			string planetsPath = GameManager.universeM.universe.directory + "/Planets";
			if (Directory.Exists(planetsPath)) {
				foreach (string planetDirectoryPath in Directory.GetDirectories(planetsPath)) {
					persistencePlanets.Add(LoadPlanet(planetDirectoryPath + "/planet.snowship"));
				}
			}
			persistencePlanets = persistencePlanets.OrderByDescending(pp => pp.lastSaveTimeChunk).ToList();
			return persistencePlanets;
		}

		public PersistencePlanet LoadPlanet(string path) {

			PersistencePlanet persistencePlanet = new PersistencePlanet(path);

			foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
				switch ((PlanetProperty)Enum.Parse(typeof(PlanetProperty), property.Key)) {
					case PlanetProperty.LastSaveDateTime:
						persistencePlanet.lastSaveDateTime = (string)property.Value;
						break;
					case PlanetProperty.LastSaveTimeChunk:
						persistencePlanet.lastSaveTimeChunk = (string)property.Value;
						break;
					case PlanetProperty.Name:
						persistencePlanet.name = (string)property.Value;
						break;
					case PlanetProperty.Seed:
						persistencePlanet.seed = int.Parse((string)property.Value);
						break;
					case PlanetProperty.Size:
						persistencePlanet.size = int.Parse((string)property.Value);
						break;
					case PlanetProperty.SunDistance:
						persistencePlanet.sunDistance = float.Parse((string)property.Value);
						break;
					case PlanetProperty.TempRange:
						persistencePlanet.temperatureRange = int.Parse((string)property.Value);
						break;
					case PlanetProperty.RandomOffsets:
						persistencePlanet.randomOffsets = bool.Parse((string)property.Value);
						break;
					case PlanetProperty.WindDirection:
						persistencePlanet.windDirection = int.Parse((string)property.Value);
						break;
					default:
						Debug.LogError("Unknown planet property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return persistencePlanet;
		}

		public Planet ApplyLoadedPlanet(PersistencePlanet persistencePlanet) {
			Planet planet = GameManager.planetM.CreatePlanet(
				new CreatePlanetData(
					persistencePlanet.name,
					persistencePlanet.seed,
					persistencePlanet.size,
					persistencePlanet.sunDistance,
					persistencePlanet.temperatureRange,
					persistencePlanet.windDirection
				));
			planet.SetLastSaveDateTime(persistencePlanet.lastSaveDateTime, persistencePlanet.lastSaveTimeChunk);
			planet.SetDirectory(Directory.GetParent(persistencePlanet.path).FullName);
			return planet;
		}

		public void CreatePlanet(Planet planet) {
			if (planet == null) {
				Debug.LogError("Planet to be saved is null.");
				return;
			}

			if (GameManager.universeM.universe == null) {
				GameManager.universeM.CreateUniverse(UniverseManager.GetRandomUniverseName());
			}

			if (string.IsNullOrEmpty(GameManager.universeM.universe?.directory)) {
				Debug.LogError("Universe directory is null or empty.");
				return;
			}

			string planetsDirectoryPath = GameManager.universeM.universe.directory + "/Planets";
			string dateTimeString = GenerateDateTimeString();
			string planetDirectoryPath = planetsDirectoryPath + "/Planet-" + dateTimeString;
			Directory.CreateDirectory(planetDirectoryPath);
			planet.SetDirectory(planetDirectoryPath);

			UpdatePlanetSave(planet);

			string citiesDirectoryPath = planetDirectoryPath + "/Cities";
			Directory.CreateDirectory(citiesDirectoryPath);

			string coloniesDirectoryPath = planetDirectoryPath + "/Colonies";
			Directory.CreateDirectory(coloniesDirectoryPath);
		}

		public void UpdatePlanetSave(Planet planet) {
			if (planet == null) {
				Debug.LogError("Planet to be saved is null.");
				return;
			}

			if (GameManager.universeM.universe == null) {
				Debug.LogError("Universe to save the planet to is null.");
				return;
			}

			if (string.IsNullOrEmpty(GameManager.universeM.universe.directory)) {
				Debug.LogError("Universe directory is null or empty.");
				return;
			}

			string planetFilePath = planet.directory + "/planet.snowship";
			if (File.Exists(planetFilePath)) {
				File.WriteAllText(planetFilePath, string.Empty);
			} else {
				CreateFileAtDirectory(planet.directory, "planet.snowship").Close();
			}
			StreamWriter planetFile = new StreamWriter(planetFilePath);
			SavePlanet(planetFile, planet);
			planetFile.Close();
		}
	}
}
