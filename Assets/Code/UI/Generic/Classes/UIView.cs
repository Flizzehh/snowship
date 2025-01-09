using UnityEngine;

namespace Snowship.NUI.Generic {
	public abstract class UIView : MonoBehaviour, IUIView {

		public abstract void OnOpen();
		public abstract void OnClose();

		public virtual void SetActive(bool active) {
			instance.SetActive(active);
		}

		private GameObject instance;

		public GameObject Instance {
			get => instance;
			set => instance = (!instance ? value : instance);
		}
	}
}
