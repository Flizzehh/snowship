using System.Collections.Generic;
using Snowship.NPersistence;

namespace Snowship.NPlanet {
	public class Planet : TileManager.Map {

		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public List<PlanetTile> planetTiles;

		public string regenerationCode;

		public Planet(string name, TileManager.MapData mapData) : base(mapData) {

			this.name = name;

			planetTiles = new List<PlanetTile>();
			foreach (TileManager.Tile tile in tiles) {
				planetTiles.Add(new PlanetTile(this, tile));
			}

			regenerationCode = string.Format(
				"{0}{1}{2}{3}{4}",
				mapData.mapSeed.ToString().PadLeft(20, '0'),
				mapData.mapSize.ToString().PadLeft(3, '0'),
				mapData.planetDistance.ToString().PadLeft(2, '0'),
				mapData.temperatureRange.ToString().PadLeft(3, '0'),
				mapData.primaryWindDirection.ToString().PadLeft(2, '0')
			);

			lastSaveDateTime = PersistenceUtilities.GenerateSaveDateTimeString();
			lastSaveTimeChunk = PersistenceUtilities.GenerateDateTimeString();
		}

		protected Planet() {
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