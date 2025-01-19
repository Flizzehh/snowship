using JetBrains.Annotations;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel {

	[UsedImplicitly]
	public class UIColonistInfoPanelPresenter : UIPresenter<UIColonistInfoPanelView> {

		public UIColonistInfoPanelPresenter(UIColonistInfoPanelView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnTabSelected += OnTabSelected;
		}

		private void OnTabSelected(Button tabButton) {
			foreach ((Button button, GameObject tab) mapping in View.ButtonToTabMap) {
				mapping.tab.SetActive(mapping.button == tabButton);
			}
		}

		public override void OnClose() {

		}

	}
}
