using JetBrains.Annotations;
using Snowship.NTime;
using Snowship.NUI.Generic;

namespace Snowship.NUI.Simulation.UIDateTime {

	[UsedImplicitly]
	public class UIDateTimePresenter : UIPresenter<UIDateTimeView> {

		public UIDateTimePresenter(UIDateTimeView view) : base(view) {
		}

		public override void OnCreate() {
			GameManager.timeM.OnTimeChanged += OnTimeChanged;
			OnTimeChanged(GameManager.timeM.Time);
		}

		public override void OnClose() {

		}

		private void OnTimeChanged(SimulationDateTime simulationDateTime) {
			View.SetSpeedText(simulationDateTime.TimeModifier > 0 ? new string('>', simulationDateTime.TimeModifier) : "-");
			View.SetDayNightText(simulationDateTime.GetDayNightString());
			View.SetTimeText($"{(simulationDateTime.Hour < 10 ? $"0{simulationDateTime.Hour}" : simulationDateTime.Hour)}:{(simulationDateTime.Minute < 10 ? $"0{simulationDateTime.Minute}" : simulationDateTime.Minute)}");
			View.SetDateSeasonText($"{simulationDateTime.GetDayWithSuffix()} of {simulationDateTime.Season}");
			View.SetYearText($"Year {(simulationDateTime.Year <= 9 ? $"0{simulationDateTime.Year}" : simulationDateTime.Year)}");
		}

	}
}
