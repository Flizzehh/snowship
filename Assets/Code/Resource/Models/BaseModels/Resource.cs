using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class Resource
	{
		public static readonly Dictionary<EResource, Resource> Resources = new();
		public static readonly Dictionary<ResourceClassEnum, List<Resource>> resourceClassToResources = new();

		public readonly EResource type;
		public string name; // Can't be readonly

		public readonly ResourceGroup.ResourceGroupEnum groupType;

		public readonly List<ResourceClassEnum> classes;

		public readonly int weight;
		public readonly int volume;

		public readonly int price;

		public Sprite image; // Can't be readonly

		// Fuel
		public readonly int fuelEnergy;

		// Crafting
		public readonly Dictionary<ObjectPrefabSubGroup.ObjectSubGroupEnum, List<ObjectPrefab.ObjectEnum>> craftingObjects;
		public readonly int craftingEnergy;
		public readonly int craftingTime;
		public readonly List<ResourceAmount> craftingResources = new(); // Filled in CreateResources() after all resources created
		public int amountCreated = 1; // Can't be readonly

		// World Amounts TODO Make into `public int WorldTotalAmount { get; private set; }`
		private int worldTotalAmount;
		private int colonistsTotalAmount;
		private int containerTotalAmount;
		private int unreservedContainerTotalAmount;
		private int unreservedTradingPostTotalAmount;
		private int availableAmount;

		public event Action<int> OnWorldTotalAmountChanged;
		public event Action<int> OnColonistsTotalAmountChanged;
		public event Action<int> OnContainerTotalAmountChanged;
		public event Action<int> OnUnreservedContainerTotalAmountChanged;
		public event Action<int> OnUnreservedTradingPostTotalAmountChanged;
		public event Action<int> OnAvailableAmountChanged;

		public enum ResourceClassEnum
		{
			Food,
			Craftable,
			Fuel,
			Clothing,
			Bag
		}

		public Resource(
			EResource type,
			ResourceGroup.ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			Dictionary<ObjectPrefabSubGroup.ObjectSubGroupEnum, List<ObjectPrefab.ObjectEnum>> craftingObjects,
			int craftingEnergy,
			int craftingTime
		) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.groupType = groupType;

			this.classes = classes;

			this.weight = weight;
			this.volume = volume;

			this.price = price;

			image = UnityEngine.Resources.Load<Sprite>(@"Sprites/Resources/" + name + "/" + name.Replace(' ', '-') + "-base");

			// Fuel
			this.fuelEnergy = fuelEnergy;

			// Crafting
			this.craftingObjects = craftingObjects;
			this.craftingEnergy = craftingEnergy;
			this.craftingTime = craftingTime;
		}

		public int GetWorldTotalAmount() {
			return worldTotalAmount;
		}

		public void SetWorldTotalAmount(int worldTotalAmount) {
			if (this.worldTotalAmount == worldTotalAmount) {
				return;
			}

			this.worldTotalAmount = worldTotalAmount;
			OnWorldTotalAmountChanged?.Invoke(worldTotalAmount);
		}

		public void AddToWorldTotalAmount(int amount) {
			SetWorldTotalAmount(GetWorldTotalAmount() + amount);
		}

		public int GetColonistsTotalAmount() {
			return colonistsTotalAmount;
		}

		public void SetColonistsTotalAmount(int colonistsTotalAmount) {
			if (this.colonistsTotalAmount == colonistsTotalAmount) {
				return;
			}

			this.colonistsTotalAmount = colonistsTotalAmount;
			OnColonistsTotalAmountChanged?.Invoke(colonistsTotalAmount);
		}

		public void AddToColonistsTotalAmount(int amount) {
			SetColonistsTotalAmount(GetColonistsTotalAmount() + amount);
		}

		public int GetContainerTotalAmount() {
			return containerTotalAmount;
		}

		public void SetContainerTotalAmount(int containerTotalAmount) {
			if (this.containerTotalAmount == containerTotalAmount) {
				return;
			}

			this.containerTotalAmount = containerTotalAmount;
			OnContainerTotalAmountChanged?.Invoke(containerTotalAmount);
		}

		public void AddToContainerTotalAmount(int amount) {
			SetContainerTotalAmount(GetContainerTotalAmount() + amount);
		}

		public int GetUnreservedContainerTotalAmount() {
			return unreservedContainerTotalAmount;
		}

		public void SetUnreservedContainerTotalAmount(int unreservedContainerTotalAmount) {
			if (this.unreservedContainerTotalAmount == unreservedContainerTotalAmount) {
				return;
			}

			this.unreservedContainerTotalAmount = unreservedContainerTotalAmount;
			OnUnreservedContainerTotalAmountChanged?.Invoke(unreservedContainerTotalAmount);
		}

		public void AddToUnreservedContainerTotalAmount(int amount) {
			SetUnreservedContainerTotalAmount(GetUnreservedContainerTotalAmount() + amount);
		}

		public int GetUnreservedTradingPostTotalAmount() {
			return unreservedTradingPostTotalAmount;
		}

		public void SetUnreservedTradingPostTotalAmount(int unreservedTradingPostTotalAmount) {
			if (this.unreservedTradingPostTotalAmount == unreservedTradingPostTotalAmount) {
				return;
			}

			this.unreservedTradingPostTotalAmount = unreservedTradingPostTotalAmount;
			OnUnreservedTradingPostTotalAmountChanged?.Invoke(unreservedTradingPostTotalAmount);
		}

		public void AddToUnreservedTradingPostTotalAmount(int amount) {
			SetUnreservedTradingPostTotalAmount(GetUnreservedTradingPostTotalAmount() + amount);
		}

		public void SetAvailableAmount(int availableAmount) {
			if (this.availableAmount == availableAmount) {
				return;
			}

			this.availableAmount = availableAmount;
			OnAvailableAmountChanged?.Invoke(availableAmount);
		}

		public void CalculateAvailableAmount() {
			availableAmount = GetColonistsTotalAmount() + GetUnreservedContainerTotalAmount();
		}

		public int GetAvailableAmount() {
			return availableAmount;
		}

		public bool CanBeCraftedBy(ObjectPrefab prefab) {
			if (craftingObjects.ContainsKey(prefab.subGroupType)) {
				if (craftingObjects[prefab.subGroupType] == null) {
					return true;
				} else {
					if (craftingObjects[prefab.subGroupType].Contains(prefab.type)) {
						return true;
					}
				}
			}
			return false;
		}

		public static List<Resource> GetResources() {
			return Resources.Values.ToList();
		}

		public static Resource GetResourceByString(string resourceString) {
			return GetResourceByEnum((EResource)Enum.Parse(typeof(EResource), resourceString));
		}

		public static Resource GetResourceByEnum(EResource resourceType) {
			return Resources[resourceType];
		}

		public static List<Resource> GetResourcesInClass(ResourceClassEnum resourceClass) {
			return resourceClassToResources[resourceClass];
		}

		public static bool IsResourceInClass(ResourceClassEnum resourceClass, Resource resource) {
			return resource.classes.Contains(resourceClass);
		}
	}
}
