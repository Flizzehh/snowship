using System;
using System.Collections.Generic;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation
{
	public class UIHumanBodyElementComponent : UIElementComponent
	{
		[SerializeField] private List<UIHumanBodyClothingSection> clothingSections = new();

		public void SetClothingOnBodySection(HumanManager.Human.Appearance appearance, Clothing clothing) {
			SetBodySectionSprite(appearance, clothing.moveSprites[0]);
		}

		public void SetBodySectionSprite(HumanManager.Human.Appearance appearance, Sprite sprite) {
			foreach (UIHumanBodyClothingSection clothingSection in clothingSections) {
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
	public struct UIHumanBodyClothingSection
	{
		public HumanManager.Human.Appearance appearance;
		public Image sectionImage;
	}
}