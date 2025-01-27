using Snowship.NColony;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Mine")]
	public class MineJob : Job
	{
		protected MineJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			TargetName = Tile.tileType.name;
			Description = $"Mining {Tile.tileType.name}.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceRange resourceRange in Tile.tileType.resourceRanges) {
				Worker.GetInventory().ChangeResourceAmount(resourceRange.resource, Random.Range(resourceRange.min, resourceRange.max + 1), false);
			}
			Tile.SetTileType(
				Tile.HasRoof()
					? TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Dirt)
					: Tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Ground],
				false,
				true,
				true
			);

			GameManager.Get<ColonyManager>().colony.map.RemoveTileBrightnessEffect(Tile);

			foreach (LightSource lightSource in LightSource.lightSources) {
				if (Vector2.Distance(Tile.obj.transform.position, lightSource.obj.transform.position) <= lightSource.prefab.maxLightDistance) {
					lightSource.SetTileBrightnesses();
				}
			}
		}
	}
}