using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Fill")]
	public class FillJob : Job
	{
		protected FillJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			Description = $"Filling {Tile.tileType.groupType.ToString().ToLower()}.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			TileManager.TileType fillType = TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Dirt);
			Tile.dugPreviously = false;
			foreach (ResourceRange resourceRange in fillType.resourceRanges) {
				Worker.GetInventory().ChangeResourceAmount(resourceRange.resource, -(resourceRange.max + 1), true);
			}
			Tile.SetTileType(fillType, false, true, true);
		}
	}
}