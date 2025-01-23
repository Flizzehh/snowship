using System.Collections.Generic;
using Snowship.NJob;
using Snowship.NUtilities;

namespace Snowship.NResource
{
	public class CraftableResourceInstance
	{
		public Resource resource;

		public Priority priority;

		public CreationMethod creationMethod;
		private int targetAmount;
		private int remainingAmount;

		public CraftingObject craftingObject;

		public bool enableable = false;
		public List<ResourceAmount> fuelAmounts = new();
		public Job job = null;

		public CraftableResourceInstance(
			Resource resource,
			int priority,
			CreationMethod creationMethod,
			int targetAmount,
			CraftingObject craftingObject,
			int? remainingAmount = null
		) {
			this.resource = resource;

			this.priority = new Priority(priority);

			this.creationMethod = creationMethod;
			this.targetAmount = targetAmount;
			this.remainingAmount = remainingAmount == null ? targetAmount : remainingAmount.Value;

			this.craftingObject = craftingObject;
		}

		public void SetTargetAmount(int targetAmount) {
			remainingAmount = targetAmount;
			this.targetAmount = targetAmount;
		}

		public void UpdateTargetAmount(int targetAmount) {
			remainingAmount += remainingAmount == 0 ? targetAmount : targetAmount - this.targetAmount;
			this.targetAmount = targetAmount;
		}

		public int GetTargetAmount() {
			return targetAmount;
		}

		public int GetRemainingAmount() {
			return remainingAmount;
		}

		public void SetRemainingAmount(int remainingAmount) {
			this.remainingAmount = remainingAmount;
		}

		public void ResetAmounts() {
			SetRemainingAmount(0);
			SetTargetAmount(0);
		}

		public enum CreationMethod
		{
			SingleRun,
			MaintainStock,
			ContinuousRun
		}
	}
}
