using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.LoadSave {

	public class UILoadColonyView : UIView {

		[SerializeField] private Button backButton;
		[SerializeField] private Button createColonyButton;

		[SerializeField] private GridLayoutGroup planetViewGridLayoutGroup;
		public GridLayoutGroup PlanetViewGridLayoutGroup => planetViewGridLayoutGroup;
		[SerializeField] private GameObject planetTilePrefab;
		public GameObject PlanetTilePrefab => planetTilePrefab;

		[SerializeField] private Transform colonyElementsParent;
		public Transform ColonyElementsParent => colonyElementsParent;

		[SerializeField] private Button loadColonyButton;
		[SerializeField] private Text loadColonyButtonText;

		public event Action OnBackButtonClicked;
		public event Action OnCreateColonyButtonClicked;
		public event Action OnLoadColonyButtonClicked;

		public override void OnOpen() {
			backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
			createColonyButton.onClick.AddListener(() => OnCreateColonyButtonClicked?.Invoke());
			loadColonyButton.onClick.AddListener(() => OnLoadColonyButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}

		public void SetLoadColonyButtonInteractable(bool interactable, string buttonText) {
			loadColonyButton.interactable = interactable;
			loadColonyButtonText.text = buttonText;
		}
	}
}
