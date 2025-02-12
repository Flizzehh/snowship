using System;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Farm", "PlantFarm", "PlantFarm")]
	public class PlantFarmJobDefinition : JobDefinition<PlantFarmJob>
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Walkable,
			Selectable.SelectionConditions.Buildable,
			Selectable.SelectionConditions.NoObjects,
			Selectable.SelectionConditions.NoRoof,
			Selectable.SelectionConditions.NoPlant,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public PlantFarmJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class PlantFarmJob : BuildJob
	{
		public PlantFarmJob(
			TileManager.Tile tile,
			BuildJobParams args
		) : base(
			tile,
			args
		) {
			Description = $"Planting a {Variation.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (Tile.tileType.classes[TileManager.TileType.ClassEnum.Dirt]) {
				Tile.SetTileType(
					TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Mud),
					false,
					true,
					false
				);
			}
		}
	}
}