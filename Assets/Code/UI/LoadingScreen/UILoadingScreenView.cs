using TMPro;
using UnityEngine;

namespace Snowship.NUI
{
	public class UILoadingScreenView : UIView {

		[SerializeField] private TMP_Text loadingStateText;
		[SerializeField] private TMP_Text loadingSubStateText;

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