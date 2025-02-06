﻿using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Hauling", "PickupResources")]
	public class PickupResourcesJobDefinition : JobDefinition
	{
		public PickupResourcesJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
			Returnable = false;
		}
	}

	public class PickupResourcesJob : Job<PickupResourcesJobDefinition>
	{
		private readonly Container container;

		protected PickupResourcesJob(Container container) : base(container.tile) {
			this.container = container;

			Description = "Picking up some resources.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Colonist colonist = (Colonist)Worker; // TODO Remove (Colonist) cast once Human class is given Job ability

			// TODO Rework to work similarly to CollectFoodJob perhaps?
			/*if (container != null && colonist.StoredJob != null) {
				ContainerPickup containerPickup = colonist.StoredJob.containerPickups.Find(pickup => pickup.container == container);
				if (containerPickup != null) {
					foreach (ReservedResources rr in containerPickup.container.Inventory.TakeReservedResources(colonist, containerPickup.resourcesToPickup)) {
						foreach (ResourceAmount ra in rr.resources) {
							if (containerPickup.resourcesToPickup.Find(rtp => rtp.Resource == ra.Resource) != null) {
								colonist.Inventory.ChangeResourceAmount(ra.Resource, ra.Amount, false);
							}
						}
					}
					colonist.StoredJob.containerPickups.RemoveAt(0);
				}
			}
			if (colonist.StoredJob != null) {
				if (colonist.StoredJob.containerPickups.Count <= 0) {
					colonist.SetJob(colonist.StoredJob);
					colonist.StoredJob = null;
				} else {
					colonist.SetJob(
						new ColonistJob(
							colonist,
							new PickupResourcesJob(colonist.StoredJob.containerPickups[0].container),
							colonist.StoredJob.resourcesColonistHas,
							colonist.StoredJob.containerPickups
						),
						false
					);
				}
			}*/
		}
	}
}