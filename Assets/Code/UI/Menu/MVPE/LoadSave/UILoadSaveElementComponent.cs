using System;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.LoadSave {
	public class UILoadSaveElementComponent : UIElementComponent {

		[SerializeField] private Image saveElementBackground;
		[SerializeField] private Image saveImage;
		[SerializeField] private Text saveText;
		[SerializeField] private Text saveDateTimeText;
		[SerializeField] private Button elementButton;

		public event Action OnLoadSaveElementComponentButtonClicked;

		public override void OnCreate() {
			elementButton.onClick.AddListener(() => OnLoadSaveElementComponentButtonClicked?.Invoke());
		}

		public override void OnClose() {
			elementButton.onClick.RemoveListener(() => OnLoadSaveElementComponentButtonClicked?.Invoke());
		}

		public void SetSaveElementBackgroundColour(Color colour) {
			saveElementBackground.color = colour;
		}

		public void SetSaveImage(Sprite sprite) {
			saveImage.sprite = sprite;
		}

		public void SetSaveText(string text) {
			saveText.text = text;
		}

		public void SetSaveDateTimeText(string text) {
			saveDateTimeText.text = text;
		}

		public void SetElementButtonInteractable(bool interactable) {
			elementButton.interactable = interactable;
		}
	}
}
