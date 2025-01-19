using TMPro;
using UnityEngine;

namespace Snowship.NUI.Simulation.UIDateTime {
	public class UIDateTimeView : UIView {

		[SerializeField] private TMP_Text speedText;
		[SerializeField] private TMP_Text dayNightText;
		[SerializeField] private TMP_Text timeText;
		[SerializeField] private TMP_Text dateSeasonText;
		[SerializeField] private TMP_Text yearText;

		public override void OnOpen() {

		}

		public override void OnClose() {

		}

		public void SetSpeedText(string speedString) {
			speedText.SetText(speedString);
		}

		public void SetDayNightText(string dayNightString) {
			dayNightText.SetText(dayNightString);
		}

		public void SetTimeText(string timeString) {
			timeText.SetText(timeString);
		}

		public void SetDateSeasonText(string dateSeasonString) {
			dateSeasonText.SetText(dateSeasonString);
		}

		public void SetYearText(string yearString) {
			yearText.SetText(yearString);
		}

	}
}
