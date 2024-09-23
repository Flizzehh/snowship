using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.Settings {
	public class UISettingsView : UIView {

		[SerializeField] private Button cancelButton;
		[SerializeField] private Button applyButton;
		[SerializeField] private Button acceptButton;

		public event Action OnCancelButtonClicked;
		public event Action OnApplyButtonClicked;
		public event Action OnAcceptButtonClicked;

		public override void OnOpen() {
			cancelButton.onClick.AddListener(() => OnCancelButtonClicked?.Invoke());
			applyButton.onClick.AddListener(() => OnApplyButtonClicked?.Invoke());
			acceptButton.onClick.AddListener(() => OnAcceptButtonClicked?.Invoke());
		}

		public override void OnClose() {

		}
	}
}
