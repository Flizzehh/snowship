using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "EmptyInventory")]
	public class EmptyInventoryJob : Job
	{
		private readonly Container container;

		public EmptyInventoryJob(Container container) : base(container.tile) {
			this.container = container;

			Description = "Emptying inventory.";

			Returnable = false;
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (container != null) {
				Inventory.TransferResourcesBetweenInventories(
					Worker.Inventory,
					container.Inventory,
					Worker.Inventory.resources,
					true
				);
			}
		}
	}
}