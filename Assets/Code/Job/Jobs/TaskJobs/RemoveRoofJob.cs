using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Command", "Remove", "RemoveRoof", true)]
	public class RemoveRoofJob : Job
	{
		protected RemoveRoofJob(TileManager.Tile tile) : base(tile) {
			TargetName = "Roof";
			Description = "Removing a roof.";
			Layer = 2;
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