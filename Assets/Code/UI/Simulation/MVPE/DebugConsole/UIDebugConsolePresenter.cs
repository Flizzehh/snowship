using JetBrains.Annotations;
using Snowship.NUI.Generic;

namespace Snowship.NUI.Simulation.DebugConsole {

	[UsedImplicitly]
	public class UIDebugConsolePresenter : UIPresenter<UIDebugConsoleView> {

		public UIDebugConsolePresenter(UIDebugConsoleView view) : base(view) {
		}

		public override void OnCreate() {

			GameManager.debugM.OnConsoleOutputProduced += OnDebugOutputReceived;
			GameManager.debugM.OnConsoleClearRequested += ClearConsole;

			View.SelectDebugInputField();

			View.OnDebugCommandSent += OnDebugCommandSent;
		}

		public override void OnClose() {

			GameManager.debugM.OnConsoleOutputProduced -= OnDebugOutputReceived;
			GameManager.debugM.OnConsoleClearRequested -= ClearConsole;

			View.OnDebugCommandSent -= OnDebugCommandSent;
		}

		private void OnDebugCommandSent(string text) {
			GameManager.debugM.ParseCommandInput(text);
		}

		private void OnDebugOutputReceived(string text) {
			View.OutputToConsole(text);
			View.SelectDebugInputField();
		}

		private void ClearConsole() {
			View.ClearConsole();
			View.SelectDebugInputField();
		}

	}
}
