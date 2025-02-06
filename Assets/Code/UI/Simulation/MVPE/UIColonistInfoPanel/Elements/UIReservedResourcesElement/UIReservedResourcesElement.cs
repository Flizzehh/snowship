using Snowship.NHuman;
using Snowship.NResource;

namespace Snowship.NUI
{
	public class UIReservedResourcesElement : UIElement<UIReservedResourcesElementComponent>
	{
		public readonly ReservedResources ReservedResources;
		private readonly Human reserver;

		private UIHumanBodyElement reserverBodyElement;

		public UIReservedResourcesElement(ReservedResources reservedResources) {
			ReservedResources = reservedResources;
			reserver = reservedResources.human;
		}

		protected override void OnCreate() {
			base.OnCreate();

			reserverBodyElement = Component.SetReserverBodyElement(reserver);
			reserver.OnClothingChanged += reserverBodyElement.SetClothingOnBodySection;

			Component.SetReserverNameText(reserver.Name);

			Component.SetReservedCountText(ReservedResources.resources.Count.ToString());
			foreach (ResourceAmount resourceAmount in ReservedResources.resources) {
				Component.AddResourceAmountElement(resourceAmount);
			}
		}

		protected override void OnClose() {
			base.OnClose();

			reserver.OnClothingChanged -= reserverBodyElement.SetClothingOnBodySection;
		}
	}
}