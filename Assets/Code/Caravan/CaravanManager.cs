using Snowship.NTime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Snowship.NCaravan {

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
						SpawnCaravan(CaravanType.Foot, 4);
					}
				}
			}
		}

		public List<Caravan> caravans = new List<Caravan>();

		public void SpawnCaravan(CaravanType caravanType, int maxNumTraders) {
			List<TileManager.Tile> validSpawnTiles = null;
			if (caravanType == CaravanType.Foot || caravanType == CaravanType.Wagon) {
				validSpawnTiles = GameManager.colonyM.colony.map.edgeTiles.Where(tile => tile.walkable && !tile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]).ToList();
			}
			else if (caravanType == CaravanType.Boat) {
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
						}
						else {
							List<TileManager.Tile> validTargetTiles = targetSpawnTile.region.tiles.Where(tile => tile.walkable && !tile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]).ToList();
							targetTile = validTargetTiles[UnityEngine.Random.Range(0, validTargetTiles.Count)];
						}

						AddCaravan(new Caravan(numTraders, CaravanType.Foot, edgeTilesCloseToTargetSpawnTile, targetTile));
					}
				}
			}
		}

		public void AddCaravan(Caravan caravan) {
			caravans.Add(caravan);

			GameManager.uiM.SetCaravanElements();
		}

		public Caravan selectedCaravan;

		public void SetSelectedCaravan(Caravan selectedCaravan) {
			this.selectedCaravan = selectedCaravan;

			GameManager.uiM.SetTradeMenu();
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
	}
}
