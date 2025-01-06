using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NUI;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob {

	public class Job {

		public static readonly List<Job> jobs = new List<Job>();

		public JobPrefab prefab;

		public TileManager.Tile tile;
		public ResourceManager.ObjectPrefab objectPrefab;
		public ResourceManager.Variation variation;
		public Colonist colonist;

		public int rotationIndex;

		public GameObject jobPreview;
		public GameObject priorityIndicator;

		public int priority;
		public bool started;
		public float jobProgress;
		public float colonistBuildTime;

		public List<ResourceManager.ResourceAmount> requiredResources = new List<ResourceManager.ResourceAmount>();

		public List<ResourceManager.ResourceAmount> resourcesColonistHas;
		public List<ContainerPickup> containerPickups;

		public UIManagerOld.JobElement jobUIElement;

		public ResourceManager.CraftableResourceInstance createResource;
		public ResourceManager.ObjectInstance activeObject;

		public List<ResourceManager.ResourceAmount> transferResources;

		public Job(JobPrefab prefab, TileManager.Tile tile, ResourceManager.ObjectPrefab objectPrefab, ResourceManager.Variation variation, int rotationIndex) {

			this.prefab = prefab;

			this.tile = tile;

			this.objectPrefab = objectPrefab;
			this.variation = variation;

			this.rotationIndex = rotationIndex;

			SetRequiredResources();

			jobPreview = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, GameManager.resourceM.jobParent.transform, false);
			jobPreview.transform.position = tile.obj.transform.position + (Vector3)objectPrefab.anchorPositionOffset[rotationIndex];
			jobPreview.name = "JobPreview: " + objectPrefab.name + " at " + jobPreview.transform.position;
			SpriteRenderer jPSR = jobPreview.GetComponent<SpriteRenderer>();
			if (objectPrefab.GetBaseSpriteForVariation(variation) != null) {
				jPSR.sprite = objectPrefab.GetBaseSpriteForVariation(variation);
			}
			if (!objectPrefab.bitmasking && objectPrefab.GetBitmaskSpritesForVariation(variation).Count > 0) {
				jPSR.sprite = objectPrefab.GetBitmaskSpritesForVariation(variation)[rotationIndex];
			}
			jPSR.sortingOrder = 5 + objectPrefab.layer; // Job Preview Sprite
			jPSR.color = ColourUtilities.GetColour(ColourUtilities.EColour.WhiteAlpha128);

			jobProgress = objectPrefab.timeToBuild;
			colonistBuildTime = objectPrefab.timeToBuild;
		}

		private void SetRequiredResources() {

			if (requiredResources.Count > 0) {
				Debug.LogError("Attempting to set requiredResources on Job when they are already set.");
			}

			requiredResources.AddRange(objectPrefab.commonResources);
			if (variation != null) {
				requiredResources.AddRange(variation.uniqueResources);
			}
		}

		public void SetCreateResourceData(ResourceManager.CraftableResourceInstance resource, bool addToResourcesToBuild = true) {
			createResource = resource;

			jobProgress += createResource.resource.craftingTime;
			colonistBuildTime += createResource.resource.craftingTime;

			if (addToResourcesToBuild) {
				requiredResources.AddRange(createResource.resource.craftingResources);
				if (resource.resource.craftingEnergy != 0) {
					requiredResources.AddRange(resource.fuelAmounts);
				}
			}
			activeObject = resource.craftingObject;
			jobPreview.GetComponent<SpriteRenderer>().sprite = resource.resource.image;
		}

		public void SetColonist(Colonist colonist) {
			this.colonist = colonist;
			if (prefab.name != "PickupResources" && containerPickups != null && containerPickups.Count > 0) {
				colonist.storedJob = this;
				colonist.SetJob(
					new ColonistJob(
						colonist,
						new Job(
							JobPrefab.GetJobPrefabByName("PickupResources"),
							containerPickups[0].container.tile,
							GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.PickupResources),
							null,
							0),
						null,
						null
					)
				);
			}
		}

		public void ChangePriority(int amount) {
			priority += amount;
			if (priorityIndicator == null && jobPreview != null) {
				priorityIndicator = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, jobPreview.transform, false);
				priorityIndicator.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(@"UI/priorityIndicator");
				priorityIndicator.GetComponent<SpriteRenderer>().sortingOrder = jobPreview.GetComponent<SpriteRenderer>().sortingOrder + 1; // Priority Indicator Sprite
				if (priority == 1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightYellow);
				} else if (priority == -1) {
					priorityIndicator.GetComponent<SpriteRenderer>().color = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed);
				}
			}
			if (priority == 0) {
				MonoBehaviour.Destroy(priorityIndicator);
			}
		}

		public void Remove() {
			if (jobUIElement != null) {
				jobUIElement.Remove();
			}
			MonoBehaviour.Destroy(jobPreview);
		}
	}
}
