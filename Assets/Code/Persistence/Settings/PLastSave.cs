using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PLastSave : PersistenceHandler {

		private readonly PUniverse pUniverse = new PUniverse();

		public class LastSaveProperties {
			public string lastSaveUniversePath;
			public string lastSavePlanetPath;
			public string lastSaveColonyPath;
			public string lastSaveSavePath;

			public LastSaveProperties(
				string lastSaveUniversePath,
				string lastSavePlanetPath,
				string lastSaveColonyPath,
				string lastSaveSavePath
			) {
				this.lastSaveUniversePath = lastSaveUniversePath;
				this.lastSavePlanetPath = lastSavePlanetPath;
				this.lastSaveColonyPath = lastSaveColonyPath;
				this.lastSaveSavePath = lastSaveSavePath;
			}
		}

		public enum LastSaveProperty {
			LastSaveUniversePath,
			LastSavePlanetPath,
			LastSaveColonyPath,
			LastSaveSavePath
		}

		public void UpdateLastSave(LastSaveProperties lastSaveProperties) {
			if (lastSaveProperties == null) {
				return;
			}

			string persistentDataPath = GetPersistentDataPath();
			string lastSaveFilePath = persistentDataPath + "/lastsave.snowship";
			if (File.Exists(lastSaveFilePath)) {
				File.WriteAllText(lastSaveFilePath, string.Empty);
			} else {
				CreateFileAtDirectory(persistentDataPath, "lastsave.snowship").Close();
			}
			StreamWriter lastSaveFile = new StreamWriter(lastSaveFilePath);
			SaveLastSave(lastSaveFile, lastSaveProperties);
			lastSaveFile.Close();
		}

		private void SaveLastSave(StreamWriter file, LastSaveProperties lastSaveProperties) {
			if (lastSaveProperties != null) {
				file.WriteLine(CreateKeyValueString(LastSaveProperty.LastSaveUniversePath, lastSaveProperties.lastSaveUniversePath, 0));
				file.WriteLine(CreateKeyValueString(LastSaveProperty.LastSavePlanetPath, lastSaveProperties.lastSavePlanetPath, 0));
				file.WriteLine(CreateKeyValueString(LastSaveProperty.LastSaveColonyPath, lastSaveProperties.lastSaveColonyPath, 0));
				file.WriteLine(CreateKeyValueString(LastSaveProperty.LastSaveSavePath, lastSaveProperties.lastSaveSavePath, 0));
			}
		}

		private Dictionary<LastSaveProperty, string> LoadLastSave(string path) {
			Dictionary<LastSaveProperty, string> properties = new Dictionary<LastSaveProperty, string>();

			foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
				LastSaveProperty key = (LastSaveProperty)Enum.Parse(typeof(LastSaveProperty), property.Key);
				object value = property.Value;
				switch (key) {
					case LastSaveProperty.LastSaveUniversePath:
						properties.Add(key, (string)value);
						break;
					case LastSaveProperty.LastSavePlanetPath:
						properties.Add(key, (string)value);
						break;
					case LastSaveProperty.LastSaveColonyPath:
						properties.Add(key, (string)value);
						break;
					case LastSaveProperty.LastSaveSavePath:
						properties.Add(key, (string)value);
						break;
					default:
						Debug.LogError("Unknown last save property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return properties;
		}

		public LastSaveProperties GetLastSaveProperties() {
			string lastSaveFilePath = GetPersistentDataPath() + "/lastsave.snowship";
			if (!File.Exists(lastSaveFilePath)) {
				return null;
			}

			Dictionary<LastSaveProperty, string> lastSaveProperties = LoadLastSave(lastSaveFilePath);

			foreach (string path in lastSaveProperties.Values) {
				if (path == null) {
					return null;
				}
			}

			return new LastSaveProperties(
				lastSaveProperties[LastSaveProperty.LastSaveUniversePath],
				lastSaveProperties[LastSaveProperty.LastSavePlanetPath],
				lastSaveProperties[LastSaveProperty.LastSaveColonyPath],
				lastSaveProperties[LastSaveProperty.LastSaveSavePath]
			);
		}

		public bool IsLastSaveUniverseLoadable() {
			LastSaveProperties lastSaveProperties = GetLastSaveProperties();

			if (lastSaveProperties == null) {
				return false;
			}

			PersistenceUniverse persistenceUniverse = pUniverse.GetPersistenceUniverses().Find(pu => string.Equals(Path.GetFullPath(pu.path), Path.GetFullPath(lastSaveProperties.lastSaveUniversePath), StringComparison.OrdinalIgnoreCase));

			if (persistenceUniverse == null) {
				return false;
			}

			return pUniverse.IsUniverseLoadable(persistenceUniverse);
		}
	}
}
