using UnityEngine;

namespace Snowship.NUI {
	public abstract class UIElementComponent : MonoBehaviour {
		public virtual void OnCreate() {
		}

		protected virtual void OnClose() {
		}

		public void Close() => OnClose();
	}
}
