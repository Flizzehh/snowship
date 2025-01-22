using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NResource.Models;
using Snowship.NResources;
using Snowship.NTime;
using Snowship.Selectable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NCaravan {
	public class Caravan : ResourceManager.IInventory, ISelectable, IDisposable {

		public List<Trader> traders = new List<Trader>();
		public int numTraders;
		public CaravanType caravanType;

		public Location location;

		private readonly Inventory inventory;

		public List<TradeResourceAmount> resourcesToTrade = new List<TradeResourceAmount>();

		public List<ConfirmedTradeResourceAmount> confirmedResourcesToTrade = new List<ConfirmedTradeResourceAmount>();

		public TileManager.Tile targetTile;

		public ResourceManager.ResourceGroup resourceGroup;

		public int leaveTimer;
		public const int leaveTimerMax = SimulationDateTime.DayLengthSeconds * 2;
		public bool leaving;

		private const int MinDistinctResources = 1;
		private const float MinDistinctResourceChance = 0.5f;
		private const float MinAvailableAmountModifier = 0.1f;
		private const float MaxAvailableAmountModifier = 0.5f;
		private const int MinMinimumCaravanAmount = 5;
		private const int MaxMinimumCaravanAmount = 15;

		private readonly List<Trader> removeTraders = new List<Trader>();

		public Caravan() {
			inventory = new Inventory(this, int.MaxValue, int.MaxValue);
		}

		public Caravan(
			int numTraders,
			CaravanType caravanType,
			List<TileManager.Tile> spawnTiles,
			TileManager.Tile targetTile
		) {
			this.numTraders = numTraders;
			this.caravanType = caravanType;
			this.targetTile = targetTile;

			location = Location.GenerateLocation();

			for (int i = 0; i < numTraders && spawnTiles.Count > 0; i++) {
				TileManager.Tile spawnTile = spawnTiles[Random.Range(0, spawnTiles.Count)];
				SpawnTrader(spawnTile);
				spawnTiles.Remove(spawnTile);
			}

			inventory = new Inventory(this, int.MaxValue, int.MaxValue);

			resourceGroup = GameManager.resourceM.GetRandomResourceGroup();
			foreach (ResourceManager.Resource resource in resourceGroup.resources.OrderBy(r => Random.Range(0f, 1f))) { // Randomize resource group list
				int resourceGroupResourceCount = Mathf.Clamp(resourceGroup.resources.Count, MinDistinctResources + 1, int.MaxValue); // Ensure minimum count of (minimumDistinctResources + 1)
				if (Random.Range(0f, 1f) < Mathf.Clamp(((resourceGroupResourceCount - inventory.resources.Count) - MinDistinctResources) / (float)(resourceGroupResourceCount - MinDistinctResources), MinDistinctResourceChance, 1f)) { // Decrease chance of additional distinct resources on caravan as distinct resources on caravan increase
					int resourceAvailableAmount = resource.GetAvailableAmount();
					int caravanAmount = Mathf.RoundToInt(Mathf.Clamp(Random.Range(resourceAvailableAmount * MinAvailableAmountModifier, resourceAvailableAmount * MaxAvailableAmountModifier), Random.Range(MinMinimumCaravanAmount, MaxMinimumCaravanAmount), int.MaxValue)); // Ensure a minimum number of the resource on the caravan
					inventory.ChangeResourceAmount(resource, caravanAmount, false);
				}
			}

			GameManager.timeM.OnTimeChanged += UpdateCaravanLeaveState;
		}

		public void Dispose() {
			GameManager.timeM.OnTimeChanged -= UpdateCaravanLeaveState;
		}

		private void SpawnTrader(TileManager.Tile spawnTile) {
			Trader trader = new Trader(spawnTile, 1f, this);
			traders.Add(trader);

			List<TileManager.Tile> targetTiles = new List<TileManager.Tile> { targetTile };
			targetTiles.AddRange(targetTile.horizontalSurroundingTiles.Where(t => t is { walkable: true, buildable: true }));

			List<TileManager.Tile> additionalTargetTiles = new List<TileManager.Tile>();
			foreach (TileManager.Tile tt in targetTiles) {
				additionalTargetTiles.AddRange(tt.horizontalSurroundingTiles.Where(t => t is { walkable: true, buildable: true } && !additionalTargetTiles.Contains(t)));
			}

			targetTiles.AddRange(additionalTargetTiles);

			trader.MoveToTile(targetTiles[Random.Range(0, targetTiles.Count)], false);
		}

		public List<TradeResourceAmount> GenerateTradeResourceAmounts() {
			List<TradeResourceAmount> tradeResourceAmounts = new List<TradeResourceAmount>();
			tradeResourceAmounts.AddRange(resourcesToTrade);

			List<ResourceAmount> caravanResourceAmounts = inventory.resources;

			foreach (ResourceAmount resourceAmount in caravanResourceAmounts) {
				TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.Resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new TradeResourceAmount(resourceAmount.Resource, resourceAmount.Amount, 0, this));
				} else {
					existingTradeResourceAmount.Update();
				}
			}

			List<ResourceAmount> colonyResourceAmounts = new();
			if (traders.Count > 0) {
				colonyResourceAmounts = GameManager.resourceM.GetAvailableResourcesInTradingPostsInRegion(traders.Find(t => t != null).overTile.region);
			}

			foreach (ResourceAmount resourceAmount in colonyResourceAmounts) {
				TradeResourceAmount existingTradeResourceAmount = tradeResourceAmounts.Find(tra => tra.resource == resourceAmount.Resource);
				if (existingTradeResourceAmount == null) {
					tradeResourceAmounts.Add(new TradeResourceAmount(resourceAmount.Resource, 0, resourceAmount.Amount, this));
				} else {
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

		public void SetSelectedResource(TradeResourceAmount tradeResourceAmount) {
			TradeResourceAmount existingTradeResourceAmount = resourcesToTrade.Find(tra => tra.resource == tradeResourceAmount.resource);
			if (existingTradeResourceAmount == null) {
				resourcesToTrade.Add(tradeResourceAmount);
			} else {
				if (existingTradeResourceAmount.GetTradeAmount() == 0) {
					resourcesToTrade.Remove(tradeResourceAmount);
				} else {
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

			foreach (TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				ConfirmedTradeResourceAmount existingConfirmedTradeResourceAmount = confirmedResourcesToTrade.Find(crtt => crtt.resource == tradeResourceAmount.resource);
				if (existingConfirmedTradeResourceAmount != null) {
					existingConfirmedTradeResourceAmount.tradeAmount += tradeResourceAmount.GetTradeAmount();
					existingConfirmedTradeResourceAmount.amountRemaining += tradeResourceAmount.GetTradeAmount();
				} else {
					confirmedResourcesToTrade.Add(new ConfirmedTradeResourceAmount(tradeResourceAmount.resource, tradeResourceAmount.GetTradeAmount()));
				}
			}

			List<ResourceAmount> resourcesToReserve = new();
			foreach (TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				if (tradeResourceAmount.GetTradeAmount() < 0) {
					resourcesToReserve.Add(new ResourceAmount(tradeResourceAmount.resource, Mathf.Abs(tradeResourceAmount.GetTradeAmount())));
				}
			}

			foreach (TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				if (tradeResourceAmount.GetTradeAmount() > 0) {
					inventory.ChangeResourceAmount(tradeResourceAmount.resource, -tradeResourceAmount.GetTradeAmount(), false);
				}
			}

			resourcesToTrade.Clear();

			List<ResourceManager.TradingPost> tradingPostsWithReservedResources = new List<ResourceManager.TradingPost>();

			Trader primaryTrader = traders[0];
			if (primaryTrader != null) {
				foreach (ResourceManager.TradingPost tradingPost in GameManager.resourceM.GetTradingPostsInRegion(primaryTrader.overTile.region).OrderBy(tp => PathManager.RegionBlockDistance(primaryTrader.overTile.regionBlock, tp.zeroPointTile.regionBlock, true, true, false))) {
					List<ResourceAmount> resourcesToReserveAtThisTradingPost = new();
					List<ResourceAmount> resourcesToReserveToRemove = new();
					foreach (ResourceAmount resourceToReserve in resourcesToReserve) {
						ResourceAmount resourceAmount = tradingPost.GetInventory().resources.Find(r => r.Resource == resourceToReserve.Resource);
						if (resourceAmount != null) {
							int amountToReserve = resourceToReserve.Amount < resourceAmount.Amount ? resourceToReserve.Amount : resourceAmount.Amount;
							resourcesToReserveAtThisTradingPost.Add(new ResourceAmount(resourceToReserve.Resource, amountToReserve));
							resourceToReserve.Amount -= amountToReserve;
							if (resourceToReserve.Amount == 0) {
								resourcesToReserveToRemove.Add(resourceToReserve);
							}
						}
					}

					if (resourcesToReserveAtThisTradingPost.Count > 0) {
						tradingPost.GetInventory().ReserveResources(resourcesToReserveAtThisTradingPost, primaryTrader);
						tradingPostsWithReservedResources.Add(tradingPost);
					}

					foreach (ResourceAmount resourceToReserveToRemove in resourcesToReserveToRemove) {
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

			return traders.Count > 0;
		}

		private void UpdateCaravanLeaveState(SimulationDateTime _) {
			if (leaving || confirmedResourcesToTrade.Count > 0) {
				return;
			}

			if (leaveTimer >= leaveTimerMax) {
				leaving = true;
				leaveTimer = 0;

				foreach (Trader trader in traders) {
					List<TileManager.Tile> validLeaveTiles = GameManager.colonyM.colony.map.edgeTiles.Where(t => t.region == trader.overTile.region).ToList();
					if (validLeaveTiles.Count > 0) {
						trader.leaveTile = validLeaveTiles[Random.Range(0, validLeaveTiles.Count)];
						trader.MoveToTile(trader.leaveTile, false);
					} else {
						trader.leaveTile = trader.overTile;
						trader.Remove();
					}
				}
			} else {
				leaveTimer += 1;
			}
		}

		public Inventory GetInventory() {
			return inventory;
		}

		void ISelectable.Select() {

		}

		void ISelectable.Deselect() {

		}
	}
}
