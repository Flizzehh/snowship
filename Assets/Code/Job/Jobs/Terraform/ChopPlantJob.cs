using System;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Plants", "ChopPlant")]
	public class ChopPlantJobDefinition : JobDefinition<ChopPlantJob>
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Plant,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public ChopPlantJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class ChopPlantJob : Job<ChopPlantJobDefinition>
	{
		public ChopPlantJob(TileManager.Tile tile) : base(tile) {
			TargetName = Tile.plant.name;
			Description = $"Chopping down a {Tile.plant.prefab.name}";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceRange resourceRange in Tile.plant.prefab.returnResources) {
				int returnAmount = Mathf.CeilToInt(Random.Range(resourceRange.min, resourceRange.max + 1) / (Tile.plant.small ? 2f : 1f));
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, returnAmount, false);
			}
			if (Tile.plant.harvestResource != null) { // TODO Should it only return if it's ready to harvest/fully grown? Does it already check that?
				ResourceRange harvestResourceRange = Tile.plant.prefab.harvestResources.Find(rr => rr.resource == Tile.plant.harvestResource);
				int returnAmount = Mathf.CeilToInt(Random.Range(harvestResourceRange.min, harvestResourceRange.max + 1) / (Tile.plant.small ? 2f : 1f));
				Worker.Inventory.ChangeResourceAmount(Tile.plant.harvestResource, returnAmount, false);
			}
			// TODO Change this when Plants become Objects
			Tile.SetPlant(true, null);
		}
	}
}