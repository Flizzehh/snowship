using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NCamera;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PCamera
	{
		private enum CameraProperty
		{
			Position,
			Zoom
		}

		public void SaveCamera(string saveDirectoryPath) {

			StreamWriter file = PU.CreateFileAtDirectory(saveDirectoryPath, "camera.snowship");

			file.WriteLine(PU.CreateKeyValueString(CameraProperty.Position, PU.FormatVector2ToString(GameManager.Get<CameraManager>().GetCameraPosition()), 0));
			file.WriteLine(PU.CreateKeyValueString(CameraProperty.Zoom, GameManager.Get<CameraManager>().camera.orthographicSize, 0));

			file.Close();
		}

		public void LoadCamera(string path) {
			foreach (KeyValuePair<string, object> property in PU.GetKeyValuePairsFromFile(path)) {
				switch ((CameraProperty)Enum.Parse(typeof(CameraProperty), property.Key)) {
					case CameraProperty.Position:
						GameManager.Get<CameraManager>().SetCameraPosition(new Vector2(float.Parse(((string)property.Value).Split(',')[0]), float.Parse(((string)property.Value).Split(',')[1])));
						break;
					case CameraProperty.Zoom:
						GameManager.Get<CameraManager>().SetCameraZoom(float.Parse((string)property.Value));
						break;
					default:
						Debug.LogError("Unknown camera property: " + property.Key + " " + property.Value);
						break;
				}
			}

			GameManager.Get<PersistenceManager>().loadingState = PersistenceManager.LoadingState.LoadedCamera;
		}

	}
}