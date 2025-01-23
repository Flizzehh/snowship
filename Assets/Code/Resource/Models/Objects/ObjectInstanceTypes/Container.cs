using System.Collections.Generic;
using System.Linq;

namespace Snowship.NResource
{
	public class Container : ObjectInstance, IInventory
	{
		public static List<Container> containers = new();

		private readonly Inventory inventory;

		public Container(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {
			inventory = new Inventory(this, prefab.maxInventoryWeight, prefab.maxInventoryVolume);
		}

		public Inventory GetInventory() {
			return inventory;
		}

		public static List<Container> GetContainersInRegion(TileManager.Map.Region region) {
			return containers.Where(c => c.tile.region == region).ToList();
		}

		public static Container GetContainerOrChildOnTile(TileManager.Tile tile) {
			Container container = null;
			if (container == null) {
				container = containers.Find(c => c.tile == tile);
			}
			if (container == null) {
				container = TradingPost.tradingPosts.Find(tp => tp.tile == tile);
			}
			return container;
		}
	}
}
