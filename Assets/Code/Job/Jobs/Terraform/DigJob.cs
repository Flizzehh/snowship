using System.Collections.Generic;
using System.Linq;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Dig", true)]
	public class DigJob : Job
	{
		protected DigJob(TileManager.Tile tile) : base(tile) {
			TargetName = Tile.tileType.name;
			Description = $"Digging {string.Join(" and ", Tile.tileType.resourceRanges.Select(rr => rr.resource.name).ToArray())}";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Tile.dugPreviously = true;
			foreach (ResourceRange resourceRange in Tile.tileType.resourceRanges) {
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, Random.Range(resourceRange.min, resourceRange.max + 1), false);
			}
			bool setToWater = Tile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water;
			if (!setToWater) {
				foreach (TileManager.Tile nTile in Tile.horizontalSurroundingTiles) {
					if (nTile != null && nTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Water) {
						setToWater = true;
						break;
					}
				}
			}
			if (setToWater) {
				Tile.SetTileType(Tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Water], false, true, true);
				foreach (TileManager.Tile nTile in Tile.horizontalSurroundingTiles) {
					if (nTile != null && nTile.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Hole) {
						List<TileManager.Tile> frontier = new() { nTile };
						List<TileManager.Tile> checkedTiles = new() { };
						while (frontier.Count > 0) {
							TileManager.Tile currentTile = frontier[0];
							frontier.RemoveAt(0);
							checkedTiles.Add(currentTile);
							currentTile.SetTileType(currentTile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Water], true, true, true);
							foreach (TileManager.Tile nTile2 in currentTile.horizontalSurroundingTiles) {
								if (nTile2 != null && nTile2.tileType.groupType == TileManager.TileTypeGroup.TypeEnum.Hole && !checkedTiles.Contains(nTile2)) {
									frontier.Add(nTile2);
								}
							}
						}
					}
				}
			} else {
				Tile.SetTileType(Tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Hole], false, true, true);
			}
		}
	}
}