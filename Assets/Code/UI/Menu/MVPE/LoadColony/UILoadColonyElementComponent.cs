using System;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UILoadColonyElementComponent : UIElementComponent {

		[SerializeField] private Text colonyNameText;
		[SerializeField] private Text lastSaveDateTimeText;
		[SerializeField] private Image lastSaveImage;
		[SerializeField] private Button elementButton;

		public event Action OnLoadColonyElementComponentButtonClicked;

		public override void OnCreate() {
			elementButton.onClick.AddListener(() => OnLoadColonyElementComponentButtonClicked?.Invoke());
		}

		protected override void OnClose() {
			elementButton.onClick.AddListener(() => OnLoadColonyElementComponentButtonClicked?.Invoke());
		}

		public void SetColonyNameText(string colonyName) {
			colonyNameText.text = colonyName;
		}

		public void SetLastSaveDateTimeText(string lastSaveDateTime) {
			lastSaveDateTimeText.text = lastSaveDateTime;
		}

		public void SetLastSaveImage(Sprite lastSaveSprite) {
			lastSaveImage.sprite = lastSaveSprite;
		}

		public void SetElementButtonInteractable(bool interactable) {
			elementButton.interactable = interactable;
		}
	}
}