using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Snowship.NUI.Generic {
	public class UIConfig<TView, TPresenter> : IUIConfig
		where TView : IUIView
		where TPresenter : IUIPresenter {

		private AsyncOperationHandle<GameObject> viewPrefabOperationHandle;

		protected virtual string AddressableKey() {
			return GetType().Name;
		}

		public async UniTask<(IUIView view, IUIPresenter presenter)> Open(Transform parent) {
			GameObject viewPrefab = await GetViewPrefab();
			GameObject viewGameObject = Object.Instantiate(viewPrefab, parent, false);
			TView viewComponent = viewGameObject.GetComponent<TView>();
			viewComponent.Instance = viewGameObject;

			TPresenter presenter = CreatePresenter(viewComponent);

			return (viewComponent, presenter);
		}

		private async UniTask<GameObject> GetViewPrefab() {
			Debug.Log(AddressableKey());
			viewPrefabOperationHandle = Addressables.LoadAssetAsync<GameObject>($"UI/{AddressableKey()}");
			return await viewPrefabOperationHandle;
		}

		public void OnClose() {
			Addressables.Release(viewPrefabOperationHandle);
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private TPresenter CreatePresenter(TView view) {
			return (TPresenter)Activator.CreateInstance(typeof(TPresenter), view);
		}
	}
}
