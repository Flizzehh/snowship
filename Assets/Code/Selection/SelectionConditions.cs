using Snowship.NJob;
using Snowship.NResource;
using static TileManager;

namespace Snowship.Selectable
{
	public static class SelectionConditions
	{
		public static bool Walkable(Tile tile, int _) {
			return tile.walkable;
		}

		public static bool NotWalkable(Tile tile, int _) {
			return !Walkable(tile, _);
		}

		public static bool WalkableIncludingFences(Tile tile, int _) { // TODO Should `int layer` replace the `layer: 2` in the GetObjectInstanceAtLayer call?
			return tile.GetObjectInstanceAtLayer(2)?.prefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Fences || tile.walkable;
		}

		public static bool Buildable(Tile tile, int _) {
			return tile.buildable;
		}

		public static bool NotBuildable(Tile tile, int _) {
			return !Buildable(tile, _);
		}

		public static bool Stone(Tile tile, int _) {
			return tile.tileType.groupType == TileTypeGroup.TypeEnum.Stone;
		}

		public static bool NotStone(Tile tile, int _) {
			return !Stone(tile, _);
		}

		public static bool WaterOrIce(Tile tile, int _) {
			return tile.tileType.groupType == TileTypeGroup.TypeEnum.Water;
		}

		public static bool NeitherWaterNorIce(Tile tile, int _) {
			return !WaterOrIce(tile, _);
		}

		public static bool LiquidWater(Tile tile, int _) {
			return tile.tileType.classes[TileType.ClassEnum.LiquidWater];
		}

		public static bool NotLiquidWater(Tile tile, int _) {
			return !LiquidWater(tile, _);
		}

		public static bool NeitherStoneNorWater(Tile tile, int _) {
			return tile.tileType is not { groupType: TileTypeGroup.TypeEnum.Water or TileTypeGroup.TypeEnum.Stone };
		}

		public static bool Plant(Tile tile, int _) {
			return tile.plant != null;
		}

		public static bool NoPlant(Tile tile, int _) {
			return !Plant(tile, _);
		}

		public static bool Farm(Tile tile, int _) {
			return tile.farm != null;
		}

		public static bool NoFarm(Tile tile, int _) {
			return !Farm(tile, _);
		}

		public static bool Roof(Tile tile, int _) {
			return tile.HasRoof();
		}

		public static bool NoRoof(Tile tile, int _) {
			return !Roof(tile, _);
		}

		public static bool Objects(Tile tile, int _) {
			return tile.GetAllObjectInstances().Count > 0;
		}

		public static bool NoObjects(Tile tile, int _) {
			return !Objects(tile, _);
		}

		public static bool CoastalWater(Tile tile, int _) {
			return tile.CoastalWater;
		}

		public static bool Hole(Tile tile, int _) {
			return tile.tileType.groupType == TileTypeGroup.TypeEnum.Hole;
		}

		public static bool NoHole(Tile tile, int _) {
			return !Hole(tile, _);
		}

		public static bool DugPreviously(Tile tile, int _) {
			return tile.dugPreviously;
		}

		public static bool NotDugPreviously(Tile tile, int _) {
			return !DugPreviously(tile, _);
		}

		public static bool Fillable(Tile tile, int _) {
			return DugPreviously(tile, _) || Hole(tile, _) || CoastalWater(tile, _);
		}

		public static bool NoSameLayerJobs(Tile tile, int layer) {
			return GameManager.Get<JobManager>().JobAtLayerExistsAtTile(tile, layer) == null; // TODO multiTilePositions need to be accounted for?
		}

		public static bool SameLayerObject(Tile tile, int layer) {
			return tile.GetObjectInstanceAtLayer(layer) != null;
		}

		public static bool NoSameLayerObject(Tile tile, int layer) {
			return !SameLayerObject(tile, layer);
		}

		public static bool NoObjectWithTileAsNonPrimaryTile(Tile tile, int layer) {
			return tile.GetObjectInstanceAtLayer(layer)?.tile == tile;
		}
	}
}