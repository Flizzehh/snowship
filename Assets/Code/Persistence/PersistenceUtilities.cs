using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NPersistence {
	public static class PersistenceUtilities
	{
		public static string GetPersistentDataPath() {
			return Application.persistentDataPath;
		}

		public static string CreateKeyValueString(object key, object value, int level) {
			return $"{new string('\t', level)}<{key}>{value}";
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

		public static string FormatVector2ToString(Vector2 vector2) {
			return vector2.x + "," + vector2.y;
		}

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

		public static async UniTask CreateScreenshot(string fileName) {
			GameObject canvas = GameObject.Find("Canvas");
			canvas.SetActive(false);
			await UniTask.WaitForEndOfFrame();
			ScreenCapture.CaptureScreenshot(fileName + ".png");
			canvas.SetActive(true);
		}

		public static Sprite LoadSpriteFromImageFile(string path, int x = 280, int y = 158) {
			if (File.Exists(path)) {
				byte[] fileData = File.ReadAllBytes(path);
				Texture2D texture = new Texture2D(x, y);
				texture.LoadImage(fileData);
				return Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			}
			return null;
		}

		public static Sprite LoadSaveImageFromSaveDirectoryPath(string saveDirectoryPath) {
			string screenshotPath = Directory.GetFiles(saveDirectoryPath).ToList().Find(f => Path.GetExtension(f).ToLower() == ".png");
			if (screenshotPath != null) {
				return LoadSpriteFromImageFile(screenshotPath);
			}
			return null;
		}
	}
}