using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Snowship.NUI
{
	public class UIConfig<TView, TPresenter> : IUIConfig
		where TView : IUIView
		where TPresenter : IUIPresenter {

		private string AddressableKey() {
			return $"UI/{GetType().Name}";
		}

		public async UniTask<(IUIView view, IUIPresenter presenter)> Open(Transform parent) {
			GameObject viewPrefab = await GetViewPrefab();
			GameObject viewGameObject = Object.Instantiate(viewPrefab, parent, false);
			TView viewComponent = viewGameObject.GetComponent<TView>();
			viewComponent.Instance = viewGameObject;

			TPresenter presenter = CreatePresenter(viewComponent);

			return (viewComponent, presenter);
		}

		protected async UniTask<GameObject> GetViewPrefab() {
			AsyncOperationHandle<GameObject> viewPrefabOperationHandle = Addressables.LoadAssetAsync<GameObject>(AddressableKey());
			viewPrefabOperationHandle.ReleaseHandleOnCompletion();
			return await viewPrefabOperationHandle;
		}

		private TPresenter CreatePresenter(TView view) {
			return (TPresenter)Activator.CreateInstance(typeof(TPresenter), view);
		}

		public void OnClose() {
		}
	}

	// ReSharper disable once UnusedTypeParameter
	public class UIConfig<TView, TPresenter, TParameters> : UIConfig<TView, TPresenter>, IUIConfigParameters
		where TView : IUIView
		where TPresenter : IUIPresenter
		where TParameters : IUIParameters
	{
		public async UniTask<(IUIView view, IUIPresenter presenter)> Open(Transform parent, IUIParameters parameters) {
			GameObject viewPrefab = await GetViewPrefab();
			GameObject viewGameObject = Object.Instantiate(viewPrefab, parent, false);
			TView viewComponent = viewGameObject.GetComponent<TView>();
			viewComponent.Instance = viewGameObject;

			TPresenter presenter = CreatePresenter(viewComponent, parameters);

			return (viewComponent, presenter);
		}

		private TPresenter CreatePresenter(TView view, IUIParameters parameters) {
			return (TPresenter)Activator.CreateInstance(typeof(TPresenter), view, parameters);
		}
	}
}