using System.Collections.Generic;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "CollectResources")]
	public class CollectResourcesJobDefinition : JobDefinition
	{
		public CollectResourcesJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
		}
	}

	public class CollectResourcesJob : Job<CollectResourcesJobDefinition>
	{
		private Container Container { get; set; }

		public CollectResourcesJob(Container container, List<ResourceAmount> requiredResources) : base(container.tile) {
			Container = container;
			RequiredResources.AddRange(requiredResources);

			Description = "Collecting resources.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Container?.Inventory.TakeReservedResources(Worker);
		}
	}
}