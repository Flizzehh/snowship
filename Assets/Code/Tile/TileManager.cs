using Snowship.NTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NPersistence;
using Snowship.NPlanet;
using Snowship.NState;
using Snowship.NUtilities;
using UnityEngine;

public class TileManager : IManager {

	private GameManager startCoroutineReference;

	public void SetStartCoroutineReference(GameManager startCoroutineReference) {
		this.startCoroutineReference = startCoroutineReference;
	}

	public class TileTypeGroup {

		public enum TypeEnum {
			Water,
			Hole,
			Ground,
			Stone
		}

		public enum PropertyEnum {
			TileTypeGroup,
			Type,
			DefaultTileType,
			TileTypes
		}

		public static readonly List<TileTypeGroup> tileTypeGroups = new List<TileTypeGroup>();

		public readonly TypeEnum type;
		public readonly string name;

		public readonly TileType.TypeEnum defaultTileType;

		public readonly List<TileType> tileTypes;

		public TileTypeGroup(TypeEnum type, TileType.TypeEnum defaultTileType, List<TileType> tileTypes) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.defaultTileType = defaultTileType;

			this.tileTypes = tileTypes;
		}

		public static TileTypeGroup GetTileTypeGroupByString(string tileTypeGroupString) {
			return GetTileTypeGroupByEnum((TypeEnum)Enum.Parse(typeof(TypeEnum), tileTypeGroupString));
		}

