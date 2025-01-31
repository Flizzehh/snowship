using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;
using UnityEngine;

namespace Snowship.NResource
{
	public class CraftingObject : ObjectInstance
	{
		public static List<CraftingObject> craftingObjectInstances = new();

		public List<CraftableResourceInstance> resources = new();
		public List<PriorityResourceInstance> fuels = new();

		public CraftingObject(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {

		}

		public override void Update() {
			base.Update();

			if (active) {
				foreach (CraftableResourceInstance resource in resources) {

					// Not enableable if job already exists
					if (resource.Job != null) {
						continue;
					}

					// Assume enableable is true by default
					resource.enableable = true;

					// Not enableable if don't have enough required resources
					foreach (ResourceAmount resourceAmount in resource.resource.craftingResources) {
						if (resourceAmount.Resource.GetAvailableAmount() < resourceAmount.Amount) {
							resource.enableable = false;
							break;
						}
					}
					if (!resource.enableable) {
						continue;
					}

					// Not enableable if don't have enough fuel
					if (resource.resource.craftingEnergy != 0) {
						int remainingCraftingEnergy = resource.resource.craftingEnergy;
						foreach (PriorityResourceInstance fuel in fuels.OrderBy(f => f.priority.Get())) {
							if (fuel.resource.GetAvailableAmount() * fuel.resource.fuelEnergy >= remainingCraftingEnergy) {
								ResourceAmount fuelResourceAmount = new(fuel.resource, Mathf.CeilToInt(remainingCraftingEnergy / (float)fuel.resource.fuelEnergy));
								resource.fuelAmounts.Add(fuelResourceAmount);
								remainingCraftingEnergy = 0;
							} else if (fuel.resource.GetAvailableAmount() > 0) {
								ResourceAmount fuelResourceAmount = new(fuel.resource, fuel.resource.GetAvailableAmount());
								resource.fuelAmounts.Add(fuelResourceAmount);
								remainingCraftingEnergy -= fuel.resource.GetAvailableAmount() * fuel.resource.fuelEnergy;
							}
							if (remainingCraftingEnergy <= 0) {
								break;
							}
						}
						if (remainingCraftingEnergy > 0) {
							resource.enableable = false;
							resource.fuelAmounts.Clear();
							continue;
						}
					}

					if (resource.enableable) {
						switch (resource.creationMethod) {
							case CraftableResourceInstance.CreationMethod.SingleRun:
								if (resource.GetRemainingAmount() > 0) {
									resource.Job = GameManager.Get<ResourceManager>().CreateResource(resource, this);
								}
								break;
							case CraftableResourceInstance.CreationMethod.MaintainStock:
								if (resource.GetTargetAmount() > resource.resource.GetAvailableAmount()) {
									resource.Job = GameManager.Get<ResourceManager>().CreateResource(resource, this);
								}
								break;
							case CraftableResourceInstance.CreationMethod.ContinuousRun:
								resource.Job = GameManager.Get<ResourceManager>().CreateResource(resource, this);
								break;
						}
					}

					if (resource.fuelAmounts.Count > 0) {
						resource.fuelAmounts.Clear();
					}
				}
			}
		}

		public override void SetActive(bool active) {
			base.SetActive(active);

			if (!active) {
				foreach (CraftableResourceInstance resource in resources) {
					if (resource.Job != null) {
						GameManager.Get<JobManager>().RemoveJob(resource.Job);
						resource.Job = null;
					}
				}
			}
		}

		public CraftableResourceInstance ToggleResource(Resource resource, int priority) {
			CraftableResourceInstance existingResource = resources.Find(r => r.resource == resource);
			if (existingResource == null) {
				existingResource = new CraftableResourceInstance(resource, priority, CraftableResourceInstance.CreationMethod.MaintainStock, 0, this);
				resources.Add(existingResource);
			} else {
				resources.Remove(existingResource);
				if (existingResource.Job != null) {
					GameManager.Get<JobManager>().RemoveJob(existingResource.Job);
					existingResource.Job = null;
				}
				existingResource = null;
			}
			return existingResource;
		}

		public PriorityResourceInstance ToggleFuel(Resource fuel, int priority) {
			PriorityResourceInstance existingFuel = fuels.Find(f => f.resource == fuel);
			if (existingFuel == null) {
				existingFuel = new PriorityResourceInstance(fuel, priority);
				fuels.Add(existingFuel);
			} else {
				fuels.Remove(existingFuel);
				foreach (CraftableResourceInstance resource in resources) {
					if (resource.fuelAmounts.Find(ra => ra.Resource == fuel) != null) {
						if (resource.Job != null) {
							GameManager.Get<JobManager>().RemoveJob(resource.Job);
							resource.Job = null;
						}
					}
				}
				existingFuel = null;
			}
			return existingFuel;
		}

		public CraftableResourceInstance GetCraftableResourceFromResource(Resource resource) {
			return resources.Find(r => r.resource == resource);
		}

		public PriorityResourceInstance GetFuelFromFuelResource(Resource resource) {
			return fuels.Find(f => f.resource == resource);
		}

		public List<Resource> GetResourcesByCraftingObject() {
			List<Resource> resources = new();
			foreach (Resource resource in Resource.GetResourcesInClass(Resource.ResourceClassEnum.Craftable)) {
				foreach (ObjectPrefabSubGroup.ObjectSubGroupEnum craftingObjectSubGroupEnum in resource.craftingObjects.Keys) {
					if (prefab.subGroupType == craftingObjectSubGroupEnum) {
						if (resource.craftingObjects[craftingObjectSubGroupEnum] == null) {
							resources.Add(resource);
							break;
						} else {
							foreach (ObjectPrefab.ObjectEnum craftingObjectEnum in resource.craftingObjects[craftingObjectSubGroupEnum]) {
								if (craftingObjectEnum == prefab.type) {
									resources.Add(resource);
									break;
								}
							}
						}
					}
				}
			}
			return resources;
		}
	}
}