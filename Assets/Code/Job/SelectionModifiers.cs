using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	public static class SelectionModifiers
	{
		public enum SelectionModifiersEnum
		{
			Outline, Walkable, OmitWalkable, WalkableIncludingFences, Buildable, OmitBuildable, StoneTypes, OmitStoneTypes, AllWaterTypes, OmitAllWaterTypes, LiquidWaterTypes, OmitLiquidWaterTypes, OmitNonStoneAndWaterTypes,
			Objects, OmitObjects, Floors, OmitFloors, Plants, OmitPlants, OmitSameLayerJobs, OmitSameLayerObjectInstances, Farms, OmitFarms, Roofs, OmitRoofs, CloseToSupport,
			ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, BiomeSupportsSelectedPlants, OmitObjectInstancesOnAdditionalTiles, Fillable
		};

		public static readonly Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ObjectPrefab, Variation, bool>> selectionModifierFunctions = new() {
			{
				SelectionModifiersEnum.Walkable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.walkable; }
			}, {
				SelectionModifiersEnum.WalkableIncludingFences, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					ObjectInstance objectInstance = posTile.GetObjectInstanceAtLayer(2);
					if (objectInstance != null && objectInstance.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Fences) {
						return true;
					}
					return posTile.walkable;
				}
			}, {
				SelectionModifiersEnum.OmitWalkable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return !posTile.walkable; }
			}, {
				SelectionModifiersEnum.Buildable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.buildable; }
			}, {
				SelectionModifiersEnum.OmitBuildable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return !posTile.buildable; }
			}, {
				SelectionModifiersEnum.StoneTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Stone; }
			}, {
				SelectionModifiersEnum.OmitStoneTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Stone; }
			}, {
				SelectionModifiersEnum.AllWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water; }
			}, {
				SelectionModifiersEnum.OmitAllWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water; }
			}, {
				SelectionModifiersEnum.LiquidWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]; }
			}, {
				SelectionModifiersEnum.OmitLiquidWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return !posTile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]; }
			}, {
				SelectionModifiersEnum.OmitNonStoneAndWaterTypes, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water && posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Stone; }
			}, {
				SelectionModifiersEnum.Plants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.plant != null; }
			}, {
				SelectionModifiersEnum.OmitPlants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.plant == null; }
			}, {
				SelectionModifiersEnum.OmitSameLayerJobs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {

					foreach (Job job in GameManager.Get<JobManager>().Jobs) {
						if (job.Layer == prefab.layer) {
							if (job.Tile == posTile) {
								return false;
							}
							if (job is BuildJob buildJob) {
								foreach (Vector2 multiTilePosition in buildJob.ObjectPrefab.multiTilePositions[buildJob.Rotation]) {
									if (GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(buildJob.Tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
										return false;
									}
								}
							}
						}
					}
					foreach (Colonist colonist in Colonist.colonists) {
						if (colonist.Job != null && colonist.Job.Layer == prefab.layer) {
							if (colonist.Job.Tile == posTile) {
								return false;
							}
							if (colonist.Job is BuildJob buildJob) {
								foreach (Vector2 multiTilePosition in buildJob.ObjectPrefab.multiTilePositions[buildJob.Rotation]) {
									if (GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(buildJob.Tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
										return false;
									}
								}
							}
						}
					}
					// foreach (Colonist colonist in Colonist.colonists) {
					// 	if (colonist.StoredJob != null && colonist.StoredJob.Layer == prefab.layer) {
					// 		if (colonist.StoredJob.Tile == posTile) {
					// 			return false;
					// 		}
					// 		if (colonist.StoredJob is BuildJob buildJob) {
					// 			foreach (Vector2 multiTilePosition in buildJob.ObjectPrefab.multiTilePositions[buildJob.Rotation]) {
					// 				if (GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(buildJob.Tile.obj.transform.position + (Vector3)multiTilePosition) == posTile) {
					// 					return false;
					// 				}
					// 			}
					// 		}
					// 	}
					// }
					return true;
				}
			}, {
				SelectionModifiersEnum.OmitSameLayerObjectInstances, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return !posTile.objectInstances.ContainsKey(prefab.layer) || posTile.objectInstances[prefab.layer] == null; }
			}, {
				SelectionModifiersEnum.Farms, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.farm != null; }
			}, {
				SelectionModifiersEnum.OmitFarms, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.farm == null; }
			}, {
				SelectionModifiersEnum.Roofs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.HasRoof(); }
			}, {
				SelectionModifiersEnum.OmitRoofs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return !posTile.HasRoof(); }
			}, {
				SelectionModifiersEnum.CloseToSupport, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					for (int y = -5; y < 5; y++) {
						for (int x = -5; x < 5; x++) {
							TileManager.Tile supportTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(new Vector2(posTile.position.x + x, posTile.position.y + y));
							if (!supportTile.buildable && !supportTile.walkable) {
								return true;
							}
						}
					}
					return false;
				}
			}, {
				SelectionModifiersEnum.ObjectsAtSameLayer, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.GetObjectInstanceAtLayer(prefab.layer) != null; }
			}, {
				SelectionModifiersEnum.Objects, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.GetAllObjectInstances().Count > 0; }
			}, {
				SelectionModifiersEnum.OmitObjects, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.GetAllObjectInstances().Count <= 0; }
			}, {
				SelectionModifiersEnum.OmitNonCoastWater, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					if (posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
						if (!(posTile.surroundingTiles.Find(t => t != null && t.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Water) != null)) {
							return false;
						}
					}
					return true;
				}
			}, {
				SelectionModifiersEnum.OmitHoles, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.tileType.groupType != TileManager.TileTypeGroup.TypeEnum.Hole; }
			}, {
				SelectionModifiersEnum.OmitPreviousDig, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return !posTile.dugPreviously; }
			}, {
				SelectionModifiersEnum.BiomeSupportsSelectedPlants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.biome.plantChances.Keys.Intersect(variation.plants.Select(plant => plant.Key.type)).Any(); }
			}, {
				SelectionModifiersEnum.OmitObjectInstancesOnAdditionalTiles, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {
					ObjectInstance objectInstance = posTile.GetObjectInstanceAtLayer(prefab.layer);
					if (objectInstance != null && objectInstance.tile != posTile) {
						return false;
					}
					return true;
				}
			}, {
				SelectionModifiersEnum.Fillable, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.dugPreviously || posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Hole || (posTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water && selectionModifierFunctions[SelectionModifiersEnum.OmitNonCoastWater](tile, posTile, prefab, variation)); }
			}
		};
	}
}