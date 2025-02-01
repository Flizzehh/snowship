using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIActionsPanelView : UIView
	{
		[SerializeField] private UIActionsButtonsDataSO buttonsData;
		public UIActionsButtonsDataSO ButtonsData => buttonsData;

		[SerializeField] private LayoutGroup buttonsLayoutGroup;
		public LayoutGroup ButtonsLayoutGroup => buttonsLayoutGroup;

		private List<UIActionGroupButtonElement> buttons = new();

		public override void OnOpen() {

		}

		public override void OnClose() {

		}
	}
}