using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NTime;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PTime : SimulationDateTime
	{

		public enum TimeProperty {
			Minute,
			Hour,
			Day,
			Season,
			Year
		}

		public void SaveTime(string saveDirectoryPath) {

			StreamWriter file = PU.CreateFileAtDirectory(saveDirectoryPath, "time.snowship");

			file.WriteLine(PU.CreateKeyValueString(TimeProperty.Minute, GameManager.Get<TimeManager>().Time.Minute, 0));
			file.WriteLine(PU.CreateKeyValueString(TimeProperty.Hour, GameManager.Get<TimeManager>().Time.Hour, 0));
			file.WriteLine(PU.CreateKeyValueString(TimeProperty.Day, GameManager.Get<TimeManager>().Time.Day, 0));
			file.WriteLine(PU.CreateKeyValueString(TimeProperty.Season, GameManager.Get<TimeManager>().Time.Season, 0));
			file.WriteLine(PU.CreateKeyValueString(TimeProperty.Year, GameManager.Get<TimeManager>().Time.Year, 0));

			file.Close();
		}

		public void LoadTime(string path) {
			foreach (KeyValuePair<string, object> property in PU.GetKeyValuePairsFromFile(path)) {
				TimeProperty key = (TimeProperty)Enum.Parse(typeof(TimeProperty), property.Key);
				object value = property.Value;
				switch (key) {
					case TimeProperty.Minute:
						GameManager.Get<TimeManager>().Time.Minute = int.Parse((string)value);
						break;
					case TimeProperty.Hour:
						GameManager.Get<TimeManager>().Time.Hour = int.Parse((string)value);
						break;
					case TimeProperty.Day:
						GameManager.Get<TimeManager>().Time.Day = int.Parse((string)value);
						break;
					case TimeProperty.Season:
						GameManager.Get<TimeManager>().Time.Season = (Season)Enum.Parse(typeof(Season), (string)value);
						break;
					case TimeProperty.Year:
						GameManager.Get<TimeManager>().Time.Year = int.Parse((string)value);
						break;
					default:
						Debug.LogError("Unknown time property: " + property.Key + " " + property.Value);
						break;
				}
			}

			GameManager.Get<PersistenceManager>().loadingState = PersistenceManager.LoadingState.LoadedTime;
		}

	}
}