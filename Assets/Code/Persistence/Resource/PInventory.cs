using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PInventory : PersistenceHandler {

		public enum ResourceAmountProperty {
			ResourceAmount,
			Type,
			Amount
		}

		public enum ReservedResourcesProperty {
			ReservedResourceAmounts,
			HumanName,
			Resources
		}

		public enum InventoryProperty {
			Inventory,
			MaxWeight,
			MaxVolume,
			Resources,
			ReservedResources,
			HumanName,
			ContainerPosition
		}

		public void WriteInventoryLines(StreamWriter file, ResourceManager.Inventory inventory, int startLevel) {
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

		public PersistenceInventory LoadPersistenceInventory(List<KeyValuePair<string, object>> properties) {
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

		public void WriteResourceAmountLines(StreamWriter file, ResourceManager.ResourceAmount resourceAmount, int startLevel) {
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

	}
}
