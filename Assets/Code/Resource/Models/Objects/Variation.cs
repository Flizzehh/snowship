using System.Collections.Generic;

namespace Snowship.NResource
{
	public class Variation
	{
		public ObjectPrefab prefab;

		public readonly string name;
		public readonly List<ResourceAmount> uniqueResources;
		public readonly float walkSpeed;
		public readonly int integrity;
		public readonly float flammability;
		public readonly int timeToBuild;

		public readonly string instanceName;

		// Plants
		public readonly Dictionary<PlantPrefab, Resource> plants;

		public Variation(
			ObjectPrefab prefab,
			string name,
			List<ResourceAmount> uniqueResources,
			float walkSpeed,
			int integrity,
			float flammability,
			int timeToBuild,
			Dictionary<PlantPrefab, Resource> plants
		) {
			this.prefab = prefab;
			this.name = name;
			this.uniqueResources = uniqueResources;
			this.walkSpeed = walkSpeed;
			this.integrity = integrity;
			this.flammability = flammability;
			this.timeToBuild = timeToBuild;
			this.plants = plants;
		}

		public static bool Equals(Variation v1, Variation v2) {
			return (v1 == null && v2 == null) || v1 == null || v2 == null || v1.name == v2.name;
		}

		public static readonly Variation nullVariation = new(null, null, null, 0, 0, 0, 0, null);

		public enum VariationNameOrderEnum
		{
			VariationObject,
			ObjectVariation
		}
	}
}
