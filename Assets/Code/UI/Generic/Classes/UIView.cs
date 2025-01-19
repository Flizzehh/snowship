using UnityEngine;

namespace Snowship.NUI.Generic {
	public abstract class UIView : MonoBehaviour, IUIView {

		private GameObject instance;
		public GameObject Instance {
			get => instance;
			set => instance = !instance ? value : instance;
		}

		public bool IsActive => instance.activeSelf;

		public virtual void SetActive(bool active) {
			instance.SetActive(active);
		}

		public abstract void OnOpen();
		public abstract void OnClose();
	}
}
