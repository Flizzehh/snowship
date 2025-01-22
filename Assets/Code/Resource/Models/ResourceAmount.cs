using System;

namespace Snowship.NResources
{
	public class ResourceAmount
	{
		public readonly ResourceManager.Resource Resource;

		private int amount;
		public int Amount {
			get => amount;
			set => SetAmount(value);
		}

		public event Action<int> OnAmountChanged;

		public ResourceAmount(ResourceManager.Resource resource, int amount) {
			Resource = resource;
			Amount = amount;
		}

		private void SetAmount(int value) {
			amount = value;
			OnAmountChanged?.Invoke(amount);
		}
	}
}
