using Snowship.NColonist;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Collect Food")]
	public class CollectFoodJob : Job
	{
		private readonly Container container;

		protected CollectFoodJob(JobPrefab jobPrefab, TileManager.Tile tile, Container container) : base(jobPrefab, tile) {
			this.container = container;

			Description = "Finding some food to eat.";

			Returnable = false;
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (container != null) {
				foreach (ReservedResources rr in container.GetInventory().TakeReservedResources(Worker)) {
					foreach (ResourceAmount ra in rr.resources) {
						Worker.GetInventory().ChangeResourceAmount(ra.Resource, ra.Amount, false);
					}
				}
			}
			((Colonist)Worker).SetEatJob(); // TODO Remove cast when Humans have Job ability
		}
	}
}