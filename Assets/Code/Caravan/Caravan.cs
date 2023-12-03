using System.Collections.Generic;
using System.Linq;
using Snowship.NTime;
using Snowship.Selectable;
using UnityEngine;

namespace Snowship.NCaravan {
	public class Caravan : ResourceManager.IInventory, ISelectable {

		public List<Trader> traders = new List<Trader>();
		public int numTraders;
		public CaravanType caravanType;

		public Location location;

		private readonly ResourceManager.Inventory inventory;

		public List<ResourceManager.TradeResourceAmount> resourcesToTrade = new List<ResourceManager.TradeResourceAmount>();

		public List<ResourceManager.ConfirmedTradeResourceAmount> confirmedResourcesToTrade = new List<ResourceManager.ConfirmedTradeResourceAmount>();

		public TileManager.Tile targetTile;

		public ResourceManager.ResourceGroup resourceGroup;

		public int leaveTimer = 0;
		public static readonly int leaveTimerMax = TimeManager.dayLengthSeconds * 2;
		public bool leaving = false;

		private static readonly int minDistinctResources = 1;
		private static readonly float minDistinctResourceChance = 0.5f;
		private static readonly float minAvailableAmountModifier = 0.1f;
		private static readonly float maxAvailableAmountModifier = 0.5f;
		private static readonly int minMinimumCaravanAmount = 5;
		private static readonly int maxMinimumCaravanAmount = 15;

		private readonly List<Trader> removeTraders = new List<Trader>();

		public Caravan() {
			inventory = new ResourceManager.Inventory(this, int.MaxValue, int.MaxValue);
		}

		public Caravan(int numTraders, CaravanType caravanType, List<TileManager.Tile> spawnTiles, TileManager.Tile targetTile) {
			this.numTraders = numTraders;
			this.caravanType = caravanType;
			this.targetTile = targetTile;

			location = GameManager.caravanM.CreateLocation();

			for (int i = 0; i < numTraders && spawnTiles.Count > 0; i++) {
				TileManager.Tile spawnTile = spawnTiles[UnityEngine.Random.Range(0, spawnTiles.Count)];
				SpawnTrader(spawnTile);
				spawnTiles.Remove(spawnTile);
			}

			inventory = new ResourceManager.Inventory(this, int.MaxValue, int.MaxValue);

			resourceGroup = GameManager.resourceM.GetRandomResourceGroup();
			foreach (ResourceManager.Resource resource in resourceGroup.resources.OrderBy(r => UnityEngine.Random.Range(0f, 1f))) { // Randomize resource group list
				int resourceGroupResourceCount = Mathf.Clamp(resourceGroup.resources.Count, minDistinctResources + 1, int.MaxValue); // Ensure minimum count of (minimumDistinctResources + 1)
				if (UnityEngine.Random.Range(0f, 1f) < Mathf.Clamp(((resourceGroupResourceCount - inventory.resources.Count) - minDistinctResources) / (float)(resourceGroupResourceCount - minDistinctResources), minDistinctResourceChance, 1f)) { // Decrease chance of additional distinct resources on caravan as distinct resources on caravan increase
					int resourceAvailableAmount = resource.GetAvailableAmount();
					int caravanAmount = Mathf.RoundToInt(Mathf.Clamp(UnityEngine.Random.Range(resourceAvailableAmount * minAvailableAmountModifier, resourceAvailableAmount * maxAvailableAmountModifier), UnityEngine.Random.Range(minMinimumCaravanAmount, maxMinimumCaravanAmount), int.MaxValue)); // Ensure a minimum number of the resource on the caravan
					inventory.ChangeResourceAmount(resource, caravanAmount, false);
				}
			}
		}

		public void SpawnTrader(TileManager.Tile spawnTile) {
			Trader trader = new Trader(spawnTile, 1f, this);
			traders.Add(trader);

			List<TileManager.Tile> targetTiles = new List<TileManager.Tile>() { targetTile };
			targetTiles.AddRange(targetTile.horizontalSurroundingTiles.Where(t => t != null && t.walkable && t.buildable));

			List<TileManager.Tile> additionalTargetTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile tt in targetTiles) {
				additionalTargetTiles.AddRange(tt.horizontalSurroundingTiles.Where(t => t != null && t.walkable && t.buildable && !additionalTargetTiles.Contains(t)));
			}

