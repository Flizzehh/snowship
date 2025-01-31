using System;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI
{
	public class UIClothingElement : UIElement<UIClothingElementComponent>
	{
		private readonly Clothing clothing;

		public event Action<Clothing> OnButtonClicked;

		public UIClothingElement(Transform parent, Clothing clothing) : base(parent) {
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