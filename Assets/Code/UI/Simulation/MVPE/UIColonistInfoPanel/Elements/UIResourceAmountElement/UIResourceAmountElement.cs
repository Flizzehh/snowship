using UnityEngine;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel.UIInventoryResource
{
	public class UIResourceAmountElement : UIElement<UIResourceAmountElementComponent>
	{
		public UIResourceAmountElement(Transform parent, ResourceManager.ResourceAmount resourceAmount) : base(parent) {
			Component.SetResourceAmount(resourceAmount);
		}
	}
}
