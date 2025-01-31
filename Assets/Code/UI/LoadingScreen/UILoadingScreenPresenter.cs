using JetBrains.Annotations;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UILoadingScreenPresenter : UIPresenter<UILoadingScreenView> {

		public UILoadingScreenPresenter(UILoadingScreenView view) : base(view) {
		}

		public override void OnCreate() {
			UIEvents.OnLoadingScreenTextChanged += UpdateLoadingStateText;
		}

		public override void OnClose() {
			UIEvents.OnLoadingScreenTextChanged -= UpdateLoadingStateText;
		}

		private void UpdateLoadingStateText(string state, string substate) {
			View.UpdateLoadingStateText(state, substate);
		}

	}
}