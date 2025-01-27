using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Eat")]
	public class EatJob : Job
	{
		protected EatJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			Description = "Eating.";

			Returnable = false;
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			Colonist colonist = (Colonist)Worker; // TODO Remove cast when Humans have Job ability

			List<ResourceAmount> resourcesToEat = colonist.GetInventory()
				.resources
				.Where(r => r.Resource.classes.Contains(Resource.ResourceClassEnum.Food))
				.OrderBy(r => ((Food)r.Resource).nutrition)
				.ToList();
			NeedInstance foodNeed = colonist.needs.Find(need => need.prefab.type == ENeed.Food);

			float startingFoodNeedValue = foodNeed.GetValue();
			foreach (ResourceAmount ra in resourcesToEat) {
				bool stopEating = false;
				for (int i = 0; i < ra.Amount; i++) {
					if (foodNeed.GetValue() <= 0) {
						stopEating = true;
						break;
					}
					foodNeed.ChangeValue(-((Food)ra.Resource).nutrition);
					colonist.GetInventory().ChangeResourceAmount(ra.Resource, -1, false);
					if (ra.Resource.type == EResource.Apple || ra.Resource.type == EResource.BakedApple) {
						colonist.GetInventory().ChangeResourceAmount(Resource.GetResourceByEnum(EResource.AppleSeed), UnityEngine.Random.Range(1, 5), false);
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