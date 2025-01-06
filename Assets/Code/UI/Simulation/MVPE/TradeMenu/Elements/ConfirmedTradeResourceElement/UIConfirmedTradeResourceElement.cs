using Snowship.NResource.Models;
using Snowship.NUI.Generic;
using UnityEngine;

public class UIConfirmedTradeResourceElement : UIElement<UIConfirmedTradeResourceElementComponent> {

	private readonly ConfirmedTradeResourceAmount resourceAmount;

	public UIConfirmedTradeResourceElement(ConfirmedTradeResourceAmount resourceAmount, Transform parent) : base(parent) {
		this.resourceAmount = resourceAmount;

		Component.SetResource(resourceAmount);
	}

	// public override void OnUpdate() {
	// 	Component.UpdateCollectedAmount(resourceAmount);
	// }
}
