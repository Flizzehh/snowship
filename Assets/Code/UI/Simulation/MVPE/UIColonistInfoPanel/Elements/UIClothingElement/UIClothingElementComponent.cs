using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation
{
	public class UIClothingElementComponent : UIElementComponent
	{
		[SerializeField] private Button button;

		[SerializeField] private Image clothingImage;
		[SerializeField] private TMP_Text clothingNameText;

		[SerializeField] private TMP_Text waterResistanceValueText;
		[SerializeField] private TMP_Text insulationValueText;

		public event Action OnButtonClicked;

		public override void OnCreate() {
			base.OnCreate();

			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
		}

		protected override void OnClose() {
			base.OnClose();

			button.onClick.RemoveListener(() => OnButtonClicked?.Invoke());
		}

		public void SetClothingImage(Sprite clothingSprite) {
			clothingImage.sprite = clothingSprite;
		}

		public void SetClothingNameText(string clothingName) {
			clothingNameText.SetText(clothingName);
		}

		public void SetWaterResistanceValueText(string waterResistanceValue) {
			waterResistanceValueText.SetText(waterResistanceValue);
		}

		public void SetInsulationValueText(string insulationValue) {
			insulationValueText.SetText(insulationValue);
		}
	}
}
