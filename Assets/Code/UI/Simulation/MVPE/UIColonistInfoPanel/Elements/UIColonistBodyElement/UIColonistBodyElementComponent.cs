using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation
{
	public class UIColonistBodyElementComponent : UIElementComponent
	{
		[SerializeField] private List<UIColonistBodyClothingSection> clothingSections = new();

		public void SetClothingOnBodySection(HumanManager.Human.Appearance appearance, ResourceManager.Clothing clothing) {
			SetBodySectionSprite(appearance, clothing.moveSprites[0]);
		}

		public void SetBodySectionSprite(HumanManager.Human.Appearance appearance, Sprite sprite) {
			foreach (UIColonistBodyClothingSection clothingSection in clothingSections) {
				if (clothingSection.appearance != appearance) {
					continue;
				}
				if (clothingSection.sectionImage == null) {
					continue;
				}

				clothingSection.sectionImage.sprite = sprite;
			}
		}
	}

	[Serializable]
	public struct UIColonistBodyClothingSection
	{
		public HumanManager.Human.Appearance appearance;
		public Image sectionImage;
	}
}
