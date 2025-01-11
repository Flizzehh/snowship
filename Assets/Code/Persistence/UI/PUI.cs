using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PUI : PersistenceHandler {

		public enum UIProperty {
			ObjectPrefabs
		}

		public enum ObjectPrefabProperty {
			ObjectPrefab,
			Type,
			LastSelectedVariation
		}

		public void SaveUI(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "ui.snowship");

			file.WriteLine(CreateKeyValueString(UIProperty.ObjectPrefabs, string.Empty, 0));
			foreach (ResourceManager.ObjectPrefab objectPrefab in GameManager.resourceM.GetObjectPrefabs()) {
				file.WriteLine(CreateKeyValueString(ObjectPrefabProperty.ObjectPrefab, string.Empty, 1));

				file.WriteLine(CreateKeyValueString(ObjectPrefabProperty.Type, objectPrefab.type, 2));
				file.WriteLine(CreateKeyValueString(ObjectPrefabProperty.LastSelectedVariation, (objectPrefab.lastSelectedVariation == null ? "null" : objectPrefab.lastSelectedVariation.name), 2));
			}

			file.Close();
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

	}
}
