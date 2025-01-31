﻿using System.Collections.Generic;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "TransferResources", false)]
	public class TransferResourcesJob : Job
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