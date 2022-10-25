using Snowship.Job;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PersistenceManager : BaseManager {

	private GameManager startCoroutineReference;

	public void SetStartCoroutineReference(GameManager startCoroutineReference) {
		this.startCoroutineReference = startCoroutineReference;
	}

	public static readonly KeyValuePair<int, string> gameVersion = new KeyValuePair<int, string>(3, "2022.1");
	public static readonly KeyValuePair<int, string> saveVersion = new KeyValuePair<int, string>(3, "2022.1");

	public SettingsState settingsState;

	public void CreateSettingsState() {
		settingsState = new SettingsState();
		if (!LoadSettings()) {
			SaveSettings();
		}
		settingsState.ApplySettings();
	}

	// Settings Saving

	public class SettingsState {
		public Resolution resolution;
		public int resolutionWidth;
		public int resolutionHeight;
		public int refreshRate;
		public bool fullscreen;
		public CanvasScaler.ScaleMode scaleMode;

		public SettingsState() {
			resolution = Screen.currentResolution;
			resolutionWidth = Screen.currentResolution.width;
			resolutionHeight = Screen.currentResolution.height;
			refreshRate = Screen.currentResolution.refreshRate;
			fullscreen = true;
			scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		}

		public enum Setting {
			ResolutionWidth,
			ResolutionHeight,
			RefreshRate,
			Fullscreen,
			ScaleMode
		}

		private static readonly Dictionary<Setting, Func<SettingsState, string>> settingToStringFunctions = new Dictionary<Setting, Func<SettingsState, string>>() {
			{ Setting.ResolutionWidth, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.resolutionWidth.ToString(); }) },
			{ Setting.ResolutionHeight, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.resolutionHeight.ToString(); }) },
			{ Setting.RefreshRate, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.refreshRate.ToString(); }) },
			{ Setting.Fullscreen, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.fullscreen.ToString(); }) },
			{ Setting.ScaleMode, new Func<SettingsState, string>(delegate (SettingsState settingsState) { return settingsState.scaleMode.ToString(); }) }
		};

		private static readonly Dictionary<Setting, Action<SettingsState, string>> stringToSettingFunctions = new Dictionary<Setting, Action<SettingsState, string>>() {
			{ Setting.ResolutionWidth, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.resolutionWidth = int.Parse(value); }) },
			{ Setting.ResolutionHeight, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.resolutionHeight = int.Parse(value); }) },
			{ Setting.RefreshRate, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.refreshRate = int.Parse(value); }) },
			{ Setting.Fullscreen, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.fullscreen = bool.Parse(value); }) },
			{ Setting.ScaleMode, new Action<SettingsState, string>(delegate (SettingsState settingsState, string value) { settingsState.scaleMode = (CanvasScaler.ScaleMode)Enum.Parse(typeof(CanvasScaler.ScaleMode), value); }) }
		};

		public void ApplySettings() {
			Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen, refreshRate);

			GameManager.uiM.canvas.GetComponent<CanvasScaler>().uiScaleMode = scaleMode;

			GameManager.uiM.SetMainMenuBackground(false);
		}

		public void LoadSetting(Setting setting, string value) {
			stringToSettingFunctions[setting](this, value);
		}

		public void SaveSetting(Setting setting, StreamWriter file) {
			file.WriteLine("<" + setting + ">" + settingToStringFunctions[setting](this));
		}
	}

	public string GenerateSettingsDirectoryPath() {
		return Application.persistentDataPath + "/Settings/";
	}

	public string GenerateSettingsFilePath() {
		return GenerateSettingsDirectoryPath() + "settings.snowship";
	}

	public void SaveSettings() {
		Directory.CreateDirectory(GenerateSettingsDirectoryPath());

		string settingsFilePath = GenerateSettingsFilePath();
		FileStream createFile = File.Create(settingsFilePath);
		createFile.Close();

		File.WriteAllText(settingsFilePath, string.Empty);

		StreamWriter file = new StreamWriter(settingsFilePath);

		foreach (SettingsState.Setting setting in Enum.GetValues(typeof(SettingsState.Setting))) {
			settingsState.SaveSetting(setting, file);
		}

		file.Close();
	}

	public bool LoadSettings() {
		StreamReader file;
		try {
			file = new StreamReader(GenerateSettingsFilePath());
			if (file == null) {
				return false;
			}
		} catch {
			return false;
		}

		foreach (string settingsFileLine in file.ReadToEnd().Split('\n')) {
			if (!string.IsNullOrEmpty(settingsFileLine)) {
				string key = settingsFileLine.Split('>')[0].Replace("<", string.Empty);
				SettingsState.Setting setting = (SettingsState.Setting)Enum.Parse(typeof(SettingsState.Setting), key);

				string value = settingsFileLine.Split('>')[1];

				settingsState.LoadSetting(setting, value);
			}
		}

		file.Close();

		return true;
	}

	public void ApplySettings() {
		settingsState.ApplySettings();
		SaveSettings();
	}

	// Game Saving

	public static string GenerateDateTimeString() {
		DateTime now = DateTime.Now;
		string dateTime = string.Format(
			"{0}{1}{2}{3}{4}{5}{6}",
			now.Year.ToString().PadLeft(4, '0'),
			now.Month.ToString().PadLeft(2, '0'),
			now.Day.ToString().PadLeft(2, '0'),
			now.Hour.ToString().PadLeft(2, '0'),
			now.Minute.ToString().PadLeft(2, '0'),
			now.Second.ToString().PadLeft(2, '0'),
			now.Millisecond.ToString().PadLeft(4, '0')
		);
		return dateTime;
	}

	public static string GenerateSaveDateTimeString() {
		DateTime now = DateTime.Now;
		return string.Format(
			"{0}:{1}:{2} {3}/{4}/{5}",
			now.Hour.ToString().PadLeft(2, '0'),
			now.Minute.ToString().PadLeft(2, '0'),
			now.Second.ToString().PadLeft(2, '0'),
			now.Day.ToString().PadLeft(2, '0'),
			now.Month.ToString().PadLeft(2, '0'),
			now.Year.ToString().PadLeft(4, '0')
		);
	}

	IEnumerator CreateScreenshot(string fileName) {
		GameObject canvas = GameObject.Find("Canvas");
		canvas.SetActive(false);
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(fileName + ".png");
		canvas.SetActive(true);
	}

	private Sprite LoadSpriteFromImageFile(string path, int x = 280, int y = 158) {
		if (File.Exists(path)) {
			byte[] fileData = File.ReadAllBytes(path);
			Texture2D texture = new Texture2D(x, y);
			texture.LoadImage(fileData);
			return Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
		}
		return null;
	}

	public Sprite LoadSaveImageFromSaveDirectoryPath(string saveDirectoryPath) {
		string screenshotPath = Directory.GetFiles(saveDirectoryPath).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
		if (screenshotPath != null) {
			return LoadSpriteFromImageFile(screenshotPath);
		}
		return null;
	}

	public static string CreateKeyValueString(object key, object value, int level) {
		return (new string('\t', level)) + "<" + key.ToString() + ">" + value.ToString();
	}

	public static string FormatVector2ToString(Vector2 vector2) {
		return vector2.x + "," + vector2.y;
	}

	public static StreamWriter CreateFileAtDirectory(string directory, string fileName) {
		string filePath = directory + "/" + fileName;
		FileStream fileStream = File.Create(filePath);
		fileStream.Close();

		return new StreamWriter(filePath);
	}

	public static List<KeyValuePair<string, object>> GetKeyValuePairsFromFile(string path) {
		return GetKeyValuePairsFromLines(File.ReadAllLines(path).ToList());
	}

	public static List<KeyValuePair<string, object>> GetKeyValuePairsFromLines(List<string> lines) {
		List<KeyValuePair<string, object>> properties = new List<KeyValuePair<string, object>>();
		while (true) {
			if (lines.Count <= 0) {
				break;
			}
			string line = lines[0];
			lines.RemoveAt(0);

			string key = line.Split('>')[0].Replace("<", string.Empty).Replace("\t", string.Empty);
			string value = line.Split('>')[1].Replace("\n", string.Empty).Replace("\r", string.Empty);

			key = Regex.Replace(key, @"\n|\r", string.Empty);
			value = Regex.Replace(value, @"\n|\r", string.Empty);

			if (string.IsNullOrEmpty(value)) {
				properties.Add(new KeyValuePair<string, object>(key, GetSubPropertiesFromProperty(lines, 1)));
			} else {
				properties.Add(new KeyValuePair<string, object>(key, value));
			}
		}
		return properties;
	}

	public static List<KeyValuePair<string, object>> GetSubPropertiesFromProperty(List<string> lines, int level) {
		List<KeyValuePair<string, object>> properties = new List<KeyValuePair<string, object>>();
		while (true) {
			if (lines.Count <= 0) {
				break;
			}

			string line = lines[0];

			string key = line.Split('>')[0].Replace("<", string.Empty);
			if (key.Length - key.Replace("\t", string.Empty).Length < level) {
				break;
			} else {
				lines.RemoveAt(0);
			}
			key = key.Replace("\t", string.Empty);
			string value = line.Split('>')[1].Replace("\n", string.Empty).Replace("\r", string.Empty);

			key = Regex.Replace(key, @"\n|\r", string.Empty);
			value = Regex.Replace(value, @"\n|\r", string.Empty);

			if (string.IsNullOrEmpty(value)) {
				properties.Add(new KeyValuePair<string, object>(key, GetSubPropertiesFromProperty(lines, level + 1)));
			} else {
				properties.Add(new KeyValuePair<string, object>(key, value));
			}
		}
		return properties;
	}

	public static string GetPersistentDataPath() {
		return Application.persistentDataPath;
	}

	public static string GenerateUniversesPath() {
		return GetPersistentDataPath() + "/Universes";
	}

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
		LastSaveUniversePath, LastSavePlanetPath, LastSaveColonyPath, LastSaveSavePath
	}

	private void UpdateLastSave(LastSaveProperties lastSaveProperties) {
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

	public void CreateUniverse(UniverseManager.Universe universe) {
		if (universe == null) {
			Debug.LogError("Universe to be saved is null.");
			return;
		}

		string universesDirectoryPath = GenerateUniversesPath();
		string dateTimeString = GenerateDateTimeString();
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
			CreateFileAtDirectory(universe.directory, "configuration.snowship").Close();
		}
		StreamWriter configurationFile = new StreamWriter(configurationFilePath);
		SaveConfiguration(configurationFile);
		configurationFile.Close();

		string universeFilePath = universe.directory + "/universe.snowship";
		if (File.Exists(universeFilePath)) {
			File.WriteAllText(universeFilePath, string.Empty);
		} else {
			CreateFileAtDirectory(universe.directory, "universe.snowship").Close();
		}
		StreamWriter universeFile = new StreamWriter(universeFilePath);
		SaveUniverse(universeFile, universe);
		universeFile.Close();
	}

	public void CreatePlanet(PlanetManager.Planet planet) {
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

	public void UpdatePlanetSave(PlanetManager.Planet planet) {
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

	public void CreateCity() {

	}

	public void CreateColony(ColonyManager.Colony colony) {
		if (colony == null) {
			Debug.LogError("Colony to be saved is null.");
			return;
		}

		if (GameManager.planetM.planet == null) {
			Debug.LogError("Planet to save the colony to is null.");
			return;
		}

		if (string.IsNullOrEmpty(GameManager.planetM.planet.directory)) {
			Debug.LogError("Planet directory is null or empty.");
			return;
		}

		string coloniesDirectoryPath = GameManager.planetM.planet.directory + "/Colonies";
		string dateTimeString = GenerateDateTimeString();
		string colonyDirectoryPath = coloniesDirectoryPath + "/Colony-" + dateTimeString;
		Directory.CreateDirectory(colonyDirectoryPath);
		colony.SetDirectory(colonyDirectoryPath);

		UpdateColonySave(colony);

		string mapDirectoryPath = colonyDirectoryPath + "/Map";
		Directory.CreateDirectory(mapDirectoryPath);

		StreamWriter tilesFile = CreateFileAtDirectory(mapDirectoryPath, "tiles.snowship");
		SaveOriginalTiles(tilesFile);
		tilesFile.Close();

		StreamWriter riversFile = CreateFileAtDirectory(mapDirectoryPath, "rivers.snowship");
		SaveOriginalRivers(riversFile);
		riversFile.Close();

		string savesDirectoryPath = colonyDirectoryPath + "/Saves";
		Directory.CreateDirectory(savesDirectoryPath);
	}

	public void UpdateColonySave(ColonyManager.Colony colony) {
		if (colony == null) {
			Debug.LogError("Colony to be saved is null.");
			return;
		}

		if (GameManager.planetM.planet == null) {
			Debug.LogError("Planet to save the colony to is null.");
			return;
		}

		if (string.IsNullOrEmpty(GameManager.planetM.planet.directory)) {
			Debug.LogError("Planet directory is null or empty.");
			return;
		}

		string colonyFilePath = colony.directory + "/colony.snowship";
		if (File.Exists(colonyFilePath)) {
			File.WriteAllText(colonyFilePath, string.Empty);
		} else {
			CreateFileAtDirectory(colony.directory, "colony.snowship").Close();
		}
		StreamWriter colonyFile = new StreamWriter(colonyFilePath);
		SaveColony(colonyFile, colony);
		colonyFile.Close();
	}

	public void CreateSave(ColonyManager.Colony colony) {
		string savesDirectoryPath = colony.directory + "/Saves";
		string dateTimeString = GenerateDateTimeString();
		string saveDirectoryPath = savesDirectoryPath + "/Save-" + dateTimeString;

		try {
			Directory.CreateDirectory(saveDirectoryPath);

			StreamWriter cameraFile = CreateFileAtDirectory(saveDirectoryPath, "camera.snowship");
			SaveCamera(cameraFile);
			cameraFile.Close();

			StreamWriter caravansFile = CreateFileAtDirectory(saveDirectoryPath, "caravans.snowship");
			SaveCaravans(caravansFile);
			caravansFile.Close();

			StreamWriter colonistsFile = CreateFileAtDirectory(saveDirectoryPath, "colonists.snowship");
			SaveColonists(colonistsFile);
			colonistsFile.Close();

			StreamWriter jobsFile = CreateFileAtDirectory(saveDirectoryPath, "jobs.snowship");
			SaveJobs(jobsFile);
			jobsFile.Close();

			StreamWriter objectsFile = CreateFileAtDirectory(saveDirectoryPath, "objects.snowship");
			SaveObjects(objectsFile);
			objectsFile.Close();

			StreamWriter resourcesFile = CreateFileAtDirectory(saveDirectoryPath, "resources.snowship");
			SaveResources(resourcesFile);
			resourcesFile.Close();

			StreamWriter riversFile = CreateFileAtDirectory(saveDirectoryPath, "rivers.snowship");
			SaveModifiedRivers(riversFile, LoadRivers(colony.directory + "/Map/rivers.snowship"));
			riversFile.Close();

			StreamWriter tilesFile = CreateFileAtDirectory(saveDirectoryPath, "tiles.snowship");
			SaveModifiedTiles(tilesFile, LoadTiles(colony.directory + "/Map/tiles.snowship"));
			tilesFile.Close();

			StreamWriter timeFile = CreateFileAtDirectory(saveDirectoryPath, "time.snowship");
			SaveTime(timeFile);
			timeFile.Close();

			StreamWriter uiFile = CreateFileAtDirectory(saveDirectoryPath, "ui.snowship");
			SaveUI(uiFile);
			uiFile.Close();

			string lastSaveDateTime = GenerateSaveDateTimeString();
			string lastSaveTimeChunk = GenerateDateTimeString();

			GameManager.universeM.universe.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
			UpdateUniverseSave(GameManager.universeM.universe);

			GameManager.planetM.planet.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
			UpdatePlanetSave(GameManager.planetM.planet);

			colony.SetLastSaveDateTime(lastSaveDateTime, lastSaveTimeChunk);
			UpdateColonySave(GameManager.colonyM.colony);

			StreamWriter saveFile = CreateFileAtDirectory(saveDirectoryPath, "save.snowship");
			SaveSave(saveFile, lastSaveDateTime);
			saveFile.Close();

			startCoroutineReference.StartCoroutine(CreateScreenshot(saveDirectoryPath + "/screenshot-" + dateTimeString));

			UpdateLastSave(new LastSaveProperties(
				GameManager.universeM.universe.directory,
				GameManager.planetM.planet.directory,
				GameManager.colonyM.colony.directory,
				saveDirectoryPath
			));
		} catch (Exception e) {
			throw e;
		}
	}

	public enum ConfigurationProperty {
		GameVersion, SaveVersion
	}

	public void SaveConfiguration(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(ConfigurationProperty.GameVersion, gameVersion.Value, 0));
		file.WriteLine(CreateKeyValueString(ConfigurationProperty.SaveVersion, saveVersion.Value, 0));
	}

	public Dictionary<ConfigurationProperty, string> LoadConfiguration(string path) {
		Dictionary<ConfigurationProperty, string> properties = new Dictionary<ConfigurationProperty, string>();

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
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

	}

	public enum UniverseProperty {
		Name, LastSaveDateTime, LastSaveTimeChunk
	}

	public void SaveUniverse(StreamWriter file, UniverseManager.Universe universe) {
		file.WriteLine(CreateKeyValueString(UniverseProperty.LastSaveDateTime, universe.lastSaveDateTime, 0));
		file.WriteLine(CreateKeyValueString(UniverseProperty.LastSaveTimeChunk, universe.lastSaveTimeChunk, 0));

		file.WriteLine(CreateKeyValueString(UniverseProperty.Name, universe.name, 0));
	}

	public class PersistenceUniverse {
		public string path;

		public Dictionary<ConfigurationProperty, string> configurationProperties;
		public Dictionary<UniverseProperty, string> universeProperties;

		public PersistenceUniverse(string path) {
			this.path = path;

			configurationProperties = GameManager.persistenceM.LoadConfiguration(path + "/configuration.snowship");
			universeProperties = GameManager.persistenceM.LoadUniverse(path + "/universe.snowship");
		}
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

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			UniverseProperty key = (UniverseProperty)Enum.Parse(typeof(UniverseProperty), property.Key);
			switch (key) {
				case UniverseProperty.LastSaveDateTime:
					properties.Add(key, (string)property.Value);
					break;
				case UniverseProperty.LastSaveTimeChunk:
					properties.Add(key, (string)property.Value);
					break;
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
		UniverseManager.Universe universe = new UniverseManager.Universe(persistenceUniverse.universeProperties[UniverseProperty.Name]) {
			directory = persistenceUniverse.path,
			lastSaveDateTime = persistenceUniverse.universeProperties[UniverseProperty.LastSaveDateTime],
			lastSaveTimeChunk = persistenceUniverse.universeProperties[UniverseProperty.LastSaveTimeChunk]
		};
		GameManager.universeM.SetUniverse(universe);
	}

	public enum PlanetProperty {
		LastSaveDateTime, LastSaveTimeChunk, Name, Seed, Size, SunDistance, TempRange, RandomOffsets, WindDirection
	}

	public void SavePlanet(StreamWriter file, PlanetManager.Planet planet) {
		file.WriteLine(CreateKeyValueString(PlanetProperty.LastSaveDateTime, planet.lastSaveDateTime, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.LastSaveTimeChunk, planet.lastSaveTimeChunk, 0));

		file.WriteLine(CreateKeyValueString(PlanetProperty.Name, planet.name, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.Seed, planet.mapData.mapSeed, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.Size, planet.mapData.mapSize, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.SunDistance, planet.mapData.planetDistance, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.TempRange, planet.mapData.temperatureRange, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.RandomOffsets, planet.mapData.randomOffsets, 0));
		file.WriteLine(CreateKeyValueString(PlanetProperty.WindDirection, planet.mapData.primaryWindDirection, 0));
	}

	public class PersistencePlanet {
		public string path;

		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public int seed;
		public int size;
		public float sunDistance;
		public int temperatureRange;
		public bool randomOffsets;
		public int windDirection;

		public PersistencePlanet(string path) {
			this.path = path;
		}
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

	public PlanetManager.Planet ApplyLoadedPlanet(PersistencePlanet persistencePlanet) {
		PlanetManager.Planet planet = GameManager.planetM.CreatePlanet(
			persistencePlanet.name,
			persistencePlanet.seed,
			persistencePlanet.size,
			persistencePlanet.sunDistance,
			persistencePlanet.temperatureRange,
			persistencePlanet.randomOffsets,
			persistencePlanet.windDirection
		);
		planet.SetLastSaveDateTime(persistencePlanet.lastSaveDateTime, persistencePlanet.lastSaveTimeChunk);
		planet.SetDirectory(Directory.GetParent(persistencePlanet.path).FullName);
		return planet;
	}

	public enum ColonyProperty {
		LastSaveDateTime, LastSaveTimeChunk, Name, PlanetPosition, Seed, Size, AverageTemperature, AveragePrecipitation, TerrainTypeHeights, SurroundingPlanetTileHeights, OnRiver, SurroundingPlanetTileRivers
	}

	public void SaveColony(StreamWriter file, ColonyManager.Colony colony) {
		file.WriteLine(CreateKeyValueString(ColonyProperty.LastSaveDateTime, colony.lastSaveDateTime, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.LastSaveTimeChunk, colony.lastSaveTimeChunk, 0));

		file.WriteLine(CreateKeyValueString(ColonyProperty.Name, colony.name, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.PlanetPosition, FormatVector2ToString(colony.map.mapData.planetTilePosition), 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.Seed, colony.map.mapData.mapSeed, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.Size, colony.map.mapData.mapSize, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.AverageTemperature, colony.map.mapData.averageTemperature, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.AveragePrecipitation, colony.map.mapData.averagePrecipitation, 0));

		file.WriteLine(CreateKeyValueString(ColonyProperty.TerrainTypeHeights, string.Empty, 0));
		foreach (KeyValuePair<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeight in colony.map.mapData.terrainTypeHeights) {
			file.WriteLine(CreateKeyValueString(terrainTypeHeight.Key, terrainTypeHeight.Value, 1));
		}

		file.WriteLine(CreateKeyValueString(ColonyProperty.SurroundingPlanetTileHeights, string.Join(",", colony.map.mapData.surroundingPlanetTileHeightDirections.Select(i => i.ToString()).ToArray()), 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.OnRiver, colony.map.mapData.isRiver, 0));
		file.WriteLine(CreateKeyValueString(ColonyProperty.SurroundingPlanetTileRivers, string.Join(",", colony.map.mapData.surroundingPlanetTileRivers.Select(i => i.ToString()).ToArray()), 0));
	}

	public class PersistenceColony {
		public string path;

		public Sprite lastSaveImage;

		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public Vector2 planetPosition;
		public int seed;
		public int size;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeights = new Dictionary<TileManager.TileTypeGroup.TypeEnum, float>();
		public List<int> surroundingPlanetTileHeights = new List<int>();
		public bool onRiver;
		public List<int> surroundingPlanetTileRivers = new List<int>();

		public PersistenceColony(string path) {
			this.path = path;
		}

		public void SetLastSaveImage(Sprite lastSaveImage) {
			this.lastSaveImage = lastSaveImage;
		}
	}

	public List<PersistenceColony> GetPersistenceColonies() {
		List<PersistenceColony> persistenceColonies = new List<PersistenceColony>();
		string coloniesPath = GameManager.planetM.planet.directory + "/Colonies";
		if (Directory.Exists(coloniesPath)) {
			foreach (string colonyDirectoryPath in Directory.GetDirectories(coloniesPath)) {
				PersistenceColony persistenceColony = LoadColony(colonyDirectoryPath + "/colony.snowship");

				string savesDirectoryPath = colonyDirectoryPath + "/Saves";
				List<string> saveDirectories = Directory.GetDirectories(savesDirectoryPath).OrderByDescending(sd => sd).ToList();
				if (saveDirectories.Count > 0) {
					string screenshotPath = Directory.GetFiles(saveDirectories[0]).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
					if (screenshotPath != null) {
						persistenceColony.lastSaveImage = LoadSpriteFromImageFile(screenshotPath);
					}
				}
				persistenceColonies.Add(persistenceColony);
			}
		}
		persistenceColonies = persistenceColonies.OrderByDescending(pc => pc.lastSaveTimeChunk).ToList();
		return persistenceColonies;
	}

	public PersistenceColony LoadColony(string path) {

		PersistenceColony persistenceColony = new PersistenceColony(path);

		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			switch ((ColonyProperty)Enum.Parse(typeof(ColonyProperty), property.Key)) {
				case ColonyProperty.LastSaveDateTime:
					persistenceColony.lastSaveDateTime = (string)property.Value;
					break;
				case ColonyProperty.LastSaveTimeChunk:
					persistenceColony.lastSaveTimeChunk = (string)property.Value;
					break;
				case ColonyProperty.Name:
					persistenceColony.name = (string)property.Value;
					break;
				case ColonyProperty.PlanetPosition:
					persistenceColony.planetPosition = new Vector2(float.Parse(((string)property.Value).Split(',')[0]), float.Parse(((string)property.Value).Split(',')[1]));
					break;
				case ColonyProperty.Seed:
					persistenceColony.seed = int.Parse((string)property.Value);
					break;
				case ColonyProperty.Size:
					persistenceColony.size = int.Parse((string)property.Value);
					break;
				case ColonyProperty.AverageTemperature:
					persistenceColony.averageTemperature = float.Parse((string)property.Value);
					break;
				case ColonyProperty.AveragePrecipitation:
					persistenceColony.averagePrecipitation = float.Parse((string)property.Value);
					break;
				case ColonyProperty.TerrainTypeHeights:
					foreach (KeyValuePair<string, object> terrainTypeHeightProperty in (List<KeyValuePair<string, object>>)property.Value) {
						TileManager.TileTypeGroup.TypeEnum terrainTypeHeightPropertyKey = (TileManager.TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileManager.TileTypeGroup.TypeEnum), terrainTypeHeightProperty.Key);
						persistenceColony.terrainTypeHeights.Add(terrainTypeHeightPropertyKey, float.Parse((string)terrainTypeHeightProperty.Value));
					}
					break;
				case ColonyProperty.SurroundingPlanetTileHeights:
					foreach (string height in ((string)property.Value).Split(',')) {
						persistenceColony.surroundingPlanetTileHeights.Add(int.Parse(height));
					}
					break;
				case ColonyProperty.OnRiver:
					persistenceColony.onRiver = bool.Parse((string)property.Value);
					break;
				case ColonyProperty.SurroundingPlanetTileRivers:
					foreach (string riverIndex in ((string)property.Value).Split(',')) {
						persistenceColony.surroundingPlanetTileRivers.Add(int.Parse(riverIndex));
					}
					break;
				default:
					Debug.LogError("Unknown colony property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceColony;
	}

	public ColonyManager.Colony ApplyLoadedColony(PersistenceColony persistenceColony) {
		ColonyManager.Colony colony = GameManager.colonyM.CreateColony(
			persistenceColony.name,
			persistenceColony.planetPosition,
			persistenceColony.seed,
			persistenceColony.size,
			persistenceColony.averageTemperature,
			persistenceColony.averagePrecipitation,
			persistenceColony.terrainTypeHeights,
			persistenceColony.surroundingPlanetTileHeights,
			persistenceColony.onRiver,
			persistenceColony.surroundingPlanetTileRivers
		);
		colony.SetLastSaveDateTime(persistenceColony.lastSaveDateTime, persistenceColony.lastSaveTimeChunk);
		colony.SetDirectory(Directory.GetParent(persistenceColony.path).FullName);

		GameManager.colonyM.SetColony(colony);

		return colony;
	}

	public enum SaveProperty {
		SaveDateTime
	}

	public void SaveSave(StreamWriter file, string saveDateTime) {
		file.WriteLine(CreateKeyValueString(SaveProperty.SaveDateTime, saveDateTime, 0));
	}

	public class PersistenceSave {

		public string path;

		public Sprite image;

		public string saveDateTime;

		public bool loadable;

		public PersistenceSave(string path) {
			this.path = path;

			loadable = true;
		}
	}

	public List<PersistenceSave> GetPersistenceSaves() {
		List<PersistenceSave> persistenceSaves = new List<PersistenceSave>();
		string savesPath = GameManager.colonyM.colony.directory + "/Saves";
		if (Directory.Exists(savesPath)) {
			foreach (string saveDirectoryPath in Directory.GetDirectories(savesPath)) {
				PersistenceSave persistenceSave = LoadSave(saveDirectoryPath + "/save.snowship");

				string screenshotPath = Directory.GetFiles(saveDirectoryPath).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
				if (screenshotPath != null) {
					persistenceSave.image = LoadSpriteFromImageFile(screenshotPath);
				}
				persistenceSaves.Add(persistenceSave);
			}
		}
		persistenceSaves = persistenceSaves.OrderByDescending(ps => ps.path).ToList();
		return persistenceSaves;
	}

	public PersistenceSave LoadSave(string path) {
		PersistenceSave persistenceSave = new PersistenceSave(path);

		List<KeyValuePair<string, object>> properties;
		try {
			properties = GetKeyValuePairsFromFile(path);
		} catch (Exception e) {
			Debug.LogError(e.ToString());
			persistenceSave.loadable = false;
			return persistenceSave;
		}
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((SaveProperty)Enum.Parse(typeof(SaveProperty), property.Key)) {
				case SaveProperty.SaveDateTime:
					persistenceSave.saveDateTime = (string)property.Value;
					break;
				default:
					Debug.LogError("Unknown save property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceSave;
	}

	public enum LoadingState {
		NothingLoaded,
		LoadingCamera, LoadedCamera,
		LoadingTime, LoadedTime,
		LoadingResources, LoadedResources,
		LoadingMap, LoadedMap,
		LoadingObjects, LoadedObjects,
		LoadingCaravans, LoadedCaravans,
		LoadingJobs, LoadedJobs,
		LoadingColonists, LoadedColonists,
		LoadingUI, LoadedUI,
		FinishedLoading
	}

	public LoadingState loadingState;

	public IEnumerator ApplyLoadedSave(PersistenceSave persistenceSave) {
		loadingState = LoadingState.NothingLoaded;
		if (persistenceSave != null) {
			GameManager.tileM.mapState = TileManager.MapState.Generating;

			GameManager.uiM.SetMainMenuActive(false);
			GameManager.uiM.SetLoadingScreenActive(true);
			GameManager.uiM.SetGameUIActive(false);

			GameManager.uiM.UpdateLoadingStateText("Loading Colony", string.Empty); yield return null;
			GameManager.colonyM.LoadColony(GameManager.colonyM.colony, false);

			string saveDirectoryPath = Directory.GetParent(persistenceSave.path).FullName;

			GameManager.timeM.SetPaused(true);

			loadingState = LoadingState.LoadingCamera;
			GameManager.uiM.UpdateLoadingStateText("Loading Camera", string.Empty); yield return null;
			LoadCamera(saveDirectoryPath + "/camera.snowship");
			while (loadingState != LoadingState.LoadedCamera) { yield return null; }

			loadingState = LoadingState.LoadingTime;
			GameManager.uiM.UpdateLoadingStateText("Loading Time", string.Empty); yield return null;
			LoadTime(saveDirectoryPath + "/time.snowship");
			while (loadingState != LoadingState.LoadedTime) { yield return null; }

			loadingState = LoadingState.LoadingResources;
			GameManager.uiM.UpdateLoadingStateText("Loading Resources", string.Empty); yield return null;
			LoadResources(saveDirectoryPath + "/resources.snowship");
			while (loadingState != LoadingState.LoadedResources) { yield return null; }

			loadingState = LoadingState.LoadingMap;
			GameManager.uiM.UpdateLoadingStateText("Loading Original Map", string.Empty); yield return null;
			GameManager.colonyM.colony.map = new TileManager.Map() { mapData = GameManager.colonyM.colony.mapData };
			TileManager.Map map = GameManager.colonyM.colony.map;

			List<PersistenceTile> originalTiles = LoadTiles(GameManager.colonyM.colony.directory + "/Map/tiles.snowship");
			List<PersistenceRiver> originalRivers = LoadRivers(GameManager.colonyM.colony.directory + "/Map/rivers.snowship");

			GameManager.uiM.UpdateLoadingStateText("Loading Modified Map", string.Empty); yield return null;
			List<PersistenceTile> modifiedTiles = LoadTiles(saveDirectoryPath + "/tiles.snowship");
			List<PersistenceRiver> modifiedRivers = LoadRivers(saveDirectoryPath + "/rivers.snowship");

			GameManager.uiM.UpdateLoadingStateText("Applying Changes to Map", string.Empty); yield return null;
			ApplyLoadedTiles(originalTiles, modifiedTiles, map);
			ApplyLoadedRivers(originalRivers, modifiedRivers, map);
			while (loadingState != LoadingState.LoadedMap) { yield return null; }

			loadingState = LoadingState.LoadingObjects;
			GameManager.uiM.UpdateLoadingStateText("Loading Object Data", string.Empty); yield return null;
			List<PersistenceObject> persistenceObjects = LoadObjects(saveDirectoryPath + "/objects.snowship");
			ApplyLoadedObjects(persistenceObjects);
			while (loadingState != LoadingState.LoadedObjects) { yield return null; }

			loadingState = LoadingState.LoadingCaravans;
			GameManager.uiM.UpdateLoadingStateText("Loading Caravan Data", string.Empty); yield return null;
			List<PersistenceCaravan> persistenceCaravans = LoadCaravans(saveDirectoryPath + "/caravans.snowship");
			ApplyLoadedCaravans(persistenceCaravans);
			while (loadingState != LoadingState.LoadedCaravans) { yield return null; }

			loadingState = LoadingState.LoadingJobs;
			GameManager.uiM.UpdateLoadingStateText("Loading Job Data", string.Empty); yield return null;
			List<PersistenceJob> persistenceJobs = LoadJobs(saveDirectoryPath + "/jobs.snowship");
			ApplyLoadedJobs(persistenceJobs);
			while (loadingState != LoadingState.LoadedJobs) { yield return null; }

			loadingState = LoadingState.LoadingColonists;
			GameManager.uiM.UpdateLoadingStateText("Loading Colonist Data", string.Empty); yield return null;
			List<PersistenceColonist> persistenceColonists = LoadColonists(saveDirectoryPath + "/colonists.snowship");
			ApplyLoadedColonists(persistenceColonists);
			while (loadingState != LoadingState.LoadedColonists) { yield return null; }

			for (int i = 0; i < persistenceObjects.Count; i++) {
				PersistenceObject persistenceObject = persistenceObjects[i];
				ResourceManager.ObjectInstance objectInstance = GameManager.colonyM.colony.map.GetTileFromPosition(persistenceObject.zeroPointTilePosition.Value).objectInstances.Values.ToList().Find(o => o.prefab.type == persistenceObject.type);

				switch (objectInstance.prefab.instanceType) {
					case ResourceManager.ObjectInstanceType.Container:
						ResourceManager.Container container = (ResourceManager.Container)objectInstance;
						foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceObject.persistenceInventory.reservedResources) {
							foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
								container.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
							}
							container.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
						}
						break;
					case ResourceManager.ObjectInstanceType.CraftingObject:
						ResourceManager.CraftingObject craftingObject = (ResourceManager.CraftingObject)objectInstance;
						craftingObject.SetActive(persistenceObject.active.Value);
						break;
					case ResourceManager.ObjectInstanceType.SleepSpot:
						ResourceManager.SleepSpot sleepSpot = (ResourceManager.SleepSpot)objectInstance;
						if (persistenceObject.occupyingColonistName != null) {
							sleepSpot.occupyingColonist = GameManager.colonistM.colonists.Find(c => c.name == persistenceObject.occupyingColonistName);
						}
						break;
				}

				objectInstance.Update();
			}

			for (int i = 0; i < persistenceCaravans.Count; i++) {
				PersistenceCaravan persistenceCaravan = persistenceCaravans[i];
				CaravanManager.Caravan caravan = GameManager.caravanM.caravans[i];

				foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceCaravan.persistenceInventory.reservedResources) {
					foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
						caravan.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
					}
					caravan.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
				}

				for (int t = 0; t < caravan.traders.Count; t++) {
					PersistenceTrader persistenceTrader = persistenceCaravan.persistenceTraders[t];
					CaravanManager.Trader trader = caravan.traders[t];

					foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceTrader.persistenceHuman.persistenceInventory.reservedResources) {
						foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
							trader.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
						}
						trader.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
					}
				}
			}

			ApplyMapBitmasking(originalTiles, modifiedTiles, map);
			map.SetInitialRegionVisibility();

			loadingState = LoadingState.FinishedLoading;
			GameManager.tileM.mapState = TileManager.MapState.Generated;
			GameManager.uiM.SetGameUIActive(true);
			GameManager.uiM.SetLoadingScreenActive(false);
		} else {
			Debug.LogError("Unable to load a save without a save being selected.");
		}
	}

	public enum TileProperty {
		Tile, Index, Height, TileType, Temperature, Precipitation, Biome, Roof, Dug, Sprite, Plant
	}

	public enum PlantProperty {
		Type, Sprite, Small, GrowthProgress, HarvestResource, Integrity
	}

	public void SaveOriginalTiles(StreamWriter file) {
		foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
			file.WriteLine(CreateKeyValueString(TileProperty.Tile, string.Empty, 0));

			file.WriteLine(CreateKeyValueString(TileProperty.Height, tile.height, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.TileType, tile.tileType.type, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Temperature, tile.temperature, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Precipitation, tile.GetPrecipitation(), 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Biome, tile.biome.type, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Roof, tile.HasRoof(), 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Dug, tile.dugPreviously, 1));
			file.WriteLine(CreateKeyValueString(TileProperty.Sprite, tile.sr.sprite.name, 1));

			if (tile.plant != null) {
				file.WriteLine(CreateKeyValueString(TileProperty.Plant, string.Empty, 1));

				file.WriteLine(CreateKeyValueString(PlantProperty.Type, tile.plant.prefab.type, 2));
				file.WriteLine(CreateKeyValueString(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name, 2));
				file.WriteLine(CreateKeyValueString(PlantProperty.Small, tile.plant.small, 2));
				file.WriteLine(CreateKeyValueString(PlantProperty.GrowthProgress, tile.plant.growthProgress, 2));
				if (tile.plant.harvestResource != null) {
					file.WriteLine(CreateKeyValueString(PlantProperty.HarvestResource, tile.plant.harvestResource.type, 2));
				}
				file.WriteLine(CreateKeyValueString(PlantProperty.Integrity, tile.plant.integrity, 2));
			}
		}
	}

	public class PersistenceTile {
		public int? tileIndex;
		public float? tileHeight;
		public TileManager.TileType tileType;
		public float? tileTemperature;
		public float? tilePrecipitation;
		public TileManager.Biome tileBiome;
		public bool? tileRoof;
		public bool? tileDug;
		public string tileSpriteName;

		public ResourceManager.PlantPrefab plantPrefab;
		public string plantSpriteName;
		public bool? plantSmall;
		public float? plantGrowthProgress;
		public ResourceManager.Resource plantHarvestResource;
		public float? plantIntegrity;

		public PersistenceTile(
			int? tileIndex, float? tileHeight, TileManager.TileType tileType, float? tileTemperature, float? tilePrecipitation, TileManager.Biome tileBiome, bool? tileRoof, bool? tileDug, string tileSpriteName,
			ResourceManager.PlantPrefab plantPrefab, string plantSpriteName, bool? plantSmall, float? plantGrowthProgress, ResourceManager.Resource plantHarvestResource, float? plantIntegrity
		) {
			this.tileIndex = tileIndex;
			this.tileHeight = tileHeight;
			this.tileType = tileType;
			this.tileTemperature = tileTemperature;
			this.tilePrecipitation = tilePrecipitation;
			this.tileBiome = tileBiome;
			this.tileRoof = tileRoof;
			this.tileDug = tileDug;
			this.tileSpriteName = tileSpriteName;

			this.plantPrefab = plantPrefab;
			this.plantSpriteName = plantSpriteName;
			this.plantSmall = plantSmall;
			this.plantGrowthProgress = plantGrowthProgress;
			this.plantHarvestResource = plantHarvestResource;
			this.plantIntegrity = plantIntegrity;
		}
	}

	public List<PersistenceTile> LoadTiles(string path) {
		List<PersistenceTile> persistenceTiles = new List<PersistenceTile>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((TileProperty)Enum.Parse(typeof(TileProperty), property.Key)) {
				case TileProperty.Tile:
					int? tileIndex = null;
					float? tileHeight = null;
					TileManager.TileType tileType = null;
					float? tileTemperature = null;
					float? tilePrecipitation = null;
					TileManager.Biome tileBiome = null;
					bool? tileRoof = null;
					bool? tileDug = null;
					string tileSpriteName = null;

					ResourceManager.PlantPrefab plantPrefab = null;
					string plantSpriteName = null;
					bool? plantSmall = null;
					float? plantGrowthProgress = null;
					ResourceManager.Resource plantHarvestResource = null;
					float? plantIntegrity = null;

					foreach (KeyValuePair<string, object> tileProperty in (List<KeyValuePair<string, object>>)property.Value) {
						switch ((TileProperty)Enum.Parse(typeof(TileProperty), tileProperty.Key)) {
							case TileProperty.Index:
								tileIndex = int.Parse((string)tileProperty.Value);
								break;
							case TileProperty.Height:
								tileHeight = float.Parse((string)tileProperty.Value);
								break;
							case TileProperty.TileType:
								tileType = TileManager.TileType.GetTileTypeByString((string)tileProperty.Value);
								break;
							case TileProperty.Temperature:
								tileTemperature = float.Parse((string)tileProperty.Value);
								break;
							case TileProperty.Precipitation:
								tilePrecipitation = float.Parse((string)tileProperty.Value);
								break;
							case TileProperty.Biome:
								tileBiome = TileManager.Biome.GetBiomeByString((string)tileProperty.Value);
								break;
							case TileProperty.Roof:
								tileRoof = bool.Parse((string)tileProperty.Value);
								break;
							case TileProperty.Dug:
								tileDug = bool.Parse((string)tileProperty.Value);
								break;
							case TileProperty.Sprite:
								tileSpriteName = (string)tileProperty.Value;
								break;
							case TileProperty.Plant:
								foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)tileProperty.Value) {
									switch ((PlantProperty)Enum.Parse(typeof(PlantProperty), plantProperty.Key)) {
										case PlantProperty.Type:
											plantPrefab = GameManager.resourceM.GetPlantPrefabByString((string)plantProperty.Value);
											break;
										case PlantProperty.Sprite:
											plantSpriteName = (string)plantProperty.Value;
											break;
										case PlantProperty.Small:
											plantSmall = bool.Parse((string)plantProperty.Value);
											break;
										case PlantProperty.GrowthProgress:
											plantGrowthProgress = float.Parse((string)plantProperty.Value);
											break;
										case PlantProperty.HarvestResource:
											plantHarvestResource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)plantProperty.Value));
											break;
										case PlantProperty.Integrity:
											plantIntegrity = float.Parse((string)plantProperty.Value);
											break;
										default:
											Debug.LogError("Unknown plant property: " + plantProperty.Key + " " + plantProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown tile property: " + tileProperty.Key + " " + tileProperty.Value);
								break;
						}
					}

					persistenceTiles.Add(new PersistenceTile(
						tileIndex, tileHeight, tileType, tileTemperature, tilePrecipitation, tileBiome, tileRoof, tileDug, tileSpriteName, 
						plantPrefab, plantSpriteName, plantSmall, plantGrowthProgress, plantHarvestResource, plantIntegrity
					));
					break;
				default:
					Debug.LogError("Unknown tile property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceTiles;
	}

	public void SaveModifiedTiles(StreamWriter file, List<PersistenceTile> originalTiles) {
		TileManager.Map map = GameManager.colonyM.colony.map;
		if (map.tiles.Count != originalTiles.Count) {
			Debug.LogError("Loaded tile count " + map.tiles.Count + " and current tile count " + originalTiles.Count + " does not match.");
		}

		for (int i = 0; i < map.tiles.Count; i++) {
			TileManager.Tile tile = map.tiles[i];
			PersistenceTile originalTile = originalTiles[i];

			Dictionary<TileProperty, string> tileDifferences = new Dictionary<TileProperty, string>();
			Dictionary<PlantProperty, string> plantDifferences = new Dictionary<PlantProperty, string>();

			if (!Mathf.Approximately(tile.height, originalTile.tileHeight.Value)) {
				tileDifferences.Add(TileProperty.Height, tile.height.ToString());
			}

			if (tile.tileType.type != originalTile.tileType.type) {
				tileDifferences.Add(TileProperty.TileType, tile.tileType.type.ToString());
			}

			if (!Mathf.Approximately(tile.temperature, originalTile.tileTemperature.Value)) {
				tileDifferences.Add(TileProperty.Temperature, tile.temperature.ToString());
			}

			if (!Mathf.Approximately(tile.GetPrecipitation(), originalTile.tilePrecipitation.Value)) {
				tileDifferences.Add(TileProperty.Precipitation, tile.GetPrecipitation().ToString());
			}

			if (tile.biome.type != originalTile.tileBiome.type) {
				tileDifferences.Add(TileProperty.Biome, tile.biome.type.ToString());
			}

			if (tile.HasRoof() != originalTile.tileRoof.Value) {
				tileDifferences.Add(TileProperty.Roof, tile.HasRoof().ToString());
			}

			if (tile.dugPreviously != originalTile.tileDug.Value) {
				tileDifferences.Add(TileProperty.Dug, tile.dugPreviously.ToString());
			}

			if (tile.sr.sprite.name != originalTile.tileSpriteName) {
				tileDifferences.Add(TileProperty.Sprite, tile.sr.sprite.name);
			}

			if (originalTile.plantPrefab == null) {
				if (tile.plant != null) { // No original plant, plant was added
					tileDifferences.Add(TileProperty.Plant, string.Empty);

					plantDifferences.Add(PlantProperty.Type, tile.plant.prefab.type.ToString());
					plantDifferences.Add(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name);
					plantDifferences.Add(PlantProperty.Small, tile.plant.small.ToString());
					plantDifferences.Add(PlantProperty.GrowthProgress, tile.plant.growthProgress.ToString());
					if (tile.plant.harvestResource != null) {
						plantDifferences.Add(PlantProperty.HarvestResource, tile.plant.harvestResource.type.ToString());
					}
					plantDifferences.Add(PlantProperty.Integrity, tile.plant.integrity.ToString());
				}
			} else {
				if (tile.plant == null) { // Original plant, plant was removed
					tileDifferences.Add(TileProperty.Plant, string.Empty);

					plantDifferences.Add(PlantProperty.Type, "None");
				} else { // Plant has remained, properties potentially changed
					if (tile.plant.prefab.type != originalTile.plantPrefab.type) {
						plantDifferences.Add(PlantProperty.Type, tile.plant.prefab.type.ToString());
					}

					if (tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name != originalTile.plantSpriteName) {
						plantDifferences.Add(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name);
					}

					if (tile.plant.small != originalTile.plantSmall.Value) {
						plantDifferences.Add(PlantProperty.Small, tile.plant.small.ToString());
					}

					if (!Mathf.Approximately(tile.plant.growthProgress, originalTile.plantGrowthProgress.Value)) {
						plantDifferences.Add(PlantProperty.GrowthProgress, tile.plant.growthProgress.ToString());
					}

					if (tile.plant.harvestResource != originalTile.plantHarvestResource) {
						if (tile.plant.harvestResource != null) {
							plantDifferences.Add(PlantProperty.HarvestResource, tile.plant.harvestResource.type.ToString());
						} else {
							plantDifferences.Add(PlantProperty.HarvestResource, "None");
						}
					}

					if (tile.plant.integrity != originalTile.plantIntegrity.Value) {
						plantDifferences.Add(PlantProperty.Integrity, tile.plant.integrity.ToString());
					}

					if (plantDifferences.Count > 0) {
						tileDifferences.Add(TileProperty.Plant, string.Empty);
					}
				}
			}

			if (tileDifferences.Count > 0) {
				file.WriteLine(CreateKeyValueString(TileProperty.Tile, string.Empty, 0));
				file.WriteLine(CreateKeyValueString(TileProperty.Index, i, 1));
				foreach (KeyValuePair<TileProperty, string> tileProperty in tileDifferences) {
					file.WriteLine(CreateKeyValueString(tileProperty.Key, tileProperty.Value, 1));
					if (tileProperty.Key == TileProperty.Plant) {
						foreach (KeyValuePair<PlantProperty, string> plantProperty in plantDifferences) {
							file.WriteLine(CreateKeyValueString(plantProperty.Key, plantProperty.Value, 2));
						}
					}
				}
			}
		}
	}

	public void ApplyLoadedTiles(List<PersistenceTile> originalTiles, List<PersistenceTile> modifiedTiles, TileManager.Map map) {
		if (originalTiles.Count != Mathf.Pow(map.mapData.mapSize, 2)) {
			Debug.LogError("Map size " + Mathf.Pow(map.mapData.mapSize, 2) + " and number of persistence tiles " + originalTiles.Count + " does not match.");
		}

		for (int y = 0; y < map.mapData.mapSize; y++) {
			List<TileManager.Tile> innerTiles = new List<TileManager.Tile>();
			for (int x = 0; x < map.mapData.mapSize; x++) {
				int tileIndex = y * map.mapData.mapSize + x;
				PersistenceTile originalTile = originalTiles[tileIndex];
				PersistenceTile modifiedTile = modifiedTiles.Find(mt => mt.tileIndex == tileIndex);

				TileManager.Tile tile = new TileManager.Tile(map, new Vector2(x, y), modifiedTile != null && modifiedTile.tileHeight.HasValue ? modifiedTile.tileHeight.Value : originalTile.tileHeight.Value);
				map.tiles.Add(tile);
				innerTiles.Add(tile);

				tile.temperature = modifiedTile != null && modifiedTile.tileTemperature.HasValue ? modifiedTile.tileTemperature.Value : originalTile.tileTemperature.Value;
				tile.SetPrecipitation(modifiedTile != null && modifiedTile.tilePrecipitation.HasValue ? modifiedTile.tilePrecipitation.Value : originalTile.tilePrecipitation.Value);
				tile.SetBiome(modifiedTile != null && modifiedTile.tileBiome != null ? modifiedTile.tileBiome : originalTile.tileBiome, false);
				tile.SetTileType(modifiedTile != null && modifiedTile.tileType != null ? modifiedTile.tileType : originalTile.tileType, false, false, false);
				tile.SetRoof(modifiedTile != null && modifiedTile.tileRoof.HasValue ? modifiedTile.tileRoof.Value : originalTile.tileRoof.Value);
				tile.dugPreviously = modifiedTile != null && modifiedTile.tileDug.HasValue ? modifiedTile.tileDug.Value : originalTile.tileDug.Value;

				bool originalTileValidPlant = originalTile.plantPrefab != null;

				bool modifiedTilePlantGroupExists = modifiedTile != null && modifiedTile.plantPrefab != null;
				bool modifiedTileValidPlant = modifiedTilePlantGroupExists && modifiedTile.plantPrefab.type != ResourceManager.PlantEnum.None;
				bool plantRemoved = modifiedTilePlantGroupExists && modifiedTile.plantPrefab.type == ResourceManager.PlantEnum.None;

				if (modifiedTileValidPlant || (originalTileValidPlant && !plantRemoved)) {
					tile.SetPlant(
						false,
						new ResourceManager.Plant(
							modifiedTile != null && modifiedTile.plantPrefab != null && modifiedTile.plantPrefab.type != ResourceManager.PlantEnum.None ? modifiedTile.plantPrefab : originalTile.plantPrefab,
							tile,
							modifiedTile != null && modifiedTile.plantSmall.HasValue ? modifiedTile.plantSmall.Value : originalTile.plantSmall.Value,
							false,
							modifiedTile != null && modifiedTile.plantHarvestResource != null && modifiedTile.plantHarvestResource.type != ResourceManager.ResourceEnum.None ? modifiedTile.plantHarvestResource : originalTile.plantHarvestResource
						) {
							integrity = modifiedTile != null && modifiedTile.plantIntegrity.HasValue ? modifiedTile.plantIntegrity.Value : originalTile.plantIntegrity.Value,
							growthProgress = modifiedTile != null && modifiedTile.plantGrowthProgress.HasValue ? modifiedTile.plantGrowthProgress.Value : originalTile.plantGrowthProgress.Value
						}
					);
				}
			}
			map.sortedTiles.Add(innerTiles);
		}

		map.SetSurroundingTiles();
		map.SetMapEdgeTiles();
		map.SetSortedMapEdgeTiles();
		map.SetTileRegions(false, true);

		map.DetermineDrainageBasins();

		map.CreateRegionBlocks();

		map.RecalculateLighting(map.tiles, true, true);

		loadingState = LoadingState.LoadedMap;
	}

	private void ApplyMapBitmasking(List<PersistenceTile> originalTiles, List<PersistenceTile> modifiedTiles, TileManager.Map map) {
		map.Bitmasking(map.tiles, true, false);

		for (int i = 0; i < map.tiles.Count; i++) {
			TileManager.Tile tile = map.tiles[i];
			PersistenceTile originalTile = originalTiles[i];
			PersistenceTile modifiedTile = modifiedTiles.Find(mf => mf.tileIndex == i);

			Sprite tileSprite = tile.tileType.baseSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
			if (tileSprite == null) {
				tileSprite = tile.biome.tileTypes[tile.tileType.groupType].baseSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
				if (tileSprite == null) {
					tileSprite = tile.tileType.bitmaskSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
					if (tileSprite == null) {
						tileSprite = tile.tileType.riverSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
					}
				}
			}
			tile.sr.sprite = tileSprite;

			if (tile.plant != null) {
				Sprite plantSprite = null;
				if (tile.plant.small) {
					plantSprite = tile.plant.prefab.smallSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.plantSpriteName != null ? modifiedTile.plantSpriteName : originalTile.plantSpriteName));
				} else {
					plantSprite = tile.plant.prefab.fullSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.plantSpriteName != null ? modifiedTile.plantSpriteName : originalTile.plantSpriteName));
				}
				if (plantSprite == null) {
					plantSprite = tile.plant.prefab.harvestResourceSprites[tile.plant.harvestResource][tile.plant.small].Find(sprite => sprite.name == (modifiedTile != null && modifiedTile.plantSpriteName != null ? modifiedTile.plantSpriteName : originalTile.plantSpriteName));
				}
				tile.plant.obj.GetComponent<SpriteRenderer>().sprite = plantSprite;
			}
		}
	}

	public enum RiverProperty {
		River, Index, Type, SmallRiver, LargeRiver, StartTilePosition, CentreTilePosition, EndTilePosition, ExpandRadius, IgnoreStone, TilePositions, AddedTilePositions, RemovedTilePositions
	}

	public void SaveOriginalRivers(StreamWriter file) {
		foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.rivers) {
			WriteOriginalRiverLines(file, river, 0, RiverProperty.SmallRiver);
		}
		foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.largeRivers) {
			WriteOriginalRiverLines(file, river, 0, RiverProperty.LargeRiver);
		}
	}

	private void WriteOriginalRiverLines(StreamWriter file, TileManager.Map.River river, int startLevel, RiverProperty riverType) {
		file.WriteLine(CreateKeyValueString(RiverProperty.River, string.Empty, startLevel));

		file.WriteLine(CreateKeyValueString(RiverProperty.Type, riverType, startLevel + 1));
		file.WriteLine(CreateKeyValueString(RiverProperty.StartTilePosition, FormatVector2ToString(river.startTile.position), startLevel + 1));
		if (river.centreTile != null) {
			file.WriteLine(CreateKeyValueString(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.position), startLevel + 1));
		}
		file.WriteLine(CreateKeyValueString(RiverProperty.EndTilePosition, FormatVector2ToString(river.endTile.position), startLevel + 1));

		file.WriteLine(CreateKeyValueString(RiverProperty.ExpandRadius, river.expandRadius, startLevel + 1));
		file.WriteLine(CreateKeyValueString(RiverProperty.IgnoreStone, river.ignoreStone, startLevel + 1));
		file.WriteLine(CreateKeyValueString(RiverProperty.TilePositions, string.Join(";", river.tiles.Select(t => FormatVector2ToString(t.position)).ToArray()), startLevel + 1));
	}

	public class PersistenceRiver {
		public int? riverIndex;

		public RiverProperty? riverType;

		public Vector2? startTilePosition;
		public Vector2? centreTilePosition;
		public Vector2? endTilePosition;
		public int? expandRadius;
		public bool? ignoreStone;
		public List<Vector2> tilePositions;

		public List<Vector2> removedTilePositions;
		public List<Vector2> addedTilePositions;

		public PersistenceRiver(int? riverIndex, RiverProperty? riverType, Vector2? startTilePosition, Vector2? centreTilePosition, Vector2? endTilePosition, int? expandRadius, bool? ignoreStone, List<Vector2> tilePositions, List<Vector2> removedTilePositions, List<Vector2> addedTilePositions) {
			this.riverIndex = riverIndex;

			this.riverType = riverType;

			this.startTilePosition = startTilePosition;
			this.centreTilePosition = centreTilePosition;
			this.endTilePosition = endTilePosition;
			this.expandRadius = expandRadius;
			this.ignoreStone = ignoreStone;
			this.tilePositions = tilePositions;

			this.removedTilePositions = removedTilePositions;
			this.addedTilePositions = addedTilePositions;
		}
	}

	public List<PersistenceRiver> LoadRivers(string path) {
		List<PersistenceRiver> rivers = new List<PersistenceRiver>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((RiverProperty)Enum.Parse(typeof(RiverProperty), property.Key)) {
				case RiverProperty.River:
					int? riverIndex = null;
					RiverProperty? riverType = null;
					Vector2? startTilePosition = null;
					Vector2? centreTilePosition = null;
					Vector2? endTilePosition = null;
					int? expandRadius = null;
					bool? ignoreStone = null;
					List<Vector2> tilePositions = new List<Vector2>();
					List<Vector2> removedTilePositions = new List<Vector2>();
					List<Vector2> addedTilePositions = new List<Vector2>();

					foreach (KeyValuePair<string, object> riverProperty in (List<KeyValuePair<string, object>>)property.Value) {
						switch ((RiverProperty)Enum.Parse(typeof(RiverProperty), riverProperty.Key)) {
							case RiverProperty.Index:
								riverIndex = int.Parse((string)riverProperty.Value);
								break;
							case RiverProperty.Type:
								riverType = (RiverProperty)Enum.Parse(typeof(RiverProperty), (string)riverProperty.Value);
								break;
							case RiverProperty.StartTilePosition:
								startTilePosition = new Vector2(float.Parse(((string)riverProperty.Value).Split(',')[0]), float.Parse(((string)riverProperty.Value).Split(',')[1]));
								break;
							case RiverProperty.CentreTilePosition:
								if (!((string)riverProperty.Value).Contains("None")) {
									centreTilePosition = new Vector2(float.Parse(((string)riverProperty.Value).Split(',')[0]), float.Parse(((string)riverProperty.Value).Split(',')[1]));
								}
								break;
							case RiverProperty.EndTilePosition:
								endTilePosition = new Vector2(float.Parse(((string)riverProperty.Value).Split(',')[0]), float.Parse(((string)riverProperty.Value).Split(',')[1]));
								break;
							case RiverProperty.ExpandRadius:
								expandRadius = int.Parse((string)riverProperty.Value);
								break;
							case RiverProperty.IgnoreStone:
								ignoreStone = bool.Parse((string)riverProperty.Value);
								break;
							case RiverProperty.TilePositions:
								foreach (string vector2String in ((string)riverProperty.Value).Split(';')) {
									tilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
								}
								break;
							case RiverProperty.RemovedTilePositions:
								foreach (string vector2String in ((string)riverProperty.Value).Split(';')) {
									removedTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
								}
								break;
							case RiverProperty.AddedTilePositions:
								foreach (string vector2String in ((string)riverProperty.Value).Split(';')) {
									addedTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
								}
								break;
							default:
								Debug.LogError("Unknown river property: " + riverProperty.Key + " " + riverProperty.Value);
								break;
						}
					}

					rivers.Add(new PersistenceRiver(riverIndex, riverType, startTilePosition, centreTilePosition, endTilePosition, expandRadius, ignoreStone, tilePositions, removedTilePositions, addedTilePositions));
					break;
				default:
					Debug.LogError("Unknown river property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return rivers;
	}

	public void SaveModifiedRivers(StreamWriter file, List<PersistenceRiver> originalRivers) {
		TileManager.Map map = GameManager.colonyM.colony.map;
		int numRivers = map.rivers.Count + map.largeRivers.Count;
		if (originalRivers.Count != numRivers) {
			Debug.LogError("Loaded river count " + originalRivers.Count + " and current river count " + numRivers + " does not match.");
		}

		List<PersistenceRiver> originalSmallRivers = originalRivers.Where(river => river.riverType == RiverProperty.SmallRiver).ToList();
		if (originalSmallRivers.Count != map.rivers.Count) {
			Debug.LogError("Loaded small river count " + originalSmallRivers.Count + " and current small river count " + map.rivers.Count + " does not match.");
		}
		for (int i = 0; i < originalSmallRivers.Count; i++) {
			WriteModifiedRiverLines(file, map.rivers[i], originalSmallRivers[i], i);
		}

		List<PersistenceRiver> originalLargeRivers = originalRivers.Where(river => river.riverType == RiverProperty.LargeRiver).ToList();
		if (originalLargeRivers.Count != map.largeRivers.Count) {
			Debug.LogError("Loaded large river count " + originalLargeRivers.Count + " and current large river count " + map.largeRivers.Count + " does not match.");
		}
		for (int i = 0; i < originalLargeRivers.Count; i++) {
			WriteModifiedRiverLines(file, map.rivers[i], originalSmallRivers[i], i);
		}
	}

	public void WriteModifiedRiverLines(StreamWriter file, TileManager.Map.River river, PersistenceRiver originalRiver, int index) {
		Dictionary<RiverProperty, string> riverDifferences = new Dictionary<RiverProperty, string>();

		if (river.startTile.position != originalRiver.startTilePosition.Value) {
			riverDifferences.Add(RiverProperty.StartTilePosition, FormatVector2ToString(river.startTile.position));
		}

		if (river.centreTile == null) {
			if (originalRiver.centreTilePosition.HasValue) { // No original centre tile, centre tile was added
				riverDifferences.Add(RiverProperty.CentreTilePosition, "None");
			}
		} else {
			if (!originalRiver.centreTilePosition.HasValue) { // Original centre tile, centre tile was removed
				riverDifferences.Add(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.position));
			} else { // Centre tile has remained, properties potentially changed
				if (river.centreTile.position != originalRiver.centreTilePosition.Value) {
					riverDifferences.Add(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.position));
				}
			}
		}

		if (river.endTile.position != originalRiver.endTilePosition.Value) {
			riverDifferences.Add(RiverProperty.EndTilePosition, FormatVector2ToString(river.endTile.position));
		}

		if (river.expandRadius != originalRiver.expandRadius.Value) {
			riverDifferences.Add(RiverProperty.ExpandRadius, river.expandRadius.ToString());
		}

		if (river.ignoreStone != originalRiver.ignoreStone.Value) {
			riverDifferences.Add(RiverProperty.IgnoreStone, river.ignoreStone.ToString());
		}

		List<Vector2> riverTilePositions = river.tiles.Select(riverTile => riverTile.position).ToList();

		List<Vector2> addedRiverTiles = new List<Vector2>();
		foreach (Vector2 riverTilePosition in riverTilePositions) {
			if (!originalRiver.tilePositions.Contains(riverTilePosition)) {
				addedRiverTiles.Add(riverTilePosition);
			}
		}
		if (addedRiverTiles.Count > 0) {
			riverDifferences.Add(RiverProperty.AddedTilePositions, string.Join(";", addedRiverTiles.Select(v2 => FormatVector2ToString(v2)).ToArray()));
		}

		List<Vector2> removedRiverTiles = new List<Vector2>();
		foreach (Vector2 originalRiverTilePosition in originalRiver.tilePositions) {
			if (!riverTilePositions.Contains(originalRiverTilePosition)) {
				removedRiverTiles.Add(originalRiverTilePosition);
			}
		}
		if (removedRiverTiles.Count > 0) {
			riverDifferences.Add(RiverProperty.RemovedTilePositions, string.Join(";", removedRiverTiles.Select(v2 => FormatVector2ToString(v2)).ToArray()));
		}

		if (riverDifferences.Count > 0) {
			file.WriteLine(CreateKeyValueString(RiverProperty.River, string.Empty, 0));
			file.WriteLine(CreateKeyValueString(RiverProperty.Index, index, 1));
			foreach (KeyValuePair<RiverProperty, string> riverProperty in riverDifferences) {
				file.WriteLine(CreateKeyValueString(riverProperty.Key, riverProperty.Value, 1));
			}
		}
	}

	public void ApplyLoadedRivers(List<PersistenceRiver> originalRivers, List<PersistenceRiver> modifiedRivers, TileManager.Map map) {
		List<TileManager.Map.River> riverList = null;
		for (int i = 0; i < originalRivers.Count; i++) {
			PersistenceRiver originalRiver = originalRivers[i];
			PersistenceRiver modifiedRiver = modifiedRivers.Find(mr => mr.riverIndex == i);

			switch (modifiedRiver != null && modifiedRiver.riverType.HasValue ? modifiedRiver.riverType.Value : originalRiver.riverType.Value) {
				case RiverProperty.SmallRiver:
					riverList = map.rivers;
					break;
				case RiverProperty.LargeRiver:
					riverList = map.largeRivers;
					break;
				default:
					Debug.LogError("Invalid river type.");
					break;
			}

			List<TileManager.Tile> riverTiles = new List<TileManager.Tile>();
			foreach (Vector2 riverTilePosition in originalRiver.tilePositions) {
				riverTiles.Add(map.GetTileFromPosition(riverTilePosition));
			}

			if (modifiedRiver != null) {
				foreach (Vector2 removedTilePosition in modifiedRiver.removedTilePositions) {
					riverTiles.Remove(map.GetTileFromPosition(removedTilePosition));
				}
				foreach (Vector2 addedTilePosition in modifiedRiver.addedTilePositions) {
					riverTiles.Add(map.GetTileFromPosition(addedTilePosition));
				}
			}

			riverList.Add(
				new TileManager.Map.River(
					modifiedRiver != null && modifiedRiver.startTilePosition.HasValue ? map.GetTileFromPosition(modifiedRiver.startTilePosition.Value) : (originalRiver.startTilePosition.HasValue ? map.GetTileFromPosition(originalRiver.startTilePosition.Value) : null),
					modifiedRiver != null && modifiedRiver.centreTilePosition.HasValue ? map.GetTileFromPosition(modifiedRiver.centreTilePosition.Value) : (originalRiver.centreTilePosition.HasValue ? map.GetTileFromPosition(originalRiver.centreTilePosition.Value) : null),
					modifiedRiver != null && modifiedRiver.endTilePosition.HasValue ? map.GetTileFromPosition(modifiedRiver.endTilePosition.Value) : (originalRiver.endTilePosition.HasValue ? map.GetTileFromPosition(originalRiver.endTilePosition.Value) : null),
					modifiedRiver != null && modifiedRiver.expandRadius.HasValue ? modifiedRiver.expandRadius.Value : originalRiver.expandRadius.Value,
					modifiedRiver != null && modifiedRiver.ignoreStone.HasValue ? modifiedRiver.ignoreStone.Value : originalRiver.ignoreStone.Value,
					map,
					false
				) {
					tiles = riverTiles
				}
			);
		}
	}

	public enum CameraProperty {
		Position, Zoom
	}

	public void SaveCamera(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(CameraProperty.Position, FormatVector2ToString(GameManager.cameraM.GetCameraPosition()), 0));
		file.WriteLine(CreateKeyValueString(CameraProperty.Zoom, GameManager.cameraM.GetCameraZoom(), 0));
	}

	public void LoadCamera(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			switch ((CameraProperty)Enum.Parse(typeof(CameraProperty), property.Key)) {
				case CameraProperty.Position:
					GameManager.cameraM.SetCameraPosition(new Vector2(float.Parse(((string)property.Value).Split(',')[0]), float.Parse(((string)property.Value).Split(',')[1])));
					break;
				case CameraProperty.Zoom:
					GameManager.cameraM.SetCameraZoom(float.Parse((string)property.Value));
					break;
				default:
					Debug.LogError("Unknown camera property: " + property.Key + " " + property.Value);
					break;
			}
		}

		loadingState = LoadingState.LoadedCamera;
	}

	public enum ResourceAmountProperty {
		ResourceAmount, Type, Amount
	}

	public enum ReservedResourcesProperty {
		ReservedResourceAmounts, HumanName, Resources
	}

	public enum LifeProperty {
		Life, Health, Gender, Position, PreviousPosition, PathEndPosition
	}

	private void WriteLifeLines(StreamWriter file, LifeManager.Life life, int startLevel) {
		file.WriteLine(CreateKeyValueString(LifeProperty.Life, string.Empty, startLevel));

		file.WriteLine(CreateKeyValueString(LifeProperty.Health, life.health, startLevel + 1));
		file.WriteLine(CreateKeyValueString(LifeProperty.Gender, life.gender, startLevel + 1));
		file.WriteLine(CreateKeyValueString(LifeProperty.Position, FormatVector2ToString(life.obj.transform.position), startLevel + 1));
		file.WriteLine(CreateKeyValueString(LifeProperty.PreviousPosition, FormatVector2ToString(life.previousPosition), startLevel + 1));
		if (life.path.Count > 0) {
			file.WriteLine(CreateKeyValueString(LifeProperty.PathEndPosition, FormatVector2ToString(life.path[life.path.Count - 1].obj.transform.position), startLevel + 1));
		}
	}

	public class PersistenceLife {
		public float? health;
		public LifeManager.Life.Gender? gender;
		public Vector2? position;
		public Vector2? previousPosition;
		public Vector2? pathEndPosition;

		public PersistenceLife(float? health, LifeManager.Life.Gender? gender, Vector2? position, Vector2? previousPosition, Vector2? pathEndPosition) {
			this.health = health;
			this.gender = gender;
			this.position = position;
			this.previousPosition = previousPosition;
			this.pathEndPosition = pathEndPosition;
		}
	}

	public List<PersistenceLife> LoadLife(string path) {
		List<PersistenceLife> persistenceLife = new List<PersistenceLife>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((LifeProperty)Enum.Parse(typeof(LifeProperty), property.Key)) {
				case LifeProperty.Life:
					persistenceLife.Add(LoadPersistenceLife((List<KeyValuePair<string, object>>)property.Value));
					break;
				default:
					Debug.LogError("Unknown life property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceLife;
	}

	public PersistenceLife LoadPersistenceLife(List<KeyValuePair<string, object>> properties) {
		float? health = null;
		LifeManager.Life.Gender? gender = null;
		Vector2? position = null;
		Vector2? previousPosition = null;
		Vector2? pathEndPosition = null;

		foreach (KeyValuePair<string, object> lifeProperty in properties) {
			switch ((LifeProperty)Enum.Parse(typeof(LifeProperty), lifeProperty.Key)) {
				case LifeProperty.Health:
					health = float.Parse((string)lifeProperty.Value);
					break;
				case LifeProperty.Gender:
					gender = (LifeManager.Life.Gender)Enum.Parse(typeof(LifeManager.Life.Gender), (string)lifeProperty.Value);
					break;
				case LifeProperty.Position:
					position = new Vector2(float.Parse(((string)lifeProperty.Value).Split(',')[0]), float.Parse(((string)lifeProperty.Value).Split(',')[1]));
					break;
				case LifeProperty.PreviousPosition:
					previousPosition = new Vector2(float.Parse(((string)lifeProperty.Value).Split(',')[0]), float.Parse(((string)lifeProperty.Value).Split(',')[1]));
					break;
				case LifeProperty.PathEndPosition:
					pathEndPosition = new Vector2(float.Parse(((string)lifeProperty.Value).Split(',')[0]), float.Parse(((string)lifeProperty.Value).Split(',')[1]));
					break;
				default:
					Debug.LogError("Unknown life property: " + lifeProperty.Key + " " + lifeProperty.Value);
					break;
			}
		}

		return new PersistenceLife(health, gender, position, previousPosition, pathEndPosition);
	}

	public enum HumanProperty {
		Human, Name, SkinIndex, HairIndex, Clothes, Inventory
	}

	private void WriteHumanLines(StreamWriter file, HumanManager.Human human, int startLevel) {
		file.WriteLine(CreateKeyValueString(HumanProperty.Human, string.Empty, startLevel));

		file.WriteLine(CreateKeyValueString(HumanProperty.Name, human.name, startLevel + 1));
		file.WriteLine(CreateKeyValueString(HumanProperty.SkinIndex, human.bodyIndices[HumanManager.Human.Appearance.Skin], startLevel + 1));
		file.WriteLine(CreateKeyValueString(HumanProperty.HairIndex, human.bodyIndices[HumanManager.Human.Appearance.Hair], startLevel + 1));

		if (human.clothes.Any(kvp => kvp.Value != null)) {
			file.WriteLine(CreateKeyValueString(HumanProperty.Clothes, string.Empty, startLevel + 1));
			foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothing in human.clothes) {
				if (appearanceToClothing.Value != null) {
					file.WriteLine(CreateKeyValueString(appearanceToClothing.Key, appearanceToClothing.Value.prefab.clothingType + ":" + appearanceToClothing.Value.colour, startLevel + 2));
				}
			}
		}

		WriteInventoryLines(file, human.GetInventory(), startLevel + 1);
	}

	public class PersistenceHuman {
		public string name;
		public int? skinIndex;
		public int? hairIndex;
		public Dictionary<HumanManager.Human.Appearance, ResourceManager.Clothing> clothes;
		public PersistenceInventory persistenceInventory;

		public PersistenceHuman(string name, int? skinIndex, int? hairIndex, Dictionary<HumanManager.Human.Appearance, ResourceManager.Clothing> clothes, PersistenceInventory persistenceInventory) {
			this.name = name;
			this.skinIndex = skinIndex;
			this.hairIndex = hairIndex;
			this.clothes = clothes;
			this.persistenceInventory = persistenceInventory;
		}
	}

	public List<PersistenceHuman> LoadHumans(string path) {
		List<PersistenceHuman> persistenceHumans = new List<PersistenceHuman>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((HumanProperty)Enum.Parse(typeof(HumanProperty), property.Key)) {
				case HumanProperty.Human:
					persistenceHumans.Add(LoadPersistenceHuman((List<KeyValuePair<string, object>>)property.Value));
					break;
				default:
					Debug.LogError("Unknown human property: " + property.Key + " " + property.Value);
					break;
			}
		}

		return persistenceHumans;
	}

	public PersistenceHuman LoadPersistenceHuman(List<KeyValuePair<string, object>> properties) {
		string name = null;
		int? skinIndex = null;
		int? hairIndex = null;
		Dictionary<HumanManager.Human.Appearance, ResourceManager.Clothing> clothes = new Dictionary<HumanManager.Human.Appearance, ResourceManager.Clothing>();
		PersistenceInventory persistenceInventory = null;

		foreach (KeyValuePair<string, object> humanProperty in properties) {
			switch ((HumanProperty)Enum.Parse(typeof(HumanProperty), humanProperty.Key)) {
				case HumanProperty.Name:
					name = (string)humanProperty.Value;
					break;
				case HumanProperty.SkinIndex:
					skinIndex = int.Parse((string)humanProperty.Value);
					break;
				case HumanProperty.HairIndex:
					hairIndex = int.Parse((string)humanProperty.Value);
					break;
				case HumanProperty.Clothes:
					foreach (KeyValuePair<string, object> clothingProperty in (List<KeyValuePair<string, object>>)humanProperty.Value) {
						HumanManager.Human.Appearance clothingPropertyKey = (HumanManager.Human.Appearance)Enum.Parse(typeof(HumanManager.Human.Appearance), clothingProperty.Key);
						ResourceManager.ClothingEnum clothingType = (ResourceManager.ClothingEnum)Enum.Parse(typeof(ResourceManager.ClothingEnum), ((string)clothingProperty.Value).Split(':')[0]);
						string colour = ((string)clothingProperty.Value).Split(':')[1];
						clothes.Add(clothingPropertyKey, GameManager.resourceM.GetClothesByAppearance(clothingPropertyKey).Find(c => c.prefab.clothingType == clothingType && c.colour == colour));
					}
					break;
				case HumanProperty.Inventory:
					persistenceInventory = LoadPersistenceInventory((List<KeyValuePair<string, object>>)humanProperty.Value);
					break;
				default:
					Debug.LogError("Unknown human property: " + humanProperty.Key + " " + humanProperty.Value);
					break;
			}
		}

		return new PersistenceHuman(name, skinIndex, hairIndex, clothes, persistenceInventory);
	}

	public enum CaravanProperty {
		CaravanTimer, Caravan, Type, Location, TargetTile, ResourceGroup, LeaveTimer, Leaving, Inventory, ResourcesToTrade, ConfirmedResourcesToTrade, Traders
	}

	public enum LocationProperty {
		Location, Name, Wealth, ResourceRichness, CitySize, BiomeType
	}

	public enum TraderProperty {
		Trader, Life, Human, LeaveTile, TradingPosts
	}

	public enum TradeResourceAmountProperty {
		TradeResourceAmount, Type, CaravanAmount, TradeAmount, Price
	}

	public enum ConfirmedTradeResourceAmountProperty {
		ConfirmedTradeResourceAmount, Type, TradeAmount, AmountRemaining
	}

	public void SaveCaravans(StreamWriter file) {

		file.WriteLine(CreateKeyValueString(CaravanProperty.CaravanTimer, GameManager.caravanM.caravanTimer, 0));

		foreach (CaravanManager.Caravan caravan in GameManager.caravanM.caravans) {
			file.WriteLine(CreateKeyValueString(CaravanProperty.Caravan, string.Empty, 0));

			file.WriteLine(CreateKeyValueString(CaravanProperty.Type, caravan.caravanType, 1));

			file.WriteLine(CreateKeyValueString(CaravanProperty.Location, string.Empty, 1));
			file.WriteLine(CreateKeyValueString(LocationProperty.Name, caravan.location.name, 2));
			file.WriteLine(CreateKeyValueString(LocationProperty.Wealth, caravan.location.wealth, 2));
			file.WriteLine(CreateKeyValueString(LocationProperty.ResourceRichness, caravan.location.resourceRichness, 2));
			file.WriteLine(CreateKeyValueString(LocationProperty.CitySize, caravan.location.citySize, 2));
			file.WriteLine(CreateKeyValueString(LocationProperty.BiomeType, caravan.location.biomeType, 2));

			file.WriteLine(CreateKeyValueString(CaravanProperty.TargetTile, FormatVector2ToString(caravan.targetTile.obj.transform.position), 1));

			file.WriteLine(CreateKeyValueString(CaravanProperty.ResourceGroup, caravan.resourceGroup.type, 1));

			file.WriteLine(CreateKeyValueString(CaravanProperty.LeaveTimer, caravan.leaveTimer, 1));
			file.WriteLine(CreateKeyValueString(CaravanProperty.Leaving, caravan.leaving, 1));

			WriteInventoryLines(file, caravan.GetInventory(), 1);

			List<ResourceManager.TradeResourceAmount> tradeResourceAmounts = caravan.GenerateTradeResourceAmounts();
			if (tradeResourceAmounts.Count > 0) {
				file.WriteLine(CreateKeyValueString(CaravanProperty.ResourcesToTrade, string.Empty, 1));
				foreach (ResourceManager.TradeResourceAmount tra in caravan.GenerateTradeResourceAmounts()) {
					file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.TradeResourceAmount, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.Type, tra.resource.type, 3));

					file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.CaravanAmount, tra.caravanAmount, 3));

					file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.TradeAmount, tra.GetTradeAmount(), 3));

					file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.Price, tra.caravanResourcePrice, 3));
				}
			}

			if (caravan.confirmedResourcesToTrade.Count > 0) {
				file.WriteLine(CreateKeyValueString(CaravanProperty.ConfirmedResourcesToTrade, string.Empty, 1));
				foreach (ResourceManager.ConfirmedTradeResourceAmount ctra in caravan.confirmedResourcesToTrade) {
					file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.ConfirmedTradeResourceAmount, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.Type, ctra.resource.type, 3));

					file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.TradeAmount, ctra.tradeAmount, 3));

					file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.AmountRemaining, ctra.amountRemaining, 3));
				}
			}

			file.WriteLine(CreateKeyValueString(CaravanProperty.Traders, string.Empty, 1));
			foreach (CaravanManager.Trader trader in caravan.traders) {
				file.WriteLine(CreateKeyValueString(TraderProperty.Trader, string.Empty, 2));

				WriteLifeLines(file, trader, 3);

				WriteHumanLines(file, trader, 3);

				if (trader.leaveTile != null) {
					file.WriteLine(CreateKeyValueString(TraderProperty.LeaveTile, FormatVector2ToString(trader.leaveTile.obj.transform.position), 3));
				}

				if (trader.tradingPosts != null && trader.tradingPosts.Count > 0) {
					file.WriteLine(CreateKeyValueString(TraderProperty.TradingPosts, string.Join(";", trader.tradingPosts.Select(tp => FormatVector2ToString(tp.zeroPointTile.obj.transform.position)).ToArray()), 3));
				}
			}
		}
	}

	public class PersistenceCaravan {
		public CaravanManager.CaravanTypeEnum? type;
		public CaravanManager.Location location;
		public Vector2? targetTilePosition;
		public ResourceManager.ResourceGroup resourceGroup;
		public int? leaveTimer;
		public bool? leaving;
		public PersistenceInventory persistenceInventory;
		public List<PersistenceTradeResourceAmount> persistenceResourcesToTrade;
		public List<PersistenceConfirmedTradeResourceAmount> persistenceConfirmedResourcesToTrade;
		public List<PersistenceTrader> persistenceTraders;

		public PersistenceCaravan(
			CaravanManager.CaravanTypeEnum? type,
			CaravanManager.Location location,
			Vector2? targetTilePosition,
			ResourceManager.ResourceGroup resourceGroup,
			int? leaveTimer,
			bool? leaving,
			PersistenceInventory persistenceInventory,
			List<PersistenceTradeResourceAmount> persistenceResourcesToTrade,
			List<PersistenceConfirmedTradeResourceAmount> persistenceConfirmedResourcesToTrade,
			List<PersistenceTrader> persistenceTraders
		) {
			this.type = type;
			this.location = location;
			this.targetTilePosition = targetTilePosition;
			this.resourceGroup = resourceGroup;
			this.leaveTimer = leaveTimer;
			this.leaving = leaving;
			this.persistenceInventory = persistenceInventory;
			this.persistenceResourcesToTrade = persistenceResourcesToTrade;
			this.persistenceConfirmedResourcesToTrade = persistenceConfirmedResourcesToTrade;
			this.persistenceTraders = persistenceTraders;
		}
	}

	public class PersistenceTradeResourceAmount {
		public ResourceManager.ResourceEnum? type;
		public int? caravanAmount;
		public int? tradeAmount;
		public int? caravanPrice;

		public PersistenceTradeResourceAmount(
			ResourceManager.ResourceEnum? type,
			int? caravanAmount,
			int? tradeAmount,
			int? caravanPrice
		) {
			this.type = type;
			this.caravanAmount = caravanAmount;
			this.tradeAmount = tradeAmount;
			this.caravanPrice = caravanPrice;
		}
	}

	public class PersistenceConfirmedTradeResourceAmount {
		public ResourceManager.ResourceEnum? type;
		public int? tradeAmount;
		public int? amountRemaining;

		public PersistenceConfirmedTradeResourceAmount(
			ResourceManager.ResourceEnum? type,
			int? tradeAmount,
			int? amountRemaining
		) {
			this.type = type;
			this.tradeAmount = tradeAmount;
			this.amountRemaining = amountRemaining;
		}
	}

	public class PersistenceTrader {
		public PersistenceLife persistenceLife;
		public PersistenceHuman persistenceHuman;
		public TileManager.Tile leaveTile;
		public List<ResourceManager.TradingPost> tradingPosts;

		public PersistenceTrader(
			PersistenceLife persistenceLife,
			PersistenceHuman persistenceHuman,
			TileManager.Tile leaveTile,
			List<ResourceManager.TradingPost> tradingPosts
		) {
			this.persistenceLife = persistenceLife;
			this.persistenceHuman = persistenceHuman;
			this.leaveTile = leaveTile;
			this.tradingPosts = tradingPosts;
		}
	}

	public List<PersistenceCaravan> LoadCaravans(string path) {
		List<PersistenceCaravan> persistenceCaravans = new List<PersistenceCaravan>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((CaravanProperty)Enum.Parse(typeof(CaravanProperty), property.Key)) {
				case CaravanProperty.CaravanTimer:
					GameManager.caravanM.caravanTimer = int.Parse((string)property.Value);
					break;
				case CaravanProperty.Caravan:

					List<KeyValuePair<string, object>> caravanProperties = (List<KeyValuePair<string, object>>)property.Value;

					CaravanManager.CaravanTypeEnum? type = null;
					CaravanManager.Location location = null;
					Vector2? targetTilePosition = null;
					ResourceManager.ResourceGroup resourceGroup = null;
					int? leaveTimer = null;
					bool? leaving = null;
					PersistenceInventory persistenceInventory = null;
					List<PersistenceTradeResourceAmount> persistenceResourcesToTrade = new List<PersistenceTradeResourceAmount>();
					List<PersistenceConfirmedTradeResourceAmount> persistenceConfirmedResourcesToTrade = new List<PersistenceConfirmedTradeResourceAmount>();
					List<PersistenceTrader> persistenceTraders = new List<PersistenceTrader>();

					foreach (KeyValuePair<string, object> caravanProperty in caravanProperties) {
						switch ((CaravanProperty)Enum.Parse(typeof(CaravanProperty), caravanProperty.Key)) {
							case CaravanProperty.Type:
								type = (CaravanManager.CaravanTypeEnum)Enum.Parse(typeof(CaravanManager.CaravanTypeEnum), (string)caravanProperty.Value);
								break;
							case CaravanProperty.Location:

								string locationName = null;
								CaravanManager.Location.Wealth? locationWealth = null;
								CaravanManager.Location.ResourceRichness? locationResourceRichness = null;
								CaravanManager.Location.CitySize? locationCitySize = null;
								TileManager.Biome.TypeEnum? locationBiomeType = null;

								foreach (KeyValuePair<string, object> locationProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
									switch ((LocationProperty)Enum.Parse(typeof(LocationProperty), locationProperty.Key)) {
										case LocationProperty.Name:
											locationName = (string)locationProperty.Value;
											break;
										case LocationProperty.Wealth:
											locationWealth = (CaravanManager.Location.Wealth)Enum.Parse(typeof(CaravanManager.Location.Wealth), (string)locationProperty.Value);
											break;
										case LocationProperty.ResourceRichness:
											locationResourceRichness = (CaravanManager.Location.ResourceRichness)Enum.Parse(typeof(CaravanManager.Location.ResourceRichness), (string)locationProperty.Value);
											break;
										case LocationProperty.CitySize:
											locationCitySize = (CaravanManager.Location.CitySize)Enum.Parse(typeof(CaravanManager.Location.CitySize), (string)locationProperty.Value);
											break;
										case LocationProperty.BiomeType:
											locationBiomeType = (TileManager.Biome.TypeEnum)Enum.Parse(typeof(TileManager.Biome.TypeEnum), (string)locationProperty.Value);
											break;
										default:
											Debug.LogError("Unknown location property: " + locationProperty.Key + " " + locationProperty.Value);
											break;
									}
								}

								location = new CaravanManager.Location(
									locationName, 
									locationWealth.Value, 
									locationResourceRichness.Value, 
									locationCitySize.Value, 
									locationBiomeType.Value
								);
								break;
							case CaravanProperty.TargetTile:
								targetTilePosition = new Vector2(float.Parse(((string)caravanProperty.Value).Split(',')[0]), float.Parse(((string)caravanProperty.Value).Split(',')[1]));
								break;
							case CaravanProperty.ResourceGroup:
								resourceGroup = GameManager.resourceM.GetResourceGroupByEnum((ResourceManager.ResourceGroupEnum)Enum.Parse(typeof(ResourceManager.ResourceGroupEnum), (string)caravanProperty.Value));
								break;
							case CaravanProperty.LeaveTimer:
								leaveTimer = int.Parse((string)caravanProperty.Value);
								break;
							case CaravanProperty.Leaving:
								leaving = bool.Parse((string)caravanProperty.Value);
								break;
							case CaravanProperty.Inventory:
								persistenceInventory = LoadPersistenceInventory((List<KeyValuePair<string, object>>)caravanProperty.Value);
								break;
							case CaravanProperty.ResourcesToTrade:
								foreach (KeyValuePair<string, object> resourceToTradeProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
									switch ((TradeResourceAmountProperty)Enum.Parse(typeof(TradeResourceAmountProperty), resourceToTradeProperty.Key)) {
										case TradeResourceAmountProperty.TradeResourceAmount:

											ResourceManager.ResourceEnum? resourceToTradeType = null;
											int? resourceToTradeCaravanAmount = null;
											int? resourceToTradeTradeAmount = null;
											int? resourceToTradeCaravanPrice = null;

											foreach (KeyValuePair<string, object> resourceToTradeSubProperty in (List<KeyValuePair<string, object>>)resourceToTradeProperty.Value) {
												switch ((TradeResourceAmountProperty)Enum.Parse(typeof(TradeResourceAmountProperty), resourceToTradeSubProperty.Key)) {
													case TradeResourceAmountProperty.Type:
														resourceToTradeType = (ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)resourceToTradeSubProperty.Value);
														break;
													case TradeResourceAmountProperty.CaravanAmount:
														resourceToTradeCaravanAmount = int.Parse((string)resourceToTradeSubProperty.Value);
														break;
													case TradeResourceAmountProperty.TradeAmount:
														resourceToTradeTradeAmount = int.Parse((string)resourceToTradeSubProperty.Value);
														break;
													case TradeResourceAmountProperty.Price:
														resourceToTradeCaravanPrice = int.Parse((string)resourceToTradeSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown trade resource amount property: " + resourceToTradeSubProperty.Key + " " + resourceToTradeSubProperty.Value);
														break;
												}
											}

											persistenceResourcesToTrade.Add(new PersistenceTradeResourceAmount(
												resourceToTradeType, 
												resourceToTradeCaravanAmount, 
												resourceToTradeTradeAmount, 
												resourceToTradeCaravanPrice
											));
											break;
										default:
											Debug.LogError("Unknown trade resource amount property: " + resourceToTradeProperty.Key + " " + resourceToTradeProperty.Value);
											break;
									}
								}
								break;
							case CaravanProperty.ConfirmedResourcesToTrade:
								foreach (KeyValuePair<string, object> confirmedResourceToTradeProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
									switch ((ConfirmedTradeResourceAmountProperty)Enum.Parse(typeof(ConfirmedTradeResourceAmountProperty), confirmedResourceToTradeProperty.Key)) {
										case ConfirmedTradeResourceAmountProperty.ConfirmedTradeResourceAmount:

											ResourceManager.ResourceEnum? confirmedResourceToTradeType = null;
											int? confirmedResourceToTradeTradeAmount = null;
											int? confirmedResourceToTradeAmountRemaining = null;

											foreach (KeyValuePair<string, object> confirmedResourceToTradeSubProperty in (List<KeyValuePair<string, object>>)confirmedResourceToTradeProperty.Value) {
												switch ((ConfirmedTradeResourceAmountProperty)Enum.Parse(typeof(ConfirmedTradeResourceAmountProperty), confirmedResourceToTradeSubProperty.Key)) {
													case ConfirmedTradeResourceAmountProperty.Type:
														confirmedResourceToTradeType = (ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)confirmedResourceToTradeSubProperty.Value);
														break;
													case ConfirmedTradeResourceAmountProperty.TradeAmount:
														confirmedResourceToTradeTradeAmount = int.Parse((string)confirmedResourceToTradeSubProperty.Value);
														break;
													case ConfirmedTradeResourceAmountProperty.AmountRemaining:
														confirmedResourceToTradeAmountRemaining = int.Parse((string)confirmedResourceToTradeSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown confirmed trade resource amount property: " + confirmedResourceToTradeSubProperty.Key + " " + confirmedResourceToTradeSubProperty.Value);
														break;
												}
											}

											persistenceConfirmedResourcesToTrade.Add(new PersistenceConfirmedTradeResourceAmount(
												confirmedResourceToTradeType,
												confirmedResourceToTradeTradeAmount,
												confirmedResourceToTradeAmountRemaining
											));
											break;
										default:
											Debug.LogError("Unknown confirmed trade resource amount property: " + confirmedResourceToTradeProperty.Key + " " + confirmedResourceToTradeProperty.Value);
											break;
									}
								}
								break;
							case CaravanProperty.Traders:
								foreach (KeyValuePair<string, object> traderProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
									switch ((TraderProperty)Enum.Parse(typeof(TraderProperty), traderProperty.Key)) {
										case TraderProperty.Trader:

											PersistenceLife persistenceLife = null;
											PersistenceHuman persistenceHuman = null;
											TileManager.Tile traderLeaveTile = null;
											List<ResourceManager.TradingPost> traderTradingPosts = new List<ResourceManager.TradingPost>();

											foreach (KeyValuePair<string, object> traderSubProperty in (List<KeyValuePair<string, object>>)traderProperty.Value) {
												switch ((TraderProperty)Enum.Parse(typeof(TraderProperty), traderSubProperty.Key)) {
													case TraderProperty.Life:
														persistenceLife = LoadPersistenceLife((List<KeyValuePair<string, object>>)traderSubProperty.Value);
														break;
													case TraderProperty.Human:
														persistenceHuman = LoadPersistenceHuman((List<KeyValuePair<string, object>>)traderSubProperty.Value);
														break;
													case TraderProperty.LeaveTile:
														traderLeaveTile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(float.Parse(((string)traderSubProperty.Value).Split(',')[0]), float.Parse(((string)traderSubProperty.Value).Split(',')[1])));
														break;
													case TraderProperty.TradingPosts:
														foreach (string vector2String in ((string)traderSubProperty.Value).Split(';')) {
															TileManager.Tile tradingPostZeroPointTile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
															traderTradingPosts.Add(GameManager.resourceM.tradingPosts.Find(tp => tp.zeroPointTile == tradingPostZeroPointTile));
														}
														break;
													default:
														Debug.LogError("Unknown trader property: " + traderSubProperty.Key + " " + traderSubProperty.Value);
														break;
												}
											}

											persistenceTraders.Add(new PersistenceTrader(
												persistenceLife, 
												persistenceHuman,
												traderLeaveTile,
												traderTradingPosts
											));
											break;
										default:
											Debug.LogError("Unknown trader property: " + traderProperty.Key + " " + traderProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown caravan property: " + caravanProperty.Key + " " + caravanProperty.Value);
								break;
						}
					}

					persistenceCaravans.Add(new PersistenceCaravan(
						type,
						location,
						targetTilePosition,
						resourceGroup,
						leaveTimer,
						leaving,
						persistenceInventory,
						persistenceResourcesToTrade,
						persistenceConfirmedResourcesToTrade,
						persistenceTraders
					));
					break;
				default:
					Debug.LogError("Unknown caravan property: " + property.Key + " " + property.Value);
					break;
			}
		}

		loadingState = LoadingState.LoadedCaravans;
		return persistenceCaravans;
	}

	public void ApplyLoadedCaravans(List<PersistenceCaravan> persistenceCaravans) {
		foreach (PersistenceCaravan persistenceCaravan in persistenceCaravans) {
			CaravanManager.Caravan caravan = new CaravanManager.Caravan() {
				numTraders = persistenceCaravan.persistenceTraders.Count,
				caravanType = persistenceCaravan.type.Value,
				location = persistenceCaravan.location,
				targetTile = GameManager.colonyM.colony.map.GetTileFromPosition(persistenceCaravan.targetTilePosition.Value),
				resourceGroup = persistenceCaravan.resourceGroup,
				leaveTimer = persistenceCaravan.leaveTimer.Value,
				leaving = persistenceCaravan.leaving.Value
			};

			caravan.GetInventory().maxWeight = persistenceCaravan.persistenceInventory.maxWeight.Value;
			caravan.GetInventory().maxVolume = persistenceCaravan.persistenceInventory.maxVolume.Value;
			foreach (ResourceManager.ResourceAmount resourceAmount in persistenceCaravan.persistenceInventory.resources) {
				caravan.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
			}

			foreach (PersistenceTradeResourceAmount persistenceTradeResourceAmount in persistenceCaravan.persistenceResourcesToTrade) {
				ResourceManager.TradeResourceAmount tradeResourceAmount = new ResourceManager.TradeResourceAmount(
					GameManager.resourceM.GetResourceByEnum(persistenceTradeResourceAmount.type.Value),
					persistenceTradeResourceAmount.caravanAmount.Value,
					caravan
				) {
					caravanResourcePrice = persistenceTradeResourceAmount.caravanPrice.Value
				};
				tradeResourceAmount.SetTradeAmount(persistenceTradeResourceAmount.tradeAmount.Value); // Added to caravan through tra.SetTradeAmount() -> caravan.SetSelectedResource()
			}

			foreach (PersistenceConfirmedTradeResourceAmount persistenceConfirmedTradeResourceAmount in persistenceCaravan.persistenceConfirmedResourcesToTrade) {
				caravan.confirmedResourcesToTrade.Add(
					new ResourceManager.ConfirmedTradeResourceAmount(
						GameManager.resourceM.GetResourceByEnum(persistenceConfirmedTradeResourceAmount.type.Value),
						persistenceConfirmedTradeResourceAmount.tradeAmount.Value
					) {
						amountRemaining = persistenceConfirmedTradeResourceAmount.amountRemaining.Value
					}
				);
			}

			foreach (PersistenceTrader persistenceTrader in persistenceCaravan.persistenceTraders) {
				CaravanManager.Trader trader = new CaravanManager.Trader(
					GameManager.colonyM.colony.map.GetTileFromPosition(persistenceTrader.persistenceLife.position.Value),
					persistenceTrader.persistenceLife.health.Value,
					caravan
				) {
					gender = persistenceTrader.persistenceLife.gender.Value,
					previousPosition = persistenceTrader.persistenceLife.previousPosition.Value
				};

				trader.obj.transform.position = persistenceTrader.persistenceLife.position.Value;

				trader.SetName(persistenceTrader.persistenceHuman.name);

				trader.bodyIndices[HumanManager.Human.Appearance.Skin] = persistenceTrader.persistenceHuman.skinIndex.Value;
				trader.moveSprites = GameManager.humanM.humanMoveSprites[trader.bodyIndices[HumanManager.Human.Appearance.Skin]];
				trader.bodyIndices[HumanManager.Human.Appearance.Hair] = persistenceTrader.persistenceHuman.hairIndex.Value;

				trader.GetInventory().maxWeight = persistenceTrader.persistenceHuman.persistenceInventory.maxWeight.Value;
				trader.GetInventory().maxVolume = persistenceTrader.persistenceHuman.persistenceInventory.maxVolume.Value;
				foreach (ResourceManager.ResourceAmount resourceAmount in persistenceTrader.persistenceHuman.persistenceInventory.resources) {
					trader.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
				}

				foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothingKVP in persistenceTrader.persistenceHuman.clothes) {
					trader.GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(appearanceToClothingKVP.Value.type), 1, false);
					trader.ChangeClothing(appearanceToClothingKVP.Key, appearanceToClothingKVP.Value);
				}

				if (persistenceTrader.persistenceLife.pathEndPosition.HasValue) {
					trader.MoveToTile(GameManager.colonyM.colony.map.GetTileFromPosition(persistenceTrader.persistenceLife.pathEndPosition.Value), true);
				}

				trader.leaveTile = persistenceTrader.leaveTile;

				trader.tradingPosts = persistenceTrader.tradingPosts;

				caravan.traders.Add(trader);
			}

			GameManager.caravanM.AddCaravan(caravan);
		}
	}

	public enum ColonistProperty {
		Colonist, Life, Human, PlayerMoved, Job, StoredJob, BacklogJobs, Professions, Skills, Traits, Needs, BaseMood, EffectiveMood, MoodModifiers
	}

	public enum BacklogJobProperty {
		BacklogJob
	}

	public enum ProfessionProperty {
		Profession, Name, Priority
	}

	public enum SkillProperty {
		Skill, Type, Level, NextLevelExperience, CurrentExperience
	}

	public enum TraitProperty {
		Trait, Type
	}

	public enum NeedProperty {
		Need, Type, Value
	}

	public enum MoodModifierProperty {
		MoodModifier, Type, TimeRemaining
	}

	public void SaveColonists(StreamWriter file) {
		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			file.WriteLine(CreateKeyValueString(ColonistProperty.Colonist, string.Empty, 0));

			WriteLifeLines(file, colonist, 1);

			WriteHumanLines(file, colonist, 1);

			file.WriteLine(CreateKeyValueString(ColonistProperty.PlayerMoved, colonist.playerMoved, 1));

			if (colonist.job != null) {
				WriteJobLines(file, colonist.job, JobProperty.Job, 1);
			}
			if (colonist.storedJob != null) {
				WriteJobLines(file, colonist.storedJob, JobProperty.StoredJob, 1);
			}
			if (colonist.backlog.Count > 0) {
				file.WriteLine(CreateKeyValueString(ColonistProperty.BacklogJobs, string.Empty, 1));
				foreach (Job backlogJob in colonist.backlog) {
					WriteJobLines(file, backlogJob, JobProperty.BacklogJob, 2);
				}
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.Professions, string.Empty, 1));
			foreach (ColonistManager.ProfessionInstance profession in colonist.professions) {
				file.WriteLine(CreateKeyValueString(ProfessionProperty.Profession, string.Empty, 2));

				file.WriteLine(CreateKeyValueString(ProfessionProperty.Name, profession.prefab.type, 3));
				file.WriteLine(CreateKeyValueString(ProfessionProperty.Priority, profession.GetPriority(), 3));
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.Skills, string.Empty, 1));
			foreach (ColonistManager.SkillInstance skill in colonist.skills) {
				file.WriteLine(CreateKeyValueString(SkillProperty.Skill, string.Empty, 2));

				file.WriteLine(CreateKeyValueString(SkillProperty.Type, skill.prefab.type, 3));
				file.WriteLine(CreateKeyValueString(SkillProperty.Level, skill.level, 3));
				file.WriteLine(CreateKeyValueString(SkillProperty.NextLevelExperience, skill.nextLevelExperience, 3));
				file.WriteLine(CreateKeyValueString(SkillProperty.CurrentExperience, skill.currentExperience, 3));
			}

			if (colonist.traits.Count > 0) {
				file.WriteLine(CreateKeyValueString(ColonistProperty.Traits, string.Empty, 1));
				foreach (ColonistManager.TraitInstance trait in colonist.traits) {
					file.WriteLine(CreateKeyValueString(TraitProperty.Trait, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(TraitProperty.Type, trait.prefab.type, 3));
				}
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.Needs, string.Empty, 1));
			foreach (ColonistManager.NeedInstance need in colonist.needs) {
				file.WriteLine(CreateKeyValueString(NeedProperty.Need, string.Empty, 2));

				file.WriteLine(CreateKeyValueString(NeedProperty.Type, need.prefab.type, 3));
				file.WriteLine(CreateKeyValueString(NeedProperty.Value, need.GetValue(), 3));
			}

			file.WriteLine(CreateKeyValueString(ColonistProperty.BaseMood, colonist.baseMood, 1));
			file.WriteLine(CreateKeyValueString(ColonistProperty.EffectiveMood, colonist.effectiveMood, 1));

			if (colonist.moodModifiers.Count > 0) {
				file.WriteLine(CreateKeyValueString(ColonistProperty.MoodModifiers, string.Empty, 1));
				foreach (ColonistManager.MoodModifierInstance moodModifier in colonist.moodModifiers) {
					file.WriteLine(CreateKeyValueString(MoodModifierProperty.MoodModifier, string.Empty, 2));

					file.WriteLine(CreateKeyValueString(MoodModifierProperty.Type, moodModifier.prefab.type, 3));
					file.WriteLine(CreateKeyValueString(MoodModifierProperty.TimeRemaining, moodModifier.timer, 3));
				}
			}
		}
	}

	public class PersistenceColonist {

		public PersistenceLife persistenceLife;
		public PersistenceHuman persistenceHuman;

		public bool? playerMoved;
		public PersistenceJob persistenceJob;
		public PersistenceJob persistenceStoredJob;
		public List<PersistenceJob> persistenceBacklogJobs;
		public List<PersistenceProfession> persistenceProfessions;
		public List<PersistenceSkill> persistenceSkills;
		public List<PersistenceTrait> persistenceTraits;
		public List<PersistenceNeed> persistenceNeeds;
		public float? baseMood;
		public float? effectiveMood;
		public List<PersistenceMoodModifier> persistenceMoodModifiers;

		public PersistenceColonist(
			PersistenceLife persistenceLife,
			PersistenceHuman persistenceHuman,
			bool? playerMoved,
			PersistenceJob persistenceJob,
			PersistenceJob persistenceStoredJob,
			List<PersistenceJob> persistenceBacklogJobs,
			List<PersistenceProfession> persistenceProfessions,
			List<PersistenceSkill> persistenceSkills,
			List<PersistenceTrait> persistenceTraits,
			List<PersistenceNeed> persistenceNeeds,
			float? baseMood,
			float? effectiveMood,
			List<PersistenceMoodModifier> persistenceMoodModifiers
		) {
			this.persistenceLife = persistenceLife;
			this.persistenceHuman = persistenceHuman;
			this.playerMoved = playerMoved;
			this.persistenceJob = persistenceJob;
			this.persistenceStoredJob = persistenceStoredJob;
			this.persistenceBacklogJobs = persistenceBacklogJobs;
			this.persistenceProfessions = persistenceProfessions;
			this.persistenceSkills = persistenceSkills;
			this.persistenceTraits = persistenceTraits;
			this.persistenceNeeds = persistenceNeeds;
			this.baseMood = baseMood;
			this.effectiveMood = effectiveMood;
			this.persistenceMoodModifiers = persistenceMoodModifiers;
		}
	}

	public class PersistenceProfession {
		public ColonistManager.ProfessionEnum? type;
		public int? priority;

		public PersistenceProfession(
			ColonistManager.ProfessionEnum? type,
			int? priority
		) {
			this.type = type;
			this.priority = priority;
		}
	}

	public class PersistenceSkill {
		public ColonistManager.SkillEnum? type;
		public int? level;
		public float? nextLevelExperience;
		public float? currentExperience;

		public PersistenceSkill(
			ColonistManager.SkillEnum? type,
			int? level,
			float? nextLevelExperience,
			float? currentExperience
		) {
			this.type = type;
			this.level = level;
			this.nextLevelExperience = nextLevelExperience;
			this.currentExperience = currentExperience;
		}
	}

	public class PersistenceTrait {
		public ColonistManager.TraitEnum? type;

		public PersistenceTrait(ColonistManager.TraitEnum? type) {
			this.type = type;
		}
	}

	public class PersistenceNeed {
		public ColonistManager.NeedEnum? type;
		public float? value;

		public PersistenceNeed(
			ColonistManager.NeedEnum? type,
			float? value
		) {
			this.type = type;
			this.value = value;
		}
	}

	public class PersistenceMoodModifier {
		public ColonistManager.MoodModifierEnum? type;
		public float? timeRemaining;

		public PersistenceMoodModifier(
			ColonistManager.MoodModifierEnum? type,
			float? timeRemaining
		) {
			this.type = type;
			this.timeRemaining = timeRemaining;
		}
	}

	public List<PersistenceColonist> LoadColonists(string path) {
		List<PersistenceColonist> persistenceColonists = new List<PersistenceColonist>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((ColonistProperty)Enum.Parse(typeof(ColonistProperty), property.Key)) {
				case ColonistProperty.Colonist:

					List<KeyValuePair<string, object>> colonistProperties = (List<KeyValuePair<string, object>>)property.Value;

					PersistenceLife persistenceLife = null;
					PersistenceHuman persistenceHuman = null;

					bool? playerMoved = null;
					PersistenceJob persistenceJob = null;
					PersistenceJob persistenceStoredJob = null;
					List<PersistenceJob> persistenceBacklogJobs = new List<PersistenceJob>();
					List<PersistenceProfession> persistenceProfessions = new List<PersistenceProfession>();
					List<PersistenceSkill> persistenceSkills = new List<PersistenceSkill>();
					List<PersistenceTrait> persistenceTraits = new List<PersistenceTrait>();
					List<PersistenceNeed> persistenceNeeds = new List<PersistenceNeed>();
					float? baseMood = null;
					float? effectiveMood = null;
					List<PersistenceMoodModifier> persistenceMoodModifiers = new List<PersistenceMoodModifier>();

					foreach (KeyValuePair<string, object> colonistProperty in colonistProperties) {
						switch ((ColonistProperty)Enum.Parse(typeof(ColonistProperty), colonistProperty.Key)) {
							case ColonistProperty.Life:
								persistenceLife = LoadPersistenceLife((List<KeyValuePair<string, object>>)colonistProperty.Value);
								break;
							case ColonistProperty.Human:
								persistenceHuman = LoadPersistenceHuman((List<KeyValuePair<string, object>>)colonistProperty.Value);
								break;
							case ColonistProperty.PlayerMoved:
								playerMoved = bool.Parse((string)colonistProperty.Value);
								break;
							case ColonistProperty.Job:
								persistenceJob = LoadPersistenceJob((List<KeyValuePair<string, object>>)colonistProperty.Value);
								break;
							case ColonistProperty.StoredJob:
								persistenceStoredJob = LoadPersistenceJob((List<KeyValuePair<string, object>>)colonistProperty.Value);
								break;
							case ColonistProperty.BacklogJobs:
								foreach (KeyValuePair<string, object> backlogJobProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
									switch ((BacklogJobProperty)Enum.Parse(typeof(BacklogJobProperty), backlogJobProperty.Key)) {
										case BacklogJobProperty.BacklogJob:
											persistenceBacklogJobs.Add(LoadPersistenceJob((List<KeyValuePair<string, object>>)backlogJobProperty.Value));
											break;
									}
								}
								break;
							case ColonistProperty.Professions:
								foreach (KeyValuePair<string, object> professionProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
									switch ((ProfessionProperty)Enum.Parse(typeof(ProfessionProperty), professionProperty.Key)) {
										case ProfessionProperty.Profession:

											ColonistManager.ProfessionEnum? professionType = null;
											int? professionPriority = null;

											foreach (KeyValuePair<string, object> professionSubProperty in (List<KeyValuePair<string, object>>)professionProperty.Value) {
												switch ((ProfessionProperty)Enum.Parse(typeof(ProfessionProperty), professionSubProperty.Key)) {
													case ProfessionProperty.Name:
														professionType = (ColonistManager.ProfessionEnum)Enum.Parse(typeof(ColonistManager.ProfessionEnum), (string)professionSubProperty.Value);
														break;
													case ProfessionProperty.Priority:
														professionPriority = int.Parse((string)professionSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown profession sub property: " + professionSubProperty.Key + " " + professionSubProperty.Value);
														break;
												}
											}

											persistenceProfessions.Add(new PersistenceProfession(
												professionType,
												professionPriority
											));
											break;
										default:
											Debug.LogError("Unknown profession property: " + professionProperty.Key + " " + professionProperty.Value);
											break;
									}
								}
								break;
							case ColonistProperty.Skills:
								foreach (KeyValuePair<string, object> skillProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
									switch ((SkillProperty)Enum.Parse(typeof(SkillProperty), skillProperty.Key)) {
										case SkillProperty.Skill:

											ColonistManager.SkillEnum? skillType = null;
											int? skillLevel = null;
											float? skillNextLevelExperience = null;
											float? skillCurrentExperience = null;

											foreach (KeyValuePair<string, object> skillSubProperty in (List<KeyValuePair<string, object>>)skillProperty.Value) {
												switch ((SkillProperty)Enum.Parse(typeof(SkillProperty), skillSubProperty.Key)) {
													case SkillProperty.Type:
														skillType = (ColonistManager.SkillEnum)Enum.Parse(typeof(ColonistManager.SkillEnum), (string)skillSubProperty.Value);
														break;
													case SkillProperty.Level:
														skillLevel = int.Parse((string)skillSubProperty.Value);
														break;
													case SkillProperty.NextLevelExperience:
														skillNextLevelExperience = float.Parse((string)skillSubProperty.Value);
														break;
													case SkillProperty.CurrentExperience:
														skillCurrentExperience = float.Parse((string)skillSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown skill sub property: " + skillSubProperty.Key + " " + skillSubProperty.Value);
														break;
												}
											}

											persistenceSkills.Add(new PersistenceSkill(
												skillType, 
												skillLevel, 
												skillNextLevelExperience, 
												skillCurrentExperience
											));
											break;
										default:
											Debug.LogError("Unknown skill property: " + skillProperty.Key + " " + skillProperty.Value);
											break;
									}
								}
								break;
							case ColonistProperty.Traits:
								foreach (KeyValuePair<string, object> traitProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
									switch ((TraitProperty)Enum.Parse(typeof(TraitProperty), traitProperty.Key)) {
										case TraitProperty.Trait:

											ColonistManager.TraitEnum? traitType = null;

											foreach (KeyValuePair<string, object> traitSubProperty in (List<KeyValuePair<string, object>>)traitProperty.Value) {
												switch ((TraitProperty)Enum.Parse(typeof(TraitProperty), traitSubProperty.Key)) {
													case TraitProperty.Type:
														traitType = (ColonistManager.TraitEnum)Enum.Parse(typeof(ColonistManager.TraitEnum), (string)traitSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown trait sub property: " + traitSubProperty.Key + " " + traitSubProperty.Value);
														break;
												}
											}

											persistenceTraits.Add(new PersistenceTrait(
												traitType
											));
											break;
										default:
											Debug.LogError("Unknown trait property: " + traitProperty.Key + " " + traitProperty.Value);
											break;
									}
								}
								break;
							case ColonistProperty.Needs:
								foreach (KeyValuePair<string, object> needProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
									switch ((NeedProperty)Enum.Parse(typeof(NeedProperty), needProperty.Key)) {
										case NeedProperty.Need:

											ColonistManager.NeedEnum? needType = null;
											float? needValue = null;

											foreach (KeyValuePair<string, object> needSubProperty in (List<KeyValuePair<string, object>>)needProperty.Value) {
												switch ((NeedProperty)Enum.Parse(typeof(NeedProperty), needSubProperty.Key)) {
													case NeedProperty.Type:
														needType = (ColonistManager.NeedEnum)Enum.Parse(typeof(ColonistManager.NeedEnum), (string)needSubProperty.Value);
														break;
													case NeedProperty.Value:
														needValue = float.Parse((string)needSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown need sub property: " + needSubProperty.Key + " " + needSubProperty.Value);
														break;
												}
											}

											persistenceNeeds.Add(new PersistenceNeed(
												needType, 
												needValue
											));
											break;
										default:
											Debug.LogError("Unknown need property: " + needProperty.Key + " " + needProperty.Value);
											break;
									}
								}
								break;
							case ColonistProperty.BaseMood:
								baseMood = float.Parse((string)colonistProperty.Value);
								break;
							case ColonistProperty.EffectiveMood:
								effectiveMood = float.Parse((string)colonistProperty.Value);
								break;
							case ColonistProperty.MoodModifiers:
								foreach (KeyValuePair<string, object> moodModifierProperty in (List<KeyValuePair<string, object>>)colonistProperty.Value) {
									switch ((MoodModifierProperty)Enum.Parse(typeof(MoodModifierProperty), moodModifierProperty.Key)) {
										case MoodModifierProperty.MoodModifier:

											ColonistManager.MoodModifierEnum? moodModifierType = null;
											float? moodModifierTimeRemaining = null;

											foreach (KeyValuePair<string, object> moodModifierSubProperty in (List<KeyValuePair<string, object>>)moodModifierProperty.Value) {
												switch ((MoodModifierProperty)Enum.Parse(typeof(MoodModifierProperty), moodModifierSubProperty.Key)) {
													case MoodModifierProperty.Type:
														moodModifierType = (ColonistManager.MoodModifierEnum)Enum.Parse(typeof(ColonistManager.MoodModifierEnum), (string)moodModifierSubProperty.Value);
														break;
													case MoodModifierProperty.TimeRemaining:
														moodModifierTimeRemaining = float.Parse((string)moodModifierSubProperty.Value);
														break;
													default:
														Debug.LogError("Unknown mood modifier sub property: " + moodModifierSubProperty.Key + " " + moodModifierSubProperty.Value);
														break;
												}
											}

											persistenceMoodModifiers.Add(new PersistenceMoodModifier(moodModifierType, moodModifierTimeRemaining));
											break;
										default:
											Debug.LogError("Unknown mood modifier property: " + moodModifierProperty.Key + " " + moodModifierProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown colonist property: " + colonistProperty.Key + " " + colonistProperty.Value);
								break;
						}
					}

					persistenceColonists.Add(new PersistenceColonist(
						persistenceLife,
						persistenceHuman,
						playerMoved,
						persistenceJob,
						persistenceStoredJob,
						persistenceBacklogJobs,
						persistenceProfessions,
						persistenceSkills,
						persistenceTraits,
						persistenceNeeds,
						baseMood,
						effectiveMood,
						persistenceMoodModifiers
					));
					break;
				default:
					Debug.LogError("Unknown colonist property: " + property.Key + " " + property.Value);
					break;
			}
		}

		loadingState = LoadingState.LoadedColonists;
		return persistenceColonists;
	}

	public void ApplyLoadedColonists(List<PersistenceColonist> persistenceColonists) {
		foreach (PersistenceColonist persistenceColonist in persistenceColonists) {
			ColonistManager.Colonist colonist = new ColonistManager.Colonist(
				GameManager.colonyM.colony.map.GetTileFromPosition(persistenceColonist.persistenceLife.position.Value),
				persistenceColonist.persistenceLife.health.Value
			) {
				gender = persistenceColonist.persistenceLife.gender.Value,
				previousPosition = persistenceColonist.persistenceLife.previousPosition.Value,
				playerMoved = persistenceColonist.playerMoved.Value,
			};

			colonist.obj.transform.position = persistenceColonist.persistenceLife.position.Value;

			colonist.SetName(persistenceColonist.persistenceHuman.name);

			colonist.bodyIndices[HumanManager.Human.Appearance.Skin] = persistenceColonist.persistenceHuman.skinIndex.Value;
			colonist.moveSprites = GameManager.humanM.humanMoveSprites[colonist.bodyIndices[HumanManager.Human.Appearance.Skin]];
			colonist.bodyIndices[HumanManager.Human.Appearance.Hair] = persistenceColonist.persistenceHuman.hairIndex.Value;

			colonist.GetInventory().maxWeight = persistenceColonist.persistenceHuman.persistenceInventory.maxWeight.Value;
			colonist.GetInventory().maxVolume = persistenceColonist.persistenceHuman.persistenceInventory.maxVolume.Value;
			foreach (ResourceManager.ResourceAmount resourceAmount in persistenceColonist.persistenceHuman.persistenceInventory.resources) {
				colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
			}

			foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothingKVP in persistenceColonist.persistenceHuman.clothes) {
				colonist.GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(appearanceToClothingKVP.Value.type), 1, false);
				colonist.ChangeClothing(appearanceToClothingKVP.Key, appearanceToClothingKVP.Value);
			}

			if (persistenceColonist.persistenceStoredJob != null) {
				Job storedJob = LoadJob(persistenceColonist.persistenceStoredJob);
				colonist.storedJob = storedJob;
			}

			if (persistenceColonist.persistenceJob != null) {
				Job job = LoadJob(persistenceColonist.persistenceJob);
				colonist.SetJob(new ColonistJob(colonist, job, persistenceColonist.persistenceJob.resourcesColonistHas, job.containerPickups));
			}

			foreach (PersistenceJob persistenceBacklogJob in persistenceColonist.persistenceBacklogJobs) {
				Job backlogJob = LoadJob(persistenceBacklogJob);
				colonist.backlog.Add(backlogJob);
			}

			if (persistenceColonist.persistenceLife.pathEndPosition.HasValue) {
				colonist.MoveToTile(GameManager.colonyM.colony.map.GetTileFromPosition(persistenceColonist.persistenceLife.pathEndPosition.Value), true);
			}

			foreach (PersistenceProfession persistenceProfession in persistenceColonist.persistenceProfessions) {
				ColonistManager.ProfessionInstance profession = colonist.GetProfessionFromEnum(persistenceProfession.type.Value);
				profession.SetPriority(persistenceProfession.priority.Value);
			}

			foreach (PersistenceSkill persistenceSkill in persistenceColonist.persistenceSkills) {
				ColonistManager.SkillInstance skill = colonist.skills.Find(s => s.prefab.type == persistenceSkill.type);
				skill.level = persistenceSkill.level.Value;
				skill.nextLevelExperience = persistenceSkill.nextLevelExperience.Value;
				skill.currentExperience = persistenceSkill.currentExperience.Value;
			}

			foreach (PersistenceTrait persistenceTrait in persistenceColonist.persistenceTraits) {
				ColonistManager.TraitInstance trait = colonist.traits.Find(t => t.prefab.type == persistenceTrait.type);
				Debug.LogWarning("Load Colonist Traits: " + trait.prefab.type);
			}

			foreach (PersistenceNeed persistenceNeed in persistenceColonist.persistenceNeeds) {
				ColonistManager.NeedInstance need = colonist.needs.Find(n => n.prefab.type == persistenceNeed.type);
				need.SetValue(persistenceNeed.value.Value);
			}

			foreach (PersistenceMoodModifier persistenceMoodModifier in persistenceColonist.persistenceMoodModifiers) {
				colonist.AddMoodModifier(persistenceMoodModifier.type.Value);
				colonist.moodModifiers.Find(hm => hm.prefab.type == persistenceMoodModifier.type.Value).timer = persistenceMoodModifier.timeRemaining.Value;
			}

			colonist.baseMood = persistenceColonist.baseMood.Value;
			colonist.effectiveMood = persistenceColonist.effectiveMood.Value;
		}

		for (int i = 0; i < persistenceColonists.Count; i++) {
			PersistenceColonist persistenceColonist = persistenceColonists[i];
			ColonistManager.Colonist colonist = GameManager.colonistM.colonists[i];

			foreach (KeyValuePair<string, List<ResourceManager.ResourceAmount>> humanToReservedResourcesKVP in persistenceColonist.persistenceHuman.persistenceInventory.reservedResources) {
				foreach (ResourceManager.ResourceAmount resourceAmount in humanToReservedResourcesKVP.Value) {
					colonist.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
				}
				colonist.GetInventory().ReserveResources(humanToReservedResourcesKVP.Value, GameManager.humanM.humans.Find(h => h.name == humanToReservedResourcesKVP.Key));
			}
		}

		GameManager.uiM.SetColonistElements();
	}

	public enum JobProperty {
		Job, StoredJob, BacklogJob, Prefab, Type, Variation, Position, RotationIndex, Priority, Started, Progress, ColonistBuildTime, RequiredResources, ResourcesColonistHas, ContainerPickups, Plant, CreateResource, ActiveObject, TransferResources
	}

	public enum ContainerPickupProperty {
		ContainerPickup, Position, ResourcesToPickup
	}

	public void SaveJobs(StreamWriter file) {
		foreach (Job job in Job.jobs) {
			WriteJobLines(file, job, JobProperty.Job, 0);
		}
	}

	private void WriteJobLines(StreamWriter file, Job job, JobProperty jobType, int startLevel) {
		file.WriteLine(CreateKeyValueString(jobType, string.Empty, startLevel));

		file.WriteLine(CreateKeyValueString(JobProperty.Prefab, job.prefab.name, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Type, job.objectPrefab.type, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Variation, job.variation == null ? "null" : job.variation.name, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Position, FormatVector2ToString(job.tile.obj.transform.position), startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.RotationIndex, job.rotationIndex, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Priority, job.priority, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Started, job.started, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.Progress, job.jobProgress, startLevel + 1));
		file.WriteLine(CreateKeyValueString(JobProperty.ColonistBuildTime, job.colonistBuildTime, startLevel + 1));

		if (job.requiredResources != null && job.requiredResources.Count > 0) {
			file.WriteLine(CreateKeyValueString(JobProperty.RequiredResources, string.Empty, startLevel + 1));
			foreach (ResourceManager.ResourceAmount resourceAmount in job.requiredResources) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}

		if (job.resourcesColonistHas != null && job.resourcesColonistHas.Count > 0) {
			file.WriteLine(CreateKeyValueString(JobProperty.ResourcesColonistHas, string.Empty, startLevel + 1));
			foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesColonistHas) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}

		if (job.containerPickups != null && job.containerPickups.Count > 0) {
			file.WriteLine(CreateKeyValueString(JobProperty.ContainerPickups, string.Empty, startLevel + 1));
			foreach (ContainerPickup containerPickup in job.containerPickups) {
				file.WriteLine(CreateKeyValueString(ContainerPickupProperty.ContainerPickup, string.Empty, startLevel + 2));

				file.WriteLine(CreateKeyValueString(ContainerPickupProperty.Position, FormatVector2ToString(containerPickup.container.zeroPointTile.obj.transform.position), startLevel + 3));

				if (containerPickup.resourcesToPickup.Count > 0) {
					file.WriteLine(CreateKeyValueString(ContainerPickupProperty.ResourcesToPickup, string.Empty, startLevel + 3));
					foreach (ResourceManager.ResourceAmount resourceAmount in containerPickup.resourcesToPickup) {
						WriteResourceAmountLines(file, resourceAmount, startLevel + 4);
					}
				}
			}
		}

		if (job.createResource != null) {
			file.WriteLine(CreateKeyValueString(JobProperty.CreateResource, job.createResource.resource.type, startLevel + 1));
		}

		if (job.activeObject != null) {
			file.WriteLine(CreateKeyValueString(JobProperty.ActiveObject, string.Empty, startLevel + 1));

			file.WriteLine(CreateKeyValueString(ObjectProperty.Position, FormatVector2ToString(job.activeObject.zeroPointTile.obj.transform.position), startLevel + 2));
			file.WriteLine(CreateKeyValueString(ObjectProperty.Type, job.activeObject.prefab.type, startLevel + 2));
		}

		if (job.transferResources != null && job.transferResources.Count > 0) {
			file.WriteLine(CreateKeyValueString(JobProperty.TransferResources, string.Empty, startLevel + 1));
			foreach (ResourceManager.ResourceAmount resourceAmount in job.transferResources) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}
	}

	public class PersistenceJob {
		public string prefab;
		public ResourceManager.ObjectEnum? type;
		public string variation;
		public Vector2? position;
		public int? rotationIndex;
		public int? priority;
		public bool? started;
		public float? progress;
		public float? colonistBuildTime;
		public List<ResourceManager.ResourceAmount> requiredResources;
		public List<ResourceManager.ResourceAmount> resourcesColonistHas;
		public List<PersistenceContainerPickup> containerPickups;
		public ResourceManager.Resource createResource;
		public PersistenceObject activeObject;
		public List<ResourceManager.ResourceAmount> transferResources;

		public PersistenceJob(
			string prefab,
			ResourceManager.ObjectEnum? type,
			string variation,
			Vector2? position,
			int? rotationIndex,
			int? priority,
			bool? started,
			float? progress,
			float? colonistBuildTime,
			List<ResourceManager.ResourceAmount> requiredResources,
			List<ResourceManager.ResourceAmount> resourcesColonistHas,
			List<PersistenceContainerPickup> containerPickups,
			ResourceManager.Resource createResource,
			PersistenceObject activeObject,
			List<ResourceManager.ResourceAmount> transferResources
		) {
			this.prefab = prefab;
			this.type = type;
			this.variation = variation;
			this.position = position;
			this.rotationIndex = rotationIndex;
			this.priority = priority;
			this.started = started;
			this.progress = progress;
			this.colonistBuildTime = colonistBuildTime;
			this.requiredResources = requiredResources;
			this.resourcesColonistHas = resourcesColonistHas;
			this.containerPickups = containerPickups;
			this.createResource = createResource;
			this.activeObject = activeObject;
			this.transferResources = transferResources;
		}
	}

	public class PersistenceContainerPickup {
		public Vector2? containerPickupZeroPointTilePosition;
		public List<ResourceManager.ResourceAmount> containerPickupResourceAmounts;

		public PersistenceContainerPickup(
			Vector2? containerPickupZeroPointTilePosition,
			List<ResourceManager.ResourceAmount> containerPickupResourceAmounts
		) {
			this.containerPickupZeroPointTilePosition = containerPickupZeroPointTilePosition;
			this.containerPickupResourceAmounts = containerPickupResourceAmounts;
		}
	}

	public List<PersistenceJob> LoadJobs(string path) {
		List<PersistenceJob> persistenceJobs = new List<PersistenceJob>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((JobProperty)Enum.Parse(typeof(JobProperty), property.Key)) {
				case JobProperty.Job:
					persistenceJobs.Add(LoadPersistenceJob((List<KeyValuePair<string, object>>)property.Value));
					break;
			}
		}

		loadingState = LoadingState.LoadedJobs;
		return persistenceJobs;
	}

	public PersistenceJob LoadPersistenceJob(List<KeyValuePair<string, object>> properties) {
		string prefab = null;
		ResourceManager.ObjectEnum? type = null;
		string variation = null;
		Vector2? position = null;
		int? rotationIndex = null;
		int? priority = null;
		bool? started = null;
		float? progress = null;
		float? colonistBuildTime = null;
		List<ResourceManager.ResourceAmount> requiredResources = new List<ResourceManager.ResourceAmount>();
		List<ResourceManager.ResourceAmount> resourcesColonistHas = null;
		List<PersistenceContainerPickup> containerPickups = null;
		ResourceManager.Resource createResource = null;
		PersistenceObject activeObject = null;
		List<ResourceManager.ResourceAmount> transferResources = null;

		foreach (KeyValuePair<string, object> jobProperty in properties) {
			JobProperty jobPropertyKey = (JobProperty)Enum.Parse(typeof(JobProperty), jobProperty.Key);
			switch (jobPropertyKey) {
				case JobProperty.Prefab:
					prefab = UIManager.RemoveNonAlphanumericChars((string)jobProperty.Value);
					break;
				case JobProperty.Type:
					type = (ResourceManager.ObjectEnum)Enum.Parse(typeof(ResourceManager.ObjectEnum), (string)jobProperty.Value);
					break;
				case JobProperty.Variation:
					variation = UIManager.RemoveNonAlphanumericChars((string)jobProperty.Value);
					break;
				case JobProperty.Position:
					position = new Vector2(float.Parse(((string)jobProperty.Value).Split(',')[0]), float.Parse(((string)jobProperty.Value).Split(',')[1]));
					break;
				case JobProperty.RotationIndex:
					rotationIndex = int.Parse((string)jobProperty.Value);
					break;
				case JobProperty.Priority:
					priority = int.Parse((string)jobProperty.Value);
					break;
				case JobProperty.Started:
					started = bool.Parse((string)jobProperty.Value);
					break;
				case JobProperty.Progress:
					progress = float.Parse((string)jobProperty.Value);
					break;
				case JobProperty.ColonistBuildTime:
					colonistBuildTime = float.Parse((string)jobProperty.Value);
					break;
				case JobProperty.RequiredResources:
					foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
						requiredResources.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
					}
					break;
				case JobProperty.ResourcesColonistHas:
					resourcesColonistHas = new List<ResourceManager.ResourceAmount>();
					foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
						resourcesColonistHas.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
					}
					if (resourcesColonistHas.Count <= 0) {
						resourcesColonistHas = null;
					}
					break;
				case JobProperty.ContainerPickups:
					containerPickups = new List<PersistenceContainerPickup>();
					foreach (KeyValuePair<string, object> containerPickupProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
						switch ((ContainerPickupProperty)Enum.Parse(typeof(ContainerPickupProperty), containerPickupProperty.Key)) {
							case ContainerPickupProperty.ContainerPickup:

								Vector2? containerPickupZeroPointTilePosition = null;
								List<ResourceManager.ResourceAmount> containerPickupResourceAmounts = new List<ResourceManager.ResourceAmount>();

								foreach (KeyValuePair<string, object> containerPickupSubProperty in (List<KeyValuePair<string, object>>)containerPickupProperty.Value) {
									switch ((ContainerPickupProperty)Enum.Parse(typeof(ContainerPickupProperty), containerPickupSubProperty.Key)) {
										case ContainerPickupProperty.Position:
											containerPickupZeroPointTilePosition = new Vector2(float.Parse(((string)containerPickupSubProperty.Value).Split(',')[0]), float.Parse(((string)containerPickupSubProperty.Value).Split(',')[1]));
											break;
										case ContainerPickupProperty.ResourcesToPickup:
											foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)containerPickupSubProperty.Value) {
												containerPickupResourceAmounts.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
											}
											break;
										default:
											Debug.LogError("Unknown container pickup sub property: " + containerPickupSubProperty.Key + " " + containerPickupSubProperty.Value);
											break;
									}
								}

								containerPickups.Add(new PersistenceContainerPickup(containerPickupZeroPointTilePosition, containerPickupResourceAmounts));
								break;
							default:
								Debug.LogError("Unknown container pickup property: " + containerPickupProperty.Key + " " + containerPickupProperty.Value);
								break;
						}
					}
					if (containerPickups.Count <= 0) {
						containerPickups = null;
					}
					break;
				case JobProperty.CreateResource:
					createResource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)jobProperty.Value));
					break;
				case JobProperty.ActiveObject:
					Vector2? activeObjectZeroPointTilePosition = null;
					ResourceManager.ObjectEnum? activeObjectType = null;

					foreach (KeyValuePair<string, object> activeObjectProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
						switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), activeObjectProperty.Key)) {
							case ObjectProperty.Position:
								activeObjectZeroPointTilePosition = new Vector2(float.Parse(((string)activeObjectProperty.Value).Split(',')[0]), float.Parse(((string)activeObjectProperty.Value).Split(',')[1]));
								break;
							case ObjectProperty.Type:
								activeObjectType = (ResourceManager.ObjectEnum)Enum.Parse(typeof(ResourceManager.ObjectEnum), (string)activeObjectProperty.Value);
								break;
							default:
								Debug.LogError("Unknown active tile object property: " + activeObjectProperty.Key + " " + activeObjectProperty.Value);
								break;
						}
					}

					activeObject = new PersistenceObject(
						activeObjectType,
						null,
						activeObjectZeroPointTilePosition,
						null, null, null, null, null, null, null, null, null
					);
					break;
				case JobProperty.TransferResources:
					transferResources = new List<ResourceManager.ResourceAmount>();
					foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
						transferResources.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
					}
					if (transferResources.Count <= 0) {
						transferResources = null;
					}
					break;
				default:
					Debug.LogError("Unknown job property: " + jobProperty.Key + " " + jobProperty.Value);
					break;
			}
		}

		return new PersistenceJob(
			prefab,
			type,
			variation,
			position,
			rotationIndex,
			priority,
			started,
			progress,
			colonistBuildTime,
			requiredResources,
			resourcesColonistHas,
			containerPickups,
			createResource,
			activeObject,
			transferResources
		);
	}

	public void ApplyLoadedJobs(List<PersistenceJob> persistenceJobs) {
		foreach (PersistenceJob persistenceJob in persistenceJobs) {
			GameManager.jobM.CreateJob(LoadJob(persistenceJob));
		}
	}

	public Job LoadJob(PersistenceJob persistenceJob) {
		List<ContainerPickup> containerPickups = null;
		if (persistenceJob.containerPickups != null) {
			containerPickups = new List<ContainerPickup>();
			foreach (PersistenceContainerPickup persistenceContainerPickup in persistenceJob.containerPickups) {
				containerPickups.Add(new ContainerPickup(
					GameManager.resourceM.containers.Find(c => c.zeroPointTile == GameManager.colonyM.colony.map.GetTileFromPosition(persistenceContainerPickup.containerPickupZeroPointTilePosition.Value)),
					persistenceContainerPickup.containerPickupResourceAmounts
				));
			}
		}

		Job job = new(
			JobPrefab.GetJobPrefabByName(persistenceJob.prefab),
			GameManager.colonyM.colony.map.GetTileFromPosition(persistenceJob.position.Value),
			GameManager.resourceM.GetObjectPrefabByEnum(persistenceJob.type.Value),
			GameManager.resourceM.GetObjectPrefabByEnum(persistenceJob.type.Value).GetVariationFromString(persistenceJob.variation),
			persistenceJob.rotationIndex.Value
		) {
			started = persistenceJob.started ?? false,
			jobProgress = persistenceJob.progress ?? 0,
			colonistBuildTime = persistenceJob.colonistBuildTime ?? 0,
			requiredResources = persistenceJob.requiredResources,
			resourcesColonistHas = persistenceJob.resourcesColonistHas,
			containerPickups = containerPickups,
			transferResources = persistenceJob.transferResources
		};
		if (persistenceJob.priority.HasValue) {
			job.ChangePriority(persistenceJob.priority.Value);
		}
		
		if (persistenceJob.createResource != null) {
			ResourceManager.CraftingObject craftingObject = (ResourceManager.CraftingObject)job.tile.GetAllObjectInstances().Find(o => o.prefab.type == persistenceJob.activeObject.type);
			ResourceManager.CraftableResourceInstance craftableResourceInstance = craftingObject.resources.Find(r => r.resource == persistenceJob.createResource);
			job.SetCreateResourceData(craftableResourceInstance, false);
			craftableResourceInstance.job = job;
		}
		return job;
	}

	public enum ObjectProperty {
		Object, Type, Variation, Position, RotationIndex, Integrity, Active, Container, CraftingObject, Farm, SleepSpot
	}

	public enum ContainerProperty {
		Inventory
	}

	public enum CraftingObjectProperty {
		Resources, Fuels
	}

	public enum CraftableResourceProperty {
		CraftableResource, Resource, Priority, CreationMethod, TargetAmount, RemainingAmount, Enableable, FuelAmounts, Job
	}

	public enum PriorityResourceProperty {
		PriorityResource, Resource, Priority
	}

	public enum FarmProperty {
		GrowTimer
	}

	public enum SleepSpotProperty {
		OccupyingColonistName
	}

	public void SaveObjects(StreamWriter file) {
		foreach (List<ResourceManager.ObjectInstance> instances in GameManager.resourceM.objectInstances.Values) {
			foreach (ResourceManager.ObjectInstance instance in instances) {
				file.WriteLine(CreateKeyValueString(ObjectProperty.Object, string.Empty, 0));

				file.WriteLine(CreateKeyValueString(ObjectProperty.Type, instance.prefab.type, 1));
				file.WriteLine(CreateKeyValueString(ObjectProperty.Variation, instance.variation == null ? "null" : instance.variation.name, 1));
				file.WriteLine(CreateKeyValueString(ObjectProperty.Position, FormatVector2ToString(instance.zeroPointTile.obj.transform.position), 1));
				file.WriteLine(CreateKeyValueString(ObjectProperty.RotationIndex, instance.rotationIndex, 1));
				file.WriteLine(CreateKeyValueString(ObjectProperty.Integrity, instance.integrity, 1));
				file.WriteLine(CreateKeyValueString(ObjectProperty.Active, instance.active, 1));

				if (instance is ResourceManager.Container container) {
					file.WriteLine(CreateKeyValueString(ObjectProperty.Container, string.Empty, 1));
					WriteInventoryLines(file, container.GetInventory(), 2);
				} else if (instance is ResourceManager.CraftingObject craftingObject) {
					if (craftingObject.resources.Count > 0 || craftingObject.fuels.Count > 0) {
						file.WriteLine(CreateKeyValueString(ObjectProperty.CraftingObject, string.Empty, 1));
						if (craftingObject.resources.Count > 0) {
							file.WriteLine(CreateKeyValueString(CraftingObjectProperty.Resources, string.Empty, 2));
							foreach (ResourceManager.CraftableResourceInstance resource in craftingObject.resources) {
								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.CraftableResource, string.Empty, 3));

								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.Resource, resource.resource.type, 4));
								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.Priority, resource.priority.Get(), 4));
								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.CreationMethod, resource.creationMethod, 4));
								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.TargetAmount, resource.GetTargetAmount(), 4));
								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.RemainingAmount, resource.GetRemainingAmount(), 4));
								
								file.WriteLine(CreateKeyValueString(CraftableResourceProperty.Enableable, resource.enableable, 4));
								if (resource.fuelAmounts.Count > 0) {
									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.FuelAmounts, string.Empty, 4));
									foreach (ResourceManager.ResourceAmount fuelAmount in resource.fuelAmounts) {
										WriteResourceAmountLines(file, fuelAmount, 5);
									}
								}
							}
						}
						if (craftingObject.fuels.Count > 0) {
							file.WriteLine(CreateKeyValueString(CraftingObjectProperty.Fuels, string.Empty, 2));
							foreach (ResourceManager.PriorityResourceInstance fuel in craftingObject.fuels) {
								file.WriteLine(CreateKeyValueString(PriorityResourceProperty.PriorityResource, string.Empty, 3));

								file.WriteLine(CreateKeyValueString(PriorityResourceProperty.Resource, fuel.resource.type, 4));
								file.WriteLine(CreateKeyValueString(PriorityResourceProperty.Priority, fuel.priority.Get(), 4));
							}
						}
					}
				} else if (instance is ResourceManager.Farm farm) {
					file.WriteLine(CreateKeyValueString(ObjectProperty.Farm, string.Empty, 1));
					file.WriteLine(CreateKeyValueString(FarmProperty.GrowTimer, farm.growTimer, 2));
				} else if (instance is ResourceManager.SleepSpot sleepSpot) {
					file.WriteLine(CreateKeyValueString(ObjectProperty.SleepSpot, string.Empty, 1));
					file.WriteLine(CreateKeyValueString(SleepSpotProperty.OccupyingColonistName, sleepSpot.occupyingColonist.name, 2));
				}
			}
		}
	}

	public class PersistenceObject {
		public ResourceManager.ObjectEnum? type;
		public string variation;
		public Vector2? zeroPointTilePosition;
		public int? rotationIndex;
		public float? integrity;
		public bool? active;

		// Container
		public PersistenceInventory persistenceInventory;

		// Crafting Object
		public List<PersistenceCraftableResourceInstance> persistenceResources;
		public List<ResourceManager.PriorityResourceInstance> fuels;

		// Farm
		public ResourceManager.Resource seedResource;
		public float? growTimer;

		// Sleep Spot
		public string occupyingColonistName;

		public PersistenceObject(
			ResourceManager.ObjectEnum? type,
			string variation,
			Vector2? zeroPointTilePosition,
			int? rotationIndex,
			float? integrity,
			bool? active,
			PersistenceInventory persistenceInventory,
			List<PersistenceCraftableResourceInstance> persistenceResources,
			List<ResourceManager.PriorityResourceInstance> fuels,
			ResourceManager.Resource seedResource,
			float? growTimer,
			string occupyingColonistName
		) {
			this.type = type;
			this.variation = variation;
			this.zeroPointTilePosition = zeroPointTilePosition;
			this.rotationIndex = rotationIndex;
			this.integrity = integrity;
			this.active = active;

			this.persistenceInventory = persistenceInventory;

			this.persistenceResources = persistenceResources;
			this.fuels = fuels;

			this.seedResource = seedResource;
			this.growTimer = growTimer;

			this.occupyingColonistName = occupyingColonistName;
		}
	}

	public class PersistenceCraftableResourceInstance {
		public ResourceManager.Resource resource;
		public int? priority;
		public ResourceManager.CreationMethod? creationMethod;
		public int? targetAmount;
		public int? remainingAmount;
		public bool? enableable;
		public List<ResourceManager.ResourceAmount> fuelAmounts;

		public PersistenceCraftableResourceInstance(
			ResourceManager.Resource resource,
			int? priority,
			ResourceManager.CreationMethod? creationMethod,
			int? targetAmount,
			int? remainingAmount,
			bool? enableable,
			List<ResourceManager.ResourceAmount> fuelAmounts
		) {
			this.resource = resource;
			this.priority = priority;
			this.creationMethod = creationMethod;
			this.targetAmount = targetAmount;
			this.remainingAmount = remainingAmount;
			this.enableable = enableable;
			this.fuelAmounts = fuelAmounts;
		}
	}

	public List<PersistenceObject> LoadObjects(string path) {
		List<PersistenceObject> persistenceObjects = new List<PersistenceObject>();

		List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
		foreach (KeyValuePair<string, object> property in properties) {
			switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), property.Key)) {
				case ObjectProperty.Object:

					ResourceManager.ObjectEnum? type = null;
					string variation = null;
					Vector2? zeroPointTilePosition = null;
					int? rotationIndex = null;
					float? integrity = null;
					bool? active = null;

					// Container
					PersistenceInventory persistenceInventory = null;

					// Crafting Object
					List<PersistenceCraftableResourceInstance> persistenceResources = new List<PersistenceCraftableResourceInstance>();
					List<ResourceManager.PriorityResourceInstance> fuels = new List<ResourceManager.PriorityResourceInstance>();

					// Farm
					ResourceManager.Resource seedResource = null;
					float? growTimer = null;

					// Sleep Spot
					string occupyingColonistName = null;

					foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)property.Value) {
						switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), objectProperty.Key)) {
							case ObjectProperty.Type:
								type = (ResourceManager.ObjectEnum)Enum.Parse(typeof(ResourceManager.ObjectEnum), (string)objectProperty.Value);
								break;
							case ObjectProperty.Variation:
								variation = UIManager.RemoveNonAlphanumericChars((string)objectProperty.Value);
								break;
							case ObjectProperty.Position:
								zeroPointTilePosition = new Vector2(float.Parse(((string)objectProperty.Value).Split(',')[0]), float.Parse(((string)objectProperty.Value).Split(',')[1]));
								break;
							case ObjectProperty.RotationIndex:
								rotationIndex = int.Parse((string)objectProperty.Value);
								break;
							case ObjectProperty.Integrity:
								integrity = float.Parse((string)objectProperty.Value);
								break;
							case ObjectProperty.Active:
								active = bool.Parse((string)objectProperty.Value);
								break;
							case ObjectProperty.Container:
								foreach (KeyValuePair<string, object> containerProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
									switch ((ContainerProperty)Enum.Parse(typeof(ContainerProperty), containerProperty.Key)) {
										case ContainerProperty.Inventory:
											persistenceInventory = LoadPersistenceInventory((List<KeyValuePair<string, object>>)containerProperty.Value);
											break;
										default:
											Debug.LogError("Unknown container property: " + containerProperty.Key + " " + containerProperty.Value);
											break;
									}
								}
								break;
							case ObjectProperty.CraftingObject:
								foreach (KeyValuePair<string, object> craftingObjectProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
									switch ((CraftingObjectProperty)Enum.Parse(typeof(CraftingObjectProperty), craftingObjectProperty.Key)) {
										case CraftingObjectProperty.Resources:
											foreach (KeyValuePair<string, object> resourcesProperty in (List<KeyValuePair<string, object>>)craftingObjectProperty.Value) {
												switch ((CraftableResourceProperty)Enum.Parse(typeof(CraftableResourceProperty), resourcesProperty.Key)) {
													case CraftableResourceProperty.CraftableResource:

														ResourceManager.Resource resource = null;
														int? priority = null;
														ResourceManager.CreationMethod? creationMethod = null;
														int? targetAmount = null;
														int? remainingAmount = null;
														bool? enableable = null;
														List<ResourceManager.ResourceAmount> fuelAmounts = new List<ResourceManager.ResourceAmount>();

														foreach (KeyValuePair<string, object> craftableResourceProperty in (List<KeyValuePair<string, object>>)resourcesProperty.Value) {
															switch ((CraftableResourceProperty)Enum.Parse(typeof(CraftableResourceProperty), craftableResourceProperty.Key)) {
																case CraftableResourceProperty.Resource:
																	resource = GameManager.resourceM.GetResourceByString((string)craftableResourceProperty.Value);
																	break;
																case CraftableResourceProperty.Priority:
																	priority = int.Parse((string)craftableResourceProperty.Value);
																	break;
																case CraftableResourceProperty.CreationMethod:
																	creationMethod = (ResourceManager.CreationMethod)Enum.Parse(typeof(ResourceManager.CreationMethod), (string)craftableResourceProperty.Value);
																	break;
																case CraftableResourceProperty.TargetAmount:
																	targetAmount = int.Parse((string)craftableResourceProperty.Value);
																	break;
																case CraftableResourceProperty.RemainingAmount:
																	remainingAmount = int.Parse((string)craftableResourceProperty.Value);
																	break;
																case CraftableResourceProperty.Enableable:
																	enableable = bool.Parse((string)craftableResourceProperty.Value);
																	break;
																case CraftableResourceProperty.FuelAmounts:
																	foreach (KeyValuePair<string, object> fuelAmountsProperty in (List<KeyValuePair<string, object>>)craftableResourceProperty.Value) {
																		fuelAmounts.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)fuelAmountsProperty.Value));
																	}
																	break;
															}
														}

														persistenceResources.Add(
															new PersistenceCraftableResourceInstance(
																resource,
																priority,
																creationMethod,
																targetAmount,
																remainingAmount,
																enableable,
																fuelAmounts
															)
														);

														break;
												}
											}
											break;
										case CraftingObjectProperty.Fuels:
											foreach (KeyValuePair<string, object> fuelsProperty in (List<KeyValuePair<string, object>>)craftingObjectProperty.Value) {
												switch ((PriorityResourceProperty)Enum.Parse(typeof(PriorityResourceProperty), fuelsProperty.Key)) {
													case PriorityResourceProperty.PriorityResource:

														ResourceManager.Resource resource = null;
														int? priority = null;

														foreach (KeyValuePair<string, object> priorityResourceProperty in (List<KeyValuePair<string, object>>)fuelsProperty.Value) {
															switch ((PriorityResourceProperty)Enum.Parse(typeof(PriorityResourceProperty), priorityResourceProperty.Key)) {
																case PriorityResourceProperty.Resource:
																	resource = GameManager.resourceM.GetResourceByString((string)priorityResourceProperty.Value);
																	break;
																case PriorityResourceProperty.Priority:
																	priority = int.Parse((string)priorityResourceProperty.Value);
																	break;
															}
														}

														fuels.Add(new ResourceManager.PriorityResourceInstance(resource, priority.Value));

														break;
												}
											}
											break;
										default:
											Debug.LogError("Unknown crafting tile object property: " + craftingObjectProperty.Key + " " + craftingObjectProperty.Value);
											break;
									}
								}
								break;
							case ObjectProperty.Farm:
								foreach (KeyValuePair<string, object> farmProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
									switch ((FarmProperty)Enum.Parse(typeof(FarmProperty), farmProperty.Key)) {
										case FarmProperty.GrowTimer:
											growTimer = float.Parse((string)farmProperty.Value);
											break;
										default:
											Debug.LogError("Unknown farm property: " + farmProperty.Key + " " + farmProperty.Value);
											break;
									}
								}
								break;
							case ObjectProperty.SleepSpot:
								foreach (KeyValuePair<string, object> sleepSpotProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
									switch ((SleepSpotProperty)Enum.Parse(typeof(SleepSpotProperty), sleepSpotProperty.Key)) {
										case SleepSpotProperty.OccupyingColonistName:
											occupyingColonistName = (string)sleepSpotProperty.Value;
											break;
										default:
											Debug.LogError("Unknown sleep spot property: " + sleepSpotProperty.Key + " " + sleepSpotProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown tile object property: " + objectProperty.Key + " " + objectProperty.Value);
								break;
						}
					}

					persistenceObjects.Add(new PersistenceObject(
						type,
						variation,
						zeroPointTilePosition,
						rotationIndex,
						integrity,
						active,
						persistenceInventory,
						persistenceResources,
						fuels,
						seedResource,
						growTimer,
						occupyingColonistName
					));
					break;
				default:
					Debug.LogError("Unknown tile object property: " + property.Key + " " + property.Value);
					break;
			}
		}

		loadingState = LoadingState.LoadedObjects;
		return persistenceObjects;
	}

	public void ApplyLoadedObjects(List<PersistenceObject> persistenceObjects) {
		foreach (PersistenceObject persistenceObject in persistenceObjects) {
			TileManager.Tile zeroPointTile = GameManager.colonyM.colony.map.GetTileFromPosition(persistenceObject.zeroPointTilePosition.Value);

			ResourceManager.ObjectPrefab objectPrefab = GameManager.resourceM.GetObjectPrefabByEnum(persistenceObject.type.Value);
			ResourceManager.ObjectInstance objectInstance = GameManager.resourceM.CreateObjectInstance(
				objectPrefab,
				objectPrefab.GetVariationFromString(persistenceObject.variation),
				zeroPointTile,
				persistenceObject.rotationIndex.Value,
				true
			);
			objectInstance.integrity = persistenceObject.integrity.Value;
			objectInstance.SetActive(persistenceObject.active.Value);

			switch (objectPrefab.instanceType) {
				case ResourceManager.ObjectInstanceType.Container:
				case ResourceManager.ObjectInstanceType.TradingPost:
					ResourceManager.Container container = (ResourceManager.Container)objectInstance;
					container.GetInventory().maxWeight = persistenceObject.persistenceInventory.maxWeight.Value;
					container.GetInventory().maxVolume = persistenceObject.persistenceInventory.maxVolume.Value;
					foreach (ResourceManager.ResourceAmount resourceAmount in persistenceObject.persistenceInventory.resources) {
						container.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
					}
					// TODO (maybe already done?) Reserved resources must be set after colonists are loaded
					break;
				case ResourceManager.ObjectInstanceType.Farm:
					ResourceManager.Farm farm = (ResourceManager.Farm)objectInstance;
					farm.growTimer = persistenceObject.growTimer.Value;
					break;
				case ResourceManager.ObjectInstanceType.CraftingObject:
					ResourceManager.CraftingObject craftingObject = (ResourceManager.CraftingObject)objectInstance;
					craftingObject.SetActive(false); // Gets set to proper state after loading jobBacklog, this prevents it from creating a CreateResource job before then
					foreach (PersistenceCraftableResourceInstance persistenceResource in persistenceObject.persistenceResources) {
						craftingObject.resources.Add(
							new ResourceManager.CraftableResourceInstance(
								persistenceResource.resource,
								persistenceResource.priority.Value,
								persistenceResource.creationMethod.Value,
								persistenceResource.targetAmount.Value,
								craftingObject,
								persistenceResource.remainingAmount
							) {
								enableable = persistenceResource.enableable.Value,
								fuelAmounts = persistenceResource.fuelAmounts
							}
						);
					}
					craftingObject.fuels = persistenceObject.fuels;
					break;
				case ResourceManager.ObjectInstanceType.SleepSpot:
					// Occupying colonist must be set after colonists are loaded
					break;
			}

			zeroPointTile.SetObject(objectInstance);
			objectInstance.obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
			objectInstance.FinishCreation();
			if (objectInstance.prefab.canRotate) {
				objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[objectInstance.rotationIndex];
			}
		}
	}

	public enum InventoryProperty {
		Inventory, MaxWeight, MaxVolume, Resources, ReservedResources, HumanName, ContainerPosition
	}

	private void WriteInventoryLines(StreamWriter file, ResourceManager.Inventory inventory, int startLevel) {
		file.WriteLine(CreateKeyValueString(InventoryProperty.Inventory, string.Empty, startLevel));
		file.WriteLine(CreateKeyValueString(InventoryProperty.MaxWeight, inventory.maxWeight, startLevel + 1));
		file.WriteLine(CreateKeyValueString(InventoryProperty.MaxVolume, inventory.maxVolume, startLevel + 1));
		if (inventory.parent is HumanManager.Human human) {
			file.WriteLine(CreateKeyValueString(InventoryProperty.HumanName, human.name, startLevel + 1));
		} else if (inventory.parent is ResourceManager.Container container) {
			file.WriteLine(CreateKeyValueString(InventoryProperty.ContainerPosition, FormatVector2ToString(container.zeroPointTile.obj.transform.position), startLevel + 1));
		}
		if (inventory.resources.Count > 0) {
			file.WriteLine(CreateKeyValueString(InventoryProperty.Resources, string.Empty, startLevel + 1));
			foreach (ResourceManager.ResourceAmount resourceAmount in inventory.resources) {
				WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
			}
		}
		if (inventory.reservedResources.Count > 0) {
			file.WriteLine(CreateKeyValueString(InventoryProperty.ReservedResources, string.Empty, startLevel + 1));
			foreach (ResourceManager.ReservedResources reservedResources in inventory.reservedResources) {
				file.WriteLine(CreateKeyValueString(ReservedResourcesProperty.ReservedResourceAmounts, string.Empty, startLevel + 2));
				file.WriteLine(CreateKeyValueString(ReservedResourcesProperty.HumanName, reservedResources.human.name, startLevel + 3));
				file.WriteLine(CreateKeyValueString(ReservedResourcesProperty.Resources, string.Empty, startLevel + 3));
				foreach (ResourceManager.ResourceAmount resourceAmount in reservedResources.resources) {
					WriteResourceAmountLines(file, resourceAmount, startLevel + 4);
				}
			}
		}
	}

	public class PersistenceInventory {
		public int? maxWeight;
		public int? maxVolume;
		public List<ResourceManager.ResourceAmount> resources;
		public List<KeyValuePair<string, List<ResourceManager.ResourceAmount>>> reservedResources;
		public string humanName;
		public Vector2? containerZeroPointTilePosition;

		public PersistenceInventory(
			int? maxWeight,
			int? maxVolume,
			List<ResourceManager.ResourceAmount> resources,
			List<KeyValuePair<string, List<ResourceManager.ResourceAmount>>> reservedResources,
			string humanName,
			Vector2? containerZeroPointTilePosition
		) {
			this.maxWeight = maxWeight;
			this.maxVolume = maxVolume;
			this.resources = resources;
			this.reservedResources = reservedResources;
			this.humanName = humanName;
			this.containerZeroPointTilePosition = containerZeroPointTilePosition;
		}
	}

	private PersistenceInventory LoadPersistenceInventory(List<KeyValuePair<string, object>> properties) {
		int? maxWeight = null;
		int? maxVolume = null;
		List<ResourceManager.ResourceAmount> resources = new List<ResourceManager.ResourceAmount>();
		List<KeyValuePair<string, List<ResourceManager.ResourceAmount>>> reservedResources = new List<KeyValuePair<string, List<ResourceManager.ResourceAmount>>>();
		string humanName = null;
		Vector2? containerZeroPointTilePosition = null;

		foreach (KeyValuePair<string, object> inventoryProperty in properties) {
			InventoryProperty inventoryPropertyKey = (InventoryProperty)Enum.Parse(typeof(InventoryProperty), inventoryProperty.Key);
			switch (inventoryPropertyKey) {
				case InventoryProperty.MaxWeight:
					maxWeight = int.Parse((string)inventoryProperty.Value);
					break;
				case InventoryProperty.MaxVolume:
					maxVolume = int.Parse((string)inventoryProperty.Value);
					break;
				case InventoryProperty.Resources:
					foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)inventoryProperty.Value) {
						resources.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
					}
					break;
				case InventoryProperty.ReservedResources:
					foreach (KeyValuePair<string, object> reservedResourcesProperty in (List<KeyValuePair<string, object>>)inventoryProperty.Value) {
						ReservedResourcesProperty reservedResourcesPropertyKey = (ReservedResourcesProperty)Enum.Parse(typeof(ReservedResourcesProperty), reservedResourcesProperty.Key);
						switch (reservedResourcesPropertyKey) {
							case ReservedResourcesProperty.ReservedResourceAmounts:

								string reservingHumanName = null;
								List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();

								foreach (KeyValuePair<string, object> reservedResourcesSubProperty in (List<KeyValuePair<string, object>>)reservedResourcesProperty.Value) {
									ReservedResourcesProperty reservedResourcesSubPropertyKey = (ReservedResourcesProperty)Enum.Parse(typeof(ReservedResourcesProperty), reservedResourcesSubProperty.Key);
									switch (reservedResourcesSubPropertyKey) {
										case ReservedResourcesProperty.HumanName:
											reservingHumanName = (string)reservedResourcesSubProperty.Value;
											break;
										case ReservedResourcesProperty.Resources:
											foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)reservedResourcesSubProperty.Value) {
												resourcesToReserve.Add(LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
											}
											break;
										default:
											Debug.LogError("Unknown reserved resources sub property: " + inventoryProperty.Key + " " + inventoryProperty.Value);
											break;
									}
								}

								reservedResources.Add(new KeyValuePair<string, List<ResourceManager.ResourceAmount>>(reservingHumanName, resourcesToReserve));

								break;
							default:
								Debug.LogError("Unknown reserved resources property: " + inventoryProperty.Key + " " + inventoryProperty.Value);
								break;
						}
					}
					break;
				case InventoryProperty.HumanName:
					humanName = (string)inventoryProperty.Value;
					break;
				case InventoryProperty.ContainerPosition:
					containerZeroPointTilePosition = new Vector2(float.Parse(((string)inventoryProperty.Value).Split(',')[0]), float.Parse(((string)inventoryProperty.Value).Split(',')[1]));
					break;
				default:
					Debug.LogError("Unknown inventory property: " + inventoryProperty.Key + " " + inventoryProperty.Value);
					break;
			}
		}

		return new PersistenceInventory(maxWeight, maxVolume, resources, reservedResources, humanName, containerZeroPointTilePosition);
	}

	private void WriteResourceAmountLines(StreamWriter file, ResourceManager.ResourceAmount resourceAmount, int startLevel) {
		file.WriteLine(CreateKeyValueString(ResourceAmountProperty.ResourceAmount, string.Empty, startLevel));
		file.WriteLine(CreateKeyValueString(ResourceAmountProperty.Type, resourceAmount.resource.type, startLevel + 1));
		file.WriteLine(CreateKeyValueString(ResourceAmountProperty.Amount, resourceAmount.amount, startLevel + 1));
	}

	public ResourceManager.ResourceAmount LoadResourceAmount(List<KeyValuePair<string, object>> properties) {
		ResourceManager.Resource resource = null;
		int? amount = null;

		foreach (KeyValuePair<string, object> resourceAmountProperty in properties) {
			ResourceAmountProperty resourceAmountPropertyKey = (ResourceAmountProperty)Enum.Parse(typeof(ResourceAmountProperty), resourceAmountProperty.Key);
			switch (resourceAmountPropertyKey) {
				case ResourceAmountProperty.Type:
					resource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)resourceAmountProperty.Value));
					break;
				case ResourceAmountProperty.Amount:
					amount = int.Parse((string)resourceAmountProperty.Value);
					break;
				default:
					Debug.LogError("Unknown resource amount property: " + resourceAmountProperty.Key + " " + resourceAmountProperty.Value);
					break;
			}
		}

		return new ResourceManager.ResourceAmount(resource, amount.Value);
	}

	public enum ResourceProperty {
		Resource, Type
	}

	public void SaveResources(StreamWriter file) {
		foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources()) {
			file.WriteLine(CreateKeyValueString(ResourceProperty.Resource, string.Empty, 0));

			file.WriteLine(CreateKeyValueString(ResourceProperty.Type, resource.type, 1));
		}
	}

	public void LoadResources(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			ResourceProperty key = (ResourceProperty)Enum.Parse(typeof(ResourceProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case ResourceProperty.Resource:

					ResourceManager.Resource resource = null;

					foreach (KeyValuePair<string, object> resourceProperty in (List<KeyValuePair<string, object>>)property.Value) {
						ResourceProperty resourcePropertyKey = (ResourceProperty)Enum.Parse(typeof(ResourceProperty), resourceProperty.Key);
						switch (resourcePropertyKey) {
							case ResourceProperty.Type:
								resource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)resourceProperty.Value));
								break;
							default:
								Debug.LogError("Unknown resource property: " + resourceProperty.Key + " " + resourceProperty.Value);
								break;
						}
					}

					break;
				default:
					Debug.LogError("Unknown resource property: " + property.Key + " " + property.Value);
					break;
			}
		}

		loadingState = LoadingState.LoadedResources;
	}

	public enum TimeProperty {
		Minute, Hour, Day, Season, Year
	}

	public void SaveTime(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(TimeProperty.Minute, GameManager.timeM.GetMinute(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Hour, GameManager.timeM.GetHour(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Day, GameManager.timeM.GetDay(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Season, GameManager.timeM.GetSeason(), 0));
		file.WriteLine(CreateKeyValueString(TimeProperty.Year, GameManager.timeM.GetYear(), 0));
	}

	public void LoadTime(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			TimeProperty key = (TimeProperty)Enum.Parse(typeof(TimeProperty), property.Key);
			object value = property.Value;
			switch (key) {
				case TimeProperty.Minute:
					GameManager.timeM.SetMinute(int.Parse((string)value));
					break;
				case TimeProperty.Hour:
					GameManager.timeM.SetHour(int.Parse((string)value));
					break;
				case TimeProperty.Day:
					GameManager.timeM.SetDay(int.Parse((string)value));
					break;
				case TimeProperty.Season:
					GameManager.timeM.SetSeason((TimeManager.Season)Enum.Parse(typeof(TimeManager.Season), (string)value));
					break;
				case TimeProperty.Year:
					GameManager.timeM.SetYear(int.Parse((string)value));
					break;
				default:
					Debug.LogError("Unknown time property: " + property.Key + " " + property.Value);
					break;
			}
		}

		loadingState = LoadingState.LoadedTime;
	}

	public enum UIProperty {
		ObjectPrefabs
	}

	public enum ObjectPrefabProperty {
		ObjectPrefab, Type, LastSelectedVariation
	}

	public void SaveUI(StreamWriter file) {
		file.WriteLine(CreateKeyValueString(UIProperty.ObjectPrefabs, string.Empty, 0));
		foreach (ResourceManager.ObjectPrefab objectPrefab in GameManager.resourceM.GetObjectPrefabs()) {
			file.WriteLine(CreateKeyValueString(ObjectPrefabProperty.ObjectPrefab, string.Empty, 1));

			file.WriteLine(CreateKeyValueString(ObjectPrefabProperty.Type, objectPrefab.type, 2));
			file.WriteLine(CreateKeyValueString(ObjectPrefabProperty.LastSelectedVariation, (objectPrefab.lastSelectedVariation == null ? "null" : objectPrefab.lastSelectedVariation.name), 2));
		}
	}

	public void LoadUI(string path) {
		foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
			switch ((UIProperty)Enum.Parse(typeof(UIProperty), property.Key)) {
				case UIProperty.ObjectPrefabs:

					Dictionary<ResourceManager.ObjectPrefab, ResourceManager.Variation> lastSelectedVariations = new Dictionary<ResourceManager.ObjectPrefab, ResourceManager.Variation>();

					foreach (KeyValuePair<string, object> objectPrefabProperty in (List<KeyValuePair<string, object>>)property.Value) {
						switch ((ObjectPrefabProperty)Enum.Parse(typeof(ObjectPrefabProperty), objectPrefabProperty.Key)) {
							case ObjectPrefabProperty.ObjectPrefab:

								ResourceManager.ObjectPrefab objectPrefab = null;
								ResourceManager.Variation lastSelectedVariation = null;

								foreach (KeyValuePair<string, object> objectPrefabSubProperty in (List<KeyValuePair<string, object>>)objectPrefabProperty.Value) {
									switch ((ObjectPrefabProperty)Enum.Parse(typeof(ObjectPrefabProperty), objectPrefabSubProperty.Key)) {
										case ObjectPrefabProperty.Type:
											objectPrefab = GameManager.resourceM.GetObjectPrefabByString((string)objectPrefabSubProperty.Value);
											break;
										case ObjectPrefabProperty.LastSelectedVariation:
											lastSelectedVariation = objectPrefab.GetVariationFromString((string)objectPrefabSubProperty.Value);
											break;
										default:
											Debug.LogError("Unknown object prefab sub property: " + objectPrefabSubProperty.Key + " " + objectPrefabSubProperty.Value);
											break;
									}
								}

								lastSelectedVariations.Add(objectPrefab, lastSelectedVariation);

								break;
							default:
								Debug.LogError("Unknown object prefab property: " + objectPrefabProperty.Key + " " + objectPrefabProperty.Value);
								break;
						}
					}

					foreach (ResourceManager.ObjectPrefab objectPrefab in GameManager.resourceM.GetObjectPrefabs()) {
						objectPrefab.lastSelectedVariation = lastSelectedVariations[objectPrefab];
					}

					break;
				default:
					Debug.LogError("Unknown UI property: " + property.Key + " " + property.Value);
					break;
			}
		}
	}

	public bool IsUniverseLoadable(PersistenceUniverse persistenceUniverse) {
		if (persistenceUniverse == null) {
			return false;
		}

		bool saveVersionValid = persistenceUniverse.configurationProperties[ConfigurationProperty.SaveVersion] == saveVersion.Value;
		return saveVersionValid;
	}

	public bool IsLastSaveUniverseLoadable() {
		LastSaveProperties lastSaveProperties = GetLastSaveProperties();

		if (lastSaveProperties == null) {
			return false;
		}

		PersistenceUniverse persistenceUniverse = GetPersistenceUniverses().Find(pu => string.Equals(Path.GetFullPath(pu.path), Path.GetFullPath(lastSaveProperties.lastSaveUniversePath), StringComparison.OrdinalIgnoreCase));

		if (persistenceUniverse == null) {
			return false;
		}

		return IsUniverseLoadable(persistenceUniverse);
	}

	public void ContinueFromMostRecentSave() {
		LastSaveProperties lastSaveProperties = GetLastSaveProperties();

		PersistenceUniverse persistenceUniverse = GetPersistenceUniverses().Find(pu => string.Equals(Path.GetFullPath(pu.path), Path.GetFullPath(lastSaveProperties.lastSaveUniversePath), StringComparison.OrdinalIgnoreCase));

		if (!IsUniverseLoadable(persistenceUniverse)) {
			return;
		}

		GameManager.persistenceM.ApplyLoadedConfiguration(persistenceUniverse);
		GameManager.persistenceM.ApplyLoadedUniverse(persistenceUniverse);

		PersistencePlanet persistencePlanet = GetPersistencePlanets().Find(pp => string.Equals(Path.GetFullPath(pp.path), Path.GetFullPath(lastSaveProperties.lastSavePlanetPath + "/planet.snowship"), StringComparison.OrdinalIgnoreCase));
		GameManager.persistenceM.ApplyLoadedPlanet(persistencePlanet);

		PersistenceColony persistenceColony = GetPersistenceColonies().Find(pc => string.Equals(Path.GetFullPath(pc.path), Path.GetFullPath(lastSaveProperties.lastSaveColonyPath + "/colony.snowship"), StringComparison.OrdinalIgnoreCase));
		GameManager.persistenceM.ApplyLoadedColony(persistenceColony);

		PersistenceSave persistenceSave = GetPersistenceSaves().Find(ps => string.Equals(Path.GetFullPath(ps.path), Path.GetFullPath(lastSaveProperties.lastSaveSavePath + "/save.snowship"), StringComparison.OrdinalIgnoreCase));
		startCoroutineReference.StartCoroutine(GameManager.persistenceM.ApplyLoadedSave(persistenceSave));
	}
}