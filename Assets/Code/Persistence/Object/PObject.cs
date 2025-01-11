using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PObject : PersistenceHandler {

		private readonly PInventory pInventory = new PInventory();

		public enum ObjectProperty {
			Object,
			Type,
			Variation,
			Position,
			RotationIndex,
			Integrity,
			Active,
			Container,
			CraftingObject,
			Farm,
			SleepSpot
		}

		public enum ContainerProperty {
			Inventory
		}

		public enum CraftingObjectProperty {
			Resources,
			Fuels
		}

		public enum CraftableResourceProperty {
			CraftableResource,
			Resource,
			Priority,
			CreationMethod,
			TargetAmount,
			RemainingAmount,
			Enableable,
			FuelAmounts,
			Job
		}

		public enum PriorityResourceProperty {
			PriorityResource,
			Resource,
			Priority
		}

		public enum FarmProperty {
			GrowTimer
		}

		public enum SleepSpotProperty {
			OccupyingColonistName
		}

		public void SaveObjects(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "objects.snowship");

			foreach (List<ResourceManager.ObjectInstance> instances in GameManager.resourceM.objectInstances.Values) {
				foreach (ResourceManager.ObjectInstance instance in instances) {
					file.WriteLine(CreateKeyValueString(ObjectProperty.Object, string.Empty, 0));

					file.WriteLine(CreateKeyValueString(ObjectProperty.Type, instance.prefab.type, 1));
					file.WriteLine(CreateKeyValueString(ObjectProperty.Variation, instance.variation == null ? "null" : instance.variation.name, 1));
					file.WriteLine(CreateKeyValueString(ObjectProperty.Position, FormatVector2ToString(instance.zeroPointTile.obj.transform.position), 1));
					file.WriteLine(CreateKeyValueString(ObjectProperty.RotationIndex, instance.rotationIndex, 1));
					file.WriteLine(CreateKeyValueString(ObjectProperty.Integrity, instance.integrity, 1));
					file.WriteLine(CreateKeyValueString(ObjectProperty.Active, instance.active, 1));

					if (instance is ResourceManager.Container container) {
						file.WriteLine(CreateKeyValueString(ObjectProperty.Container, string.Empty, 1));
						pInventory.WriteInventoryLines(file, container.GetInventory(), 2);
					} else if (instance is ResourceManager.CraftingObject craftingObject) {
						if (craftingObject.resources.Count > 0 || craftingObject.fuels.Count > 0) {
							file.WriteLine(CreateKeyValueString(ObjectProperty.CraftingObject, string.Empty, 1));
							if (craftingObject.resources.Count > 0) {
								file.WriteLine(CreateKeyValueString(CraftingObjectProperty.Resources, string.Empty, 2));
								foreach (ResourceManager.CraftableResourceInstance resource in craftingObject.resources) {
									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.CraftableResource, string.Empty, 3));

									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.Resource, resource.resource.type, 4));
									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.Priority, resource.priority.Get(), 4));
									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.CreationMethod, resource.creationMethod, 4));
									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.TargetAmount, resource.GetTargetAmount(), 4));
									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.RemainingAmount, resource.GetRemainingAmount(), 4));

									file.WriteLine(CreateKeyValueString(CraftableResourceProperty.Enableable, resource.enableable, 4));
									if (resource.fuelAmounts.Count > 0) {
										file.WriteLine(CreateKeyValueString(CraftableResourceProperty.FuelAmounts, string.Empty, 4));
										foreach (ResourceManager.ResourceAmount fuelAmount in resource.fuelAmounts) {
											pInventory.WriteResourceAmountLines(file, fuelAmount, 5);
										}
									}
								}
							}
							if (craftingObject.fuels.Count > 0) {
								file.WriteLine(CreateKeyValueString(CraftingObjectProperty.Fuels, string.Empty, 2));
								foreach (ResourceManager.PriorityResourceInstance fuel in craftingObject.fuels) {
									file.WriteLine(CreateKeyValueString(PriorityResourceProperty.PriorityResource, string.Empty, 3));

									file.WriteLine(CreateKeyValueString(PriorityResourceProperty.Resource, fuel.resource.type, 4));
									file.WriteLine(CreateKeyValueString(PriorityResourceProperty.Priority, fuel.priority.Get(), 4));
								}
							}
						}
					} else if (instance is ResourceManager.Farm farm) {
						file.WriteLine(CreateKeyValueString(ObjectProperty.Farm, string.Empty, 1));
						file.WriteLine(CreateKeyValueString(FarmProperty.GrowTimer, farm.growTimer, 2));
					} else if (instance is ResourceManager.SleepSpot sleepSpot) {
						file.WriteLine(CreateKeyValueString(ObjectProperty.SleepSpot, string.Empty, 1));
						file.WriteLine(CreateKeyValueString(SleepSpotProperty.OccupyingColonistName, sleepSpot.occupyingColonist.name, 2));
					}
				}
			}

			file.Close();
		}

		public class PersistenceObject {
			public ResourceManager.ObjectEnum? type;
			public string variation;
			public Vector2? zeroPointTilePosition;
			public int? rotationIndex;
			public float? integrity;
			public bool? active;

			// Container
			public PInventory.PersistenceInventory persistenceInventory;

			// Crafting Object
			public List<PersistenceCraftableResourceInstance> persistenceResources;
			public List<ResourceManager.PriorityResourceInstance> fuels;

			// Farm
			public ResourceManager.Resource seedResource;
			public float? growTimer;

			// Sleep Spot
			public string occupyingColonistName;

			public PersistenceObject(
				ResourceManager.ObjectEnum? type,
				string variation,
				Vector2? zeroPointTilePosition,
				int? rotationIndex,
				float? integrity,
				bool? active,
				PInventory.PersistenceInventory persistenceInventory,
				List<PersistenceCraftableResourceInstance> persistenceResources,
				List<ResourceManager.PriorityResourceInstance> fuels,
				ResourceManager.Resource seedResource,
				float? growTimer,
				string occupyingColonistName
			) {
				this.type = type;
				this.variation = variation;
				this.zeroPointTilePosition = zeroPointTilePosition;
				this.rotationIndex = rotationIndex;
				this.integrity = integrity;
				this.active = active;

				this.persistenceInventory = persistenceInventory;

				this.persistenceResources = persistenceResources;
				this.fuels = fuels;

				this.seedResource = seedResource;
				this.growTimer = growTimer;

				this.occupyingColonistName = occupyingColonistName;
			}
		}

		public class PersistenceCraftableResourceInstance {
			public ResourceManager.Resource resource;
			public int? priority;
			public ResourceManager.CreationMethod? creationMethod;
			public int? targetAmount;
			public int? remainingAmount;
			public bool? enableable;
			public List<ResourceManager.ResourceAmount> fuelAmounts;

			public PersistenceCraftableResourceInstance(
				ResourceManager.Resource resource,
				int? priority,
				ResourceManager.CreationMethod? creationMethod,
				int? targetAmount,
				int? remainingAmount,
				bool? enableable,
				List<ResourceManager.ResourceAmount> fuelAmounts
			) {
				this.resource = resource;
				this.priority = priority;
				this.creationMethod = creationMethod;
				this.targetAmount = targetAmount;
				this.remainingAmount = remainingAmount;
				this.enableable = enableable;
				this.fuelAmounts = fuelAmounts;
			}
		}

		public List<PersistenceObject> LoadObjects(string path) {
			List<PersistenceObject> persistenceObjects = new List<PersistenceObject>();

			List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), property.Key)) {
					case ObjectProperty.Object:

						ResourceManager.ObjectEnum? type = null;
						string variation = null;
						Vector2? zeroPointTilePosition = null;
						int? rotationIndex = null;
						float? integrity = null;
						bool? active = null;

						// Container
						PInventory.PersistenceInventory persistenceInventory = null;

						// Crafting Object
						List<PersistenceCraftableResourceInstance> persistenceResources = new List<PersistenceCraftableResourceInstance>();
						List<ResourceManager.PriorityResourceInstance> fuels = new List<ResourceManager.PriorityResourceInstance>();

						// Farm
						ResourceManager.Resource seedResource = null;
						float? growTimer = null;

						// Sleep Spot
						string occupyingColonistName = null;

						foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)property.Value) {
							switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), objectProperty.Key)) {
								case ObjectProperty.Type:
									type = (ResourceManager.ObjectEnum)Enum.Parse(typeof(ResourceManager.ObjectEnum), (string)objectProperty.Value);
									break;
								case ObjectProperty.Variation:
									variation = StringUtilities.RemoveNonAlphanumericChars((string)objectProperty.Value);
									break;
								case ObjectProperty.Position:
									zeroPointTilePosition = new Vector2(float.Parse(((string)objectProperty.Value).Split(',')[0]), float.Parse(((string)objectProperty.Value).Split(',')[1]));
									break;
								case ObjectProperty.RotationIndex:
									rotationIndex = int.Parse((string)objectProperty.Value);
									break;
								case ObjectProperty.Integrity:
									integrity = float.Parse((string)objectProperty.Value);
									break;
								case ObjectProperty.Active:
									active = bool.Parse((string)objectProperty.Value);
									break;
								case ObjectProperty.Container:
									foreach (KeyValuePair<string, object> containerProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
										switch ((ContainerProperty)Enum.Parse(typeof(ContainerProperty), containerProperty.Key)) {
											case ContainerProperty.Inventory:
												persistenceInventory = pInventory.LoadPersistenceInventory((List<KeyValuePair<string, object>>)containerProperty.Value);
												break;
											default:
												Debug.LogError("Unknown container property: " + containerProperty.Key + " " + containerProperty.Value);
												break;
										}
									}
									break;
								case ObjectProperty.CraftingObject:
									foreach (KeyValuePair<string, object> craftingObjectProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
										switch ((CraftingObjectProperty)Enum.Parse(typeof(CraftingObjectProperty), craftingObjectProperty.Key)) {
											case CraftingObjectProperty.Resources:
												foreach (KeyValuePair<string, object> resourcesProperty in (List<KeyValuePair<string, object>>)craftingObjectProperty.Value) {
													switch ((CraftableResourceProperty)Enum.Parse(typeof(CraftableResourceProperty), resourcesProperty.Key)) {
														case CraftableResourceProperty.CraftableResource:

															ResourceManager.Resource resource = null;
															int? priority = null;
															ResourceManager.CreationMethod? creationMethod = null;
															int? targetAmount = null;
															int? remainingAmount = null;
															bool? enableable = null;
															List<ResourceManager.ResourceAmount> fuelAmounts = new List<ResourceManager.ResourceAmount>();

															foreach (KeyValuePair<string, object> craftableResourceProperty in (List<KeyValuePair<string, object>>)resourcesProperty.Value) {
																switch ((CraftableResourceProperty)Enum.Parse(typeof(CraftableResourceProperty), craftableResourceProperty.Key)) {
																	case CraftableResourceProperty.Resource:
																		resource = GameManager.resourceM.GetResourceByString((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.Priority:
																		priority = int.Parse((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.CreationMethod:
																		creationMethod = (ResourceManager.CreationMethod)Enum.Parse(typeof(ResourceManager.CreationMethod), (string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.TargetAmount:
																		targetAmount = int.Parse((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.RemainingAmount:
																		remainingAmount = int.Parse((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.Enableable:
																		enableable = bool.Parse((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.FuelAmounts:
																		foreach (KeyValuePair<string, object> fuelAmountsProperty in (List<KeyValuePair<string, object>>)craftableResourceProperty.Value) {
																			fuelAmounts.Add(pInventory.LoadResourceAmount((List<KeyValuePair<string, object>>)fuelAmountsProperty.Value));
																		}
																		break;
																}
															}

															persistenceResources.Add(
																new PersistenceCraftableResourceInstance(
																	resource,
																	priority,
																	creationMethod,
																	targetAmount,
																	remainingAmount,
																	enableable,
																	fuelAmounts
																)
															);

															break;
													}
												}
												break;
											case CraftingObjectProperty.Fuels:
												foreach (KeyValuePair<string, object> fuelsProperty in (List<KeyValuePair<string, object>>)craftingObjectProperty.Value) {
													switch ((PriorityResourceProperty)Enum.Parse(typeof(PriorityResourceProperty), fuelsProperty.Key)) {
														case PriorityResourceProperty.PriorityResource:

															ResourceManager.Resource resource = null;
															int? priority = null;

															foreach (KeyValuePair<string, object> priorityResourceProperty in (List<KeyValuePair<string, object>>)fuelsProperty.Value) {
																switch ((PriorityResourceProperty)Enum.Parse(typeof(PriorityResourceProperty), priorityResourceProperty.Key)) {
																	case PriorityResourceProperty.Resource:
																		resource = GameManager.resourceM.GetResourceByString((string)priorityResourceProperty.Value);
																		break;
																	case PriorityResourceProperty.Priority:
																		priority = int.Parse((string)priorityResourceProperty.Value);
																		break;
																}
															}

															fuels.Add(new ResourceManager.PriorityResourceInstance(resource, priority.Value));

															break;
													}
												}
												break;
											default:
												Debug.LogError("Unknown crafting tile object property: " + craftingObjectProperty.Key + " " + craftingObjectProperty.Value);
												break;
										}
									}
									break;
								case ObjectProperty.Farm:
									foreach (KeyValuePair<string, object> farmProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
										switch ((FarmProperty)Enum.Parse(typeof(FarmProperty), farmProperty.Key)) {
											case FarmProperty.GrowTimer:
												growTimer = float.Parse((string)farmProperty.Value);
												break;
											default:
												Debug.LogError("Unknown farm property: " + farmProperty.Key + " " + farmProperty.Value);
												break;
										}
									}
									break;
								case ObjectProperty.SleepSpot:
									foreach (KeyValuePair<string, object> sleepSpotProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
										switch ((SleepSpotProperty)Enum.Parse(typeof(SleepSpotProperty), sleepSpotProperty.Key)) {
											case SleepSpotProperty.OccupyingColonistName:
												occupyingColonistName = (string)sleepSpotProperty.Value;
												break;
											default:
												Debug.LogError("Unknown sleep spot property: " + sleepSpotProperty.Key + " " + sleepSpotProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown tile object property: " + objectProperty.Key + " " + objectProperty.Value);
									break;
							}
						}

						persistenceObjects.Add(
							new PersistenceObject(
								type,
								variation,
								zeroPointTilePosition,
								rotationIndex,
								integrity,
								active,
								persistenceInventory,
								persistenceResources,
								fuels,
								seedResource,
								growTimer,
								occupyingColonistName
							));
						break;
					default:
						Debug.LogError("Unknown tile object property: " + property.Key + " " + property.Value);
						break;
				}
			}

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedObjects;
			return persistenceObjects;
		}

		public void ApplyLoadedObjects(List<PersistenceObject> persistenceObjects) {
			foreach (PersistenceObject persistenceObject in persistenceObjects) {
				TileManager.Tile zeroPointTile = GameManager.colonyM.colony.map.GetTileFromPosition(persistenceObject.zeroPointTilePosition.Value);

				ResourceManager.ObjectPrefab objectPrefab = GameManager.resourceM.GetObjectPrefabByEnum(persistenceObject.type.Value);
				ResourceManager.ObjectInstance objectInstance = GameManager.resourceM.CreateObjectInstance(
					objectPrefab,
					objectPrefab.GetVariationFromString(persistenceObject.variation),
					zeroPointTile,
					persistenceObject.rotationIndex.Value,
					true
				);
				objectInstance.integrity = persistenceObject.integrity.Value;
				objectInstance.SetActive(persistenceObject.active.Value);

				switch (objectPrefab.instanceType) {
					case ResourceManager.ObjectInstanceType.Container:
					case ResourceManager.ObjectInstanceType.TradingPost:
						ResourceManager.Container container = (ResourceManager.Container)objectInstance;
						container.GetInventory().maxWeight = persistenceObject.persistenceInventory.maxWeight.Value;
						container.GetInventory().maxVolume = persistenceObject.persistenceInventory.maxVolume.Value;
						foreach (ResourceManager.ResourceAmount resourceAmount in persistenceObject.persistenceInventory.resources) {
							container.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
						}
						// TODO (maybe already done?) Reserved resources must be set after colonists are loaded
						break;
					case ResourceManager.ObjectInstanceType.Farm:
						ResourceManager.Farm farm = (ResourceManager.Farm)objectInstance;
						farm.growTimer = persistenceObject.growTimer.Value;
						break;
					case ResourceManager.ObjectInstanceType.CraftingObject:
						ResourceManager.CraftingObject craftingObject = (ResourceManager.CraftingObject)objectInstance;
						craftingObject.SetActive(false); // Gets set to proper state after loading jobBacklog, this prevents it from creating a CreateResource job before then
						foreach (PersistenceCraftableResourceInstance persistenceResource in persistenceObject.persistenceResources) {
							craftingObject.resources.Add(
								new ResourceManager.CraftableResourceInstance(
									persistenceResource.resource,
									persistenceResource.priority.Value,
									persistenceResource.creationMethod.Value,
									persistenceResource.targetAmount.Value,
									craftingObject,
									persistenceResource.remainingAmount
								) { enableable = persistenceResource.enableable.Value, fuelAmounts = persistenceResource.fuelAmounts }
							);
						}
						craftingObject.fuels = persistenceObject.fuels;
						break;
					case ResourceManager.ObjectInstanceType.SleepSpot:
						// Occupying colonist must be set after colonists are loaded
						break;
				}

				zeroPointTile.SetObject(objectInstance);
				objectInstance.obj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				objectInstance.FinishCreation();
				if (objectInstance.prefab.canRotate) {
					objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[objectInstance.rotationIndex];
				}
			}
		}

	}
}
