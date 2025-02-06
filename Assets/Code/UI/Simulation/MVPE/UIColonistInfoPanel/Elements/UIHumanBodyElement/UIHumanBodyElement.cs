using JetBrains.Annotations;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIHumanBodyElement : UIElement<UIHumanBodyElementComponent>
	{
		private readonly Human human;

		public UIHumanBodyElement(Human human) {
			this.human = human;

		}

		protected override void OnCreate() {
			base.OnCreate();

			foreach ((BodySection appearance, Clothing clothing) in human.clothes) {
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