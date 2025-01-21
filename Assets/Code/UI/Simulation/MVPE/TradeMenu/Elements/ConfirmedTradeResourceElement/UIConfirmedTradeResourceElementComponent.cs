using Snowship.NResource.Models;
using Snowship.NUI;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmedTradeResourceElementComponent : UIElementComponent {

	[SerializeField] private Image background;
	[SerializeField] private Image resourceImage;
	[SerializeField] private Text resourceNameText;
	[SerializeField] private Text collectedVsRemainingAmountText;

	public void SetResource(ConfirmedTradeResourceAmount resourceAmount) {
		resourceImage.sprite = resourceAmount.resource.image;
		resourceNameText.text = resourceAmount.resource.name;

		UpdateCollectedAmount(resourceAmount);
	}

	public void UpdateCollectedAmount(ConfirmedTradeResourceAmount resourceAmount) {
		int collected = Mathf.Abs(resourceAmount.tradeAmount - resourceAmount.amountRemaining);
		int remaining = Mathf.Abs(resourceAmount.tradeAmount);
		collectedVsRemainingAmountText.text = $"{collected} / {remaining}";

		background.color = ColourUtilities.GetColour(
			resourceAmount.amountRemaining == 0
				? ColourUtilities.EColour.LightGreen
				: ColourUtilities.EColour.LightGrey200
		);
	}
}
