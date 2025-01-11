using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PResource : PersistenceHandler {

		public enum ResourceProperty {
			Resource,
			Type
		}

		public void SaveResources(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "resources.snowship");

			foreach (ResourceManager.Resource resource in GameManager.resourceM.GetResources()) {
				file.WriteLine(CreateKeyValueString(ResourceProperty.Resource, string.Empty, 0));

				file.WriteLine(CreateKeyValueString(ResourceProperty.Type, resource.type, 1));
			}

			file.Close();
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

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedResources;
		}

	}
}
