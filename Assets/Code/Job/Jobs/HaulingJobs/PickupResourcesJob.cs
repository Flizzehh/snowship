using Snowship.NColonist;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Pickup Resources")]
	public class PickupResourcesJob : Job
	{
		private readonly Container container;

		protected PickupResourcesJob(JobPrefab jobPrefab, TileManager.Tile tile, Container container) : base(jobPrefab, tile) {
			this.container = container;

			Description = "Picking up some resources.";

			Returnable = false;
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			// TODO Remove (Colonist) cast once Human class is given Job ability
			Colonist colonist = (Colonist)Worker;

			if (container != null && colonist.StoredJobInstance != null) {
				ContainerPickup containerPickup = colonist.StoredJobInstance.containerPickups.Find(pickup => pickup.container == container);
				if (containerPickup != null) {
					foreach (ReservedResources rr in containerPickup.container.GetInventory().TakeReservedResources(colonist, containerPickup.resourcesToPickup)) {
						foreach (ResourceAmount ra in rr.resources) {
							if (containerPickup.resourcesToPickup.Find(rtp => rtp.Resource == ra.Resource) != null) {
								colonist.GetInventory().ChangeResourceAmount(ra.Resource, ra.Amount, false);
							}
						}
					}
					colonist.StoredJobInstance.containerPickups.RemoveAt(0);
				}
			}
			if (colonist.StoredJobInstance != null) {
				if (colonist.StoredJobInstance.containerPickups.Count <= 0) {
					colonist.SetJob(new ColonistJob(colonist, colonist.StoredJobInstance, colonist.StoredJobInstance.resourcesColonistHas, null));
					colonist.StoredJobInstance = null;
				} else {
					colonist.SetJob(
						new ColonistJob(
							colonist,
							new JobInstance(
								JobPrefab.GetJobPrefabByName("PickupResources"),
								colonist.StoredJobInstance.containerPickups[0].container.tile,
								ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.PickupResources),
								null,
								0
							),
							colonist.StoredJobInstance.resourcesColonistHas,
							colonist.StoredJobInstance.containerPickups
						),
						false
					);
				}
			}
		}
	}
}