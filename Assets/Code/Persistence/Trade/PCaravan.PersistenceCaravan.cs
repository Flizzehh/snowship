using System.Collections.Generic;
using Snowship.NCaravan;
using UnityEngine;

namespace Snowship.NPersistence {
	public partial class PCaravan {

		public class PersistenceCaravan {

			public CaravanType? type;
			public Location location;
			public Vector2? targetTilePosition;
			public ResourceManager.ResourceGroup resourceGroup;
			public int? leaveTimer;
			public bool? leaving;
			public PInventory.PersistenceInventory persistenceInventory;
			public List<PersistenceTradeResourceAmount> persistenceResourcesToTrade;
			public List<PersistenceConfirmedTradeResourceAmount> persistenceConfirmedResourcesToTrade;
			public List<PersistenceTrader> persistenceTraders;

			public PersistenceCaravan(
				CaravanType? type,
				Location location,
				Vector2? targetTilePosition,
				ResourceManager.ResourceGroup resourceGroup,
				int? leaveTimer,
				bool? leaving,
				PInventory.PersistenceInventory persistenceInventory,
				List<PersistenceTradeResourceAmount> persistenceResourcesToTrade,
				List<PersistenceConfirmedTradeResourceAmount> persistenceConfirmedResourcesToTrade,
				List<PersistenceTrader> persistenceTraders
			) {
				this.type = type;
				this.location = location;
				this.targetTilePosition = targetTilePosition;
				this.resourceGroup = resourceGroup;
				this.leaveTimer = leaveTimer;
				this.leaving = leaving;
				this.persistenceInventory = persistenceInventory;
				this.persistenceResourcesToTrade = persistenceResourcesToTrade;
				this.persistenceConfirmedResourcesToTrade = persistenceConfirmedResourcesToTrade;
				this.persistenceTraders = persistenceTraders;
			}

		}

	}
}