			targetTiles.AddRange(additionalTargetTiles);

			trader.MoveToTile(targetTiles[UnityEngine.Random.Range(0, targetTiles.Count)], false);
		}

		public List<ResourceManager.TradeResourceAmount> GenerateTradeResourceAmounts() {
			List<ResourceManager.TradeResourceAmount> tradeResourceAmounts = new List<ResourceManager.TradeResourceAmount>();
			tradeResourceAmounts.AddRange(resourcesToTrade);

			List<ResourceManager.ResourceAmount> caravanResourceAmounts = inventory.resources;

			foreach (ResourceManager.ResourceAmount resourceAmount in caravanResourceAmounts) {
				ResourceManager.TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new ResourceManager.TradeResourceAmount(resourceAmount.resource, resourceAmount.amount, this));
				}
				else {
					existingTradeResourceAmount.Update();
				}
			}

			List<ResourceManager.ResourceAmount> colonyResourceAmounts = new List<ResourceManager.ResourceAmount>();
			if (traders.Count > 0) {
				colonyResourceAmounts = GameManager.resourceM.GetAvailableResourcesInTradingPostsInRegion(traders.Find(t => t != null).overTile.region);
			}

			foreach (ResourceManager.ResourceAmount resourceAmount in colonyResourceAmounts) {
				ResourceManager.TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new ResourceManager.TradeResourceAmount(resourceAmount.resource, 0, this));
				}
				else {
					existingTradeResourceAmount.Update();
				}
			}

			tradeResourceAmounts = tradeResourceAmounts.OrderBy(tra => tra.resource.name).ToList();

			return tradeResourceAmounts;
		}

		public bool DetermineImportanceForResource(ResourceManager.Resource resource) {
			return false; // TODO This needs to be implemented once the originLocation is properly implemented (see above)
		}

		public int DeterminePriceForResource(ResourceManager.Resource resource) {
			return resource.price;
		}

		public void SetSelectedResource(ResourceManager.TradeResourceAmount tradeResourceAmount) {
			ResourceManager.TradeResourceAmount existingTradeResourceAmount = resourcesToTrade.Find(tra => tra.resource == tradeResourceAmount.resource);
			if (existingTradeResourceAmount == null) {
				resourcesToTrade.Add(tradeResourceAmount);
			}
			else {
				if (existingTradeResourceAmount.GetTradeAmount() == 0) {
					resourcesToTrade.Remove(tradeResourceAmount);
				}
				else {
					existingTradeResourceAmount.SetTradeAmount(tradeResourceAmount.GetTradeAmount());
				}
			}
		}

		public void ConfirmTrade() {
			if (leaving) {
				leaving = false;
				leaveTimer = 0;
				foreach (Trader trader in traders) {
					trader.MoveToTile(trader.overTile, false);
				}
			}

			foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				ResourceManager.ConfirmedTradeResourceAmount existingConfirmedTradeResourceAmount = confirmedResourcesToTrade.Find(crtt => crtt.resource == tradeResourceAmount.resource);
				if (existingConfirmedTradeResourceAmount != null) {
					existingConfirmedTradeResourceAmount.tradeAmount += tradeResourceAmount.GetTradeAmount();
					existingConfirmedTradeResourceAmount.amountRemaining += tradeResourceAmount.GetTradeAmount();
				}
				else {
					confirmedResourcesToTrade.Add(new ResourceManager.ConfirmedTradeResourceAmount(tradeResourceAmount.resource, tradeResourceAmount.GetTradeAmount()));
				}
			}

			List<ResourceManager.ResourceAmount> resourcesToReserve = new List<ResourceManager.ResourceAmount>();
			foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				if (tradeResourceAmount.GetTradeAmount() < 0) {
					resourcesToReserve.Add(new ResourceManager.ResourceAmount(tradeResourceAmount.resource, Mathf.Abs(tradeResourceAmount.GetTradeAmount())));
				}
			}

			foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				if (tradeResourceAmount.GetTradeAmount() > 0) {
					inventory.ChangeResourceAmount(tradeResourceAmount.resource, -tradeResourceAmount.GetTradeAmount(), false);
				}
			}

			resourcesToTrade.Clear();

			List<ResourceManager.TradingPost> tradingPostsWithReservedResources = new List<ResourceManager.TradingPost>();

			Trader primaryTrader = traders[0];
			if (primaryTrader != null) {
				foreach (ResourceManager.TradingPost tradingPost in GameManager.resourceM.GetTradingPostsInRegion(primaryTrader.overTile.region).OrderBy(tp => PathManager.RegionBlockDistance(primaryTrader.overTile.regionBlock, tp.zeroPointTile.regionBlock, true, true, false))) {
					List<ResourceManager.ResourceAmount> resourcesToReserveAtThisTradingPost = new List<ResourceManager.ResourceAmount>();
					List<ResourceManager.ResourceAmount> resourcesToReserveToRemove = new List<ResourceManager.ResourceAmount>();
					foreach (ResourceManager.ResourceAmount resourceToReserve in resourcesToReserve) {
						ResourceManager.ResourceAmount resourceAmount = tradingPost.GetInventory().resources.Find(r => r.resource == resourceToReserve.resource);
						if (resourceAmount != null) {
							int amountToReserve = resourceToReserve.amount < resourceAmount.amount ? resourceToReserve.amount : resourceAmount.amount;
							resourcesToReserveAtThisTradingPost.Add(new ResourceManager.ResourceAmount(resourceToReserve.resource, amountToReserve));
							resourceToReserve.amount -= amountToReserve;
							if (resourceToReserve.amount == 0) {
								resourcesToReserveToRemove.Add(resourceToReserve);
							}
						}
					}

					if (resourcesToReserveAtThisTradingPost.Count > 0) {
						tradingPost.GetInventory().ReserveResources(resourcesToReserveAtThisTradingPost, primaryTrader);
						tradingPostsWithReservedResources.Add(tradingPost);
					}

					foreach (ResourceManager.ResourceAmount resourceToReserveToRemove in resourcesToReserveToRemove) {
						resourcesToReserve.Remove(resourceToReserveToRemove);
					}

					resourcesToReserveToRemove.Clear();
				}

				if (tradingPostsWithReservedResources.Count > 0) {
					primaryTrader.tradingPosts = tradingPostsWithReservedResources;
				}
			}
		}

		/*
			* Returns:
			*		true - if any of the caravan's traders still exist on the map
			*		false - if all of the caravan's traders have left the map
			*/
		public bool Update() {
			foreach (Trader trader in traders) {
				trader.Update();

				if (trader.overTile == trader.leaveTile) {
					removeTraders.Add(trader);
				}
			}

			foreach (Trader trader in removeTraders) {
				trader.Remove();

				traders.Remove(trader);
			}

			removeTraders.Clear();

			if (!GameManager.timeM.GetPaused() && GameManager.timeM.minuteChanged) {
				if (!leaving && confirmedResourcesToTrade.Count <= 0) {
					if (leaveTimer >= leaveTimerMax) {
						leaving = true;
						leaveTimer = 0;

						foreach (Trader trader in traders) {
							List<TileManager.Tile> validLeaveTiles = GameManager.colonyM.colony.map.edgeTiles.Where(t => t.region == trader.overTile.region).ToList();
							if (validLeaveTiles.Count > 0) {
								trader.leaveTile = validLeaveTiles[UnityEngine.Random.Range(0, validLeaveTiles.Count)];
								trader.MoveToTile(trader.leaveTile, false);
							}
							else {
								trader.leaveTile = trader.overTile;
								trader.Remove();
							}
						}
					}
					else {
						leaveTimer += 1;
					}
				}
			}

			return traders.Count > 0;
		}

		public ResourceManager.Inventory GetInventory() {
			return inventory;
		}

		void ISelectable.Select() {

		}

		void ISelectable.Deselect() {

		}
	}
}
