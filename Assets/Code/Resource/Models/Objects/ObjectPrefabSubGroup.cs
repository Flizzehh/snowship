using System.Collections.Generic;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class ObjectPrefabSubGroup : IGroupItem
	{
		public static Dictionary<ObjectSubGroupEnum, ObjectPrefabSubGroup> objectPrefabSubGroups = new();

		public readonly ObjectSubGroupEnum type;
		public string Name { get; }
		public Sprite Icon { get; set; }

		public List<IGroupItem> Children { get; }

		public ObjectPrefabSubGroup(ObjectSubGroupEnum type, List<IGroupItem> children) {
			this.type = type;
			Name = StringUtilities.SplitByCapitals(type.ToString());

			Children = children;
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