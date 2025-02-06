using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NJob;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class ObjectPrefab : IGroupItem
	{
		public static Dictionary<ObjectEnum, ObjectPrefab> objectPrefabs = new();

		public readonly ObjectEnum type;
		public string Name { get; }
		public Sprite Icon => GetBaseSpriteForVariation(lastSelectedVariation);
		public List<IGroupItem> Children => null;

		public readonly ObjectPrefabGroup.ObjectGroupEnum groupType;
		public readonly ObjectPrefabSubGroup.ObjectSubGroupEnum subGroupType;

		public readonly ObjectInstance.ObjectInstanceType instanceType;

		public readonly int layer;

		public readonly bool bitmasking;
		public readonly bool canRotate;

		public readonly bool blocksLight;

		public readonly int integrity;

		public readonly Dictionary<int, List<Vector2>> multiTilePositions = new();
		public readonly Dictionary<int, Vector2> anchorPositionOffset = new();
		public readonly Dictionary<int, Vector2> dimensions = new();

		public readonly float walkSpeed;
		public readonly bool walkable;

		public readonly bool buildable;

		public readonly float flammability;

		// Container
		public readonly int maxInventoryWeight;
		public readonly int maxInventoryVolume;

		// Light Source
		public readonly int maxLightDistance;
		public readonly Color lightColour;

		// Sleep Spot
		public readonly float restComfortAmount;

		// Crafting Object
		public readonly bool usesFuel;

		// Farm
		public readonly int growthTimeDays;
		public readonly Resource seedResource;
		public readonly List<ResourceRange> harvestResources;

		// Job
		public readonly int timeToBuild;
		public readonly List<ResourceAmount> commonResources = new();
		public readonly List<Variation> variations = new();
		public readonly Variation.VariationNameOrderEnum variationNameOrder;
		public readonly List<SelectionModifiers.SelectionModifiersEnum> selectionModifiers = new();
		public readonly string jobType;
		public readonly bool addToTileWhenBuilt;

		// Sprites

		public enum SpriteType
		{
			Base,
			Bitmask,
			Active
		}

		public readonly Dictionary<Variation, Dictionary<SpriteType, List<Sprite>>> sprites = new();

		// UI State
		public Variation lastSelectedVariation; // Used to show the most recently selected variation on the UI

		public ObjectPrefab(
			ObjectEnum type,
			ObjectPrefabGroup.ObjectGroupEnum groupType,
			ObjectPrefabSubGroup.ObjectSubGroupEnum subGroupType,
			ObjectInstance.ObjectInstanceType instanceType,
			int layer,
			bool bitmasking,
			bool blocksLight,
			int integrity,
			List<Vector2> multiTilePositions,
			float walkSpeed,
			bool walkable,
			bool buildable,
			float flammability,
			int maxInventoryVolume,
			int maxInventoryWeight,
			int maxLightDistance,
			Color lightColour,
			float restComfortAmount,
			bool usesFuel,
			int growthTimeDays,
			Resource seedResource,
			List<ResourceRange> harvestResources,
			int timeToBuild,
			List<ResourceAmount> commonResources,
			List<Variation> variations,
			Variation.VariationNameOrderEnum variationNameOrder,
			// List<SelectionModifiers.SelectionModifiersEnum> selectionModifiers,
			string jobType,
			bool addToTileWhenBuilt
		) {
			this.type = type;
			Name = StringUtilities.SplitByCapitals(type.ToString());

			this.groupType = groupType;
			this.subGroupType = subGroupType;

			this.instanceType = instanceType;

			this.layer = layer;

			this.bitmasking = bitmasking;

			this.blocksLight = blocksLight;

			this.integrity = integrity;

			this.walkable = walkable;
			this.walkSpeed = walkSpeed;

			this.buildable = buildable;

			this.flammability = flammability;

			// Container
			this.maxInventoryWeight = maxInventoryWeight;
			this.maxInventoryVolume = maxInventoryVolume;

			// Light
			this.maxLightDistance = maxLightDistance;
			this.lightColour = lightColour;

			// Sleep Spot
			this.restComfortAmount = restComfortAmount;

			// Crafting Object
			this.usesFuel = usesFuel;

			// Farm
			this.growthTimeDays = growthTimeDays;
			this.seedResource = seedResource;
			this.harvestResources = harvestResources;

			// Job
			this.timeToBuild = timeToBuild;
			this.commonResources = commonResources;
			this.variations = variations;
			this.variationNameOrder = variationNameOrder;
			// this.selectionModifiers = selectionModifiers;
			this.jobType = jobType;
			this.addToTileWhenBuilt = addToTileWhenBuilt;

			// Sprites
			if (variations.Count == 0) {
				Dictionary<SpriteType, List<Sprite>> spriteGroups = new() {
					{ SpriteType.Base, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + Name + "/" + Name.Replace(' ', '-') + "-base").ToList() },
					{ SpriteType.Bitmask, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + Name + "/" + Name.Replace(' ', '-') + "-bitmask").ToList() },
					{ SpriteType.Active, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + Name + "/" + Name.Replace(' ', '-') + "-active").ToList() }
				};
				sprites.Add(Variation.nullVariation, spriteGroups);
			} else {
				foreach (Variation variation in variations) {
					Dictionary<SpriteType, List<Sprite>> spriteGroups = new() {
						{ SpriteType.Base, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + Name + "/" + variation.name + "/" + (variationNameOrder == Variation.VariationNameOrderEnum.VariationObject ? variation.name + " " + Name : Name + " " + variation.name).Replace(' ', '-') + "-base").ToList() },
						{ SpriteType.Bitmask, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + Name + "/" + variation.name + "/" + (variationNameOrder == Variation.VariationNameOrderEnum.VariationObject ? variation.name + " " + Name : Name + " " + variation.name).Replace(' ', '-') + "-bitmask").ToList() },
						{ SpriteType.Active, Resources.LoadAll<Sprite>(@"Sprites/Objects/" + Name + "/" + variation.name + "/" + (variationNameOrder == Variation.VariationNameOrderEnum.VariationObject ? variation.name + " " + Name : Name + " " + variation.name).Replace(' ', '-') + "-active").ToList() }
					};

					sprites.Add(variation, spriteGroups);
				}
			}

			bool allSpritesHaveBitmasking = true;
			int bitmaskSpritesCount = -1;

			foreach (Dictionary<SpriteType, List<Sprite>> spriteGroups in sprites.Values) {
				if (spriteGroups[SpriteType.Base].Count == 0 && spriteGroups[SpriteType.Bitmask].Count > 0) {
					spriteGroups[SpriteType.Base].Add(spriteGroups[SpriteType.Bitmask][0]);
				}

				if (jobType == "PlantFarm") {
					spriteGroups[SpriteType.Base] = new List<Sprite>() { harvestResources[0].resource.image };
				}

				if (spriteGroups[SpriteType.Bitmask].Count == 0) {
					allSpritesHaveBitmasking = false;
				}

				if (bitmaskSpritesCount == -1 || bitmaskSpritesCount == spriteGroups[SpriteType.Bitmask].Count) {
					bitmaskSpritesCount = spriteGroups[SpriteType.Bitmask].Count;
				} else {
					foreach (Variation variation in variations) {
						Debug.Log(variation.name + " " + GetBitmaskSpritesForVariation(variation).Count);
					}
					Debug.LogError("Differing number of bitmask sprites between different variations on object: " + Name);
				}
			}

			// Set Rotation
			canRotate = !bitmasking && allSpritesHaveBitmasking;

			// Multi Tile Positions
			float largestX = 0;
			float largestY = 0;

			if (multiTilePositions.Count <= 0) {
				multiTilePositions.Add(new Vector2(0, 0));
			}

			for (int i = 0; i < (allSpritesHaveBitmasking ? bitmaskSpritesCount : 1); i++) {
				if (i == 0 || (i > 0 && canRotate)) {
					this.multiTilePositions.Add(i, new List<Vector2>());
					if (i == 0) {
						this.multiTilePositions[i].AddRange(multiTilePositions);
					} else {
						foreach (Vector2 oldMultiTilePosition in this.multiTilePositions[i - 1]) {
							Vector2 newMultiTilePosition = new(oldMultiTilePosition.y, largestX - oldMultiTilePosition.x);
							this.multiTilePositions[i].Add(newMultiTilePosition);
						}
					}
					largestX = this.multiTilePositions[i].OrderByDescending(MTP => MTP.x).ToList()[0].x;
					largestY = this.multiTilePositions[i].OrderByDescending(MTP => MTP.y).ToList()[0].y;
					dimensions.Add(i, new Vector2(largestX + 1, largestY + 1));
					anchorPositionOffset.Add(i, new Vector2(largestX / 2, largestY / 2));
				}
			}

			lastSelectedVariation = variations.Count > 0 ? variations[0] : null;
		}

		public void SetVariation(Variation variation) {
			lastSelectedVariation = variation;
		}

		public Variation GetVariationFromString(string variationName) {
			return variations.Count > 0
				? variations.Find(v => v.name.Replace(" ", string.Empty).ToLower() == variationName.Replace(" ", string.Empty).ToLower())
				: null;
		}

		public string GetInstanceNameFromVariation(Variation variation) { // TODO Make better
			return variation == null
				? Name
				: (variation.prefab.variationNameOrder == Variation.VariationNameOrderEnum.ObjectVariation ? Name : string.Empty)
				+ (string.IsNullOrEmpty(variation.name) || variation.prefab.variationNameOrder == Variation.VariationNameOrderEnum.VariationObject ? string.Empty : " ")
				+ variation.name
				+ (string.IsNullOrEmpty(variation.name) || variation.prefab.variationNameOrder == Variation.VariationNameOrderEnum.ObjectVariation ? string.Empty : " ")
				+ (variation.prefab.variationNameOrder == Variation.VariationNameOrderEnum.VariationObject ? Name : string.Empty);
		}

		public Sprite GetBaseSpriteForVariation(Variation variation) {
			if (variation == null) {
				if (variations.Count > 0) {
					if (sprites[variations[0]][SpriteType.Base].Count <= 0) {
						return null;
					} else {
						return sprites[variations[0]][SpriteType.Base][0];
					}
				} else {
					if (sprites.ContainsKey(Variation.nullVariation)) {
						return sprites[Variation.nullVariation][SpriteType.Base][0];
					} else {
						return null;
					}
				}
			} else {
				if (sprites[variation][SpriteType.Base].Count <= 0) {
					return null;
				} else {
					return sprites[variation][SpriteType.Base][0];
				}
			}
		}

		public List<Sprite> GetBitmaskSpritesForVariation(Variation variation) {
			if (variation == null) {
				if (variations.Count > 0) {
					return sprites[variations[0]][SpriteType.Bitmask];
				} else {
					return sprites[Variation.nullVariation][SpriteType.Bitmask];
				}
			} else {
				return sprites[variation][SpriteType.Bitmask];
			}
		}

		public List<Sprite> GetActiveSpritesForVariation(Variation variation) {
			if (variation == null) {
				if (variations.Count > 0) {
					return sprites[variations[0]][SpriteType.Active];
				} else {
					return sprites[Variation.nullVariation][SpriteType.Active];
				}
			} else {
				return sprites[variation][SpriteType.Active];
			}
		}

		public static ObjectPrefab GetObjectPrefabByString(string objectString) {
			return GetObjectPrefabByEnum((ObjectEnum)Enum.Parse(typeof(ObjectEnum), objectString));
		}

		public static ObjectPrefab GetObjectPrefabByEnum(ObjectEnum objectEnum) {
			return objectPrefabs[objectEnum];
		}

		public static List<ObjectPrefab> GetObjectPrefabs() {
			return objectPrefabs.Values.ToList();
		}

		public enum ObjectEnum
		{
			Roof,
			Wall,
			Fence,
			WoodenDoor,
			WoodenFloor, BrickFloor,
			WoodenDock,
			Basket, WoodenChest, WoodenDrawers,
			WoodenBed,
			WoodenChair,
			WoodenTable,
			Torch, WoodenLamp,
			TradingPost,
			Furnace,
			Gin, Loom, SplittingBlock, SplittingLog, Anvil, BrickFormer,
			ChopPlant, Plant,
			Mine, Dig, Fill,
			RemoveRoof, RemoveObject, RemoveFloor, RemoveAll,
			Cancel,
			IncreasePriority, DecreasePriority,
			WheatFarm, PotatoFarm, CottonFarm,
			HarvestFarm,
			CreateResource, PickupResources, TransferResources, CollectResources, EmptyInventory, Sleep, CollectWater, Drink, CollectFood, Eat, WearClothes
		}
	}
}