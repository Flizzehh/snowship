using UnityEngine;

namespace Snowship.NUI {
	public abstract class UIElementComponent : MonoBehaviour {
		private void OnEnable() {
			OnCreate();
		}

		private void OnDisable() {
			OnClose();
		}

		public abstract void OnCreate();
		public abstract void OnClose();
		public void Close() => OnClose();
	}
}
