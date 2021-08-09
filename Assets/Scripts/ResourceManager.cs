using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : BaseManager {

	public GameObject tilePrefab;
	public GameObject objectPrefab;
	public GameObject humanPrefab;
	public Sprite selectionCornersSprite;
	public Sprite whiteSquareSprite;
	public Sprite clearSquareSprite;
	public GameObject planetTilePrefab;
	public GameObject colonyObj;
	public GameObject tileImage;
	public GameObject objectDataPanel;

	public void SetResourceReferences() {
		tilePrefab = Resources.Load<GameObject>(@"Prefabs/Tile");
		objectPrefab = Resources.Load<GameObject>(@"Prefabs/Object");
		humanPrefab = Resources.Load<GameObject>(@"Prefabs/Human");
		selectionCornersSprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		whiteSquareSprite = Resources.Load<Sprite>(@"UI/white-square");
		clearSquareSprite = Resources.Load<Sprite>(@"UI/clear-square");
		planetTilePrefab = Resources.Load<GameObject>(@"UI/UIElements/PlanetTile");
		colonyObj = Resources.Load<GameObject>(@"UI/UIElements/ColonyObj");
		tileImage = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage");
		objectDataPanel = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel");
	}

	public GameObject tileParent;
	public GameObject colonistParent;
	public GameObject traderParent;
	public GameObject selectionParent;
	public GameObject jobParent;

	public void SetGameObjectReferences() {
		tileParent = GameObject.Find("TileParent");
		colonistParent = GameObject.Find("ColonistParent");
		traderParent = GameObject.Find("TraderParent");
		selectionParent = GameObject.Find("SelectionParent");
		jobParent = GameObject.Find("JobParent");
	}

	public override void Update() {
		CalculateResourceTotals();

		GrowPlants();

		foreach (Farm farm in farms) {
			farm.Update();
		}
		foreach (CraftingObject craftingObject in craftingObjectInstances) {
			craftingObject.Update();
		}
	}

	public enum ResourceGroupPropertyEnum {
		ResourceGroup,
		Type,
		Resources
	}

	public enum ResourcePropertyEnum {
		Resource,
		Type,
		Classes,
		Weight,
		Volume,
		Price,
		Food,
		Fuel,
		Crafting,
		Clothing
	}

	public enum ResourceFoodPropertyEnum {
		Nutrition
	}

	public enum ResourceFuelPropertyEnum {
		FuelEnergy
	}

	public enum ResourceCraftingPropertyEnum {
		Objects,
		CraftingEnergy,
		CraftingTime,
		Resources
	}

	public enum ResourceClothingPropertyEnum {
		Appearance,
		ClothingType,
		Insulation,
		WaterResistance,
		WeightCapacity,
		VolumeCapacity,
		Colours
	}

	public enum ResourceGroupEnum {
		None,
		NaturalResources,
		Ores,
		Metals,
		Materials,
		Seeds,
		RawFoods,
		Foods,
		Coins,
		Clothing
	}

	public enum ResourceEnum {
		None,
		Dirt, Mud, Andesite, Basalt, Diorite, Granite, Kimberlite, Obsidian, Rhyolite, Chalk, Claystone, Coal, Flint, Lignite, Limestone, Sandstone, Anthracite, Marble, Quartz, Slate,
			Clay, Log, Snow, Sand, Cactus, Leaf, Grass, Sap,
		GoldOre, SilverOre, BronzeOre, IronOre, CopperOre,
		Gold, Silver, Bronze, Iron, Copper,
		Wood, Firewood, Charcoal, Mudbrick, Brick, Glass, Cotton, Cloth,
		WheatSeed, CottonSeed, TreeSeed, AppleSeed, BushSeed, CactusSeed,
		Wheat,
		Potato, BakedPotato, Blueberry, Apple, BakedApple,
		GoldCoin, SilverCoin, BronzeCoin,
		CyanSweater, GreenSweater, BlueSweater, PurpleSweater, NavySweater, YellowSweater, OrangeSweater, RedSweater, WhiteSweater, GraySweater,
		CyanShirt, GreenShirt, BlueShirt, PurpleShirt, NavyShirt, YellowShirt, OrangeShirt, RedShirt, WhiteShirt, GrayShirt,
		BrownSmallBackpack
	}

	public enum ResourceClassEnum {
		Food,
		Craftable,
		Fuel,
		Clothing,
		Bag
	}

	public enum ClothingEnum {
		Sweater, Shirt, SmallBackpack
	}

	public void CreateResources() {

		Dictionary<ResourceEnum, List<KeyValuePair<ResourceEnum, float>>> craftingResourcesTemp = new Dictionary<ResourceEnum, List<KeyValuePair<ResourceEnum, float>>>();

		List<KeyValuePair<string, object>> resourceGroupProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/resources").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> resourceGroupProperty in resourceGroupProperties) {
			switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupProperty.Key)) {
				case ResourceGroupPropertyEnum.ResourceGroup:

					ResourceGroupEnum? groupType = null;
					List<Resource> resources = new List<Resource>();

					foreach (KeyValuePair<string, object> resourceGroupSubProperty in (List<KeyValuePair<string, object>>)resourceGroupProperty.Value) {
						switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupSubProperty.Key)) {
							case ResourceGroupPropertyEnum.Type:
								groupType = (ResourceGroupEnum)Enum.Parse(typeof(ResourceGroupEnum), (string)resourceGroupSubProperty.Value);
								break;
							case ResourceGroupPropertyEnum.Resources:
								foreach (KeyValuePair<string, object> resourceProperty in (List<KeyValuePair<string, object>>)resourceGroupSubProperty.Value) {
									switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceProperty.Key)) {
										case ResourcePropertyEnum.Resource:

											ResourceEnum? type = null;
											List<ResourceClassEnum> classes = new List<ResourceClassEnum>();
											int? weight = null;
											int? volume = null;
											int? price = null;

											// Food
											int? foodNutrition = null;

											// Fuel
											int? fuelEnergy = 0;

											// Crafting
											Dictionary<ObjectSubGroupEnum, List<ObjectEnum>> craftingObjects = new Dictionary<ObjectSubGroupEnum, List<ObjectEnum>>();
											int? craftingEnergy = 0;
											int? craftingTime = 0;
											// craftingResources -> craftingResourcesTemp

											// Clothing
											HumanManager.Human.Appearance? clothingAppearance = null;
											ClothingEnum? clothingType = null;
											int? clothingInsulation = null;
											int? clothingWaterResistance = null;
											int? clothingWeightCapacity = 0;
											int? clothingVolumeCapacity = 0;
											List<string> clothingColours = new List<string>();

											foreach (KeyValuePair<string, object> resourceSubProperty in (List<KeyValuePair<string, object>>)resourceProperty.Value) {
												switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceSubProperty.Key)) {
													case ResourcePropertyEnum.Type:
														type = (ResourceEnum)Enum.Parse(typeof(ResourceEnum), (string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Classes:
														foreach (string classString in ((string)resourceSubProperty.Value).Split(',')) {
															classes.Add((ResourceClassEnum)Enum.Parse(typeof(ResourceClassEnum), classString));
														}
														break;
													case ResourcePropertyEnum.Weight:
														weight = int.Parse((string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Volume:
														volume = int.Parse((string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Price:
														price = int.Parse((string)resourceSubProperty.Value);
														break;
													case ResourcePropertyEnum.Food:
														foreach (KeyValuePair<string, object> foodProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceFoodPropertyEnum)Enum.Parse(typeof(ResourceFoodPropertyEnum), foodProperty.Key)) {
																case ResourceFoodPropertyEnum.Nutrition:
																	foodNutrition = int.Parse((string)foodProperty.Value);
																	break;
																default:
																	Debug.LogError("Unknown resource food property: " + foodProperty.Key + " " + foodProperty.Value);
																	break;
															}
														}
														break;
													case ResourcePropertyEnum.Fuel:
														foreach (KeyValuePair<string, object> fuelProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceFuelPropertyEnum)Enum.Parse(typeof(ResourceFuelPropertyEnum), fuelProperty.Key)) {
																case ResourceFuelPropertyEnum.FuelEnergy:
																	fuelEnergy = int.Parse((string)fuelProperty.Value);
																	break;
																default:
																	Debug.LogError("Unknown resource fuel property: " + fuelProperty.Key + " " + fuelProperty.Value);
																	break;
															}
														}
														break;
													case ResourcePropertyEnum.Crafting:
														foreach (KeyValuePair<string, object> craftingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceCraftingPropertyEnum)Enum.Parse(typeof(ResourceCraftingPropertyEnum), craftingProperty.Key)) {
																case ResourceCraftingPropertyEnum.Objects:
																	foreach (string objectSubGroupString in ((string)craftingProperty.Value).Split(';')) {
																		ObjectSubGroupEnum objectSubGroupEnum = (ObjectSubGroupEnum)Enum.Parse(typeof(ObjectSubGroupEnum), objectSubGroupString.Split(':')[0]);
																		craftingObjects.Add(
																			objectSubGroupEnum,
																			null
																		);
																		if (objectSubGroupString.Split(':').Count() > 1) {
																			craftingObjects[objectSubGroupEnum] = new List<ObjectEnum>();
																			foreach (string objectString in objectSubGroupString.Split(':')[1].Split(',')) {
																				craftingObjects[objectSubGroupEnum].Add((ObjectEnum)Enum.Parse(typeof(ObjectEnum), objectString));
																			}
																		}
																	}
																	break;
																case ResourceCraftingPropertyEnum.CraftingEnergy:
																	craftingEnergy = int.Parse((string)craftingProperty.Value);
																	break;
																case ResourceCraftingPropertyEnum.CraftingTime:
																	craftingTime = int.Parse((string)craftingProperty.Value);
																	break;
																case ResourceCraftingPropertyEnum.Resources:
																	craftingResourcesTemp.Add(type.Value, new List<KeyValuePair<ResourceEnum, float>>());
																	foreach (string resourceAmountString in ((string)craftingProperty.Value).Split(',')) {
																		float amount = float.Parse(resourceAmountString.Split(':')[1]);
																		craftingResourcesTemp[type.Value].Add(new KeyValuePair<ResourceEnum, float>(
																			(ResourceEnum)Enum.Parse(typeof(ResourceEnum), resourceAmountString.Split(':')[0]),
																			amount
																		));
																	}
																	break;
																default:
																	Debug.LogError("Unknown resource crafting property: " + craftingProperty.Key + " " + craftingProperty.Value);
																	break;
															}
														}
														break;
													case ResourcePropertyEnum.Clothing:
														foreach (KeyValuePair<string, object> clothingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
															switch ((ResourceClothingPropertyEnum)Enum.Parse(typeof(ResourceClothingPropertyEnum), clothingProperty.Key)) {
																case ResourceClothingPropertyEnum.Appearance:
																	clothingAppearance = (HumanManager.Human.Appearance)Enum.Parse(typeof(HumanManager.Human.Appearance), (string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.ClothingType:
																	clothingType = (ClothingEnum)Enum.Parse(typeof(ClothingEnum), (string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.Insulation:
																	clothingInsulation = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.WaterResistance:
																	clothingWaterResistance = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.WeightCapacity:
																	clothingWeightCapacity = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.VolumeCapacity:
																	clothingVolumeCapacity = int.Parse((string)clothingProperty.Value);
																	break;
																case ResourceClothingPropertyEnum.Colours:
																	foreach (string colourString in ((string)clothingProperty.Value).Split(',')) {
																		clothingColours.Add(UIManager.RemoveNonAlphanumericChars(colourString));
																	}
																	break;
																default:
																	Debug.LogError("Unknown resource clothing property: " + clothingProperty.Key + " " + clothingProperty.Value);
																	break;
															}
														}
														break;
													default:
														Debug.LogError("Unknown resource sub property: " + resourceSubProperty.Key + " " + resourceSubProperty.Value);
														break;
												}
											}

											if (classes.Contains(ResourceClassEnum.Food)) {
												Food food = new Food(
													type.Value,
													groupType.Value,
													classes,
													weight.Value,
													volume.Value,
													price.Value,
													fuelEnergy.Value,
													craftingObjects,
													craftingEnergy.Value,
													craftingTime.Value,
													foodNutrition.Value
												);
												resources.Add(food);
												this.resources.Add(type.Value, food);
											} else if (classes.Contains(ResourceClassEnum.Clothing)) {
												ClothingPrefab clothingPrefab = new ClothingPrefab(
													clothingAppearance.Value,
													clothingType.Value,
													clothingInsulation.Value,
													clothingWaterResistance.Value,
													clothingWeightCapacity.Value,
													clothingVolumeCapacity.Value,
													clothingColours,
													clothingPrefabs.Where(c => c.appearance == clothingAppearance.Value).Sum(c => c.colours.Count)
												);
												clothingPrefabs.Add(clothingPrefab);
												foreach (string colour in clothingPrefab.colours) {
													Clothing clothing = new Clothing(
														(ResourceEnum)Enum.Parse(typeof(ResourceEnum), colour + clothingType.Value),
														groupType.Value,
														classes,
														weight.Value,
														volume.Value,
														price.Value,
														fuelEnergy.Value,
														craftingObjects,
														craftingEnergy.Value,
														craftingTime.Value,
														clothingPrefab,
														colour
													);
													resources.Add(clothing);
													this.resources.Add(clothing.type, clothing);
												}
											} else {
												Resource resource = new Resource(
													type.Value,
													groupType.Value,
													classes,
													weight.Value,
													volume.Value,
													price.Value,
													fuelEnergy.Value,
													craftingObjects,
													craftingEnergy.Value,
													craftingTime.Value
												);
												resources.Add(resource);
												this.resources.Add(type.Value, resource);
											}

											break;
										default:
											Debug.LogError("Unknown resource property: " + resourceProperty.Key + " " + resourceProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown resource group sub property: " + resourceGroupSubProperty.Key + " " + resourceGroupSubProperty.Value);
								break;
						}

					}

					ResourceGroup resourceGroup = new ResourceGroup(
						groupType.Value,
						resources
					);
					resourceGroups.Add(groupType.Value, resourceGroup);

					break;
				default:
					Debug.LogError("Unknown resource group property: " + resourceGroupProperty.Key + " " + resourceGroupProperty.Value);
					break;

			}
		}

		// Set Resource Classes
		foreach (ResourceClassEnum resourceClassEnum in Enum.GetValues(typeof(ResourceClassEnum))) {
			resourceClassToResources.Add(resourceClassEnum, new List<Resource>());
		}

		foreach (Resource resource in GetResources()) {
			foreach (ResourceClassEnum resourceClassEnum in resource.classes) {
				resourceClassToResources[resourceClassEnum].Add(resource);
			}
		}

		// Set Crafting Resources
		foreach (KeyValuePair<ResourceEnum, List<KeyValuePair<ResourceEnum, float>>> craftingResourceToResourceAmount in craftingResourcesTemp) {
			List<Resource> resourcesToApplyTo = new List<Resource>();
			Resource craftableResource = GetResourceByEnum(craftingResourceToResourceAmount.Key);
			if (craftableResource.classes.Contains(ResourceClassEnum.Clothing)) {
				Clothing craftableClothing = (Clothing)craftableResource;
				foreach (Resource resource in GetResourcesInClass(ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.clothingType == craftableClothing.prefab.clothingType)) {
					resourcesToApplyTo.Add(resource);
				}
			} else {
				resourcesToApplyTo.Add(craftableResource);
			}
			foreach (Resource resource in resourcesToApplyTo) {
				foreach (KeyValuePair<ResourceEnum, float> resourceToAmount in craftingResourceToResourceAmount.Value) {
					float amount = resourceToAmount.Value;
					if (amount < 1 && amount > 0) {
						resource.amountCreated = Mathf.RoundToInt(1 / amount);
						resource.craftingResources.Add(new ResourceAmount(GetResourceByEnum(resourceToAmount.Key), 1));
					} else {
						resource.amountCreated = 1;
						resource.craftingResources.Add(new ResourceAmount(GetResourceByEnum(resourceToAmount.Key), Mathf.RoundToInt(resourceToAmount.Value)));
					}
				}
			}
		}
	}

	private static readonly Dictionary<ResourceClassEnum, List<Resource>> resourceClassToResources = new Dictionary<ResourceClassEnum, List<Resource>>();

	public static List<Resource> GetResourcesInClass(ResourceClassEnum resourceClass) {
		return resourceClassToResources[resourceClass];
	}

	public static bool IsResourceInClass(ResourceClassEnum resourceClass, Resource resource) {
		return resource.classes.Contains(resourceClass);
	}

	public Dictionary<ResourceGroupEnum, ResourceGroup> resourceGroups = new Dictionary<ResourceGroupEnum, ResourceGroup>();

	public class ResourceGroup {
		public readonly ResourceGroupEnum type;
		public readonly string name;

		public readonly List<Resource> resources;

		public ResourceGroup(
			ResourceGroupEnum type,
			List<Resource> resources
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.resources = resources;
		}
	}

	public ResourceGroup GetResourceGroupByEnum(ResourceGroupEnum resourceGroupEnum) {
		return resourceGroups[resourceGroupEnum];
	}

	public List<ResourceGroup> GetResourceGroups() {
		return resourceGroups.Values.ToList();
	}

	public ResourceGroup GetRandomResourceGroup() {
		List<ResourceGroup> resourcesGroupsWithoutNone = GetResourceGroups().Where(rg => rg.type != ResourceGroupEnum.None).ToList();
		return resourcesGroupsWithoutNone[UnityEngine.Random.Range(0, resourcesGroupsWithoutNone.Count)];
	}

	public Dictionary<ResourceEnum, Resource> resources = new Dictionary<ResourceEnum, Resource>();

	public class Resource {
		public readonly ResourceEnum type;
		public string name; // Can't be readonly

		public readonly ResourceGroupEnum groupType;

		public readonly List<ResourceClassEnum> classes;

		public readonly int weight;
		public readonly int volume;

		public readonly int price;

		public Sprite image; // Can't be readonly

		// Fuel
		public readonly int fuelEnergy;

		// Crafting
		public readonly Dictionary<ObjectSubGroupEnum, List<ObjectEnum>> craftingObjects;
		public readonly int craftingEnergy;
		public readonly int craftingTime;
		public readonly List<ResourceAmount> craftingResources = new List<ResourceAmount>(); // Filled in CreateResources() after all resources created
		public int amountCreated = 1; // Can't be readonly

		// World Amounts
		private int worldTotalAmount;
		private int colonistsTotalAmount;
		private int containerTotalAmount;
		private int unreservedContainerTotalAmount;
		private int unreservedTradingPostTotalAmount;
		private int availableAmount;

		// UI
		public UIManager.ResourceElement resourceListElement; // TODO REMOVE THIS (replace with a UIManager Dictionary<ResourceEnum, ResourceElement> and access like that

		public Resource(
			ResourceEnum type,
			ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			Dictionary<ObjectSubGroupEnum, List<ObjectEnum>> craftingObjects,
			int craftingEnergy,
			int craftingTime
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;

			this.classes = classes;

			this.weight = weight;
			this.volume = volume;

			this.price = price;

			image = Resources.Load<Sprite>(@"Sprites/Resources/" + name + "/" + name.Replace(' ', '-') + "-base");

			// Fuel
			this.fuelEnergy = fuelEnergy;

			// Crafting
			this.craftingObjects = craftingObjects;
			this.craftingEnergy = craftingEnergy;
			this.craftingTime = craftingTime;
		}

		public int GetWorldTotalAmount() {
			return worldTotalAmount;
		}

		public void SetWorldTotalAmount(int worldTotalAmount) {
			this.worldTotalAmount = worldTotalAmount;
		}

		public void AddToWorldTotalAmount(int amount) {
			SetWorldTotalAmount(GetWorldTotalAmount() + amount);
		}

		public int GetColonistsTotalAmount() {
			return colonistsTotalAmount;
		}

		public void SetColonistsTotalAmount(int colonistsTotalAmount) {
			this.colonistsTotalAmount = colonistsTotalAmount;
		}

		public void AddToColonistsTotalAmount(int amount) {
			SetColonistsTotalAmount(GetColonistsTotalAmount() + amount);
		}

		public int GetContainerTotalAmount() {
			return containerTotalAmount;
		}

		public void SetContainerTotalAmount(int containerTotalAmount) {
			this.containerTotalAmount = containerTotalAmount;
		}

		public void AddToContainerTotalAmount(int amount) {
			SetContainerTotalAmount(GetContainerTotalAmount() + amount);
		}

		public int GetUnreservedContainerTotalAmount() {
			return unreservedContainerTotalAmount;
		}

		public void SetUnreservedContainerTotalAmount(int unreservedContainerTotalAmount) {
			this.unreservedContainerTotalAmount = unreservedContainerTotalAmount;
		}

		public void AddToUnreservedContainerTotalAmount(int amount) {
			SetUnreservedContainerTotalAmount(GetUnreservedContainerTotalAmount() + amount);
		}

		public int GetUnreservedTradingPostTotalAmount() {
			return unreservedTradingPostTotalAmount;
		}

		public void SetUnreservedTradingPostTotalAmount(int unreservedTradingPostTotalAmount) {
			this.unreservedTradingPostTotalAmount = unreservedTradingPostTotalAmount;
		}

		public void AddToUnreservedTradingPostTotalAmount(int amount) {
			SetUnreservedTradingPostTotalAmount(GetUnreservedTradingPostTotalAmount() + amount);
		}

		public void SetAvailableAmount(int availableAmount) {
			this.availableAmount = availableAmount;
		}

		public void CalculateAvailableAmount() {
			availableAmount = GetColonistsTotalAmount() + GetUnreservedContainerTotalAmount();
		}

		public int GetAvailableAmount() {
			return availableAmount;
		}

		public bool CanBeCraftedBy(ObjectPrefab prefab) {
			if (craftingObjects.ContainsKey(prefab.subGroupType)) {
				if (craftingObjects[prefab.subGroupType] == null) {
					return true;
				} else {
					if (craftingObjects[prefab.subGroupType].Contains(prefab.type)) {
						return true;
					}
				}
			}
			return false;
		}
	}

	public List<Resource> GetResources() {
		return resources.Values.ToList();
	}

	public Resource GetResourceByString(string resourceString) {
		return GetResourceByEnum((ResourceEnum)Enum.Parse(typeof(ResourceEnum), resourceString));
	}

	public Resource GetResourceByEnum(ResourceEnum resourceEnum) {
		return resources[resourceEnum];
	}

	public List<Resource> GetResourcesByCraftingObject(CraftingObject craftingObject) {
		List<Resource> resources = new List<Resource>();
		foreach (Resource resource in GetResourcesInClass(ResourceClassEnum.Craftable)) {
			foreach (ObjectSubGroupEnum craftingObjectSubGroupEnum in resource.craftingObjects.Keys) {
				if (craftingObject.prefab.subGroupType == craftingObjectSubGroupEnum) {
					if (resource.craftingObjects[craftingObjectSubGroupEnum] == null) {
						resources.Add(resource);
						break;
					} else {
						foreach (ObjectEnum craftingObjectEnum in resource.craftingObjects[craftingObjectSubGroupEnum]) {
							if (craftingObjectEnum == craftingObject.prefab.type) {
								resources.Add(resource);
								break;
							}
						}
					}
				}
			}
		}
		return resources;
	}

	public class Food : Resource {

		public int nutrition = 0;

		public Food(
			ResourceEnum type,
			ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			Dictionary<ObjectSubGroupEnum, List<ObjectEnum>> craftingObjects,
			int craftingEnergy,
			int craftingTime,
			int nutrition
		) : base(
			type,
			groupType,
			classes,
			weight,
			volume,
			price,
			fuelEnergy,
			craftingObjects,
			craftingEnergy,
			craftingTime
		) {
			this.nutrition = nutrition;
		}
	}

	public List<ClothingPrefab> clothingPrefabs = new List<ClothingPrefab>();

	public class ClothingPrefab {
		public HumanManager.Human.Appearance appearance;
		public ClothingEnum clothingType;
		public int insulation;
		public int waterResistance;
		public int weightCapacity;
		public int volumeCapacity;

		public List<string> colours;

		public List<List<Sprite>> moveSprites = new List<List<Sprite>>();

		public List<Clothing> clothes = new List<Clothing>();

		public ClothingPrefab(
			HumanManager.Human.Appearance appearance,
			ClothingEnum clothingType,
			int insulation,
			int waterResistance,
			int weightCapacity,
			int volumeCapacity,
			List<string> colours,
			int typeIndex
		) {
			this.appearance = appearance;
			this.clothingType = clothingType;
			this.insulation = insulation;
			this.waterResistance = waterResistance;
			this.weightCapacity = weightCapacity;
			this.volumeCapacity = volumeCapacity;
			this.colours = colours;

			for (int i = 0; i < 4; i++) {
				moveSprites.Add(Resources.LoadAll<Sprite>(@"Sprites/Clothes/" + appearance + "/clothes-" + appearance.ToString().ToLower() + "-" + i).Skip(typeIndex).Take(colours.Count).ToList());
			}
		}
	}

	public List<ClothingPrefab> GetClothingPrefabsByAppearance(HumanManager.Human.Appearance appearance) {
		return clothingPrefabs.FindAll(c => c.appearance == appearance);
	}

	public class Clothing : Resource {
		public ClothingPrefab prefab;
		public string colour;

		public List<Sprite> moveSprites = new List<Sprite>();

		public Clothing(
			ResourceEnum type,
			ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			Dictionary<ObjectSubGroupEnum, List<ObjectEnum>> craftingObjects,
			int craftingEnergy,
			int craftingTime,
			ClothingPrefab prefab,
			string colour
		) : base(
			type,
			groupType,
			classes,
			weight,
			volume,
			price,
			fuelEnergy,
			craftingObjects,
			craftingEnergy,
			craftingTime
		) {
			this.prefab = prefab;
			this.colour = colour;

			name = UIManager.SplitByCapitals(colour + prefab.clothingType);

			for (int i = 0; i < 4; i++) {
				moveSprites.Add(prefab.moveSprites[i][prefab.colours.IndexOf(colour)]);
			}

			if (prefab.appearance == HumanManager.Human.Appearance.Backpack) {
				image = moveSprites[1];
			} else {
				image = moveSprites[0];
			}
		}
	}

	public List<Clothing> GetClothesByAppearance(HumanManager.Human.Appearance appearance) {
		return GetResourcesInClass(ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.appearance == appearance).ToList();
	}

	public class ResourceAmount {
		public Resource resource;
		public int amount;

		public ResourceAmount(Resource resource, int amount) {
			this.resource = resource;
			this.amount = amount;
		}
	}

	public class ResourceRange {
		public Resource resource;
		public int min;
		public int max;

		public ResourceRange(Resource resource, int min, int max) {
			this.resource = resource;
			this.min = min;
			this.max = max;
		}
	}

	public class ReservedResources {
		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public HumanManager.Human human;

		public ReservedResources(List<ResourceAmount> resourcesToReserve, HumanManager.Human humanReservingResources) {
			resources.AddRange(resourcesToReserve);
			human = humanReservingResources;
		}
	}

	public class TradeResourceAmount {
		public Resource resource;

		public int caravanAmount;
		public int colonyAmount;

		private int oldTradeAmount = 0;
		private int tradeAmount;

		public int caravanResourcePrice;

		public CaravanManager.Caravan caravan;

		public TradeResourceAmount(Resource resource, int caravanAmount, CaravanManager.Caravan caravan) {
			this.resource = resource;

			this.caravanAmount = caravanAmount;

			caravanResourcePrice = caravan.DeterminePriceForResource(resource);

			this.caravan = caravan;
		}

		public void Update() {
			UpdateCaravanAmount();
		}

		private void UpdateCaravanAmount() {
			caravanAmount = 0;
			ResourceAmount caravanResourceAmount = caravan.GetInventory().resources.Find(ra => ra.resource == resource);
			if (caravanResourceAmount != null) {
				caravanAmount = caravanResourceAmount.amount;
			}
		}

		public void SetColonyAmount(int colonyAmount) {
			this.colonyAmount = colonyAmount;
		}

		public int GetTradeAmount() {
			return tradeAmount;
		}

		public void SetTradeAmount(int tradeAmount) {
			oldTradeAmount = this.tradeAmount;
			this.tradeAmount = tradeAmount;
			if (oldTradeAmount != this.tradeAmount) {
				caravan.SetSelectedResource(this);
			}
		}
	}

	public class ConfirmedTradeResourceAmount {
		public Resource resource;
		public int tradeAmount;
		public int amountRemaining;

		public ConfirmedTradeResourceAmount(Resource resource, int tradeAmount) {
			this.resource = resource;
			this.tradeAmount = tradeAmount;
			amountRemaining = tradeAmount;
		}
	}

	public JobManager.Job CreateResource(CraftableResourceInstance resource, CraftingObject craftingObject) {
		JobManager.Job job = new JobManager.Job(craftingObject.tile, GetObjectPrefabByEnum(ObjectEnum.CreateResource), null, 0);
		job.SetCreateResourceData(resource);
		GameManager.jobM.CreateJob(job);
		return job;
	}

	public interface IInventory {
		Inventory GetInventory();
	}

	public class Inventory {

		public List<ResourceAmount> resources = new List<ResourceAmount>();
		public List<ReservedResources> reservedResources = new List<ReservedResources>();

		public IInventory parent;

		//public int maxAmount;

		public int maxWeight;
		public int maxVolume;

		public Inventory(IInventory parent, /*int maxAmount*/int maxWeight, int maxVolume) {
			this.parent = parent;
			//this.maxAmount = maxAmount;
			this.maxWeight = maxWeight;
			this.maxVolume = maxVolume;
		}

		//public int CountResources() {
		//	return resources.Sum(resource => resource.amount) + reservedResources.Sum(reservedResource => reservedResource.resources.Sum(rr => rr.amount));
		//}

		public int UsedWeight() {
			return resources.Sum(ra => ra.amount * ra.resource.weight) + reservedResources.Sum(rr => rr.resources.Sum(ra => ra.amount * ra.resource.weight));
		}

		public int UsedVolume() {
			return resources.Sum(ra => ra.amount * ra.resource.volume) + reservedResources.Sum(rr => rr.resources.Sum(ra => ra.amount * ra.resource.volume));
		}

		public int ChangeResourceAmount(Resource resource, int amount, bool limitToMaxAmount) {
			//int remainingAmount = 0;

			//int remainingWeight = 0;
			//int remainingVolume = 0;

			int highestOverflowAmount = 0;

			if (limitToMaxAmount && amount > 0) {
				//remainingAmount = maxAmount - (CountResources() + amount);

				//remainingWeight = maxWeight - (TotalWeight() + (amount * resource.weight));
				//remainingVolume = maxVolume - (TotalVolume() + (amount * resource.volume));

				//if (remainingAmount < 0) {
				//	remainingAmount = Mathf.Abs(remainingAmount);
				//	amount -= remainingAmount;
				//} else {
				//	remainingAmount = 0;
				//}

				int totalWeight = UsedWeight();
				//Debug.Log("totalWeight: " + totalWeight);
				int totalVolume = UsedVolume();
				//Debug.Log("totalVolume: " + totalVolume);
				int weightOverflowAmount = ((resource.weight * amount) - maxWeight) / resource.weight;
				int volumeOverflowAmount = ((resource.volume * amount) - maxVolume) / resource.volume;

				highestOverflowAmount = Mathf.Max(weightOverflowAmount, volumeOverflowAmount);

				if (highestOverflowAmount > 0) {
					amount -= highestOverflowAmount;
				}
			}

			ResourceAmount existingResourceAmount = resources.Find(ra => ra.resource == resource);
			if (existingResourceAmount != null) {
				if (amount >= 0 || (amount - existingResourceAmount.amount) >= 0) {
					existingResourceAmount.amount += amount;
				} else if (amount < 0 && (existingResourceAmount.amount + amount) >= 0) {
					existingResourceAmount.amount += amount;
				}
			} else {
				if (amount > 0) {
					resources.Add(new ResourceAmount(resource, amount));
				}
			}
			existingResourceAmount = resources.Find(ra => ra.resource == resource);
			if (existingResourceAmount != null) {
				if (existingResourceAmount.amount == 0) {
					resources.Remove(existingResourceAmount);
				}
			}

			GameManager.resourceM.CalculateResourceTotals();
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.UpdateSelectedTradingPostInfo();
			GameManager.jobM.UpdateColonistJobs();

			//return remainingAmount;
			return highestOverflowAmount;
		}

		public bool ReserveResources(List<ResourceAmount> resourcesToReserve, HumanManager.Human humanReservingResources) {
			bool allResourcesFound = true;
			foreach (ResourceAmount raReserve in resourcesToReserve) {
				ResourceAmount raInventory = resources.Find(ra => ra.resource == raReserve.resource);
				if (!(raInventory != null && raInventory.amount >= raReserve.amount)) {
					allResourcesFound = false;
				}
			}
			if (allResourcesFound) {
				foreach (ResourceAmount raReserve in resourcesToReserve) {
					ResourceAmount raInventory = resources.Find(ra => ra.resource == raReserve.resource);
					ChangeResourceAmount(raInventory.resource, -raReserve.amount, false);
				}
				reservedResources.Add(new ReservedResources(resourcesToReserve, humanReservingResources));
			}
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.UpdateSelectedTradingPostInfo();
			return allResourcesFound;
		}

		public List<ReservedResources> TakeReservedResources(HumanManager.Human humanReservingResources, List<ResourceAmount> resourcesToTake = null) {
			List<ReservedResources> reservedResourcesByHuman = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.human == humanReservingResources && (resourcesToTake == null || rr.resources.Find(ra => resourcesToTake.Find(rtt => rtt.resource == ra.resource) != null) != null)) {
					reservedResourcesByHuman.Add(rr);
				}
			}
			foreach (ReservedResources rr in reservedResourcesByHuman) {
				reservedResources.Remove(rr);
			}
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.UpdateSelectedTradingPostInfo();
			return reservedResourcesByHuman;
		}

		public void ReleaseReservedResources(HumanManager.Human human) {
			List<ReservedResources> reservedResourcesToRemove = new List<ReservedResources>();
			foreach (ReservedResources rr in reservedResources) {
				if (rr.human == human) {
					foreach (ResourceAmount ra in rr.resources) {
						ChangeResourceAmount(ra.resource, ra.amount, false);
					}
					reservedResourcesToRemove.Add(rr);
				}
			}
			foreach (ReservedResources rrRemove in reservedResourcesToRemove) {
				reservedResources.Remove(rrRemove);
			}
			reservedResourcesToRemove.Clear();
			GameManager.uiM.SetSelectedColonistInformation(true);
			GameManager.uiM.SetSelectedTraderMenu();
			GameManager.uiM.SetSelectedContainerInfo();
			GameManager.uiM.UpdateSelectedTradingPostInfo();
		}

		public static void TransferResourcesBetweenInventories(Inventory fromInventory, Inventory toInventory, ResourceAmount resourceAmount, bool limitToMaxAmount) {
			Resource resource = resourceAmount.resource;
			int amount = resourceAmount.amount;
			
			fromInventory.ChangeResourceAmount(resource, -amount, false);
			int remainingAmount = toInventory.ChangeResourceAmount(resource, amount, limitToMaxAmount);
			if (remainingAmount > 0) {
				fromInventory.ChangeResourceAmount(resource, remainingAmount, false);
			}
		}

		public static void TransferResourcesBetweenInventories(Inventory fromInventory, Inventory toInventory, List<ResourceAmount> resourceAmounts, bool limitToMaxAmount) {
			foreach (ResourceAmount resourceAmount in resourceAmounts.ToList()) {
				TransferResourcesBetweenInventories(
					fromInventory, 
					toInventory, 
					resourceAmount, 
					limitToMaxAmount
				);
			}
		}

		public bool ContainsResourceAmount(ResourceAmount resourceAmount) {
			ResourceAmount matchingResource = resources.Find(ra => ra.resource == resourceAmount.resource);
			if (matchingResource != null) {
				return matchingResource.amount <= resourceAmount.amount;
			}
			return false;
		}
	}

	public void CalculateResourceTotals() {
		foreach (Resource resource in GetResources()) {
			resource.SetWorldTotalAmount(0);
			resource.SetColonistsTotalAmount(0);
			resource.SetContainerTotalAmount(0);
			resource.SetUnreservedContainerTotalAmount(0);
			resource.SetUnreservedTradingPostTotalAmount(0);
			resource.SetAvailableAmount(0);
		}

		foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
			foreach (ResourceAmount resourceAmount in colonist.GetInventory().resources) {
				resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
				resourceAmount.resource.AddToColonistsTotalAmount(resourceAmount.amount);
			}
			foreach (ReservedResources reservedResources in colonist.GetInventory().reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
					resourceAmount.resource.AddToColonistsTotalAmount(resourceAmount.amount);
				}
			}
		}
		foreach (Container container in containers) {
			foreach (ResourceAmount resourceAmount in container.GetInventory().resources) {
				resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
				resourceAmount.resource.AddToContainerTotalAmount(resourceAmount.amount);
				resourceAmount.resource.AddToUnreservedContainerTotalAmount(resourceAmount.amount);
			}
			foreach (ReservedResources reservedResources in container.GetInventory().reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.resource.AddToWorldTotalAmount(resourceAmount.amount);
					resourceAmount.resource.AddToContainerTotalAmount(resourceAmount.amount);
				}
			}
		}
		foreach (TradingPost tradingPost in tradingPosts) {
			foreach (ResourceAmount resourceAmount in tradingPost.GetInventory().resources) {
				resourceAmount.resource.AddToUnreservedTradingPostTotalAmount(resourceAmount.amount);
			}
		}

		foreach (Resource resource in GetResources()) {
			resource.CalculateAvailableAmount();
		}
	}

	public List<ResourceAmount> GetFilteredResources(bool colonistInventory, bool colonistReserved, bool containerInventory, bool containerReserved) {
		List<ResourceAmount> returnResources = new List<ResourceAmount>();
		if (colonistInventory || colonistReserved) {
			foreach (ColonistManager.Colonist colonist in GameManager.colonistM.colonists) {
				if (colonistInventory) {
					foreach (ResourceAmount resourceAmount in colonist.GetInventory().resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
						} else {
							existingResourceAmount.amount += resourceAmount.amount;
						}
					}
				}
				if (colonistReserved) {
					foreach (ReservedResources reservedResources in colonist.GetInventory().reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
							} else {
								existingResourceAmount.amount += resourceAmount.amount;
							}
						}
					}
				}
			}
		}
		if (containerInventory || containerReserved) {
			foreach (Container container in containers) {
				if (containerInventory) {
					foreach (ResourceAmount resourceAmount in container.GetInventory().resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
						} else {
							existingResourceAmount.amount += resourceAmount.amount;
						}
					}
				}
				if (containerReserved) {
					foreach (ReservedResources reservedResources in container.GetInventory().reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.resource == resourceAmount.resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
							} else {
								existingResourceAmount.amount += resourceAmount.amount;
							}
						}
					}
				}
			}
		}
		return returnResources;
	}

	public enum ObjectGroupPropertyEnum {
		Group,
		Type,
		SubGroups
	}

	public enum ObjectGroupEnum {
		Structure, Furniture, Containers, Trading, Crafting,
		Farm,
		Terraform,
		Command,
		None,
	}

	public enum ObjectSubGroupPropertyEnum {
		SubGroup,
		Type,
		Objects
	}

	public enum ObjectSubGroupEnum {
		Roofs, Walls, Fences, Doors, Floors, Foundations,
		Beds, Chairs, Tables, Lights,
		Containers,
		TradingPosts,
		Furnaces, Processing,
		Plants, Terrain, Remove, Cancel, Priority,
		PlantFarm, HarvestFarm,
		None
	}

	public enum ObjectPropertyEnum {
		Object,
		Type,
		InstanceType,
		Layer,
		Bitmasking,
		BlocksLight,
		MultiTilePositions,
		Integrity,
		Walkable,
		WalkSpeed,
		Buildable,
		Flammability,
		MaxInventoryVolume,
		MaxInventoryWeight,
		MaxLightDistance,
		LightColour,
		RestComfortAmount,
		UsesFuel,
		GrowthTimeDays,
		Seed,
		HarvestResource,
		TimeToBuild,
		CommonResources,
		Variations,
		VariationNameOrder,
		SelectionModifiers,
		JobType,
		AddToTileWhenBuilt
	}

	public enum VariationPropertyEnum {
		Variation,
		Name,
		UniqueResources,
		WalkSpeed,
		Integrity,
		Flammability,
		TimeToBuild,
		Plants
	}

	public enum VariationNameOrderEnum {
		VariationObject,
		ObjectVariation
	}

	public enum ObjectEnum {
		Roof,
		Wall,
		Fence,
		WoodenDoor,
		WoodenFloor, BrickFloor,
		WoodenDock,
		Basket, WoodenChest, WoodenDrawers,
		WoodenBed,
		WoodenChair,
		WoodenTable,
		Torch, WoodenLamp,
		TradingPost,
		Furnace,
		Gin, Loom, SplittingBlock, SplittingLog, Anvil, BrickFormer,
		ChopPlant, Plant,
		Mine, Dig, Fill,
		RemoveRoof, RemoveObject, RemoveFloor, RemoveAll,
		Cancel,
		IncreasePriority, DecreasePriority,
		WheatFarm, PotatoFarm, CottonFarm,
		HarvestFarm,
		CreateResource, PickupResources, TransferResources, CollectResources, EmptyInventory, Sleep, CollectWater, Drink, CollectFood, Eat, WearClothes
	}

	public enum ObjectInstanceType {
		Normal,
		Container,
		TradingPost,
		SleepSpot,
		LightSource,
		CraftingObject,
		Farm
	}

	public void CreateObjectPrefabs() {
		List<KeyValuePair<string, object>> objectGroupProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/objects").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> objectGroupProperty in objectGroupProperties) {
			switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupProperty.Key)) {
				case ObjectGroupPropertyEnum.Group:

					ObjectGroupEnum? groupType = null;
					List<ObjectPrefabSubGroup> subGroups = new List<ObjectPrefabSubGroup>();

					foreach (KeyValuePair<string, object> objectGroupSubProperty in (List<KeyValuePair<string, object>>)objectGroupProperty.Value) {
						switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupSubProperty.Key)) {
							case ObjectGroupPropertyEnum.Type:
								groupType = (ObjectGroupEnum)Enum.Parse(typeof(ObjectGroupEnum), (string)objectGroupSubProperty.Value);
								break;
							case ObjectGroupPropertyEnum.SubGroups:
								foreach (KeyValuePair<string, object> objectSubGroupProperty in (List<KeyValuePair<string, object>>)objectGroupSubProperty.Value) {
									switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupProperty.Key)) {
										case ObjectSubGroupPropertyEnum.SubGroup:

											ObjectSubGroupEnum? subGroupType = null;
											List<ObjectPrefab> prefabs = new List<ObjectPrefab>();

											foreach (KeyValuePair<string, object> objectSubGroupSubProperty in (List<KeyValuePair<string, object>>)objectSubGroupProperty.Value) {
												switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupSubProperty.Key)) {
													case ObjectSubGroupPropertyEnum.Type:
														subGroupType = (ObjectSubGroupEnum)Enum.Parse(typeof(ObjectSubGroupEnum), (string)objectSubGroupSubProperty.Value);
														break;
													case ObjectSubGroupPropertyEnum.Objects:
														foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)objectSubGroupSubProperty.Value) {
															switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectProperty.Key)) {
																case ObjectPropertyEnum.Object:

																	ObjectEnum? type = null;
																	ObjectInstanceType instanceType = ObjectInstanceType.Normal;
																	int? layer = null;
																	bool? bitmasking = false;
																	bool? blocksLight = false;
																	int? integrity = 0;
																	List<Vector2> multiTilePositions = new List<Vector2>();

																	float? walkSpeed = 1;
																	bool? walkable = true;
																	bool? buildable = true;
																	float? flammability = 0;

																	// Container
																	int? maxInventoryVolume = null;
																	int? maxInventoryWeight = null;

																	// Light
																	int? maxLightDistance = null;
																	Color? lightColour = null;

																	// Sleep Spot
																	float? restComfortAmount = null;

																	// Crafting Object
																	bool? usesFuel = null;

																	// Farm
																	int? growthTimeDays = null;
																	Resource seedResource = null;
																	Resource harvestResource = null;

																	// Job
																	int? timeToBuild = null;
																	List<ResourceAmount> commonResources = new List<ResourceAmount>();
																	List<Variation> variations = new List<Variation>();
																	VariationNameOrderEnum variationNameOrder = VariationNameOrderEnum.VariationObject;
																	List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
																	JobManager.JobEnum? jobType = null;
																	bool? addToTileWhenBuilt = true;

																	foreach (KeyValuePair<string, object> objectSubProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
																		switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectSubProperty.Key)) {
																			case ObjectPropertyEnum.Type:
																				type = (ObjectEnum)Enum.Parse(typeof(ObjectEnum), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.InstanceType:
																				instanceType = (ObjectInstanceType)Enum.Parse(typeof(ObjectInstanceType), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Layer:
																				layer = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Bitmasking:
																				bitmasking = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.BlocksLight:
																				blocksLight = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Integrity:
																				integrity = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.MultiTilePositions:
																				foreach (string vector2String in ((string)objectSubProperty.Value).Split(';')) {
																					multiTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
																				}
																				break;
																			case ObjectPropertyEnum.WalkSpeed:
																				walkSpeed = float.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Walkable:
																				walkable = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Buildable:
																				buildable = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Flammability:
																				flammability = float.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.MaxInventoryVolume:
																				if (((string)objectSubProperty.Value).Contains("Max")) {
																					maxInventoryVolume = int.MaxValue;
																				} else {
																					maxInventoryVolume = int.Parse((string)objectSubProperty.Value);
																				}
																				break;
																			case ObjectPropertyEnum.MaxInventoryWeight:
																				if (((string)objectSubProperty.Value).Contains("Max")) {
																					maxInventoryWeight = int.MaxValue;
																				} else {
																					maxInventoryWeight = int.Parse((string)objectSubProperty.Value);
																				}
																				break;
																			case ObjectPropertyEnum.MaxLightDistance:
																				maxLightDistance = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.LightColour:
																				lightColour = UIManager.HexToColor((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.RestComfortAmount:
																				restComfortAmount = float.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.UsesFuel:
																				usesFuel = bool.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.GrowthTimeDays:
																				growthTimeDays = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.Seed:
																				seedResource = GetResourceByString((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.HarvestResource:
																				harvestResource = GetResourceByString((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.TimeToBuild:
																				timeToBuild = int.Parse((string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.CommonResources:
																				foreach (string resourceAmountString in ((string)objectSubProperty.Value).Split(',')) {
																					commonResources.Add(new ResourceAmount(GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
																				}
																				break;
																			case ObjectPropertyEnum.Variations:
																				foreach (KeyValuePair<string, object> variationsProperty in (List<KeyValuePair<string, object>>)objectSubProperty.Value) {
																					switch ((VariationPropertyEnum)Enum.Parse(typeof(VariationPropertyEnum), variationsProperty.Key)) {
																						case VariationPropertyEnum.Variation:

																							string variationName = null;
																							List<ResourceAmount> variationUniqueResources = new List<ResourceAmount>();
																							float? variationWalkSpeed = null;
																							int? variationIntegrity = null;
																							float? variationFlammability = null;
																							int? variationTimeToBuild = null;

																							// Plants
																							Dictionary<PlantPrefab, Resource> variationPlants = new Dictionary<PlantPrefab, Resource>();

																							foreach (KeyValuePair<string, object> variationProperty in (List<KeyValuePair<string, object>>)variationsProperty.Value) {
																								switch ((VariationPropertyEnum)Enum.Parse(typeof(VariationPropertyEnum), variationProperty.Key)) {
																									case VariationPropertyEnum.Name:
																										variationName = UIManager.RemoveNonAlphanumericChars((string)variationProperty.Value);
																										break;
																									case VariationPropertyEnum.UniqueResources:
																										foreach (string resourceAmountString in ((string)variationProperty.Value).Split(',')) {
																											variationUniqueResources.Add(new ResourceAmount(GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
																										}
																										break;
																									case VariationPropertyEnum.WalkSpeed:
																										variationWalkSpeed = float.Parse((string)variationProperty.Value);
																										break;
																									case VariationPropertyEnum.Integrity:
																										variationIntegrity = int.Parse((string)variationProperty.Value);
																										break;
																									case VariationPropertyEnum.Flammability:
																										variationFlammability = float.Parse((string)variationProperty.Value);
																										break;
																									case VariationPropertyEnum.TimeToBuild:
																										variationTimeToBuild = int.Parse((string)variationProperty.Value);
																										break;
																									case VariationPropertyEnum.Plants:
																										foreach (string plantToHarvestResourceString in ((string)variationProperty.Value).Split(',')) {
																											string harvestResourceString = plantToHarvestResourceString.Split(':')[1];
																											variationPlants.Add(
																												GetPlantPrefabByString(plantToHarvestResourceString.Split(':')[0]),
																												harvestResourceString.Contains("None") ? null : GetResourceByString(harvestResourceString)
																											);
																										}
																										break;
																									default:
																										Debug.LogError("Unknown variation property: " + variationProperty.Key + " " + variationProperty.Value);
																										break;
																								}
																							}

																							variations.Add(new Variation(
																								null, // Set after objectPrefab is created below
																								variationName,
																								variationUniqueResources,
																								variationWalkSpeed ?? walkSpeed.Value,
																								variationIntegrity ?? integrity.Value,
																								variationFlammability ?? flammability.Value,
																								variationTimeToBuild ?? timeToBuild.Value,
																								variationPlants
																							));

																							break;
																					}
																				}
																				break;
																			case ObjectPropertyEnum.VariationNameOrder:
																				variationNameOrder = (VariationNameOrderEnum)Enum.Parse(typeof(VariationNameOrderEnum), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.SelectionModifiers:
																				foreach (string selectionModifierString in ((string)objectSubProperty.Value).Split(',')) {
																					selectionModifiers.Add((JobManager.SelectionModifiersEnum)Enum.Parse(typeof(JobManager.SelectionModifiersEnum), selectionModifierString));
																				}
																				break;
																			case ObjectPropertyEnum.JobType:
																				jobType = (JobManager.JobEnum)Enum.Parse(typeof(JobManager.JobEnum), (string)objectSubProperty.Value);
																				break;
																			case ObjectPropertyEnum.AddToTileWhenBuilt:
																				addToTileWhenBuilt = bool.Parse((string)objectSubProperty.Value);
																				break;
																			default:
																				Debug.LogError("Unknown object sub property: " + objectSubProperty.Key + " " + objectSubProperty.Value);
																				break;
																		}
																	}

																	if (instanceType != ObjectInstanceType.Container && instanceType != ObjectInstanceType.TradingPost) {
																		if (!maxInventoryVolume.HasValue) {
																			maxInventoryVolume = 0;
																		}
																		if (!maxInventoryWeight.HasValue) {
																			maxInventoryWeight = 0;
																		}
																	}

																	if (instanceType != ObjectInstanceType.LightSource) {
																		if (!maxLightDistance.HasValue) {
																			maxLightDistance = 0;
																		}
																		if (!lightColour.HasValue) {
																			lightColour = Color.white;
																		}
																	}

																	if (instanceType != ObjectInstanceType.SleepSpot) {
																		if (!restComfortAmount.HasValue) {
																			restComfortAmount = 0;
																		}
																	}

																	if (instanceType != ObjectInstanceType.CraftingObject) {
																		if (!usesFuel.HasValue) {
																			usesFuel = false;
																		}
																	}

																	if (instanceType != ObjectInstanceType.Farm) {
																		if (!growthTimeDays.HasValue) {
																			growthTimeDays = 0;
																		}
																	}

																	ObjectPrefab objectPrefab = new ObjectPrefab(
																		type.Value,
																		groupType.Value,
																		subGroupType.Value,
																		instanceType,
																		layer.Value,
																		bitmasking.Value,
																		blocksLight.Value,
																		integrity.Value,
																		multiTilePositions,
																		walkSpeed.Value,
																		walkable.Value,
																		buildable.Value,
																		flammability.Value,
																		maxInventoryVolume.Value,
																		maxInventoryWeight.Value,
																		maxLightDistance.Value,
																		lightColour.Value,
																		restComfortAmount.Value,
																		usesFuel.Value,
																		growthTimeDays.Value,
																		seedResource,
																		harvestResource,
																		timeToBuild.Value,
																		commonResources,
																		variations,
																		variationNameOrder,
																		selectionModifiers,
																		jobType.Value,
																		addToTileWhenBuilt.Value
																	);
																	foreach (Variation variation in objectPrefab.variations) {
																		variation.prefab = objectPrefab;
																	}
																	prefabs.Add(objectPrefab);
																	objectPrefabs.Add(type.Value, objectPrefab);

																	break;
																default:
																	Debug.LogError("Unknown object property: " + objectProperty.Key + " " + objectProperty.Value);
																	break;
															}
														}
														break;
													default:
														Debug.LogError("Unknown object sub group sub property: " + objectSubGroupSubProperty.Key + " " + objectSubGroupSubProperty.Value);
														break;
												}
											}

											ObjectPrefabSubGroup objectPrefabSubGroup = new ObjectPrefabSubGroup(
												subGroupType.Value,
												groupType.Value,
												prefabs
											);
											subGroups.Add(objectPrefabSubGroup);
											objectPrefabSubGroups.Add(subGroupType.Value, objectPrefabSubGroup);

											break;
										default:
											Debug.LogError("Unknown object sub group property: " + objectSubGroupProperty.Key + " " + objectSubGroupProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown object group sub property: " + objectGroupSubProperty.Key + " " + objectGroupSubProperty.Value);
								break;
						}
					}

					ObjectPrefabGroup objectPrefabGroup = new ObjectPrefabGroup(
						groupType.Value,
						subGroups
					);
					objectPrefabGroups.Add(groupType.Value, objectPrefabGroup);

					break;
				default:
					Debug.LogError("Unknown object group property: " + objectGroupProperty.Key + " " + objectGroupProperty.Value);
					break;
			}
		}
	}

	private readonly Dictionary<ObjectGroupEnum, ObjectPrefabGroup> objectPrefabGroups = new Dictionary<ObjectGroupEnum, ObjectPrefabGroup>();

	public class ObjectPrefabGroup {
		public readonly ObjectGroupEnum type;
		public readonly string name;

		public readonly List<ObjectPrefabSubGroup> subGroups = new List<ObjectPrefabSubGroup>();

		public ObjectPrefabGroup(ObjectGroupEnum type, List<ObjectPrefabSubGroup> subGroups) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.subGroups = subGroups;
		}
	}

	public ObjectPrefabGroup GetObjectPrefabGroupByString(string objectPrefabGroupString) {
		return GetObjectPrefabGroupByEnum((ObjectGroupEnum)Enum.Parse(typeof(ObjectGroupEnum), objectPrefabGroupString));
	}

	public ObjectPrefabGroup GetObjectPrefabGroupByEnum(ObjectGroupEnum objectGroupEnum) {
		return objectPrefabGroups[objectGroupEnum];
	}

	public List<ObjectPrefabGroup> GetObjectPrefabGroups() {
		return objectPrefabGroups.Values.ToList();
	}

	public Dictionary<ObjectSubGroupEnum, ObjectPrefabSubGroup> objectPrefabSubGroups = new Dictionary<ObjectSubGroupEnum, ObjectPrefabSubGroup>();

	public class ObjectPrefabSubGroup {
		public readonly ObjectSubGroupEnum type;
		public readonly string name;

		public readonly ObjectGroupEnum groupType;
		public readonly List<ObjectPrefab> prefabs;

		public ObjectPrefabSubGroup(ObjectSubGroupEnum type, ObjectGroupEnum groupType, List<ObjectPrefab> prefabs) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;
			this.prefabs = prefabs;
		}
	}

	public ObjectPrefabSubGroup GetObjectPrefabSubGroupByEnum(ObjectSubGroupEnum objectSubGroupEnum) {
		return objectPrefabSubGroups[objectSubGroupEnum];
	}

	public class Variation {
		public ObjectPrefab prefab; // Can't be readonly because set after instantiation

		public readonly string name;
		public readonly List<ResourceAmount> uniqueResources;
		public readonly float walkSpeed;
		public readonly int integrity;
		public readonly float flammability;
		public readonly int timeToBuild;

		public readonly string instanceName;

		// Plants
		public readonly Dictionary<PlantPrefab, Resource> plants;

		public Variation(
			ObjectPrefab prefab,
			string name,
			List<ResourceAmount> uniqueResources,
			float walkSpeed,
			int integrity,
			float flammability,
			int timeToBuild,
			Dictionary<PlantPrefab, Resource> plants
		) {
			this.prefab = prefab;
			this.name = name;
			this.uniqueResources = uniqueResources;
			this.walkSpeed = walkSpeed;
			this.integrity = integrity;
			this.flammability = flammability;
			this.timeToBuild = timeToBuild;
			this.plants = plants;
		}

		public static bool Equals(Variation v1, Variation v2) {
			return ((v1 == null) && (v2 == null)) || (v1 == null || v2 == null) || (v1.name == v2.name);
		}

		public static readonly Variation nullVariation = new Variation(null, null, null, 0, 0, 0, 0, null);
	}

	public Dictionary<ObjectEnum, ObjectPrefab> objectPrefabs = new Dictionary<ObjectEnum, ObjectPrefab>();

	public class ObjectPrefab {
		public readonly ObjectEnum type;
		public readonly string name;

		public readonly ObjectGroupEnum groupType;
		public readonly ObjectSubGroupEnum subGroupType;

		public readonly ObjectInstanceType instanceType;

		public readonly int layer;

		public readonly bool bitmasking;
		public readonly bool canRotate;

		public readonly bool blocksLight;

		public readonly int integrity;

		public readonly Dictionary<int, List<Vector2>> multiTilePositions = new Dictionary<int, List<Vector2>>();
		public readonly Dictionary<int, Vector2> anchorPositionOffset = new Dictionary<int, Vector2>();
		public readonly Dictionary<int, Vector2> dimensions = new Dictionary<int, Vector2>();

		public readonly float walkSpeed;
		public readonly bool walkable;

		public readonly bool buildable;

		public readonly float flammability;

		// Container
		public readonly int maxInventoryWeight;
		public readonly int maxInventoryVolume;

		// Light Source
		public readonly int maxLightDistance;
		public readonly Color lightColour;

		// Sleep Spot
		public readonly float restComfortAmount;

		// Crafting Object
		public readonly bool usesFuel;

		// Farm
		public readonly int growthTimeDays;
		public readonly Resource seedResource;
		public readonly Resource harvestResource;

		// Job
		public readonly int timeToBuild;
		public readonly List<ResourceAmount> commonResources = new List<ResourceAmount>();
		public readonly List<Variation> variations = new List<Variation>();
		public readonly VariationNameOrderEnum variationNameOrder;
		public readonly List<JobManager.SelectionModifiersEnum> selectionModifiers = new List<JobManager.SelectionModifiersEnum>();
		public readonly JobManager.JobEnum jobType;
		public readonly bool addToTileWhenBuilt;

		// Sprites

		public enum SpriteType {
			Base,
			Bitmask,
			Active
		}

		public readonly Dictionary<Variation, Dictionary<SpriteType, List<Sprite>>> sprites = new Dictionary<Variation, Dictionary<SpriteType, List<Sprite>>>();

		// UI State
		public Variation lastSelectedVariation; // Used to show the most recently selected variation on the UI

		public ObjectPrefab(
			ObjectEnum type,
			ObjectGroupEnum groupType,
			ObjectSubGroupEnum subGroupType,
			ObjectInstanceType instanceType,
			int layer,
			bool bitmasking,
			bool blocksLight,
			int integrity,
			List<Vector2> multiTilePositions,
			float walkSpeed,
			bool walkable,
			bool buildable,
			float flammability,
			int maxInventoryVolume,
			int maxInventoryWeight,
			int maxLightDistance,
			Color lightColour,
			float restComfortAmount,
			bool usesFuel,
			int growthTimeDays,
			Resource seedResource,
			Resource harvestResource,
			int timeToBuild,
			List<ResourceAmount> commonResources,
			List<Variation> variations,
			VariationNameOrderEnum variationNameOrder,
			List<JobManager.SelectionModifiersEnum> selectionModifiers,
			JobManager.JobEnum jobType,
			bool addToTileWhenBuilt
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;
			this.subGroupType = subGroupType;

			this.instanceType = instanceType;

			this.layer = layer;

			this.bitmasking = bitmasking;

			this.blocksLight = blocksLight;

			this.integrity = integrity;

			this.walkable = walkable;
			this.walkSpeed = walkSpeed;

			this.buildable = buildable;

			this.flammability = flammability;

			// Container
			this.maxInventoryWeight = maxInventoryWeight;
			this.maxInventoryVolume = maxInventoryVolume;

			// Light
			this.maxLightDistance = maxLightDistance;
			this.lightColour = lightColour;

			// Sleep Spot
			this.restComfortAmount = restComfortAmount;

			// Crafting Object
			this.usesFuel = usesFuel;

			// Farm
			this.growthTimeDays = growthTimeDays;
			this.seedResource = seedResource;
			this.harvestResource = harvestResource;

			// Job
			this.timeToBuild = timeToBuild;
			this.commonResources = commonResources;
			this.variations = variations;
			this.variationNameOrder = variationNameOrder;
			this.selectionModifiers = selectionModifiers;
			this.jobType = jobType;
			this.addToTileWhenBuilt = addToTileWhenBuilt;

			// Sprites
			if (variations.Count == 0) {
				Dictionary<SpriteType, List<Sprite>> spriteGroups = new Dictionary<SpriteType, List<Sprite>> {
					{ SpriteType.Base, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + name + "/" + name.Replace(' ', '-') + "-base").ToList() },
					{ SpriteType.Bitmask, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + name + "/" + name.Replace(' ', '-') + "-bitmask").ToList() },
					{ SpriteType.Active, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + name + "/" + name.Replace(' ', '-') + "-active").ToList() }
				};
				sprites.Add(Variation.nullVariation, spriteGroups);
			} else {
				foreach (Variation variation in variations) {
					Dictionary<SpriteType, List<Sprite>> spriteGroups = new Dictionary<SpriteType, List<Sprite>> {
						{ SpriteType.Base, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + name + "/" + variation.name + "/" + (variationNameOrder == VariationNameOrderEnum.VariationObject ? variation.name + " " + name : name + " " + variation.name).Replace(' ', '-') + "-base").ToList() },
						{ SpriteType.Bitmask, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + name + "/" + variation.name + "/" + (variationNameOrder == VariationNameOrderEnum.VariationObject ? variation.name + " " + name : name + " " + variation.name).Replace(' ', '-') + "-bitmask").ToList() },
						{ SpriteType.Active, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + name + "/" + variation.name + "/" + (variationNameOrder == VariationNameOrderEnum.VariationObject ? variation.name + " " + name : name + " " + variation.name).Replace(' ', '-') + "-active").ToList() }
					};

					sprites.Add(variation, spriteGroups);
				}
			}

			bool allSpritesHaveBitmasking = true;
			int bitmaskSpritesCount = -1;

			foreach (Dictionary<SpriteType, List<Sprite>> spriteGroups in sprites.Values) {
				if (spriteGroups[SpriteType.Base].Count == 0 && spriteGroups[SpriteType.Bitmask].Count > 0) {
					spriteGroups[SpriteType.Base].Add(spriteGroups[SpriteType.Bitmask][0]);
				}

				if (jobType == JobManager.JobEnum.PlantFarm) {
					spriteGroups[SpriteType.Base] = new List<Sprite>() { harvestResource.image };
				}

				if (spriteGroups[SpriteType.Bitmask].Count == 0) {
					allSpritesHaveBitmasking = false;
				}

				if (bitmaskSpritesCount == -1 || bitmaskSpritesCount == spriteGroups[SpriteType.Bitmask].Count) {
					bitmaskSpritesCount = spriteGroups[SpriteType.Bitmask].Count;
				} else {
					foreach (Variation variation in variations) {
						Debug.Log(variation.name + " " + GetBitmaskSpritesForVariation(variation).Count);
					}
					Debug.LogError("Differing number of bitmask sprites between different variations on object: " + name);
				}
			}

			// Set Rotation
			canRotate = !bitmasking && allSpritesHaveBitmasking;

			// Multi Tile Positions
			float largestX = 0;
			float largestY = 0;

			if (multiTilePositions.Count <= 0) {
				multiTilePositions.Add(new Vector2(0, 0));
			}

			for (int i = 0; i < (allSpritesHaveBitmasking ? bitmaskSpritesCount : 1); i++) {
				if (i == 0 || i > 0 && canRotate) {
					this.multiTilePositions.Add(i, new List<Vector2>());
					if (i == 0) {
						this.multiTilePositions[i].AddRange(multiTilePositions);
					} else {
						foreach (Vector2 oldMultiTilePosition in this.multiTilePositions[i - 1]) {
							Vector2 newMultiTilePosition = new Vector2(oldMultiTilePosition.y, largestX - oldMultiTilePosition.x);
							this.multiTilePositions[i].Add(newMultiTilePosition);
						}
					}
					largestX = this.multiTilePositions[i].OrderByDescending(MTP => MTP.x).ToList()[0].x;
					largestY = this.multiTilePositions[i].OrderByDescending(MTP => MTP.y).ToList()[0].y;
					dimensions.Add(i, new Vector2(largestX + 1, largestY + 1));
					anchorPositionOffset.Add(i, new Vector2(largestX / 2, largestY / 2));
				}
			}

			lastSelectedVariation = variations.Count > 0 ? variations[0] : null;
		}

		public void SetVariation(Variation variation) {
			lastSelectedVariation = variation;
		}

		public Variation GetVariationFromString(string variationName) {
			return variations.Count > 0
				? variations.Find(v => v.name.Replace(" ", string.Empty).ToLower() == variationName.Replace(" ", string.Empty).ToLower())
				: null;
		}

		public string GetInstanceNameFromVariation(Variation variation) {
			return variation == null
				? name
				: (variation.prefab.variationNameOrder == VariationNameOrderEnum.ObjectVariation ? name : string.Empty)
					+ (string.IsNullOrEmpty(variation.name) || variation.prefab.variationNameOrder == VariationNameOrderEnum.VariationObject ? string.Empty : " ")
					+ variation.name
					+ (string.IsNullOrEmpty(variation.name) || variation.prefab.variationNameOrder == VariationNameOrderEnum.ObjectVariation ? string.Empty : " ")
					+ (variation.prefab.variationNameOrder == VariationNameOrderEnum.VariationObject ? name : string.Empty);
		}

		public Sprite GetBaseSpriteForVariation(Variation variation) {
			if (variation == null) {
				if (variations.Count > 0) {
					if (sprites[variations[0]][SpriteType.Base].Count <= 0) {
						return null;
					} else {
						return sprites[variations[0]][SpriteType.Base][0];
					}
				} else {
					if (sprites.ContainsKey(Variation.nullVariation)) {
						return sprites[Variation.nullVariation][SpriteType.Base][0];
					} else {
						return null;
					}
				}
			} else {
				if (sprites[variation][SpriteType.Base].Count <= 0) {
					return null;
				} else {
					return sprites[variation][SpriteType.Base][0];
				}
			}
		}

		public List<Sprite> GetBitmaskSpritesForVariation(Variation variation) {
			if (variation == null) {
				if (variations.Count > 0) {
					return sprites[variations[0]][SpriteType.Bitmask];
				} else {
					return sprites[Variation.nullVariation][SpriteType.Bitmask];
				}
			} else {
				return sprites[variation][SpriteType.Bitmask];
			}
		}

		public List<Sprite> GetActiveSpritesForVariation(Variation variation) {
			if (variation == null) {
				if (variations.Count > 0) {
					return sprites[variations[0]][SpriteType.Active];
				} else {
					return sprites[Variation.nullVariation][SpriteType.Active];
				}
			} else {
				return sprites[variation][SpriteType.Active];
			}
		}
	}

	public ObjectPrefab GetObjectPrefabByString(string objectString) {
		return GetObjectPrefabByEnum((ObjectEnum)Enum.Parse(typeof(ObjectEnum), objectString));
	}

	public ObjectPrefab GetObjectPrefabByEnum(ObjectEnum objectEnum) {
		return objectPrefabs[objectEnum];
	}

	public List<ObjectPrefab> GetObjectPrefabs() {
		return objectPrefabs.Values.ToList();
	}

	public Dictionary<ObjectPrefab, List<ObjectInstance>> objectInstances = new Dictionary<ObjectPrefab, List<ObjectInstance>>();

	public List<ObjectInstance> GetObjectInstancesByPrefab(ObjectPrefab prefab) {
		if (objectInstances.ContainsKey(prefab)) {
			return objectInstances[prefab];
		}
		return null;
	}

	public void AddObjectInstance(ObjectInstance objectInstance) {
		if (objectInstances.ContainsKey(objectInstance.prefab)) {
			objectInstances[objectInstance.prefab].Add(objectInstance);
			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Update, objectInstance.prefab);
		} else {
			objectInstances.Add(objectInstance.prefab, new List<ObjectInstance>() { objectInstance });
			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Add, objectInstance.prefab);
		}
	}

	public void RemoveObjectInstance(ObjectInstance instance) {
		switch (instance.prefab.instanceType) {
			case ObjectInstanceType.Normal:
				break;
			case ObjectInstanceType.Container:
				Container container = (Container)instance;

				if (GameManager.uiM.selectedContainer == container) {
					GameManager.uiM.SetSelectedContainer(null);
				}

				containers.Remove(container);
				break;
			case ObjectInstanceType.TradingPost:
				TradingPost tradingPost = (TradingPost)instance;

				if (GameManager.uiM.selectedTradingPost == tradingPost) {
					GameManager.uiM.SetSelectedTradingPost(null);
				}

				tradingPosts.Remove(tradingPost);
				break;
			case ObjectInstanceType.SleepSpot:
				SleepSpot sleepSpot = (SleepSpot)instance;

				sleepSpots.Remove(sleepSpot);
				break;
			case ObjectInstanceType.LightSource:
				LightSource lightSource = (LightSource)instance;

				lightSource.RemoveTileBrightnesses();

				lightSources.Remove(lightSource);
				break;
			case ObjectInstanceType.CraftingObject:
				CraftingObject craftingObject = (CraftingObject)instance;

				if (GameManager.uiM.selectedCraftingObject == craftingObject) {
					GameManager.uiM.SetSelectedCraftingObject(null);
				}

				craftingObjectInstances.Remove(craftingObject);
				break;
			case ObjectInstanceType.Farm:
				Farm farm = (Farm)instance;

				farm.tile.farm = null;

				farms.Remove(farm);
				break;
			default:
				Debug.LogWarning("No removal case for removing " + instance.prefab.name);
				break;
		}


		if (objectInstances.ContainsKey(instance.prefab)) {
			objectInstances[instance.prefab].Remove(instance);

			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Update, instance.prefab);
		} else {
			Debug.LogWarning("Tried removing a tile object instance which isn't in the list");
		}

		if (objectInstances[instance.prefab].Count <= 0) {
			objectInstances.Remove(instance.prefab);

			GameManager.uiM.ChangeObjectPrefabElements(UIManager.ChangeTypeEnum.Remove, instance.prefab);
		}
	}

	public ObjectInstance CreateObjectInstance(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex, bool addToList) {
		ObjectInstance instance = null;
		switch (prefab.instanceType) {
			case ObjectInstanceType.Normal:
				instance = new ObjectInstance(prefab, variation, tile, rotationIndex);
				break;
			case ObjectInstanceType.Container:
				instance = new Container(prefab, variation, tile, rotationIndex);
				containers.Add((Container)instance);
				break;
			case ObjectInstanceType.TradingPost:
				instance = new TradingPost(prefab, variation, tile, rotationIndex);
				tradingPosts.Add((TradingPost)instance);
				break;
			case ObjectInstanceType.SleepSpot:
				instance = new SleepSpot(prefab, variation, tile, rotationIndex);
				sleepSpots.Add((SleepSpot)instance);
				break;
			case ObjectInstanceType.LightSource:
				instance = new LightSource(prefab, variation, tile, rotationIndex);
				lightSources.Add((LightSource)instance);
				break;
			case ObjectInstanceType.CraftingObject:
				instance = new CraftingObject(prefab, variation, tile, rotationIndex);
				craftingObjectInstances.Add((CraftingObject)instance);
				break;
			case ObjectInstanceType.Farm:
				instance = new Farm(prefab, variation, tile);
				farms.Add((Farm)instance);
				tile.farm = (Farm)instance;
				break;
		}
		if (instance == null) {
			Debug.LogError("Instance is null for prefab " + (prefab != null ? prefab.name : "null") + " at tile " + tile.obj.transform.position);
		}
		if (addToList) {
			AddObjectInstance(instance);
		}
		return instance;
	}

	public class ObjectInstance : SelectableManager.ISelectable {

		public readonly TileManager.Tile tile; // The tile that this object covers that is closest to the zeroPointTile (usually they are the same tile)
		public readonly List<TileManager.Tile> additionalTiles = new List<TileManager.Tile>();
		public readonly TileManager.Tile zeroPointTile; // The tile representing the (0,0) position of the object even if the object doesn't cover it

		public readonly ObjectPrefab prefab;
		public readonly Variation variation;
		
		public readonly GameObject obj;
		public readonly SpriteRenderer sr;

		public readonly GameObject activeOverlay;
		public readonly SpriteRenderer aosr;

		public readonly int rotationIndex;

		public bool active;

		public float integrity;

		public bool visible;

		public ObjectInstance(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) {
			this.prefab = prefab;
			this.variation = variation;

			this.tile = tile;
			zeroPointTile = tile;
			foreach (Vector2 multiTilePosition in prefab.multiTilePositions[rotationIndex]) {
				TileManager.Tile additionalTile = zeroPointTile.map.GetTileFromPosition(zeroPointTile.obj.transform.position + (Vector3)multiTilePosition);
				additionalTiles.Add(additionalTile);
				if (additionalTile != zeroPointTile) {
					additionalTile.SetObjectInstanceReference(this);
				}
			}
			if (additionalTiles.Count > 0 && !additionalTiles.Contains(tile)) {
				tile = additionalTiles.OrderBy(additionalTile => Vector2.Distance(tile.obj.transform.position, additionalTile.obj.transform.position)).ToList()[0];
			} else if (additionalTiles.Count <= 0) {
				additionalTiles.Add(tile);
			}

			this.rotationIndex = rotationIndex;

			obj = MonoBehaviour.Instantiate(GameManager.resourceM.objectPrefab, zeroPointTile.obj.transform, false);
			sr = obj.GetComponent<SpriteRenderer>();
			obj.transform.position += (Vector3)prefab.anchorPositionOffset[rotationIndex];
			obj.name = "Tile Object Instance: " + prefab.name;
			sr.sortingOrder = 1 + prefab.layer; // Tile Object Sprite
			sr.sprite = prefab.GetBaseSpriteForVariation(variation);

			activeOverlay = obj.transform.Find("ActiveOverlay").gameObject;
			aosr = activeOverlay.GetComponent<SpriteRenderer>();
			aosr.sortingOrder = sr.sortingOrder + 1;

			if (prefab.blocksLight) {
				foreach (LightSource lightSource in GameManager.resourceM.lightSources) {
					foreach (TileManager.Tile objectTile in additionalTiles) {
						if (lightSource.litTiles.Contains(objectTile)) {
							lightSource.SetTileBrightnesses();
						}
					}
				}
			}

			SetColour(tile.sr.color);

			integrity = prefab.integrity;
		}

		public void FinishCreation() {
			List<TileManager.Tile> bitmaskingTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile additionalTile in additionalTiles) {
				bitmaskingTiles.Add(additionalTile);
				bitmaskingTiles.AddRange(additionalTile.surroundingTiles);
			}
			bitmaskingTiles = bitmaskingTiles.Distinct().ToList();
			GameManager.resourceM.Bitmask(bitmaskingTiles);
			GameManager.colonyM.colony.map.Bitmasking(bitmaskingTiles, true, true);
			foreach (TileManager.Tile tile in additionalTiles) {
				SetColour(tile.sr.color);
			}
		}

		public void SetColour(Color newColour) {
			sr.color = new Color(newColour.r, newColour.g, newColour.b, 1f);
		}

		public void SetActiveSprite(JobManager.Job job, bool jobActive) {
			if (active && jobActive) {
				if (prefab.GetActiveSpritesForVariation(variation).Count > 0) {
					if (prefab.type == ObjectEnum.SplittingBlock) {
						int customActiveSpriteIndex = 0;
						if (job.createResource.resource.type == ResourceEnum.Wood) {
							customActiveSpriteIndex = 0;
						} else if (job.createResource.resource.type == ResourceEnum.Firewood) {
							customActiveSpriteIndex = 1;
						}
						aosr.sprite = prefab.GetActiveSpritesForVariation(variation)[4 * customActiveSpriteIndex + rotationIndex];
					} else {
						aosr.sprite = prefab.GetActiveSpritesForVariation(variation)[rotationIndex];
					}
				}
			} else {
				aosr.sprite = GameManager.resourceM.clearSquareSprite;
			}
		}

		public virtual void SetActive(bool active) {
			this.active = active;
		}

		public virtual void Update() {

		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);
		}

		void SelectableManager.ISelectable.Select() {
			
		}

		void SelectableManager.ISelectable.Deselect() {
			
		}
	}

	public List<Container> containers = new List<Container>();

	public List<Container> GetContainersInRegion(TileManager.Map.Region region) {
		return containers.Where(c => c.tile.region == region).ToList();
	}

	public Container GetContainerOrChildOnTile(TileManager.Tile tile) {
		Container container = null;
		if (container == null) {
			container = containers.Find(c => c.tile == tile);
		}
		if (container == null) {
			container = GameManager.resourceM.tradingPosts.Find(tp => tp.tile == tile);
		}
		return container;
	}

	public class Container : ObjectInstance, IInventory {

		private readonly Inventory inventory;

		public Container(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {
			inventory = new Inventory(this, prefab.maxInventoryWeight, prefab.maxInventoryVolume);
		}

		public Inventory GetInventory() {
			return inventory;
		}
	}

	public List<TradingPost> tradingPosts = new List<TradingPost>();

	public List<TradingPost> GetTradingPostsInRegion(TileManager.Map.Region region) {
		return tradingPosts.Where(tp => tp.tile.region == region).ToList();
	}

	public List<ResourceAmount> GetAvailableResourcesInTradingPostsInRegion(TileManager.Map.Region region) {
		List<ResourceAmount> availableResources = new List<ResourceAmount>();
		foreach (TradingPost tradingPost in GetTradingPostsInRegion(region)) {
			foreach (ResourceAmount resourceAmount in tradingPost.GetInventory().resources) {
				ResourceAmount accessibleResource = availableResources.Find(ra => ra.resource == resourceAmount.resource);
				if (accessibleResource != null) {
					accessibleResource.amount += resourceAmount.amount;
				} else {
					availableResources.Add(new ResourceAmount(resourceAmount.resource, resourceAmount.amount));
				}
			}
		}
		return availableResources;
	}

	public class TradingPost : Container {

		public List<ResourceAmount> targetResourceAmounts = new List<ResourceAmount>();

		public TradingPost(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {

		}
	}

	public List<SleepSpot> sleepSpots = new List<SleepSpot>();

	public class SleepSpot : ObjectInstance {

		public ColonistManager.Colonist occupyingColonist;

		public SleepSpot(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {

		}

		public void StartSleeping(ColonistManager.Colonist colonist) {
			occupyingColonist = colonist;
		}

		public void StopSleeping() {
			occupyingColonist = null;
		}
	}

	public List<CraftingObject> craftingObjectInstances = new List<CraftingObject>();

	public class Priority {

		public readonly int min;
		public readonly int max;

		private int priority = 0;

		public Priority(int priority = 0, int min = 0, int max = 9) {
			this.priority = priority;
			this.min = min;
			this.max = max;
		}

		public int Set(int priority) {
			if (priority > max) {
				priority = min;
			} else if (priority < min) {
				priority = max;
			}

			this.priority = priority;
			
			return this.priority;
		}

		public int Change(int amount) {
			return Set(Get() + amount);
		}

		public int Get() {
			return priority;
		}
	}

	public class PriorityResourceInstance {
		public Resource resource;

		public readonly static int priorityMax = 9;
		public Priority priority;

		public PriorityResourceInstance(Resource resource, int priority) {
			this.resource = resource;
			
			this.priority = new Priority(priority);
		}
	}

	public enum CreationMethod {
		SingleRun,
		MaintainStock,
		ContinuousRun
	}

	public class CraftableResourceInstance {

		public Resource resource;

		public Priority priority;

		public CreationMethod creationMethod;
		private int targetAmount;
		private int remainingAmount;

		public CraftingObject craftingObject;

		public bool enableable = false;
		public List<ResourceAmount> fuelAmounts = new List<ResourceAmount>();
		public JobManager.Job job = null;

		public CraftableResourceInstance(
			Resource resource,
			int priority,
			CreationMethod creationMethod,
			int targetAmount,
			CraftingObject craftingObject,
			int? remainingAmount = null
		) {
			this.resource = resource;
			
			this.priority = new Priority(priority);
			
			this.creationMethod = creationMethod;
			this.targetAmount = targetAmount;
			this.remainingAmount = remainingAmount == null ? targetAmount : remainingAmount.Value;
			
			this.craftingObject = craftingObject;
		}

		public void SetTargetAmount(int targetAmount) {
			remainingAmount = targetAmount;
			this.targetAmount = targetAmount;
		}

		public void UpdateTargetAmount(int targetAmount) {
			remainingAmount += remainingAmount == 0 ? targetAmount : targetAmount - this.targetAmount;
			this.targetAmount = targetAmount;
		}

		public int GetTargetAmount() {
			return targetAmount;
		}

		public int GetRemainingAmount() {
			return remainingAmount;
		}

		public void SetRemainingAmount(int remainingAmount) {
			this.remainingAmount = remainingAmount;
		}

		public void ResetAmounts() {
			SetRemainingAmount(0);
			SetTargetAmount(0);
		}
	}

	public class CraftingObject : ObjectInstance {

		public List<CraftableResourceInstance> resources = new List<CraftableResourceInstance>();
		public List<PriorityResourceInstance> fuels = new List<PriorityResourceInstance>();

		public CraftingObject(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {

		}

		public override void Update() {
			base.Update();

			if (active) {
				foreach (CraftableResourceInstance resource in resources) {

					// Not enableable if job already exists
					if (resource.job != null) {
						continue;
					}

					// Assume enableable is true by default
					resource.enableable = true;

					// Not enableable if don't have enough required resources
					foreach (ResourceAmount resourceAmount in resource.resource.craftingResources) {
						if (resourceAmount.resource.GetAvailableAmount() < resourceAmount.amount) {
							resource.enableable = false;
							break;
						}
					}
					if (!resource.enableable) {
						continue;
					}

					// Not enableable if don't have enough fuel
					if (resource.resource.craftingEnergy != 0) {
						int remainingCraftingEnergy = resource.resource.craftingEnergy;
						foreach (PriorityResourceInstance fuel in fuels.OrderBy(f => f.priority.Get())) {
							if ((fuel.resource.GetAvailableAmount() * fuel.resource.fuelEnergy) >= remainingCraftingEnergy) {
								ResourceAmount fuelResourceAmount = new ResourceAmount(fuel.resource, Mathf.CeilToInt(remainingCraftingEnergy / (float)fuel.resource.fuelEnergy));
								resource.fuelAmounts.Add(fuelResourceAmount);
								remainingCraftingEnergy = 0;
							} else if (fuel.resource.GetAvailableAmount() > 0) {
								ResourceAmount fuelResourceAmount = new ResourceAmount(fuel.resource, fuel.resource.GetAvailableAmount());
								resource.fuelAmounts.Add(fuelResourceAmount);
								remainingCraftingEnergy -= fuel.resource.GetAvailableAmount() * fuel.resource.fuelEnergy;
							}
							if (remainingCraftingEnergy <= 0) {
								break;
							}
						}
						if (remainingCraftingEnergy > 0) {
							resource.enableable = false;
							resource.fuelAmounts.Clear();
							continue;
						}
					}

					if (resource.enableable) {
						switch (resource.creationMethod) {
							case CreationMethod.SingleRun:
								if (resource.GetRemainingAmount() > 0) {
									resource.job = GameManager.resourceM.CreateResource(resource, this);
								}
								break;
							case CreationMethod.MaintainStock:
								if (resource.GetTargetAmount() > resource.resource.GetAvailableAmount()) {
									resource.job = GameManager.resourceM.CreateResource(resource, this);
								}
								break;
							case CreationMethod.ContinuousRun:
								resource.job = GameManager.resourceM.CreateResource(resource, this);
								break;
						}
					}

					if (resource.fuelAmounts.Count > 0) {
						resource.fuelAmounts.Clear();
					}
				}
			}
		}

		public override void SetActive(bool active) {
			base.SetActive(active);

			if (!active) {
				foreach (CraftableResourceInstance resource in resources) {
					if (resource.job != null) {
						GameManager.jobM.CancelJob(resource.job);
						resource.job = null;
					}
				}
			}
		}

		public CraftableResourceInstance ToggleResource(Resource resource, int priority) {
			CraftableResourceInstance existingResource = resources.Find(r => r.resource == resource);
			if (existingResource == null) {
				existingResource = new CraftableResourceInstance(resource, priority, CreationMethod.MaintainStock, 0, this);
				resources.Add(existingResource);
			} else {
				resources.Remove(existingResource);
				if (existingResource.job != null) {
					GameManager.jobM.CancelJob(existingResource.job);
					existingResource.job = null;
				}
				existingResource = null;
			}
			return existingResource;
		}

		public PriorityResourceInstance ToggleFuel(Resource fuel, int priority) {
			PriorityResourceInstance existingFuel = fuels.Find(f => f.resource == fuel);
			if (existingFuel == null) {
				existingFuel = new PriorityResourceInstance(fuel, priority);
				fuels.Add(existingFuel);
			} else {
				fuels.Remove(existingFuel);
				foreach (CraftableResourceInstance resource in resources) {
					if (resource.fuelAmounts.Find(ra => ra.resource == fuel) != null) {
						if (resource.job != null) {
							GameManager.jobM.CancelJob(resource.job);
							resource.job = null;
						}
					}
				}
				existingFuel = null;
			}
			return existingFuel;
		}

		public CraftableResourceInstance GetCraftableResourceFromResource(Resource resource) {
			return resources.Find(r => r.resource == resource);
		}

		public PriorityResourceInstance GetFuelFromFuelResource(Resource resource) {
			return fuels.Find(f => f.resource == resource);
		}
	}

	public List<LightSource> lightSources = new List<LightSource>();

	public class LightSource : ObjectInstance {

		public List<TileManager.Tile> litTiles = new List<TileManager.Tile>();

		public LightSource(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {
			SetTileBrightnesses();
		}

		public void SetTileBrightnesses() {
			RemoveTileBrightnesses();
			List<TileManager.Tile> newLitTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles) {
				float distance = Vector2.Distance(tile.obj.transform.position, this.tile.obj.transform.position);
				if (distance <= prefab.maxLightDistance) {
					float intensityAtTile = Mathf.Clamp(prefab.maxLightDistance * (1f / Mathf.Pow(distance, 2f)), 0f, 1f);
					if (tile != this.tile) {
						bool lightTile = true;
						Vector3 lightVector = obj.transform.position;
						while ((obj.transform.position - lightVector).magnitude <= distance) {
							TileManager.Tile lightVectorTile = GameManager.colonyM.colony.map.GetTileFromPosition(lightVector);
							if (lightVectorTile != this.tile) {
								if (lightVectorTile.blocksLight/*GameManager.colonyM.colony.map.TileBlocksLight(lightVectorTile)*/) {
									/*
									if (!lightVectorTile.horizontalSurroundingTiles.Any(t => newLitTiles.Contains(t) && !tileM.map.TileBlocksLight(t))) {
										lightTile = false;
										break;
									}
									*/
									lightTile = false;
									break;
								}
							}
							lightVector += (tile.obj.transform.position - obj.transform.position).normalized * 0.1f;
						}
						if (lightTile) {
							tile.AddLightSourceBrightness(this, intensityAtTile);
							newLitTiles.Add(tile);
						}
					} else {
						this.tile.AddLightSourceBrightness(this, intensityAtTile);
					}
				}
			}
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
			litTiles.AddRange(newLitTiles);
		}

		public void RemoveTileBrightnesses() {
			foreach (TileManager.Tile tile in litTiles) {
				tile.RemoveLightSourceBrightness(this);
			}
			litTiles.Clear();
			tile.RemoveLightSourceBrightness(this);
			GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
		}
	}

	public enum PlantGroupPropertyEnum {
		PlantGroup,
		Type,
		Plants
	}

	public enum PlantPropertyEnum {
		Plant,
		Type,
		Living,
		Integrity,
		Seed,
		PlantJob,
		ReturnResources,
		HarvestResources
	}

	public enum PlantGroupEnum {
		None,
		Cactus,
		Tree,
		Bush
	};

	public enum PlantEnum {
		None,
		Cactus,
		SnowTree,
		ThinTree,
		WideTree,
		PalmTree,
		DeadTree,
		Bush
	};

	public static readonly Dictionary<ResourceEnum, ResourceEnum> seedToHarvestResource = new Dictionary<ResourceEnum, ResourceEnum>() {
		{ ResourceEnum.AppleSeed, ResourceEnum.Apple },
		{ ResourceEnum.Blueberry, ResourceEnum.Blueberry }
	};

	public void CreatePlantPrefabs() {
		List<KeyValuePair<string, object>> plantGroupProperties = PersistenceManager.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/plants").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> plantGroupProperty in plantGroupProperties) {
			switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupProperty.Key)) {
				case PlantGroupPropertyEnum.PlantGroup:

					PlantGroupEnum? groupType = null;
					List<PlantPrefab> prefabs = new List<PlantPrefab>();

					foreach (KeyValuePair<string, object> plantGroupSubProperty in (List<KeyValuePair<string, object>>)plantGroupProperty.Value) {
						switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupSubProperty.Key)) {
							case PlantGroupPropertyEnum.Type:
								groupType = (PlantGroupEnum)Enum.Parse(typeof(PlantGroupEnum), (string)plantGroupSubProperty.Value);
								break;
							case PlantGroupPropertyEnum.Plants:
								foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)plantGroupSubProperty.Value) {
									switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantProperty.Key)) {
										case PlantPropertyEnum.Plant:

											PlantEnum? type = null;
											bool? living = null;
											int? integrity = null;
											Resource seed = null;
											List<ResourceRange> returnResources = new List<ResourceRange>();
											List<ResourceRange> harvestResources = new List<ResourceRange>();

											foreach (KeyValuePair<string, object> plantSubProperty in (List<KeyValuePair<string, object>>)plantProperty.Value) {
												switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantSubProperty.Key)) {
													case PlantPropertyEnum.Type:
														type = (PlantEnum)Enum.Parse(typeof(PlantEnum), (string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.Living:
														living = bool.Parse((string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.Integrity:
														integrity = int.Parse((string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.Seed:
														seed = GetResourceByString((string)plantSubProperty.Value);
														break;
													case PlantPropertyEnum.ReturnResources:
														foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
															Resource resource = GameManager.resourceM.GetResourceByString(resourceRangeString.Split(':')[0]);
															int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
															int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
															returnResources.Add(new ResourceRange(resource, min, max));
														}
														break;
													case PlantPropertyEnum.HarvestResources:
														foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
															Resource resource = GameManager.resourceM.GetResourceByString(resourceRangeString.Split(':')[0]);
															int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
															int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
															harvestResources.Add(new ResourceRange(resource, min, max));
														}
														break;
													default:
														Debug.LogError("Unknown plant sub property: " + plantSubProperty.Key + " " + plantSubProperty.Value);
														break;
												}
											}

											PlantPrefab plantPrefab = new PlantPrefab(
												type.Value,
												groupType.Value,
												living.Value,
												integrity.Value,
												seed,
												returnResources,
												harvestResources
											);
											prefabs.Add(plantPrefab);
											plantPrefabs.Add(type.Value, plantPrefab);

											break;
										default:
											Debug.LogError("Unknown plant property: " + plantProperty.Key + " " + plantProperty.Value);
											break;
									}
								}
								break;
							default:
								Debug.LogError("Unknown plant group sub property: " + plantGroupSubProperty.Key + " " + plantGroupSubProperty.Value);
								break;
						}

					}

					PlantGroup plantGroup = new PlantGroup(
						groupType.Value,
						prefabs
					);
					plantGroups.Add(groupType.Value, plantGroup);

					break;
				default:
					Debug.LogError("Unknown plant group property: " + plantGroupProperty.Key + " " + plantGroupProperty.Value);
					break;

			}
		}
	}

	public Dictionary<PlantGroupEnum, PlantGroup> plantGroups = new Dictionary<PlantGroupEnum, PlantGroup>();

	public class PlantGroup {
		public readonly PlantGroupEnum type;
		public readonly string name;

		public readonly List<PlantPrefab> prefabs;

		public PlantGroup(
			PlantGroupEnum type,
			List<PlantPrefab> prefabs
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.prefabs = prefabs;
		}
	}

	public PlantGroup GetPlantGroupByEnum(PlantGroupEnum plantGroupEnum) {
		return plantGroups[plantGroupEnum];
	}

	public Dictionary<PlantEnum, PlantPrefab> plantPrefabs = new Dictionary<PlantEnum, PlantPrefab>();

	public class PlantPrefab {
		public readonly PlantEnum type;
		public readonly string name;

		public readonly PlantGroupEnum groupType;

		public readonly Resource seed;

		public readonly bool living;

		public readonly int integrity;

		public readonly List<ResourceRange> returnResources;

		public readonly List<ResourceRange> harvestResources;

		public readonly List<Sprite> smallSprites;
		public readonly List<Sprite> fullSprites;
		public readonly Dictionary<Resource, Dictionary<bool, List<Sprite>>> harvestResourceSprites = new Dictionary<Resource, Dictionary<bool, List<Sprite>>>();

		public PlantPrefab(
			PlantEnum type,
			PlantGroupEnum groupType,
			bool living,
			int integrity,
			Resource seed,
			List<ResourceRange> returnResources,
			List<ResourceRange> harvestResources
		) {
			this.type = type;
			name = UIManager.SplitByCapitals(type.ToString());

			this.groupType = groupType;

			this.living = living;

			this.integrity = integrity;

			this.seed = seed;

			this.returnResources = returnResources;

			this.harvestResources = harvestResources;

			smallSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + type.ToString() + "-small").ToList();
			fullSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + type.ToString() + "-full").ToList();

			foreach (Resource harvestResource in harvestResources.Select(rr => rr.resource)) {
				Dictionary<bool, List<Sprite>> foundSpriteSizes = new Dictionary<bool, List<Sprite>>();

				List<Sprite> smallResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + harvestResource.type.ToString() + "/" + type.ToString() + "-small-" + harvestResource.type.ToString().ToLower()).ToList();
				if (smallResourceSprites != null && smallResourceSprites.Count > 0) {
					foundSpriteSizes.Add(true, smallResourceSprites);
				}

				List<Sprite> fullResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + harvestResource.type.ToString() + "/" + type.ToString() + "-full-" + harvestResource.type.ToString().ToLower()).ToList();
				if (fullResourceSprites != null && fullResourceSprites.Count > 0) {
					foundSpriteSizes.Add(false, fullResourceSprites);
				}

				if (foundSpriteSizes.Count > 0) {
					harvestResourceSprites.Add(harvestResource, foundSpriteSizes);
				}
			}
		}
	}

	public PlantPrefab GetPlantPrefabByString(string plantString) {
		return GetPlantPrefabByEnum((PlantEnum)Enum.Parse(typeof(PlantEnum), plantString));
	}

	public PlantPrefab GetPlantPrefabByEnum(PlantEnum plantEnum) {
		return plantPrefabs[plantEnum];
	}

	public List<PlantPrefab> GetPlantPrefabs() {
		return plantPrefabs.Values.ToList();
	}

	public PlantPrefab GetPlantPrefabByBiome(TileManager.Biome biome, bool guaranteedTree) {
		if (guaranteedTree) {
			List<PlantEnum> biomePlantEnums = biome.plantChances.Keys.Where(plantEnum => plantEnum != PlantEnum.DeadTree).ToList();
			if (biomePlantEnums.Count > 0) {
				return GetPlantPrefabByEnum(biomePlantEnums[UnityEngine.Random.Range(0, biomePlantEnums.Count)]);
			} else {
				return null;
			}
		} else {
			foreach (KeyValuePair<PlantEnum, float> plantChancesKVP in biome.plantChances) {
				if (UnityEngine.Random.Range(0f, 1f) < biome.plantChances[plantChancesKVP.Key]) {
					return GetPlantPrefabByEnum(plantChancesKVP.Key);
				}
			}
		}
		return null;
	}

	public List<Plant> smallPlants = new List<Plant>();

	public class Plant {
		public readonly string name;

		public readonly PlantPrefab prefab;
		public readonly TileManager.Tile tile;

		public readonly GameObject obj;
		public readonly SpriteRenderer sr;

		public float integrity;

		public bool small;

		public float growthProgress = 0;

		public readonly Resource harvestResource;

		public bool visible;

		public Plant(PlantPrefab prefab, TileManager.Tile tile, bool? small, bool randomHarvestResource, Resource specificHarvestResource) {
			this.prefab = prefab;
			this.tile = tile;

			integrity = prefab.integrity;

			this.small = small ?? UnityEngine.Random.Range(0, 100) <= 10;
			if (this.small) {
				GameManager.resourceM.smallPlants.Add(this);
			}

			harvestResource = null;
			if (randomHarvestResource && prefab.harvestResources.Count > 0) {
				if (UnityEngine.Random.Range(0, 100) <= 5) {
					List<ResourceAmount> resourceChances = new List<ResourceAmount>();
					foreach (Resource harvestResource in prefab.harvestResources.Select(rr => rr.resource)) {
						resourceChances.Add(new ResourceAmount(harvestResource, UnityEngine.Random.Range(0, 100)));
					}
					harvestResource = resourceChances.OrderByDescending(ra => ra.amount).First().resource;
				}
			}
			if (specificHarvestResource != null) {
				harvestResource = specificHarvestResource;
			}

			obj = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, tile.obj.transform.position, Quaternion.identity);
			sr = obj.GetComponent<SpriteRenderer>();
			if (harvestResource != null) {
				name = harvestResource.name + " " + prefab.name;
				sr.sprite = prefab.harvestResourceSprites[harvestResource][this.small][UnityEngine.Random.Range(0, prefab.harvestResourceSprites[harvestResource][this.small].Count)];
			} else {
				name = prefab.name;
				sr.sprite = this.small ? prefab.smallSprites[UnityEngine.Random.Range(0, prefab.smallSprites.Count)] : prefab.fullSprites[UnityEngine.Random.Range(0, prefab.fullSprites.Count)];
			}
			sr.sortingOrder = 1; // Plant Sprite

			obj.name = "Plant: " + prefab.name + " " + sr.sprite.name;
			obj.transform.parent = tile.obj.transform;
		}

		public void Grow() {
			small = false;
			if (harvestResource != null) {
				sr.sprite = prefab.harvestResourceSprites[harvestResource][small][UnityEngine.Random.Range(0, prefab.harvestResourceSprites[harvestResource][small].Count)];
			} else {
				sr.sprite = prefab.fullSprites[UnityEngine.Random.Range(0, prefab.fullSprites.Count)];
			}
			GameManager.resourceM.smallPlants.Remove(this);
		}

		public void Remove() {
			MonoBehaviour.Destroy(obj);
			GameManager.resourceM.smallPlants.Remove(this);
		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);
		}
	}

	public void GrowPlants() {
		if (!GameManager.timeM.GetPaused() && GameManager.timeM.minuteChanged) {
			List<Plant> growPlants = new List<Plant>();
			foreach (Plant plant in smallPlants) {
				plant.growthProgress += 1;
				if (plant.growthProgress > TimeManager.dayLengthSeconds * 4) {
					if (UnityEngine.Random.Range(0, 100) < (0.01f * (plant.growthProgress / TimeManager.dayLengthSeconds * 4))) {
						growPlants.Add(plant);
					}
				}
			}
			foreach (Plant plant in growPlants) {
				plant.Grow();
			}
		}
	}

	public static readonly Dictionary<ResourceEnum, ResourceEnum> farmSeedToReturnResource = new Dictionary<ResourceEnum, ResourceEnum>() {
		{ ResourceEnum.WheatSeed, ResourceEnum.Wheat },
		{ ResourceEnum.Potato, ResourceEnum.Potato },
		{ ResourceEnum.CottonSeed, ResourceEnum.Cotton }
	};
	public static readonly Dictionary<ResourceEnum, ObjectEnum> farmSeedToObject = new Dictionary<ResourceEnum, ObjectEnum>() {
		{ ResourceEnum.WheatSeed, ObjectEnum.WheatFarm },
		{ ResourceEnum.Potato, ObjectEnum.PotatoFarm },
		{ ResourceEnum.CottonSeed, ObjectEnum.CottonFarm }
	};

	public List<Farm> farms = new List<Farm>();

	public class Farm : ObjectInstance {

		public readonly string name;

		public float growTimer = 0;

		public int growProgressSpriteIndex = -1;
		public readonly List<Sprite> growProgressSprites = new List<Sprite>();
		public readonly int maxSpriteIndex = 0;

		private readonly float precipitationGrowthMultiplier;
		private readonly float temperatureGrowthMultipler;

		public Farm(ObjectPrefab prefab, Variation variation, TileManager.Tile tile) : base(prefab, variation, tile, 0) {
			name = prefab.harvestResource.name + " Farm";

			growProgressSprites = prefab.GetBitmaskSpritesForVariation(variation);
			maxSpriteIndex = growProgressSprites.Count - 1;

			precipitationGrowthMultiplier = CalculatePrecipitationGrowthMultiplierForTile(tile);
			temperatureGrowthMultipler = CalculateTemperatureGrowthMultiplierForTile(tile);

			Update();
		}

		public override void Update() {
			base.Update();

			if (growTimer >= prefab.growthTimeDays * TimeManager.dayLengthSeconds) {
				if (!GameManager.jobM.JobOfTypeExistsAtTile(JobManager.JobEnum.HarvestFarm, tile)) {
					GameManager.jobM.CreateJob(new JobManager.Job(tile, GameManager.resourceM.GetObjectPrefabByEnum(ObjectEnum.HarvestFarm), null, 0));
				}
			} else {
				growTimer += CalculateGrowthRate();

				int newGrowProgressSpriteIndex = Mathf.FloorToInt((growTimer / ((prefab.growthTimeDays * TimeManager.dayLengthSeconds) + 10)) * growProgressSprites.Count);
				if (newGrowProgressSpriteIndex != growProgressSpriteIndex) {
					growProgressSpriteIndex = newGrowProgressSpriteIndex;
					obj.GetComponent<SpriteRenderer>().sprite = growProgressSprites[Mathf.Clamp(growProgressSpriteIndex, 0, maxSpriteIndex)];
				}
			}
		}

		public float CalculateGrowthRate() {
			float growthRate = GameManager.timeM.deltaTime;
			growthRate *= Mathf.Max(GameManager.colonyM.colony.map.CalculateBrightnessLevelAtHour(GameManager.timeM.tileBrightnessTime), tile.lightSourceBrightness);
			growthRate *= precipitationGrowthMultiplier;
			growthRate *= temperatureGrowthMultipler;
			growthRate = Mathf.Clamp(growthRate, 0, 1);
			return growthRate;
		}

		public static float CalculatePrecipitationGrowthMultiplierForTile(TileManager.Tile tile) {
			return Mathf.Min((-2 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 2) + 1), (-30 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 3) + 1));
		}

		public static float CalculateTemperatureGrowthMultiplierForTile(TileManager.Tile tile) {
			return Mathf.Clamp(Mathf.Min(((tile.temperature - 10) / 15f + 1), (-((tile.temperature - 50) / 20f))), 0, 1);
		}
	}

	public List<string> locationNames = new List<string>();

	public void LoadLocationNames() {
		locationNames = Resources.Load<TextAsset>(@"Data/names-locations").text.Split('\n').Select(s => UIManager.RemoveNonAlphanumericChars(s)).ToList();
	}

	public string GetRandomLocationName() {
		List<string> filteredLocationNames = locationNames.Where(ln =>
				(GameManager.universeM.universe == null || ln != GameManager.universeM.universe.name)
			&&	(GameManager.planetM.planet == null || ln != GameManager.planetM.planet.name)
			&&	(GameManager.colonyM.colony == null || ln != GameManager.colonyM.colony.name)
			&&	GameManager.caravanM.caravans.Find(c => c.location.name == ln) == null
		).ToList();
		
		return filteredLocationNames[UnityEngine.Random.Range(0, filteredLocationNames.Count)];
	}

	public int BitSumObjects(List<ObjectEnum> compareObjectTypes, List<TileManager.Tile> tileSurroundingTiles) {
		List<int> layers = new List<int>();
		foreach (TileManager.Tile tile in tileSurroundingTiles) {
			if (tile != null) {
				foreach (KeyValuePair<int, ObjectInstance> kvp in tile.objectInstances) {
					if (!layers.Contains(kvp.Key)) {
						layers.Add(kvp.Key);
					}
				}
			}
		}
		layers.Sort();

		Dictionary<int, List<int>> layersSumTiles = new Dictionary<int, List<int>>();
		foreach (int layer in layers) {
			List<int> layerSumTiles = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0 };
			for (int i = 0; i < tileSurroundingTiles.Count; i++) {
				if (tileSurroundingTiles[i] != null && tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer) != null) {
					if (compareObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type)) {
						bool ignoreTile = false;
						if (compareObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type) && TileManager.Map.diagonalCheckMap.ContainsKey(i)) {
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]], tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
							List<TileManager.Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							layerSumTiles[i] = 1;
						}
					}
				} else {
					if (tileSurroundingTiles.Find(tile => tile != null && tileSurroundingTiles.IndexOf(tile) <= 3 && tile.GetObjectInstanceAtLayer(layer) != null && !compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
						layerSumTiles[i] = 1;
					} else {
						if (i <= 3) {
							layerSumTiles[i] = 1;
						} else {
							List<TileManager.Tile> surroundingHorizontalTiles = new List<TileManager.Tile>() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]], tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && !compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
								layerSumTiles[i] = 1;
							}
						}
					}
				}
			}
			layersSumTiles.Add(layer, layerSumTiles);
		}

		List<bool> sumTiles = new List<bool>() { false, false, false, false, false, false, false, false };

		foreach (KeyValuePair<int, List<int>> layerSumTiles in layersSumTiles) {
			foreach (ObjectEnum objectEnum in compareObjectTypes) {
				ObjectPrefab objectPrefab = GetObjectPrefabByEnum(objectEnum);
				if (objectPrefab.layer == layerSumTiles.Key) {
					foreach (TileManager.Tile tile in tileSurroundingTiles) {
						if (tile != null) {
							ObjectInstance objectInstance = tile.GetAllObjectInstances().Find(instances => instances.prefab == objectPrefab);
							if (objectInstance != null) {
								if (layerSumTiles.Value[tileSurroundingTiles.IndexOf(tile)] > 0) {
									sumTiles[tileSurroundingTiles.IndexOf(tile)] = true;
								}
							}
						}
					}
				}
			}
		}

		int sum = 0;

		for (int i = 0; i < sumTiles.Count; i++) {
			if (sumTiles[i]) {
				sum += Mathf.RoundToInt(Mathf.Pow(2, i));
			}
		}

		return sum;
	}

	private void BitmaskObjects(
		ObjectInstance objectInstance, 
		bool includeDiagonalSurroundingTiles, 
		bool customBitSumInputs, 
		bool compareEquivalentObjects, 
		List<ObjectEnum> customCompareObjectTypes
	) {
		List<TileManager.Tile> surroundingTilesToUse = includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles;

		int sum = 0;
		if (customBitSumInputs) {
			sum = BitSumObjects(
				customCompareObjectTypes,
				surroundingTilesToUse
			);
		} else {
			if (compareEquivalentObjects) {
				if (objectInstance.prefab.subGroupType == ObjectSubGroupEnum.Walls) {
					sum = BitSumObjects(
						new List<ObjectEnum>() { objectInstance.prefab.type },
						surroundingTilesToUse
					);
					// Not-fully-working implementation of walls and stone connecting
					//sum += GameManager.colonyM.colony.map.BitSum(
					//	TileManager.TileTypeGroup.GetTileTypeGroupByEnum(TileManager.TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tileType => tileType.type).ToList(),
					//	new List<ObjectEnum>() { objectInstance.prefab.type },
					//	surroundingTilesToUse,
					//	true
					//);
				} else {
					sum = BitSumObjects(
						new List<ObjectEnum>() { objectInstance.prefab.type },
						surroundingTilesToUse
					);
				}
			} else {
				sum = BitSumObjects(
					new List<ObjectEnum>() { objectInstance.prefab.type },
					surroundingTilesToUse
				);
			}
		}
		SpriteRenderer oISR = objectInstance.obj.GetComponent<SpriteRenderer>();
		if (sum >= 16) {
			oISR.sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[TileManager.Map.bitmaskMap[sum]];
		} else {
			oISR.sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[sum];
		}
	}

	public void Bitmask(List<TileManager.Tile> tilesToBitmask) {
		foreach (TileManager.Tile tile in tilesToBitmask) {
			if (tile != null && tile.GetAllObjectInstances().Count > 0) {
				foreach (ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
					if (objectInstance.prefab.bitmasking) {
						BitmaskObjects(
							objectInstance,
							true, // includeDiagonalSurroundingTiles -- default: true
							false, // customBitSumInput -- default: false
							true, // compareEquivalentObjects -- default: false
							null // customCompareObjectTypes -- default: null
						);
					} else {
						if (objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation).Count > 0) {
							if (objectInstance.prefab.jobType != JobManager.JobEnum.PlantFarm) {
								objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[objectInstance.rotationIndex];
							}
						} else {
							objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBaseSpriteForVariation(objectInstance.variation);
						}
					}
				}
			}
		}
	}
}