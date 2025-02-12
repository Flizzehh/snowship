using System.Collections.Generic;
using System.Linq;
using Snowship.NColony;
using Snowship.NJob;
using Snowship.NUtilities;
using Snowship.Selectable;
using UnityEngine;

namespace Snowship.NResource
{
	public class ObjectInstance : ISelectable
	{
		public static readonly Dictionary<ObjectPrefab, List<ObjectInstance>> ObjectInstances = new();

		public readonly TileManager.Tile tile; // The tile that this object covers that is closest to the zeroPointTile (usually they are the same tile)
		public readonly List<TileManager.Tile> additionalTiles = new();
		public readonly TileManager.Tile zeroPointTile; // The tile representing the (0,0) position of the object even if the object doesn't cover it

		public readonly ObjectPrefab prefab;
		public readonly Variation variation;

		public readonly GameObject obj;
		public readonly SpriteRenderer sr;

		public readonly GameObject activeOverlay;
		public readonly SpriteRenderer aosr;

		public readonly int rotationIndex;

		public bool active;

		public float integrity;

		public bool visible;

		public enum ObjectInstanceType
		{
			Normal,
			Container,
			TradingPost,
			Bed,
			LightSource,
			CraftingObject,
			Farm
		}

		protected ObjectInstance(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) {
			this.prefab = prefab;
			this.variation = variation;

			this.tile = tile;
			zeroPointTile = tile;
			foreach (Vector2 multiTilePosition in prefab.multiTilePositions[rotationIndex]) {
				TileManager.Tile additionalTile = zeroPointTile.map.GetTileFromPosition(zeroPointTile.obj.transform.position + (Vector3)multiTilePosition);
				additionalTiles.Add(additionalTile);
				if (additionalTile != zeroPointTile) {
					additionalTile.SetObjectInstanceReference(this);
				}
			}
			if (additionalTiles.Count > 0 && !additionalTiles.Contains(tile)) {
				tile = additionalTiles.OrderBy(additionalTile => Vector2.Distance(tile.obj.transform.position, additionalTile.obj.transform.position)).ToList()[0];
			} else if (additionalTiles.Count <= 0) {
				additionalTiles.Add(tile);
			}

			this.rotationIndex = rotationIndex;

			obj = MonoBehaviour.Instantiate(GameManager.Get<ResourceManager>().objectPrefab, zeroPointTile.obj.transform, false);
			sr = obj.GetComponent<SpriteRenderer>();
			obj.transform.position += (Vector3)prefab.anchorPositionOffset[rotationIndex];
			obj.name = "Tile Object Instance: " + prefab.Name;
			sr.sortingOrder = (int)SortingOrder.Object + prefab.layer; // Tile Object Sprite
			sr.sprite = prefab.GetBaseSpriteForVariation(variation);

			activeOverlay = obj.transform.Find("ActiveOverlay").gameObject;
			aosr = activeOverlay.GetComponent<SpriteRenderer>();
			aosr.sortingOrder = sr.sortingOrder + 1;

			if (prefab.blocksLight) {
				foreach (LightSource lightSource in LightSource.lightSources) {
					foreach (TileManager.Tile objectTile in additionalTiles) {
						if (lightSource.litTiles.Contains(objectTile)) {
							lightSource.SetTileBrightnesses();
						}
					}
				}
			}

			SetColour(tile.sr.color);

			integrity = prefab.integrity;
		}

		public void FinishCreation() {
			List<TileManager.Tile> bitmaskingTiles = new();
			foreach (TileManager.Tile additionalTile in additionalTiles) {
				bitmaskingTiles.Add(additionalTile);
				bitmaskingTiles.AddRange(additionalTile.surroundingTiles);
			}
			bitmaskingTiles = bitmaskingTiles.Distinct().ToList();
			GameManager.Get<ResourceManager>().Bitmask(bitmaskingTiles);
			GameManager.Get<ColonyManager>().colony.map.Bitmasking(bitmaskingTiles, true, true);
			foreach (TileManager.Tile tile in additionalTiles) {
				SetColour(tile.sr.color);
			}
		}

		public void SetColour(Color newColour) {
			sr.color = new Color(newColour.r, newColour.g, newColour.b, 1f);
		}

		public void SetActiveSprite(CreateResourceJob createResourceJob, bool jobActive) {
			if (active && jobActive) {
				if (prefab.GetActiveSpritesForVariation(variation).Count > 0) {
					if (prefab.type == ObjectPrefab.ObjectEnum.SplittingBlock) {
						int customActiveSpriteIndex = 0;
						if (createResourceJob.CreateResource.resource.type == EResource.Wood) {
							customActiveSpriteIndex = 0;
						} else if (createResourceJob.CreateResource.resource.type == EResource.Firewood) {
							customActiveSpriteIndex = 1;
						}
						aosr.sprite = prefab.GetActiveSpritesForVariation(variation)[4 * customActiveSpriteIndex + rotationIndex];
					} else {
						aosr.sprite = prefab.GetActiveSpritesForVariation(variation)[rotationIndex];
					}
				}
			} else {
				aosr.sprite = GameManager.Get<ResourceManager>().clearSquareSprite;
			}
		}

		public virtual void SetActive(bool active) {
			this.active = active;
		}

