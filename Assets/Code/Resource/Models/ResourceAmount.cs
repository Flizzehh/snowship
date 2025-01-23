using System;

namespace Snowship.NResource
{
	public class ResourceAmount
	{
		public readonly Resource Resource;

		private int amount;
		public int Amount {
			get => amount;
			set => SetAmount(value);
		}

		public event Action<int> OnAmountChanged;

		public ResourceAmount(Resource resource, int amount) {
			Resource = resource;
			Amount = amount;
		}

		private void SetAmount(int value) {
			amount = value;
			OnAmountChanged?.Invoke(amount);
		}
	}
}
