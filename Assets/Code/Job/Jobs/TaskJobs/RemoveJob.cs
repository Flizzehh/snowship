using System.Collections.Generic;
using System.Linq;
using Snowship.NColony;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Remove")]
	public class RemoveJob : Job
	{
		private readonly ObjectPrefab objectPrefab;

		public RemoveJob(JobPrefab jobPrefab, TileManager.Tile tile, ObjectPrefab objectPrefab) : base(jobPrefab, tile) {
			TargetName = Tile.GetObjectInstanceAtLayer(objectPrefab.layer).prefab.name;
			Description = $"Removing a {TargetName}.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			bool previousWalkability = Tile.walkable;
			ObjectInstance instance = Tile.GetObjectInstanceAtLayer(objectPrefab.layer);
			if (instance != null) {
				foreach (ResourceAmount resourceAmount in instance.prefab.commonResources) {
					Worker.GetInventory().ChangeResourceAmount(resourceAmount.Resource, Mathf.RoundToInt(resourceAmount.Amount), false);
				}
				if (instance.variation != null) {
					foreach (ResourceAmount resourceAmount in instance.variation.uniqueResources) {
						Worker.GetInventory().ChangeResourceAmount(resourceAmount.Resource, Mathf.RoundToInt(resourceAmount.Amount), false);
					}
				}
				switch (instance) {
					case Farm farm: {
						if (farm.growProgressSpriteIndex == 0) {
							Worker.GetInventory().ChangeResourceAmount(farm.prefab.seedResource, 1, false);
						}
						break;
					}
					case IInventory inventory: {
						List<ResourceAmount> nonReservedResourcesToRemove = new();
						foreach (ResourceAmount resourceAmount in inventory.GetInventory().resources) {
							nonReservedResourcesToRemove.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
							Worker.GetInventory().ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
						}
						foreach (ResourceAmount resourceAmount in nonReservedResourcesToRemove) {
							inventory.GetInventory().ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
						}
						List<HumanManager.Human> humansWithReservedResources = inventory
							.GetInventory()
							.reservedResources
							.Select(rr => rr.human)
							.Distinct()
							.ToList();
						foreach (HumanManager.Human reserver in humansWithReservedResources) {
							Worker.GetInventory().ReleaseReservedResources(reserver);
						}
						break;
					}
					case CraftingObject craftingObject: {
						foreach (JobInstance removeJob in craftingObject.resources.Where(resource => resource.JobInstance != null).Select(resource => resource.JobInstance)) {
							GameManager.Get<JobManager>().CancelJob(removeJob);
						}
						break;
					}
					case SleepSpot sleepSpot: {
						sleepSpot.occupyingColonist?.ReturnJob();
						break;
					}
				}
				ObjectInstance.RemoveObjectInstance(instance);
				Tile.RemoveObjectAtLayer(instance.prefab.layer);
			}

			GameManager.Get<ResourceManager>().Bitmask(new List<TileManager.Tile> { Tile }.Concat(Tile.surroundingTiles).ToList());
			if (Tile.walkable && !previousWalkability) {
				GameManager.Get<ColonyManager>().colony.map.RemoveTileBrightnessEffect(Tile);
			}
		}
	}
}