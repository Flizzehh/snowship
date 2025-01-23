using Snowship.NResource;

namespace Snowship.NPersistence {
	public class PersistenceTile {

		public int? tileIndex;
		public float? tileHeight;
		public TileManager.TileType tileType;
		public float? tileTemperature;
		public float? tilePrecipitation;
		public TileManager.Biome tileBiome;
		public bool? tileRoof;
		public bool? tileDug;
		public string tileSpriteName;

		public PlantPrefab plantPrefab;
		public string plantSpriteName;
		public bool? plantSmall;
		public float? plantGrowthProgress;
		public Resource plantHarvestResource;
		public float? plantIntegrity;

		public PersistenceTile(
			int? tileIndex,
			float? tileHeight,
			TileManager.TileType tileType,
			float? tileTemperature,
			float? tilePrecipitation,
			TileManager.Biome tileBiome,
			bool? tileRoof,
			bool? tileDug,
			string tileSpriteName,
			PlantPrefab plantPrefab,
			string plantSpriteName,
			bool? plantSmall,
			float? plantGrowthProgress,
			Resource plantHarvestResource,
			float? plantIntegrity
		) {
			this.tileIndex = tileIndex;
			this.tileHeight = tileHeight;
			this.tileType = tileType;
			this.tileTemperature = tileTemperature;
			this.tilePrecipitation = tilePrecipitation;
			this.tileBiome = tileBiome;
			this.tileRoof = tileRoof;
			this.tileDug = tileDug;
			this.tileSpriteName = tileSpriteName;

			this.plantPrefab = plantPrefab;
			this.plantSpriteName = plantSpriteName;
			this.plantSmall = plantSmall;
			this.plantGrowthProgress = plantGrowthProgress;
			this.plantHarvestResource = plantHarvestResource;
			this.plantIntegrity = plantIntegrity;
		}

	}
}
