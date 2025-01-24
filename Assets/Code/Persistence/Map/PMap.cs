using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NColony;
using Snowship.NResource;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PMap : TileManager.Map
	{

		private readonly PRiver pRiver = new PRiver();

		public enum TileProperty {
			Tile,
			Index,
			Height,
			TileType,
			Temperature,
			Precipitation,
			Biome,
			Roof,
			Dug,
			Sprite,
			Plant
		}

		public enum PlantProperty {
			Type,
			Sprite,
			Small,
			GrowthProgress,
			HarvestResource,
			Integrity
		}

		public void SaveOriginalTiles(StreamWriter file) {
			foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Tile, string.Empty, 0));

				file.WriteLine(PU.CreateKeyValueString(TileProperty.Height, tile.height, 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.TileType, tile.tileType.type, 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Temperature, tile.temperature, 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Precipitation, tile.GetPrecipitation(), 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Biome, tile.biome.type, 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Roof, tile.HasRoof(), 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Dug, tile.dugPreviously, 1));
				file.WriteLine(PU.CreateKeyValueString(TileProperty.Sprite, tile.sr.sprite.name, 1));

				if (tile.plant != null) {
					file.WriteLine(PU.CreateKeyValueString(TileProperty.Plant, string.Empty, 1));

					file.WriteLine(PU.CreateKeyValueString(PlantProperty.Type, tile.plant.prefab.type, 2));
					file.WriteLine(PU.CreateKeyValueString(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name, 2));
					file.WriteLine(PU.CreateKeyValueString(PlantProperty.Small, tile.plant.small, 2));
					file.WriteLine(PU.CreateKeyValueString(PlantProperty.GrowthProgress, tile.plant.growthProgress, 2));
					if (tile.plant.harvestResource != null) {
						file.WriteLine(PU.CreateKeyValueString(PlantProperty.HarvestResource, tile.plant.harvestResource.type, 2));
					}
					file.WriteLine(PU.CreateKeyValueString(PlantProperty.Integrity, tile.plant.integrity, 2));
				}
			}
		}

		public List<PersistenceTile> LoadTiles(string path) {
			List<PersistenceTile> persistenceTiles = new List<PersistenceTile>();

			List<KeyValuePair<string, object>> properties = PU.GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((TileProperty)Enum.Parse(typeof(TileProperty), property.Key)) {
					case TileProperty.Tile:
						int? tileIndex = null;
						float? tileHeight = null;
						TileManager.TileType tileType = null;
						float? tileTemperature = null;
						float? tilePrecipitation = null;
						TileManager.Biome tileBiome = null;
						bool? tileRoof = null;
						bool? tileDug = null;
						string tileSpriteName = null;

						PlantPrefab plantPrefab = null;
						string plantSpriteName = null;
						bool? plantSmall = null;
						float? plantGrowthProgress = null;
						Resource plantHarvestResource = null;
						float? plantIntegrity = null;

						foreach (KeyValuePair<string, object> tileProperty in (List<KeyValuePair<string, object>>)property.Value) {
							switch ((TileProperty)Enum.Parse(typeof(TileProperty), tileProperty.Key)) {
								case TileProperty.Index:
									tileIndex = int.Parse((string)tileProperty.Value);
									break;
								case TileProperty.Height:
									tileHeight = float.Parse((string)tileProperty.Value);
									break;
								case TileProperty.TileType:
									tileType = TileManager.TileType.GetTileTypeByString((string)tileProperty.Value);
									break;
								case TileProperty.Temperature:
									tileTemperature = float.Parse((string)tileProperty.Value);
									break;
								case TileProperty.Precipitation:
									tilePrecipitation = float.Parse((string)tileProperty.Value);
									break;
								case TileProperty.Biome:
									tileBiome = TileManager.Biome.GetBiomeByString((string)tileProperty.Value);
									break;
								case TileProperty.Roof:
									tileRoof = bool.Parse((string)tileProperty.Value);
									break;
								case TileProperty.Dug:
									tileDug = bool.Parse((string)tileProperty.Value);
									break;
								case TileProperty.Sprite:
									tileSpriteName = (string)tileProperty.Value;
									break;
								case TileProperty.Plant:
									foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)tileProperty.Value) {
										switch ((PlantProperty)Enum.Parse(typeof(PlantProperty), plantProperty.Key)) {
											case PlantProperty.Type:
												plantPrefab = PlantPrefab.GetPlantPrefabByString((string)plantProperty.Value);
												break;
											case PlantProperty.Sprite:
												plantSpriteName = (string)plantProperty.Value;
												break;
											case PlantProperty.Small:
												plantSmall = bool.Parse((string)plantProperty.Value);
												break;
											case PlantProperty.GrowthProgress:
												plantGrowthProgress = float.Parse((string)plantProperty.Value);
												break;
											case PlantProperty.HarvestResource:
												plantHarvestResource = Resource.GetResourceByEnum((EResource)Enum.Parse(typeof(EResource), (string)plantProperty.Value));
												break;
											case PlantProperty.Integrity:
												plantIntegrity = float.Parse((string)plantProperty.Value);
												break;
											default:
												Debug.LogError("Unknown plant property: " + plantProperty.Key + " " + plantProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown tile property: " + tileProperty.Key + " " + tileProperty.Value);
									break;
							}
						}

						persistenceTiles.Add(
							new PersistenceTile(
								tileIndex,
								tileHeight,
								tileType,
								tileTemperature,
								tilePrecipitation,
								tileBiome,
								tileRoof,
								tileDug,
								tileSpriteName,
								plantPrefab,
								plantSpriteName,
								plantSmall,
								plantGrowthProgress,
								plantHarvestResource,
								plantIntegrity
							));
						break;
					default:
						Debug.LogError("Unknown tile property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return persistenceTiles;
		}

		public void SaveModifiedTiles(string saveDirectoryPath, List<PersistenceTile> originalTiles) {

			StreamWriter file = PU.CreateFileAtDirectory(saveDirectoryPath, "tiles.snowship");

			TileManager.Map map = GameManager.Get<ColonyManager>().colony.map;
			if (map.tiles.Count != originalTiles.Count) {
				Debug.LogError("Loaded tile count " + map.tiles.Count + " and current tile count " + originalTiles.Count + " does not match.");
			}

			for (int i = 0; i < map.tiles.Count; i++) {
				TileManager.Tile tile = map.tiles[i];
				PersistenceTile originalTile = originalTiles[i];

				Dictionary<TileProperty, string> tileDifferences = new Dictionary<TileProperty, string>();
				Dictionary<PlantProperty, string> plantDifferences = new Dictionary<PlantProperty, string>();

				if (!Mathf.Approximately(tile.height, originalTile.tileHeight.Value)) {
					tileDifferences.Add(TileProperty.Height, tile.height.ToString());
				}

				if (tile.tileType.type != originalTile.tileType.type) {
					tileDifferences.Add(TileProperty.TileType, tile.tileType.type.ToString());
				}

				if (!Mathf.Approximately(tile.temperature, originalTile.tileTemperature.Value)) {
					tileDifferences.Add(TileProperty.Temperature, tile.temperature.ToString());
				}

				if (!Mathf.Approximately(tile.GetPrecipitation(), originalTile.tilePrecipitation.Value)) {
					tileDifferences.Add(TileProperty.Precipitation, tile.GetPrecipitation().ToString());
				}

				if (tile.biome.type != originalTile.tileBiome.type) {
					tileDifferences.Add(TileProperty.Biome, tile.biome.type.ToString());
				}

				if (tile.HasRoof() != originalTile.tileRoof.Value) {
					tileDifferences.Add(TileProperty.Roof, tile.HasRoof().ToString());
				}

				if (tile.dugPreviously != originalTile.tileDug.Value) {
					tileDifferences.Add(TileProperty.Dug, tile.dugPreviously.ToString());
				}

				if (tile.sr.sprite.name != originalTile.tileSpriteName) {
					tileDifferences.Add(TileProperty.Sprite, tile.sr.sprite.name);
				}

				if (originalTile.plantPrefab == null) {
					if (tile.plant != null) { // No original plant, plant was added
						tileDifferences.Add(TileProperty.Plant, string.Empty);

						plantDifferences.Add(PlantProperty.Type, tile.plant.prefab.type.ToString());
						plantDifferences.Add(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name);
						plantDifferences.Add(PlantProperty.Small, tile.plant.small.ToString());
						plantDifferences.Add(PlantProperty.GrowthProgress, tile.plant.growthProgress.ToString());
						if (tile.plant.harvestResource != null) {
							plantDifferences.Add(PlantProperty.HarvestResource, tile.plant.harvestResource.type.ToString());
						}
						plantDifferences.Add(PlantProperty.Integrity, tile.plant.integrity.ToString());
					}
				} else {
					if (tile.plant == null) { // Original plant, plant was removed
						tileDifferences.Add(TileProperty.Plant, string.Empty);

						plantDifferences.Add(PlantProperty.Type, "None");
					} else { // Plant has remained, properties potentially changed
						if (tile.plant.prefab.type != originalTile.plantPrefab.type) {
							plantDifferences.Add(PlantProperty.Type, tile.plant.prefab.type.ToString());
						}

						if (tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name != originalTile.plantSpriteName) {
							plantDifferences.Add(PlantProperty.Sprite, tile.plant.obj.GetComponent<SpriteRenderer>().sprite.name);
						}

						if (tile.plant.small != originalTile.plantSmall.Value) {
							plantDifferences.Add(PlantProperty.Small, tile.plant.small.ToString());
						}

						if (!Mathf.Approximately(tile.plant.growthProgress, originalTile.plantGrowthProgress.Value)) {
							plantDifferences.Add(PlantProperty.GrowthProgress, tile.plant.growthProgress.ToString());
						}

						if (tile.plant.harvestResource != originalTile.plantHarvestResource) {
							if (tile.plant.harvestResource != null) {
								plantDifferences.Add(PlantProperty.HarvestResource, tile.plant.harvestResource.type.ToString());
							} else {
								plantDifferences.Add(PlantProperty.HarvestResource, "None");
							}
						}

						if (tile.plant.integrity != originalTile.plantIntegrity.Value) {
							plantDifferences.Add(PlantProperty.Integrity, tile.plant.integrity.ToString());
						}

						if (plantDifferences.Count > 0) {
							tileDifferences.Add(TileProperty.Plant, string.Empty);
						}
					}
				}

				if (tileDifferences.Count > 0) {
					file.WriteLine(PU.CreateKeyValueString(TileProperty.Tile, string.Empty, 0));
					file.WriteLine(PU.CreateKeyValueString(TileProperty.Index, i, 1));
					foreach (KeyValuePair<TileProperty, string> tileProperty in tileDifferences) {
						file.WriteLine(PU.CreateKeyValueString(tileProperty.Key, tileProperty.Value, 1));
						if (tileProperty.Key == TileProperty.Plant) {
							foreach (KeyValuePair<PlantProperty, string> plantProperty in plantDifferences) {
								file.WriteLine(PU.CreateKeyValueString(plantProperty.Key, plantProperty.Value, 2));
							}
						}
					}
				}
			}
		}

		public void ApplyLoadedTiles(List<PersistenceTile> originalTiles, List<PersistenceTile> modifiedTiles, TileManager.Map map) {
			if (originalTiles.Count != Mathf.Pow(map.mapData.mapSize, 2)) {
				Debug.LogError("Map size " + Mathf.Pow(map.mapData.mapSize, 2) + " and number of persistence tiles " + originalTiles.Count + " does not match.");
			}

			for (int y = 0; y < map.mapData.mapSize; y++) {
				List<TileManager.Tile> innerTiles = new List<TileManager.Tile>();
				for (int x = 0; x < map.mapData.mapSize; x++) {
					int tileIndex = y * map.mapData.mapSize + x;
					PersistenceTile originalTile = originalTiles[tileIndex];
					PersistenceTile modifiedTile = modifiedTiles.Find(mt => mt.tileIndex == tileIndex);

					TileManager.Tile tile = new TileManager.Tile(map, new Vector2(x, y), modifiedTile != null && modifiedTile.tileHeight.HasValue ? modifiedTile.tileHeight.Value : originalTile.tileHeight.Value);
					map.tiles.Add(tile);
					innerTiles.Add(tile);

					tile.temperature = modifiedTile != null && modifiedTile.tileTemperature.HasValue ? modifiedTile.tileTemperature.Value : originalTile.tileTemperature.Value;
					tile.SetPrecipitation(modifiedTile != null && modifiedTile.tilePrecipitation.HasValue ? modifiedTile.tilePrecipitation.Value : originalTile.tilePrecipitation.Value);
					tile.SetBiome(modifiedTile != null && modifiedTile.tileBiome != null ? modifiedTile.tileBiome : originalTile.tileBiome, false);
					tile.SetTileType(modifiedTile != null && modifiedTile.tileType != null ? modifiedTile.tileType : originalTile.tileType, false, false, false);
					tile.SetRoof(modifiedTile != null && modifiedTile.tileRoof.HasValue ? modifiedTile.tileRoof.Value : originalTile.tileRoof.Value);
					tile.dugPreviously = modifiedTile != null && modifiedTile.tileDug.HasValue ? modifiedTile.tileDug.Value : originalTile.tileDug.Value;

					bool originalTileValidPlant = originalTile.plantPrefab != null;

					bool modifiedTilePlantGroupExists = modifiedTile != null && modifiedTile.plantPrefab != null;
					bool modifiedTileValidPlant = modifiedTilePlantGroupExists && modifiedTile.plantPrefab.type != Plant.PlantEnum.None;
					bool plantRemoved = modifiedTilePlantGroupExists && modifiedTile.plantPrefab.type == Plant.PlantEnum.None;

					if (modifiedTileValidPlant || (originalTileValidPlant && !plantRemoved)) {
						tile.SetPlant(
							false,
							new Plant(
								modifiedTile != null && modifiedTile.plantPrefab != null && modifiedTile.plantPrefab.type != Plant.PlantEnum.None ? modifiedTile.plantPrefab : originalTile.plantPrefab,
								tile,
								modifiedTile != null && modifiedTile.plantSmall.HasValue ? modifiedTile.plantSmall.Value : originalTile.plantSmall.Value,
								false,
								modifiedTile != null && modifiedTile.plantHarvestResource != null && modifiedTile.plantHarvestResource.type != EResource.None ? modifiedTile.plantHarvestResource : originalTile.plantHarvestResource
							) { integrity = modifiedTile != null && modifiedTile.plantIntegrity.HasValue ? modifiedTile.plantIntegrity.Value : originalTile.plantIntegrity.Value, growthProgress = modifiedTile != null && modifiedTile.plantGrowthProgress.HasValue ? modifiedTile.plantGrowthProgress.Value : originalTile.plantGrowthProgress.Value }
						);
					}
				}
				map.sortedTiles.Add(innerTiles);
			}

			map.SetSurroundingTiles();
			map.SetMapEdgeTiles();
			map.SetSortedMapEdgeTiles();
			map.SetTileRegions(false, true);

			map.DetermineDrainageBasins();

			map.CreateRegionBlocks();

			map.RecalculateLighting(map.tiles, true, true);

			GameManager.Get<PersistenceManager>().loadingState = PersistenceManager.LoadingState.LoadedMap;
		}

		public void ApplyMapBitmasking(List<PersistenceTile> originalTiles, List<PersistenceTile> modifiedTiles, TileManager.Map map) {
			map.Bitmasking(map.tiles, true, false);

			for (int i = 0; i < map.tiles.Count; i++) {
				TileManager.Tile tile = map.tiles[i];
				PersistenceTile originalTile = originalTiles[i];
				PersistenceTile modifiedTile = modifiedTiles.Find(mf => mf.tileIndex == i);

				Sprite tileSprite = tile.tileType.baseSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
				if (tileSprite == null) {
					tileSprite = tile.biome.tileTypes[tile.tileType.groupType].baseSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
					if (tileSprite == null) {
						tileSprite = tile.tileType.bitmaskSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
						if (tileSprite == null) {
							tileSprite = tile.tileType.riverSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.tileSpriteName != null ? modifiedTile.tileSpriteName : originalTile.tileSpriteName));
						}
					}
				}
				tile.sr.sprite = tileSprite;

				if (tile.plant != null) {
					Sprite plantSprite = null;
					if (tile.plant.small) {
						plantSprite = tile.plant.prefab.smallSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.plantSpriteName != null ? modifiedTile.plantSpriteName : originalTile.plantSpriteName));
					} else {
						plantSprite = tile.plant.prefab.fullSprites.Find(s => s.name == (modifiedTile != null && modifiedTile.plantSpriteName != null ? modifiedTile.plantSpriteName : originalTile.plantSpriteName));
					}
					if (plantSprite == null) {
						plantSprite = tile.plant.prefab.harvestResourceSprites[tile.plant.harvestResource][tile.plant.small].Find(sprite => sprite.name == (modifiedTile != null && modifiedTile.plantSpriteName != null ? modifiedTile.plantSpriteName : originalTile.plantSpriteName));
					}
					tile.plant.obj.GetComponent<SpriteRenderer>().sprite = plantSprite;
				}
			}
		}

	}
}