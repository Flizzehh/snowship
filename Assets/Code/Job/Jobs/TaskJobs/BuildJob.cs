using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Build")]
	public class BuildJob : Job
	{
		private readonly ObjectPrefab objectPrefab;

		public BuildJob(JobPrefab jobPrefab, TileManager.Tile tile, ObjectPrefab objectPrefab, Variation variation) : base(jobPrefab, tile) {
			this.objectPrefab = objectPrefab;

			Description = $"Building a {objectPrefab.name}.";
			RequiredResources.AddRange(objectPrefab.commonResources);
			RequiredResources.AddRange(variation.uniqueResources);
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (objectPrefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Roofs) {
				Tile.SetRoof(true);
			}
		}
	}
}