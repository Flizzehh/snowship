using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel.UIInventoryResource
{
	public class UIResourceAmountElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text resourceNameText;
		[SerializeField] private TMP_Text resourceAmountText;
		[SerializeField] private Image resourceImage;

		public void SetResourceAmount(ResourceManager.ResourceAmount resourceAmount) {
			resourceNameText.SetText(resourceAmount.resource.name);
			resourceAmountText.SetText(resourceAmount.amount.ToString());
			resourceImage.sprite = resourceAmount.resource.image;
		}
	}
}
