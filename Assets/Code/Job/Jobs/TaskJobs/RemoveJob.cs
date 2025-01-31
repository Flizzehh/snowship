using System.Collections.Generic;
using System.Linq;
using Snowship.NColony;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Command", "Remove", "Remove", true)]
	public class RemoveJob : Job
	{
		private readonly ObjectPrefab objectPrefab;

		public RemoveJob(TileManager.Tile tile, ObjectPrefab objectPrefab) : base(tile) {
			TargetName = Tile.GetObjectInstanceAtLayer(objectPrefab.layer).prefab.name;
			Description = $"Removing a {TargetName}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			bool previousWalkability = Tile.walkable;
			ObjectInstance instance = Tile.GetObjectInstanceAtLayer(objectPrefab.layer);
			if (instance != null) {
				foreach (ResourceAmount resourceAmount in instance.prefab.commonResources) {
					Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, Mathf.RoundToInt(resourceAmount.Amount), false);
				}
				if (instance.variation != null) {
					foreach (ResourceAmount resourceAmount in instance.variation.uniqueResources) {
						Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, Mathf.RoundToInt(resourceAmount.Amount), false);
					}
				}
				switch (instance) {
					case Farm farm: {
						if (farm.growProgressSpriteIndex == 0) {
							Worker.Inventory.ChangeResourceAmount(farm.prefab.seedResource, 1, false);
						}
						break;
					}
					case IInventory inventory: {
						List<ResourceAmount> nonReservedResourcesToRemove = new();
						foreach (ResourceAmount resourceAmount in inventory.Inventory.resources) {
							nonReservedResourcesToRemove.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
							Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, false);
						}
						foreach (ResourceAmount resourceAmount in nonReservedResourcesToRemove) {
							inventory.Inventory.ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
						}
						List<Human> humansWithReservedResources = inventory
							.Inventory
							.reservedResources
							.Select(rr => rr.human)
							.Distinct()
							.ToList();
						foreach (Human reserver in humansWithReservedResources) {
							Worker.Inventory.ReleaseReservedResources(reserver);
						}
						break;
					}
					case CraftingObject craftingObject: {
						foreach (Job removeJob in craftingObject.resources.Where(craftableResourceInstance => craftableResourceInstance.Job != null).Select(resource => resource.Job)) {
							GameManager.Get<JobManager>().RemoveJob(removeJob);
						}
						break;
					}
					case Bed bed: {
						bed.StopSleeping();
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