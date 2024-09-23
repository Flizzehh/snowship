using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Snowship.NUI.Generic {
	public abstract class UIElement<TComponent> where TComponent : UIElementComponent {

		protected readonly TComponent component;

		protected UIElement(Transform parent) {
			string addressableKey = GetType().Name;
			GameObject addressablePrefab = Addressables.LoadAssetAsync<GameObject>(addressableKey).WaitForCompletion();
			component = Object.Instantiate(addressablePrefab, parent, false).GetComponent<TComponent>();
		}

		public virtual void OnClose() {
			component.OnClose();
		}
	}
}
