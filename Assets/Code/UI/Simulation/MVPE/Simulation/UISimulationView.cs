using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Simulation.SimulationUI {
	public class UISimulationView : UIView {

		[Header("General")]
		[SerializeField] private Text disclaimerText;

		public override void OnOpen() {

		}

		public override void OnClose() {

		}

		public void SetDisclaimerText(string text) {
			disclaimerText.text = text;
		}

	}
}
