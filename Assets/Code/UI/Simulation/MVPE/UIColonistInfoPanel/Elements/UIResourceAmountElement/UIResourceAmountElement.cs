using System;
using Snowship.NResource;

namespace Snowship.NUI
{
	public class UIResourceAmountElement : UIElement<UIResourceAmountElementComponent>
	{
		public readonly ResourceAmount ResourceAmount;
		public event Action<UIResourceAmountElement> OnAmountChanged;

		public UIResourceAmountElement(ResourceAmount resourceAmount) {
			ResourceAmount = resourceAmount;
		}

		protected override void OnCreate() {
			base.OnCreate();

			Component.SetResourceImage(ResourceAmount.Resource.image);
			Component.SetResourceName(ResourceAmount.Resource.name);

			ResourceAmount.OnAmountChanged += OnResourceAmountChanged;
			OnResourceAmountChanged(ResourceAmount.Amount);
		}

		protected override void OnClose() {
			base.OnClose();
			ResourceAmount.OnAmountChanged -= OnResourceAmountChanged;
		}

		private void OnResourceAmountChanged(int amount) {
			Component.SetResourceAmount(amount.ToString());
			OnAmountChanged?.Invoke(this);
		}
	}
}