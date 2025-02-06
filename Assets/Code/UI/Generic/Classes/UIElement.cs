using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Snowship.NUI {
	public abstract class UIElement<TComponent> where TComponent : UIElementComponent {
		protected TComponent Component { get; private set; }

		public async UniTask Open(Transform parent) {
			await CreateComponent(parent);
			OnCreate();
		}

		private string AddressableKey() {
			return $"UI/Element/{GetType().Name}";
		}

		private async UniTask CreateComponent(Transform parent) {
			AsyncOperationHandle<GameObject> componentPrefabOperationHandle = Addressables.LoadAssetAsync<GameObject>(AddressableKey());
			componentPrefabOperationHandle.ReleaseHandleOnCompletion();
			GameObject componentPrefab = await componentPrefabOperationHandle;

			Component = Object.Instantiate(componentPrefab, parent, false).GetComponent<TComponent>();
			Component.OnCreate();
		}

		protected virtual void OnCreate() {

		}

		protected virtual void OnClose() {
			Component.Close();
			Object.Destroy(Component.gameObject);
		}

		public void Close() => OnClose();
	}
}