using Snowship.NResource;
using Snowship.NUI;

public class UIConfirmedTradeResourceElement : UIElement<UIConfirmedTradeResourceElementComponent> {

	private readonly ConfirmedTradeResourceAmount resourceAmount;

	public UIConfirmedTradeResourceElement(ConfirmedTradeResourceAmount resourceAmount) {
		this.resourceAmount = resourceAmount;
	}

	protected override void OnCreate() {
		base.OnCreate();

		Component.SetResource(resourceAmount);
	}
}