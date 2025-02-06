using System;
using Snowship.NResource;

namespace Snowship.NUI
{
	public class UIClothingElement : UIElement<UIClothingElementComponent>
	{
		private readonly Clothing clothing;

		public event Action<Clothing> OnButtonClicked;

		public UIClothingElement(Clothing clothing) {
			this.clothing = clothing;
		}

		protected override void OnCreate() {
			base.OnCreate();

			Component.SetClothingImage(clothing.image);
			Component.SetClothingNameText(clothing.name);
			Component.SetWaterResistanceValueText(clothing.prefab.waterResistance.ToString());
			Component.SetInsulationValueText(clothing.prefab.insulation.ToString());

			Component.OnButtonClicked += OnComponentButtonClicked;
		}

		private void OnComponentButtonClicked() {
			OnButtonClicked?.Invoke(clothing);
		}
	}
}