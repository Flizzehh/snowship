using System;
using Snowship.NResource;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIActionItemButtonElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text itemNameText;
		[SerializeField] private Image itemIconImage;

		[SerializeField] private Button button;

		[SerializeField] private LayoutGroup requiredResourcesLayoutGroup;
		public LayoutGroup RequiredResourcesLayoutGroup => requiredResourcesLayoutGroup;

		[SerializeField] private Image variationIndicatorImage;
		[SerializeField] private LayoutGroup variationsLayoutGroup;

		public event Action OnButtonClicked;

		public override void OnCreate() {
			variationIndicatorImage.gameObject.SetActive(false);

			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
		}

		protected override void OnClose() {

		}

		public void SetItemName(string itemName) {
			itemNameText.SetText(itemName);
		}

		public void SetItemIcon(Sprite itemIcon) {
			itemIconImage.sprite = itemIcon;
		}

		public void SetChildElementsActive(bool active) {
			requiredResourcesLayoutGroup.gameObject.SetActive(active);
			variationsLayoutGroup.gameObject.SetActive(active);
		}

		public UIActionItemButtonElement AddVariation(ObjectPrefab prefab, Variation variation) {
			variationIndicatorImage.gameObject.SetActive(true);

			UIActionItemButtonElement variationButton = new(variationsLayoutGroup.transform, variation.name, prefab.GetBaseSpriteForVariation(variation));
			return variationButton;
		}
	}
}