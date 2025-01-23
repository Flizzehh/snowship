using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;

namespace Snowship.NResource
{
	public class ObjectPrefabGroup
	{
		public static readonly Dictionary<ObjectGroupEnum, ObjectPrefabGroup> objectPrefabGroups = new();

		public readonly ObjectGroupEnum type;
		public readonly string name;

		public readonly List<ObjectPrefabSubGroup> subGroups = new();

		public ObjectPrefabGroup(ObjectGroupEnum type, List<ObjectPrefabSubGroup> subGroups) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.subGroups = subGroups;
		}

		public static ObjectPrefabGroup GetObjectPrefabGroupByString(string objectPrefabGroupString) {
			return GetObjectPrefabGroupByEnum((ObjectGroupEnum)Enum.Parse(typeof(ObjectGroupEnum), objectPrefabGroupString));
		}

		public static ObjectPrefabGroup GetObjectPrefabGroupByEnum(ObjectGroupEnum objectGroupEnum) {
			return objectPrefabGroups[objectGroupEnum];
		}

		public static List<ObjectPrefabGroup> GetObjectPrefabGroups() {
			return objectPrefabGroups.Values.ToList();
		}

		public enum ObjectGroupEnum
		{
			Structure, Furniture, Containers, Trading, Crafting,
			Farm,
			Terraform,
			Command,
			None
		}
	}
}
