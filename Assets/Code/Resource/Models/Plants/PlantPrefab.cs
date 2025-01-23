using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class PlantPrefab
	{
		public static Dictionary<Plant.PlantEnum, PlantPrefab> plantPrefabs = new();

		public readonly Plant.PlantEnum type;
		public readonly string name;

		public readonly PlantGroup.PlantGroupEnum groupType;

		public readonly Resource seed;

		public readonly bool living;

		public readonly int integrity;

		public readonly List<ResourceRange> returnResources;

		public readonly List<ResourceRange> harvestResources;

		public readonly List<Sprite> smallSprites;
		public readonly List<Sprite> fullSprites;
		public readonly Dictionary<Resource, Dictionary<bool, List<Sprite>>> harvestResourceSprites = new();

		public PlantPrefab(
			Plant.PlantEnum type,
			PlantGroup.PlantGroupEnum groupType,
			bool living,
			int integrity,
			Resource seed,
			List<ResourceRange> returnResources,
			List<ResourceRange> harvestResources
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.groupType = groupType;

			this.living = living;

			this.integrity = integrity;

			this.seed = seed;

			this.returnResources = returnResources;

			this.harvestResources = harvestResources;

			smallSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + type.ToString() + "-small").ToList();
			fullSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + type.ToString() + "-full").ToList();

			foreach (Resource harvestResource in harvestResources.Select(rr => rr.resource)) {
				Dictionary<bool, List<Sprite>> foundSpriteSizes = new();

				List<Sprite> smallResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + harvestResource.type.ToString() + "/" + type.ToString() + "-small-" + harvestResource.type.ToString().ToLower()).ToList();
				if (smallResourceSprites != null && smallResourceSprites.Count > 0) {
					foundSpriteSizes.Add(true, smallResourceSprites);
				}

				List<Sprite> fullResourceSprites = Resources.LoadAll<Sprite>(@"Sprites/Map/Plants/" + type.ToString() + "/" + harvestResource.type.ToString() + "/" + type.ToString() + "-full-" + harvestResource.type.ToString().ToLower()).ToList();
				if (fullResourceSprites != null && fullResourceSprites.Count > 0) {
					foundSpriteSizes.Add(false, fullResourceSprites);
				}

				if (foundSpriteSizes.Count > 0) {
					harvestResourceSprites.Add(harvestResource, foundSpriteSizes);
				}
			}
		}

		public static PlantPrefab GetPlantPrefabByString(string plantString) {
			return GetPlantPrefabByEnum((Plant.PlantEnum)Enum.Parse(typeof(Plant.PlantEnum), plantString));
		}

		public static PlantPrefab GetPlantPrefabByEnum(Plant.PlantEnum plantEnum) {
			return plantPrefabs[plantEnum];
		}

		public static List<PlantPrefab> GetPlantPrefabs() {
			return plantPrefabs.Values.ToList();
		}

		public static PlantPrefab GetPlantPrefabByBiome(TileManager.Biome biome, bool guaranteedTree) {
			if (guaranteedTree) {
				List<Plant.PlantEnum> biomePlantEnums = biome.plantChances.Keys.Where(plantEnum => plantEnum != Plant.PlantEnum.DeadTree).ToList();
				if (biomePlantEnums.Count > 0) {
					return GetPlantPrefabByEnum(biomePlantEnums[UnityEngine.Random.Range(0, biomePlantEnums.Count)]);
				} else {
					return null;
				}
			} else {
				foreach (KeyValuePair<Plant.PlantEnum, float> plantChancesKVP in biome.plantChances) {
					if (UnityEngine.Random.Range(0f, 1f) < biome.plantChances[plantChancesKVP.Key]) {
						return GetPlantPrefabByEnum(plantChancesKVP.Key);
					}
				}
			}
			return null;
		}
	}
}
