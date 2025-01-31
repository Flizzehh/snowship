using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUI
{
	public class UIActionGroupButtonElement : UIElement<UIActionGroupButtonElementComponent>, ITreeButton
	{
		public bool ChildElementsActiveState { get; private set; } = false;
		private readonly List<ITreeButton> childButtons = new();

		public event Action OnButtonClicked;

		public UIActionGroupButtonElement(Transform parent, string groupName, Sprite groupIcon) : base(parent) {
			Component.SetGroupName(groupName);
			Component.SetGroupIcon(groupIcon);

			Component.OnButtonClicked += OnComponentButtonClicked;

			Component.SetChildElementsActive(ChildElementsActiveState);
		}

		private void OnComponentButtonClicked() {
			OnButtonClicked?.Invoke();

			// Component.SetChildElementsActive(!childElementsActiveState);
		}

		public void SetChildSiblingChildElementsActive(ITreeButton childButtonToBeActive) {
			foreach (ITreeButton childButton in childButtons) {
				childButton.SetChildElementsActive(childButton == childButtonToBeActive && !childButtonToBeActive.ChildElementsActiveState);
			}
		}

		public void SetChildElementsActive(bool active) {

			ChildElementsActiveState = active;

			Component.SetChildElementsActive(active);

			if (active) {
				return;
			}
			foreach (ITreeButton childButton in childButtons) {
				childButton.SetChildElementsActive(false);
			}
		}

		public UIActionGroupButtonElement AddChildGroupButton(string groupName, Sprite groupIcon) {
			UIActionGroupButtonElement childButton = new(Component.SubGroupsLayoutGroup.transform, groupName, groupIcon);
			childButtons.Add(childButton);
			return childButton;
		}

		public UIActionItemButtonElement AddChildItemButton(string itemName, Sprite itemIcon) {
			UIActionItemButtonElement childButton = new(Component.SubGroupsLayoutGroup.transform, itemName, itemIcon);
			childButtons.Add(childButton);
			return childButton;
		}

		protected override void OnClose() {
			base.OnClose();

			foreach (ITreeButton childButton in childButtons) {
				childButton.Close();
			}
		}
	}
}