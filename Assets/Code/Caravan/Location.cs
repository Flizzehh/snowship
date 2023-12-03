namespace Snowship.NCaravan {
	public class Location {

		public enum Wealth {
			Destitute,
			Poor,
			Comfortable,
			Wealthy
		}

		public enum ResourceRichness {
			Sparse,
			Average,
			Abundant
		}

		public enum CitySize {
			Hamlet,
			Village,
			Town,
			City
		}

		public string name;
		public Wealth wealth;
		public ResourceRichness resourceRichness;
		public CitySize citySize;
		public TileManager.Biome.TypeEnum biomeType;

		public Location(
			string name,
			Wealth wealth,
			ResourceRichness resourceRichness,
			CitySize citySize,
			TileManager.Biome.TypeEnum biomeType
		) {
			this.name = name;
			this.wealth = wealth;
			this.resourceRichness = resourceRichness;
			this.citySize = citySize;
			this.biomeType = biomeType;
		}
	}
}