using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIActionGroupButtonElementComponent : UIElementComponent
	{
		[SerializeField] private TMP_Text groupNameText;
		[SerializeField] private Image groupIconImage;

		[SerializeField] private Button button;
		[SerializeField] private GameObject selectedIndicator;

		[SerializeField] private LayoutGroup subGroupsLayoutGroup;
		public LayoutGroup SubGroupsLayoutGroup => subGroupsLayoutGroup;

		public event Action OnButtonClicked;

		public override void OnCreate() {
			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
		}

		protected override void OnClose() {
			button.onClick.RemoveListener(() => OnButtonClicked?.Invoke());
		}

		public void SetGroupIcon(Sprite groupIcon) {
			groupIconImage.sprite = groupIcon;
		}

		public void SetGroupName(string groupName) {
			groupNameText.SetText(groupName);
		}

		public void SetChildElementsActive(bool active) {
			subGroupsLayoutGroup.gameObject.SetActive(active);
			selectedIndicator.SetActive(active);
		}
	}
}