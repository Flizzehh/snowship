using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Snowship.NResource
{
	public class Plant
	{
		public static List<Plant> smallPlants = new();

		public readonly string name;

		public readonly PlantPrefab prefab;
		public readonly TileManager.Tile tile;

		public readonly GameObject obj;
		public readonly SpriteRenderer sr;

		public float integrity;

		public bool small;

		public float growthProgress = 0;

		public readonly Resource harvestResource;

		public bool visible;

		public Plant(PlantPrefab prefab, TileManager.Tile tile, bool? small, bool randomHarvestResource, Resource specificHarvestResource) {
			this.prefab = prefab;
			this.tile = tile;

			integrity = prefab.integrity;

			this.small = small ?? Random.Range(0, 100) <= 10;
			if (this.small) {
				smallPlants.Add(this);
			}

			harvestResource = null;
			if (randomHarvestResource && prefab.harvestResources.Count > 0) {
				if (Random.Range(0, 100) <= 5) {
					List<ResourceAmount> resourceChances = new();
					foreach (Resource harvestResource in prefab.harvestResources.Select(rr => rr.resource)) {
						resourceChances.Add(new ResourceAmount(harvestResource, Random.Range(0, 100)));
					}
					harvestResource = resourceChances.OrderByDescending(ra => ra.Amount).First().Resource;
				}
			}
			if (specificHarvestResource != null) {
				harvestResource = specificHarvestResource;
			}

			obj = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, tile.obj.transform.position, Quaternion.identity);
			sr = obj.GetComponent<SpriteRenderer>();
			if (harvestResource != null) {
				name = harvestResource.name + " " + prefab.name;
				sr.sprite = prefab.harvestResourceSprites[harvestResource][this.small][Random.Range(0, prefab.harvestResourceSprites[harvestResource][this.small].Count)];
			} else {
				name = prefab.name;
				sr.sprite = this.small ? prefab.smallSprites[Random.Range(0, prefab.smallSprites.Count)] : prefab.fullSprites[Random.Range(0, prefab.fullSprites.Count)];
			}
			sr.sortingOrder = 1; // Plant Sprite

			obj.name = "Plant: " + prefab.name + " " + sr.sprite.name;
			obj.transform.parent = tile.obj.transform;
		}

		public void Grow() {
			small = false;
			if (harvestResource != null) {
				sr.sprite = prefab.harvestResourceSprites[harvestResource][small][Random.Range(0, prefab.harvestResourceSprites[harvestResource][small].Count)];
			} else {
				sr.sprite = prefab.fullSprites[Random.Range(0, prefab.fullSprites.Count)];
			}
			smallPlants.Remove(this);
		}

		public void Remove() {
			MonoBehaviour.Destroy(obj);
			smallPlants.Remove(this);
		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);
		}

		public enum PlantEnum
		{
			None,
			Cactus,
			SnowTree,
			ThinTree,
			WideTree,
			PalmTree,
			DeadTree,
			Bush
		};

		public static readonly Dictionary<EResource, EResource> seedToHarvestResource = new() {
			{ EResource.AppleSeed, EResource.Apple },
			{ EResource.Blueberry, EResource.Blueberry }
		};
	}
}
