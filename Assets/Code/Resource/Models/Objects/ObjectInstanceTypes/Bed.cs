using System.Collections.Generic;
using Snowship.NHuman;

namespace Snowship.NResource
{
	public class Bed : ObjectInstance
	{
		public static readonly List<Bed> Beds = new();

		public Human Occupant;

		public Bed(
			ObjectPrefab prefab,
			Variation variation,
			TileManager.Tile tile,
			int rotationIndex
		) : base(
			prefab,
			variation,
			tile,
			rotationIndex
		) {

		}

		public void StartSleeping(Human human) {
			Occupant = human;
		}

		public void StopSleeping() {
			Occupant = null;
		}
	}
}