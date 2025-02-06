using System;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Craft", "CreateResource")]
	public class CreateResourceJobDefinition : JobDefinition
	{
		public CreateResourceJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
		}
	}

	public class CreateResourceJob : Job<CreateResourceJobDefinition>
	{
		public readonly CraftingObject CraftingObject;
		public readonly CraftableResourceInstance CreateResource;

		public CreateResourceJob(CraftingObject craftingObject, CraftableResourceInstance createResource) : base(craftingObject.tile) {
			CraftingObject = craftingObject;
			CreateResource = createResource;

			TargetName = createResource.resource.name;
			Description = $"Creating {createResource.resource.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceAmount resourceAmount in RequiredResources) {
				Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
			}
			Worker.Inventory.ChangeResourceAmount(CreateResource.resource, CreateResource.resource.amountCreated, false);

			switch (CreateResource.creationMethod) {
				case CraftableResourceInstance.CreationMethod.SingleRun:
					CreateResource.SetRemainingAmount(CreateResource.GetRemainingAmount() - CreateResource.resource.amountCreated);
					break;
				case CraftableResourceInstance.CreationMethod.MaintainStock:
					CreateResource.SetRemainingAmount(CreateResource.GetTargetAmount() - CreateResource.resource.GetAvailableAmount());
					break;
				case CraftableResourceInstance.CreationMethod.ContinuousRun:
					CreateResource.SetRemainingAmount(0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			CreateResource.Job = null;
		}
	}
}