using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NPersistence {
	public class PSettings {

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

		// Settings Saving

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

	[SuppressMessage("ReSharper", "ParameterHidesMember")]
	public class SettingsState {

		public Resolution Resolution { get; private set; }
		public bool Fullscreen { get; private set; }
		public CanvasScaler.ScaleMode ScaleMode { get; private set; }

		public enum Setting {
			Resolution,
			Fullscreen,
			ScaleMode
		}

		public SettingsState() {
			SetResolution(Screen.currentResolution);
			Fullscreen = true;
			ScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		}

		public void SetResolution(Resolution resolution) {
			Resolution = resolution;
		}

		public void SetFullscreen(bool fullscreen) {
			Fullscreen = fullscreen;
		}

		public void SetScaleMode(CanvasScaler.ScaleMode scaleMode) {
			ScaleMode = scaleMode;
		}

		public void ApplySettings() {
			Screen.SetResolution(
				Resolution.width,
				Resolution.height,
				Fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
				Resolution.refreshRateRatio
			);

			GameManager.uiMOld.canvas.GetComponent<CanvasScaler>().uiScaleMode = ScaleMode;
		}

		public void LoadSetting(Setting setting, string value) {
			stringToSettingFunctions[setting](this, value);
		}

		public void SaveSetting(Setting setting, StreamWriter file) {
			file.WriteLine("<" + setting + ">" + settingToStringFunctions[setting](this));
		}

		private static readonly Dictionary<Setting, Func<SettingsState, string>> settingToStringFunctions = new Dictionary<Setting, Func<SettingsState, string>> {
			{ Setting.Resolution, settingsState => $"{settingsState.Resolution.width},{settingsState.Resolution.height},{settingsState.Resolution.refreshRateRatio.value}" },
			{ Setting.Fullscreen, settingsState => settingsState.Fullscreen.ToString() },
			{ Setting.ScaleMode, settingsState => settingsState.ScaleMode.ToString() }
		};

		private static readonly Dictionary<Setting, Action<SettingsState, string>> stringToSettingFunctions = new Dictionary<Setting, Action<SettingsState, string>> {
			{
				Setting.Resolution, delegate(SettingsState settingsState, string value) {
					string[] split = value.Split(',');
					int width = int.Parse(split[0]);
					int height = int.Parse(split[1]);
					double refreshRate = double.Parse(split[2]);

					for (int i = Screen.resolutions.Length - 1; i >= 0; i--) {
						Resolution resolution = Screen.resolutions[i];
						Debug.Log(resolution.ToString());

						if (resolution.width != width) {
							continue;
						}

						if (resolution.height != height) {
							continue;
						}

						if (resolution.refreshRateRatio.value - refreshRate > 0.01d) {
							continue;
						}

						settingsState.SetResolution(resolution);
						return;
					}

					settingsState.SetResolution(Screen.currentResolution);
				}
			}, {
				Setting.Fullscreen, delegate(SettingsState settingsState, string value) {
					settingsState.Fullscreen = bool.Parse(value);
				}
			}, {
				Setting.ScaleMode, delegate(SettingsState settingsState, string value) {
					settingsState.ScaleMode = (CanvasScaler.ScaleMode)Enum.Parse(typeof(CanvasScaler.ScaleMode), value);
				}
			}
		};

		public override string ToString() {
			return $"{nameof(Resolution)}: {Resolution}, {nameof(Fullscreen)}: {Fullscreen}, {nameof(ScaleMode)}: {ScaleMode}";
		}

		public void CopyValuesTo(SettingsState other) {
			other.Resolution = Resolution;
			other.Fullscreen = Fullscreen;
			other.ScaleMode = ScaleMode;
		}

		public bool Equals(SettingsState other) {
			if (other == null) {
				return false;
			}

			if (!Resolution.Equals(other.Resolution)) {
				return false;
			}

			if (!Fullscreen.Equals(other.Fullscreen)) {
				return false;
			}

			if (!ScaleMode.Equals(other.ScaleMode)) {
				return false;
			}

			return true;
		}
	}
}
