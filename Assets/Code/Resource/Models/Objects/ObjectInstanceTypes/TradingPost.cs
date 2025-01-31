using System.Collections.Generic;
using System.Linq;

namespace Snowship.NResource
{
	public class TradingPost : Container
	{
		public static List<TradingPost> tradingPosts = new();

		public List<ResourceAmount> targetResourceAmounts = new();

		public TradingPost(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {

		}

		public static List<TradingPost> GetTradingPostsInRegion(TileManager.Map.Region region) {
			return tradingPosts.Where(tp => tp.tile.region == region).ToList();
		}

		public static List<ResourceAmount> GetAvailableResourcesInTradingPostsInRegion(TileManager.Map.Region region) {
			List<ResourceAmount> availableResources = new();
			foreach (TradingPost tradingPost in GetTradingPostsInRegion(region)) {
				foreach (ResourceAmount resourceAmount in tradingPost.Inventory.resources) {
					ResourceAmount accessibleResource = availableResources.Find(ra => ra.Resource == resourceAmount.Resource);
					if (accessibleResource != null) {
						accessibleResource.Amount += resourceAmount.Amount;
					} else {
						availableResources.Add(new ResourceAmount(resourceAmount.Resource, resourceAmount.Amount));
					}
				}
			}
			return availableResources;
		}
	}
}