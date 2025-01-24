using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI
{
	public interface IUIManager : IManager
	{
		UniTask<IUIPresenter> OpenViewAsync<TUIConfig>() where TUIConfig : IUIConfig, new();

		UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(
			IUIPresenter parent
		)
			where TUIConfig : IUIConfig, new();

		UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(
			IUIPresenter parent,
			bool showParent
		)
			where TUIConfig : IUIConfig, new();

		UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(
			IUIPresenter parent,
			IUIParameters parameters,
			bool showParent,
			bool useParentTransform
		) where TUIConfig : IUIConfig, new();

		UniTask<IUIPresenter> SwitchViewsAsync<TUIConfigToClose, TUIConfigToOpen>(
			IUIParameters parameters,
			bool showParent = true,
			bool useParentTransform = false
		)
			where TUIConfigToClose : IUIConfig
			where TUIConfigToOpen : IUIConfig, new();

		UniTask<IUIPresenter> SwitchViewsOrOpenAsync<TUIConfigToClose, TUIConfigToOpen>(
			IUIPresenter parent = null,
			IUIParameters parameters = null,
			bool showParent = true,
			bool useParentTransform = false
		)
			where TUIConfigToClose : IUIConfig
			where TUIConfigToOpen : IUIConfig, new();

		UniTask<IUIPresenter> ReopenView<TUIConfigToReopen>(
			IUIPresenter parent = null,
			IUIParameters parameters = null,
			bool showParent = true
		)
			where TUIConfigToReopen : IUIConfig, new();

		void CloseView<TP>(TP presenter) where TP : IUIPresenter;
		void CloseView<TC>() where TC : IUIConfig;
		void GoBack<TP>(TP presenter) where TP : IUIPresenter;
		void ToggleAllViews();
		void CloseAllViews();
	}
}