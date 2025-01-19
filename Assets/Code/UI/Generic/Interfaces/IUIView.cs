using UnityEngine;

namespace Snowship.NUI {
	public interface IUIView {

		void OnOpen();
		void OnOpen(IUIParameters parameters);
		void OnPostOpen();
		void OnClose();
		void SetActive(bool active);
		GameObject Instance { get; set; }
		bool IsActive { get; }
	}
}
