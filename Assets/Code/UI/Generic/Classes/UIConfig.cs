using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Snowship.NUI.Generic {
	public class UIConfig<TView, TPresenter> : IUIConfig
		where TView : IUIView
		where TPresenter : IUIPresenter
	{
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
			return await Addressables.LoadAssetAsync<GameObject>($"UI/{AddressableKey()}");
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private TPresenter CreatePresenter(TView view) {
			return (TPresenter)Activator.CreateInstance(typeof(TPresenter), view);
		}
	}
}
