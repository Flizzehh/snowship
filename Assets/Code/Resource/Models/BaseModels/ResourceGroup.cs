using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class ResourceGroup
	{
		public static readonly Dictionary<ResourceGroupEnum, ResourceGroup> resourceGroups = new();

		public readonly ResourceGroupEnum type;
		public readonly string name;

		public readonly List<Resource> resources;

		public ResourceGroup(
			ResourceGroupEnum type,
			List<Resource> resources
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.resources = resources;
		}

		public static ResourceGroup GetResourceGroupByEnum(ResourceGroupEnum resourceGroupEnum) {
			return resourceGroups[resourceGroupEnum];
		}

		public static List<ResourceGroup> GetResourceGroups() {
			return resourceGroups.Values.ToList();
		}

		public static ResourceGroup GetRandomResourceGroup() {
			List<ResourceGroup> resourcesGroupsWithoutNone = GetResourceGroups().Where(rg => rg.type != ResourceGroupEnum.None).ToList();
			return resourcesGroupsWithoutNone[Random.Range(0, resourcesGroupsWithoutNone.Count)];
		}

		public enum ResourceGroupEnum
		{
			None,
			NaturalResources,
			Ores,
			Metals,
			Materials,
			Seeds,
			RawFoods,
			Foods,
			Coins,
			Clothing
		}
	}
}
