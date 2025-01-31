using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Food", "Eat", false)]
	public class EatJob : Job
	{
		private Colonist colonist;
		private NeedInstance foodNeed;

		public EatJob(TileManager.Tile tile) : base(tile) {
			Description = "Eating.";

			Returnable = false;
		}

		protected override void OnJobTaken() {
			base.OnJobTaken();

			colonist = Worker as Colonist; // TODO Remove cast when Humans have Job ability
			foodNeed = colonist?.needs.Find(n => n.prefab.type == ENeed.Food);
		}

		protected override void OnJobStarted() {
			base.OnJobStarted();

			// Find a chair (ideally next to a table) for the colonist to sit at to eat
			List<ObjectInstance> chairs = new();
			foreach (ObjectPrefab chairPrefab in ObjectPrefabSubGroup.GetObjectPrefabSubGroupByEnum(ObjectPrefabSubGroup.ObjectSubGroupEnum.Chairs).prefabs) {
				List<ObjectInstance> chairsFromPrefab = ObjectInstance.GetObjectInstancesByPrefab(chairPrefab);
				if (chairsFromPrefab != null) {
					chairs.AddRange(chairsFromPrefab);
				}
			}

			ObjectInstance chair = chairs
				.Where(chair => chair.tile.region == colonist.overTile.region)
				.OrderBy(chair => PathManager.RegionBlockDistance(colonist.overTile.regionBlock, chair.tile.regionBlock, true, true, false))
				.ThenByDescending(
					chair => chair.tile.surroundingTiles.Find(
						surroundingTile => {
							ObjectInstance tableNextToChair = surroundingTile.GetObjectInstanceAtLayer(2);
							return tableNextToChair?.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Tables;
						}
					) != null
				)
				.FirstOrDefault();

		}

		protected override void OnJobInProgress() {
			base.OnJobInProgress();

		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			List<ResourceAmount> resourcesToEat = colonist.Inventory.resources
				.Where(r => r.Resource.classes.Contains(Resource.ResourceClassEnum.Food))
				.OrderBy(r => ((Food)r.Resource).nutrition)
				.ToList();

			float startingFoodNeedValue = foodNeed.GetValue();
			foreach (ResourceAmount ra in resourcesToEat) {
				bool stopEating = false;
				for (int i = 0; i < ra.Amount; i++) {
					if (foodNeed.GetValue() <= 0) {
						stopEating = true;
						break;
					}
					foodNeed.ChangeValue(-((Food)ra.Resource).nutrition);
					colonist.Inventory.ChangeResourceAmount(ra.Resource, -1, false);
					if (ra.Resource.type == EResource.Apple || ra.Resource.type == EResource.BakedApple) {
						colonist.Inventory.ChangeResourceAmount(Resource.GetResourceByEnum(EResource.AppleSeed), Random.Range(1, 5), false);
					}
				}
				if (stopEating) {
					break;
				}
			}

			float amountEaten = startingFoodNeedValue - foodNeed.GetValue();
			if (amountEaten >= 15 && foodNeed.GetValue() <= -10) {
				colonist.Moods.AddMoodModifier(MoodModifierEnum.Stuffed);
			} else if (amountEaten >= 15) {
				colonist.Moods.AddMoodModifier(MoodModifierEnum.Full);
			}

			if (foodNeed.GetValue() < 0) {
				foodNeed.SetValue(0);
			}

			ObjectInstance objectOnTile = colonist.overTile.GetObjectInstanceAtLayer(2);
			if (objectOnTile != null && objectOnTile.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Chairs) {
				if (objectOnTile.tile.surroundingTiles.Find(
					tile => {
						ObjectInstance tableNextToChair = tile.GetObjectInstanceAtLayer(2);
						if (tableNextToChair != null) {
							return tableNextToChair.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Tables;
						}
						return false;
					}
				) == null) {
					colonist.Moods.AddMoodModifier(MoodModifierEnum.AteWithoutATable);
				}
			} else {
				colonist.Moods.AddMoodModifier(MoodModifierEnum.AteOnTheFloor);
			}
		}
	}
}