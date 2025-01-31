using System.Collections.Generic;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "CollectResources", false)]
	public class CollectResourcesJob : Job
	{
		private readonly Container container;

		public CollectResourcesJob(Container container, List<ResourceAmount> requiredResources) : base(container.tile) {
			this.container = container;
			RequiredResources.AddRange(requiredResources);

			Description = "Collecting resources.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			container?.Inventory.TakeReservedResources(Worker);
		}
	}
}