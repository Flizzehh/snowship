namespace Snowship.NPlanet {

	public class CreatePlanetData {

		public string name;
		public int seed;
		public int size;
		public float distance;
		public int temperatureRange;
		public bool randomOffsets;
		public int windDirection;

		public CreatePlanetData(
			string name,
			int seed,
			int size,
			float distance,
			int temperatureRange,
			bool randomOffsets,
			int windDirection
		) {
			this.name = name;
			this.seed = seed;
			this.size = size;
			this.distance = distance;
			this.temperatureRange = temperatureRange;
			this.randomOffsets = randomOffsets;
			this.windDirection = windDirection;
		}
	}
}
