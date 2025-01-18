using System;
using System.Collections.Generic;

namespace Snowship.NColonist {
	public class TraitPrefab {

		public static readonly List<TraitPrefab> traitPrefabs = new List<TraitPrefab>();
		public ETrait type;
		public string name;
		public float effectAmount;

		public TraitPrefab() {

		}

		public static TraitPrefab GetTraitPrefabFromString(string traitTypeString) {
			return traitPrefabs.Find(traitPrefab => traitPrefab.type == (ETrait)Enum.Parse(typeof(ETrait), traitTypeString));
		}

	}
}
