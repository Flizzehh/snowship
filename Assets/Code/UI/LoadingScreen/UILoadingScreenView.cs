using System;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.LoadingScreen {
	public class UILoadingScreenView : UIView {

		[SerializeField] private Text loadingStateText;
		[SerializeField] private Text loadingSubStateText;

		public override void OnOpen() {

		}

		public override void OnClose() {

		}

		public void UpdateLoadingStateText(string state, string substate) {
			loadingStateText.text = state;
			loadingSubStateText.text = substate;
		}

	}
}
