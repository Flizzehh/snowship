﻿using System;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI
{
	public class UIActionItemButtonElement : UIElement<UIActionItemButtonElementComponent>, ITreeButton
	{
		public bool ChildElementsActiveState { get; private set; } = false;

		public event Action OnButtonClicked;

		public UIActionItemButtonElement(Transform parent, string itemName, Sprite itemIcon) : base(parent) {
			Component.SetItemName(itemName);
			Component.SetItemIcon(itemIcon);

			Component.OnButtonClicked += OnComponentButtonClicked;

			SetChildElementsActive(ChildElementsActiveState);
		}

		private void OnComponentButtonClicked() {
			OnButtonClicked?.Invoke();
			SetChildElementsActive(!ChildElementsActiveState);
		}

		public void SetChildSiblingChildElementsActive(ITreeButton childButtonToBeActive) {
			throw new NotImplementedException();
		}

		public void SetChildElementsActive(bool active) {
			ChildElementsActiveState = active;
			Component.SetChildElementsActive(active);
		}

		public void AddVariation(ObjectPrefab prefab, Variation variation) {
			Component.AddVariation(prefab, variation);
		}
	}
}