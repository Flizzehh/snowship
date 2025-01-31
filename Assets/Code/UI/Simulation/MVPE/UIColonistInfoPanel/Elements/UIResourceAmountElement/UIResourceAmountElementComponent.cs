using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIResourceAmountElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text resourceNameText;
		[SerializeField] private TMP_Text resourceAmountText;
		[SerializeField] private Image resourceImage;

		public void SetResourceImage(Sprite resourceSprite) {
			resourceImage.sprite = resourceSprite;
		}

		public void SetResourceName(string resourceName) {
			resourceNameText.SetText(resourceName);
		}

		public void SetResourceAmount(string resourceAmount) {
			resourceAmountText.SetText(resourceAmount);
		}
	}
}