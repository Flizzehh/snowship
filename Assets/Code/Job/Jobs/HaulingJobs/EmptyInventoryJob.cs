using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Empty Inventory")]
	public class EmptyInventoryJob : Job
	{
		private readonly Container container;

		protected EmptyInventoryJob(JobPrefab jobPrefab, TileManager.Tile tile, Container container) : base(jobPrefab, tile) {
			this.container = container;

			Description = "Emptying inventory.";

			Returnable = false;
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (container != null) {
				Inventory.TransferResourcesBetweenInventories(
					Worker.GetInventory(),
					container.GetInventory(),
					Worker.GetInventory().resources,
					true
				);
			}
		}
	}
}