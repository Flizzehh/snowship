﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Snowship.NColonist {

	public class ColonistManager : IManager {

		private readonly List<Colonist> deadColonists = new List<Colonist>();

		public void Update() {
			UpdateColonists();
			UpdateColonistJobs();
		}

		private void UpdateColonists() {
			foreach (Colonist colonist in Colonist.colonists) {
				colonist.Update();
				if (colonist.dead) {
					deadColonists.Add(colonist);
				}
			}
			foreach (Colonist deadColonist in deadColonists) {
				deadColonist.Die();
			}
			deadColonists.Clear();
		}

		private void UpdateColonistJobs() {
			if (!GameManager.timeM.Time.Paused) {
				GameManager.jobM.GiveJobsToColonists();
			}
		}

		public void SpawnStartColonists(int amount) {
			SpawnColonists(amount);

			Vector2 averageColonistPosition = new Vector2(0, 0);
			foreach (Colonist colonist in Colonist.colonists) {
				averageColonistPosition = new Vector2(averageColonistPosition.x + colonist.obj.transform.position.x, averageColonistPosition.y + colonist.obj.transform.position.y);
			}
			averageColonistPosition /= Colonist.colonists.Count;
			GameManager.cameraM.SetCameraPosition(averageColonistPosition);

			// TODO TEMPORARY COLONIST TESTING STUFF
			Colonist.colonists[UnityEngine.Random.Range(0, Colonist.colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.WheatSeed), UnityEngine.Random.Range(5, 11), false);
			Colonist.colonists[UnityEngine.Random.Range(0, Colonist.colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.Potato), UnityEngine.Random.Range(5, 11), false);
			Colonist.colonists[UnityEngine.Random.Range(0, Colonist.colonists.Count)].GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(ResourceManager.ResourceEnum.CottonSeed), UnityEngine.Random.Range(5, 11), false);
		}

		public void SpawnColonists(int amount) {
			if (amount > 0) {
				int mapSize = GameManager.colonyM.colony.map.mapData.mapSize;
				for (int i = 0; i < amount; i++) {
					List<TileManager.Tile> walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && o.buildable && Colonist.colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
					if (walkableTilesByDistanceToCentre.Count <= 0) {
						foreach (TileManager.Tile tile in GameManager.colonyM.colony.map.tiles.Where(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f)) <= 4f)) {
							tile.SetTileType(tile.biome.tileTypes[TileManager.TileTypeGroup.TypeEnum.Ground], true, true, true);
						}
						walkableTilesByDistanceToCentre = GameManager.colonyM.colony.map.tiles.Where(o => o.walkable && Colonist.colonists.Find(c => c.overTile == o) == null).OrderBy(o => Vector2.Distance(o.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f))/*pathM.RegionBlockDistance(o.regionBlock,tileM.GetTileFromPosition(new Vector2(mapSize / 2f,mapSize / 2f)).regionBlock,true,true)*/).ToList();
					}

					List<TileManager.Tile> validSpawnTiles = new List<TileManager.Tile>();
					TileManager.Tile currentTile = walkableTilesByDistanceToCentre[0];
					float minimumDistance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
					foreach (TileManager.Tile tile in walkableTilesByDistanceToCentre) {
						float distance = Vector2.Distance(currentTile.obj.transform.position, new Vector2(mapSize / 2f, mapSize / 2f));
						if (distance < minimumDistance) {
							currentTile = tile;
							minimumDistance = distance;
						}
					}
					List<TileManager.Tile> frontier = new List<TileManager.Tile>() { currentTile };
					List<TileManager.Tile> checkedTiles = new List<TileManager.Tile>();
					while (frontier.Count > 0) {
						currentTile = frontier[0];
						frontier.RemoveAt(0);
						checkedTiles.Add(currentTile);
						validSpawnTiles.Add(currentTile);
						if (validSpawnTiles.Count > 100) {
							break;
						}
						foreach (TileManager.Tile nTile in currentTile.horizontalSurroundingTiles) {
							if (walkableTilesByDistanceToCentre.Contains(nTile) && !checkedTiles.Contains(nTile)) {
								frontier.Add(nTile);
							}
						}
					}
					TileManager.Tile colonistSpawnTile = validSpawnTiles.Count >= amount ? validSpawnTiles[UnityEngine.Random.Range(0, validSpawnTiles.Count)] : walkableTilesByDistanceToCentre[UnityEngine.Random.Range(0, (walkableTilesByDistanceToCentre.Count > 100 ? 100 : walkableTilesByDistanceToCentre.Count))];

					new Colonist(colonistSpawnTile, 1);
				}

				//GameManager.uiMOld.SetColonistElements();
				GameManager.colonyM.colony.map.Bitmasking(GameManager.colonyM.colony.map.tiles, true, true);
				GameManager.colonyM.colony.map.SetTileBrightness(GameManager.timeM.Time.TileBrightnessTime, true);
			}
		}
	}
}
