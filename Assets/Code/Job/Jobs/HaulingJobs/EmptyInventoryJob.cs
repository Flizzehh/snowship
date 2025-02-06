using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "EmptyInventory")]
	public class EmptyInventoryJobDefinition : JobDefinition
	{
		public EmptyInventoryJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
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