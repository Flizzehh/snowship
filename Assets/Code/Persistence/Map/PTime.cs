using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NTime;
using UnityEngine;
using Time = Snowship.NTime.Time;

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

			file.WriteLine(CreateKeyValueString(TimeProperty.Minute, Time.Minute, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Hour, Time.Hour, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Day, Time.Day, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Season, Time.Season, 0));
			file.WriteLine(CreateKeyValueString(TimeProperty.Year, Time.Year, 0));

			file.Close();
		}

		public void LoadTime(string path) {
			foreach (KeyValuePair<string, object> property in GetKeyValuePairsFromFile(path)) {
				TimeProperty key = (TimeProperty)Enum.Parse(typeof(TimeProperty), property.Key);
				object value = property.Value;
				switch (key) {
					case TimeProperty.Minute:
						Time.Minute = int.Parse((string)value);
						break;
					case TimeProperty.Hour:
						Time.Hour = int.Parse((string)value);
						break;
					case TimeProperty.Day:
						Time.Day = int.Parse((string)value);
						break;
					case TimeProperty.Season:
						Time.Season = (Season)Enum.Parse(typeof(Season), (string)value);
						break;
					case TimeProperty.Year:
						Time.Year = int.Parse((string)value);
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
