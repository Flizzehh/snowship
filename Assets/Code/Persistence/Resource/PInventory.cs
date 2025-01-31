using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PInventory : Inventory
	{

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

		public void WriteInventoryLines(StreamWriter file, Inventory inventory, int startLevel) {
			file.WriteLine(PU.CreateKeyValueString(InventoryProperty.Inventory, string.Empty, startLevel));
			file.WriteLine(PU.CreateKeyValueString(InventoryProperty.MaxWeight, inventory.maxWeight, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(InventoryProperty.MaxVolume, inventory.maxVolume, startLevel + 1));
			if (inventory.parent is Human human) {
				file.WriteLine(PU.CreateKeyValueString(InventoryProperty.HumanName, human.Name, startLevel + 1));
			} else if (inventory.parent is Container container) {
				file.WriteLine(PU.CreateKeyValueString(InventoryProperty.ContainerPosition, PU.FormatVector2ToString(container.zeroPointTile.obj.transform.position), startLevel + 1));
			}
			if (inventory.resources.Count > 0) {
				file.WriteLine(PU.CreateKeyValueString(InventoryProperty.Resources, string.Empty, startLevel + 1));
				foreach (ResourceAmount resourceAmount in inventory.resources) {
					WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}
			if (inventory.reservedResources.Count > 0) {
				file.WriteLine(PU.CreateKeyValueString(InventoryProperty.ReservedResources, string.Empty, startLevel + 1));
				foreach (ReservedResources reservedResources in inventory.reservedResources) {
					file.WriteLine(PU.CreateKeyValueString(ReservedResourcesProperty.ReservedResourceAmounts, string.Empty, startLevel + 2));
					file.WriteLine(PU.CreateKeyValueString(ReservedResourcesProperty.HumanName, reservedResources.human.Name, startLevel + 3));
					file.WriteLine(PU.CreateKeyValueString(ReservedResourcesProperty.Resources, string.Empty, startLevel + 3));
					foreach (ResourceAmount resourceAmount in reservedResources.resources) {
						WriteResourceAmountLines(file, resourceAmount, startLevel + 4);
					}
				}
			}
		}

		public class PersistenceInventory {
			public int? maxWeight;
			public int? maxVolume;
			public List<ResourceAmount> resources;
			public List<KeyValuePair<string, List<ResourceAmount>>> reservedResources;
			public string humanName;
			public Vector2? containerZeroPointTilePosition;

			public PersistenceInventory(
				int? maxWeight,
				int? maxVolume,
				List<ResourceAmount> resources,
				List<KeyValuePair<string, List<ResourceAmount>>> reservedResources,
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
			List<ResourceAmount> resources = new();
			List<KeyValuePair<string, List<ResourceAmount>>> reservedResources = new();
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
									List<ResourceAmount> resourcesToReserve = new();

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

									reservedResources.Add(new KeyValuePair<string, List<ResourceAmount>>(reservingHumanName, resourcesToReserve));

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

		public void WriteResourceAmountLines(StreamWriter file, ResourceAmount resourceAmount, int startLevel) {
			file.WriteLine(PU.CreateKeyValueString(ResourceAmountProperty.ResourceAmount, string.Empty, startLevel));
			file.WriteLine(PU.CreateKeyValueString(ResourceAmountProperty.Type, resourceAmount.Resource.type, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(ResourceAmountProperty.Amount, resourceAmount.Amount, startLevel + 1));
		}

		public ResourceAmount LoadResourceAmount(List<KeyValuePair<string, object>> properties) {
			Resource resource = null;
			int? amount = null;

			foreach (KeyValuePair<string, object> resourceAmountProperty in properties) {
				ResourceAmountProperty resourceAmountPropertyKey = (ResourceAmountProperty)Enum.Parse(typeof(ResourceAmountProperty), resourceAmountProperty.Key);
				switch (resourceAmountPropertyKey) {
					case ResourceAmountProperty.Type:
						resource = Resource.GetResourceByEnum((EResource)Enum.Parse(typeof(EResource), (string)resourceAmountProperty.Value));
						break;
					case ResourceAmountProperty.Amount:
						amount = int.Parse((string)resourceAmountProperty.Value);
						break;
					default:
						Debug.LogError("Unknown resource amount property: " + resourceAmountProperty.Key + " " + resourceAmountProperty.Value);
						break;
				}
			}

			return new ResourceAmount(resource, amount.Value);
		}

	}
}