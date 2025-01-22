using JetBrains.Annotations;
using Snowship.NColonist;
using UnityEngine;

namespace Snowship.NUI.Simulation
{
	[UsedImplicitly]
	public class UIColonistBodyElement : UIElement<UIColonistBodyElementComponent>
	{
		public UIColonistBodyElement(Transform parent, Colonist colonist) : base(parent) {
			foreach ((HumanManager.Human.Appearance appearance, ResourceManager.Clothing clothing) in colonist.clothes) {
				SetClothingOnBodySection(appearance, clothing);
			}
		}

		public void SetClothingOnBodySection(HumanManager.Human.Appearance appearance, ResourceManager.Clothing clothing) {
			Component.SetClothingOnBodySection(appearance, clothing);
		}

		public void SetSpriteOnBodySection(Sprite sprite) {
			Component.SetBodySectionSprite(HumanManager.Human.Appearance.Skin, sprite);
		}
	}
}
