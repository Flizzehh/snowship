using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Remove Roof")]
	public class RemoveRoofJob : Job
	{
		protected RemoveRoofJob(JobPrefab jobPrefab, TileManager.Tile tile) : base(jobPrefab, tile) {
			TargetName = "Roof";
			Description = "Removing a roof.";
		}

		public override void OnJobTaken() {
			base.OnJobTaken();

			if (!Tile.HasRoof()) {
				ShouldBeCancelled = true;
				// TODO Check after calling the OnJobTaken method in JobManager to know whether it should be cancelled
			}
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			if (!Tile.HasRoof()) {
				return;
			}

			Tile.SetRoof(false);
			foreach (ResourceAmount resourceAmount in ObjectPrefab.GetObjectPrefabByEnum(ObjectPrefab.ObjectEnum.Roof).commonResources) {
				Worker.GetInventory().ChangeResourceAmount(resourceAmount.Resource, Mathf.RoundToInt(resourceAmount.Amount), false);
			}
		}
	}
}