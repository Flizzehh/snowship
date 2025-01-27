using System;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Create Resource")]
	public class CreateResourceJob : Job
	{
		private readonly CraftableResourceInstance createResource;

		protected CreateResourceJob(JobPrefab jobPrefab, TileManager.Tile tile, CraftableResourceInstance createResource) : base(jobPrefab, tile) {
			this.createResource = createResource;

			TargetName = createResource.resource.name;
			Description = $"Creating {createResource.resource.name}.";
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceAmount resourceAmount in RequiredResources) {
				Worker.GetInventory().ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
			}
			Worker.GetInventory().ChangeResourceAmount(createResource.resource, createResource.resource.amountCreated, false);

			switch (createResource.creationMethod) {
				case CraftableResourceInstance.CreationMethod.SingleRun:
					createResource.SetRemainingAmount(createResource.GetRemainingAmount() - createResource.resource.amountCreated);
					break;
				case CraftableResourceInstance.CreationMethod.MaintainStock:
					createResource.SetRemainingAmount(createResource.GetTargetAmount() - createResource.resource.GetAvailableAmount());
					break;
				case CraftableResourceInstance.CreationMethod.ContinuousRun:
					createResource.SetRemainingAmount(0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			createResource.JobInstance = null;
		}
	}
}