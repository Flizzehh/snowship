using System.Collections.Generic;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "TransferResources")]
	public class TransferResourcesJobDefinition : JobDefinition<TransferResourcesJob>
	{
		public TransferResourcesJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class TransferResourcesJob : Job<TransferResourcesJobDefinition>
	{
		private readonly Container container;

		public TransferResourcesJob(Container container, List<ResourceAmount> requiredResources) : base(container.tile) {
			this.container = container;
			RequiredResources.AddRange(requiredResources);

			Description = "Transferring resources.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (container == null) {
				return;
			}
			Inventory.TransferResourcesBetweenInventories(
				Worker.Inventory,
				container.Inventory,
				RequiredResources,
				true
			);
		}
	}
}