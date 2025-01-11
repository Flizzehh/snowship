using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PersistenceRiver {
		public int? riverIndex;

		public PRiver.RiverProperty? riverType;

		public Vector2? startTilePosition;
		public Vector2? centreTilePosition;
		public Vector2? endTilePosition;
		public int? expandRadius;
		public bool? ignoreStone;
		public List<Vector2> tilePositions;

		public List<Vector2> removedTilePositions;
		public List<Vector2> addedTilePositions;

		public PersistenceRiver(
			int? riverIndex,
			PRiver.RiverProperty? riverType,
			Vector2? startTilePosition,
			Vector2? centreTilePosition,
			Vector2? endTilePosition,
			int? expandRadius,
			bool? ignoreStone,
			List<Vector2> tilePositions,
			List<Vector2> removedTilePositions,
			List<Vector2> addedTilePositions
		) {
			this.riverIndex = riverIndex;

			this.riverType = riverType;

			this.startTilePosition = startTilePosition;
			this.centreTilePosition = centreTilePosition;
			this.endTilePosition = endTilePosition;
			this.expandRadius = expandRadius;
			this.ignoreStone = ignoreStone;
			this.tilePositions = tilePositions;

			this.removedTilePositions = removedTilePositions;
			this.addedTilePositions = addedTilePositions;
		}
	}
}
