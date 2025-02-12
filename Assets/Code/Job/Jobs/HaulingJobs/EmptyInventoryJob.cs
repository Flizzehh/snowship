using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "EmptyInventory")]
	public class EmptyInventoryJobDefinition : JobDefinition<EmptyInventoryJob>
	{
		public EmptyInventoryJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
			Returnable = false;
		}
	}

	public class EmptyInventoryJob : Job<EmptyInventoryJobDefinition>
	{
		private readonly Container container;

		public EmptyInventoryJob(Container container) : base(container.tile) {
			this.container = container;

			Description = "Emptying inventory.";
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