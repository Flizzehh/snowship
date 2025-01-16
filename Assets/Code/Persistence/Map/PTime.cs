using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PTime : PersistenceHandler {

		public enum TimeProperty {
			Minute,
			Hour,
			Day,
			Season,
			Year
		}

		public void SaveTime(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "time.snowship");

			file.WriteLine(CreateKeyValueString(TimeProperty.Minute, GameManager.timeM.Time.Minute, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Hour, GameManager.timeM.Time.Hour, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Day, GameManager.timeM.Time.Day, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Season, GameManager.timeM.Time.Season, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Year, GameManager.timeM.Time.Year, 0));

			file.Close();
		}

		public void LoadTime(string path) {
			foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
				TimeProperty key = (TimeProperty)Enum.Parse(typeof(TimeProperty), property.Key);
				object value = property.Value;
				switch (key) {
					case TimeProperty.Minute:
						GameManager.timeM.Time.Minute = int.Parse((string)value);
						break;
					case TimeProperty.Hour:
						GameManager.timeM.Time.Hour = int.Parse((string)value);
						break;
					case TimeProperty.Day:
						GameManager.timeM.Time.Day = int.Parse((string)value);
						break;
					case TimeProperty.Season:
						GameManager.timeM.Time.Season = (Season)Enum.Parse(typeof(Season), (string)value);
						break;
					case TimeProperty.Year:
						GameManager.timeM.Time.Year = int.Parse((string)value);
						break;
					default:
						Debug.LogError("Unknown time property: " + property.Key + " " + property.Value);
						break;
				}
			}

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedTime;
		}

	}
}
