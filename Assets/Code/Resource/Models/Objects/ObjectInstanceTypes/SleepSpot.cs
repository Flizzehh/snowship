using System.Collections.Generic;
using Snowship.NColonist;

namespace Snowship.NResource
{
	public class SleepSpot : ObjectInstance
	{
		public static List<SleepSpot> sleepSpots = new();

		public Colonist occupyingColonist;

		public SleepSpot(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {

		}

		public void StartSleeping(Colonist colonist) {
			occupyingColonist = colonist;
		}

		public void StopSleeping() {
			occupyingColonist = null;
		}
	}
}
