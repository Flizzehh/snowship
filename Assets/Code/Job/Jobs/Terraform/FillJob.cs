using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Fill", true)]
	public class FillJob : Job
	{
		protected FillJob(TileManager.Tile tile) : base(tile) {
			Description = $"Filling {Tile.tileType.groupType.ToString().ToLower()}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			TileManager.TileType fillType = TileManager.TileType.GetTileTypeByEnum(TileManager.TileType.TypeEnum.Dirt);
			Tile.dugPreviously = false;
			foreach (ResourceRange resourceRange in fillType.resourceRanges) {
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, -(resourceRange.max + 1), true);
			}
			Tile.SetTileType(fillType, false, true, true);
		}
	}
}