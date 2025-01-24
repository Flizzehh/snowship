using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PUniverse : UniverseManager.Universe
	{

		public enum ConfigurationProperty {
			GameVersion,
			SaveVersion
		}

		public enum UniverseProperty {
			Name,
			LastSaveDateTime,
			LastSaveTimeChunk
		}

		// Universe

		public string GenerateUniversesPath() {
			return PU.GetPersistentDataPath() + "/Universes";
		}

		public void SaveUniverse(StreamWriter file, UniverseManager.Universe universe) {
			file.WriteLine(PU.CreateKeyValueString(UniverseProperty.LastSaveDateTime, universe.lastSaveDateTime, 0));
			file.WriteLine(PU.CreateKeyValueString(UniverseProperty.LastSaveTimeChunk, universe.lastSaveTimeChunk, 0));

			file.WriteLine(PU.CreateKeyValueString(UniverseProperty.Name, universe.name, 0));
		}

		public List<PersistenceUniverse> GetPersistenceUniverses() {
			List<PersistenceUniverse> persistenceUniverses = new List<PersistenceUniverse>();
			string universesPath = GenerateUniversesPath();
			if (Directory.Exists(universesPath)) {
				foreach (string universeDirectoryPath in Directory.GetDirectories(universesPath)) {
					persistenceUniverses.Add(new PersistenceUniverse(universeDirectoryPath));
				}
			}
			persistenceUniverses = persistenceUniverses.OrderByDescending(pu => pu.universeProperties[UniverseProperty.LastSaveTimeChunk]).ToList();
			return persistenceUniverses;
		}

		public Dictionary<UniverseProperty, string> LoadUniverse(string path) {
			Dictionary<UniverseProperty, string> properties = new Dictionary<UniverseProperty, string>();

			foreach (KeyValuePair<string, object> property in PU.GetKeyValuePairsFromFile(path)) {
				UniverseProperty key = (UniverseProperty)Enum.Parse(typeof(UniverseProperty), property.Key);
				switch (key) {
					case UniverseProperty.LastSaveDateTime:
					case UniverseProperty.LastSaveTimeChunk:
					case UniverseProperty.Name:
						properties.Add(key, (string)property.Value);
						break;
					default:
						Debug.LogError("Unknown universe property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return properties;
		}

		public void ApplyLoadedUniverse(PersistenceUniverse persistenceUniverse) {
			UniverseManager.Universe universe = new UniverseManager.Universe(persistenceUniverse.universeProperties[UniverseProperty.Name]) { directory = persistenceUniverse.path, lastSaveDateTime = persistenceUniverse.universeProperties[UniverseProperty.LastSaveDateTime], lastSaveTimeChunk = persistenceUniverse.universeProperties[UniverseProperty.LastSaveTimeChunk] };
			GameManager.Get<UniverseManager>().SetUniverse(universe);
		}

		public void CreateUniverse(UniverseManager.Universe universe) {
			if (universe == null) {
				Debug.LogError("Universe to be saved is null.");
				return;
			}

			string universesDirectoryPath = GenerateUniversesPath();
			string dateTimeString = PU.GenerateDateTimeString();
			string universeDirectoryPath = universesDirectoryPath + "/Universe-" + dateTimeString;
			Directory.CreateDirectory(universeDirectoryPath);
			universe.SetDirectory(universeDirectoryPath);

			UpdateUniverseSave(universe);

			string planetsDirectoryPath = universeDirectoryPath + "/Planets";
			Directory.CreateDirectory(planetsDirectoryPath);
		}

		public void UpdateUniverseSave(UniverseManager.Universe universe) {
			if (universe == null) {
				Debug.LogError("Universe to be saved is null.");
				return;
			}

			string configurationFilePath = universe.directory + "/configuration.snowship";
			if (File.Exists(configurationFilePath)) {
				File.WriteAllText(configurationFilePath, string.Empty);
			} else {
				PU.CreateFileAtDirectory(universe.directory, "configuration.snowship").Close();
			}
			StreamWriter configurationFile = new StreamWriter(configurationFilePath);
			SaveConfiguration(configurationFile);
			configurationFile.Close();

			string universeFilePath = universe.directory + "/universe.snowship";
			if (File.Exists(universeFilePath)) {
				File.WriteAllText(universeFilePath, string.Empty);
			} else {
				PU.CreateFileAtDirectory(universe.directory, "universe.snowship").Close();
			}
			StreamWriter universeFile = new StreamWriter(universeFilePath);
			SaveUniverse(universeFile, universe);
			universeFile.Close();
		}

		// Configuration

		public void SaveConfiguration(StreamWriter file) {
			file.WriteLine(PU.CreateKeyValueString(ConfigurationProperty.GameVersion, PersistenceManager.GameVersion.text, 0));
			file.WriteLine(PU.CreateKeyValueString(ConfigurationProperty.SaveVersion, PersistenceManager.SaveVersion.text, 0));
		}

		public Dictionary<ConfigurationProperty, string> LoadConfiguration(string path) {
			Dictionary<ConfigurationProperty, string> properties = new Dictionary<ConfigurationProperty, string>();

			foreach (KeyValuePair<string, object> property in PU.GetKeyValuePairsFromFile(path)) {
				ConfigurationProperty key = (ConfigurationProperty)Enum.Parse(typeof(ConfigurationProperty), property.Key);
				object value = property.Value;
				switch (key) {
					case ConfigurationProperty.GameVersion:
						properties.Add(key, (string)value);
						break;
					case ConfigurationProperty.SaveVersion:
						properties.Add(key, (string)value);
						break;
					default:
						Debug.LogError("Unknown configuration property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return properties;
		}

		public void ApplyLoadedConfiguration(PersistenceUniverse persistenceUniverse) {
			// TODO ApplyLoadedConfiguration? Is this needed?
		}

		public bool IsUniverseLoadable(PersistenceUniverse persistenceUniverse) {
			if (persistenceUniverse == null) {
				return false;
			}

			bool saveVersionValid = persistenceUniverse.configurationProperties[ConfigurationProperty.SaveVersion] == PersistenceManager.SaveVersion.text;
			return saveVersionValid;
		}
	}
}