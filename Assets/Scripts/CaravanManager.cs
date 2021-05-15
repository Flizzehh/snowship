using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaravanManager : BaseManager {

	private List<Caravan> removeCaravans = new List<Caravan>();

	public override void Update() {
		UpdateCaravanTimer();
		
		foreach (Caravan caravan in caravans) {
			if (!caravan.Update()) {
				removeCaravans.Add(caravan);
			}
		}
		foreach (Caravan caravan in removeCaravans) {
			if (selectedCaravan == caravan) {
				SetSelectedCaravan(null);
			}

			caravans.Remove(caravan);

			GameManager.uiM.SetCaravanElements();
		}
		removeCaravans.Clear();

		if (Input.GetMouseButtonDown(1)) {
			SetSelectedCaravan(null);
		}
	}

	public int caravanTimer = 0; // The time since the last caravan visited
	private static readonly int caravanTimeMin = TimeManager.dayLengthSeconds * 7; // Caravans will only come once every caravanTimeMin in-game minutes
	private static readonly int caravanTimeMax = TimeManager.dayLengthSeconds * 27; // Caravans will definitely come if the caravanTimer is greater than caravanTimeMax

	public void UpdateCaravanTimer() {
		if (!GameManager.timeM.GetPaused() && GameManager.timeM.minuteChanged) {
			caravanTimer += 1;
			if (caravanTimer > caravanTimeMin) {
				if (UnityEngine.Random.Range(0f, 1f) < (((float)caravanTimer - caravanTimeMin) / (caravanTimeMax - caravanTimeMin))) {
					caravanTimer = 0;
					SpawnCaravan(CaravanTypeEnum.Foot, 4);
				}
			}
		}
	}

	public List<Caravan> caravans = new List<Caravan>();

	public void SpawnCaravan(CaravanTypeEnum caravanType, int maxNumTraders) {
		List<TileManager.Tile> validSpawnTiles = null;
		if (caravanType == CaravanTypeEnum.Foot || caravanType == CaravanTypeEnum.Wagon) {
			validSpawnTiles = GameManager.colonyM.colony.map.edgeTiles.Where(tile => tile.walkable && !tile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]).ToList();
		} else if (caravanType == CaravanTypeEnum.Boat) {
			validSpawnTiles = new List<TileManager.Tile>(); // TODO Implement boat caravans
		}
		if (validSpawnTiles.Count > 0) {
			TileManager.Tile targetSpawnTile = validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)];

			List<TileManager.Tile> edgeTilesConnectedToTargetSpawnTile = new List<TileManager.Tile>();

			List<TileManager.Tile> spawnTilesFrontier = new List<TileManager.Tile>() { targetSpawnTile };
			List<TileManager.Tile> spawnTilesChecked = new List<TileManager.Tile>();
			while (spawnTilesFrontier.Count > 0) {
				TileManager.Tile currentTile = spawnTilesFrontier[0];
				spawnTilesFrontier.RemoveAt(0);
				spawnTilesChecked.Add(currentTile);
				edgeTilesConnectedToTargetSpawnTile.Add(currentTile);
				foreach (TileManager.Tile nTile in currentTile.horizontalSurroundingTiles) {
					if (!spawnTilesChecked.Contains(nTile) && validSpawnTiles.Contains(nTile)) {
						spawnTilesFrontier.Add(nTile);
					}
				}
			}

			if (edgeTilesConnectedToTargetSpawnTile.Count > 0) {
				int numTraders = UnityEngine.Random.Range(1, maxNumTraders + 1);
				List<TileManager.Tile> edgeTilesCloseToTargetSpawnTile = edgeTilesConnectedToTargetSpawnTile.Where(tile => Vector2.Distance(tile.obj.transform.position, targetSpawnTile.obj.transform.position) <= numTraders * 2).ToList();
				if (edgeTilesCloseToTargetSpawnTile.Count > 0) {
					if (edgeTilesCloseToTargetSpawnTile.Count < numTraders) {
						numTraders = edgeTilesCloseToTargetSpawnTile.Count;
					}

					List<ResourceManager.ObjectInstance> tradingPosts = new List<ResourceManager.ObjectInstance>();
					foreach (ResourceManager.ObjectPrefab prefab in GameManager.resourceM.objectInstances.Keys.Where(op => op.subGroupType == ResourceManager.ObjectSubGroupEnum.TradingPosts)) {
						foreach (ResourceManager.ObjectInstance tradingPost in GameManager.resourceM.objectInstances[prefab]) {
							if (tradingPost.zeroPointTile.region == targetSpawnTile.region) {
								tradingPosts.Add(tradingPost);
							}
						}
					}

					ResourceManager.ObjectInstance selectedTradingPost = null;
					if (tradingPosts.Count > 0) {
						selectedTradingPost = tradingPosts[UnityEngine.Random.Range(0, tradingPosts.Count)];
					}

					TileManager.Tile targetTile = null;
					if (selectedTradingPost != null) {
						targetTile = selectedTradingPost.zeroPointTile;
					} else {
						List<TileManager.Tile> validTargetTiles = targetSpawnTile.region.tiles.Where(tile => tile.walkable && !tile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]).ToList();
						targetTile = validTargetTiles[UnityEngine.Random.Range(0, validTargetTiles.Count)];
					}

					AddCaravan(new Caravan(numTraders, CaravanTypeEnum.Foot, edgeTilesCloseToTargetSpawnTile, targetTile));
				}
			}
		}
	}

	public void AddCaravan(Caravan caravan) {
		caravans.Add(caravan);

		GameManager.uiM.SetCaravanElements();
	}

	public enum CaravanTypeEnum { Foot, Wagon, Boat };

	public Caravan selectedCaravan;

	public void SetSelectedCaravan(Caravan selectedCaravan) {
		this.selectedCaravan = selectedCaravan;

		GameManager.uiM.SetTradeMenu();
	}

	public class Caravan : ResourceManager.IInventory {

		public List<Trader> traders = new List<Trader>();
		public int numTraders;
		public CaravanTypeEnum caravanType;

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

		public Caravan(int numTraders, CaravanTypeEnum caravanType, List<TileManager.Tile> spawnTiles, TileManager.Tile targetTile) {
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
				} else {
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

		public void SetSelectedResource(ResourceManager.TradeResourceAmount tradeResourceAmount) {
			ResourceManager.TradeResourceAmount existingTradeResourceAmount = resourcesToTrade.Find(tra => tra.resource == tradeResourceAmount.resource);
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

			foreach (ResourceManager.TradeResourceAmount tradeResourceAmount in resourcesToTrade) {
				ResourceManager.ConfirmedTradeResourceAmount existingConfirmedTradeResourceAmount = confirmedResourcesToTrade.Find(crtt => crtt.resource == tradeResourceAmount.resource);
				if (existingConfirmedTradeResourceAmount != null) {
					existingConfirmedTradeResourceAmount.tradeAmount += tradeResourceAmount.GetTradeAmount();
					existingConfirmedTradeResourceAmount.amountRemaining += tradeResourceAmount.GetTradeAmount();
				} else {
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
							} else {
								trader.leaveTile = trader.overTile;
								trader.Remove();
							}
						}
					} else {
						leaveTimer += 1;
					}
				}
			}
			return traders.Count > 0;
		}
		public ResourceManager.Inventory GetInventory() {
			return inventory;
		}
	}

	public class Trader : HumanManager.Human {

		public Caravan caravan;

		public TileManager.Tile leaveTile;

		public List<ResourceManager.TradingPost> tradingPosts;

		public Trader(TileManager.Tile spawnTile, float startingHealth, Caravan caravan) : base(spawnTile, startingHealth) {
			this.caravan = caravan;

			obj.transform.SetParent(GameManager.resourceM.traderParent.transform, false);
		}

		public override void SetName(string name) {
			base.SetName(name);

			SetNameColour(UIManager.GetColour(UIManager.Colours.LightPurple100));
		}

		public override void Update() {
			base.Update();

			if (tradingPosts != null && tradingPosts.Count > 0) {
				ResourceManager.TradingPost tradingPost = tradingPosts[0];
				if (path.Count <= 0) {
					MoveToTile(tradingPost.zeroPointTile, false);
				}
				if (overTile == tradingPost.zeroPointTile) {
					foreach (ResourceManager.ReservedResources rr in tradingPost.GetInventory().TakeReservedResources(this)) {
						foreach (ResourceManager.ResourceAmount ra in rr.resources) {
							caravan.GetInventory().ChangeResourceAmount(ra.resource, ra.amount, false);

							ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount = caravan.confirmedResourcesToTrade.Find(crtt => crtt.resource == ra.resource);
							if (confirmedTradeResourceAmount != null) {
								confirmedTradeResourceAmount.amountRemaining += ra.amount;
							}
						}
					}
					tradingPosts.RemoveAt(0);
				}
				if (tradingPosts.Count <= 0) {
					JobManager.Job job = new JobManager.Job(
						tradingPost.tile,
						GameManager.resourceM.GetObjectPrefabByEnum(ResourceManager.ObjectEnum.CollectResources),
						null,
						0
					) {
						transferResources = new List<ResourceManager.ResourceAmount>()
					};
					foreach (ResourceManager.ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade) {
						if (confirmedTradeResourceAmount.tradeAmount > 0) {
							tradingPost.GetInventory().ChangeResourceAmount(confirmedTradeResourceAmount.resource, confirmedTradeResourceAmount.tradeAmount, false);
							confirmedTradeResourceAmount.amountRemaining = 0;
							job.transferResources.Add(new ResourceManager.ResourceAmount(confirmedTradeResourceAmount.resource, confirmedTradeResourceAmount.tradeAmount));
						}
					}
					caravan.confirmedResourcesToTrade.Clear();
					GameManager.jobM.CreateJob(job);
				}
			} else {
				if (path.Count <= 0) {
					Wander(caravan.targetTile, 4);
				} else {
					wanderTimer = UnityEngine.Random.Range(10f, 20f);
				}
			}
		}

		public override void Remove() {
			base.Remove();

			if (GameManager.humanM.selectedHuman == this) {
				GameManager.humanM.SetSelectedHuman(null);
			}
		}
	}

	public Location CreateLocation() {

		string name = GameManager.resourceM.GetRandomLocationName();

		List<Location.Wealth> wealthes = ((Location.Wealth[])Enum.GetValues(typeof(Location.Wealth))).ToList();
		Location.Wealth wealth = wealthes[UnityEngine.Random.Range(0, wealthes.Count)];

		List<Location.ResourceRichness> resourceRichnesses = ((Location.ResourceRichness[])Enum.GetValues(typeof(Location.ResourceRichness))).ToList();
		Location.ResourceRichness resourceRichness = resourceRichnesses[UnityEngine.Random.Range(0, resourceRichnesses.Count)];

		List<Location.CitySize> citySizes = ((Location.CitySize[])Enum.GetValues(typeof(Location.CitySize))).ToList();
		Location.CitySize citySize = citySizes[UnityEngine.Random.Range(0, citySizes.Count)];

		List<TileManager.Biome.TypeEnum> biomeTypes = ((TileManager.Biome.TypeEnum[])Enum.GetValues(typeof(TileManager.Biome.TypeEnum))).ToList();
		TileManager.Biome.TypeEnum biomeType = biomeTypes[UnityEngine.Random.Range(0, biomeTypes.Count)];

		return new Location(name, wealth, resourceRichness, citySize, biomeType);
	}

	public class Location {

		public enum Wealth {
			Destitute,
			Poor,
			Comfortable,
			Wealthy
		}

		public enum ResourceRichness {
			Sparse,
			Average,
			Abundant
		}

		public enum CitySize {
			Hamlet,
			Village,
			Town,
			City
		}

		public string name;
		public Wealth wealth;
		public ResourceRichness resourceRichness;
		public CitySize citySize;
		public TileManager.Biome.TypeEnum biomeType;

		public Location(
			string name, 
			Wealth wealth, 
			ResourceRichness resourceRichness, 
			CitySize citySize, 
			TileManager.Biome.TypeEnum biomeType
		) {
			this.name = name;
			this.wealth = wealth;
			this.resourceRichness = resourceRichness;
			this.citySize = citySize;
			this.biomeType = biomeType;
		}
	}
}