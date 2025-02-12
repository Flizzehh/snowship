using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class ObjectPrefabGroup : IGroupItem
	{
		public static readonly Dictionary<ObjectGroupEnum, ObjectPrefabGroup> objectPrefabGroups = new();

		public readonly ObjectGroupEnum type;

		public string Name { get; }
		public Sprite Icon { get; set; }

		public List<IGroupItem> Children { get; }

		public ObjectPrefabGroup(ObjectGroupEnum type, List<IGroupItem> children) {
			this.type = type;
			Name = StringUtilities.SplitByCapitals(type.ToString());

			Children = children;
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