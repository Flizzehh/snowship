using System;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI
{
	public class UIResourceAmountElement : UIElement<UIResourceAmountElementComponent>
	{
		public readonly ResourceAmount ResourceAmount;
		public event Action<UIResourceAmountElement> OnAmountChanged;

		public UIResourceAmountElement(Transform parent, ResourceAmount resourceAmount) : base(parent) {
			ResourceAmount = resourceAmount;

			Component.SetResourceImage(resourceAmount.Resource.image);
			Component.SetResourceName(resourceAmount.Resource.name);

			resourceAmount.OnAmountChanged += OnResourceAmountChanged;
			OnResourceAmountChanged(resourceAmount.Amount);
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