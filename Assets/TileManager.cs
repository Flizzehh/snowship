﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager:MonoBehaviour {

	public enum PlantGroups { Cactus, ColourfulShrubs, ColourfulTrees, DeadTrees, Shrubs, SnowTrees, ThinTrees, WideTrees };

	public List<PlantGroup> plantGroups = new List<PlantGroup>();

	public class PlantGroup {
		public PlantGroups type;
		public string name;

		public List<Sprite> smallPlants = new List<Sprite>();
		public List<Sprite> fullPlants = new List<Sprite>();

		public PlantGroup(PlantGroups type) {
			this.type = type;
			name = type.ToString();

			smallPlants = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + name + "/" + name + "-small").ToList();
			fullPlants = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + name + "/" + name + "-full").ToList();
		}
	}

	public void CreatePlantGroups() {
		foreach (PlantGroups plantGroup in System.Enum.GetValues(typeof(PlantGroups))) {
			plantGroups.Add(new PlantGroup(plantGroup));
		}
	}

	public PlantGroup GetPlantGroupByEnum(PlantGroups plantGroup) {
		return plantGroups.Find(group => group.type == plantGroup);
	}

	public enum TileTypes { GrassWater, Ice, Dirt, DirtWater, Mud, DirtGrass, DirtThinGrass, DirtDryGrass, Grass, ThickGrass, ColdGrass, ColdGrassWater, DryGrass, DryGrassWater, Sand, SandWater,
		Snow, SnowIce, SnowStone, Stone, StoneIce, StoneWater, StoneThinGrass, StoneSand, StoneSnow, Granite, Limestone, Marble, Sandstone, Slate, Clay
	};
	List<TileTypes> WaterEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.SnowIce, TileTypes.StoneIce, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater
	};
	List<TileTypes> LiquidWaterEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater
	};
	List<TileTypes> StoneEquivalentTileTypes = new List<TileTypes>() {
		TileTypes.Stone, TileTypes.Granite, TileTypes.Limestone, TileTypes.Marble, TileTypes.Sandstone, TileTypes.Slate, TileTypes.Clay
	};
	List<TileTypes> PlantableTileTypes = new List<TileTypes>() {
		TileTypes.Dirt, TileTypes.Mud, TileTypes.DirtGrass, TileTypes.DirtThinGrass, TileTypes.DirtDryGrass, TileTypes.Grass, TileTypes.ThickGrass,
		TileTypes.DryGrass, TileTypes.ColdGrass, TileTypes.Sand, TileTypes.Snow, TileTypes.SnowStone, TileTypes.StoneThinGrass, TileTypes.StoneSand, TileTypes.StoneSnow
	};
	List<TileTypes> BitmaskingTileTypes = new List<TileTypes>() {
		TileTypes.GrassWater, TileTypes.SnowIce, TileTypes.StoneIce, TileTypes.DirtWater, TileTypes.SandWater, TileTypes.DryGrassWater, TileTypes.ColdGrassWater, TileTypes.StoneWater, TileTypes.Stone
	};

	public List<TileTypes> GetWaterEquivalentTileTypes() {
		return WaterEquivalentTileTypes;
	}
	public List<TileTypes> GetLiquidWaterEquivalentTileTypes() {
		return LiquidWaterEquivalentTileTypes;
	}
	public List<TileTypes> GetStoneEquivalentTileTypes() {
		return StoneEquivalentTileTypes;
	}
	public List<TileTypes> GetPlantableTileTypes() {
		return PlantableTileTypes;
	}
	public List<TileTypes> GetBitmaskingTileTypes() {
		return BitmaskingTileTypes;
	}

	public List<TileType> tileTypes = new List<TileType>();

	public class TileType {
		public TileTypes type;
		public string name;

		public float walkSpeed;

		public bool walkable;
		public bool buildable;

		public List<Sprite> baseSprites = new List<Sprite>();
		public List<Sprite> bitmaskSprites = new List<Sprite>();
		public List<Sprite> riverSprites = new List<Sprite>();

		public TileType(List<string> tileTypeData, TileManager tm) {
			type = (TileTypes)System.Enum.Parse(typeof(TileTypes),tileTypeData[0]);
			name = type.ToString();

			walkSpeed = float.Parse(tileTypeData[1]);

			walkable = bool.Parse(tileTypeData[2]);
			buildable = bool.Parse(tileTypeData[3]);

			baseSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + name + "/" + name + "-base").ToList();
			bitmaskSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + name + "/" + name + "-bitmask").ToList();

			if (tm.LiquidWaterEquivalentTileTypes.Contains(type)) {
				riverSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Tiles/" + name + "/" + name + "-river").ToList();
			}
		}
	}

	public void CreateTileTypes() {
		List<string> stringTileTypes = Resources.Load<TextAsset>(@"Data/tiletypes").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringTileType in stringTileTypes) {
			List<string> stringTileTypeData = stringTileType.Split('/').ToList();
			tileTypes.Add(new TileType(stringTileTypeData,this));
		}
		foreach (TileType tileType in tileTypes) {
			tileType.name = GetComponent<UIManager>().SplitByCapitals(tileType.name);
		}
	}

	public TileType GetTileTypeByEnum(TileTypes find) {
		return tileTypes.Find(tileType => tileType.type == find);
	}

	public enum BiomeTypes {
		None,
		PolarDesert, IceCap, Tundra, WetTundra, PolarWetlands, CoolDesert, Steppe, BorealForest, TemperateWoodlands, TemperateForest,
		TemperateWetForest, TemperateWetlands, ExtremeDesert, Desert, SubtropicalScrub, TropicalScrub, SubtropicalWoodlands, TropicalWoodlands,
		Mediterranean, SubtropicalDryForest, TropicalDryForest, SubtropicalForest, SubtropicalWetForest, SubtropicalWetlands, TropicalWetForest, TropicalWetlands
	};

	public List<Biome> biomes = new List<Biome>();

	public class Biome {
		public BiomeTypes type;
		public string name;

		public List<int> temperatureRange = new List<int>();
		public List<float> precipitationRange = new List<float>();

		public Dictionary<PlantGroups,float> vegetationChances = new Dictionary<PlantGroups,float>();

		public Color colour;

		public TileType tileType;
		public TileType waterType;

		public Biome(List<string> biomeData, TileManager tm) {
			type = (BiomeTypes)System.Enum.Parse(typeof(BiomeTypes),biomeData[0]);
			name = type.ToString();

			List <string> stringTemperatureRange = biomeData[1].Split(',').ToList();

			if (int.Parse(stringTemperatureRange[0]) == -1000) {
				temperatureRange.Add(Mathf.RoundToInt(-1000));
			} else {
				temperatureRange.Add(int.Parse(stringTemperatureRange[0]));
			}
			if (int.Parse(stringTemperatureRange[1]) == 1000) {
				temperatureRange.Add(Mathf.RoundToInt(1000));
			} else {
				temperatureRange.Add(int.Parse(stringTemperatureRange[1]));
			}

			List<string> stringPrecipitationRange = biomeData[2].Split(',').ToList();

			if (float.Parse(stringPrecipitationRange[0]) == -1) {
				precipitationRange.Add(-1);
			} else {
				precipitationRange.Add(float.Parse(stringPrecipitationRange[0]));
			}
			if (float.Parse(stringPrecipitationRange[1]) == 2) {
				precipitationRange.Add(2);
			} else {
				precipitationRange.Add(float.Parse(stringPrecipitationRange[1]));
			}

			List<string> tileTypeData = biomeData[3].Split(',').ToList();
			tileType = tm.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes),tileTypeData[0]));
			waterType = tm.GetTileTypeByEnum((TileTypes)System.Enum.Parse(typeof(TileTypes),tileTypeData[1]));

			if (float.Parse(biomeData[4].Split(',')[0]) != 0) {
				int vegetationIndex = 0;
				foreach (string vegetationChance in biomeData[4].Split(',').ToList()) {
					vegetationChances.Add((PlantGroups)System.Enum.Parse(typeof(PlantGroups),biomeData[5].Split(',').ToList()[vegetationIndex]),float.Parse(vegetationChance));
					vegetationIndex += 1;
				}
			}

			int r = System.Int32.Parse("" + biomeData[6][2] + biomeData[6][3],System.Globalization.NumberStyles.HexNumber);
			int g = System.Int32.Parse("" + biomeData[6][4] + biomeData[6][5],System.Globalization.NumberStyles.HexNumber);
			int b = System.Int32.Parse("" + biomeData[6][6] + biomeData[6][7],System.Globalization.NumberStyles.HexNumber);
			colour = new Color(r,g,b,255f) / 255f;

		}
	}

	public void CreateBiomes() {
		List<string> stringBiomeTypes = Resources.Load<TextAsset>(@"Data/biomes").text.Replace("\n",string.Empty).Replace("\t",string.Empty).Split('`').ToList();
		foreach (string stringBiomeType in stringBiomeTypes) {
			List<string> stringBiomeData = stringBiomeType.Split('/').ToList();
			biomes.Add(new Biome(stringBiomeData,this));
		}
		foreach (Biome biome in biomes) {
			biome.name = GetComponent<UIManager>().SplitByCapitals(biome.name);
		}
	}

	public enum TileResourceTypes { Copper, Silver, Gold, Iron, Steel, Diamond };

	public List<Tile> tiles = new List<Tile>();
	public List<List<Tile>> sortedTiles = new List<List<Tile>>();
	public List<Tile> edgeTiles = new List<Tile>();

	public class Tile {

		private TileManager tm;

		public GameObject obj;
		public Vector2 position;

		public List<Tile> horizontalSurroundingTiles = new List<Tile>();
		public List<Tile> diagonalSurroundingTiles = new List<Tile>();
		public List<Tile> surroundingTiles = new List<Tile>();

		public float height;

		public TileType tileType;
		public TileResourceTypes tileResource;

		public Region region;
		public Region drainageBasin;

		public Biome biome;
		public GameObject plant;

		public float precipitation;
		public float temperature;

		public bool walkable;
		public float walkSpeed;

		public ResourceManager.TileObjectInstance objectInstance;
		public ResourceManager.TileObjectInstance floorInstance;

		public Tile(Vector2 position,float height,TileManager tm) {

			this.tm = tm;

			this.position = position;
			obj = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),new Vector2(position.x + 0.5f,position.y + 0.5f),Quaternion.identity);
			obj.transform.SetParent(GameObject.Find("TileParent").transform,true);

			SetTileHeight(height);
		}

		public void SetTileHeight(float height) {
			this.height = height;
			SetTileTypeBasedOnHeight();
		}

		public void SetTileType(TileType tileType, bool bitmask) {
			this.tileType = tileType;
			walkable = tileType.walkable;
			if (bitmask) {
				tm.Bitmasking(new List<Tile>() { this }.Concat(surroundingTiles).ToList());
			}
			if (plant != null && !tm.PlantableTileTypes.Contains(tileType.type)) {
				Destroy(plant);
				plant = null;
			}
			SetWalkSpeed();
		}

		public void SetTileTypeBasedOnHeight() {
			if (height < 0.40f) {
				SetTileType(tm.GetTileTypeByEnum(TileTypes.GrassWater),false);
			} else if (height > 0.75f) {
				SetTileType(tm.GetTileTypeByEnum(TileTypes.Stone),false);
			} else {
				SetTileType(tm.GetTileTypeByEnum(TileTypes.Grass),false);
			}
		}

		public void ChangeRegion(Region newRegion, bool changeTileTypeToRegionType, bool bitmask) {
			region = newRegion;
			region.tiles.Add(this);
			if (!tm.regions.Contains(region)) {
				tm.regions.Add(region);
			}
			if (changeTileTypeToRegionType) {
				SetTileType(region.tileType,bitmask);
			}
		}

		public void SetBiome(Biome biome) {
			this.biome = biome;
			if (!tm.StoneEquivalentTileTypes.Contains(tileType.type)) {
				if (!tm.WaterEquivalentTileTypes.Contains(tileType.type)) {
					SetTileType(biome.tileType,false);
				} else {
					SetTileType(biome.waterType,false);
				}
			}
			if (tm.PlantableTileTypes.Contains(tileType.type)) {
				SetPlant();
			}
		}

		public void SetPlant() {
			if (plant != null) {
				Destroy(plant);
			}
			foreach (KeyValuePair<PlantGroups,float> kvp in biome.vegetationChances) {
				PlantGroups plantGroup = kvp.Key;
				if (Random.Range(0f,1f) < biome.vegetationChances[plantGroup]) {
					GameObject plant = Instantiate(Resources.Load<GameObject>(@"Prefabs/Tile"),obj.transform.position,Quaternion.identity);
					if (Random.Range(0f,1f) < 0.1f) {
						plant.GetComponent<SpriteRenderer>().sprite = tm.GetPlantGroupByEnum(plantGroup).smallPlants[Random.Range(0,tm.GetPlantGroupByEnum(plantGroup).smallPlants.Count)];
					} else {
						plant.GetComponent<SpriteRenderer>().sprite = tm.GetPlantGroupByEnum(plantGroup).fullPlants[Random.Range(0,tm.GetPlantGroupByEnum(plantGroup).fullPlants.Count)];
					}
					plant.GetComponent<SpriteRenderer>().sortingOrder = 1;
					plant.name = "PLANT " + plant.GetComponent<SpriteRenderer>().sprite.name;
					plant.transform.parent = obj.transform;
					this.plant = plant;
					break;
				}
			}
			SetWalkSpeed();
			
		}

		public void SetTileObject(ResourceManager.TileObjectPrefab tileObjectPrefab) {
			objectInstance = new ResourceManager.TileObjectInstance(tileObjectPrefab,this);
			SetWalkSpeed();
			
		}

		public void SetWalkSpeed() {
			walkSpeed = tileType.walkSpeed;
			if (plant != null && walkSpeed > 0.5f) {
				walkSpeed = 0.5f;
			}
			if (objectInstance != null && walkSpeed > objectInstance.prefab.walkSpeed) {
				walkSpeed = objectInstance.prefab.walkSpeed;
			}
		}
	}

	private bool generated;

	public void Initialize(int mapSize,int mapSeed) {
		SetMapInformation(mapSize,mapSeed);

		GetComponent<CameraManager>().SetCameraPosition(new Vector2(mapSize / 2f,mapSize / 2f));
		GetComponent<CameraManager>().SetCameraZoom((mapSize / 2f) + 2);

		GetComponent<ResourceManager>().CreateResources();
		GetComponent<ResourceManager>().CreateTileObjectPrefabs();

		CreatePlantGroups();
		CreateTileTypes();
		CreateBiomes();
		CreateMap();

		generated = true;

		GetComponent<ColonistManager>().SpawnColonists(3);
	}

	ColonistManager.Colonist selectedColonist;

	private bool debugMode;

	void Update() {
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote)) {
			debugMode = !debugMode;
		}
		if (debugMode) {
			if (generated) {
				if (Input.GetKeyDown(KeyCode.Z)) {
					foreach (Tile tile in tiles) {
						tile.obj.GetComponent<SpriteRenderer>().color = Color.white;
					}
					Bitmasking(tiles);
				}
				if (Input.GetKeyDown(KeyCode.X)) {
					foreach (Region region in regions) {
						region.ColourRegion();
					}
				}
				if (Input.GetKeyDown(KeyCode.C)) {
					Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
					foreach (Tile tile in tiles) {
						tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
						tile.obj.GetComponent<SpriteRenderer>().color = new Color(tile.height,tile.height,tile.height,1f);
					}
				}
				if (Input.GetKeyDown(KeyCode.V)) {
					Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
					foreach (Tile tile in tiles) {
						tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
						tile.obj.GetComponent<SpriteRenderer>().color = new Color(tile.precipitation,tile.precipitation,tile.precipitation,1f);
					}
				}
				if (Input.GetKeyDown(KeyCode.B)) {
					Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
					foreach (Tile tile in tiles) {
						tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
						tile.obj.GetComponent<SpriteRenderer>().color = new Color((tile.temperature + 50f) / 100f,(tile.temperature + 50f) / 100f,(tile.temperature + 50f) / 100f,1f);
					}
				}
				if (Input.GetKeyDown(KeyCode.N)) {
					Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
					foreach (Tile tile in tiles) {
						tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
						if (tile.biome != null) {
							tile.obj.GetComponent<SpriteRenderer>().color = tile.biome.colour;
						} else {
							tile.obj.GetComponent<SpriteRenderer>().color = Color.black;
						}
					}
				}
				if (Input.GetKeyDown(KeyCode.M)) {
					Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
					foreach (KeyValuePair<Region,Tile> kvp in drainageBasins) {
						foreach (Tile tile in kvp.Key.tiles) {
							tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
							tile.obj.GetComponent<SpriteRenderer>().color = kvp.Key.colour;
						}
					}
				}
				if (Input.GetKeyDown(KeyCode.Comma)) {
					Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
					foreach (List<Tile> river in rivers) {
						foreach (Tile tile in river) {
							tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
							tile.obj.GetComponent<SpriteRenderer>().color = Color.blue;
						}
						river[0].obj.GetComponent<SpriteRenderer>().color = Color.red;
						river[river.Count - 1].obj.GetComponent<SpriteRenderer>().color = Color.green;
					}
				}

				Vector2 mousePosition = GetComponent<CameraManager>().cameraComponent.ScreenToWorldPoint(Input.mousePosition);
				if (Input.GetMouseButtonDown(0)) {
					bool foundColonist = false;
					foreach (ColonistManager.Colonist colonist in GetComponent<ColonistManager>().colonists) {
						if (colonist.overTile == GetTileFromPosition(mousePosition)) {
							selectedColonist = colonist;
							foundColonist = true;
							break;
						}
					}
					if (!foundColonist && selectedColonist != null) {
						selectedColonist.MoveToTile(GetTileFromPosition(mousePosition));
					}
					/*
					Tile tile = sortedTiles[Mathf.FloorToInt(mousePosition.y)][Mathf.FloorToInt(mousePosition.x)];
					print(tile.biome.name);
					//tile.SetTileType(GetTileTypeByEnum(TileTypes.GrassWater),true);
					*/
				}
				if (Input.GetMouseButtonDown(1)) {
					selectedColonist = null;
					/*
					Tile tile = sortedTiles[Mathf.FloorToInt(mousePosition.y)][Mathf.FloorToInt(mousePosition.x)];
					//tile.SetTileType(GetTileTypeByEnum(TileTypes.Grass),true);
					print(tile.tileType.name);
					*/
				}
			}
		}
	}

	public int mapSize;

	public void SetMapInformation(int mapSize, int mapSeed) {
		this.mapSize = mapSize;

		if (mapSeed < 0) {
			mapSeed = Random.Range(0,int.MaxValue);
		}
		UnityEngine.Random.InitState(mapSeed);
		print(mapSeed);
	}

	void CreateMap() {
		CreateTiles();

		SetTileRegions(true);

		ReduceNoise(Mathf.RoundToInt(mapSize / 5f),new List<TileTypes>() { TileTypes.GrassWater,TileTypes.Stone,TileTypes.Grass });
		ReduceNoise(Mathf.RoundToInt(mapSize / 2f),new List<TileTypes>() { TileTypes.GrassWater });

		CalculatePrecipitation();
		CalculateTemperature();

		SetTileRegions(false);
		SetBiomes();

		SetMapEdgeTiles();
		DetermineDrainageBasins();
		CreateRivers();

		Bitmasking(tiles);
	}

	void CreateTiles() {
		for (int y = 0;y < mapSize;y++) {
			List<Tile> innerTiles = new List<Tile>();
			for (int x = 0;x < mapSize;x++) {

				float height = Random.Range(0f,1f);

				Vector2 position = new Vector2(x,y);

				Tile tile = new Tile(position,height,this);

				innerTiles.Add(tile);
				tiles.Add(tile);
			}
			sortedTiles.Add(innerTiles);
		}

		SetSurroundingTiles();
		GenerateTerrain();
		AverageTileHeights();
	}

	void SetSurroundingTiles() {
		for (int y = 0;y < mapSize;y++) {
			for (int x = 0;x < mapSize;x++) {
				/* Horizontal */
				if (y + 1 < mapSize) {
					sortedTiles[y][x].horizontalSurroundingTiles.Add(sortedTiles[y + 1][x]);
				} else {
					sortedTiles[y][x].horizontalSurroundingTiles.Add(null);
				}
				if (x + 1 < mapSize) {
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
				if (x + 1 < mapSize && y + 1 < mapSize) {
					sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y + 1][x + 1]);
				} else {
					sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
				}
				if (y - 1 >= 0 && x + 1 < mapSize) {
					sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x + 1]);
				} else {
					sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
				}
				if (x - 1 >= 0 && y - 1 >= 0) {
					sortedTiles[y][x].diagonalSurroundingTiles.Add(sortedTiles[y - 1][x - 1]);
				} else {
					sortedTiles[y][x].diagonalSurroundingTiles.Add(null);
				}
				if (y + 1 < mapSize && x - 1 >= 0) {
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
		int lastSize = mapSize;
		for (int halves = 0;halves < Mathf.CeilToInt(Mathf.Log(mapSize,2));halves++) {
			int size = Mathf.CeilToInt(lastSize / 2f);
			for (int sectionY = 0;sectionY < mapSize;sectionY += size) {

				for (int sectionX = 0;sectionX < mapSize;sectionX += size) {
					float sectionAverage = 0;
					for (int y = sectionY;(y < sectionY + size && y < mapSize);y++) {
						for (int x = sectionX;(x < sectionX + size && x < mapSize);x++) {
							sectionAverage += sortedTiles[y][x].height;
						}
					}
					sectionAverage /= (size * size);
					sectionAverage += Random.Range(-0.25f,0.25f);
					for (int y = sectionY;(y < sectionY + size && y < mapSize);y++) {
						for (int x = sectionX;(x < sectionX + size && x < mapSize);x++) {
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
		for (int i = 0;i < 10;i++) { // 3
			List<float> averageTileHeights = new List<float>();

			foreach (Tile tile in tiles) {
				float averageHeight = tile.height;
				float numValidTiles = 1;
				for (int t = 0;t < tile.surroundingTiles.Count;t++) {
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

			for (int k = 0;k < tiles.Count;k++) {
				tiles[k].height = averageTileHeights[k];
				tiles[k].SetTileTypeBasedOnHeight();
			}
		}
	}

	List<Region> regions = new List<Region>();
	int currentRegionID = 0;

	public class Region {
		public TileType tileType;
		public List<Tile> tiles = new List<Tile>();
		public int id;

		public List<Region> connectedRegions = new List<Region>();

		public Color colour;

		public Region(TileType regionTileType,int regionID) {
			this.tileType = regionTileType;
			this.id = regionID;

			colour = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),1f);
		}

		public void ColourRegion() {
			Sprite whiteSquare = Resources.Load<Sprite>(@"UI/white-square");
			foreach (Tile tile in this.tiles) {
				tile.obj.GetComponent<SpriteRenderer>().sprite = whiteSquare;
				tile.obj.GetComponent<SpriteRenderer>().color = colour;
			}
		}
	}

	void SetTileRegions(bool splitByTileType) {
		regions.Clear();

		EstablishInitialRegions(splitByTileType);
		FindConnectedRegions(splitByTileType);
		MergeConnectedRegions(splitByTileType);

		RemoveEmptyRegions();
	}

	void EstablishInitialRegions(bool splitByTileType) {
		foreach (Tile tile in tiles) { // Go through all tiles
			List<Region> foundRegions = new List<Region>(); // For each tile, store a list of the regions around them
			for (int i = 0;i < tile.surroundingTiles.Count;i++) { // Go through the tiles around each tile
				Tile nTile = tile.surroundingTiles[i];
				if (nTile != null && (splitByTileType ? tile.tileType == nTile.tileType : (tile.walkable == nTile.walkable)) && (i == 2 || i == 3 /*|| i == 5 || i == 6 */)) { // Uncomment indexes 5 and 6 to enable 8-connectivity connected-component labeling -- If the tiles have the same type
					if (nTile.region != null && !foundRegions.Contains(nTile.region)) { // If the tiles have a region and it hasn't already been looked at
						foundRegions.Add(nTile.region); // Add the surrounding tile's region to the regions found around the original tile
					}
				}
			}
			if (foundRegions.Count <= 0) { // If there weren't any tiles with the same region/tiletype found around them, make a new region for this tile
				tile.ChangeRegion(new Region(tile.tileType,currentRegionID),false,false);
				currentRegionID += 1;
			} else if (foundRegions.Count == 1) { // If there was a single region found around them, give them that region
				tile.ChangeRegion(foundRegions[0],false,false);
			} else if (foundRegions.Count > 1) { // If there was more than one around found around them, give them the region with the lowest ID
				tile.ChangeRegion(FindLowestRegion(foundRegions),false,false);
			}
		}
	}

	void FindConnectedRegions(bool splitByTileType) {
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

	void MergeConnectedRegions(bool splitByTileType) {
		while (regions.Where(region => region.connectedRegions.Count > 0).ToList().Count > 0) { // While there are regions that have connected regions
			foreach (Region region in regions) { // Go through each region
				if (region.connectedRegions.Count > 0) { // If this region has connected regions
					Region lowestRegion = FindLowestRegion(region.connectedRegions); // Find the lowest ID region from the connected regions
					if (region != lowestRegion) { // If this region is not the lowest region
						foreach (Tile tile in region.tiles) { // Set each tile's region in this region to the lowest region
							tile.ChangeRegion(lowestRegion,false,false);
						}
						region.tiles.Clear(); // Clear the tiles from this region
					}
					foreach (Region connectedRegion in region.connectedRegions) { // Set each tile's region in the connected regions that aren't the lowest region to the lowest region
						if (connectedRegion != lowestRegion) {
							foreach (Tile tile in connectedRegion.tiles) {
								tile.ChangeRegion(lowestRegion,false,false);
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

	Region FindLowestRegion(List<Region> searchRegions) {
		Region lowestRegion = searchRegions[0];
		foreach (Region region in searchRegions) {
			if (region.id < lowestRegion.id) {
				lowestRegion = region;
			}
		}
		return lowestRegion;
	}

	void RemoveEmptyRegions() {
		for (int i = 0;i < regions.Count;i++) {
			if (regions[i].tiles.Count <= 0) {
				regions.RemoveAt(i);
				i = (i - 1 < 0 ? 0 : i - 1);
			}
		}

		for (int i = 0;i < regions.Count;i++) {
			regions[i].id = i;
		}
	}

	void ReduceNoise(int removeRegionsBelowSize, List<TileTypes> typesToRemove) {
		foreach (Region region in regions) {
			if (typesToRemove.Contains(region.tileType.type)) {
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
							tile.ChangeRegion(lowestRegion,true,false);
						}
						region.tiles.Clear(); // Clear the tiles from this region
					}
				}
			}
		}
		RemoveEmptyRegions();
	}

	private int windDirection = 0;
	void CalculatePrecipitation() {
		//int windDirection = 0;/*Random.Range(0,3);*/ // 0 - up, 1 - right, 2 - down, 3 - left, 4 - up/right, 5 - down/right, 6 - down-left, 7 - up/left
		Dictionary<int,int> windDirectionOppositeMap = new Dictionary<int,int>() { { 0,2 },{ 1,3 },{ 2,0 },{ 3,1 },{ 4,6 },{ 5,7 },{ 6,4 },{ 7,5 } };

		List<List<float>> precipitations = new List<List<float>>();

		for (int i = 0;i < 5;i++) {
			windDirection = i;
			if (windDirection <= 3) { // Wind is going horizontally/vertically
				bool yStartAtTop = (windDirection == 2);
				bool xStartAtRight = (windDirection == 3);

				for (int y = (yStartAtTop ? mapSize - 1 : 0);(yStartAtTop ? y >= 0 : y < mapSize);y += (yStartAtTop ? -1 : 1)) {
					for (int x = (xStartAtRight ? mapSize - 1 : 0);(xStartAtRight ? x >= 0 : x < mapSize);x += (xStartAtRight ? -1 : 1)) {
						Tile tile = sortedTiles[y][x];
						Tile previousTile = tile.surroundingTiles[windDirectionOppositeMap[windDirection]];
						if (previousTile != null) {
							if (LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
								tile.precipitation = previousTile.precipitation + (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,(1 - previousTile.precipitation) / 5f) : 0f);
							} else {
								tile.precipitation = previousTile.precipitation - (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,previousTile.precipitation / 5f) * tile.height : 0f);//(tile.height * Random.Range(0.005f,0.03f));
							}
						} else {
							tile.precipitation = Random.Range(0.1f,0.5f) * (1 - tile.height);
						}
					}
				}
			} else { // Wind is going diagonally
				for (int k = 0; k < mapSize * 2; k++) {
					for (int x = 0; x <= k; x++) {
						int y = k - x;
						if (y < mapSize && x < mapSize) {
							Tile tile = sortedTiles[y][x];
							Tile previousTile = tile.surroundingTiles[windDirectionOppositeMap[windDirection]];
							if (previousTile != null) {
								if (LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type)) {
									tile.precipitation = previousTile.precipitation + (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,(1 - previousTile.precipitation) / 5f) : 0f);
								} else {
									tile.precipitation = previousTile.precipitation - (Random.Range(0f,1f) < 1 - previousTile.precipitation ? Random.Range(0f,previousTile.precipitation / 5f) * tile.height : 0f);
								}
							} else {
								tile.precipitation = Random.Range(0.1f,0.5f) * (1 - tile.height);
							}
						}
					}
				}
			}
			List<float> directionPrecipitations = new List<float>();
			foreach (Tile tile in tiles) {
				directionPrecipitations.Add(tile.precipitation);
				tile.precipitation = 0;
			}
			precipitations.Add(directionPrecipitations);
		}
		int primaryDirection = Random.Range(0,3);
		int oppositeDirection = windDirectionOppositeMap[primaryDirection];

		for (int t = 0; t < tiles.Count; t++) {
			tiles[t].precipitation = 0;
			for (int i = 0;i < 4;i++) {
				if (i == oppositeDirection) {
					tiles[t].precipitation += precipitations[i][t] * 0.1f; // 0.25f
				} else if (i != primaryDirection) {
					tiles[t].precipitation += precipitations[i][t] * 0.25f; // 0.5f
				} else {
					tiles[t].precipitation += precipitations[i][t];
				}
			}
			tiles[t].precipitation /= 1.35f;//1.3f; // 2.25f
		}
		AverageTilePrecipitations();

		foreach (Tile tile in tiles) {
			tile.precipitation = Mathf.Clamp(tile.precipitation,0f,1f);
		}
	}

	void AverageTilePrecipitations() {
		for (int i = 0;i < 3;i++) {
			List<float> averageTilePrecipitations = new List<float>();

			foreach (Tile tile in tiles) {
				float averagePrecipitation = tile.precipitation;
				int numValidTiles = 1;
				for (int t = 0;t < tile.surroundingTiles.Count;t++) {
					Tile nTile = tile.surroundingTiles[t];
					if (nTile != null) {
						numValidTiles += 1;
						averagePrecipitation += nTile.precipitation;
					}
				}
				averagePrecipitation /= numValidTiles;
				averageTilePrecipitations.Add(averagePrecipitation);
			}

			for (int k = 0;k < tiles.Count;k++) {
				tiles[k].precipitation = averageTilePrecipitations[k];
			}
		}
	}

	float TemperatureFunction(float yPos,float temperatureOffset) {
		return ((-2 * Mathf.Abs((yPos - (mapSize / 2f)) / ((mapSize / 100f) / (temperatureOffset / 50f)))) + temperatureOffset);
	}

	void CalculateTemperature() {

		float temperatureOffset = 50;

		foreach (Tile tile in tiles) {
			tile.temperature = TemperatureFunction(tile.position.y,temperatureOffset);//(Mathf.FloorToInt(tile.position.y) - (mapSize / 2f)) / ((mapSize / 100f) / (temperatureOffset / 50f));//TemperatureFunction(tile.position.y,temperatureOffset);
			tile.temperature += -(250f * Mathf.Pow(tile.height - 0.5f,3));
		}

		AverageTileTemperatures();

		foreach (Tile tile in tiles) {
			tile.temperature = Mathf.Clamp(tile.temperature,-temperatureOffset,temperatureOffset);
		}
	}

	void AverageTileTemperatures() {
		for (int i = 0;i < 3;i++) {
			List<float> averageTileTemperatures = new List<float>();

			foreach (Tile tile in tiles) {
				float averageTemperature = tile.temperature;
				int numValidTiles = 1;
				for (int t = 0;t < tile.surroundingTiles.Count;t++) {
					Tile nTile = tile.surroundingTiles[t];
					if (nTile != null) {
						numValidTiles += 1;
						averageTemperature += nTile.temperature;
					}
				}
				averageTemperature /= numValidTiles;
				averageTileTemperatures.Add(averageTemperature);
			}

			for (int k = 0;k < tiles.Count;k++) {
				tiles[k].temperature = averageTileTemperatures[k];
			}
		}
	}

	void SetBiomes() {
		foreach (Tile tile in tiles) {
			foreach (Biome biome in biomes) {
				if (biome.temperatureRange[0] <= tile.temperature && biome.temperatureRange[1] >= tile.temperature && biome.precipitationRange[0] <= tile.precipitation && biome.precipitationRange[1] >= tile.precipitation) {
					tile.SetBiome(biome);
					break;
				}
			}
		}
	}

	void SetMapEdgeTiles() {
		for (int i = 1;i < mapSize-1;i++) {
			edgeTiles.Add(sortedTiles[0][i]);
			edgeTiles.Add(sortedTiles[mapSize - 1][i]);
			edgeTiles.Add(sortedTiles[i][0]);
			edgeTiles.Add(sortedTiles[i][mapSize - 1]);
		}
		edgeTiles.Add(sortedTiles[0][0]);
		edgeTiles.Add(sortedTiles[0][mapSize-1]);
		edgeTiles.Add(sortedTiles[mapSize-1][0]);
		edgeTiles.Add(sortedTiles[mapSize-1][mapSize-1]);
	}

	public List<List<Tile>> rivers = new List<List<Tile>>();

	public Dictionary<Region,Tile> drainageBasins = new Dictionary<Region,Tile>();
	public int drainageBasinID = 0;

	void DetermineDrainageBasins() {
		List<Tile> tilesByHeight = tiles.OrderBy(o => o.height).ToList();
		foreach (Tile tile in tilesByHeight) {
			if (!StoneEquivalentTileTypes.Contains(tile.tileType.type) && tile.drainageBasin == null) {
				Region drainageBasin = new Region(null,drainageBasinID);
				drainageBasinID += 1;

				Tile currentTile = tile;

				List<Tile> checkedTiles = new List<Tile>();
				checkedTiles.Add(currentTile);
				List<Tile> frontier = new List<Tile>();
				frontier.Add(currentTile);

				while (frontier.Count > 0) {
					currentTile = frontier[0];
					frontier.RemoveAt(0);

					drainageBasin.tiles.Add(currentTile);
					currentTile.drainageBasin = drainageBasin;

					foreach (Tile nTile in currentTile.surroundingTiles) {
						if (nTile != null && !checkedTiles.Contains(nTile) && !StoneEquivalentTileTypes.Contains(nTile.tileType.type) && nTile.drainageBasin == null) {
							if (nTile.height >= currentTile.height) {
								frontier.Add(nTile);
								checkedTiles.Add(nTile);
							}
						}
					}
				}
				drainageBasins.Add(drainageBasin,tile);
			}
		}
	}

	void CreateRivers() {
		Dictionary<Tile,Tile> riverStartTiles = new Dictionary<Tile,Tile>();
		foreach (KeyValuePair<Region,Tile> kvp in drainageBasins) {
			Region drainageBasin = kvp.Key;
			if (drainageBasin.tiles.Find(o => WaterEquivalentTileTypes.Contains(o.tileType.type)) != null && drainageBasin.tiles.Find(o => o.horizontalSurroundingTiles.Find(o2 => o2 != null && StoneEquivalentTileTypes.Contains(o2.tileType.type)) != null) != null) {
				foreach (Tile tile in drainageBasin.tiles) {
					if (tile.walkable && !WaterEquivalentTileTypes.Contains(tile.tileType.type) && tile.horizontalSurroundingTiles.Find(o => o != null && StoneEquivalentTileTypes.Contains(o.tileType.type)) != null) {
						riverStartTiles.Add(tile,kvp.Value);
					}
				}
			}
		}
		for (int i = 0;i < mapSize / 10f && i < riverStartTiles.Count;i++) {
			Tile riverStartTile = Enumerable.ToList(riverStartTiles.Keys)[Random.Range(0,riverStartTiles.Count)];
			Tile riverEndTile = riverStartTiles[riverStartTile];
			List<Tile> removeTiles = new List<Tile>();
			foreach (KeyValuePair<Tile,Tile> kvp in riverStartTiles) {
				if (Vector2.Distance(kvp.Key.obj.transform.position,riverStartTile.obj.transform.position) < 5f) {
					removeTiles.Add(kvp.Key);
				}
			}
			foreach (Tile removeTile in removeTiles) {
				riverStartTiles.Remove(removeTile);
			}
			removeTiles.Clear();

			PathManager.PathfindingTile currentTile = new PathManager.PathfindingTile(riverStartTile,null,0);

			List<PathManager.PathfindingTile> checkedTiles = new List<PathManager.PathfindingTile>();
			checkedTiles.Add(currentTile);
			List<PathManager.PathfindingTile> frontier = new List<PathManager.PathfindingTile>();
			frontier.Add(currentTile);

			List<Tile> river = new List<Tile>();

			while (frontier.Count > 0) {
				currentTile = frontier[0];
				frontier.RemoveAt(0);

				if (WaterEquivalentTileTypes.Contains(currentTile.tile.tileType.type)) {
					Tile foundOtherRiverAtTile = null;
					List<Tile> foundOtherRiver = null;
					bool expandRiver = false;
					while (currentTile != null) {
						river.Add(currentTile.tile);
						currentTile.tile.SetTileType(currentTile.tile.biome.waterType,false);
						/*
						foreach (Tile nTile in currentTile.tile.surroundingTiles) {
							if (nTile != null) {
								foreach (List<Tile> otherRiver in rivers) {
									foreach (Tile otherRiverTile in otherRiver) {
										if (currentTile.tile.surroundingTiles.Contains(otherRiverTile) && !river.Contains(otherRiverTile)) {
											foundOtherRiverAtTile = currentTile.tile;
											expandRiver = true;
											break;
										}
									}
								}
							}
						}
						*/
						if (!expandRiver) {
							foreach (List<Tile> otherRiver in rivers) {
								foreach (Tile otherRiverTile in otherRiver) {
									if (otherRiverTile == currentTile.tile) {
										foundOtherRiver = otherRiver;
										foundOtherRiverAtTile = otherRiverTile;
										expandRiver = true;
										break;
									}
								}
								if (expandRiver) {
									break;
								}
							}
						}
						/*
						if ((rivers.Find(otherRiver => otherRiver.Find(riverTile => currentTile.tile == riverTile) != null) != null)) {
							print("Expand");
							foundOtherRiverAtTile = currentTile.tile;
							print("Found: " + foundOtherRiverAtTile.obj.transform.position);
							expandRiver = true;
						}
						*/
						currentTile = currentTile.cameFrom;
					}
					if (foundOtherRiver != null && foundOtherRiver.Count > 0) {
						int riverTileIndex = 0;
						while (expandRiver) {
							Tile riverTile = foundOtherRiver[riverTileIndex];
							print("Checking: " + riverTile.obj.transform.position);
							if (riverTile == foundOtherRiverAtTile) {
								print("Found other river");
								break;
							}
							int maxExpandRadius = 2;
							List<Tile> expandFrontier = new List<Tile>();
							expandFrontier.Add(riverTile);
							while (expandFrontier.Count > 0) {
								print("Changed tiles");
								Tile expandTile = expandFrontier[0];
								expandFrontier.RemoveAt(0);
								expandTile.SetTileType(expandTile.biome.waterType,false);
								foreach (Tile nTile in expandTile.surroundingTiles) {
									if (nTile != null && !StoneEquivalentTileTypes.Contains(nTile.tileType.type) && Vector2.Distance(nTile.obj.transform.position,riverTile.obj.transform.position) < maxExpandRadius) {
										expandFrontier.Add(nTile);
									}
								}
							}
							riverTileIndex += 1;
							break;
						}
					}
					break;
				}

				foreach (Tile nTile in currentTile.tile.horizontalSurroundingTiles) {
					if (nTile != null && checkedTiles.Find(o => o.tile == nTile) == null && !StoneEquivalentTileTypes.Contains(nTile.tileType.type)) {
						if (rivers.Find(otherRiver => otherRiver.Find(riverTile => nTile == riverTile) != null) != null) {
							frontier.Clear();
							frontier.Add(new PathManager.PathfindingTile(nTile,currentTile,0));
							nTile.SetTileType(nTile.biome.waterType,false);
							break;
						}
						float cost = Vector2.Distance(nTile.obj.transform.position,riverEndTile.obj.transform.position) + (nTile.height * (mapSize/10f)) + Random.Range(0,10);
						PathManager.PathfindingTile pTile = new PathManager.PathfindingTile(nTile,currentTile,cost);
						frontier.Add(pTile);
						checkedTiles.Add(pTile);
					}
				}
				frontier = frontier.OrderBy(o => o.cost).ToList();
			}
			rivers.Add(river);
		}
	}

	Dictionary<int,int> bitmaskMap = new Dictionary<int,int>() {
		{ 19,16 },{ 23,17 },{ 27,18 },{ 31,19 },{ 38,20 },{ 39,21 },{ 46,22 },
		{ 47,23 },{ 55,24 },{ 63,25 },{ 76,26 },{ 77,27 },{ 78,28 },{ 79,29 },
		{ 95,30 },{ 110,31 },{ 111,32 },{ 127,33 },{ 137,34 },{ 139,35 },{ 141,36 },
		{ 143,37 },{ 155,38 },{ 159,39 },{ 175,40 },{ 191,41 },{ 205,42 },{ 207,43 },
		{ 223,44 },{ 239,45 },{ 255,46 }
	};
	Dictionary<int,List<int>> diagonalCheckMap = new Dictionary<int,List<int>>() {
		{4,new List<int>() {0,1 } },
		{5,new List<int>() {1,2 } },
		{6,new List<int>() {2,3 } },
		{7,new List<int>() {3,0 } }
	};

	int BitSum(List<TileTypes> compareTileTypes,List<Tile> tilesToSum) {
		int sum = 0;
		for (int i = 0;i < tilesToSum.Count;i++) {
			if (tilesToSum[i] != null) {
				if (compareTileTypes.Contains(tilesToSum[i].tileType.type)) {
					bool ignoreTile = false;
					if (compareTileTypes.Contains(tilesToSum[i].tileType.type) && diagonalCheckMap.ContainsKey(i)) {
						List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]],tilesToSum[diagonalCheckMap[i][1]] };

						List<Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && compareTileTypes.Contains(tile.tileType.type)).ToList();
						List<Tile> oppositeTiles = surroundingHorizontalTiles.Where(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)).ToList();

						if (similarTiles.Count == 0 && oppositeTiles.Count == 2) {
							ignoreTile = true;
						} else if (similarTiles.Count == 2 && oppositeTiles.Count == 0) {
							ignoreTile = false;
						} else if (similarTiles.Count == 1 && oppositeTiles.Count == 1) {
							ignoreTile = true;
						} else if (similarTiles.Count == 0 && oppositeTiles.Count == 1) {
							print("Not yet");
						} else if (similarTiles.Count == 1 && oppositeTiles.Count == 0) {
							print("Not yet");
						} else {
							print("Not yet");
						}
					}
					if (!ignoreTile) {
						sum += Mathf.RoundToInt(Mathf.Pow(2,i));
					}
				}
			} else {
				if (tilesToSum.Find(tile => tile != null && tilesToSum.IndexOf(tile) <= 3 && !compareTileTypes.Contains(tile.tileType.type)) == null) {
					sum += Mathf.RoundToInt(Mathf.Pow(2,i));
				} else {
					if (i <= 3) {
						sum += Mathf.RoundToInt(Mathf.Pow(2,i));
					} else {
						List<Tile> surroundingHorizontalTiles = new List<Tile>() { tilesToSum[diagonalCheckMap[i][0]],tilesToSum[diagonalCheckMap[i][1]] };
						if (surroundingHorizontalTiles.Find(tile => tile != null && !compareTileTypes.Contains(tile.tileType.type)) == null) {
							sum += Mathf.RoundToInt(Mathf.Pow(2,i));
						}
					}
				}
			}
		}
		return sum;
	}

	void BitmaskTile(Tile tile,bool includeDiagonalSurroundingTiles,bool customBitSumInputs,List<TileTypes> customCompareTileTypes) {
		int sum = 0;
		if (customBitSumInputs) {
			sum = BitSum(customCompareTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles));
		} else {
			if (WaterEquivalentTileTypes.Contains(tile.tileType.type)) {
				sum = BitSum(WaterEquivalentTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles));
			} else if (StoneEquivalentTileTypes.Contains(tile.tileType.type)) {
				sum = BitSum(StoneEquivalentTileTypes,(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles));
			} else {
				sum = BitSum(new List<TileTypes>() { tile.tileType.type },(includeDiagonalSurroundingTiles ? tile.surroundingTiles : tile.horizontalSurroundingTiles));
			}
		}
		if ((sum < 16) || (bitmaskMap[sum] != 46)) {
			if (sum >= 16) {
				if (LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type) && rivers.Find(river => river.Find(riverTile => riverTile == tile) != null) != null) {
					tile.obj.GetComponent<SpriteRenderer>().sprite = tile.tileType.riverSprites[bitmaskMap[sum]];
				} else {
					tile.obj.GetComponent<SpriteRenderer>().sprite = tile.tileType.bitmaskSprites[bitmaskMap[sum]];
				}
			} else {
				if (LiquidWaterEquivalentTileTypes.Contains(tile.tileType.type) && rivers.Find(river => river.Find(riverTile => riverTile == tile) != null) != null) {
					tile.obj.GetComponent<SpriteRenderer>().sprite = tile.tileType.riverSprites[sum];
				} else {
					tile.obj.GetComponent<SpriteRenderer>().sprite = tile.tileType.bitmaskSprites[sum];
				}
			}
		} else {
			if (!tile.tileType.baseSprites.Contains(tile.obj.GetComponent<SpriteRenderer>().sprite)) {
				tile.obj.GetComponent<SpriteRenderer>().sprite = tile.tileType.baseSprites[Random.Range(0,tile.tileType.baseSprites.Count)];
			}
		}
	}

	public void Bitmasking(List<Tile> tilesToBitmask) {
		foreach (Tile tile in tilesToBitmask) {
			if (tile != null) {
				if (BitmaskingTileTypes.Contains(tile.tileType.type)) {
					BitmaskTile(tile,true,false,null);
				} else {
					if (!tile.tileType.baseSprites.Contains(tile.obj.GetComponent<SpriteRenderer>().sprite)) {
						tile.obj.GetComponent<SpriteRenderer>().sprite = tile.tileType.baseSprites[Random.Range(0,tile.tileType.baseSprites.Count)];
					}
				}
			}
		}
		BitmaskRiverStartTiles();
	}

	void BitmaskRiverStartTiles() {
		foreach (List<Tile> river in rivers) {
			List<TileTypes> compareTileTypes = new List<TileTypes>();
			compareTileTypes.AddRange(WaterEquivalentTileTypes);
			compareTileTypes.AddRange(StoneEquivalentTileTypes);
			BitmaskTile(river[river.Count - 1],false,true,compareTileTypes);
		}
	}

	public Tile GetTileFromPosition(Vector2 position) {
		position = new Vector2(Mathf.Clamp(position.x,0,mapSize - 1),Mathf.Clamp(position.y,0,mapSize - 1));
		return sortedTiles[Mathf.FloorToInt(position.y)][Mathf.FloorToInt(position.x)];
	}
}