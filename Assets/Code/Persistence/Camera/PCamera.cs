using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PCamera : PersistenceHandler {

		public enum CameraProperty {
			Position,
			Zoom
		}

		public void SaveCamera(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "camera.snowship");

			file.WriteLine(CreateKeyValueString(CameraProperty.Position, FormatVector2ToString(GameManager.cameraM.GetCameraPosition()), 0));
			file.WriteLine(CreateKeyValueString(CameraProperty.Zoom, GameManager.cameraM.camera.orthographicSize, 0));

			file.Close();
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

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedCamera;
		}

	}
}
