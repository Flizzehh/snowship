using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PRiver : PersistenceHandler {

		public enum RiverProperty {
			River,
			Index,
			Type,
			SmallRiver,
			LargeRiver,
			StartTilePosition,
			CentreTilePosition,
			EndTilePosition,
			ExpandRadius,
			IgnoreStone,
			TilePositions,
			AddedTilePositions,
			RemovedTilePositions
		}

		public void SaveOriginalRivers(StreamWriter file) {
			foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.rivers) {
				WriteOriginalRiverLines(file, river, 0, RiverProperty.SmallRiver);
			}
			foreach (TileManager.Map.River river in GameManager.colonyM.colony.map.largeRivers) {
				WriteOriginalRiverLines(file, river, 0, RiverProperty.LargeRiver);
			}
		}

		private void WriteOriginalRiverLines(StreamWriter file, TileManager.Map.River river, int startLevel, RiverProperty riverType) {
			file.WriteLine(CreateKeyValueString(RiverProperty.River, string.Empty, startLevel));

			file.WriteLine(CreateKeyValueString(RiverProperty.Type, riverType, startLevel + 1));
			file.WriteLine(CreateKeyValueString(RiverProperty.StartTilePosition, FormatVector2ToString(river.startTile.position), startLevel + 1));
			if (river.centreTile != null) {
				file.WriteLine(CreateKeyValueString(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.position), startLevel + 1));
			}
			file.WriteLine(CreateKeyValueString(RiverProperty.EndTilePosition, FormatVector2ToString(river.endTile.position), startLevel + 1));

			file.WriteLine(CreateKeyValueString(RiverProperty.ExpandRadius, river.expandRadius, startLevel + 1));
			file.WriteLine(CreateKeyValueString(RiverProperty.IgnoreStone, river.ignoreStone, startLevel + 1));
			file.WriteLine(CreateKeyValueString(RiverProperty.TilePositions, string.Join(";", river.tiles.Select(t => FormatVector2ToString(t.position)).ToArray()), startLevel + 1));
		}



		public List<PersistenceRiver> LoadRivers(string path) {
			List<PersistenceRiver> rivers = new List<PersistenceRiver>();

			List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((RiverProperty)Enum.Parse(typeof(RiverProperty), property.Key)) {
					case RiverProperty.River:
						int? riverIndex = null;
						RiverProperty? riverType = null;
						Vector2? startTilePosition = null;
						Vector2? centreTilePosition = null;
						Vector2? endTilePosition = null;
						int? expandRadius = null;
						bool? ignoreStone = null;
						List<Vector2> tilePositions = new List<Vector2>();
						List<Vector2> removedTilePositions = new List<Vector2>();
						List<Vector2> addedTilePositions = new List<Vector2>();

						foreach (KeyValuePair<string, object> riverProperty in (List<KeyValuePair<string, object>>)property.Value) {
							switch ((RiverProperty)Enum.Parse(typeof(RiverProperty), riverProperty.Key)) {
								case RiverProperty.Index:
									riverIndex = int.Parse((string)riverProperty.Value);
									break;
								case RiverProperty.Type:
									riverType = (RiverProperty)Enum.Parse(typeof(RiverProperty), (string)riverProperty.Value);
									break;
								case RiverProperty.StartTilePosition:
									startTilePosition = new Vector2(float.Parse(((string)riverProperty.Value).Split(',')[0]), float.Parse(((string)riverProperty.Value).Split(',')[1]));
									break;
								case RiverProperty.CentreTilePosition:
									if (!((string)riverProperty.Value).Contains("None")) {
										centreTilePosition = new Vector2(float.Parse(((string)riverProperty.Value).Split(',')[0]), float.Parse(((string)riverProperty.Value).Split(',')[1]));
									}
									break;
								case RiverProperty.EndTilePosition:
									endTilePosition = new Vector2(float.Parse(((string)riverProperty.Value).Split(',')[0]), float.Parse(((string)riverProperty.Value).Split(',')[1]));
									break;
								case RiverProperty.ExpandRadius:
									expandRadius = int.Parse((string)riverProperty.Value);
									break;
								case RiverProperty.IgnoreStone:
									ignoreStone = bool.Parse((string)riverProperty.Value);
									break;
								case RiverProperty.TilePositions:
									foreach (string vector2String in ((string)riverProperty.Value).Split(';')) {
										tilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
									}
									break;
								case RiverProperty.RemovedTilePositions:
									foreach (string vector2String in ((string)riverProperty.Value).Split(';')) {
										removedTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
									}
									break;
								case RiverProperty.AddedTilePositions:
									foreach (string vector2String in ((string)riverProperty.Value).Split(';')) {
										addedTilePositions.Add(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
									}
									break;
								default:
									Debug.LogError("Unknown river property: " + riverProperty.Key + " " + riverProperty.Value);
									break;
							}
						}

						rivers.Add(new PersistenceRiver(riverIndex, riverType, startTilePosition, centreTilePosition, endTilePosition, expandRadius, ignoreStone, tilePositions, removedTilePositions, addedTilePositions));
						break;
					default:
						Debug.LogError("Unknown river property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return rivers;
		}

		public void SaveModifiedRivers(string saveDirectoryPath, List<PersistenceRiver> originalRivers) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "rivers.snowship");

			TileManager.Map map = GameManager.colonyM.colony.map;
			int numRivers = map.rivers.Count + map.largeRivers.Count;
			if (originalRivers.Count != numRivers) {
				Debug.LogError("Loaded river count " + originalRivers.Count + " and current river count " + numRivers + " does not match.");
			}

			List<PersistenceRiver> originalSmallRivers = originalRivers.Where(river => river.riverType == RiverProperty.SmallRiver).ToList();
			if (originalSmallRivers.Count != map.rivers.Count) {
				Debug.LogError("Loaded small river count " + originalSmallRivers.Count + " and current small river count " + map.rivers.Count + " does not match.");
			}
			for (int i = 0; i < originalSmallRivers.Count; i++) {
				WriteModifiedRiverLines(file, map.rivers[i], originalSmallRivers[i], i);
			}

			List<PersistenceRiver> originalLargeRivers = originalRivers.Where(river => river.riverType == RiverProperty.LargeRiver).ToList();
			if (originalLargeRivers.Count != map.largeRivers.Count) {
				Debug.LogError("Loaded large river count " + originalLargeRivers.Count + " and current large river count " + map.largeRivers.Count + " does not match.");
			}
			for (int i = 0; i < originalLargeRivers.Count; i++) {
				WriteModifiedRiverLines(file, map.rivers[i], originalSmallRivers[i], i);
			}

			file.Close();
		}

		public void WriteModifiedRiverLines(StreamWriter file, TileManager.Map.River river, PersistenceRiver originalRiver, int index) {
			Dictionary<RiverProperty, string> riverDifferences = new Dictionary<RiverProperty, string>();

			if (river.startTile.position != originalRiver.startTilePosition.Value) {
				riverDifferences.Add(RiverProperty.StartTilePosition, FormatVector2ToString(river.startTile.position));
			}

			if (river.centreTile == null) {
				if (originalRiver.centreTilePosition.HasValue) { // No original centre tile, centre tile was added
					riverDifferences.Add(RiverProperty.CentreTilePosition, "None");
				}
			} else {
				if (!originalRiver.centreTilePosition.HasValue) { // Original centre tile, centre tile was removed
					riverDifferences.Add(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.position));
				} else { // Centre tile has remained, properties potentially changed
					if (river.centreTile.position != originalRiver.centreTilePosition.Value) {
						riverDifferences.Add(RiverProperty.CentreTilePosition, FormatVector2ToString(river.centreTile.position));
					}
				}
			}

			if (river.endTile.position != originalRiver.endTilePosition.Value) {
				riverDifferences.Add(RiverProperty.EndTilePosition, FormatVector2ToString(river.endTile.position));
			}

			if (river.expandRadius != originalRiver.expandRadius.Value) {
				riverDifferences.Add(RiverProperty.ExpandRadius, river.expandRadius.ToString());
			}

			if (river.ignoreStone != originalRiver.ignoreStone.Value) {
				riverDifferences.Add(RiverProperty.IgnoreStone, river.ignoreStone.ToString());
			}

			List<Vector2> riverTilePositions = river.tiles.Select(riverTile => riverTile.position).ToList();

			List<Vector2> addedRiverTiles = new List<Vector2>();
			foreach (Vector2 riverTilePosition in riverTilePositions) {
				if (!originalRiver.tilePositions.Contains(riverTilePosition)) {
					addedRiverTiles.Add(riverTilePosition);
				}
			}
			if (addedRiverTiles.Count > 0) {
				riverDifferences.Add(RiverProperty.AddedTilePositions, string.Join(";", addedRiverTiles.Select(v2 => FormatVector2ToString(v2)).ToArray()));
			}

			List<Vector2> removedRiverTiles = new List<Vector2>();
			foreach (Vector2 originalRiverTilePosition in originalRiver.tilePositions) {
				if (!riverTilePositions.Contains(originalRiverTilePosition)) {
					removedRiverTiles.Add(originalRiverTilePosition);
				}
			}
			if (removedRiverTiles.Count > 0) {
				riverDifferences.Add(RiverProperty.RemovedTilePositions, string.Join(";", removedRiverTiles.Select(v2 => FormatVector2ToString(v2)).ToArray()));
			}

			if (riverDifferences.Count > 0) {
				file.WriteLine(CreateKeyValueString(RiverProperty.River, string.Empty, 0));
				file.WriteLine(CreateKeyValueString(RiverProperty.Index, index, 1));
				foreach (KeyValuePair<RiverProperty, string> riverProperty in riverDifferences) {
					file.WriteLine(CreateKeyValueString(riverProperty.Key, riverProperty.Value, 1));
				}
			}
		}

		public void ApplyLoadedRivers(List<PersistenceRiver> originalRivers, List<PersistenceRiver> modifiedRivers, TileManager.Map map) {
			List<TileManager.Map.River> riverList = null;
			for (int i = 0; i < originalRivers.Count; i++) {
				PersistenceRiver originalRiver = originalRivers[i];
				PersistenceRiver modifiedRiver = modifiedRivers.Find(mr => mr.riverIndex == i);

				switch (modifiedRiver != null && modifiedRiver.riverType.HasValue ? modifiedRiver.riverType.Value : originalRiver.riverType.Value) {
					case RiverProperty.SmallRiver:
						riverList = map.rivers;
						break;
					case RiverProperty.LargeRiver:
						riverList = map.largeRivers;
						break;
					default:
						Debug.LogError("Invalid river type.");
						break;
				}

				List<TileManager.Tile> riverTiles = new List<TileManager.Tile>();
				foreach (Vector2 riverTilePosition in originalRiver.tilePositions) {
					riverTiles.Add(map.GetTileFromPosition(riverTilePosition));
				}

				if (modifiedRiver != null) {
					foreach (Vector2 removedTilePosition in modifiedRiver.removedTilePositions) {
						riverTiles.Remove(map.GetTileFromPosition(removedTilePosition));
					}
					foreach (Vector2 addedTilePosition in modifiedRiver.addedTilePositions) {
						riverTiles.Add(map.GetTileFromPosition(addedTilePosition));
					}
				}

				riverList.Add(
					new TileManager.Map.River(
						modifiedRiver != null && modifiedRiver.startTilePosition.HasValue ? map.GetTileFromPosition(modifiedRiver.startTilePosition.Value) : (originalRiver.startTilePosition.HasValue ? map.GetTileFromPosition(originalRiver.startTilePosition.Value) : null),
						modifiedRiver != null && modifiedRiver.centreTilePosition.HasValue ? map.GetTileFromPosition(modifiedRiver.centreTilePosition.Value) : (originalRiver.centreTilePosition.HasValue ? map.GetTileFromPosition(originalRiver.centreTilePosition.Value) : null),
						modifiedRiver != null && modifiedRiver.endTilePosition.HasValue ? map.GetTileFromPosition(modifiedRiver.endTilePosition.Value) : (originalRiver.endTilePosition.HasValue ? map.GetTileFromPosition(originalRiver.endTilePosition.Value) : null),
						modifiedRiver != null && modifiedRiver.expandRadius.HasValue ? modifiedRiver.expandRadius.Value : originalRiver.expandRadius.Value,
						modifiedRiver != null && modifiedRiver.ignoreStone.HasValue ? modifiedRiver.ignoreStone.Value : originalRiver.ignoreStone.Value,
						map,
						false
					) { tiles = riverTiles }
				);
			}
		}

	}
}
