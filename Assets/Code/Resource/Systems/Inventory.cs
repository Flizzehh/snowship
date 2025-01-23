using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;
using Snowship.NResource;
using UnityEngine;

public class Inventory
{
	public List<ResourceAmount> resources = new();
	public List<ReservedResources> reservedResources = new();

	public IInventory parent;

	public int maxWeight;
	public int maxVolume;

	public event Action<Inventory> OnInventoryChanged;
	public event Action<ResourceAmount> OnResourceAmountAdded;
	public event Action<ResourceAmount> OnResourceAmountRemoved;

	public event Action<ReservedResources> OnReservedResourcesAdded;
	public event Action<ReservedResources> OnReservedResourcesRemoved;

	public Inventory(IInventory parent, /*int maxAmount*/int maxWeight, int maxVolume) {
		this.parent = parent;
		this.maxWeight = maxWeight;
		this.maxVolume = maxVolume;
	}

	public int UsedWeight() {
		return resources.Sum(ra => ra.Amount * ra.Resource.weight) + reservedResources.Sum(rr => rr.resources.Sum(ra => ra.Amount * ra.Resource.weight));
	}

	public int UsedVolume() {
		return resources.Sum(ra => ra.Amount * ra.Resource.volume) + reservedResources.Sum(rr => rr.resources.Sum(ra => ra.Amount * ra.Resource.volume));
	}

	public int ChangeResourceAmount(Resource resource, int amount, bool limitToMaxAmount) {
		int highestOverflowAmount = 0;

		if (limitToMaxAmount && amount > 0) {
			int weightOverflowAmount = (resource.weight * amount - maxWeight) / resource.weight;
			int volumeOverflowAmount = (resource.volume * amount - maxVolume) / resource.volume;

			highestOverflowAmount = Mathf.Max(weightOverflowAmount, volumeOverflowAmount);

			if (highestOverflowAmount > 0) {
				amount -= highestOverflowAmount;
			}
		}

		ResourceAmount existingResourceAmount = resources.Find(ra => ra.Resource == resource);
		if (existingResourceAmount != null) {
			if (amount >= 0 || amount - existingResourceAmount.Amount >= 0) {
				existingResourceAmount.Amount += amount;
			} else if (amount < 0 && existingResourceAmount.Amount + amount >= 0) {
				existingResourceAmount.Amount += amount;
			}
		} else {
			if (amount > 0) {
				ResourceAmount newResourceAmount = new(resource, amount);
				resources.Add(newResourceAmount);
				OnResourceAmountAdded?.Invoke(newResourceAmount);
			}
		}
		existingResourceAmount = resources.Find(ra => ra.Resource == resource);
		if (existingResourceAmount is { Amount: 0 }) {
			resources.Remove(existingResourceAmount);
			OnResourceAmountRemoved?.Invoke(existingResourceAmount);
		}

		GameManager.resourceM.CalculateResourceTotals();
		ColonistJob.UpdateColonistJobs();

		OnInventoryChanged?.Invoke(this);

		//return remainingAmount;
		return highestOverflowAmount;
	}

	public bool ReserveResources(List<ResourceAmount> resourcesToReserve, HumanManager.Human humanReservingResources) {
		bool allResourcesFound = true;
		foreach (ResourceAmount raReserve in resourcesToReserve) {
			ResourceAmount raInventory = resources.Find(ra => ra.Resource == raReserve.Resource);
			if (!(raInventory != null && raInventory.Amount >= raReserve.Amount)) {
				allResourcesFound = false;
			}
		}
		if (allResourcesFound) {
			foreach (ResourceAmount raReserve in resourcesToReserve) {
				ResourceAmount raInventory = resources.Find(ra => ra.Resource == raReserve.Resource);
				ChangeResourceAmount(raInventory.Resource, -raReserve.Amount, false);
			}
			ReservedResources rr = new(resourcesToReserve, humanReservingResources);
			reservedResources.Add(rr);
			OnReservedResourcesAdded?.Invoke(rr);
		}

		OnInventoryChanged?.Invoke(this);

		return allResourcesFound;
	}

	public List<ReservedResources> TakeReservedResources(HumanManager.Human humanReservingResources, List<ResourceAmount> resourcesToTake = null) {
		List<ReservedResources> reservedResourcesByHuman = new();
		foreach (ReservedResources rr in reservedResources) {
			if (rr.human == humanReservingResources && (resourcesToTake == null || rr.resources.Find(ra => resourcesToTake.Find(rtt => rtt.Resource == ra.Resource) != null) != null)) {
				reservedResourcesByHuman.Add(rr);
			}
		}
		foreach (ReservedResources rr in reservedResourcesByHuman) {
			reservedResources.Remove(rr);
			OnReservedResourcesRemoved?.Invoke(rr);
		}

		OnInventoryChanged?.Invoke(this);

		return reservedResourcesByHuman;
	}

	public void ReleaseReservedResources(HumanManager.Human human) {
		List<ReservedResources> reservedResourcesToRemove = new();
		foreach (ReservedResources rr in reservedResources) {
			if (rr.human == human) {
				foreach (ResourceAmount ra in rr.resources) {
					ChangeResourceAmount(ra.Resource, ra.Amount, false);
				}
				reservedResourcesToRemove.Add(rr);
			}
		}
		foreach (ReservedResources rrRemove in reservedResourcesToRemove) {
			reservedResources.Remove(rrRemove);
			OnReservedResourcesRemoved?.Invoke(rrRemove);
		}
		reservedResourcesToRemove.Clear();
		OnInventoryChanged?.Invoke(this);
	}

	public static void TransferResourcesBetweenInventories(Inventory fromInventory, Inventory toInventory, ResourceAmount resourceAmount, bool limitToMaxAmount) {
		Resource resource = resourceAmount.Resource;
		int amount = resourceAmount.Amount;

		fromInventory.ChangeResourceAmount(resource, -amount, false);
		int remainingAmount = toInventory.ChangeResourceAmount(resource, amount, limitToMaxAmount);
		if (remainingAmount > 0) {
			fromInventory.ChangeResourceAmount(resource, remainingAmount, false);
		}
	}

	public static void TransferResourcesBetweenInventories(Inventory fromInventory, Inventory toInventory, List<ResourceAmount> resourceAmounts, bool limitToMaxAmount) {
		foreach (ResourceAmount resourceAmount in resourceAmounts.ToList()) {
			TransferResourcesBetweenInventories(
				fromInventory,
				toInventory,
				resourceAmount,
				limitToMaxAmount
			);
		}
	}

	public bool ContainsResourceAmount(ResourceAmount resourceAmount) {
		ResourceAmount matchingResource = resources.Find(ra => ra.Resource == resourceAmount.Resource);
		if (matchingResource != null) {
			return matchingResource.Amount <= resourceAmount.Amount;
		}
		return false;
	}

	public List<ReservedResources> GetReservedResourcesByHuman(HumanManager.Human human) {
		return reservedResources.Where(rr => rr.human == human).ToList();
	}
}