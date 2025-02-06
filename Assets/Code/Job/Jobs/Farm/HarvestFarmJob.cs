using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NJob
{
	[RegisterJob("Farm", "Harvest", "HarvestFarm")]
	public class HarvestFarmJobDefinition : JobDefinition
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Farm,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public HarvestFarmJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
		}
	}

	public class HarvestFarmJob : Job<HarvestFarmJobDefinition>
	{
		private Farm Farm { get; }

		public HarvestFarmJob(TileManager.Tile tile) : base(tile) {
			Farm = tile.farm;

			TargetName = Farm.name;
			Description = $"Harvesting a farm of {Farm.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (Farm != null) {
				// TODO Set the amount of seeds returned from a data file
				Worker.Inventory.ChangeResourceAmount(Farm.prefab.seedResource, Random.Range(1, 3), false);
				foreach (ResourceRange harvestResourceRange in Farm.prefab.harvestResources) {
					Worker.Inventory.ChangeResourceAmount(harvestResourceRange.resource, Random.Range(harvestResourceRange.min, harvestResourceRange.max), false);
				}

				GameManager.Get<JobManager>().AddJob(new PlantFarmJob(Tile, new BuildJobParams(Farm.prefab, Farm.variation, 0)));

				// TODO ObjectInstance.RemoveObjectInstance should probably be called inside of Tile.RemoveObjectAtLayer?
				ObjectInstance.RemoveObjectInstance(Farm);
				Tile.RemoveObjectAtLayer(Farm.prefab.layer);
			}
			GameManager.Get<ResourceManager>().Bitmask(new List<TileManager.Tile> { Tile }.Concat(Tile.surroundingTiles).ToList());
		}
	}
}