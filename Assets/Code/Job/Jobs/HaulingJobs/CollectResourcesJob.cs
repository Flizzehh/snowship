using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Hauling", "Collect Resources")]
	public class CollectResourcesJob : Job
	{
		private readonly Container container;

		protected CollectResourcesJob(JobPrefab jobPrefab, TileManager.Tile tile, Container container) : base(jobPrefab, tile) {
			this.container = container;

			Description = "Collecting resources.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (container == null) {
				return;
			}
			foreach (ReservedResources rr in container.GetInventory().TakeReservedResources(Worker)) {
				foreach (ResourceAmount ra in rr.resources) {
					Worker.GetInventory().ChangeResourceAmount(ra.Resource, ra.Amount, false);
				}
			}
		}
	}
}