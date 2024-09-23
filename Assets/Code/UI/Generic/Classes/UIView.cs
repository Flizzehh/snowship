using UnityEngine;

namespace Snowship.NUI.Generic {
	public abstract class UIView : MonoBehaviour, IUIView {
		public abstract void OnOpen();
		public abstract void OnClose();
	}
}
