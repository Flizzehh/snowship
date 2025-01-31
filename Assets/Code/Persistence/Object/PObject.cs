using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PObject
	{

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
			Bed
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

		public enum BedProperty
		{
			OccupyingColonistName
		}

		public void SaveObjects(string saveDirectoryPath) {

			StreamWriter file = PU.CreateFileAtDirectory(saveDirectoryPath, "objects.snowship");

			foreach (List<ObjectInstance> instances in ObjectInstance.ObjectInstances.Values) {
				foreach (ObjectInstance instance in instances) {
					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Object, string.Empty, 0));

					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Type, instance.prefab.type, 1));
					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Variation, instance.variation == null ? "null" : instance.variation.name, 1));
					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Position, PU.FormatVector2ToString(instance.zeroPointTile.obj.transform.position), 1));
					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.RotationIndex, instance.rotationIndex, 1));
					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Integrity, instance.integrity, 1));
					file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Active, instance.active, 1));

					if (instance is Container container) {
						file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Container, string.Empty, 1));
						pInventory.WriteInventoryLines(file, container.Inventory, 2);
					} else if (instance is CraftingObject craftingObject) {
						if (craftingObject.resources.Count > 0 || craftingObject.fuels.Count > 0) {
							file.WriteLine(PU.CreateKeyValueString(ObjectProperty.CraftingObject, string.Empty, 1));
							if (craftingObject.resources.Count > 0) {
								file.WriteLine(PU.CreateKeyValueString(CraftingObjectProperty.Resources, string.Empty, 2));
								foreach (CraftableResourceInstance resource in craftingObject.resources) {
									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.CraftableResource, string.Empty, 3));

									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.Resource, resource.resource.type, 4));
									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.Priority, resource.priority.Get(), 4));
									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.CreationMethod, resource.creationMethod, 4));
									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.TargetAmount, resource.GetTargetAmount(), 4));
									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.RemainingAmount, resource.GetRemainingAmount(), 4));

									file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.Enableable, resource.enableable, 4));
									if (resource.fuelAmounts.Count > 0) {
										file.WriteLine(PU.CreateKeyValueString(CraftableResourceProperty.FuelAmounts, string.Empty, 4));
										foreach (ResourceAmount fuelAmount in resource.fuelAmounts) {
											pInventory.WriteResourceAmountLines(file, fuelAmount, 5);
										}
									}
								}
							}
							if (craftingObject.fuels.Count > 0) {
								file.WriteLine(PU.CreateKeyValueString(CraftingObjectProperty.Fuels, string.Empty, 2));
								foreach (PriorityResourceInstance fuel in craftingObject.fuels) {
									file.WriteLine(PU.CreateKeyValueString(PriorityResourceProperty.PriorityResource, string.Empty, 3));

									file.WriteLine(PU.CreateKeyValueString(PriorityResourceProperty.Resource, fuel.resource.type, 4));
									file.WriteLine(PU.CreateKeyValueString(PriorityResourceProperty.Priority, fuel.priority.Get(), 4));
								}
							}
						}
					} else if (instance is Farm farm) {
						file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Farm, string.Empty, 1));
						file.WriteLine(PU.CreateKeyValueString(FarmProperty.GrowTimer, farm.growTimer, 2));
					} else if (instance is Bed bed) {
						file.WriteLine(PU.CreateKeyValueString(ObjectProperty.Bed, string.Empty, 1));
						file.WriteLine(PU.CreateKeyValueString(BedProperty.OccupyingColonistName, bed.Occupant.Name, 2));
					}
				}
			}

			file.Close();
		}

		public class PersistenceObject {
			public ObjectPrefab.ObjectEnum? type;
			public string variation;
			public Vector2? zeroPointTilePosition;
			public int? rotationIndex;
			public float? integrity;
			public bool? active;

			// Container
			public PInventory.PersistenceInventory persistenceInventory;

			// Crafting Object
			public List<PersistenceCraftableResourceInstance> persistenceResources;
			public List<PriorityResourceInstance> fuels;

			// Farm
			public Resource seedResource;
			public float? growTimer;

			// Sleep Spot
			public string occupyingColonistName;

			public PersistenceObject(
				ObjectPrefab.ObjectEnum? type,
				string variation,
				Vector2? zeroPointTilePosition,
				int? rotationIndex,
				float? integrity,
				bool? active,
				PInventory.PersistenceInventory persistenceInventory,
				List<PersistenceCraftableResourceInstance> persistenceResources,
				List<PriorityResourceInstance> fuels,
				Resource seedResource,
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
			public Resource resource;
			public int? priority;
			public CraftableResourceInstance.CreationMethod? creationMethod;
			public int? targetAmount;
			public int? remainingAmount;
			public bool? enableable;
			public List<ResourceAmount> fuelAmounts;

			public PersistenceCraftableResourceInstance(
				Resource resource,
				int? priority,
				CraftableResourceInstance.CreationMethod? creationMethod,
				int? targetAmount,
				int? remainingAmount,
				bool? enableable,
				List<ResourceAmount> fuelAmounts
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

			List<KeyValuePair<string, object>> properties = PU.GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), property.Key)) {
					case ObjectProperty.Object:

						ObjectPrefab.ObjectEnum? type = null;
						string variation = null;
						Vector2? zeroPointTilePosition = null;
						int? rotationIndex = null;
						float? integrity = null;
						bool? active = null;

						// Container
						PInventory.PersistenceInventory persistenceInventory = null;

						// Crafting Object
						List<PersistenceCraftableResourceInstance> persistenceResources = new List<PersistenceCraftableResourceInstance>();
						List<PriorityResourceInstance> fuels = new();

						// Farm
						Resource seedResource = null;
						float? growTimer = null;

						// Sleep Spot
						string occupyingColonistName = null;

						foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)property.Value) {
							switch ((ObjectProperty)Enum.Parse(typeof(ObjectProperty), objectProperty.Key)) {
								case ObjectProperty.Type:
									type = (ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), (string)objectProperty.Value);
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

															Resource resource = null;
															int? priority = null;
															CraftableResourceInstance.CreationMethod? creationMethod = null;
															int? targetAmount = null;
															int? remainingAmount = null;
															bool? enableable = null;
															List<ResourceAmount> fuelAmounts = new();

															foreach (KeyValuePair<string, object> craftableResourceProperty in (List<KeyValuePair<string, object>>)resourcesProperty.Value) {
																switch ((CraftableResourceProperty)Enum.Parse(typeof(CraftableResourceProperty), craftableResourceProperty.Key)) {
																	case CraftableResourceProperty.Resource:
																		resource = Resource.GetResourceByString((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.Priority:
																		priority = int.Parse((string)craftableResourceProperty.Value);
																		break;
																	case CraftableResourceProperty.CreationMethod:
																		creationMethod = (CraftableResourceInstance.CreationMethod)Enum.Parse(typeof(CraftableResourceInstance.CreationMethod), (string)craftableResourceProperty.Value);
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

															Resource resource = null;
															int? priority = null;

															foreach (KeyValuePair<string, object> priorityResourceProperty in (List<KeyValuePair<string, object>>)fuelsProperty.Value) {
																switch ((PriorityResourceProperty)Enum.Parse(typeof(PriorityResourceProperty), priorityResourceProperty.Key)) {
																	case PriorityResourceProperty.Resource:
																		resource = Resource.GetResourceByString((string)priorityResourceProperty.Value);
																		break;
																	case PriorityResourceProperty.Priority:
																		priority = int.Parse((string)priorityResourceProperty.Value);
																		break;
																}
															}

															fuels.Add(new PriorityResourceInstance(resource, priority.Value));

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
								case ObjectProperty.Bed:
									foreach (KeyValuePair<string, object> bedProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
										switch ((BedProperty)Enum.Parse(typeof(BedProperty), bedProperty.Key)) {
											case BedProperty.OccupyingColonistName:
												occupyingColonistName = (string)bedProperty.Value;
												break;
											default:
												Debug.LogError("Unknown sleep spot property: " + bedProperty.Key + " " + bedProperty.Value);
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

			GameManager.Get<PersistenceManager>().loadingState = PersistenceManager.LoadingState.LoadedObjects;
			return persistenceObjects;
		}

		public void ApplyLoadedObjects(List<PersistenceObject> persistenceObjects) {
			foreach (PersistenceObject persistenceObject in persistenceObjects) {
				TileManager.Tile zeroPointTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(persistenceObject.zeroPointTilePosition.Value);

				ObjectPrefab objectPrefab = ObjectPrefab.GetObjectPrefabByEnum(persistenceObject.type.Value);
				ObjectInstance objectInstance = ObjectInstance.CreateObjectInstance(
					objectPrefab,
					objectPrefab.GetVariationFromString(persistenceObject.variation),
					zeroPointTile,
					persistenceObject.rotationIndex.Value,
					true
				);
				objectInstance.integrity = persistenceObject.integrity.Value;
				objectInstance.SetActive(persistenceObject.active.Value);

				switch (objectPrefab.instanceType) {
					case ObjectInstance.ObjectInstanceType.Container:
					case ObjectInstance.ObjectInstanceType.TradingPost:
						Container container = (Container)objectInstance;
						container.Inventory.maxWeight = persistenceObject.persistenceInventory.maxWeight.Value;
						container.Inventory.maxVolume = persistenceObject.persistenceInventory.maxVolume.Value;
						foreach (ResourceAmount resourceAmount in persistenceObject.persistenceInventory.resources) {
							container.Inventory.ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
						}
						// TODO (maybe already done?) Reserved resources must be set after colonists are loaded
						break;
					case ObjectInstance.ObjectInstanceType.Farm:
						Farm farm = (Farm)objectInstance;
						farm.growTimer = persistenceObject.growTimer.Value;
						break;
					case ObjectInstance.ObjectInstanceType.CraftingObject:
						CraftingObject craftingObject = (CraftingObject)objectInstance;
						craftingObject.SetActive(false); // Gets set to proper state after loading jobBacklog, this prevents it from creating a CreateResource job before then
						foreach (PersistenceCraftableResourceInstance persistenceResource in persistenceObject.persistenceResources) {
							craftingObject.resources.Add(
								new CraftableResourceInstance(
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
					case ObjectInstance.ObjectInstanceType.Bed:
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