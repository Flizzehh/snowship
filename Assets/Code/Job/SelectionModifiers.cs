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
			OmitSameLayerJobs, OmitSameLayerObjectInstances, CloseToSupport,
			ObjectsAtSameLayer, OmitNonCoastWater, OmitHoles, OmitPreviousDig, BiomeSupportsSelectedPlants, OmitObjectInstancesOnAdditionalTiles, Fillable
		}

		public static readonly Dictionary<SelectionModifiersEnum, Func<TileManager.Tile, TileManager.Tile, ObjectPrefab, Variation, bool>> selectionModifierFunctions = new() {
			{
				SelectionModifiersEnum.OmitSameLayerJobs, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) {

					foreach (IJob job in GameManager.Get<JobManager>().Jobs) {
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
				SelectionModifiersEnum.BiomeSupportsSelectedPlants, delegate(TileManager.Tile tile, TileManager.Tile posTile, ObjectPrefab prefab, Variation variation) { return posTile.biome.plantChances.Keys.Intersect(variation.plants.Select(plant => plant.Key.type)).Any(); }
			}
		};
	}
}