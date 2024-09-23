using JetBrains.Annotations;
using Snowship.NUI.Generic;

namespace Snowship.NUI.Menu.Settings {

	[UsedImplicitly]
	public class UISettingsPresenter : UIPresenter<UISettingsView> {

		public UISettingsPresenter(UISettingsView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnCancelButtonClicked += OnCancelButtonClicked;
			View.OnApplyButtonClicked += OnApplyButtonClicked;
			View.OnAcceptButtonClicked += OnAcceptButtonClicked;
		}

		public override void OnClose() {
			View.OnCancelButtonClicked -= OnCancelButtonClicked;
			View.OnApplyButtonClicked -= OnApplyButtonClicked;
			View.OnAcceptButtonClicked -= OnAcceptButtonClicked;
		}

		private void OnCancelButtonClicked() {
			GameManager.uiM.CloseView(this);
		}

		private void OnApplyButtonClicked() {
			GameManager.persistenceM.ApplySettings();
			GameManager.uiM.CloseView(this);
		}

		private void OnAcceptButtonClicked() {
			GameManager.persistenceM.ApplySettings();
			GameManager.uiM.CloseView(this);
		}
	}
}
