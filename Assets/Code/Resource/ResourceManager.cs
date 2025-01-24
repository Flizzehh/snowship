using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NJob;
using Snowship.NPersistence;
using Snowship.NPlanet;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : IManager, IDisposable
{
	public GameObject tilePrefab;
	public GameObject objectPrefab;
	public GameObject humanPrefab;
	public Sprite selectionCornersSprite;
	public Sprite whiteSquareSprite;
	public Sprite clearSquareSprite;
	public GameObject planetTilePrefab;
	public GameObject colonyObj;
	public GameObject tileImage;
	public GameObject objectDataPanel;

	public GameObject tileParent;
	public GameObject colonistParent;
	public GameObject traderParent;
	public GameObject selectionParent;
	public GameObject jobParent;

	public event Action OnResourceTotalsUpdated;

	private static readonly List<string> locationNames = new();

	public void OnCreate() {
		SetResourceReferences();
		SetGameObjectReferences();
		CreateJobPrefabs();
		CreateResources();
		CreatePlantPrefabs();
		CreateObjectPrefabs();
		LoadLocationNames();
	}

	public void OnGameSetupComplete() {
		GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
	}

	public void Dispose() {
		GameManager.Get<TimeManager>().OnTimeChanged -= OnTimeChanged;
	}

	private void OnTimeChanged(SimulationDateTime time) {
		GrowPlants();
	}

	private void SetResourceReferences() {
		tilePrefab = Resources.Load<GameObject>(@"Prefabs/Tile");
		objectPrefab = Resources.Load<GameObject>(@"Prefabs/Object");
		humanPrefab = Resources.Load<GameObject>(@"Prefabs/Human");
		selectionCornersSprite = Resources.Load<Sprite>(@"UI/selectionCorners");
		whiteSquareSprite = Resources.Load<Sprite>(@"UI/white-square");
		clearSquareSprite = Resources.Load<Sprite>(@"UI/clear-square");
		planetTilePrefab = Resources.Load<GameObject>(@"UI/UIElements/PlanetTile");
		colonyObj = Resources.Load<GameObject>(@"UI/UIElements/ColonyObj");
		tileImage = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-TileImage");
		objectDataPanel = Resources.Load<GameObject>(@"UI/UIElements/TileInfoElement-ObjectData-Panel");
	}

	private void SetGameObjectReferences() {
		tileParent = GameObject.Find("TileParent");
		colonistParent = GameObject.Find("ColonistParent");
		traderParent = GameObject.Find("TraderParent");
		selectionParent = GameObject.Find("SelectionParent");
		jobParent = GameObject.Find("JobParent");
	}

	public void OnUpdate() {
		CalculateResourceTotals();

		foreach (Farm farm in Farm.farms) {
			farm.Update();
		}
		foreach (CraftingObject craftingObject in CraftingObject.craftingObjectInstances) {
			craftingObject.Update();
		}
	}

	private enum ResourceGroupPropertyEnum
	{
		ResourceGroup,
		Type,
		Resources
	}

	private enum ResourcePropertyEnum
	{
		Resource,
		Type,
		Classes,
		Weight,
		Volume,
		Price,
		Food,
		Fuel,
		Crafting,
		Clothing
	}

	private enum ResourceFoodPropertyEnum
	{
		Nutrition
	}

	private enum ResourceFuelPropertyEnum
	{
		FuelEnergy
	}

	private enum ResourceCraftingPropertyEnum
	{
		Objects,
		CraftingEnergy,
		CraftingTime,
		Resources
	}

	private enum ResourceClothingPropertyEnum
	{
		Appearance,
		ClothingType,
		Insulation,
		WaterResistance,
		WeightCapacity,
		VolumeCapacity,
		Colours
	}

	private void CreateResources() {

		Dictionary<EResource, List<(EResource resource, float amount)>> craftingResourcesTemp = new();

		List<KeyValuePair<string, object>> resourceGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/resources").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> resourceGroupProperty in resourceGroupProperties) {
			switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupProperty.Key)) {
				case ResourceGroupPropertyEnum.ResourceGroup:

					ResourceGroup resourceGroup = ParseResourceGroup(craftingResourcesTemp, resourceGroupProperty);
					ResourceGroup.resourceGroups.Add(resourceGroup.type, resourceGroup);

					break;
				default:
					Debug.LogError("Unknown resource group property: " + resourceGroupProperty.Key + " " + resourceGroupProperty.Value);
					break;

			}
		}

		// Set Resource Classes
		foreach (Resource.ResourceClassEnum resourceClassEnum in Enum.GetValues(typeof(Resource.ResourceClassEnum))) {
			Resource.resourceClassToResources.Add(resourceClassEnum, new List<Resource>());
		}

		foreach (Resource resource in Resource.GetResources()) {
			foreach (Resource.ResourceClassEnum resourceClassEnum in resource.classes) {
				Resource.resourceClassToResources[resourceClassEnum].Add(resource);
			}
		}

		// Set Crafting Resources
		foreach (KeyValuePair<EResource, List<(EResource resource, float amount)>> craftingResourceToResourceAmount in craftingResourcesTemp) {
			List<Resource> resourcesToApplyTo = new();
			Resource craftableResource = Resource.GetResourceByEnum(craftingResourceToResourceAmount.Key);
			if (craftableResource.classes.Contains(Resource.ResourceClassEnum.Clothing)) {
				Clothing craftableClothing = (Clothing)craftableResource;
				foreach (Clothing resource in Resource.GetResourcesInClass(Resource.ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.clothingType == craftableClothing.prefab.clothingType)) {
					resourcesToApplyTo.Add(resource);
				}
			} else {
				resourcesToApplyTo.Add(craftableResource);
			}
			foreach (Resource resource in resourcesToApplyTo) {
				foreach ((EResource resource, float amount) resourceAmount in craftingResourceToResourceAmount.Value) {
					float amount = resourceAmount.amount;
					if (amount is < 1 and > 0) {
						resource.amountCreated = Mathf.RoundToInt(1 / amount);
						resource.craftingResources.Add(new ResourceAmount(Resource.GetResourceByEnum(resourceAmount.resource), 1));
					} else {
						resource.amountCreated = 1;
						resource.craftingResources.Add(new ResourceAmount(Resource.GetResourceByEnum(resourceAmount.resource), Mathf.RoundToInt(resourceAmount.amount)));
					}
				}
			}
		}
	}

	private ResourceGroup ParseResourceGroup(
		Dictionary<EResource, List<(EResource resource, float amount)>> craftingResourcesTemp,
		KeyValuePair<string, object> resourceGroupProperty
	) {
		ResourceGroup.ResourceGroupEnum? groupType = null;
		List<Resource> resources = new();

		foreach (KeyValuePair<string, object> resourceGroupSubProperty in (List<KeyValuePair<string, object>>)resourceGroupProperty.Value) {
			switch ((ResourceGroupPropertyEnum)Enum.Parse(typeof(ResourceGroupPropertyEnum), resourceGroupSubProperty.Key)) {
				case ResourceGroupPropertyEnum.Type:
					groupType = (ResourceGroup.ResourceGroupEnum)Enum.Parse(typeof(ResourceGroup.ResourceGroupEnum), (string)resourceGroupSubProperty.Value);
					break;
				case ResourceGroupPropertyEnum.Resources:
					foreach (KeyValuePair<string, object> resourceProperty in (List<KeyValuePair<string, object>>)resourceGroupSubProperty.Value) {
						switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceProperty.Key)) {
							case ResourcePropertyEnum.Resource:

								System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

								Resource resource = ParseResource(craftingResourcesTemp, groupType.Value, resourceProperty);
								resources.Add(resource);
								Resource.Resources.Add(resource.type, resource);

								break;
							default:
								Debug.LogError("Unknown resource property: " + resourceProperty.Key + " " + resourceProperty.Value);
								break;
						}
					}
					break;
				default:
					Debug.LogError("Unknown resource group sub property: " + resourceGroupSubProperty.Key + " " + resourceGroupSubProperty.Value);
					break;
			}

		}

		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

		return new ResourceGroup(
			groupType.Value,
			resources
		);
	}

	private Resource ParseResource(
		Dictionary<EResource, List<(EResource resource, float amount)>> craftingResourcesTemp,
		ResourceGroup.ResourceGroupEnum groupType,
		KeyValuePair<string, object> resourceProperty
	) {
		EResource? type = null;
		List<Resource.ResourceClassEnum> classes = new();
		int? weight = null;
		int? volume = null;
		int? price = null;

		// Food
		int? foodNutrition = null;

		// Fuel
		int? fuelEnergy = 0;

		// Crafting
		Dictionary<ObjectPrefabSubGroup.ObjectSubGroupEnum, List<ObjectPrefab.ObjectEnum>> craftingObjects = new();
		int? craftingEnergy = 0;
		int? craftingTime = 0;
		// craftingResources -> craftingResourcesTemp

		// Clothing
		HumanManager.Human.Appearance? clothingAppearance = null;
		Clothing.ClothingEnum? clothingType = null;
		int? clothingInsulation = null;
		int? clothingWaterResistance = null;
		int? clothingWeightCapacity = 0;
		int? clothingVolumeCapacity = 0;
		List<string> clothingColours = new();

		foreach (KeyValuePair<string, object> resourceSubProperty in (List<KeyValuePair<string, object>>)resourceProperty.Value) {
			switch ((ResourcePropertyEnum)Enum.Parse(typeof(ResourcePropertyEnum), resourceSubProperty.Key)) {
				case ResourcePropertyEnum.Type:
					type = (EResource)Enum.Parse(typeof(EResource), (string)resourceSubProperty.Value);
					break;
				case ResourcePropertyEnum.Classes:
					foreach (string classString in ((string)resourceSubProperty.Value).Split(',')) {
						classes.Add((Resource.ResourceClassEnum)Enum.Parse(typeof(Resource.ResourceClassEnum), classString));
					}
					break;
				case ResourcePropertyEnum.Weight:
					weight = int.Parse((string)resourceSubProperty.Value);
					break;
				case ResourcePropertyEnum.Volume:
					volume = int.Parse((string)resourceSubProperty.Value);
					break;
				case ResourcePropertyEnum.Price:
					price = int.Parse((string)resourceSubProperty.Value);
					break;
				case ResourcePropertyEnum.Food:
					foreach (KeyValuePair<string, object> foodProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
						switch ((ResourceFoodPropertyEnum)Enum.Parse(typeof(ResourceFoodPropertyEnum), foodProperty.Key)) {
							case ResourceFoodPropertyEnum.Nutrition:
								foodNutrition = int.Parse((string)foodProperty.Value);
								break;
							default:
								Debug.LogError("Unknown resource food property: " + foodProperty.Key + " " + foodProperty.Value);
								break;
						}
					}
					break;
				case ResourcePropertyEnum.Fuel:
					foreach (KeyValuePair<string, object> fuelProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
						switch ((ResourceFuelPropertyEnum)Enum.Parse(typeof(ResourceFuelPropertyEnum), fuelProperty.Key)) {
							case ResourceFuelPropertyEnum.FuelEnergy:
								fuelEnergy = int.Parse((string)fuelProperty.Value);
								break;
							default:
								Debug.LogError("Unknown resource fuel property: " + fuelProperty.Key + " " + fuelProperty.Value);
								break;
						}
					}
					break;
				case ResourcePropertyEnum.Crafting:
					foreach (KeyValuePair<string, object> craftingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
						switch ((ResourceCraftingPropertyEnum)Enum.Parse(typeof(ResourceCraftingPropertyEnum), craftingProperty.Key)) {
							case ResourceCraftingPropertyEnum.Objects:
								foreach (string objectSubGroupString in ((string)craftingProperty.Value).Split(';')) {
									ObjectPrefabSubGroup.ObjectSubGroupEnum objectSubGroupEnum = (ObjectPrefabSubGroup.ObjectSubGroupEnum)Enum.Parse(typeof(ObjectPrefabSubGroup.ObjectSubGroupEnum), objectSubGroupString.Split(':')[0]);
									craftingObjects.Add(
										objectSubGroupEnum,
										null
									);
									if (objectSubGroupString.Split(':').Count() > 1) {
										craftingObjects[objectSubGroupEnum] = new List<ObjectPrefab.ObjectEnum>();
										foreach (string objectString in objectSubGroupString.Split(':')[1].Split(',')) {
											craftingObjects[objectSubGroupEnum].Add((ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), objectString));
										}
									}
								}
								break;
							case ResourceCraftingPropertyEnum.CraftingEnergy:
								craftingEnergy = int.Parse((string)craftingProperty.Value);
								break;
							case ResourceCraftingPropertyEnum.CraftingTime:
								craftingTime = int.Parse((string)craftingProperty.Value);
								break;
							case ResourceCraftingPropertyEnum.Resources:
								craftingResourcesTemp.Add(type.Value, new List<(EResource resource, float amount)>());
								foreach (string resourceAmountString in ((string)craftingProperty.Value).Split(',')) {
									float amount = float.Parse(resourceAmountString.Split(':')[1]);
									craftingResourcesTemp[type.Value]
										.Add(((EResource)Enum.Parse(typeof(EResource), resourceAmountString.Split(':')[0]), amount));
								}
								break;
							default:
								Debug.LogError("Unknown resource crafting property: " + craftingProperty.Key + " " + craftingProperty.Value);
								break;
						}
					}
					break;
				case ResourcePropertyEnum.Clothing:
					foreach (KeyValuePair<string, object> clothingProperty in (List<KeyValuePair<string, object>>)resourceSubProperty.Value) {
						switch ((ResourceClothingPropertyEnum)Enum.Parse(typeof(ResourceClothingPropertyEnum), clothingProperty.Key)) {
							case ResourceClothingPropertyEnum.Appearance:
								clothingAppearance = (HumanManager.Human.Appearance)Enum.Parse(typeof(HumanManager.Human.Appearance), (string)clothingProperty.Value);
								break;
							case ResourceClothingPropertyEnum.ClothingType:
								clothingType = (Clothing.ClothingEnum)Enum.Parse(typeof(Clothing.ClothingEnum), (string)clothingProperty.Value);
								break;
							case ResourceClothingPropertyEnum.Insulation:
								clothingInsulation = int.Parse((string)clothingProperty.Value);
								break;
							case ResourceClothingPropertyEnum.WaterResistance:
								clothingWaterResistance = int.Parse((string)clothingProperty.Value);
								break;
							case ResourceClothingPropertyEnum.WeightCapacity:
								clothingWeightCapacity = int.Parse((string)clothingProperty.Value);
								break;
							case ResourceClothingPropertyEnum.VolumeCapacity:
								clothingVolumeCapacity = int.Parse((string)clothingProperty.Value);
								break;
							case ResourceClothingPropertyEnum.Colours:
								foreach (string colourString in ((string)clothingProperty.Value).Split(',')) {
									clothingColours.Add(StringUtilities.RemoveNonAlphanumericChars(colourString));
								}
								break;
							default:
								Debug.LogError("Unknown resource clothing property: " + clothingProperty.Key + " " + clothingProperty.Value);
								break;
						}
					}
					break;
				default:
					Debug.LogError("Unknown resource sub property: " + resourceSubProperty.Key + " " + resourceSubProperty.Value);
					break;
			}
		}

		if (classes.Contains(Resource.ResourceClassEnum.Food)) {
			return new Food(
				type.Value,
				groupType,
				classes,
				weight.Value,
				volume.Value,
				price.Value,
				fuelEnergy.Value,
				craftingObjects,
				craftingEnergy.Value,
				craftingTime.Value,
				foodNutrition.Value
			);
		}
		if (classes.Contains(Resource.ResourceClassEnum.Clothing)) {
			ClothingPrefab clothingPrefab = new(
				clothingAppearance.Value,
				clothingType.Value,
				clothingInsulation.Value,
				clothingWaterResistance.Value,
				clothingWeightCapacity.Value,
				clothingVolumeCapacity.Value,
				clothingColours,
				ClothingPrefab.clothingPrefabs.Where(c => c.appearance == clothingAppearance.Value).Sum(c => c.colours.Count)
			);
			ClothingPrefab.clothingPrefabs.Add(clothingPrefab);
			return new Clothing( // TODO This will only return a colourless clothing resource, need to move Clothing to a ResourceInstance system for colours
				(EResource)Enum.Parse(typeof(EResource), clothingType.Value.ToString()),
				groupType,
				classes,
				weight.Value,
				volume.Value,
				price.Value,
				fuelEnergy.Value,
				craftingObjects,
				craftingEnergy.Value,
				craftingTime.Value,
				clothingPrefab,
				string.Empty // TODO Fix this too
			);
		}
		Resource resource = new(
			type.Value,
			groupType,
			classes,
			weight.Value,
			volume.Value,
			price.Value,
			fuelEnergy.Value,
			craftingObjects,
			craftingEnergy.Value,
			craftingTime.Value
		);
		return resource;
	}

	public Job CreateResource(CraftableResourceInstance resource, CraftingObject craftingObject) {
		Job job = new(
			JobPrefab.GetJobPrefabByName("CreateResource"),
			craftingObject.tile,
			ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.CreateResource),
			null,
			0
		);
		job.SetCreateResourceData(resource);
		GameManager.Get<JobManager>().CreateJob(job);
		return job;
	}

	public void CalculateResourceTotals() {
		foreach (Resource resource in Resource.GetResources()) {
			resource.SetWorldTotalAmount(0);
			resource.SetColonistsTotalAmount(0);
			resource.SetContainerTotalAmount(0);
			resource.SetUnreservedContainerTotalAmount(0);
			resource.SetUnreservedTradingPostTotalAmount(0);
			resource.SetAvailableAmount(0);
		}

		foreach (Colonist colonist in Colonist.colonists) {
			foreach (ResourceAmount resourceAmount in colonist.GetInventory().resources) {
				resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
				resourceAmount.Resource.AddToColonistsTotalAmount(resourceAmount.Amount);
			}
			foreach (ReservedResources reservedResources in colonist.GetInventory().reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
					resourceAmount.Resource.AddToColonistsTotalAmount(resourceAmount.Amount);
				}
			}
		}
		foreach (Container container in Container.containers) {
			foreach (ResourceAmount resourceAmount in container.GetInventory().resources) {
				resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
				resourceAmount.Resource.AddToContainerTotalAmount(resourceAmount.Amount);
				resourceAmount.Resource.AddToUnreservedContainerTotalAmount(resourceAmount.Amount);
			}
			foreach (ReservedResources reservedResources in container.GetInventory().reservedResources) {
				foreach (ResourceAmount resourceAmount in reservedResources.resources) {
					resourceAmount.Resource.AddToWorldTotalAmount(resourceAmount.Amount);
					resourceAmount.Resource.AddToContainerTotalAmount(resourceAmount.Amount);
				}
			}
		}
		foreach (TradingPost tradingPost in TradingPost.tradingPosts) {
			foreach (ResourceAmount resourceAmount in tradingPost.GetInventory().resources) {
				resourceAmount.Resource.AddToUnreservedTradingPostTotalAmount(resourceAmount.Amount);
			}
		}

		foreach (Resource resource in Resource.GetResources()) {
			resource.CalculateAvailableAmount();
		}

		OnResourceTotalsUpdated?.Invoke();
	}

	public List<ResourceAmount> GetFilteredResources(bool colonistInventory, bool colonistReserved, bool containerInventory, bool containerReserved) {
		List<ResourceAmount> returnResources = new();
		if (colonistInventory || colonistReserved) {
			foreach (Colonist colonist in Colonist.colonists) {
				if (colonistInventory) {
					foreach (ResourceAmount resourceAmount in colonist.GetInventory().resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
						} else {
							existingResourceAmount.Amount += resourceAmount.Amount;
						}
					}
				}
				if (colonistReserved) {
					foreach (ReservedResources reservedResources in colonist.GetInventory().reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
							} else {
								existingResourceAmount.Amount += resourceAmount.Amount;
							}
						}
					}
				}
			}
		}
		if (containerInventory || containerReserved) {
			foreach (Container container in Container.containers) {
				if (containerInventory) {
					foreach (ResourceAmount resourceAmount in container.GetInventory().resources) {
						ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
						if (existingResourceAmount == null) {
							returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
						} else {
							existingResourceAmount.Amount += resourceAmount.Amount;
						}
					}
				}
				if (containerReserved) {
					foreach (ReservedResources reservedResources in container.GetInventory().reservedResources) {
						foreach (ResourceAmount resourceAmount in reservedResources.resources) {
							ResourceAmount existingResourceAmount = returnResources.Find(ra => ra.Resource == resourceAmount.Resource);
							if (existingResourceAmount == null) {
								returnResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
							} else {
								existingResourceAmount.Amount += resourceAmount.Amount;
							}
						}
					}
				}
			}
		}
		return returnResources;
	}

	private enum JobPrefabGroupPropertyEnum
	{
		Group,
		Name,
		Returnable,
		JobPrefabs
	}

	private enum JobPrefabPropertyEnum
	{
		JobPrefab,
		Name,
		Returnable
	}

	private void CreateJobPrefabs() {
		List<KeyValuePair<string, object>> jobPrefabGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/job-prefabs").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> jobPrefabGroupProperty in jobPrefabGroupProperties) {
			switch ((JobPrefabGroupPropertyEnum)Enum.Parse(typeof(JobPrefabGroupPropertyEnum), jobPrefabGroupProperty.Key)) {
				case JobPrefabGroupPropertyEnum.Group:

					JobPrefabGroup jobPrefabGroup = ParseJobPrefabGroup(jobPrefabGroupProperty);
					JobPrefabGroup.jobPrefabGroups.Add(jobPrefabGroup.name, jobPrefabGroup);

					break;
				default:
					Debug.LogError($"Unknown job prefab group property: {jobPrefabGroupProperty.Key} {jobPrefabGroupProperty.Value}");
					break;
			}
		}
	}

	private JobPrefabGroup ParseJobPrefabGroup(KeyValuePair<string, object> jobPrefabGroupProperty) {

		string jobPrefabGroupName = null;
		bool? returnableAsGroup = null;
		List<JobPrefab> jobPrefabs = new();

		foreach (KeyValuePair<string, object> jobPrefabGroupSubProperty in (List<KeyValuePair<string, object>>)jobPrefabGroupProperty.Value) {
			switch ((JobPrefabGroupPropertyEnum)Enum.Parse(typeof(JobPrefabGroupPropertyEnum), jobPrefabGroupSubProperty.Key)) {
				case JobPrefabGroupPropertyEnum.Name:
					jobPrefabGroupName = (string)jobPrefabGroupSubProperty.Value;
					break;
				case JobPrefabGroupPropertyEnum.Returnable:
					returnableAsGroup = bool.Parse((string)jobPrefabGroupSubProperty.Value);
					break;
				case JobPrefabGroupPropertyEnum.JobPrefabs:
					foreach (KeyValuePair<string, object> jobPrefabProperty in (List<KeyValuePair<string, object>>)jobPrefabGroupSubProperty.Value) {
						switch ((JobPrefabPropertyEnum)Enum.Parse(typeof(JobPrefabPropertyEnum), jobPrefabProperty.Key)) {
							case JobPrefabPropertyEnum.JobPrefab:

								System.Diagnostics.Debug.Assert(returnableAsGroup != null, nameof(returnableAsGroup) + " != null");

								JobPrefab jobPrefab = ParseJobPrefab(returnableAsGroup.Value, jobPrefabProperty);
								jobPrefabs.Add(jobPrefab);
								JobPrefab.jobPrefabs.Add(jobPrefab.name, jobPrefab);

								break;
							default:
								Debug.LogError($"Unknown job prefab property: {jobPrefabProperty.Key} {jobPrefabProperty.Value}");
								break;
						}
					}
					break;
				default:
					Debug.LogError($"Unknown job prefab group sub property: {jobPrefabGroupSubProperty.Key} {jobPrefabGroupSubProperty.Value}");
					break;
			}
		}

		return new JobPrefabGroup(
			jobPrefabGroupName,
			jobPrefabs
		);
	}

	private JobPrefab ParseJobPrefab(bool returnableAsGroup, KeyValuePair<string, object> jobPrefabProperty) {
		string jobPrefabName = null;
		bool? returnableIndividually = returnableAsGroup;

		foreach (KeyValuePair<string, object> jobPrefabSubProperty in (List<KeyValuePair<string, object>>)jobPrefabProperty.Value) {
			switch ((JobPrefabPropertyEnum)Enum.Parse(typeof(JobPrefabPropertyEnum), jobPrefabSubProperty.Key)) {
				case JobPrefabPropertyEnum.Name:
					jobPrefabName = (string)jobPrefabSubProperty.Value;
					break;
				case JobPrefabPropertyEnum.Returnable:
					returnableIndividually = bool.Parse((string)jobPrefabSubProperty.Value);
					break;
				default:
					Debug.LogError($"Unknown job prefab sub property: {jobPrefabSubProperty.Key} {jobPrefabSubProperty.Value}");
					break;
			}
		}

		return new JobPrefab(
			jobPrefabName,
			returnableIndividually.Value
		);
	}

	private enum ObjectGroupPropertyEnum
	{
		Group,
		Type,
		SubGroups
	}

	private enum ObjectSubGroupPropertyEnum
	{
		SubGroup,
		Type,
		Objects
	}

	private enum ObjectPropertyEnum
	{
		Object,
		Type,
		InstanceType,
		Layer,
		Bitmasking,
		BlocksLight,
		MultiTilePositions,
		Integrity,
		Walkable,
		WalkSpeed,
		Buildable,
		Flammability,
		MaxInventoryVolume,
		MaxInventoryWeight,
		MaxLightDistance,
		LightColour,
		RestComfortAmount,
		UsesFuel,
		GrowthTimeDays,
		Seed,
		HarvestResources,
		TimeToBuild,
		CommonResources,
		Variations,
		VariationNameOrder,
		SelectionModifiers,
		JobType,
		AddToTileWhenBuilt
	}

	private enum VariationPropertyEnum
	{
		Variation,
		Name,
		UniqueResources,
		WalkSpeed,
		Integrity,
		Flammability,
		TimeToBuild,
		Plants
	}

	private void CreateObjectPrefabs() {
		List<KeyValuePair<string, object>> objectGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/objects").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> objectGroupProperty in objectGroupProperties) {
			switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupProperty.Key)) {
				case ObjectGroupPropertyEnum.Group:

					ObjectPrefabGroup objectPrefabGroup = ParseObjectPrefabGroup(objectGroupProperty);
					ObjectPrefabGroup.objectPrefabGroups.Add(objectPrefabGroup.type, objectPrefabGroup);

					break;
				default:
					Debug.LogError("Unknown object group property: " + objectGroupProperty.Key + " " + objectGroupProperty.Value);
					break;
			}
		}
	}

	private ObjectPrefabGroup ParseObjectPrefabGroup(KeyValuePair<string, object> objectGroupProperty) {

		ObjectPrefabGroup.ObjectGroupEnum? groupType = null;
		List<ObjectPrefabSubGroup> subGroups = new();

		foreach (KeyValuePair<string, object> objectGroupSubProperty in (List<KeyValuePair<string, object>>)objectGroupProperty.Value) {
			switch ((ObjectGroupPropertyEnum)Enum.Parse(typeof(ObjectGroupPropertyEnum), objectGroupSubProperty.Key)) {
				case ObjectGroupPropertyEnum.Type:
					groupType = (ObjectPrefabGroup.ObjectGroupEnum)Enum.Parse(typeof(ObjectPrefabGroup.ObjectGroupEnum), (string)objectGroupSubProperty.Value);
					break;
				case ObjectGroupPropertyEnum.SubGroups:
					foreach (KeyValuePair<string, object> objectSubGroupProperty in (List<KeyValuePair<string, object>>)objectGroupSubProperty.Value) {
						switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupProperty.Key)) {
							case ObjectSubGroupPropertyEnum.SubGroup:

								ObjectPrefabSubGroup objectPrefabSubGroup = ParseObjectPrefabSubGroup(groupType, objectSubGroupProperty);
								subGroups.Add(objectPrefabSubGroup);
								ObjectPrefabSubGroup.objectPrefabSubGroups.Add(objectPrefabSubGroup.type, objectPrefabSubGroup);

								break;
							default:
								Debug.LogError("Unknown object sub group property: " + objectSubGroupProperty.Key + " " + objectSubGroupProperty.Value);
								break;
						}
					}
					break;
				default:
					Debug.LogError("Unknown object group sub property: " + objectGroupSubProperty.Key + " " + objectGroupSubProperty.Value);
					break;
			}
		}

		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

		return new ObjectPrefabGroup(
			groupType.Value,
			subGroups
		);
	}

	private ObjectPrefabSubGroup ParseObjectPrefabSubGroup(ObjectPrefabGroup.ObjectGroupEnum? groupType, KeyValuePair<string, object> objectSubGroupProperty) {

		ObjectPrefabSubGroup.ObjectSubGroupEnum? subGroupType = null;
		List<ObjectPrefab> prefabs = new();

		foreach (KeyValuePair<string, object> objectSubGroupSubProperty in (List<KeyValuePair<string, object>>)objectSubGroupProperty.Value) {
			switch ((ObjectSubGroupPropertyEnum)Enum.Parse(typeof(ObjectSubGroupPropertyEnum), objectSubGroupSubProperty.Key)) {
				case ObjectSubGroupPropertyEnum.Type:
					subGroupType = (ObjectPrefabSubGroup.ObjectSubGroupEnum)Enum.Parse(typeof(ObjectPrefabSubGroup.ObjectSubGroupEnum), (string)objectSubGroupSubProperty.Value);
					break;
				case ObjectSubGroupPropertyEnum.Objects:
					foreach (KeyValuePair<string, object> objectProperty in (List<KeyValuePair<string, object>>)objectSubGroupSubProperty.Value) {
						switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectProperty.Key)) {
							case ObjectPropertyEnum.Object:

								ObjectPrefab objectPrefab = ParseObjectPrefab(groupType, subGroupType, objectProperty);
								prefabs.Add(objectPrefab);
								ObjectPrefab.objectPrefabs.Add(objectPrefab.type, objectPrefab);

								break;
							default:
								Debug.LogError("Unknown object property: " + objectProperty.Key + " " + objectProperty.Value);
								break;
						}
					}
					break;
				default:
					Debug.LogError("Unknown object sub group sub property: " + objectSubGroupSubProperty.Key + " " + objectSubGroupSubProperty.Value);
					break;
			}
		}

		System.Diagnostics.Debug.Assert(subGroupType != null, nameof(subGroupType) + " != null");
		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");

		return new ObjectPrefabSubGroup(
			subGroupType.Value,
			groupType.Value,
			prefabs
		);
	}

	private ObjectPrefab ParseObjectPrefab(
		ObjectPrefabGroup.ObjectGroupEnum? groupType,
		ObjectPrefabSubGroup.ObjectSubGroupEnum? subGroupType,
		KeyValuePair<string, object> objectProperty
	) {
		ObjectPrefab.ObjectEnum? type = null;
		ObjectInstance.ObjectInstanceType instanceType = ObjectInstance.ObjectInstanceType.Normal;
		int? layer = null;
		bool? bitmasking = false;
		bool? blocksLight = false;
		int? integrity = 0;
		List<Vector2> multiTilePositions = new();

		float? walkSpeed = 1;
		bool? walkable = true;
		bool? buildable = true;
		float? flammability = 0;

		// Container
		int? maxInventoryVolume = null;
		int? maxInventoryWeight = null;

		// Light
		int? maxLightDistance = null;
		Color? lightColour = null;

		// Sleep Spot
		float? restComfortAmount = null;

		// Crafting Object
		bool? usesFuel = null;

		// Farm
		int? growthTimeDays = null;
		Resource seedResource = null;
		List<ResourceRange> harvestResources = new();

		// Job
		int? timeToBuild = null;
		List<ResourceAmount> commonResources = new();
		List<Variation> variations = new();
		Variation.VariationNameOrderEnum variationNameOrder = Variation.VariationNameOrderEnum.VariationObject;
		List<JobManager.SelectionModifiersEnum> selectionModifiers = new();
		string jobType = null;
		bool? addToTileWhenBuilt = true;

		foreach (KeyValuePair<string, object> objectSubProperty in (List<KeyValuePair<string, object>>)objectProperty.Value) {
			switch ((ObjectPropertyEnum)Enum.Parse(typeof(ObjectPropertyEnum), objectSubProperty.Key)) {
				case ObjectPropertyEnum.Type:
					type = (ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), (string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.InstanceType:
					instanceType = (ObjectInstance.ObjectInstanceType)Enum.Parse(typeof(ObjectInstance.ObjectInstanceType), (string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Layer:
					layer = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Bitmasking:
					bitmasking = bool.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.BlocksLight:
					blocksLight = bool.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Integrity:
					integrity = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.MultiTilePositions:
					foreach (string vector2String in ((string)objectSubProperty.Value).Split(';')) {
						multiTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
					}
					break;
				case ObjectPropertyEnum.WalkSpeed:
					walkSpeed = float.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Walkable:
					walkable = bool.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Buildable:
					buildable = bool.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Flammability:
					flammability = float.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.MaxInventoryVolume:
					maxInventoryVolume = ((string)objectSubProperty.Value).Contains("Max") ? int.MaxValue : int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.MaxInventoryWeight:
					maxInventoryWeight = ((string)objectSubProperty.Value).Contains("Max") ? int.MaxValue : int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.MaxLightDistance:
					maxLightDistance = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.LightColour:
					lightColour = ColourUtilities.HexToColor((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.RestComfortAmount:
					restComfortAmount = float.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.UsesFuel:
					usesFuel = bool.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.GrowthTimeDays:
					growthTimeDays = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.Seed:
					seedResource = Resource.GetResourceByString((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.HarvestResources:
					foreach (string resourceRangeString in ((string)objectSubProperty.Value).Split(',')) {
						Resource resource = Resource.GetResourceByString(resourceRangeString.Split(':')[0]);
						int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
						int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
						harvestResources.Add(new ResourceRange(resource, min, max));
					}
					break;
				case ObjectPropertyEnum.TimeToBuild:
					timeToBuild = int.Parse((string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.CommonResources:
					foreach (string resourceAmountString in ((string)objectSubProperty.Value).Split(',')) {
						commonResources.Add(new ResourceAmount(Resource.GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
					}
					break;
				case ObjectPropertyEnum.Variations:
					foreach (KeyValuePair<string, object> variationsProperty in (List<KeyValuePair<string, object>>)objectSubProperty.Value) {
						switch ((VariationPropertyEnum)Enum.Parse(typeof(VariationPropertyEnum), variationsProperty.Key)) {
							case VariationPropertyEnum.Variation:
								Variation variation = ParseVariation((walkSpeed.Value, integrity.Value, flammability.Value, timeToBuild.Value), variationsProperty);
								variations.Add(variation);
								break;
						}
					}
					break;
				case ObjectPropertyEnum.VariationNameOrder:
					variationNameOrder = (Variation.VariationNameOrderEnum)Enum.Parse(typeof(Variation.VariationNameOrderEnum), (string)objectSubProperty.Value);
					break;
				case ObjectPropertyEnum.SelectionModifiers:
					foreach (string selectionModifierString in ((string)objectSubProperty.Value).Split(',')) {
						selectionModifiers.Add((JobManager.SelectionModifiersEnum)Enum.Parse(typeof(JobManager.SelectionModifiersEnum), selectionModifierString));
					}
					break;
				case ObjectPropertyEnum.JobType:
					jobType = (string)objectSubProperty.Value;
					break;
				case ObjectPropertyEnum.AddToTileWhenBuilt:
					addToTileWhenBuilt = bool.Parse((string)objectSubProperty.Value);
					break;
				default:
					Debug.LogError("Unknown object sub property: " + objectSubProperty.Key + " " + objectSubProperty.Value);
					break;
			}
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.Container && instanceType != ObjectInstance.ObjectInstanceType.TradingPost) {
			maxInventoryVolume ??= 0;
			maxInventoryWeight ??= 0;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.LightSource) {
			maxLightDistance ??= 0;
			lightColour ??= Color.white;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.SleepSpot) {
			restComfortAmount ??= 0;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.CraftingObject) {
			usesFuel ??= false;
		}

		if (instanceType != ObjectInstance.ObjectInstanceType.Farm) {
			growthTimeDays ??= 0;
		}

		ObjectPrefab objectPrefab = new(
			type.Value,
			groupType.Value,
			subGroupType.Value,
			instanceType,
			layer.Value,
			bitmasking.Value,
			blocksLight.Value,
			integrity.Value,
			multiTilePositions,
			walkSpeed.Value,
			walkable.Value,
			buildable.Value,
			flammability.Value,
			maxInventoryVolume.Value,
			maxInventoryWeight.Value,
			maxLightDistance.Value,
			lightColour.Value,
			restComfortAmount.Value,
			usesFuel.Value,
			growthTimeDays.Value,
			seedResource,
			harvestResources,
			timeToBuild.Value,
			commonResources,
			variations,
			variationNameOrder,
			selectionModifiers,
			jobType,
			addToTileWhenBuilt.Value
		);
		foreach (Variation variation in objectPrefab.variations) {
			variation.prefab = objectPrefab;
		}
		return objectPrefab;
	}

	private Variation ParseVariation((float walkSpeed, int integrity, float flammability, int timeToBuild) parentProperties, KeyValuePair<string, object> variationsProperty) {

		string variationName = null;
		List<ResourceAmount> variationUniqueResources = new();
		float? variationWalkSpeed = null;
		int? variationIntegrity = null;
		float? variationFlammability = null;
		int? variationTimeToBuild = null;

		// Plants
		Dictionary<PlantPrefab, Resource> variationPlants = new();

		foreach (KeyValuePair<string, object> variationProperty in (List<KeyValuePair<string, object>>)variationsProperty.Value) {
			switch ((VariationPropertyEnum)Enum.Parse(typeof(VariationPropertyEnum), variationProperty.Key)) {
				case VariationPropertyEnum.Name:
					variationName = StringUtilities.RemoveNonAlphanumericChars((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.UniqueResources:
					foreach (string resourceAmountString in ((string)variationProperty.Value).Split(',')) {
						variationUniqueResources.Add(new ResourceAmount(Resource.GetResourceByString(resourceAmountString.Split(':')[0]), int.Parse(resourceAmountString.Split(':')[1])));
					}
					break;
				case VariationPropertyEnum.WalkSpeed:
					variationWalkSpeed = float.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.Integrity:
					variationIntegrity = int.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.Flammability:
					variationFlammability = float.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.TimeToBuild:
					variationTimeToBuild = int.Parse((string)variationProperty.Value);
					break;
				case VariationPropertyEnum.Plants:
					foreach (string plantToHarvestResourceString in ((string)variationProperty.Value).Split(',')) {
						string harvestResourceString = plantToHarvestResourceString.Split(':')[1];
						variationPlants.Add(
							PlantPrefab.GetPlantPrefabByString(plantToHarvestResourceString.Split(':')[0]),
							harvestResourceString.Contains("None") ? null : Resource.GetResourceByString(harvestResourceString)
						);
					}
					break;
				default:
					Debug.LogError("Unknown variation property: " + variationProperty.Key + " " + variationProperty.Value);
					break;
			}
		}

		return new Variation(
			null, // Set after objectPrefab is created
			variationName,
			variationUniqueResources,
			variationWalkSpeed ?? parentProperties.walkSpeed,
			variationIntegrity ?? parentProperties.integrity,
			variationFlammability ?? parentProperties.flammability,
			variationTimeToBuild ?? parentProperties.timeToBuild,
			variationPlants
		);
	}

	private enum PlantGroupPropertyEnum
	{
		PlantGroup,
		Type,
		Plants
	}

	private enum PlantPropertyEnum
	{
		Plant,
		Type,
		Living,
		Integrity,
		Seed,
		//PlantJob, // TODO Use this?
		ReturnResources,
		HarvestResources
	}

	private void CreatePlantPrefabs() {
		List<KeyValuePair<string, object>> plantGroupProperties = PersistenceUtilities.GetKeyValuePairsFromLines(Resources.Load<TextAsset>(@"Data/plants").text.Split('\n').ToList());
		foreach (KeyValuePair<string, object> plantGroupProperty in plantGroupProperties) {
			switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupProperty.Key)) {
				case PlantGroupPropertyEnum.PlantGroup:

					PlantGroup plantGroup = ParsePlantGroup(plantGroupProperty);
					PlantGroup.plantGroups.Add(plantGroup.type, plantGroup);

					break;
				default:
					Debug.LogError("Unknown plant group property: " + plantGroupProperty.Key + " " + plantGroupProperty.Value);
					break;

			}
		}
	}

	private PlantGroup ParsePlantGroup(KeyValuePair<string, object> plantGroupProperty) {

		PlantGroup.PlantGroupEnum? groupType = null;
		List<PlantPrefab> prefabs = new();

		foreach (KeyValuePair<string, object> plantGroupSubProperty in (List<KeyValuePair<string, object>>)plantGroupProperty.Value) {
			switch ((PlantGroupPropertyEnum)Enum.Parse(typeof(PlantGroupPropertyEnum), plantGroupSubProperty.Key)) {
				case PlantGroupPropertyEnum.Type:
					groupType = (PlantGroup.PlantGroupEnum)Enum.Parse(typeof(PlantGroup.PlantGroupEnum), (string)plantGroupSubProperty.Value);
					break;
				case PlantGroupPropertyEnum.Plants:
					foreach (KeyValuePair<string, object> plantProperty in (List<KeyValuePair<string, object>>)plantGroupSubProperty.Value) {
						switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantProperty.Key)) {
							case PlantPropertyEnum.Plant:

								PlantPrefab plantPrefab = ParsePlantPrefab(groupType, plantProperty);
								prefabs.Add(plantPrefab);
								PlantPrefab.plantPrefabs.Add(plantPrefab.type, plantPrefab);

								break;
							default:
								Debug.LogError("Unknown plant property: " + plantProperty.Key + " " + plantProperty.Value);
								break;
						}
					}
					break;
				default:
					Debug.LogError("Unknown plant group sub property: " + plantGroupSubProperty.Key + " " + plantGroupSubProperty.Value);
					break;
			}

		}

		return new PlantGroup(
			groupType.GetValueOrDefault(),
			prefabs
		);
	}

	private PlantPrefab ParsePlantPrefab(PlantGroup.PlantGroupEnum? groupType, KeyValuePair<string, object> plantProperty) {

		Plant.PlantEnum? type = null;
		bool? living = null;
		int? integrity = null;
		Resource seed = null;
		List<ResourceRange> returnResources = new();
		List<ResourceRange> harvestResources = new();

		foreach (KeyValuePair<string, object> plantSubProperty in (List<KeyValuePair<string, object>>)plantProperty.Value) {
			switch ((PlantPropertyEnum)Enum.Parse(typeof(PlantPropertyEnum), plantSubProperty.Key)) {
				case PlantPropertyEnum.Type:
					type = (Plant.PlantEnum)Enum.Parse(typeof(Plant.PlantEnum), (string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.Living:
					living = bool.Parse((string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.Integrity:
					integrity = int.Parse((string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.Seed:
					seed = Resource.GetResourceByString((string)plantSubProperty.Value);
					break;
				case PlantPropertyEnum.ReturnResources:
					foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
						Resource resource = Resource.GetResourceByString(resourceRangeString.Split(':')[0]);
						int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
						int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
						returnResources.Add(new ResourceRange(resource, min, max));
					}
					break;
				case PlantPropertyEnum.HarvestResources:
					foreach (string resourceRangeString in ((string)plantSubProperty.Value).Split(',')) {
						Resource resource = Resource.GetResourceByString(resourceRangeString.Split(':')[0]);
						int min = int.Parse(resourceRangeString.Split(':')[1].Split('-')[0]);
						int max = int.Parse(resourceRangeString.Split(':')[1].Split('-')[1]);
						harvestResources.Add(new ResourceRange(resource, min, max));
					}
					break;
				default:
					Debug.LogError("Unknown plant sub property: " + plantSubProperty.Key + " " + plantSubProperty.Value);
					break;
			}
		}

		System.Diagnostics.Debug.Assert(type != null, nameof(type) + " != null");
		System.Diagnostics.Debug.Assert(groupType != null, nameof(groupType) + " != null");
		System.Diagnostics.Debug.Assert(living != null, nameof(living) + " != null");
		System.Diagnostics.Debug.Assert(integrity != null, nameof(integrity) + " != null");

		return new PlantPrefab(
			type.Value,
			groupType.Value,
			living.Value,
			integrity.Value,
			seed,
			returnResources,
			harvestResources
		);
	}

	private void GrowPlants() {
		List<Plant> growPlants = new();
		foreach (Plant plant in Plant.smallPlants) {
			plant.growthProgress += 1;
			if (plant.growthProgress > SimulationDateTime.DayLengthSeconds * 4) {
				if (Random.Range(0, 100) < 0.01f * (plant.growthProgress / SimulationDateTime.DayLengthSeconds * 4)) {
					growPlants.Add(plant);
				}
			}
		}
		foreach (Plant plant in growPlants) {
			plant.Grow();
		}
	}

	private void LoadLocationNames() {
		locationNames.AddRange(Resources.Load<TextAsset>(@"Data/names-locations").text.Split('\n').Select(StringUtilities.RemoveNonAlphanumericChars).ToList());
	}

	public string GetRandomLocationName() {
		List<string> filteredLocationNames = locationNames.Where(
				ln =>
					(GameManager.Get<UniverseManager>().universe == null || ln != GameManager.Get<UniverseManager>().universe.name)
					&& (GameManager.Get<PlanetManager>().planet == null || ln != GameManager.Get<PlanetManager>().planet.name)
					&& (GameManager.Get<ColonyManager>().colony == null || ln != GameManager.Get<ColonyManager>().colony.name)
					&& GameManager.Get<CaravanManager>().caravans.Find(c => c.location.name == ln) == null
			)
			.ToList();

		return filteredLocationNames[Random.Range(0, filteredLocationNames.Count)];
	}

	private int BitSumObjects(List<ObjectPrefab.ObjectEnum> compareObjectTypes, List<TileManager.Tile> tileSurroundingTiles) {
		List<int> layers = new();
		foreach (TileManager.Tile tile in tileSurroundingTiles) {
			if (tile != null) {
				foreach (KeyValuePair<int, ObjectInstance> kvp in tile.objectInstances) {
					if (!layers.Contains(kvp.Key)) {
						layers.Add(kvp.Key);
					}
				}
			}
		}
		layers.Sort();

		Dictionary<int, List<int>> layersSumTiles = new();
		foreach (int layer in layers) {
			List<int> layerSumTiles = new() { 0, 0, 0, 0, 0, 0, 0, 0 };
			for (int i = 0; i < tileSurroundingTiles.Count; i++) {
				if (tileSurroundingTiles[i] != null && tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer) != null) {
					if (compareObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type)) {
						bool ignoreTile = false;
						if (compareObjectTypes.Contains(tileSurroundingTiles[i].GetObjectInstanceAtLayer(layer).prefab.type) && TileManager.Map.diagonalCheckMap.ContainsKey(i)) {
							List<TileManager.Tile> surroundingHorizontalTiles = new() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]], tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
							List<TileManager.Tile> similarTiles = surroundingHorizontalTiles.Where(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)).ToList();
							if (similarTiles.Count < 2) {
								ignoreTile = true;
							}
						}
						if (!ignoreTile) {
							layerSumTiles[i] = 1;
						}
					}
				} else {
					if (tileSurroundingTiles.Find(tile => tile != null && tileSurroundingTiles.IndexOf(tile) <= 3 && tile.GetObjectInstanceAtLayer(layer) != null && !compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
						layerSumTiles[i] = 1;
					} else {
						if (i <= 3) {
							layerSumTiles[i] = 1;
						} else {
							List<TileManager.Tile> surroundingHorizontalTiles = new() { tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][0]], tileSurroundingTiles[TileManager.Map.diagonalCheckMap[i][1]] };
							if (surroundingHorizontalTiles.Find(tile => tile != null && tile.GetObjectInstanceAtLayer(layer) != null && !compareObjectTypes.Contains(tile.GetObjectInstanceAtLayer(layer).prefab.type)) == null) {
								layerSumTiles[i] = 1;
							}
						}
					}
				}
			}
			layersSumTiles.Add(layer, layerSumTiles);
		}

		List<bool> sumTiles = new() { false, false, false, false, false, false, false, false };

		foreach (KeyValuePair<int, List<int>> layerSumTiles in layersSumTiles) {
			foreach (ObjectPrefab.ObjectEnum objectEnum in compareObjectTypes) {
				ObjectPrefab objectPrefab = ObjectPrefab.GetObjectPrefabByEnum(objectEnum);
				if (objectPrefab.layer == layerSumTiles.Key) {
					foreach (TileManager.Tile tile in tileSurroundingTiles) {
						if (tile != null) {
							ObjectInstance objectInstance = tile.GetAllObjectInstances().Find(instances => instances.prefab == objectPrefab);
							if (objectInstance != null) {
								if (layerSumTiles.Value[tileSurroundingTiles.IndexOf(tile)] > 0) {
									sumTiles[tileSurroundingTiles.IndexOf(tile)] = true;
								}
							}
						}
					}
				}
			}
		}

		int sum = 0;

		for (int i = 0; i < sumTiles.Count; i++) {
			if (sumTiles[i]) {
				sum += Mathf.RoundToInt(Mathf.Pow(2, i));
			}
		}

		return sum;
	}

	private void BitmaskObjects(
		ObjectInstance objectInstance,
		bool includeDiagonalSurroundingTiles,
		bool customBitSumInputs,
		bool compareEquivalentObjects,
		List<ObjectPrefab.ObjectEnum> customCompareObjectTypes
	) {
		List<TileManager.Tile> surroundingTilesToUse = includeDiagonalSurroundingTiles ? objectInstance.tile.surroundingTiles : objectInstance.tile.horizontalSurroundingTiles;

		int sum;
		if (customBitSumInputs) {
			sum = BitSumObjects(
				customCompareObjectTypes,
				surroundingTilesToUse
			);
		} else {
			if (compareEquivalentObjects) {
				if (objectInstance.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Walls) {
					sum = BitSumObjects(
						new List<ObjectPrefab.ObjectEnum> { objectInstance.prefab.type },
						surroundingTilesToUse
					);
					// TODO Not-fully-working implementation of walls and stone connecting
					//sum += GameManager.Get<ColonyManager>().colony.map.BitSum(
					//	TileManager.TileTypeGroup.GetTileTypeGroupByEnum(TileManager.TileTypeGroup.TypeEnum.Stone).tileTypes.Select(tileType => tileType.type).ToList(),
					//	new List<ObjectEnum>() { objectInstance.prefab.type },
					//	surroundingTilesToUse,
					//	true
					//);
				} else {
					sum = BitSumObjects(
						new List<ObjectPrefab.ObjectEnum> { objectInstance.prefab.type },
						surroundingTilesToUse
					);
				}
			} else {
				sum = BitSumObjects(
					new List<ObjectPrefab.ObjectEnum> { objectInstance.prefab.type },
					surroundingTilesToUse
				);
			}
		}

		SpriteRenderer spriteRenderer = objectInstance.obj.GetComponent<SpriteRenderer>();
		List<Sprite> sprites = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation);
		spriteRenderer.sprite = sum >= 16 ? sprites[TileManager.Map.bitmaskMap[sum]] : sprites[sum];
	}

	public void Bitmask(List<TileManager.Tile> tilesToBitmask) {
		foreach (TileManager.Tile tile in tilesToBitmask) {
			if (tile != null && tile.GetAllObjectInstances().Count > 0) {
				foreach (ObjectInstance objectInstance in tile.GetAllObjectInstances()) {
					if (objectInstance.prefab.bitmasking) {
						BitmaskObjects(
							objectInstance,
							true, // includeDiagonalSurroundingTiles -- default: true
							false, // customBitSumInput -- default: false
							true, // compareEquivalentObjects -- default: false
							null // customCompareObjectTypes -- default: null
						);
					} else {
						if (objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation).Count > 0) {
							if (objectInstance.prefab.jobType != "PlantFarm") {
								objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBitmaskSpritesForVariation(objectInstance.variation)[objectInstance.rotationIndex];
							}
						} else {
							objectInstance.obj.GetComponent<SpriteRenderer>().sprite = objectInstance.prefab.GetBaseSpriteForVariation(objectInstance.variation);
						}
					}
				}
			}
		}
	}
}