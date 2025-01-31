using JetBrains.Annotations;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UIDebugConsolePresenter : UIPresenter<UIDebugConsoleView> {

		public UIDebugConsolePresenter(UIDebugConsoleView view) : base(view) {
		}

		public override void OnCreate() {

			GameManager.Get<DebugManager>().OnConsoleOutputProduced += OnDebugOutputReceived;
			GameManager.Get<DebugManager>().OnConsoleClearRequested += ClearConsole;

			View.SelectDebugInputField();

			View.OnDebugCommandSent += OnDebugCommandSent;
		}

		public override void OnClose() {

			GameManager.Get<DebugManager>().OnConsoleOutputProduced -= OnDebugOutputReceived;
			GameManager.Get<DebugManager>().OnConsoleClearRequested -= ClearConsole;

			View.OnDebugCommandSent -= OnDebugCommandSent;
		}

		private void OnDebugCommandSent(string text) {
			GameManager.Get<DebugManager>().ParseCommandInput(text);
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