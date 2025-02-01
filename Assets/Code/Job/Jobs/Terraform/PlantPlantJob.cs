using System.Collections.Generic;
using System.Linq;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Plants", "PlantPlant")]
	public class PlantPlantJob : Job
	{
		private readonly Variation variation;

		protected PlantPlantJob(TileManager.Tile tile, Variation variation) : base(tile) {
			this.variation = variation;

			TargetName = PlantGroup.GetPlantGroupByEnum(variation.plants.First().Key.groupType).name;
			Description = "Planting a plant.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceAmount ra in RequiredResources) {
				Worker.Inventory.ChangeResourceAmount(ra.Resource, -ra.Amount, false);
			}

			Dictionary<Plant.PlantEnum, float> plantChances = Tile.biome.plantChances
				.Where(
					plantChance => variation.plants
						.Select(plant => plant.Key.type)
						.Contains(plantChance.Key)
				)
				.ToDictionary(p => p.Key, p => p.Value);

			float totalPlantChanceWeight = plantChances.Sum(plantChance => plantChance.Value);
			float randomRangeValue = Random.Range(0, totalPlantChanceWeight);
			float iterativeTotal = 0;
			Plant.PlantEnum chosenPlantEnum = plantChances.First().Key;
			foreach ((Plant.PlantEnum plant, float chance) in plantChances) {
				if (randomRangeValue >= iterativeTotal && randomRangeValue <= iterativeTotal + chance) {
					chosenPlantEnum = plant;
					break;
				}
				iterativeTotal += chance;
			}
			PlantPrefab chosenPlantPrefab = PlantPrefab.GetPlantPrefabByEnum(chosenPlantEnum);

			Tile.SetPlant(
				false,
				new Plant(
					chosenPlantPrefab,
					Tile,
					true,
					false,
					variation.plants[chosenPlantPrefab]
				)
			);
			GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
		}
	}
}