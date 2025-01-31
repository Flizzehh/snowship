using JetBrains.Annotations;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIHumanBodyElement : UIElement<UIHumanBodyElementComponent>
	{
		public UIHumanBodyElement(Transform parent, Human colonist) : base(parent) {
			foreach ((BodySection appearance, Clothing clothing) in colonist.clothes) {
				SetClothingOnBodySection(appearance, clothing);
			}
		}

		public void SetClothingOnBodySection(BodySection bodySection, Clothing clothing) {
			Component.SetClothingOnBodySection(bodySection, clothing);
		}

		public void SetSpriteOnBodySection(Sprite sprite) {
			Component.SetBodySectionSprite(BodySection.Skin, sprite);
		}
	}
}