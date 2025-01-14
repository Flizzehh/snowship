using Snowship.NUI.Generic;
using UnityEngine;

namespace Snowship.NUI.Simulation.DebugConsole {
	public class UIDebugConsoleOutputTextbox : UIElement<UIDebugConsoleOutputTextboxComponent> {

		public UIDebugConsoleOutputTextbox(Transform parent, string text) : base(parent) {
			Component.OutputToConsole(text);
		}

	}
}
