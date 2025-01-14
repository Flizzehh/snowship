using System;

namespace Snowship.NUI {
	public static class UIEvents {

		public static event Action<string, string> OnLoadingScreenTextChanged;

		public static void UpdateLoadingScreenText(string state, string substate) {
			OnLoadingScreenTextChanged?.Invoke(state, substate);
		}

	}
}
