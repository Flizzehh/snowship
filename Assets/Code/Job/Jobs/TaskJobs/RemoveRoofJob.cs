using System;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Remove", "Remove", "RemoveRoof")]
	public class RemoveRoofJobDefinition : JobDefinition<RemoveRoofJob>
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Roof,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public RemoveRoofJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
			Layer = 2;
		}
	}

	public class RemoveRoofJob : Job<RemoveRoofJobDefinition>
	{
		public RemoveRoofJob(TileManager.Tile tile) : base(tile) {
			TargetName = "Roof";
			Description = "Removing a roof.";
		}

		protected override void OnJobTaken() {
			base.OnJobTaken();

			if (!Tile.HasRoof()) {
				ShouldBeCancelled = true;
				// TODO Check after calling the OnJobTaken method in JobManager to know whether it should be cancelled
			}
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (!Tile.HasRoof()) {
				return;
			}

			Tile.SetRoof(false);
			foreach (ResourceAmount resourceAmount in ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.Roof).commonResources) {
				Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, Mathf.RoundToInt(resourceAmount.Amount), false);
			}
		}
	}
}