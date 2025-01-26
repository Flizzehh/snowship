using System.Collections.Generic;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NCaravan {
	public class Trader : HumanManager.Human {

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

			if (tradingPosts != null && tradingPosts.Count > 0) {
				TradingPost tradingPost = tradingPosts[0];
				if (path.Count <= 0) {
					MoveToTile(tradingPost.zeroPointTile, false);
				}

				if (overTile == tradingPost.zeroPointTile) {
					foreach (ReservedResources rr in tradingPost.GetInventory().TakeReservedResources(this)) {
						foreach (ResourceAmount ra in rr.resources) {
							caravan.GetInventory().ChangeResourceAmount(ra.Resource, ra.Amount, false);

							ConfirmedTradeResourceAmount confirmedTradeResourceAmount = caravan.confirmedResourcesToTrade.Find(crtt => crtt.resource == ra.Resource);
							if (confirmedTradeResourceAmount != null) {
								confirmedTradeResourceAmount.amountRemaining += ra.Amount;
							}
						}
					}

					tradingPosts.RemoveAt(0);
				}

				if (tradingPosts.Count <= 0) {
					Job job = new Job(
						JobPrefab.GetJobPrefabByName("CollectResources"),
						tradingPost.tile,
						ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.CollectResources),
						null,
						0
					) {
						transferResources = new List<ResourceAmount>()
					};
					foreach (ConfirmedTradeResourceAmount confirmedTradeResourceAmount in caravan.confirmedResourcesToTrade) {
						if (confirmedTradeResourceAmount.tradeAmount > 0) {
							tradingPost.GetInventory().ChangeResourceAmount(confirmedTradeResourceAmount.resource, confirmedTradeResourceAmount.tradeAmount, false);
							confirmedTradeResourceAmount.amountRemaining = 0;
							job.transferResources.Add(new ResourceAmount(confirmedTradeResourceAmount.resource, confirmedTradeResourceAmount.tradeAmount));
						}
					}

					caravan.confirmedResourcesToTrade.Clear();
					GameManager.Get<JobManager>().CreateJob(job);
				}
			}
			else {
				if (path.Count <= 0) {
					Wander(caravan.targetTile, 4);
				}
				else {
					wanderTimer = UnityEngine.Random.Range(10f, 20f);
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