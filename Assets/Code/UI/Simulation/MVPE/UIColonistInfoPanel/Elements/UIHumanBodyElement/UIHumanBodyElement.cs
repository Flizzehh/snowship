using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI.Simulation
{
	[UsedImplicitly]
	public class UIHumanBodyElement : UIElement<UIHumanBodyElementComponent>
	{
		public UIHumanBodyElement(Transform parent, HumanManager.Human colonist) : base(parent) {
			foreach ((HumanManager.Human.Appearance appearance, Clothing clothing) in colonist.clothes) {
				SetClothingOnBodySection(appearance, clothing);
			}
		}

		public void SetClothingOnBodySection(HumanManager.Human.Appearance appearance, Clothing clothing) {
			Component.SetClothingOnBodySection(appearance, clothing);
		}

		public void SetSpriteOnBodySection(Sprite sprite) {
			Component.SetBodySectionSprite(HumanManager.Human.Appearance.Skin, sprite);
		}
	}
}