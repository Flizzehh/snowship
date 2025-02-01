using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Farm", "PlantFarm", "PlantFarm")]
	public class PlantFarmJob : BuildJob
	{
		public PlantFarmJob(
			TileManager.Tile tile,
			ObjectPrefab objectPrefab,
			Variation variation,
			int rotation
		) : base(
			tile,
			objectPrefab,
			variation,
			rotation
		) {
			Description = $"Planting a {variation.name}.";
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