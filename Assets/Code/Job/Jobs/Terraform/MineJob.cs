using System;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Mine")]
	public class MineJobDefinition : JobDefinition
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Stone,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public MineJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
		}
	}

	public class MineJob : Job<MineJobDefinition>
	{
		protected MineJob(TileManager.Tile tile) : base(tile) {
			TargetName = Tile.tileType.name;
			Description = $"Mining {Tile.tileType.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceRange resourceRange in Tile.tileType.resourceRanges) {
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, Random.Range(resourceRange.min, resourceRange.max + 1), false);
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