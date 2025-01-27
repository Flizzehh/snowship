using System.Linq;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Wear Clothes")]
	public class WearClothesJob : Job
	{
		protected WearClothesJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			Description = $"Wearing {RequiredResources[0].Resource.name}.";

			Returnable = false;
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceAmount resourceAmount in RequiredResources.Where(ra => ra.Resource.classes.Contains(Resource.ResourceClassEnum.Clothing))) {
				Clothing clothing = (Clothing)resourceAmount.Resource;
				Worker.ChangeClothing(clothing.prefab.appearance, clothing);
			}
		}
	}
}