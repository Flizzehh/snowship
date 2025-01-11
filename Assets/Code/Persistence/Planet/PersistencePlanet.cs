namespace Snowship.NPersistence {
	public class PersistencePlanet {
		public string path;

		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public int seed;
		public int size;
		public float sunDistance;
		public int temperatureRange;
		public bool randomOffsets;
		public int windDirection;

		public PersistencePlanet(string path) {
			this.path = path;
		}
	}
}
