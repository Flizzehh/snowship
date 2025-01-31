using UnityEngine;

namespace Snowship.NUI
{
	public class UIDebugConsoleOutputTextbox : UIElement<UIDebugConsoleOutputTextboxComponent> {

		public UIDebugConsoleOutputTextbox(Transform parent, string text) : base(parent) {
			Component.OutputToConsole(text);
		}

	}
}