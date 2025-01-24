using System.Collections.Generic;
using Snowship.NColony;
using Snowship.NTime;
using UnityEngine;

namespace Snowship.NResource
{
	public class LightSource : ObjectInstance
	{
		public static List<LightSource> lightSources = new();

		public List<TileManager.Tile> litTiles = new();

		public LightSource(ObjectPrefab prefab, Variation variation, TileManager.Tile tile, int rotationIndex) : base(prefab, variation, tile, rotationIndex) {
			SetTileBrightnesses();
		}

		public void SetTileBrightnesses() {
			RemoveTileBrightnesses();
			List<TileManager.Tile> newLitTiles = new();
			foreach (TileManager.Tile tile in GameManager.Get<ColonyManager>().colony.map.tiles) {
				float distance = Vector2.Distance(tile.obj.transform.position, this.tile.obj.transform.position);
				if (distance <= prefab.maxLightDistance) {
					float intensityAtTile = Mathf.Clamp(prefab.maxLightDistance * (1f / Mathf.Pow(distance, 2f)), 0f, 1f);
					if (tile != this.tile) {
						bool lightTile = true;
						Vector3 lightVector = obj.transform.position;
						while ((obj.transform.position - lightVector).magnitude <= distance) {
							TileManager.Tile lightVectorTile = GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(lightVector);
							if (lightVectorTile != this.tile) {
								if (lightVectorTile.blocksLight /*GameManager.Get<ColonyManager>().colony.map.TileBlocksLight(lightVectorTile)*/) {
									/*
									if (!lightVectorTile.horizontalSurroundingTiles.Any(t => newLitTiles.Contains(t) && !tileM.map.TileBlocksLight(t))) {
										lightTile = false;
										break;
									}
									*/
									lightTile = false;
									break;
								}
							}
							lightVector += (tile.obj.transform.position - obj.transform.position).normalized * 0.1f;
						}
						if (lightTile) {
							tile.AddLightSourceBrightness(this, intensityAtTile);
							newLitTiles.Add(tile);
						}
					} else {
						this.tile.AddLightSourceBrightness(this, intensityAtTile);
					}
				}
			}
			GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
			litTiles.AddRange(newLitTiles);
		}

		public void RemoveTileBrightnesses() {
			foreach (TileManager.Tile tile in litTiles) {
				tile.RemoveLightSourceBrightness(this);
			}
			litTiles.Clear();
			tile.RemoveLightSourceBrightness(this);
			GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(GameManager.Get<TimeManager>().Time.TileBrightnessTime, true);
		}
	}
}