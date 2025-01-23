using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI.Simulation
{
	public class UIReservedResourcesElement : UIElement<UIReservedResourcesElementComponent>
	{
		public readonly ReservedResources ReservedResources;
		private readonly HumanManager.Human reserver;
		private readonly UIHumanBodyElement reserverBodyElement;

		public UIReservedResourcesElement(Transform parent, ReservedResources reservedResources) : base(parent) {
			ReservedResources = reservedResources;
			reserver = reservedResources.human;

			reserverBodyElement = Component.SetReserverBodyElement(reserver);
			reserver.OnClothingChanged += reserverBodyElement.SetClothingOnBodySection;

			Component.SetReserverNameText(reserver.name);

			Component.SetReservedCountText(reservedResources.resources.Count.ToString());
			foreach (ResourceAmount resourceAmount in reservedResources.resources) {
				Component.AddResourceAmountElement(resourceAmount);
			}
		}

		protected override void OnClose() {
			base.OnClose();

			reserver.OnClothingChanged -= reserverBodyElement.SetClothingOnBodySection;
		}
	}
}