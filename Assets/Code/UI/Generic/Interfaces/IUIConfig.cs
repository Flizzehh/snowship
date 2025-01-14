using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI.Generic {
	public interface IUIConfig {
		public UniTask<(IUIView view, IUIPresenter presenter)> Open(Transform parent);
		public void OnClose();

	}
}
