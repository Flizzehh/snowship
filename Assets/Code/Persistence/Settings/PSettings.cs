using System;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PSettings : PersistenceHandler {

		public SettingsState SettingsState;
		public SettingsState NewSettingsState;

		public void CreateSettingsState() {
			SettingsState = new SettingsState();
			NewSettingsState = new SettingsState();

			if (!LoadSettings()) {
				SaveSettings();
			}

			ApplySettings();
		}

		private string GenerateSettingsDirectoryPath() {
			return Application.persistentDataPath + "/Settings/";
		}

		private string GenerateSettingsFilePath() {
			return GenerateSettingsDirectoryPath() + "settings.snowship";
		}

		private void SaveSettings() {
			Directory.CreateDirectory(GenerateSettingsDirectoryPath());

			string settingsFilePath = GenerateSettingsFilePath();
			FileStream createFile = File.Create(settingsFilePath);
			createFile.Close();

			File.WriteAllText(settingsFilePath, string.Empty);

			StreamWriter file = new StreamWriter(settingsFilePath);

			foreach (SettingsState.Setting setting in Enum.GetValues(typeof(SettingsState.Setting))) {
				SettingsState.SaveSetting(setting, file);
			}

			file.Close();
		}

		private bool LoadSettings() {
			StreamReader file;
			try {
				file = new StreamReader(GenerateSettingsFilePath());
			} catch {
				return false;
			}

			foreach (string settingsFileLine in file.ReadToEnd().Split('\n')) {
				if (string.IsNullOrEmpty(settingsFileLine)) {
					continue;
				}

				string key = settingsFileLine.Split('>')[0].Replace("<", string.Empty);
				SettingsState.Setting setting = (SettingsState.Setting)Enum.Parse(typeof(SettingsState.Setting), key);

				string value = settingsFileLine.Split('>')[1];

				SettingsState.LoadSetting(setting, value);
			}

			file.Close();

			return true;
		}

		public void ApplySettings() {
			NewSettingsState.ApplySettings();
			NewSettingsState.CopyValuesTo(SettingsState);
			SaveSettings();
		}
	}
}
