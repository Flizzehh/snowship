using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI {
	public class UIManager : IUIManager
	{
		private Transform uiParent;

		private readonly List<IUIGroup> parentGroups = new List<IUIGroup>();

		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		public void Initialize(Transform uiParent) {
			this.uiParent = uiParent;
		}

		public async UniTask<IUIPresenter> OpenViewAsync<TUIConfig>() where TUIConfig : IUIConfig, new() {
			return await OpenViewAsync<TUIConfig>(null, null, true);
		}

		public async UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(IUIPresenter parent) where TUIConfig : IUIConfig, new() {
			return await OpenViewAsync<TUIConfig>(parent, null, true);
		}

		public async UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(IUIPresenter parent, bool showParent) where TUIConfig : IUIConfig, new() {
			return await OpenViewAsync<TUIConfig>(parent, null, showParent);
		}

		public async UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(IUIPresenter parent, IUIParameters parameters) where TUIConfig : IUIConfig, new() {
			return await OpenViewAsync<TUIConfig>(parent, parameters, true);
		}

		public async UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(
			IUIPresenter parent,
			IUIParameters parameters,
			bool showParent
		) where TUIConfig : IUIConfig, new() {

			IUIGroup parentGroup = FindGroup(parent);

			TUIConfig config = new();
			Transform parentTransform = parentGroup?.View.Instance.transform ?? uiParent;
			(IUIView view, IUIPresenter presenter) ui;
			if (parameters == null) {
				ui = await config.Open(parentTransform);
			} else {
				ui = await ((IUIConfigParameters)config).Open(parentTransform, parameters);
			}

			UIGroup<IUIConfig, IUIView, IUIPresenter> group = new(
				config,
				ui.view,
				ui.presenter,
				parentGroup
			);
			group.OnCreate();

			parentGroup?.SetViewActive(showParent);

			if (parent == null) {
				parentGroups.Add(group);
			}

			group.OnPostCreate();

			return ui.presenter;
		}

		public async UniTask<IUIPresenter> SwitchViewsAsync<TUIConfigToClose, TUIConfigToOpen>(IUIParameters parameters, bool showParent = true)
			where TUIConfigToClose : IUIConfig
			where TUIConfigToOpen : IUIConfig, new()
		{
			IUIGroup closeGroup = FindGroup<TUIConfigToClose>();
			if (closeGroup == null) {
				return null;
			}

			IUIPresenter parentPresenter = closeGroup.Parent.Presenter;
			IUIPresenter presenter = await OpenViewAsync<TUIConfigToOpen>(parentPresenter, parameters, showParent);
			closeGroup.Close();
			return presenter;
		}

		public async UniTask<IUIPresenter> SwitchViewsOrOpenAsync<TUIConfigToClose, TUIConfigToOpen>(
			IUIPresenter parent = null,
			IUIParameters parameters = null,
			bool showParent = true
		)
			where TUIConfigToClose : IUIConfig
			where TUIConfigToOpen : IUIConfig, new() {

			IUIPresenter presenter = await SwitchViewsAsync<TUIConfigToClose, TUIConfigToOpen>(parameters, showParent);
			if (presenter == null) {
				return await GameManager.uiM.OpenViewAsync<TUIConfigToOpen>(parent, parameters, showParent);
			}
			return null;
		}

		public async UniTask<IUIPresenter> ReopenView<TUIConfigToReopen>(
			IUIPresenter parent = null,
			IUIParameters parameters = null,
			bool showParent = true
		)
			where TUIConfigToReopen : IUIConfig, new() {

			return await SwitchViewsOrOpenAsync<TUIConfigToReopen, TUIConfigToReopen>(parent, parameters, showParent);

		}

		private IUIGroup FindGroup(IUIPresenter presenter) {
			if (presenter == null) {
				return null;
			}

			foreach (IUIGroup group in parentGroups) {
				IUIGroup foundGroup = group.FindGroup(presenter);
				if (foundGroup == null) {
					continue;
				}
				if (foundGroup.Presenter.Equals(presenter)) {
					return foundGroup;
				}
			}

			return null;
		}

		private IUIGroup FindGroup<TConfigToFind>() where TConfigToFind : IUIConfig {
			foreach (IUIGroup group in parentGroups) {
				IUIGroup foundGroup = group.FindGroup<TConfigToFind>();
				if (foundGroup == null) {
					continue;
				}
				if (foundGroup.Config.GetType() == typeof(TConfigToFind)) {
					return foundGroup;
				}
			}

			return null;
		}

		public void CloseView<TP>(TP presenter) where TP : IUIPresenter {
			IUIGroup group = FindGroup(presenter);
			if (group.Parent == null) {
				parentGroups.Remove(group);
			}
			group.Close();
		}

		public void CloseView<TC>() where TC : IUIConfig {
			IUIGroup group = FindGroup<TC>();
			if (group == null) {
				return;
			}
			if (group.Parent == null) {
				parentGroups.Remove(group);
			}
			group.Close();
		}

		public void GoBack<TP>(TP presenter) where TP : IUIPresenter {
			IUIGroup group = FindGroup(presenter);
			IUIGroup parent = group.Parent;
			parent?.SetViewActive(true);
			CloseView(presenter);
		}

		public void ToggleAllViews() {
			foreach (IUIGroup group in parentGroups) {
				group.SetViewActive(!group.IsActive);
			}
		}

		public void CloseAllViews() {
			foreach (IUIGroup group in parentGroups) {
				group.Close();
			}
			parentGroups.Clear();
		}
	}

	public interface IUIManager : IManager
	{
		void Initialize(Transform uiParent);
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
			IUIParameters parameters
		)
			where TUIConfig : IUIConfig, new();

		UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(
			IUIPresenter parent,
			IUIParameters parameters,
			bool showParent
		) where TUIConfig : IUIConfig, new();

		UniTask<IUIPresenter> SwitchViewsAsync<TUIConfigToClose, TUIConfigToOpen>(
			IUIParameters parameters,
			bool showParent = true
		)
			where TUIConfigToClose : IUIConfig
			where TUIConfigToOpen : IUIConfig, new();

		UniTask<IUIPresenter> SwitchViewsOrOpenAsync<TUIConfigToClose, TUIConfigToOpen>(
			IUIPresenter parent = null,
			IUIParameters parameters = null,
			bool showParent = true
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
