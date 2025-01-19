using System;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.LoadSave {
	public class UILoadSaveView : UIView {

		[SerializeField] private Button backButton;

		[SerializeField] private Transform saveElementsParent;

		[SerializeField] private Button loadSaveButton;
		[SerializeField] private Text loadSaveButtonText;

		public event Action OnBackButtonClicked;
		public event Action OnLoadSaveButtonClicked;

		public override void OnOpen() {
			backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
			loadSaveButton.onClick.AddListener(() => OnLoadSaveButtonClicked?.Invoke());
		}

		public override void OnClose() {
		}

		public Transform GetSaveElementsParentTransform() {
			return saveElementsParent;
		}

		public void SetLoadSaveButtonInteractable(bool interactable, string buttonText) {
			loadSaveButton.interactable = interactable;
			loadSaveButtonText.text = buttonText;
		}
	}
}
