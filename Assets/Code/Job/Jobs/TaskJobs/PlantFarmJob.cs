using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Plant Farm")]
	public class PlantFarmJob : BuildJob
	{
		public PlantFarmJob(
			JobPrefab jobPrefab,
			TileManager.Tile tile,
			ObjectPrefab objectPrefab,
			Variation variation
		) : base(
			jobPrefab,
			tile,
			objectPrefab,
			variation
		) {
			Description = $"Planting a {variation.name}.";
		}

		public override void OnJobFinished() {
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