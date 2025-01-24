using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowship.NColony;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence
{
	public class PSave
	{

		public enum SaveProperty {
			SaveDateTime
		}

		public void SaveSave(string saveDirectoryPath, string saveDateTime) {

			StreamWriter file = PU.CreateFileAtDirectory(saveDirectoryPath, "save.snowship");

			file.WriteLine(PU.CreateKeyValueString(SaveProperty.SaveDateTime, saveDateTime, 0));

			file.Close();
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
			string savesPath = GameManager.Get<ColonyManager>().colony.directory + "/Saves";
			if (Directory.Exists(savesPath)) {
				foreach (string saveDirectoryPath in Directory.GetDirectories(savesPath)) {
					PersistenceSave persistenceSave = LoadSave(saveDirectoryPath + "/save.snowship");

					string screenshotPath = Directory.GetFiles(saveDirectoryPath).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
					if (screenshotPath != null) {
						persistenceSave.image = PU.LoadSpriteFromImageFile(screenshotPath);
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
				properties = PU.GetKeyValuePairsFromFile(path);
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

	}
}