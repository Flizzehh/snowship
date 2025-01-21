using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.UIColonistInfoPanel {

	[UsedImplicitly]
	public class UIColonistInfoPanelPresenter : UIPresenter<UIColonistInfoPanelView, UIColonistInfoPanelParameters>
	{
		public UIColonistInfoPanelPresenter(UIColonistInfoPanelView view, UIColonistInfoPanelParameters parameters) : base(view, parameters) {
		}

		public override void OnCreate() {

			View.OnTabSelected += OnTabSelected;

			OnTabSelected(View.ButtonToTabMap.First().button);

			View.SetColonist(Parameters.Colonist);
			View.SetupUI();
		}

		private void OnTabSelected(Button tabButton) {
			foreach ((Button button, GameObject tab) mapping in View.ButtonToTabMap) {
				mapping.tab.SetActive(mapping.button == tabButton);
			}
		}
	}
}
