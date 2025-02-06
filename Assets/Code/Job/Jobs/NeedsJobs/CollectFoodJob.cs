using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Food", "CollectFood")]
	public class CollectFoodJobDefinition : JobDefinition
	{
		public CollectFoodJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
			Returnable = false;
		}
	}

	public class CollectFoodJob : Job<CollectFoodJobDefinition>
	{
		private readonly Container container;
		private List<Container> alreadyCheckedContainers;
		private float nutritionTarget;

		private Colonist colonist;

		public CollectFoodJob(
			TileManager.Tile tile,
			Container container,
			List<Container> alreadyCheckedContainers,
			float nutritionTarget
		) : base(
			tile
		) {
			this.container = container;
			this.alreadyCheckedContainers = alreadyCheckedContainers ?? new List<Container>();
			this.nutritionTarget = nutritionTarget;

			Description = "Finding something to eat.";
		}

		protected override void OnJobTaken() {
			base.OnJobTaken();

			colonist = Worker as Colonist; // TODO Remove cast when Humans have Job ability
			if (colonist == null) {
				Close();
				return;
			}

			if (container == null) {
				List<ResourceAmount> currentFood = Worker.Inventory.GetResourcesByClass(Resource.ResourceClassEnum.Food).ToList();
				if (currentFood.Count > 0) {
					colonist.SetJob(new EatJob(Worker.overTile));
				}
			} else {
				alreadyCheckedContainers.Add(container);

				List<ResourceAmount> foodInContainer = container.Inventory.GetResourcesByClass(Resource.ResourceClassEnum.Food).ToList();
				List<ResourceAmount> resourcesToEat = new();
				foreach (ResourceAmount food in foodInContainer.OrderBy(ra => ((Food)ra.Resource).nutrition)) {
					nutritionTarget -= ((Food)food.Resource).nutrition;
					resourcesToEat.Add(food);
					if (nutritionTarget <= 0) {
						break;
					}
				}
				if (resourcesToEat.Count > 0) {
					container.Inventory.ReserveResources(resourcesToEat, Worker);
				}
			}
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Worker.Inventory.TakeReservedResources(Worker);

			if (nutritionTarget <= 0) {
				colonist.SetJob(new EatJob(Worker.overTile));
			} else {
				Container nextContainerToCheck = Container
					.GetContainersInRegion(Worker.overTile.region)
					.Where(c => !alreadyCheckedContainers.Contains(c))
					.OrderBy(c => PathManager.RegionBlockDistance(Worker.overTile.regionBlock, c.tile.regionBlock, true, true, false))
					.FirstOrDefault();
				if (nextContainerToCheck == null) {
					colonist.SetJob(new CollectFoodJob(colonist.overTile, null, alreadyCheckedContainers, nutritionTarget));
					return;
				}
				alreadyCheckedContainers.Add(nextContainerToCheck);
				colonist.SetJob(new CollectFoodJob(nextContainerToCheck.tile, nextContainerToCheck, alreadyCheckedContainers, nutritionTarget));
			}
		}
	}
}