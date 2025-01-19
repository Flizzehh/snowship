using UnityEngine;

namespace Snowship.NUI {
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

		public virtual void OnOpen() {
		}

		public virtual void OnOpen(IUIParameters parameters) {
		}

		public virtual void OnPostOpen() {
		}

		public virtual void OnClose() {
		}
	}
}
