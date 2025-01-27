using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Transfer Resources")]
	public class TransferResourcesJob : Job
	{
		private readonly Container container;

		protected TransferResourcesJob(JobPrefab jobPrefab, TileManager.Tile tile, Container container) : base(jobPrefab, tile) {
			this.container = container;

			Description = "Transferring resources.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (container == null) {
				return;
			}
			Inventory.TransferResourcesBetweenInventories(
				Worker.GetInventory(),
				container.GetInventory(),
				RequiredResources,
				true
			);
		}
	}
}