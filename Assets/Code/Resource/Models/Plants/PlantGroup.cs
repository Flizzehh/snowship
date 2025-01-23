using System.Collections.Generic;
using Snowship.NUtilities;

namespace Snowship.NResource
{
	public class PlantGroup
	{
		public static Dictionary<PlantGroupEnum, PlantGroup> plantGroups = new();

		public readonly PlantGroupEnum type;
		public readonly string name;

		public readonly List<PlantPrefab> prefabs;

		public PlantGroup(
			PlantGroupEnum type,
			List<PlantPrefab> prefabs
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.prefabs = prefabs;
		}

		public static PlantGroup GetPlantGroupByEnum(PlantGroupEnum plantGroupEnum) {
			return plantGroups[plantGroupEnum];
		}

		public enum PlantGroupEnum
		{
			None,
			Cactus,
			Tree,
			Bush
		};
	}
}
