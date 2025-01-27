using System.Collections.Generic;
using Snowship.NColony;
using Snowship.NJob;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NResource
{
	public class Farm : ObjectInstance
	{
		public static List<Farm> farms = new();

		public readonly string name;

		public float growTimer = 0;

		public int growProgressSpriteIndex = -1;
		public readonly List<Sprite> growProgressSprites = new();
		public readonly int maxSpriteIndex = 0;

		private readonly float precipitationGrowthMultiplier;
		private readonly float temperatureGrowthMultipler;

		public Farm(ObjectPrefab prefab, Variation variation, TileManager.Tile tile) : base(prefab, variation, tile, 0) {
			name = prefab.harvestResources[0].resource.name + " Farm";

			growProgressSprites = prefab.GetBitmaskSpritesForVariation(variation);
			maxSpriteIndex = growProgressSprites.Count - 1;

			precipitationGrowthMultiplier = CalculatePrecipitationGrowthMultiplierForTile(tile);
			temperatureGrowthMultipler = CalculateTemperatureGrowthMultiplierForTile(tile);

			Update();
		}

		public override void Update() {
			base.Update();

			if (growTimer >= prefab.growthTimeDays * SimulationDateTime.DayLengthSeconds) {
				if (!GameManager.Get<JobManager>().JobOfTypeExistsAtTile("HarvestFarm", tile)) {
					GameManager.Get<JobManager>()
						.CreateJob(
							new JobInstance(
							JobPrefab.GetJobPrefabByName("HarvestFarm"),
							tile,
							ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.HarvestFarm),
							null,
							0
						)
					);
				}
			} else {
				growTimer += CalculateGrowthRate();

				int newGrowProgressSpriteIndex = Mathf.FloorToInt(growTimer / (prefab.growthTimeDays * SimulationDateTime.DayLengthSeconds + 10) * growProgressSprites.Count);
				if (newGrowProgressSpriteIndex != growProgressSpriteIndex) {
					growProgressSpriteIndex = newGrowProgressSpriteIndex;
					obj.GetComponent<SpriteRenderer>().sprite = growProgressSprites[Mathf.Clamp(growProgressSpriteIndex, 0, maxSpriteIndex)];
				}
			}
		}

		public float CalculateGrowthRate() {
			float growthRate = GameManager.Get<TimeManager>().Time.DeltaTime;
			growthRate *= Mathf.Max(GameManager.Get<ColonyManager>().colony.map.CalculateBrightnessLevelAtHour(GameManager.Get<TimeManager>().Time.TileBrightnessTime), tile.lightSourceBrightness);
			growthRate *= precipitationGrowthMultiplier;
			growthRate *= temperatureGrowthMultipler;
			growthRate = Mathf.Clamp(growthRate, 0, 1);
			return growthRate;
		}

		public static float CalculatePrecipitationGrowthMultiplierForTile(TileManager.Tile tile) {
			return Mathf.Min(-2 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 2) + 1, -30 * Mathf.Pow(tile.GetPrecipitation() - 0.7f, 3) + 1);
		}

		public static float CalculateTemperatureGrowthMultiplierForTile(TileManager.Tile tile) {
			return Mathf.Clamp(Mathf.Min((tile.temperature - 10) / 15f + 1, -((tile.temperature - 50) / 20f)), 0, 1);
		}

		public static readonly Dictionary<EResource, EResource> farmSeedToReturnResource = new() {
			{ EResource.WheatSeed, EResource.Wheat },
			{ EResource.Potato, EResource.Potato },
			{ EResource.CottonSeed, EResource.Cotton }
		};
		public static readonly Dictionary<EResource, ObjectPrefab.ObjectEnum> farmSeedToObject = new() {
			{ EResource.WheatSeed, ObjectPrefab.ObjectEnum.WheatFarm },
			{ EResource.Potato, ObjectPrefab.ObjectEnum.PotatoFarm },
			{ EResource.CottonSeed, ObjectPrefab.ObjectEnum.CottonFarm }
		};
	}
}