		public virtual void Update() {

		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);
		}

		void ISelectable.Select() {

		}

		void ISelectable.Deselect() {

		}

		public static ObjectInstance CreateObjectInstance(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex, bool addToList) {
			ObjectInstance instance = null;
			switch (prefab.instanceType) {
				case ObjectInstanceType.Normal:
					instance = new ObjectInstance(prefab, variation, tile, rotationIndex);
					break;
				case ObjectInstanceType.Container:
					instance = new Container(prefab, variation, tile, rotationIndex);
					Container.containers.Add((Container)instance);
					break;
				case ObjectInstanceType.TradingPost:
					instance = new TradingPost(prefab, variation, tile, rotationIndex);
					TradingPost.tradingPosts.Add((TradingPost)instance);
					break;
				case ObjectInstanceType.Bed:
					instance = new Bed(prefab, variation, tile, rotationIndex);
					Bed.Beds.Add((Bed)instance);
					break;
				case ObjectInstanceType.LightSource:
					instance = new LightSource(prefab, variation, tile, rotationIndex);
					LightSource.lightSources.Add((LightSource)instance);
					break;
				case ObjectInstanceType.CraftingObject:
					instance = new CraftingObject(prefab, variation, tile, rotationIndex);
					CraftingObject.craftingObjectInstances.Add((CraftingObject)instance);
					break;
				case ObjectInstanceType.Farm:
					instance = new Farm(prefab, variation, tile);
					Farm.farms.Add((Farm)instance);
					tile.farm = (Farm)instance;
					break;
			}
			if (instance == null) {
				Debug.LogError("Instance is null for prefab " + (prefab != null ? prefab.Name : "null") + " at tile " + tile.obj.transform.position);
			}
			if (addToList) {
				AddObjectInstance(instance);
			}
			return instance;
		}

		public static List<ObjectInstance> GetObjectInstancesByPrefab(ObjectPrefab prefab) {
			if (ObjectInstances.ContainsKey(prefab)) {
				return ObjectInstances[prefab];
			}
			return null;
		}

		public static void AddObjectInstance(ObjectInstance objectInstance) {
			if (ObjectInstances.ContainsKey(objectInstance.prefab)) {
				ObjectInstances[objectInstance.prefab].Add(objectInstance);
				// GameManager.Get<UIManagerOld>().ChangeObjectPrefabElements(UIManagerOld.ChangeTypeEnum.Update, objectInstance.prefab);
			} else {
				ObjectInstances.Add(objectInstance.prefab, new List<ObjectInstance>() { objectInstance });
				// GameManager.Get<UIManagerOld>().ChangeObjectPrefabElements(UIManagerOld.ChangeTypeEnum.Add, objectInstance.prefab);
			}
		}

		public static void RemoveObjectInstance(ObjectInstance instance) {
			switch (instance.prefab.instanceType) {
				case ObjectInstanceType.Normal:
					break;
				case ObjectInstanceType.Container:
					Container container = (Container)instance;

					// if (GameManager.Get<UIManagerOld>().selectedContainer == container) {
					// 	GameManager.Get<UIManagerOld>().SetSelectedContainer(null);
					// }

					Container.containers.Remove(container);
					break;
				case ObjectInstanceType.TradingPost:
					TradingPost tradingPost = (TradingPost)instance;

					// if (GameManager.Get<UIManagerOld>().selectedTradingPost == tradingPost) {
					// 	GameManager.Get<UIManagerOld>().SetSelectedTradingPost(null);
					// }

					TradingPost.tradingPosts.Remove(tradingPost);
					break;
				case ObjectInstanceType.Bed:
					Bed bed = (Bed)instance;

					Bed.Beds.Remove(bed);
					break;
				case ObjectInstanceType.LightSource:
					LightSource lightSource = (LightSource)instance;

					lightSource.RemoveTileBrightnesses();

					LightSource.lightSources.Remove(lightSource);
					break;
				case ObjectInstanceType.CraftingObject:
					CraftingObject craftingObject = (CraftingObject)instance;

					// if (GameManager.Get<UIManagerOld>().selectedCraftingObject == craftingObject) {
					// 	GameManager.Get<UIManagerOld>().SetSelectedCraftingObject(null);
					// }

					CraftingObject.craftingObjectInstances.Remove(craftingObject);
					break;
				case ObjectInstanceType.Farm:
					Farm farm = (Farm)instance;

					farm.tile.farm = null;

					Farm.farms.Remove(farm);
					break;
				default:
					Debug.LogWarning("No removal case for removing " + instance.prefab.Name);
					break;
			}

			if (ObjectInstances.ContainsKey(instance.prefab)) {
				ObjectInstances[instance.prefab].Remove(instance);

				// GameManager.Get<UIManagerOld>().ChangeObjectPrefabElements(UIManagerOld.ChangeTypeEnum.Update, instance.prefab);
			} else {
				Debug.LogWarning("Tried removing a tile object instance which isn't in the list");
			}

			if (ObjectInstances[instance.prefab].Count <= 0) {
				ObjectInstances.Remove(instance.prefab);

				// GameManager.Get<UIManagerOld>().ChangeObjectPrefabElements(UIManagerOld.ChangeTypeEnum.Remove, instance.prefab);
			}
		}
	}
}