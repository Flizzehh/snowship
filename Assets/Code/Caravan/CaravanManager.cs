using Snowship.NTime;
using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUI.Simulation.TradeMenu;
using UnityEngine;

namespace Snowship.NCaravan {

	public class CaravanManager : IManager {

		public List<Caravan> caravans = new List<Caravan>();
		private readonly List<Caravan> removeCaravans = new List<Caravan>();

		public int caravanTimer = 0; // The time since the last caravan visited
		private static readonly int CaravanTimeMin = TimeManager.dayLengthSeconds * 7; // Caravans will only come once every caravanTimeMin in-game minutes
		private static readonly int CaravanTimeMax = TimeManager.dayLengthSeconds * 27; // Caravans will definitely come if the caravanTimer is greater than caravanTimeMax

		public Caravan selectedCaravan;

		public void Update() {
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

				GameManager.uiMOld.SetCaravanElements();
			}

			removeCaravans.Clear();

			if (selectedCaravan != null && Input.GetMouseButtonDown(1)) {
				SetSelectedCaravan(null);
			}
		}

		private void UpdateCaravanTimer() {
			if (GameManager.timeM.GetPaused() || !GameManager.timeM.minuteChanged) {
				return;
			}

			caravanTimer += 1;

			if (caravanTimer <= CaravanTimeMin) {
				return;
			}

			if (UnityEngine.Random.Range(0f, 1f) < (((float)caravanTimer - CaravanTimeMin) / (CaravanTimeMax - CaravanTimeMin))) {
				caravanTimer = 0;
				SpawnCaravan(CaravanType.Foot, 4);
			}
		}

		public void SpawnCaravan(CaravanType caravanType, int maxNumTraders) {

			List<TileManager.Tile> validSpawnTiles = new List<TileManager.Tile>();

			switch (caravanType) {
				case CaravanType.Foot or CaravanType.Wagon:
					validSpawnTiles.AddRange(GameManager.colonyM.colony.map.edgeTiles.Where(tile => tile.walkable && !tile.tileType.classes[TileManager.TileType.ClassEnum.LiquidWater]).ToList());
					break;
				case CaravanType.Boat:
					// TODO Implement boat caravans
					break;
			}

			if (validSpawnTiles.Count <= 0) {
				return;
			}

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

			if (edgeTilesConnectedToTargetSpawnTile.Count <= 0) {
				return;
			}

			int numTraders = UnityEngine.Random.Range(1, maxNumTraders + 1);
			List<TileManager.Tile> edgeTilesCloseToTargetSpawnTile = edgeTilesConnectedToTargetSpawnTile.Where(tile => Vector2.Distance(tile.obj.transform.position, targetSpawnTile.obj.transform.position) <= numTraders * 2).ToList();

			if (edgeTilesCloseToTargetSpawnTile.Count <= 0) {
				return;
			}

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

		public void AddCaravan(Caravan caravan) {
			caravans.Add(caravan);

			GameManager.uiMOld.SetCaravanElements();
		}

		public void SetSelectedCaravan(Caravan selectedCaravan) {
			this.selectedCaravan = selectedCaravan;

			_ = GameManager.uiM.OpenViewAsync<UITradeMenu>(null, true);
		}
	}
}
