using System;
using UnityEngine;

namespace Snowship.NUI.Simulation
{
	public class UIClothingElement : UIElement<UIClothingElementComponent>
	{
		private readonly ResourceManager.Clothing clothing;

		public event Action<ResourceManager.Clothing> OnButtonClicked;

		public UIClothingElement(Transform parent, ResourceManager.Clothing clothing) : base(parent) {
			this.clothing = clothing;

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
