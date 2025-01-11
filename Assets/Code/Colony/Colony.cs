using Snowship.NPersistence;

namespace Snowship.NColony {
	public class Colony {
		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;

		public TileManager.MapData mapData;
		public TileManager.Map map;

		public Colony(string name, TileManager.MapData mapData) {
			this.name = name;
			this.mapData = mapData;

			lastSaveDateTime = PersistenceManager.GenerateSaveDateTimeString();
			lastSaveTimeChunk = PersistenceManager.GenerateDateTimeString();
		}

		public void SetDirectory(string directory) {
			this.directory = directory;
		}

		public void SetLastSaveDateTime(string lastSaveDateTime, string lastSaveTimeChunk) {
			this.lastSaveDateTime = lastSaveDateTime;
			this.lastSaveTimeChunk = lastSaveTimeChunk;
		}
	}
}
