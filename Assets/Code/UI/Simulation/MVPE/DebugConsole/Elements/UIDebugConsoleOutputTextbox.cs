namespace Snowship.NUI
{
	public class UIDebugConsoleOutputTextbox : UIElement<UIDebugConsoleOutputTextboxComponent> {
		private readonly string text;

		public UIDebugConsoleOutputTextbox(string text) {
			this.text = text;
		}

		protected override void OnCreate() {
			base.OnCreate();

			Component.OutputToConsole(text);
		}
	}
}