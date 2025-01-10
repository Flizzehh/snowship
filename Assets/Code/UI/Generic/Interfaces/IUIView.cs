using UnityEngine;

namespace Snowship.NUI.Generic {
	public interface IUIView {
		public void OnOpen();
		public void OnClose();
		public void SetActive(bool active);
		public GameObject Instance { get; set; }
		public bool IsActive { get; }
	}
}
