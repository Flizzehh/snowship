using System.Collections.Generic;
using Snowship.NPlanet;
using UnityEngine;

namespace Snowship.NColony {
	public class CreateColonyData {

		public string name;
		public int seed;
		public int size;
		public PlanetTile planetTile;

		public CreateColonyData(
			string name,
			int seed,
			int size,
			PlanetTile planetTile
		) {
			this.name = name;
			this.seed = seed;
			this.size = size;
			this.planetTile = planetTile;
		}
	};
}
