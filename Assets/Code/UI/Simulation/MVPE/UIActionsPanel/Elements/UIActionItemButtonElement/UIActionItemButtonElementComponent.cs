using System;
using Cysharp.Threading.Tasks;
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
		[SerializeField] private LayoutElement itemIconLayoutElement;

		[SerializeField] private Button button;
		[SerializeField] private GameObject selectedIndicator;

		[SerializeField] private LayoutGroup requiredResourcesLayoutGroup;
		public LayoutGroup RequiredResourcesLayoutGroup => requiredResourcesLayoutGroup;

		[SerializeField] private Image variationIndicatorImage;
		[SerializeField] private LayoutGroup variationsLayoutGroup;
		[SerializeField] private LayoutGroup buttonLayoutGroup;

		public event Action OnButtonClicked;

		public override void OnCreate() {
			variationIndicatorImage.gameObject.SetActive(false);
			buttonLayoutGroup.padding.right = 2;

			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
		}

		protected override void OnClose() {

		}

		public void SetItemName(string itemName) {
			itemNameText.SetText(itemName);
		}

		public void SetItemIcon(Sprite itemIcon) {
			itemIconImage.sprite = itemIcon;
			itemIconLayoutElement.preferredWidth = itemIconLayoutElement.preferredHeight * (itemIcon?.rect.width / itemIcon?.rect.height) ?? 0;
		}

		public void SetChildElementsActive(bool active) {
			requiredResourcesLayoutGroup.gameObject.SetActive(active);
			variationsLayoutGroup.gameObject.SetActive(active);
			selectedIndicator.SetActive(active);
		}

		public UIActionItemButtonElement AddVariation(ObjectPrefab prefab, Variation variation) {
			variationIndicatorImage.gameObject.SetActive(true);
			buttonLayoutGroup.padding.right = 12;

			UIActionItemButtonElement variationButton = new(variation.name, prefab.GetBaseSpriteForVariation(variation));
			variationButton.Open(variationsLayoutGroup.transform).Forget();
			return variationButton;
		}
	}
}