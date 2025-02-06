using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI
{
	public class UIActionGroupButtonElement : UIElement<UIActionGroupButtonElementComponent>, ITreeButton
	{
		private readonly string groupName;
		private readonly Sprite groupIcon;

		public bool ChildElementsActiveState { get; private set; } = false;
		private readonly List<ITreeButton> childButtons = new();

		public event Action OnButtonClicked;

		public UIActionGroupButtonElement(string groupName, Sprite groupIcon) {
			this.groupName = groupName;
			this.groupIcon = groupIcon;

		}

		protected override void OnCreate() {
			base.OnCreate();

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

		public async UniTask<UIActionGroupButtonElement> AddChildGroupButton(string groupName, Sprite groupIcon) {
			UIActionGroupButtonElement childButton = new(groupName, groupIcon);
			await UniTask.WaitUntil(() => Component);
			await childButton.Open(Component.SubGroupsLayoutGroup.transform);
			childButtons.Add(childButton);
			return childButton;
		}

		public async UniTask<UIActionItemButtonElement> AddChildItemButton(string itemName, Sprite itemIcon) {
			UIActionItemButtonElement childButton = new(itemName, itemIcon);
			await UniTask.WaitUntil(() => Component);
			await childButton.Open(Component.SubGroupsLayoutGroup.transform);
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