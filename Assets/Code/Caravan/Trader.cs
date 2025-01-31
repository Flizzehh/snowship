using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NCaravan {
	public class Trader : Human
	{

		public Caravan caravan;

		public TileManager.Tile leaveTile;

		public List<TradingPost> tradingPosts;

		public Trader(TileManager.Tile spawnTile, float startingHealth, Caravan caravan) : base(spawnTile, startingHealth) {
			this.caravan = caravan;

			obj.transform.SetParent(GameManager.SharedReferences.LifeParent, false);
		}

		public override void SetName(string name) {
			base.SetName(name);

			SetNameColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightPurple100));
		}

		public override void Update() {
			base.Update();

			if (tradingPosts is { Count: > 0 }) {
				TradingPost tradingPost = tradingPosts[0];
				if (path.Count <= 0) {
					MoveToTile(tradingPost.zeroPointTile, false);
				}

				if (overTile == tradingPost.zeroPointTile) {
					foreach (ReservedResources rr in tradingPost.Inventory.TakeReservedResourcesWithoutTransfer(this)) {
						foreach (ResourceAmount ra in rr.resources) {
							caravan.Inventory.ChangeResourceAmount(ra.Resource, ra.Amount, false);

							ConfirmedTradeResourceAmount confirmedTradeResourceAmount = caravan.confirmedResourcesToTrade.Find(crtt => crtt.resource == ra.Resource);
							if (confirmedTradeResourceAmount != null) {
								confirmedTradeResourceAmount.amountRemaining += ra.Amount;
							}
						}
					}

					tradingPosts.RemoveAt(0);
				}

				if (tradingPosts.Count <= 0) {
					List<ResourceAmount> transferResources = caravan.confirmedResourcesToTrade
						.Where(ctra => ctra.tradeAmount > 0)
						.Select(
							ctra => {
								tradingPost.Inventory.ChangeResourceAmount(ctra.resource, ctra.tradeAmount, false);
								ctra.amountRemaining = 0;
								return new ResourceAmount(ctra.resource, ctra.tradeAmount);
							}
						)
						.ToList();
					Job job = new CollectResourcesJob(tradingPost, transferResources);

					caravan.confirmedResourcesToTrade.Clear();
					GameManager.Get<JobManager>().AddJob(job);
				}
			} else {
				if (path.Count <= 0) {
					Wander(caravan.targetTile, 4);
				} else {
					WanderTimer = UnityEngine.Random.Range(10f, 20f);
				}
			}
		}

		public override void Remove() {
			base.Remove();

			if (GameManager.Get<HumanManager>().selectedHuman == this) {
				GameManager.Get<HumanManager>().SetSelectedHuman(null);
			}
		}
	}
}