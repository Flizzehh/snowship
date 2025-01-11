using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PersistenceColony {
		public string path;

		public Sprite lastSaveImage;

		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public Vector2 planetPosition;
		public int seed;
		public int size;
		public float averageTemperature;
		public float averagePrecipitation;
		public Dictionary<TileManager.TileTypeGroup.TypeEnum, float> terrainTypeHeights = new Dictionary<TileManager.TileTypeGroup.TypeEnum, float>();
		public List<int> surroundingPlanetTileHeights = new List<int>();
		public bool onRiver;
		public List<int> surroundingPlanetTileRivers = new List<int>();

		public PersistenceColony(string path) {
			this.path = path;
		}

		public void SetLastSaveImage(Sprite lastSaveImage) {
			this.lastSaveImage = lastSaveImage;
		}
	}

}
