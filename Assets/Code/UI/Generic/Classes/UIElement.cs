using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Snowship.NUI.Generic {
	public abstract class UIElement<TComponent> where TComponent : UIElementComponent {

		protected readonly TComponent Component;

		protected UIElement(Transform parent) {
			string addressableKey = $"UI/Element/{GetType().Name}";
			GameObject addressablePrefab = Addressables.LoadAssetAsync<GameObject>(addressableKey).WaitForCompletion();
			Component = Object.Instantiate(addressablePrefab, parent, false).GetComponent<TComponent>();
			Component.OnCreate();
		}

		public void OnClose() {
			Component.OnClose();
			Object.Destroy(Component);
		}

		public void Close() => OnClose();
	}
}
