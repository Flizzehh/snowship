using System.Collections.Generic;
using Snowship.NUtilities;

namespace Snowship.NResource
{
	public class ObjectPrefabSubGroup
	{
		public static Dictionary<ObjectSubGroupEnum, ObjectPrefabSubGroup> objectPrefabSubGroups = new();

		public readonly ObjectSubGroupEnum type;
		public readonly string name;

		public readonly ObjectPrefabGroup.ObjectGroupEnum groupType;
		public readonly List<ObjectPrefab> prefabs;

		public ObjectPrefabSubGroup(ObjectSubGroupEnum type, ObjectPrefabGroup.ObjectGroupEnum groupType, List<ObjectPrefab> prefabs) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.groupType = groupType;
			this.prefabs = prefabs;
		}

		public static ObjectPrefabSubGroup GetObjectPrefabSubGroupByEnum(ObjectSubGroupEnum objectSubGroupEnum) {
			return objectPrefabSubGroups[objectSubGroupEnum];
		}

		public enum ObjectSubGroupEnum
		{
			Roofs, Walls, Fences, Doors, Floors, Foundations,
			Beds, Chairs, Tables, Lights,
			Containers,
			TradingPosts,
			Furnaces, Processing,
			Plants, Terrain, Remove, Cancel, Priority,
			PlantFarm, HarvestFarm,
			None
		}
	}
}
