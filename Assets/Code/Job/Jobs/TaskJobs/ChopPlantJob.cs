using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Chop Plant")]
	public class ChopPlantJob : Job
	{
		protected ChopPlantJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			TargetName = Tile.plant.name;
			Description = $"Chopping down a {Tile.plant.prefab.name}";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceRange resourceRange in Tile.plant.prefab.returnResources) {
				int returnAmount = Mathf.CeilToInt(Random.Range(resourceRange.min, resourceRange.max + 1) / (Tile.plant.small ? 2f : 1f));
				Worker.GetInventory().ChangeResourceAmount(resourceRange.resource, returnAmount, false);
			}
			if (Tile.plant.harvestResource != null) { // TODO Should it only return if it's ready to harvest/fully grown? Does it already check that?
				ResourceRange harvestResourceRange = Tile.plant.prefab.harvestResources.Find(rr => rr.resource == Tile.plant.harvestResource);
				int returnAmount = Mathf.CeilToInt(Random.Range(harvestResourceRange.min, harvestResourceRange.max + 1) / (Tile.plant.small ? 2f : 1f));
				Worker.GetInventory().ChangeResourceAmount(Tile.plant.harvestResource, returnAmount, false);
			}
			// TODO Change this when Plants become Objects
			Tile.SetPlant(true, null);
		}
	}
}