		public static TileTypeGroup GetTileTypeGroupByEnum(TypeEnum tileTypeGroupEnum) {
			return tileTypeGroups.Find(tileTypeGroup => tileTypeGroup.type == tileTypeGroupEnum);
		}
	}

	public class TileType {

		public enum TypeEnum {
			DirtWater, GrassWater, ColdGrassWater, DryGrassWater, SandWater, SnowIce, StoneIce, StoneWater, ClayWater,
			DirtHole, SandHole, SnowHole, StoneHole,
			Dirt, Mud, DirtGrass, DirtThinGrass, DirtDryGrass, Grass, ThickGrass, ColdGrass, DryGrass, Sand, Snow, SnowStone, StoneThinGrass, StoneSand, StoneSnow, Clay,
			Andesite, Basalt, Diorite, Granite, Kimberlite, Obsidian, Rhyolite, Chalk, Claystone, Coal, Flint, Lignite, Limestone, Sandstone, Anthracite, Marble, Quartz, Slate,
			IronOre, GoldOre, SilverOre, BronzeOre, CopperOre
		};

		public enum ClassEnum {
			LiquidWater,
			Dirt,
			Plantable
		};

		public enum PropertyEnum {
			TileType,
			Type,
			Classes,
			WalkSpeed,
			Walkable,
			Buildable,
			Bitmasking,
			BlocksLight,
			ResourceRanges
		}

		public static readonly List<TileType> tileTypes = new List<TileType>();

		public readonly TileTypeGroup.TypeEnum groupType;

		public readonly TypeEnum type;
		public readonly string name;

		public readonly Dictionary<ClassEnum, bool> classes;

		public readonly float walkSpeed;

		public readonly bool walkable;
		public readonly bool buildable;

		public readonly bool bitmasking;

		public readonly bool blocksLight;

		public readonly List<ResourceManager.ResourceRange> resourceRanges;

		public readonly List<Sprite> baseSprites = new List<Sprite>();
		public readonly List<Sprite> bitmaskSprites = new List<Sprite>();
		public readonly List<Sprite> riverSprites = new List<Sprite>();

		public TileType(TileTypeGroup.TypeEnum groupType, TypeEnum type, Dictionary<ClassEnum, bool> classes, float walkSpeed, bool walkable, bool buildable, bool bitmasking, bool blocksLight, List<ResourceManager.ResourceRange> resourceRanges) {
			this.groupType = groupType;

			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.classes = classes;

			this.walkSpeed = walkSpeed;

			this.walkable = walkable;
			this.buildable = buildable;

			this.bitmasking = bitmasking;

			this.blocksLight = blocksLight;

			this.resourceRanges = resourceRanges;

			baseSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + type + "/" + type + "-base").ToList();
			bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + type + "/" + type + "-bitmask").ToList();

			if (classes[ClassEnum.LiquidWater]) {
				riverSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + type + "/" + type + "-river").ToList();
			}
		}

		public static void InitializeTileTypes() {
			List<KeyValuePair<string, object>> tileTypeGroupProperties = PersistenceHandler.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/tile-types").text.Split('\n').ToList());
			foreach (KeyValuePair<string, object> tileTypeGroupProperty in tileTypeGroupProperties) {
				switch ((TileTypeGroup.PropertyEnum)Enum.Parse(typeof(TileTypeGroup.PropertyEnum), tileTypeGroupProperty.Key)) {
					case TileTypeGroup.PropertyEnum.TileTypeGroup:

						TileTypeGroup.TypeEnum? groupType = null;
						TypeEnum? defaultTileType = null;
						List<TileType> tileTypes = new List<TileType>();

						foreach (KeyValuePair<string, object> tileTypeGroupSubProperty in (List<KeyValuePair<string, object>>)tileTypeGroupProperty.Value) {
							switch ((TileTypeGroup.PropertyEnum)Enum.Parse(typeof(TileTypeGroup.PropertyEnum), tileTypeGroupSubProperty.Key)) {
								case TileTypeGroup.PropertyEnum.Type:
									groupType = (TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), (string)tileTypeGroupSubProperty.Value);
									break;
								case TileTypeGroup.PropertyEnum.DefaultTileType:
									defaultTileType = (TypeEnum)Enum.Parse(typeof(TypeEnum), (string)tileTypeGroupSubProperty.Value);
									break;
								case TileTypeGroup.PropertyEnum.TileTypes:
									foreach (KeyValuePair<string, object> tileTypeProperty in (List<KeyValuePair<string, object>>)tileTypeGroupSubProperty.Value) {
										switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), tileTypeProperty.Key)) {
											case PropertyEnum.TileType:

												TypeEnum? type = null;
												Dictionary<ClassEnum, bool> classes = new Dictionary<ClassEnum, bool>();
												float? walkSpeed = null;
												bool? walkable = null;
												bool? buildable = null;
												bool? bitmasking = null;
												bool? blocksLight = null;
												List<ResourceManager.ResourceRange> resourceRanges = new List<ResourceManager.ResourceRange>();

												foreach (ClassEnum tileTypeClassEnum in Enum.GetValues(typeof(ClassEnum))) {
													classes.Add(tileTypeClassEnum, false);
												}

												foreach (KeyValuePair<string, object> tileTypeSubProperty in (List<KeyValuePair<string, object>>)tileTypeProperty.Value) {
													switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), tileTypeSubProperty.Key)) {
														case PropertyEnum.Type:
															type = (TypeEnum)Enum.Parse(typeof(TypeEnum), (string)tileTypeSubProperty.Value);
															break;
														case PropertyEnum.Classes:
															foreach (string tileTypeClassString in ((string)tileTypeSubProperty.Value).Split(',')) {
																classes[(ClassEnum)Enum.Parse(typeof(ClassEnum), tileTypeClassString)] = true;
															}
															break;
														case PropertyEnum.WalkSpeed:
															walkSpeed = float.Parse((string)tileTypeSubProperty.Value);
															break;
														case PropertyEnum.Walkable:
															walkable = bool.Parse((string)tileTypeSubProperty.Value);
															break;
														case PropertyEnum.Buildable:
															buildable = bool.Parse((string)tileTypeSubProperty.Value);
															break;
														case PropertyEnum.Bitmasking:
															bitmasking = bool.Parse((string)tileTypeSubProperty.Value);
															break;
														case PropertyEnum.BlocksLight:
															blocksLight = bool.Parse((string)tileTypeSubProperty.Value);
															break;
														case PropertyEnum.ResourceRanges:
															foreach (string resourceRangeString in ((string)tileTypeSubProperty.Value).Split(',')) {
																ResourceManager.Resource resource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), resourceRangeString.Split(':')[0]));
																int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
																int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
																resourceRanges.Add(new ResourceManager.ResourceRange(resource, min, max));
															}
															break;
														default:
															Debug.LogError("Unknown tile type sub property: " + tileTypeSubProperty.Key + " " + tileTypeSubProperty.Value);
															break;
													}
												}

												TileType tileType = new TileType(
													groupType.Value,
													type.Value,
													classes,
													walkSpeed.Value,
													walkable.Value,
													buildable.Value,
													bitmasking.Value,
													blocksLight.Value,
													resourceRanges
												);
												tileTypes.Add(tileType);
												TileType.tileTypes.Add(tileType);

												break;
											default:
												Debug.LogError("Unknown tile type property: " + tileTypeProperty.Key + " " + tileTypeProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown tile type group sub property: " + tileTypeGroupSubProperty.Key + " " + tileTypeGroupSubProperty.Value);
									break;
							}

						}

						TileTypeGroup tileTypeGroup = new TileTypeGroup(
							groupType.Value,
							defaultTileType.Value,
							tileTypes
						);
						TileTypeGroup.tileTypeGroups.Add(tileTypeGroup);

						break;
					default:
						Debug.LogError("Unknown tile type group property: " + tileTypeGroupProperty.Key + " " + tileTypeGroupProperty.Value);
						break;

				}
			}
		}

		public static TileType GetTileTypeByString(string tileTypeString) {
			return GetTileTypeByEnum((TypeEnum)Enum.Parse(typeof(TypeEnum), tileTypeString));
		}

		public static TileType GetTileTypeByEnum(TypeEnum tileTypeEnum) {
			return tileTypes.Find(tileType => tileType.type == tileTypeEnum);
		}

		public static List<TileType> GetTileTypesWithClass(ClassEnum tileTypeClassEnum) {
			return tileTypes.Where(tileType => tileType.classes[tileTypeClassEnum]).ToList();
		}
	}

	public class Biome {

		public enum PropertyEnum {
			Biome, Type, TileTypes, PlantChances, Ranges, Colour
		}

		public enum RangePropertyEnum {
			Range, Precipitation, Temperature
		}

		public enum TypeEnum {
			IceCap,
			Tundra, WetTundra, PolarWetlands,
			Steppe, BorealForest,
			TemperateWoodlands, TemperateForest, TemperateWetForest, TemperateWetlands,
			Mediterranean,
			SubtropicalScrub, SubtropicalWoodlands, SubtropicalDryForest, SubtropicalForest, SubtropicalWetForest, SubtropicalWetlands,
			TropicalScrub, TropicalWoodlands, TropicalDryForest, TropicalForest, TropicalWetForest, TropicalWetlands,
			PolarDesert, CoolDesert, TemperateDesert, Desert, ExtremeDesert
		};

		public static readonly List<Biome> biomes = new List<Biome>();

		public readonly TypeEnum type;
		public readonly string name;

		public readonly Dictionary<TileTypeGroup.TypeEnum, TileType> tileTypes;

		public readonly Dictionary<ResourceManager.PlantEnum, float> plantChances;

		public readonly List<Range> ranges;

		public readonly Color colour;

		public Biome(
			TypeEnum type,
			Dictionary<TileTypeGroup.TypeEnum, TileType> tileTypes,
			Dictionary<ResourceManager.PlantEnum, float> plantChances,
			List<Range> ranges,
			Color colour
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.tileTypes = tileTypes;

			this.plantChances = plantChances;

			this.ranges = ranges;

			this.colour = colour;
		}

		public static void InitializeBiomes() {
			List<KeyValuePair<string, object>> biomeProperties = PersistenceHandler.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/biomes").text.Split('\n').ToList());
			foreach (KeyValuePair<string, object> biomeProperty in biomeProperties) {
				switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), biomeProperty.Key)) {
					case PropertyEnum.Biome:

						TypeEnum? type = null;
						Dictionary<TileTypeGroup.TypeEnum, TileType> tileTypes = new Dictionary<TileTypeGroup.TypeEnum, TileType>();
						Dictionary<ResourceManager.PlantEnum, float> plantChances = new Dictionary<ResourceManager.PlantEnum, float>();
						List<Range> ranges = new List<Range>();
						Color? colour = null;

						foreach (KeyValuePair<string, object> biomeSubProperty in (List<KeyValuePair<string, object>>)biomeProperty.Value) {
							switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), biomeSubProperty.Key)) {
								case PropertyEnum.Type:
									type = (TypeEnum)Enum.Parse(typeof(TypeEnum), (string)biomeSubProperty.Value);
									break;
								case PropertyEnum.TileTypes:
									foreach (KeyValuePair<string, object> tileTypeProperty in (List<KeyValuePair<string, object>>)biomeSubProperty.Value) {
										tileTypes.Add(
											(TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), tileTypeProperty.Key),
											TileType.GetTileTypeByString((string)tileTypeProperty.Value)
										);
									}
									break;
								case PropertyEnum.PlantChances:
									foreach (KeyValuePair<string, object> plantChanceProperty in (List<KeyValuePair<string, object>>)biomeSubProperty.Value) {
										plantChances.Add(
											(ResourceManager.PlantEnum)Enum.Parse(typeof(ResourceManager.PlantEnum), plantChanceProperty.Key),
											float.Parse((string)plantChanceProperty.Value)
										);
									}
									break;
								case PropertyEnum.Ranges:
									foreach (KeyValuePair<string, object> rangeProperty in (List<KeyValuePair<string, object>>)biomeSubProperty.Value) {
										switch ((RangePropertyEnum)Enum.Parse(typeof(RangePropertyEnum), rangeProperty.Key)) {
											case RangePropertyEnum.Range:

												float? pMin = null;
												float? pMax = null;

												float? tMin = null;
												float? tMax = null;

												foreach (KeyValuePair<string, object> rangeSubProperty in (List<KeyValuePair<string, object>>)rangeProperty.Value) {
													switch ((RangePropertyEnum)Enum.Parse(typeof(RangePropertyEnum), rangeSubProperty.Key)) {
														case RangePropertyEnum.Precipitation:

															string[] pString = ((string)rangeSubProperty.Value).Split(',');

															if (pString[0] == "-Inf") {
																pMin = float.MinValue;
															} else {
																pMin = float.Parse(pString[0]);
															}

															if (pString[1] == "Inf") {
																pMax = float.MaxValue;
															} else {
																pMax = float.Parse(pString[1]);
															}

															break;
														case RangePropertyEnum.Temperature:
															string[] tString = ((string)rangeSubProperty.Value).Split(',');

															if (tString[0] == "-Inf") {
																tMin = float.MinValue;
															} else {
																tMin = float.Parse(tString[0]);
															}

															if (tString[1] == "Inf") {
																tMax = float.MaxValue;
															} else {
																tMax = float.Parse(tString[1]);
															}

															break;
														default:
															Debug.LogError("Unknown range sub property: " + rangeSubProperty.Key + " " + rangeSubProperty.Value);
															break;
													}
												}

												ranges.Add(
													new Range(
														pMin.Value,
														pMax.Value,
														tMin.Value,
														tMax.Value
													)
												);

												break;
											default:
												Debug.LogError("Unknown range property: " + rangeProperty.Key + " " + rangeProperty.Value);
												break;
										}
									}
									break;
								case PropertyEnum.Colour:
									colour = ColourUtilities.HexToColor((string)biomeSubProperty.Value);
									break;
								default:
									Debug.LogError("Unknown biome sub property: " + biomeSubProperty.Key + " " + biomeSubProperty.Value);
									break;
							}

						}

						Biome biome = new Biome(
							type.Value,
							tileTypes,
							plantChances,
							ranges,
							colour.Value
						);
						biomes.Add(biome);

						break;
					default:
						Debug.LogError("Unknown biome property: " + biomeProperty.Key + " " + biomeProperty.Value);
						break;
				}
			}
		}

		public static Biome GetBiomeByString(string biomeTypeString) {
			return GetBiomeByEnum((TypeEnum)Enum.Parse(typeof(TypeEnum), biomeTypeString));
		}

		public static Biome GetBiomeByEnum(TypeEnum biomeTypeEnum) {
			return biomes.Find(biome => biome.type == biomeTypeEnum);
		}

		public class Range {

			public readonly float pMin;
			public readonly float pMax;

			public readonly float tMin;
			public readonly float tMax;

			public Range(float pMin, float pMax, float tMin, float tMax) {
				this.pMin = pMin;
				this.pMax = pMax;

				this.tMin = tMin;
				this.tMax = tMax;
			}

			public bool IsInRange(float precipitation, float temperature) {
				if (pMin <= precipitation && pMax > precipitation) {
					if (tMin <= temperature && tMax > temperature) {
						return true;
					}
				}
				return false;
			}
		}
	}

	public class ResourceVein {

		public enum GroupEnum {
			Stone, Coast
		}

		public enum GroupPropertyEnum {
			VeinGroup,
			Type,
			Veins
		}

		public enum PropertyEnum {
			Vein,
			ResourceType,
			TileTypes,
			NumVeinsByMapSize,
			VeinDistance,
			VeinSize,
			VeinSizeRange
		}

		public static readonly List<ResourceVein> resourceVeins = new List<ResourceVein>();

		public ResourceManager.ResourceEnum resourceType;
		public GroupEnum groupType;
		public Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum> tileTypes;
		public int numVeinsByMapSize = 0;
		public int veinDistance = 0;
		public int veinSize = 0;
		public int veinSizeRange = 0;

		public ResourceVein(
			ResourceManager.ResourceEnum resourceType,
			GroupEnum groupType,
			Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum> tileTypes,
			int numVeinsByMapSize,
			int veinDistance,
			int veinSize,
			int veinSizeRange
		) {
			this.resourceType = resourceType;
			this.groupType = groupType;
			this.tileTypes = tileTypes;
			this.numVeinsByMapSize = numVeinsByMapSize;
			this.veinDistance = veinDistance;
			this.veinSize = veinSize;
			this.veinSizeRange = veinSizeRange;
		}

		public static readonly Dictionary<ResourceManager.ResourceEnum, Func<Tile, bool>> resourceVeinValidTileFunctions = new Dictionary<ResourceManager.ResourceEnum, Func<Tile, bool>>() {
			{ ResourceManager.ResourceEnum.Clay, delegate (Tile tile) {
				if (((tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && tile.horizontalSurroundingTiles.Find(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) != null) || (tile.tileType.groupType != TileTypeGroup.TypeEnum.Water)) && (tile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
					if (tile.temperature >= -30) {
						return true;
					}
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.Coal, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.GoldOre, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.SilverOre, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.BronzeOre, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.IronOre, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.CopperOre, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} },
			{ ResourceManager.ResourceEnum.Chalk, delegate (Tile tile) {
				if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					return true;
				}
				return false;
			} }
		};

		public static void InitializeResourceVeins() {
			List<KeyValuePair<string, object>> resourceVeinGroupProperties = PersistenceHandler.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/resource-veins").text.Split('\n').ToList());
			foreach (KeyValuePair<string, object> resourceVeinGroupProperty in resourceVeinGroupProperties) {
				switch ((GroupPropertyEnum)Enum.Parse(typeof(GroupPropertyEnum), resourceVeinGroupProperty.Key)) {
					case GroupPropertyEnum.VeinGroup:

						GroupEnum? groupType = null;

						foreach (KeyValuePair<string, object> resourceVeinGroupSubProperty in (List<KeyValuePair<string, object>>)resourceVeinGroupProperty.Value) {
							switch ((GroupPropertyEnum)Enum.Parse(typeof(GroupPropertyEnum), resourceVeinGroupSubProperty.Key)) {
								case GroupPropertyEnum.Type:
									groupType = (GroupEnum)Enum.Parse(typeof(GroupEnum), (string)resourceVeinGroupSubProperty.Value);
									break;
								case GroupPropertyEnum.Veins:
									foreach (KeyValuePair<string, object> resourceVeinProperty in (List<KeyValuePair<string, object>>)resourceVeinGroupSubProperty.Value) {
										switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), resourceVeinProperty.Key)) {
											case PropertyEnum.Vein:

												ResourceManager.ResourceEnum? resourceType = null;
												Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum> tileTypes = new Dictionary<TileTypeGroup.TypeEnum, TileType.TypeEnum>();
												int? numVeinsByMapSize = null;
												int? veinDistance = null;
												int? veinSize = null;
												int? veinSizeRange = null;

												foreach (KeyValuePair<string, object> resourceVeinSubProperty in (List<KeyValuePair<string, object>>)resourceVeinProperty.Value) {
													switch ((PropertyEnum)Enum.Parse(typeof(PropertyEnum), resourceVeinSubProperty.Key)) {
														case PropertyEnum.ResourceType:
															resourceType = (ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.TileTypes:
															foreach (string tileTypesString in ((string)resourceVeinSubProperty.Value).Split(',')) {
																TileTypeGroup.TypeEnum tileTypeGroup = (TileTypeGroup.TypeEnum)Enum.Parse(typeof(TileTypeGroup.TypeEnum), tileTypesString.Split(':')[0]);
																TileType.TypeEnum tileType = (TileType.TypeEnum)Enum.Parse(typeof(TileType.TypeEnum), tileTypesString.Split(':')[1]);
																tileTypes.Add(tileTypeGroup, tileType);
															}
															break;
														case PropertyEnum.NumVeinsByMapSize:
															numVeinsByMapSize = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.VeinDistance:
															veinDistance = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.VeinSize:
															veinSize = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														case PropertyEnum.VeinSizeRange:
															veinSizeRange = int.Parse((string)resourceVeinSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown resource vein sub property: " + resourceVeinSubProperty.Key + " " + resourceVeinSubProperty.Value);
															break;
													}
												}

												ResourceVein resourceVein = new ResourceVein(
													resourceType.Value,
													groupType.Value,
													tileTypes,
													numVeinsByMapSize.Value,
													veinDistance.Value,
													veinSize.Value,
													veinSizeRange.Value
												);
												resourceVeins.Add(resourceVein);

												break;
											default:
												Debug.LogError("Unknown resource vein property: " + resourceVeinProperty.Key + " " + resourceVeinProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown resource vein group sub property: " + resourceVeinGroupSubProperty.Key + " " + resourceVeinGroupSubProperty.Value);
									break;
							}
						}
						break;
					default:
						Debug.LogError("Unknown resource vein group property: " + resourceVeinGroupProperty.Key + " " + resourceVeinGroupProperty.Value);
						break;

				}
			}
		}

		public static List<ResourceVein> GetResourceVeinsByGroup(GroupEnum resourceVeinGroupEnum) {
			return resourceVeins.Where(resourceVein => resourceVein.groupType == resourceVeinGroupEnum).ToList();
		}
	}

	public static readonly Dictionary<int, List<List<int>>> nonWalkableSurroundingTilesComparatorMap = new Dictionary<int, List<List<int>>>() {
		{ 0, new List<List<int>>() { new List<int>() { 4, 1, 5, 2 }, new List<int>() { 7, 3, 6, 2 } } },
		{ 1, new List<List<int>>() { new List<int>() { 4, 0, 7, 3 }, new List<int>() { 5, 2, 6, 3 } } },
		{ 2, new List<List<int>>() { new List<int>() { 5, 1, 4, 0 }, new List<int>() { 6, 3, 7, 0 } } },
		{ 3, new List<List<int>>() { new List<int>() { 6, 2, 5, 1 }, new List<int>() { 7, 0, 4, 1 } } }
	};

	public class Tile {
		public readonly Map map;

		public GameObject obj;
		public readonly Vector2 position;

		public SpriteRenderer sr;

		public List<Tile> horizontalSurroundingTiles = new List<Tile>();
		public List<Tile> diagonalSurroundingTiles = new List<Tile>();
		public List<Tile> surroundingTiles = new List<Tile>();

		public float height;

		public TileType tileType;

		public Map.Region region;
		public Map.Region drainageBasin;
		public Map.RegionBlock regionBlock;
		public Map.RegionBlock squareRegionBlock;

		public Biome biome;
		public ResourceManager.Plant plant;
		public ResourceManager.Farm farm;

		private float precipitation = 0;
		public float temperature = 0;

		public bool walkable = false;
		public float walkSpeed = 0;

		public bool buildable = false;

		public bool blocksLight = false;

		private bool roof = false;

		public float brightness = 0;
		public Dictionary<int, float> brightnessAtHour = new Dictionary<int, float>();
		public Dictionary<int, Dictionary<Tile, float>> shadowsFrom = new Dictionary<int, Dictionary<Tile, float>>(); // Tiles that affect the shadow on this tile
		public Dictionary<int, List<Tile>> shadowsTo = new Dictionary<int, List<Tile>>(); // Tiles that have shadows due to this tile
		public Dictionary<int, List<Tile>> blockingShadowsFrom = new Dictionary<int, List<Tile>>(); // Tiles that have shadows that were cut short because this tile was in the way

		public Dictionary<ResourceManager.LightSource, float> lightSourceBrightnesses = new Dictionary<ResourceManager.LightSource, float>();
		public ResourceManager.LightSource primaryLightSource;
		public float lightSourceBrightness;

		public Dictionary<int, ResourceManager.ObjectInstance> objectInstances = new Dictionary<int, ResourceManager.ObjectInstance>();

		public bool dugPreviously;

		public bool visible;

		public Tile(Map map, Vector2 position, float height) {
			this.map = map;

			this.position = position;

			obj = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, new Vector2(position.x + 0.5f, position.y + 0.5f), Quaternion.identity);
			obj.transform.SetParent(GameManager.resourceM.tileParent.transform, true);
			obj.name = "Tile: " + position;

			sr = obj.GetComponent<SpriteRenderer>();

			SetTileHeight(height);

			SetBrightness(1f, 12);
		}

		public void SetTileHeight(float height) {
			this.height = height;
			SetTileTypeByHeight();
		}

		public void SetTileType(TileType tileType, bool setBiomeTileType, bool bitmask, bool redetermineRegion) {
			TileType oldTileType = this.tileType;
			this.tileType = tileType;

			if (setBiomeTileType && biome != null) {
				SetBiome(biome, true);
			}

			walkable = tileType.walkable;
			buildable = tileType.buildable;
			blocksLight = tileType.blocksLight;

			if (bitmask) {
				map.Bitmasking(new List<Tile>() { this }.Concat(surroundingTiles).ToList(), true, !redetermineRegion); // Lighting automatically recalculated in RedetermineRegion()
			}

			if (plant != null && !tileType.classes[TileType.ClassEnum.Plantable]) {
				plant.Remove();
				plant = null;
			}

			if (redetermineRegion) {
				RedetermineRegion(oldTileType);
			}

			SetWalkSpeed();
		}

		public void RedetermineRegion(TileType oldTileType) {
			if (walkable != oldTileType.walkable) { // Difference in walkability
				if (region != null) {
					region.tiles.Remove(this);
				}
				if (walkable && !oldTileType.walkable) { // Type is walkable, old type wasn't (e.g. stone mined, now ground)
					List<Map.Region> surroundingRegions = new List<Map.Region>();
					bool anyVisible = false;
					foreach (Tile tile in horizontalSurroundingTiles) {
						if (tile != null && tile.region != null && !surroundingRegions.Contains(tile.region)) {
							surroundingRegions.Add(tile.region);
							if (tile.visible) {
								anyVisible = true;
							}
						}
					}
					if (surroundingRegions.Count > 0) {
						Map.Region largestRegion = surroundingRegions.OrderByDescending(r => r.tiles.Count).FirstOrDefault();
						ChangeRegion(largestRegion, false, false);
						surroundingRegions.Remove(largestRegion);
						foreach (Map.Region surroundingRegion in surroundingRegions) {
							if (surroundingRegion.visible != anyVisible) {
								surroundingRegion.SetVisible(anyVisible, false, true);
							}
							foreach (Tile tile in surroundingRegion.tiles) {
								tile.ChangeRegion(largestRegion, false, false);
							}
							surroundingRegion.tiles.Clear();
							GameManager.colonyM.colony.map.regions.Remove(surroundingRegion);
						}
						region.SetVisible(anyVisible, true, false);
					} else {
						ChangeRegion(new Map.Region(tileType, GameManager.colonyM.colony.map.regions[GameManager.colonyM.colony.map.regions.Count - 1].id + 1), false, false);
					}
				} else { // Type is not walkable, old type was walkable (e.g. was ground, now stone)
					ChangeRegion(null, false, false);
				}
			}
			/*
			if (oldTileType.walkable != walkable && region != null) {
				bool setParentTileRegion = false;
				if (!oldTileType.walkable && walkable) { // If a non-walkable tile became a walkable tile (splits two non-walkable regions)
					setParentTileRegion = true;

					List<Tile> nonWalkableSurroundingTiles = new List<Tile>();
					foreach (Tile tile in horizontalSurroundingTiles) {
						if (tile != null && !tile.walkable) {
							nonWalkableSurroundingTiles.Add(tile);
						}
					}
					List<Tile> removeFromNonWalkableSurroundingTiles = new List<Tile>();
					foreach (Tile tile in nonWalkableSurroundingTiles) {
						if (!removeFromNonWalkableSurroundingTiles.Contains(tile)) {
							int tileIndex = surroundingTiles.IndexOf(tile);
							List<List<int>> orderedIndexesToCheckList = nonWalkableSurroundingTilesComparatorMap[tileIndex];
							bool removedOppositeTile = false;
							foreach (List<int> orderedIndexesToCheck in orderedIndexesToCheckList) {
								if (surroundingTiles[orderedIndexesToCheck[0]] != null && !surroundingTiles[orderedIndexesToCheck[0]].walkable) {
									if (nonWalkableSurroundingTiles.Contains(surroundingTiles[orderedIndexesToCheck[1]])) {
										removeFromNonWalkableSurroundingTiles.Add(surroundingTiles[orderedIndexesToCheck[1]]);
										if (!removedOppositeTile && surroundingTiles[orderedIndexesToCheck[2]] != null && !surroundingTiles[orderedIndexesToCheck[2]].walkable) {
											if (nonWalkableSurroundingTiles.Contains(surroundingTiles[orderedIndexesToCheck[3]])) {
												removeFromNonWalkableSurroundingTiles.Add(surroundingTiles[orderedIndexesToCheck[3]]);
												removedOppositeTile = true;
											}
										}
									}
								}
							}
						}
					}
					foreach (Tile tile in removeFromNonWalkableSurroundingTiles) {
						nonWalkableSurroundingTiles.Remove(tile);
					}
					if (nonWalkableSurroundingTiles.Count > 1) {
						Debug.Log("Independent tiles");
						Map.Region oldRegion = region;
						oldRegion.tiles.Clear();
						map.regions.Remove(oldRegion);
						List<List<Tile>> nonWalkableTileGroups = new List<List<Tile>>();
						foreach (Tile nonWalkableTile in nonWalkableSurroundingTiles) {
							Tile currentTile = nonWalkableTile;
							List<Tile> frontier = new List<Tile>() { currentTile };
							List<Tile> checkedTiles = new List<Tile>() { currentTile };
							List<Tile> nonWalkableTiles = new List<Tile>();
							bool addGroup = true;
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								if (nonWalkableTileGroups.Find(group => group.Contains(currentTile)) != null) {
									Debug.Log("Separate tiles part of the same group");
									addGroup = false;
									break;
								}
								frontier.RemoveAt(0);
								nonWalkableTiles.Add(currentTile);
								foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && !checkedTiles.Contains(nTile) && !nTile.walkable) {
										frontier.Add(nTile);
										checkedTiles.Add(nTile);
									}
								}
							}
							if (addGroup) {
								nonWalkableTileGroups.Add(nonWalkableTiles);
							}
						}
						foreach (List<Tile> nonWalkableTileGroup in nonWalkableTileGroups) {
							Map.Region groupRegion = new Map.Region(nonWalkableTileGroup[0].tileType, map.currentRegionID);
							map.currentRegionID += 1;
							foreach (Tile tile in nonWalkableTileGroup) {
								tile.ChangeRegion(groupRegion, false, false);
							}
							map.regions.Add(groupRegion);
						}
					}
				}
				if (setParentTileRegion || (oldTileType.walkable && !walkable)) { // If a walkable tile became a non-walkable tile (add non-walkable tile to nearby non-walkable region if exists, if not create it)
					List<Map.Region> similarRegions = new List<Map.Region>();
					foreach (Tile tile in horizontalSurroundingTiles) {
						if (tile != null && tile.region != null && tile.walkable == walkable) {
							if (tile.region != region) {
								similarRegions.Add(tile.region);
							}
						}
					}
					if (similarRegions.Count == 0) {
						region.tiles.Remove(this);
						ChangeRegion(new Map.Region(tileType, map.currentRegionID), false, false);
						map.currentRegionID += 1;
					} else if (similarRegions.Count == 1) {
						region.tiles.Remove(this);
						ChangeRegion(similarRegions[0], false, false);
					} else {
						region.tiles.Remove(this);
						ChangeRegion(similarRegions.OrderByDescending(similarRegion => similarRegion.tiles.Count).ToList()[0], false, false);
						foreach (Map.Region similarRegion in similarRegions) {
							if (similarRegion != region) {
								foreach (Tile tile in similarRegion.tiles) {
									tile.ChangeRegion(region, false, false);
								}
								similarRegion.tiles.Clear();
								map.regions.Remove(similarRegion);
							}
						}
					}
				}
			}
			*/
		}

		public void ChangeRegion(Map.Region region, bool changeTileTypeToRegionType, bool bitmask) {
			//if (this.region != null) {
			//	this.region.tiles.Remove(this);
			//}
			this.region = region;
			if (region != null) {
				region.tiles.Add(this);
				if (!map.regions.Contains(region)) {
					map.regions.Add(region);
				}
				if (changeTileTypeToRegionType) {
					SetTileType(region.tileType, true, bitmask, false);
				}
			}
		}

		public void SetTileTypeByHeight() {
			if (height < map.mapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Water]) {
				SetTileType(TileType.GetTileTypeByEnum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).defaultTileType), false, false, false);
			} else if (height > map.mapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Stone]) {
				SetTileType(TileType.GetTileTypeByEnum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).defaultTileType), false, false, false);
			} else {
				SetTileType(TileType.GetTileTypeByEnum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Ground).defaultTileType), false, false, false);
			}
		}

		public void SetBiome(Biome biome, bool setPlant) {
			this.biome = biome;
			SetTileType(biome.tileTypes[tileType.groupType], false, false, false);
			if (setPlant && tileType.classes[TileType.ClassEnum.Plantable]) {
				SetPlant(false, null);
			}
		}

		public void SetPlant(bool onlyRemovePlant, ResourceManager.Plant specificPlant) {
			if (plant != null) {
				plant.Remove();
				plant = null;
			}
			if (!onlyRemovePlant) {
				if (specificPlant == null) {
					ResourceManager.PlantPrefab biomePlantGroup = GameManager.resourceM.GetPlantPrefabByBiome(biome, false);
					if (biomePlantGroup != null) {
						plant = new ResourceManager.Plant(biomePlantGroup, this, null, true, null);
					}
				} else {
					plant = specificPlant;
				}
			}
			SetWalkSpeed();
		}

		public void SetObject(ResourceManager.ObjectInstance instance) {
			AddObjectInstanceToLayer(instance, instance.prefab.layer);
			PostChangeObject();
		}

		public void PostChangeObject() {
			walkable = tileType.walkable;
			buildable = tileType.buildable;
			blocksLight = tileType.blocksLight;

			bool recalculatedLighting = false;
			bool recalculatedRegion = false;

			foreach (KeyValuePair<int, ResourceManager.ObjectInstance> layerToObjectInstance in objectInstances) {
				if (layerToObjectInstance.Value != null) {

					// Object Instances are iterated from lowest layer to highest layer (sorted in AddObjectInstaceToLayer),
					// therefore, the highest layer is the buildable value that should be applied
					buildable = layerToObjectInstance.Value.prefab.buildable;

					if (!recalculatedLighting && layerToObjectInstance.Value.prefab.blocksLight) {
						blocksLight = true;
						map.RecalculateLighting(new List<Tile>() { this }, true);

						recalculatedLighting = true;
					}

					if (!recalculatedRegion && !layerToObjectInstance.Value.prefab.walkable) {
						walkable = false;
						map.RecalculateRegionsAtTile(this);

						recalculatedRegion = true;
					}
				}
			}
			SetWalkSpeed();
		}

		private void AddObjectInstanceToLayer(ResourceManager.ObjectInstance instance, int layer) {
			if (objectInstances.ContainsKey(layer)) { // If the layer exists
				if (objectInstances[layer] != null) { // If the object at the layer exists
					if (instance != null) { // If the object being added exists, throw error
						Debug.LogError("Trying to add object where one already exists at " + obj.transform.position);
					} else { // If the object being added is null, set this layer to null
						objectInstances[layer] = null;
					}
				} else { // If the object at the layer does not exist
					objectInstances[layer] = instance;
				}
			} else { // If the layer does not exist
				objectInstances.Add(layer, instance);
			}
			objectInstances.OrderBy(kvp => kvp.Key); // Sorted from lowest layer to highest layer for iterating
		}

		public void RemoveObjectAtLayer(int layer) {
			if (objectInstances.ContainsKey(layer)) {
				ResourceManager.ObjectInstance instance = objectInstances[layer];
				if (instance != null) {
					MonoBehaviour.Destroy(instance.obj);
					foreach (Tile additionalTile in instance.additionalTiles) {
						additionalTile.objectInstances[layer] = null;
						additionalTile.PostChangeObject();
					}
					if (instance.prefab.instanceType == ResourceManager.ObjectInstanceType.Farm) {
						farm = null;
					}
					objectInstances[layer] = null;
				}
			}
			PostChangeObject();
		}

		public void SetObjectInstanceReference(ResourceManager.ObjectInstance objectInstanceReference) {
			if (objectInstances.ContainsKey(objectInstanceReference.prefab.layer)) {
				if (objectInstances[objectInstanceReference.prefab.layer] != null) {
					if (objectInstanceReference != null) {
						Debug.LogError("Trying to add object where one already exists at " + obj.transform.position);
					} else {
						objectInstances[objectInstanceReference.prefab.layer] = null;
					}
				} else {
					objectInstances[objectInstanceReference.prefab.layer] = objectInstanceReference;
				}
			} else {
				objectInstances.Add(objectInstanceReference.prefab.layer, objectInstanceReference);
			}
			PostChangeObject();
		}

		public ResourceManager.ObjectInstance GetObjectInstanceAtLayer(int layer) {
			if (objectInstances.ContainsKey(layer)) {
				return objectInstances[layer];
			}
			return null;
		}

		public List<ResourceManager.ObjectInstance> GetAllObjectInstances() {
			List<ResourceManager.ObjectInstance> allObjectInstances = new List<ResourceManager.ObjectInstance>();
			foreach (KeyValuePair<int, ResourceManager.ObjectInstance> kvp in objectInstances) {
				if (kvp.Value != null) {
					allObjectInstances.Add(kvp.Value);
				}
			}
			return allObjectInstances;
		}

		public bool HasRoof() {
			return roof;
		}

		public void SetRoof(bool roof) {
			this.roof = roof;
		}

		public void SetWalkSpeed() {
			walkSpeed = tileType.walkSpeed;
			if (plant != null && walkSpeed > 0.6f) {
				walkSpeed = 0.6f;
			}
			ResourceManager.ObjectInstance lowestWalkSpeedObject = objectInstances.Values.Where(o => o != null).OrderBy(o => o.prefab.walkSpeed).FirstOrDefault();
			if (lowestWalkSpeedObject != null) {
				walkSpeed = lowestWalkSpeedObject.prefab.walkSpeed;
			}
		}

		public void SetColour(Color newColour, int hour) {
			float currentHourBrightness = Mathf.Max((brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f), lightSourceBrightness);
			int nextHour = (hour == 23 ? 0 : hour + 1);
			float nextHourBrightness = Mathf.Max((brightnessAtHour.ContainsKey(nextHour) ? brightnessAtHour[nextHour] : 1f), lightSourceBrightness);

			if (primaryLightSource != null) {
				sr.color = Color.Lerp(newColour, primaryLightSource.prefab.lightColour + (newColour * (brightnessAtHour.ContainsKey(hour) ? brightnessAtHour[hour] : 1f) * 0.8f), lightSourceBrightness);
			} else {
				sr.color = newColour;
			}
			float colourBrightnessMultiplier = Mathf.Lerp(currentHourBrightness, nextHourBrightness, GameManager.timeM.tileBrightnessTime - hour);
			sr.color = new Color(sr.color.r * colourBrightnessMultiplier, sr.color.g * colourBrightnessMultiplier, sr.color.b * colourBrightnessMultiplier, 1f);

			if (plant != null) {
				plant.obj.GetComponent<SpriteRenderer>().color = sr.color;
			}
			foreach (ResourceManager.ObjectInstance instance in GetAllObjectInstances()) {
				instance.SetColour(sr.color);
			}
			brightness = colourBrightnessMultiplier;
		}

		public void SetBrightness(float newBrightness, int hour) {
			brightness = newBrightness;
			SetColour(sr.color, hour);
		}

		public void AddLightSourceBrightness(ResourceManager.LightSource lightSource, float brightness) {
			lightSourceBrightnesses.Add(lightSource, brightness);
			lightSourceBrightness = lightSourceBrightnesses.Max(kvp => kvp.Value);
			primaryLightSource = lightSourceBrightnesses.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		public void RemoveLightSourceBrightness(ResourceManager.LightSource lightSource) {
			lightSourceBrightnesses.Remove(lightSource);
			if (lightSourceBrightnesses.Count > 0) {
				lightSourceBrightness = lightSourceBrightnesses.Max(kvp => kvp.Value);
				primaryLightSource = lightSourceBrightnesses.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
			} else {
				lightSourceBrightness = 0;
				primaryLightSource = null;
			}
		}

		public void SetPrecipitation(float precipitation) {
			this.precipitation = precipitation;
		}

		public float GetPrecipitation() {
			return precipitation;
		}

		public bool IsVisibleToAColonist() {
			if (walkable) {
				foreach (Colonist colonist in Colonist.colonists) {
					if (colonist.overTile.walkable) {
						if (colonist.overTile.region == region) {
							return true;
						}
					} else {
						foreach (Tile tile in colonist.overTile.horizontalSurroundingTiles) {
							if (tile != null && tile.visible) {
								if (tile.region == region) {
									return true;
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < surroundingTiles.Count; i++) {
				Tile surroundingTile = surroundingTiles[i];
				if (surroundingTile != null && surroundingTile.walkable) {
					if (Map.diagonalCheckMap.ContainsKey(i)) {
						bool skip = true;
						foreach (int horizontalTileIndex in Map.diagonalCheckMap[i]) {
							Tile horizontalTile = surroundingTile.surroundingTiles[horizontalTileIndex];
							if (horizontalTile != null && horizontalTile.walkable) {
								skip = false;
								break;
							}
						}
						if (skip) {
							continue;
						}
					}
					foreach (Colonist colonist in Colonist.colonists) {
						if (colonist.overTile.region == surroundingTile.region) {
							return true;
						}
					}
				}
			}
			return false;
		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);

			if (plant != null) {
				plant.SetVisible(visible);
			}

			foreach (ResourceManager.ObjectInstance objectInstance in GetAllObjectInstances()) {
				objectInstance.SetVisible(visible);
			}
		}
	}

	public enum MapState {
		Nothing, Generating, Generated
	}

	public MapState mapState = MapState.Nothing;

	public void Update() {
		if (mapState == MapState.Generated) {
			GameManager.colonyM.colony.map.DetermineVisibleRegionBlocks();
		}
	}

	public class MapData {
		public int mapSeed;
		public int mapSize;
		public bool actualMap;

		public float equatorOffset;
		public bool planetTemperature;
		public int temperatureRange;
		public float planetDistance;
		public float temperatureOffset;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileTypeGroup.TypeEnum, float> terrainTypeHeights;
		public List<int> surroundingPlanetTileHeightDirections;
		public bool isRiver;
		public List<int> surroundingPlanetTileRivers;
		public Vector2 planetTilePosition;

		public bool preventEdgeTouching;

		public int primaryWindDirection = -1;

		public string mapRegenerationCode = string.Empty;

		public MapData(
			MapData planetMapData,
			int mapSeed,
			int mapSize,
			bool actualMap,
			bool planetTemperature,
			int temperatureRange,
			float planetDistance,
			float averageTemperature,
			float averagePrecipitation,
			Dictionary<TileTypeGroup.TypeEnum, float> terrainTypeHeights,
			List<int> surroundingPlanetTileHeightDirections,
			bool isRiver,
			List<int> surroundingPlanetTileRivers,
			bool preventEdgeTouching,
			int primaryWindDirection,
			Vector2 planetTilePosition
		) {
			this.mapSeed = mapSeed;
			UnityEngine.Random.InitState(mapSeed);

			this.mapSize = mapSize;
			this.actualMap = actualMap;

			this.planetTemperature = planetTemperature;
			this.temperatureRange = temperatureRange;
			this.planetDistance = planetDistance;
			temperatureOffset = CreatePlanetData.CalculatePlanetTemperature(planetDistance);
			this.averageTemperature = averageTemperature;
			this.averagePrecipitation = averagePrecipitation;
			this.terrainTypeHeights = terrainTypeHeights;
			this.surroundingPlanetTileHeightDirections = surroundingPlanetTileHeightDirections;
			this.isRiver = isRiver;
			this.surroundingPlanetTileRivers = surroundingPlanetTileRivers;
			this.preventEdgeTouching = preventEdgeTouching;
			this.primaryWindDirection = primaryWindDirection;
			this.planetTilePosition = planetTilePosition;

			equatorOffset = ((planetTilePosition.y - (mapSize / 2f)) * 2) / mapSize;

			if (planetMapData != null) {
				mapRegenerationCode = planetMapData.mapSeed + "~" + planetMapData.mapSize + "~" + planetMapData.temperatureRange + "~" + planetMapData.planetDistance + "~" + planetMapData.primaryWindDirection + "~" + planetTilePosition.x + "~" + planetTilePosition.y + "~" + mapSize + "~" + mapSeed;
			}
		}
	}

	public enum MapInitializeType {
		NewMap, LoadMap
	}

	public void Initialize(Colony colony, MapInitializeType mapInitializeType) {

		// TODO GameManager.uiM.CloseView<UIMainMenuPresenter>();

		//GameManager.uiMOld.SetMainMenuActive(false);

		GameManager.uiMOld.SetLoadingScreenActive(true);
		GameManager.uiMOld.SetGameUIActive(false);

		mapState = MapState.Generating;

		startCoroutineReference.StartCoroutine(InitializeMap(colony));
		startCoroutineReference.StartCoroutine(PostInitializeMap(mapInitializeType));
	}

	private IEnumerator InitializeMap(Colony colony) {


		colony.map = CreateMap(colony.mapData);

		yield return null;
	}

	private IEnumerator PostInitializeMap(MapInitializeType mapInitializeType) {
		while (!GameManager.colonyM.colony.map.created) {
			yield return null;
		}

		GameManager.tileM.mapState = MapState.Generated;

		if (mapInitializeType == MapInitializeType.NewMap) {
			GameManager.colonyM.SetupNewColony(GameManager.colonyM.colony, true);
		} else if (mapInitializeType == MapInitializeType.LoadMap) {
			GameManager.colonyM.LoadColony(GameManager.colonyM.colony, true);
		}

		GameManager.colonyM.colony.map.SetInitialRegionVisibility();

		GameManager.uiMOld.SetGameUIActive(true);
	}

	public Map CreateMap(MapData mapData) {
		return new Map(mapData);
	}

	public class Map {

		public bool created = false;

		public MapData mapData;
		public Map(MapData mapData) {

			this.mapData = mapData;

			DetermineShadowDirectionsAtHour(mapData.equatorOffset);

			GameManager.tileM.startCoroutineReference.StartCoroutine(CreateMap());
		}

		public Map() {
			DetermineShadowDirectionsAtHour(GameManager.colonyM.colony.mapData.equatorOffset);
		}

		public List<Tile> tiles = new List<Tile>();
		public List<List<Tile>> sortedTiles = new List<List<Tile>>();
		public List<Tile> edgeTiles = new List<Tile>();
		public Dictionary<int, List<Tile>> sortedEdgeTiles = new Dictionary<int, List<Tile>>();

		public IEnumerator CreateMap() {
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Map", "Creating Tiles"); yield return null; }
			CreateTiles();
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Map", "Validating"); yield return null; Bitmasking(tiles, false, false); }

			if (mapData.preventEdgeTouching) {
				PreventEdgeTouching();
			}

			if (mapData.actualMap) {
				GameManager.uiMOld.UpdateLoadingStateText("Map", "Determining Map Edges"); yield return null;
				SetMapEdgeTiles();
				GameManager.uiMOld.UpdateLoadingStateText("Map", "Determining Sorted Map Edges"); yield return null;
				SetSortedMapEdgeTiles();
				GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Merging Terrain with Planet"); yield return null;
				SmoothHeightWithSurroundingPlanetTiles();
				GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Validating"); yield return null;
				Bitmasking(tiles, false, false);
			}

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Determining Regions by Tile Type"); yield return null; }
			SetTileRegions(true, false);

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Reducing Terrain Noise"); yield return null; }
			ReduceNoise();

			if (mapData.actualMap) {
				GameManager.uiMOld.UpdateLoadingStateText("Rivers", "Determining Large River Paths"); yield return null;
				CreateLargeRivers();
				GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Determining Regions by Walkability"); yield return null;
				SetTileRegions(false, false);
				GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Reducing Terrain Noise"); yield return null;
				ReduceNoise();
			}
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Determining Regions by Walkability"); yield return null; }
			SetTileRegions(false, true);
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Terrain", "Validating"); yield return null; Bitmasking(tiles, false, false); }

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Rivers", "Determining Drainage Basins"); yield return null; }
			DetermineDrainageBasins();
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Rivers", "Determining River Paths"); yield return null; }
			CreateRivers();
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Rivers", "Validating"); yield return null; Bitmasking(tiles, false, false); }

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Biomes", "Calculating Temperature"); yield return null; }
			CalculateTemperature();

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Biomes", "Calculating Precipitation"); yield return null; }
			CalculatePrecipitation();
			mapData.primaryWindDirection = primaryWindDirection;

			/*
			foreach (Tile tile in tiles) {
				tile.SetTileHeight(0.5f);
				tile.SetPrecipitation(tile.position.x / mapData.mapSize);
				tile.temperature = ((1 - (tile.position.y / mapData.mapSize)) * 140) - 50;
			}
			*/

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Biomes", "Setting Biomes"); yield return null; }
			SetBiomes(mapData.actualMap);
			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Biomes", "Validating"); yield return null; Bitmasking(tiles, false, false); }

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Region Blocks", "Determining Region Blocks"); yield return null; }
			CreateRegionBlocks();

			if (mapData.actualMap) {
				GameManager.uiMOld.UpdateLoadingStateText("Roofs", "Determining Roofs"); yield return null;
				SetRoofs();

				GameManager.uiMOld.UpdateLoadingStateText("Resources", "Creating Resource Veins"); yield return null;
				SetResourceVeins();
				GameManager.uiMOld.UpdateLoadingStateText("Resources", "Validating"); yield return null;
				Bitmasking(tiles, false, false);

				GameManager.uiMOld.UpdateLoadingStateText("Lighting", "Calculating Shadows"); yield return null;
				RecalculateLighting(tiles, false);
				GameManager.uiMOld.UpdateLoadingStateText("Lighting", "Determining Visible Region Blocks"); yield return null;
				DetermineVisibleRegionBlocks();
				GameManager.uiMOld.UpdateLoadingStateText("Lighting", "Applying Shadows"); yield return null;
				SetTileBrightness(GameManager.timeM.tileBrightnessTime, true);
			}

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Lighting", "Validating"); yield return null; }
			Bitmasking(tiles, false, false);

			if (mapData.actualMap) { GameManager.uiMOld.UpdateLoadingStateText("Finalizing", string.Empty); yield return null; }
			created = true;

			if (mapData.actualMap) {
				GameManager.stateM.TransitionToState(EState.Simulation);
			}
		}

		void CreateTiles() {
			for (int y = 0; y < mapData.mapSize; y++) {
				List<Tile> innerTiles = new List<Tile>();
				for (int x = 0; x < mapData.mapSize; x++) {

					float height = UnityEngine.Random.Range(0f, 1f);

					Vector2 position = new Vector2(x, y);

					Tile tile = new Tile(this, position, height);

					innerTiles.Add(tile);
					tiles.Add(tile);
				}
				sortedTiles.Add(innerTiles);
			}

			SetSurroundingTiles();
			GenerateTerrain();
			AverageTileHeights();
		}

		public void SetSurroundingTiles() {
			for (int y = 0; y < mapData.mapSize; y++) {
				for (int x = 0; x < mapData.mapSize; x++) {
					/* Horizontal */
					if (y + 1 < mapData.mapSize) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y + 1][x]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x + 1 < mapData.mapSize) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y][x + 1]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y - 1][x]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0) {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y][x - 1]);
					} else {
						sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
					}

					/* Diagonal */
					if (x + 1 < mapData.mapSize && y + 1 < mapData.mapSize) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y + 1][x + 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y - 1 >= 0 && x + 1 < mapData.mapSize) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x + 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (x - 1 >= 0 && y - 1 >= 0) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x - 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}
					if (y + 1 < mapData.mapSize && x - 1 >= 0) {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y + 1][x - 1]);
					} else {
						sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
					}

					sortedTiles[y][x].surroundingTiles.AddRange(sortedTiles[y][x].horizontalSurroundingTiles);
					sortedTiles[y][x].surroundingTiles.AddRange(sortedTiles[y][x].diagonalSurroundingTiles);
				}
			}
		}

		void GenerateTerrain() {
			int lastSize = mapData.mapSize;
			for (int halves = 0; halves < Mathf.CeilToInt(Mathf.Log(mapData.mapSize, 2)); halves++) {
				int size = Mathf.CeilToInt(lastSize / 2f);
				for (int sectionY = 0; sectionY < mapData.mapSize; sectionY += size) {
					for (int sectionX = 0; sectionX < mapData.mapSize; sectionX += size) {
						float sectionAverage = 0;
						for (int y = sectionY; (y < sectionY + size && y < mapData.mapSize); y++) {
							for (int x = sectionX; (x < sectionX + size && x < mapData.mapSize); x++) {
								sectionAverage += sortedTiles[y][x].height;
							}
						}
						sectionAverage /= (size * size);
						float maxDeviationSize = -(((float)(size - mapData.mapSize)) / (4 * mapData.mapSize));
						sectionAverage += UnityEngine.Random.Range(-maxDeviationSize, maxDeviationSize);
						for (int y = sectionY; (y < sectionY + size && y < mapData.mapSize); y++) {
							for (int x = sectionX; (x < sectionX + size && x < mapData.mapSize); x++) {
								sortedTiles[y][x].height = sectionAverage;
							}
						}
					}
				}
				lastSize = size;
			}

			foreach (Tile tile in tiles) {
				tile.SetTileHeight(tile.height);
			}
		}

		void AverageTileHeights() {
			for (int i = 0; i < 3; i++) { // 3
				List<float> averageTileHeights = new List<float>();

				foreach (Tile tile in tiles) {
					float averageHeight = tile.height;
					float numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						float multiplicationValue = 1f; // Reduces the weight of horizontal tiles by 50% to help prevent visible edges/corners on the map
						if (nTile != null) {
							if (i > 3) {
								numValidTiles += 1f;
							} else {
								numValidTiles += 0.5f;
								multiplicationValue = 0.5f;
							}
							averageHeight += nTile.height * multiplicationValue;
						}
					}
					averageHeight /= numValidTiles;
					averageTileHeights.Add(averageHeight);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].height = averageTileHeights[k];
					tiles[k].SetTileTypeByHeight();
				}
			}
		}

		void PreventEdgeTouching() {
			foreach (Tile tile in tiles) {
				float edgeDistance = (mapData.mapSize - (Vector2.Distance(tile.obj.transform.position, new Vector2(mapData.mapSize / 2f, mapData.mapSize / 2f)))) / mapData.mapSize;
				tile.SetTileHeight(tile.height * Mathf.Clamp(-Mathf.Pow(edgeDistance - 1.5f, 10) + 1, 0f, 1f));
			}
		}

		public List<Region> regions = new List<Region>();
		public int currentRegionID = 0;

		public class Region {
			public TileType tileType;
			public List<Tile> tiles = new List<Tile>();
			public int id;

			public List<Region> connectedRegions = new List<Region>();

			public bool visible;

			public Region(TileType regionTileType, int regionID) {
				tileType = regionTileType;
				id = regionID;
			}

			public bool IsVisibleToAColonist() {
				if (tileType.walkable) {
					foreach (Colonist colonist in Colonist.colonists) {
						if (colonist.overTile.region == this) {
							return true;
						}
					}
				}
				return false;
			}

			public void SetVisible(bool visible, bool bitmasking, bool recalculateLighting) {
				this.visible = visible;

				List<Tile> tilesToModify = new List<Tile>();
				foreach (Tile tile in tiles) {
					tile.SetVisible(this.visible);

					tilesToModify.Add(tile);
					tilesToModify.AddRange(tile.surroundingTiles);
				}

				tilesToModify = tilesToModify.Distinct().ToList();

				if (bitmasking) {
					GameManager.colonyM.colony.map.Bitmasking(tilesToModify, true, false);
				}

				if (recalculateLighting) {
					GameManager.colonyM.colony.map.RecalculateLighting(tilesToModify, true);
				}

			}
		}

		void SmoothHeightWithSurroundingPlanetTiles() {
			for (int i = 0; i < mapData.surroundingPlanetTileHeightDirections.Count; i++) {
				if (mapData.surroundingPlanetTileHeightDirections[i] != 0) {
					foreach (Tile tile in tiles) {
						float closestEdgeDistance = sortedEdgeTiles[i].Min(edgeTile => Vector2.Distance(edgeTile.obj.transform.position, tile.obj.transform.position)) / (mapData.mapSize);
						float heightMultiplier = mapData.surroundingPlanetTileHeightDirections[i] * Mathf.Pow(closestEdgeDistance - 1f, 10f) + 1f;
						float newHeight = Mathf.Clamp(tile.height * heightMultiplier, 0f, 1f);
						tile.SetTileHeight(newHeight);
					}
				}
			}
		}

		public void SetTileRegions(bool splitByTileType, bool removeNonWalkableRegions) {
			regions.Clear();

			EstablishInitialRegions(splitByTileType);
			FindConnectedRegions(splitByTileType);
			MergeConnectedRegions(splitByTileType);

			RemoveEmptyRegions();

			if (removeNonWalkableRegions) {
				RemoveNonWalkableRegions();
			}
		}

		private void EstablishInitialRegions(bool splitByTileType) {
			foreach (Tile tile in tiles) { // Go through all tiles
				List<Region> foundRegions = new List<Region>(); // For each tile, store a list of the regions around them
				for (int i = 0; i < tile.surroundingTiles.Count; i++) { // Go through the tiles around each tile
					Tile nTile = tile.surroundingTiles[i];
					if (nTile != null && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable)) && (i == 2 || i == 3 /*|| i == 5 || i == 6 */)) { // Uncomment indexes 5 and 6 to enable 8-connectivity connected-component labeling -- If the tiles have the same type
						if (nTile.region != null && !foundRegions.Contains(nTile.region)) { // If the tiles have a region and it hasn't already been looked at
							foundRegions.Add(nTile.region); // Add the surrounding tile's region to the regions found around the original tile
						}
					}
				}
				if (foundRegions.Count <= 0) { // If there weren't any tiles with the same region/tiletype found around them, make a new region for this tile
					tile.ChangeRegion(new Region(tile.tileType, currentRegionID), false, false);
					currentRegionID += 1;
				} else if (foundRegions.Count == 1) { // If there was a single region found around them, give them that region
					tile.ChangeRegion(foundRegions[0], false, false);
				} else if (foundRegions.Count > 1) { // If there was more than one around found around them, give them the region with the lowest ID
					tile.ChangeRegion(FindLowestRegion(foundRegions), false, false);
				}
			}
		}

		private void FindConnectedRegions(bool splitByTileType) {
			foreach (Region region in regions) {
				foreach (Tile tile in region.tiles) {
					foreach (Tile nTile in tile.horizontalSurroundingTiles) {
						if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains(nTile.region) && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable))) {
							region.connectedRegions.Add(nTile.region);
						}
					}
				}
			}
		}

		private void MergeConnectedRegions(bool splitByTileType) {
			while (regions.Where(region => region.connectedRegions.Count > 0).ToList().Count > 0) { // While there are regions that have connected regions
				foreach (Region region in regions) { // Go through each region
					if (region.connectedRegions.Count > 0) { // If this region has connected regions
						Region lowestRegion = FindLowestRegion(region.connectedRegions); // Find the lowest ID region from the connected regions
						if (region != lowestRegion) { // If this region is not the lowest region
							foreach (Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
								tile.ChangeRegion(lowestRegion, false, false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
						foreach (Region connectedRegion in region.connectedRegions) { // Set each tile's region in the connected regions that aren't the lowest region to the lowest region
							if (connectedRegion != lowestRegion) {
								foreach (Tile tile in connectedRegion.tiles) {
									tile.ChangeRegion(lowestRegion, false, false);
								}
								connectedRegion.tiles.Clear();
							}
						}
					}
					region.connectedRegions.Clear(); // Clear the connected regions from this region
				}
				FindConnectedRegions(splitByTileType); // Find the new connected regions
			}
		}

		public List<RegionBlock> regionBlocks = new List<RegionBlock>();

		public class RegionBlock : Region {

			public Vector2 averagePosition = new Vector2(0, 0);
			public List<RegionBlock> surroundingRegionBlocks = new List<RegionBlock>();
			public List<RegionBlock> horizontalSurroundingRegionBlocks = new List<RegionBlock>();

			public float lastBrightnessUpdate;

			public RegionBlock(TileType regionTileType, int regionID) : base(regionTileType, regionID) {
				lastBrightnessUpdate = -1;
			}
		}

		public List<RegionBlock> squareRegionBlocks = new List<RegionBlock>();
		public void CreateRegionBlocks() {
			int regionBlockSize = 10;/*Mathf.RoundToInt(mapData.mapSize / 10f);*/

			regionBlocks.Clear();
			squareRegionBlocks.Clear();

			int size = regionBlockSize;
			int regionIndex = 0;
			for (int sectionY = 0; sectionY < mapData.mapSize; sectionY += size) {
				for (int sectionX = 0; sectionX < mapData.mapSize; sectionX += size) {
					RegionBlock regionBlock = new RegionBlock(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass), regionIndex);
					RegionBlock squareRegionBlock = new RegionBlock(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass), regionIndex);
					for (int y = sectionY; (y < sectionY + size && y < mapData.mapSize); y++) {
						for (int x = sectionX; (x < sectionX + size && x < mapData.mapSize); x++) {
							regionBlock.tiles.Add(sortedTiles[y][x]);
							squareRegionBlock.tiles.Add(sortedTiles[y][x]);
							sortedTiles[y][x].squareRegionBlock = squareRegionBlock;
						}
					}
					regionIndex += 1;
					regionBlocks.Add(regionBlock);
					squareRegionBlocks.Add(squareRegionBlock);
				}
			}
			foreach (RegionBlock squareRegionBlock in squareRegionBlocks) {
				foreach (Tile tile in squareRegionBlock.tiles) {
					foreach (Tile nTile in tile.surroundingTiles) {
						if (nTile != null && nTile.squareRegionBlock != tile.squareRegionBlock && nTile.squareRegionBlock != null && !squareRegionBlock.surroundingRegionBlocks.Contains(nTile.squareRegionBlock)) {
							squareRegionBlock.surroundingRegionBlocks.Add(nTile.squareRegionBlock);
						}
					}
					squareRegionBlock.averagePosition = new Vector2(squareRegionBlock.averagePosition.x + tile.obj.transform.position.x, squareRegionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				squareRegionBlock.averagePosition = new Vector2(squareRegionBlock.averagePosition.x / squareRegionBlock.tiles.Count, squareRegionBlock.averagePosition.y / squareRegionBlock.tiles.Count);
			}
			regionIndex += 1;
			List<RegionBlock> removeRegionBlocks = new List<RegionBlock>();
			List<RegionBlock> newRegionBlocks = new List<RegionBlock>();
			foreach (RegionBlock regionBlock in regionBlocks) {
				if (regionBlock.tiles.Find(tile => !tile.walkable) != null) {
					removeRegionBlocks.Add(regionBlock);
					List<Tile> unwalkableTiles = new List<Tile>();
					List<Tile> walkableTiles = new List<Tile>();
					foreach (Tile tile in regionBlock.tiles) {
						if (tile.walkable) {
							walkableTiles.Add(tile);
						} else {
							unwalkableTiles.Add(tile);
						}
					}
					regionBlock.tiles.Clear();
					foreach (Tile unwalkableTile in unwalkableTiles) {
						if (unwalkableTile.regionBlock == null) {
							RegionBlock unwalkableRegionBlock = new RegionBlock(unwalkableTile.tileType, regionIndex);
							regionIndex += 1;
							Tile currentTile = unwalkableTile;
							List<Tile> frontier = new List<Tile>() { currentTile };
							List<Tile> checkedTiles = new List<Tile>() { currentTile };
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								unwalkableRegionBlock.tiles.Add(currentTile);
								currentTile.regionBlock = unwalkableRegionBlock;
								foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && !nTile.walkable && !checkedTiles.Contains(nTile) && unwalkableTiles.Contains(nTile) && nTile.regionBlock == null) {
										frontier.Add(nTile);
									}
									checkedTiles.Add(nTile);
								}
							}
							newRegionBlocks.Add(unwalkableRegionBlock);
						}
					}
					foreach (Tile walkableTile in walkableTiles) {
						if (walkableTile.regionBlock == null) {
							RegionBlock walkableRegionBlock = new RegionBlock(walkableTile.tileType, regionIndex);
							regionIndex += 1;
							Tile currentTile = walkableTile;
							List<Tile> frontier = new List<Tile>() { currentTile };
							List<Tile> checkedTiles = new List<Tile>() { currentTile };
							while (frontier.Count > 0) {
								currentTile = frontier[0];
								frontier.RemoveAt(0);
								walkableRegionBlock.tiles.Add(currentTile);
								currentTile.regionBlock = walkableRegionBlock;
								foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
									if (nTile != null && nTile.walkable && !checkedTiles.Contains(nTile) && walkableTiles.Contains(nTile) && nTile.regionBlock == null) {
										frontier.Add(nTile);
									}
									checkedTiles.Add(nTile);
								}
							}
							newRegionBlocks.Add(walkableRegionBlock);
						}
					}
				} else {
					foreach (Tile tile in regionBlock.tiles) {
						tile.regionBlock = regionBlock;
					}
				}
			}
			foreach (RegionBlock regionBlock in removeRegionBlocks) {
				regionBlocks.Remove(regionBlock);
			}
			removeRegionBlocks.Clear();
			regionBlocks.AddRange(newRegionBlocks);
			foreach (RegionBlock regionBlock in regionBlocks) {
				foreach (Tile tile in regionBlock.tiles) {
					foreach (Tile nTile in tile.horizontalSurroundingTiles) {
						if (nTile != null && nTile.regionBlock != tile.regionBlock && nTile.regionBlock != null && !regionBlock.horizontalSurroundingRegionBlocks.Contains(nTile.regionBlock)) {
							regionBlock.horizontalSurroundingRegionBlocks.Add(nTile.regionBlock);
						}
					}
					foreach (Tile nTile in tile.surroundingTiles) {
						if (nTile != null && nTile.regionBlock != tile.regionBlock && nTile.regionBlock != null && !regionBlock.surroundingRegionBlocks.Contains(nTile.regionBlock)) {
							regionBlock.surroundingRegionBlocks.Add(nTile.regionBlock);
						}
					}
					regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x + tile.obj.transform.position.x, regionBlock.averagePosition.y + tile.obj.transform.position.y);
				}
				regionBlock.averagePosition = new Vector2(regionBlock.averagePosition.x / regionBlock.tiles.Count, regionBlock.averagePosition.y / regionBlock.tiles.Count);
			}
		}

		private Region FindLowestRegion(List<Region> searchRegions) {
			Region lowestRegion = searchRegions[0];
			foreach (Region region in searchRegions) {
				if (region.id < lowestRegion.id) {
					lowestRegion = region;
				}
			}
			return lowestRegion;
		}

		private void RemoveEmptyRegions() {
			for (int i = 0; i < regions.Count; i++) {
				if (regions[i].tiles.Count <= 0) {
					regions.RemoveAt(i);
					i -= 1;
				}
			}

			for (int i = 0; i < regions.Count; i++) {
				regions[i].id = i;
			}
		}

		private void RemoveNonWalkableRegions() {
			List<Region> removeRegions = new List<Region>();
			foreach (Region region in regions) {
				if (!region.tileType.walkable) {
					foreach (Tile tile in region.tiles) {
						tile.ChangeRegion(null, false, false);
					}
					removeRegions.Add(region);
				}
			}
			foreach (Region region in removeRegions) {
				regions.Remove(region);
			}
		}

		public void SetInitialRegionVisibility() {
			// This only sets the "visible" variable itself, initial visibility is set on a per-tile basis in Map.Bitmasking()
			foreach (Region region in regions) {
				region.visible = region.IsVisibleToAColonist();
			}
		}

		public void RecalculateRegionsAtTile(Tile tile) {
			if (!tile.walkable) {
				List<Tile> orderedSurroundingTiles = new List<Tile>() {
					tile.surroundingTiles[0], tile.surroundingTiles[4], tile.surroundingTiles[1], tile.surroundingTiles[5],
					tile.surroundingTiles[2], tile.surroundingTiles[6], tile.surroundingTiles[3], tile.surroundingTiles[7]
				};
				List<List<Tile>> separateTileGroups = new List<List<Tile>>();
				int groupIndex = 0;
				for (int i = 0; i < orderedSurroundingTiles.Count; i++) {
					if (groupIndex == separateTileGroups.Count) {
						separateTileGroups.Add(new List<Tile>());
					}
					if (orderedSurroundingTiles[i] != null && orderedSurroundingTiles[i].walkable) {
						separateTileGroups[groupIndex].Add(orderedSurroundingTiles[i]);
						if (i == orderedSurroundingTiles.Count - 1 && groupIndex != 0) {
							if (orderedSurroundingTiles[i] != null && orderedSurroundingTiles[i].walkable && orderedSurroundingTiles[0] != null && orderedSurroundingTiles[0].walkable) {
								separateTileGroups[0].AddRange(separateTileGroups[groupIndex]);
								separateTileGroups.RemoveAt(groupIndex);
							}
						}
					} else {
						if (separateTileGroups[groupIndex].Count > 0) {
							groupIndex += 1;
						}
					}
				}
				List<Tile> horizontalGroups = new List<Tile>();
				foreach (List<Tile> tileGroup in separateTileGroups) {
					List<Tile> horizontalTilesInGroup = tileGroup.Where(groupTile => tile.horizontalSurroundingTiles.Contains(groupTile)).ToList();
					if (horizontalTilesInGroup.Count > 0) {
						horizontalGroups.Add(horizontalTilesInGroup[0]);
					}
				}
				if (horizontalGroups.Count > 1) {
					List<Tile> removeTiles = new List<Tile>();
					foreach (Tile startTile in horizontalGroups) {
						if (!removeTiles.Contains(startTile)) {
							foreach (Tile endTile in horizontalGroups) {
								if (!removeTiles.Contains(endTile) && startTile != endTile) {
									if (PathManager.PathExists(startTile, endTile, true, mapData.mapSize, PathManager.WalkableSetting.Walkable, PathManager.DirectionSetting.Horizontal)) {
										removeTiles.Add(endTile);
									}
								}
							}
						}
					}
					foreach (Tile removeTile in removeTiles) {
						horizontalGroups.Remove(removeTile);
					}
					if (horizontalGroups.Count > 1) {
						SetTileRegions(false, true);
					}
				}
			}
		}

		public void ReduceNoise() {
			ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 5f), new List<TileTypeGroup.TypeEnum>() { TileTypeGroup.TypeEnum.Water, TileTypeGroup.TypeEnum.Stone, TileTypeGroup.TypeEnum.Ground });
			ReduceNoise(Mathf.RoundToInt(mapData.mapSize / 2f), new List<TileTypeGroup.TypeEnum>() { TileTypeGroup.TypeEnum.Water });
		}

		private void ReduceNoise(int removeRegionsBelowSize, List<TileTypeGroup.TypeEnum> tileTypeGroupsToRemove) {
			foreach (Region region in regions) {
				if (tileTypeGroupsToRemove.Contains(region.tileType.groupType)) {
					if (region.tiles.Count < removeRegionsBelowSize) {
						/* --- This code is essentially copied from FindConnectedRegions() */
						foreach (Tile tile in region.tiles) {
							foreach (Tile nTile in tile.horizontalSurroundingTiles) {
								if (nTile != null && nTile.region != null && nTile.region != region && !region.connectedRegions.Contains(nTile.region)) {
									region.connectedRegions.Add(nTile.region);
								}
							}
						}
						/* --- This code is essentially copied from MergeConnectedRegions() */
						if (region.connectedRegions.Count > 0) {
							Region lowestRegion = FindLowestRegion(region.connectedRegions);
							foreach (Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
								tile.ChangeRegion(lowestRegion, true, false);
							}
							region.tiles.Clear(); // Clear the tiles from this region
						}
					}
				}
			}
			RemoveEmptyRegions();
		}

		public List<River> rivers = new List<River>();
		public List<River> largeRivers = new List<River>();

		public Dictionary<Region, Tile> drainageBasins = new Dictionary<Region, Tile>();
		public int drainageBasinID = 0;

		public void DetermineDrainageBasins() {
			drainageBasins.Clear();
			drainageBasinID = 0;

			List<Tile> tilesByHeight = tiles.OrderBy(tile => tile.height).ToList();
			foreach (Tile tile in tilesByHeight) {
				if (tile.tileType.groupType != TileTypeGroup.TypeEnum.Stone && tile.drainageBasin == null) {
					Region drainageBasin = new Region(null, drainageBasinID);
					drainageBasinID += 1;

					Tile currentTile = tile;

					List<Tile> checkedTiles = new List<Tile> { currentTile };
					List<Tile> frontier = new List<Tile>() { currentTile };

					while (frontier.Count > 0) {
						currentTile = frontier[0];
						frontier.RemoveAt(0);

						drainageBasin.tiles.Add(currentTile);
						currentTile.drainageBasin = drainageBasin;

						foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone && nTile.drainageBasin == null) {
								if (nTile.height * 1.2f >= currentTile.height) {
									frontier.Add(nTile);
									checkedTiles.Add(nTile);
								}
							}
						}
					}
					drainageBasins.Add(drainageBasin, tile);
				}
			}
		}

		public class River {
			public Tile startTile;
			public Tile centreTile;
			public Tile endTile;
			public List<Tile> tiles = new List<Tile>();
			public int expandRadius;
			public bool ignoreStone;

			public River(Tile startTile, Tile centreTile, Tile endTile, int expandRadius, bool ignoreStone, Map map, bool performPathfinding) {
				this.startTile = startTile;
				this.centreTile = centreTile;
				this.endTile = endTile;
				this.expandRadius = expandRadius;
				this.ignoreStone = ignoreStone;

				if (performPathfinding) {
					if (centreTile != null) {
						tiles.AddRange(map.RiverPathfinding(startTile, centreTile, expandRadius, ignoreStone));
						tiles.AddRange(map.RiverPathfinding(centreTile, endTile, expandRadius, ignoreStone));
					} else {
						tiles = map.RiverPathfinding(startTile, endTile, expandRadius, ignoreStone);
					}
				}
			}
		}

		void CreateLargeRivers() {
			largeRivers.Clear();
			if (mapData.isRiver) {
				int riverEndRiverIndex = mapData.surroundingPlanetTileRivers.OrderByDescending(i => i).ToList()[0];
				int riverEndListIndex = mapData.surroundingPlanetTileRivers.IndexOf(riverEndRiverIndex);

				List<Tile> validEndTiles = sortedEdgeTiles[riverEndListIndex].Where(tile => Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverEndListIndex][0].obj.transform.position) >= 10 && Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverEndListIndex][sortedEdgeTiles[riverEndListIndex].Count - 1].obj.transform.position) >= 10).ToList();
				Tile riverEndTile = validEndTiles[UnityEngine.Random.Range(0, validEndTiles.Count)];

				int riverStartListIndex = 0;
				foreach (int riverStartRiverIndex in mapData.surroundingPlanetTileRivers) {
					if (riverStartRiverIndex != -1 && riverStartRiverIndex != riverEndRiverIndex) {
						int expandRadius = UnityEngine.Random.Range(1, 3) * Mathf.CeilToInt(mapData.mapSize / 100f);
						List<Tile> validStartTiles = sortedEdgeTiles[riverStartListIndex].Where(tile => Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverStartListIndex][0].obj.transform.position) >= 10 && Vector2.Distance(tile.obj.transform.position, sortedEdgeTiles[riverStartListIndex][sortedEdgeTiles[riverStartListIndex].Count - 1].obj.transform.position) >= 10).ToList();
						Tile riverStartTile = validStartTiles[UnityEngine.Random.Range(0, validStartTiles.Count)];
						List<Tile> possibleCentreTiles = tiles.Where(t => Vector2.Distance(new Vector2(mapData.mapSize / 2f, mapData.mapSize / 2f), t.obj.transform.position) < mapData.mapSize / 5f).ToList();
						River river = new River(riverStartTile, possibleCentreTiles[UnityEngine.Random.Range(0, possibleCentreTiles.Count)], riverEndTile, expandRadius, true, this, true);
						if (river.tiles.Count > 0) {
							largeRivers.Add(river);
						} else {
							Debug.LogWarning("Large River has no tiles. startTile: " + riverStartTile.obj.transform.position + " endTile: " + riverEndTile.obj.transform.position);
						}
					}
					riverStartListIndex += 1;
				}
			}
		}

		void CreateRivers() {
			rivers.Clear();
			Dictionary<Tile, Tile> riverStartTiles = new Dictionary<Tile, Tile>();
			foreach (KeyValuePair<Region, Tile> kvp in drainageBasins) {
				Region drainageBasin = kvp.Key;
				if (drainageBasin.tiles.Find(o => o.tileType.groupType == TileTypeGroup.TypeEnum.Water) != null && drainageBasin.tiles.Find(o => o.horizontalSurroundingTiles.Find(o2 => o2 != null && o2.tileType.groupType == TileTypeGroup.TypeEnum.Stone) != null) != null) {
					foreach (Tile tile in drainageBasin.tiles) {
						if (tile.walkable && tile.tileType.groupType != TileTypeGroup.TypeEnum.Water && tile.horizontalSurroundingTiles.Find(o => o != null && o.tileType.groupType == TileTypeGroup.TypeEnum.Stone) != null) {
							riverStartTiles.Add(tile, kvp.Value);
						}
					}
				}
			}
			for (int i = 0; i < mapData.mapSize / 10f && i < riverStartTiles.Count; i++) {
				Tile riverStartTile = Enumerable.ToList(riverStartTiles.Keys)[UnityEngine.Random.Range(0, riverStartTiles.Count)];
				Tile riverEndTile = riverStartTiles[riverStartTile];
				List<Tile> removeTiles = new List<Tile>();
				foreach (KeyValuePair<Tile, Tile> kvp in riverStartTiles) {
					if (Vector2.Distance(kvp.Key.obj.transform.position, riverStartTile.obj.transform.position) < 5f) {
						removeTiles.Add(kvp.Key);
					}
				}
				foreach (Tile removeTile in removeTiles) {
					riverStartTiles.Remove(removeTile);
				}
				removeTiles.Clear();

				River river = new River(riverStartTile, null, riverEndTile, 0, false, this, true);
				if (river.tiles.Count > 0) {
					rivers.Add(river);
				} else {
					Debug.LogWarning("River has no tiles. startTile: " + riverStartTile.obj.transform.position + " endTile: " + riverEndTile.obj.transform.position);
				}
			}
		}

		public List<Tile> RiverPathfinding(Tile riverStartTile, Tile riverEndTile, int expandRadius, bool ignoreStone) {
			PathManager.PathfindingTile currentTile = new PathManager.PathfindingTile(riverStartTile, null, 0);

			List<PathManager.PathfindingTile> checkedTiles = new List<PathManager.PathfindingTile>() { currentTile };
			List<PathManager.PathfindingTile> frontier = new List<PathManager.PathfindingTile>() { currentTile };

			List<Tile> river = new List<Tile>();

			while (frontier.Count > 0) {
				currentTile = frontier[0];
				frontier.RemoveAt(0);

				if (currentTile.tile == riverEndTile || (expandRadius == 0 && (currentTile.tile.tileType.groupType == TileTypeGroup.TypeEnum.Water || (currentTile.tile.horizontalSurroundingTiles.Find(tile => tile != null && tile.tileType.groupType == TileTypeGroup.TypeEnum.Water && RiversContainTile(tile, true).Key == null) != null)))) {
					while (currentTile != null) {
						river.Add(currentTile.tile);
						currentTile.tile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.GrassWater), true, false, false);
						currentTile = currentTile.cameFrom;
					}
					break;
				}

				foreach (Tile nTile in currentTile.tile.horizontalSurroundingTiles) {
					if (nTile != null && checkedTiles.Find(checkedTile => checkedTile.tile == nTile) == null && (ignoreStone || nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
						if (rivers.Find(otherRiver => otherRiver.tiles.Find(riverTile => nTile == riverTile) != null) != null) {
							frontier.Clear();
							frontier.Add(new PathManager.PathfindingTile(nTile, currentTile, 0));
							nTile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.GrassWater), true, false, false);
							break;
						}
						float cost = Vector2.Distance(nTile.obj.transform.position, riverEndTile.obj.transform.position) + (nTile.height * (mapData.mapSize / 10f)) + UnityEngine.Random.Range(0, 10);
						PathManager.PathfindingTile pTile = new PathManager.PathfindingTile(nTile, currentTile, cost);
						frontier.Add(pTile);
						checkedTiles.Add(pTile);
					}
				}
				frontier = frontier.OrderBy(frontierTile => frontierTile.cost).ToList();
			}

			if (river.Count == 0) {
				return river;
			}

			if (expandRadius > 0) {
				float expandedExpandRadius = expandRadius * UnityEngine.Random.Range(2f, 4f);
				List<Tile> riverAdditions = new List<Tile>();
				riverAdditions.AddRange(river);
				foreach (Tile riverTile in river) {
					riverTile.SetTileHeight(CalculateLargeRiverTileHeight(expandRadius, 0));

					List<Tile> expandFrontier = new List<Tile>() { riverTile };
					List<Tile> checkedExpandTiles = new List<Tile>() { riverTile };
					while (expandFrontier.Count > 0) {
						Tile expandTile = expandFrontier[0];
						expandFrontier.RemoveAt(0);
						float distanceExpandTileRiverTile = Vector2.Distance(expandTile.obj.transform.position, riverTile.obj.transform.position);
						float newRiverHeight = CalculateLargeRiverTileHeight(expandRadius, distanceExpandTileRiverTile);
						float newRiverBankHeight = CalculateLargeRiverBankTileHeight(expandRadius, distanceExpandTileRiverTile);
						if (distanceExpandTileRiverTile <= expandRadius) {
							if (!riverAdditions.Contains(expandTile)) {
								riverAdditions.Add(expandTile);
								expandTile.SetTileHeight(newRiverHeight);
							}
						} else if (!riverAdditions.Contains(expandTile) && expandTile.height > newRiverBankHeight) {
							expandTile.SetTileHeight(newRiverBankHeight);
						}
						foreach (Tile nTile in expandTile.surroundingTiles) {
							if (nTile != null && !checkedExpandTiles.Contains(nTile) && (ignoreStone || nTile.tileType.groupType != TileTypeGroup.TypeEnum.Stone)) {
								if (Vector2.Distance(nTile.obj.transform.position, riverTile.obj.transform.position) <= expandedExpandRadius) {
									expandFrontier.Add(nTile);
									checkedExpandTiles.Add(nTile);
								}
							}
						}
					}
				}
				river.AddRange(riverAdditions);
			}

			return river;
		}

		private float CalculateLargeRiverTileHeight(int expandRadius, float distanceExpandTileRiverTile) {
			float height = (mapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Water] / expandRadius) * distanceExpandTileRiverTile;//(2 * mapData.terrainTypeHeights[TileTypes.GrassWater]) * (distanceExpandTileRiverTile / expandedExpandRadius);
			height -= 0.01f;
			return Mathf.Clamp(height, 0f, 1f);
		}

		private float CalculateLargeRiverBankTileHeight(int expandRadius, float distanceExpandTileRiverTile) {
			float height = CalculateLargeRiverTileHeight(expandRadius, distanceExpandTileRiverTile / 2f);
			height += (mapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Water] / 2f);
			return Mathf.Clamp(height, 0f, 1f);
		}

		public KeyValuePair<Tile, River> RiversContainTile(Tile tile, bool includeLargeRivers) {
			foreach (River river in includeLargeRivers ? rivers.Concat(largeRivers) : rivers) {
				foreach (Tile riverTile in river.tiles) {
					if (riverTile == tile) {
						return new KeyValuePair<Tile, River>(riverTile, river);
					}
				}
			}
			return new KeyValuePair<Tile, River>(null, null);
		}

		public float TemperatureFromMapLatitude(float yPos, float temperatureRange, float temperatureOffset, int mapSize) {
			return ((-2 * Mathf.Abs((yPos - (mapSize / 2f)) / ((mapSize / 100f) / (temperatureRange / 50f)))) + temperatureRange) + temperatureOffset + (UnityEngine.Random.Range(-50f, 50f));
		}

		public void CalculateTemperature() {
			foreach (Tile tile in tiles) {
				if (mapData.planetTemperature) {
					tile.temperature = TemperatureFromMapLatitude(tile.position.y, mapData.temperatureRange, mapData.temperatureOffset, mapData.mapSize);
				} else {
					tile.temperature = mapData.averageTemperature;
				}
				tile.temperature += -(50f * Mathf.Pow(tile.height - 0.5f, 3));
			}

			AverageTileTemperatures();
		}

		void AverageTileTemperatures() {
			int numPasses = 3; // 3
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTileTemperatures = new List<float>();

				foreach (Tile tile in tiles) {
					float averageTemperature = tile.temperature;
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						if (nTile != null) {
							numValidTiles += 1;
							averageTemperature += nTile.temperature;
						}
					}
					averageTemperature /= numValidTiles;
					averageTileTemperatures.Add(averageTemperature);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].temperature = averageTileTemperatures[k];
				}
			}
		}

		private static readonly List<int> oppositeDirectionTileMap = new List<int>() { 2, 3, 0, 1, 6, 7, 4, 5 };
		private static readonly List<List<float>> windStrengthMap = new List<List<float>>() {
			new List<float>(){ 1.0f,0.6f,0.1f,0.6f,0.8f,0.2f,0.2f,0.8f },
			new List<float>(){ 0.6f,1.0f,0.6f,0.1f,0.8f,0.8f,0.2f,0.2f },
			new List<float>(){ 0.1f,0.6f,1.0f,0.6f,0.2f,0.8f,0.8f,0.2f },
			new List<float>(){ 0.6f,0.1f,0.6f,1.0f,0.2f,0.2f,0.8f,0.8f },
			new List<float>(){ 0.8f,0.8f,0.2f,0.2f,1.0f,0.6f,0.1f,0.6f },
			new List<float>(){ 0.2f,0.8f,0.8f,0.2f,0.6f,1.0f,0.6f,0.1f },
			new List<float>(){ 0.2f,0.2f,0.8f,0.8f,0.1f,0.6f,1.0f,0.6f },
			new List<float>(){ 0.8f,0.2f,0.2f,0.8f,0.6f,0.1f,0.6f,1.0f }
		};

		public int primaryWindDirection = -1;
		public void CalculatePrecipitation() {
			int windDirectionMin = 0;
			int windDirectionMax = 7;

			List<List<float>> directionPrecipitations = new List<List<float>>();
			for (int i = 0; i < windDirectionMin; i++) {
				directionPrecipitations.Add(new List<float>());
			}
			for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) { // 0 - up, 1 - right, 2 - down, 3 - left, 4 - up/right, 5 - down/right, 6 - down-left, 7 - up/left
				int windDirection = i;
				if (windDirection <= 3) { // Wind is going horizontally/vertically
					bool yStartAtTop = (windDirection == 2);
					bool xStartAtRight = (windDirection == 3);

					for (int y = (yStartAtTop ? mapData.mapSize - 1 : 0); (yStartAtTop ? y >= 0 : y < mapData.mapSize); y += (yStartAtTop ? -1 : 1)) {
						for (int x = (xStartAtRight ? mapData.mapSize - 1 : 0); (xStartAtRight ? x >= 0 : x < mapData.mapSize); x += (xStartAtRight ? -1 : 1)) {
							Tile tile = sortedTiles[y][x];
							Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
							SetTilePrecipitation(tile, previousTile, mapData.planetTemperature);
						}
					}
				} else { // Wind is going diagonally
					bool up = (windDirection == 4 || windDirection == 7);
					bool left = (windDirection == 6 || windDirection == 7);
					int mapSize2x = mapData.mapSize * 2;
					for (int k = (up ? 0 : mapSize2x); (up ? k < mapSize2x : k >= 0); k += (up ? 1 : -1)) {
						for (int x = (left ? k : 0); (left ? x >= 0 : x <= k); x += (left ? -1 : 1)) {
							int y = k - x;
							if (y < mapData.mapSize && x < mapData.mapSize) {
								Tile tile = sortedTiles[y][x];
								Tile previousTile = tile.surroundingTiles[oppositeDirectionTileMap[windDirection]];
								SetTilePrecipitation(tile, previousTile, mapData.planetTemperature);
							}
						}
					}
				}
				List<float> singleDirectionPrecipitations = new List<float>();
				foreach (Tile tile in tiles) {
					singleDirectionPrecipitations.Add(tile.GetPrecipitation());
					tile.SetPrecipitation(0);
				}
				directionPrecipitations.Add(singleDirectionPrecipitations);
			}

			if (mapData.primaryWindDirection == -1) {
				primaryWindDirection = UnityEngine.Random.Range(windDirectionMin, (windDirectionMax + 1));
			} else {
				primaryWindDirection = mapData.primaryWindDirection;
			}

			float windStrengthMapSum = 0;
			for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) {
				windStrengthMapSum += windStrengthMap[primaryWindDirection][i];
			}

			for (int t = 0; t < tiles.Count; t++) {
				Tile tile = tiles[t];
				tile.SetPrecipitation(0);
				for (int i = windDirectionMin; i < (windDirectionMax + 1); i++) {
					tile.SetPrecipitation(tile.GetPrecipitation() + (directionPrecipitations[i][t] * windStrengthMap[primaryWindDirection][i]));
				}
				tile.SetPrecipitation(tile.GetPrecipitation() / windStrengthMapSum);
			}

			AverageTilePrecipitations();

			foreach (Tile tile in tiles) {
				if (Mathf.RoundToInt(mapData.averagePrecipitation) != -1) {
					tile.SetPrecipitation((tile.GetPrecipitation() + mapData.averagePrecipitation) / 2f);
				}
				tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
			}
		}

		private void SetTilePrecipitation(Tile tile, Tile previousTile, bool planet) {
			if (planet) {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation(((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier) * (mapData.mapSize / 5f));
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.9f);
					} else {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * 0.95f);
					}
				} else {
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation(1f);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(1f);
					} else {
						tile.SetPrecipitation(0.1f);
					}
				}
			} else {
				if (previousTile != null) {
					float previousTileDistanceMultiplier = -Vector2.Distance(tile.obj.transform.position, previousTile.obj.transform.position) + 2;
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						float waterMultiplier = (mapData.mapSize / 5f);
						if (RiversContainTile(tile, true).Value != null) {
							waterMultiplier *= 5;
						}
						tile.SetPrecipitation(((previousTile.GetPrecipitation() + (Mathf.Approximately(previousTile.GetPrecipitation(), 0f) ? 0.01f : 0f)) * previousTileDistanceMultiplier) * waterMultiplier);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * UnityEngine.Random.Range(0.95f, 0.99f));
					} else {
						tile.SetPrecipitation((previousTile.GetPrecipitation() * previousTileDistanceMultiplier) * UnityEngine.Random.Range(0.98f, 1f));
					}
				} else {
					if (tile.tileType.classes[TileType.ClassEnum.LiquidWater]) {
						tile.SetPrecipitation(1f);
					} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
						tile.SetPrecipitation(1f);
					} else {
						tile.SetPrecipitation(mapData.averagePrecipitation);
					}
				}
			}
			tile.SetPrecipitation(ChangePrecipitationByTemperature(tile.GetPrecipitation(), tile.temperature));
			tile.SetPrecipitation(Mathf.Clamp(tile.GetPrecipitation(), 0f, 1f));
		}

		private float ChangePrecipitationByTemperature(float precipitation, float temperature) {
			return precipitation * (Mathf.Clamp(-Mathf.Pow((temperature - 30) / (90 - 30), 3) + 1, 0f, 1f)); // Less precipitation as the temperature gets higher
		}

		public void AverageTilePrecipitations() {
			int numPasses = 5;
			for (int i = 0; i < numPasses; i++) {
				List<float> averageTilePrecipitations = new List<float>();

				foreach (Tile tile in tiles) {
					float averagePrecipitation = tile.GetPrecipitation();
					int numValidTiles = 1;
					for (int t = 0; t < tile.surroundingTiles.Count; t++) {
						Tile nTile = tile.surroundingTiles[t];
						if (nTile != null) {
							numValidTiles += 1;
							averagePrecipitation += nTile.GetPrecipitation();
						}
					}
					averagePrecipitation /= numValidTiles;
					averageTilePrecipitations.Add(averagePrecipitation);
				}

				for (int k = 0; k < tiles.Count; k++) {
					tiles[k].SetPrecipitation(averageTilePrecipitations[k]);
				}
			}
		}

		public void SetBiomes(bool setPlant) {

			/* Biome Testing
			for (int y = mapData.mapSize - 1; y >= 0; y--) {
				for (int x = 0; x < mapData.mapSize; x++) {
					Tile tile = sortedTiles[y][x];
					tile.SetTileType(TileType.GetTileTypeByEnum(TileType.TypeEnum.Grass), false, false, false);
					tile.temperature = 2f * (mapData.temperatureRange * (y / (float)mapData.mapSize) - (mapData.temperatureRange / 2f));
					tile.SetPrecipitation(x / (float)mapData.mapSize);
				}
			}
			*/

			foreach (Tile tile in tiles) {
				foreach (Biome biome in TileManager.Biome.biomes) {
					foreach (Biome.Range range in biome.ranges) {
						if (range.IsInRange(tile.GetPrecipitation(), tile.temperature)) {
							tile.SetBiome(biome, setPlant);
							if (tile.plant != null && tile.plant.small) {
								tile.plant.growthProgress = UnityEngine.Random.Range(0, TimeManager.dayLengthSeconds * 4);
							}
						}
					}
				}
			}
		}

		public void SetMapEdgeTiles() {
			edgeTiles.Clear();
			for (int i = 1; i < mapData.mapSize - 1; i++) {
				edgeTiles.Add(sortedTiles[0][i]);
				edgeTiles.Add(sortedTiles[mapData.mapSize - 1][i]);
				edgeTiles.Add(sortedTiles[i][0]);
				edgeTiles.Add(sortedTiles[i][mapData.mapSize - 1]);
			}
			edgeTiles.Add(sortedTiles[0][0]);
			edgeTiles.Add(sortedTiles[0][mapData.mapSize - 1]);
			edgeTiles.Add(sortedTiles[mapData.mapSize - 1][0]);
			edgeTiles.Add(sortedTiles[mapData.mapSize - 1][mapData.mapSize - 1]);
		}

		public void SetSortedMapEdgeTiles() {
			sortedEdgeTiles.Clear();

			int sideNum = -1;
			List<Tile> tilesOnThisEdge = null;
			for (int i = 0; i <= mapData.mapSize; i++) {
				i %= mapData.mapSize;
				if (i == 0) {
					sideNum += 1;
					sortedEdgeTiles.Add(sideNum, new List<Tile>());
					tilesOnThisEdge = sortedEdgeTiles[sideNum];
				}
				if (sideNum == 0) {
					tilesOnThisEdge.Add(sortedTiles[mapData.mapSize - 1][i]);
				} else if (sideNum == 1) {
					tilesOnThisEdge.Add(sortedTiles[i][mapData.mapSize - 1]);
				} else if (sideNum == 2) {
					tilesOnThisEdge.Add(sortedTiles[0][i]);
				} else if (sideNum == 3) {
					tilesOnThisEdge.Add(sortedTiles[i][0]);
				} else {
					break;
				}
			}
		}

		public void SetRoofs() {
			float roofHeightMultiplier = 1.25f;
			foreach (Tile tile in tiles) {
				tile.SetRoof(tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone && tile.height >= mapData.terrainTypeHeights[TileTypeGroup.TypeEnum.Stone] * roofHeightMultiplier);
			}
		}

		public void SetResourceVeins() {

			List<Tile> stoneTiles = new List<Tile>();
			foreach (RegionBlock regionBlock in regionBlocks) {
				if (regionBlock.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					stoneTiles.AddRange(regionBlock.tiles);
				}
			}
			if (stoneTiles.Count > 0) {
				foreach (ResourceVein resourceVein in ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Stone)) {
					PlaceResourceVeins(resourceVein, stoneTiles);
				}
			}

			List<Tile> coastTiles = new List<Tile>();
			foreach (RegionBlock regionBlock in regionBlocks) {
				if (regionBlock.tileType.groupType == TileTypeGroup.TypeEnum.Water) {
					foreach (Tile tile in regionBlock.tiles) {
						if (tile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileTypeGroup.TypeEnum.Water) != null) {
							coastTiles.Add(tile);
						}
					}
				}
			}
			if (coastTiles.Count > 0) {
				foreach (ResourceVein resourceVein in ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Coast)) {
					PlaceResourceVeins(resourceVein, coastTiles);
				}
			}
		}

		void PlaceResourceVeins(ResourceVein resourceVeinData, List<Tile> mediumTiles) {
			List<Tile> previousVeinStartTiles = new List<Tile>();
			for (int i = 0; i < Mathf.CeilToInt(mapData.mapSize / (float)resourceVeinData.numVeinsByMapSize); i++) {
				List<Tile> validVeinStartTiles = mediumTiles.Where(tile => !resourceVeinData.tileTypes.ContainsValue(tile.tileType.type) && resourceVeinData.tileTypes.ContainsKey(tile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](tile)).ToList();
				foreach (Tile previousVeinStartTile in previousVeinStartTiles) {
					List<Tile> removeTiles = new List<Tile>();
					foreach (Tile validVeinStartTile in validVeinStartTiles) {
						if (Vector2.Distance(validVeinStartTile.obj.transform.position, previousVeinStartTile.obj.transform.position) < resourceVeinData.veinDistance) {
							removeTiles.Add(validVeinStartTile);
						}
					}
					foreach (Tile removeTile in removeTiles) {
						validVeinStartTiles.Remove(removeTile);
					}
				}
				if (validVeinStartTiles.Count > 0) {

					int veinSizeMax = resourceVeinData.veinSize + UnityEngine.Random.Range(-resourceVeinData.veinSizeRange, resourceVeinData.veinSizeRange);

					Tile veinStartTile = validVeinStartTiles[UnityEngine.Random.Range(0, validVeinStartTiles.Count)];
					previousVeinStartTiles.Add(veinStartTile);

					List<Tile> frontier = new List<Tile>() { veinStartTile };
					List<Tile> checkedTiles = new List<Tile>();
					Tile currentTile = veinStartTile;

					int veinSize = 0;

					while (frontier.Count > 0) {
						currentTile = frontier[UnityEngine.Random.Range(0, frontier.Count)];
						frontier.RemoveAt(0);
						checkedTiles.Add(currentTile);

						currentTile.SetTileType(TileType.GetTileTypeByEnum(resourceVeinData.tileTypes[currentTile.tileType.groupType]), false, true, false);

						foreach (Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (nTile != null && !checkedTiles.Contains(nTile) && !resourceVeinData.tileTypes.Values.Contains(nTile.tileType.type)) {
								if (resourceVeinData.tileTypes.ContainsKey(nTile.tileType.groupType) && ResourceVein.resourceVeinValidTileFunctions[resourceVeinData.resourceType](nTile)) {
									frontier.Add(nTile);
								}
							}
						}

						veinSize += 1;

						if (veinSize >= veinSizeMax) {
							break;
						}
					}
				}
			}
		}

		public static readonly Dictionary<int, int> bitmaskMap = new Dictionary<int, int>() {
			{ 19, 16 },
			{ 23, 17 },
			{ 27, 18 },
			{ 31, 19 },
			{ 38, 20 },
			{ 39, 21 },
			{ 46, 22 },
			{ 47, 23 },
			{ 55, 24 },
			{ 63, 25 },
			{ 76, 26 },
			{ 77, 27 },
			{ 78, 28 },
			{ 79, 29 },
			{ 95, 30 },
			{ 110, 31 },
			{ 111, 32 },
			{ 127, 33 },
			{ 137, 34 },
			{ 139, 35 },
			{ 141, 36 },
			{ 143, 37 },
			{ 155, 38 },
			{ 159, 39 },
			{ 175, 40 },
			{ 191, 41 },
			{ 205, 42 },
			{ 207, 43 },
			{ 223, 44 },
			{ 239, 45 },
			{ 255, 46 }
		};
		public static readonly Dictionary<int, List<int>> diagonalCheckMap = new Dictionary<int, List<int>>() {
			{ 4, new List<int>() { 0, 1 } },
			{ 5, new List<int>() { 1, 2 } },
			{ 6, new List<int>() { 2, 3 } },
			{ 7, new List<int>() { 3, 0 } }
		};

		public int BitSum(
			List<TileType.TypeEnum> compareTileTypes,
			List<ResourceManager.ObjectEnum> compareObjectTypes,
			List<Tile> tilesToSum,
			bool includeMapEdge
		) {
			//if (compareObjectTypes == null) {
			//	compareObjectTypes = new List<ResourceManager.ObjectEnum>();
			//}

			int sum = 0;
			for (int i = 0; i < tilesToSum.Count; i++) {
				if (tilesToSum[i] != null) {
					if (compareTileTypes.Contains(tilesToSum[i].tileType.type)
						//|| compareObjectTypes.Intersect(tilesToSum[i].objectInstances.Values.Select(obj => obj.prefab.type)).ToList().Count > 0
					) {
						bool ignoreTile = false;
						if (diagonalCheckMap.ContainsKey(i)) {
							List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
							List<Tile> similarTiles = surroundingHorizontalTiles.Where(tile =>
								tile != null
								&& (compareTileTypes.Contains(tile.tileType.type)
									/*|| compareObjectTypes.Intersect(tile.objectInstances.Values.Select(obj => obj.prefab.type)).ToList().Count > 0*/)
							).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							sum += Mathf.RoundToInt(Mathf.Pow(2, i));
						}
					}
				} else if (includeMapEdge) {
					if (tilesToSum.Find(tile => tile != null && tilesToSum.IndexOf(tile) <= 3 && !compareTileTypes.Contains(tile.tileType.type)) == null) {
						sum += Mathf.RoundToInt(Mathf.Pow(2, i));
					} else {
						if (i <= 3) {
							sum += Mathf.RoundToInt(Mathf.Pow(2, i));
						} else {
							List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]], tilesToSum[diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)) == null) {
								sum += Mathf.RoundToInt(Mathf.Pow(2, i));
							}
						}
					}
				}
			}
			return sum;
		}

		void BitmaskTile(Tile tile, bool includeDiagonalSurroundingTiles, bool customBitSumInputs, List<TileType.TypeEnum> customCompareTileTypes, bool includeMapEdge) {
			int sum = 0;
			List<Tile> surroundingTilesToUse = includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles;
			if (customBitSumInputs) {
				sum = BitSum(customCompareTileTypes, null, surroundingTilesToUse, includeMapEdge);
			} else {
				if (RiversContainTile(tile, false).Key != null) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, false);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Water) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, includeMapEdge);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, includeMapEdge);
					// Not-fully-working implementation of walls and stone connecting
					//sum += GameManager.resourceM.BitSumObjects(
					//	GameManager.resourceM.GetObjectPrefabSubGroupByEnum(ResourceManager.ObjectSubGroupEnum.Walls).prefabs.Select(prefab => prefab.type).ToList(),
					//	surroundingTilesToUse
					//);
				} else if (tile.tileType.groupType == TileTypeGroup.TypeEnum.Hole) {
					sum = BitSum(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Hole).tileTypes.Select(tt => tt.type).ToList(), null, surroundingTilesToUse, false);
				} else {
					sum = BitSum(new List<TileType.TypeEnum>() { tile.tileType.type }, null, surroundingTilesToUse, includeMapEdge);
				}
			}
			if ((sum < 16) || (bitmaskMap[sum] != 46)) {
				if (sum >= 16) {
					sum = bitmaskMap[sum];
				}
				if (tile.tileType.classes[TileType.ClassEnum.LiquidWater] && RiversContainTile(tile, false).Key != null) {
					tile.sr.sprite = tile.tileType.riverSprites[sum];
				} else {
					try {
						tile.sr.sprite = tile.tileType.bitmaskSprites[sum];
					} catch (ArgumentOutOfRangeException) {
						Debug.LogWarning("BitmaskTile Error: Index " + sum + " does not exist in bitmaskSprites. " + tile.obj.transform.position + " " + tile.tileType.type + " " + tile.tileType.bitmaskSprites.Count);
					}
				}
			} else {
				if (tile.tileType.baseSprites.Count > 0 && !tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
					tile.sr.sprite = tile.tileType.baseSprites[UnityEngine.Random.Range(0, tile.tileType.baseSprites.Count)];
				}
				if (ResourceVein.GetResourceVeinsByGroup(ResourceVein.GroupEnum.Stone).Find(rvd => rvd.tileTypes.ContainsValue(tile.tileType.type)) != null) {
					TileType biomeTileType = tile.biome.tileTypes[tile.tileType.groupType];
					tile.sr.sprite = biomeTileType.baseSprites[UnityEngine.Random.Range(0, biomeTileType.baseSprites.Count)];
				}
			}
		}

		public void Bitmasking(List<Tile> tilesToBitmask, bool careAboutColonistVisibility, bool recalculateLighting) {
			foreach (Tile tile in tilesToBitmask) {
				if (tile != null) {
					if (!careAboutColonistVisibility || tile.IsVisibleToAColonist()) {
						tile.SetVisible(true); // "true" on "recalculateBitmasking" would cause stack overflow
						if (tile.tileType.bitmasking) {
							BitmaskTile(tile, true, false, null, true);
						} else {
							if (!tile.tileType.baseSprites.Contains(tile.sr.sprite)) {
								tile.sr.sprite = tile.tileType.baseSprites[UnityEngine.Random.Range(0, tile.tileType.baseSprites.Count)];
							}
						}
					} else {
						tile.SetVisible(false); // "true" on "recalculateBitmasking" would cause stack overflow
					}
				}
			}
			BitmaskRiverStartTiles();
			if (recalculateLighting) {
				RecalculateLighting(tilesToBitmask, true);
			}
		}

		void BitmaskRiverStartTiles() {
			foreach (River river in rivers) {
				List<TileType.TypeEnum> compareTileTypes = new List<TileType.TypeEnum>();
				compareTileTypes.AddRange(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Water).tileTypes.Select(tt => tt.type).ToList());
				compareTileTypes.AddRange(TileTypeGroup.GetTileTypeGroupByEnum(TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tt => tt.type).ToList());
				BitmaskTile(river.startTile, false, true, compareTileTypes, false/*river.expandRadius > 0*/);
			}
		}

		private readonly List<RegionBlock> visibleRegionBlocks = new List<RegionBlock>();
		private RegionBlock centreRegionBlock;
		private int lastOrthographicSize = -1;

		public void DetermineVisibleRegionBlocks() {
			RegionBlock newCentreRegionBlock = GetTileFromPosition(GameManager.cameraM.cameraGO.transform.position).squareRegionBlock;
			if (newCentreRegionBlock != centreRegionBlock || Mathf.RoundToInt(GameManager.cameraM.camera.orthographicSize) != lastOrthographicSize) {
				visibleRegionBlocks.Clear();
				lastOrthographicSize = Mathf.RoundToInt(GameManager.cameraM.camera.orthographicSize);
				centreRegionBlock = newCentreRegionBlock;
				float maxVisibleRegionBlockDistance = GameManager.cameraM.camera.orthographicSize * ((float)Screen.width / Screen.height);
				List<RegionBlock> frontier = new List<RegionBlock>() { centreRegionBlock };
				List<RegionBlock> checkedBlocks = new List<RegionBlock>() { centreRegionBlock };
				while (frontier.Count > 0) {
					RegionBlock currentRegionBlock = frontier[0];
					frontier.RemoveAt(0);
					visibleRegionBlocks.Add(currentRegionBlock);
					float currentRegionBlockCameraDistance = Vector2.Distance(currentRegionBlock.averagePosition, GameManager.cameraM.cameraGO.transform.position);
					foreach (RegionBlock nBlock in currentRegionBlock.surroundingRegionBlocks) {
						if (currentRegionBlockCameraDistance <= maxVisibleRegionBlockDistance) {
							if (!checkedBlocks.Contains(nBlock)) {
								frontier.Add(nBlock);
								checkedBlocks.Add(nBlock);
							}
						} else {
							if (!checkedBlocks.Contains(nBlock)) {
								visibleRegionBlocks.Add(nBlock);
								checkedBlocks.Add(nBlock);
							}
						}
					}
				}
				SetTileBrightness(GameManager.timeM.tileBrightnessTime, false);
			}
		}

		public void SetTileBrightness(float time, bool forceUpdate) {
			Color newColour = GetTileColourAtHour(time);
			foreach (RegionBlock visibleRegionBlock in visibleRegionBlocks) {
				if (forceUpdate || !Mathf.Approximately(visibleRegionBlock.lastBrightnessUpdate, time)) {
					visibleRegionBlock.lastBrightnessUpdate = time;
					foreach (Tile tile in visibleRegionBlock.tiles) {
						tile.SetColour(newColour, Mathf.FloorToInt(time));
					}
				}
			}
			foreach (LifeManager.Life life in GameManager.lifeM.life) {
				life.SetColour(life.overTile.sr.color);
			}
			GameManager.cameraM.camera.backgroundColor = newColour * 0.5f;
		}

		private readonly Dictionary<int, Vector2> shadowDirectionAtHour = new Dictionary<int, Vector2>();
		public void DetermineShadowDirectionsAtHour(float equatorOffset) {
			for (int h = 0; h < 24; h++) {
				float hShadow = (2f * ((h - 12f) / 24f)) * (1f - Mathf.Pow(equatorOffset, 2f));
				float vShadow = Mathf.Pow(2f * ((h - 12f) / 24f), 2f) * equatorOffset + (equatorOffset / 2f);
				shadowDirectionAtHour.Add(h, new Vector2(hShadow, vShadow) * 5f);
			}
		}

		public float CalculateBrightnessLevelAtHour(float time) {
			return ((-(1f / 144f)) * Mathf.Pow(((1 + (24 - (1 - time))) % 24) - 12, 2) + 1.2f);
		}

		public Color GetTileColourAtHour(float time) {
			float r = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.4f * time + 7.2f), 10)) / 5f, 0f, 1f);
			float g = Mathf.Clamp((Mathf.Pow(CalculateBrightnessLevelAtHour(0.5f * time + 6), 10)) / 5f - 0.2f, 0f, 1f);
			float b = Mathf.Clamp((-1.5f * Mathf.Pow(Mathf.Cos(CalculateBrightnessLevelAtHour(2 * time + 12) / 1.5f), 3) + 1.65f * (CalculateBrightnessLevelAtHour(time) / 2f)) + 0.7f, 0f, 1f);
			return new Color(r, g, b, 1f);
		}

		public bool TileCanShadowTiles(Tile tile) {
			return tile.surroundingTiles.Any(nTile => nTile != null && !nTile.blocksLight) && (tile.blocksLight || tile.HasRoof());
		}

		public bool TileCanBeShadowed(Tile tile) {
			return !tile.blocksLight || (!tile.blocksLight && tile.HasRoof());
		}

		public void RecalculateLighting(List<Tile> tilesToRecalculate, bool setBrightnessAtEnd, bool forceBrightnessUpdate = false) {
			List<Tile> shadowSourceTiles = DetermineShadowSourceTiles(tilesToRecalculate);
			DetermineShadowTiles(shadowSourceTiles, setBrightnessAtEnd, forceBrightnessUpdate);
		}

		public List<Tile> DetermineShadowSourceTiles(List<Tile> tilesToRecalculate) {
			List<Tile> shadowSourceTiles = new List<Tile>();
			foreach (Tile tile in tilesToRecalculate) {
				if (tile != null && TileCanShadowTiles(tile)) {
					shadowSourceTiles.Add(tile);
				}
			}
			return shadowSourceTiles;
		}

		private static readonly float distanceIncreaseAmount = 0.1f; // 0.1f
		private void DetermineShadowTiles(List<Tile> shadowSourceTiles, bool setBrightnessAtEnd, bool forceBrightnessUpdate) {
			for (int h = 0; h < 24; h++) {
				Vector2 hourDirection = shadowDirectionAtHour[h];
				float maxShadowDistanceAtHour = hourDirection.magnitude * 5f + (Mathf.Pow(h - 12, 2) / 6f);
				float shadowedBrightnessAtHour = Mathf.Clamp(1 - (0.6f * CalculateBrightnessLevelAtHour(h)) + 0.3f, 0, 1);

				foreach (Tile shadowSourceTile in shadowSourceTiles) {
					Vector2 shadowSourceTilePosition = shadowSourceTile.obj.transform.position;
					bool shadowedAnyTile = false;

					List<Tile> shadowTiles = new List<Tile>();
					for (float distance = 0; distance <= maxShadowDistanceAtHour; distance += distanceIncreaseAmount) {
						Vector2 nextTilePosition = shadowSourceTilePosition + (hourDirection * distance);
						if (nextTilePosition.x < 0 || nextTilePosition.x >= mapData.mapSize || nextTilePosition.y < 0 || nextTilePosition.y >= mapData.mapSize) {
							break;
						}
						Tile tileToShadow = GetTileFromPosition(nextTilePosition);
						if (shadowTiles.Contains(tileToShadow)) {
							distance += distanceIncreaseAmount;
							continue;
						}
						if (tileToShadow != shadowSourceTile) {
							float newBrightness = 1;
							if (TileCanBeShadowed(tileToShadow)) {
								shadowedAnyTile = true;
								newBrightness = shadowedBrightnessAtHour;
								if (tileToShadow.brightnessAtHour.ContainsKey(h)) {
									tileToShadow.brightnessAtHour[h] = Mathf.Min(tileToShadow.brightnessAtHour[h], newBrightness);
								} else {
									tileToShadow.brightnessAtHour.Add(h, newBrightness);
								}
								shadowTiles.Add(tileToShadow);
							} else {
								if (shadowedAnyTile || Vector2.Distance(tileToShadow.position, shadowSourceTile.position) > maxShadowDistanceAtHour) {
									if (tileToShadow.blockingShadowsFrom.ContainsKey(h)) {
										tileToShadow.blockingShadowsFrom[h].Add(shadowSourceTile);
									} else {
										tileToShadow.blockingShadowsFrom.Add(h, new List<Tile>() { shadowSourceTile });
									}
									tileToShadow.blockingShadowsFrom[h] = tileToShadow.blockingShadowsFrom[h].Distinct().ToList();
									break;
								}
							}
							if (tileToShadow.shadowsFrom.ContainsKey(h)) {
								if (tileToShadow.shadowsFrom[h].ContainsKey(shadowSourceTile)) {
									tileToShadow.shadowsFrom[h][shadowSourceTile] = newBrightness;
								} else {
									tileToShadow.shadowsFrom[h].Add(shadowSourceTile, newBrightness);
								}
							} else {
								tileToShadow.shadowsFrom.Add(h, new Dictionary<Tile, float>() { { shadowSourceTile, newBrightness } });
							}
						}
					}
					if (shadowSourceTile.shadowsTo.ContainsKey(h)) {
						shadowSourceTile.shadowsTo[h].AddRange(shadowTiles);
					} else {
						shadowSourceTile.shadowsTo.Add(h, shadowTiles);
					}
					shadowSourceTile.shadowsTo[h] = shadowSourceTile.shadowsTo[h].Distinct().ToList();
				}
			}
			if (setBrightnessAtEnd) {
				SetTileBrightness(GameManager.timeM.tileBrightnessTime, forceBrightnessUpdate);
			}
		}

		public void RemoveTileBrightnessEffect(Tile tile) {
			List<Tile> tilesToRecalculateShadowsFor = new List<Tile>();
			for (int h = 0; h < 24; h++) {
				if (tile.shadowsTo.ContainsKey(h)) {
					foreach (Tile nTile in tile.shadowsTo[h]) {
						float darkestBrightnessAtHour = 1f;
						if (nTile.shadowsFrom.ContainsKey(h)) {
							nTile.shadowsFrom[h].Remove(tile);
							if (nTile.shadowsFrom[h].Count > 0) {
								darkestBrightnessAtHour = nTile.shadowsFrom[h].Min(shadowFromTile => shadowFromTile.Value);
							}
						}
						if (nTile.brightnessAtHour.ContainsKey(h)) {
							nTile.brightnessAtHour[h] = darkestBrightnessAtHour;
						}
						nTile.SetBrightness(darkestBrightnessAtHour, 12);
					}
				}
				if (tile.shadowsFrom.ContainsKey(h)) {
					tilesToRecalculateShadowsFor.AddRange(tile.shadowsFrom[h].Keys);
				}
				if (tile.blockingShadowsFrom.ContainsKey(h)) {
					tilesToRecalculateShadowsFor.AddRange(tile.blockingShadowsFrom[h]);
				}
			}
			tilesToRecalculateShadowsFor.AddRange(tile.surroundingTiles.Where(nTile => nTile != null));

			tile.shadowsFrom.Clear();
			tile.shadowsTo.Clear();
			tile.blockingShadowsFrom.Clear();

			RecalculateLighting(tilesToRecalculateShadowsFor.Distinct().ToList(), true);
		}

		public Tile GetTileFromPosition(Vector2 position) {
			position = new Vector2(Mathf.Clamp(position.x, 0, mapData.mapSize - 1), Mathf.Clamp(position.y, 0, mapData.mapSize - 1));
			return sortedTiles[Mathf.FloorToInt(position.y)][Mathf.FloorToInt(position.x)];
		}

		public static int GetRandomMapSeed() {
			return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
	}
}
