using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Snowship.NUI.Generic;
using UnityEngine;

namespace Snowship.NUI {
	public class UIManager : IManager {

		private Transform uiParent;

		private readonly List<IUIGroup> parentGroups = new List<IUIGroup>();

		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		public void Initialize(Transform uiParent) {
			this.uiParent = uiParent;
		}

		public async UniTask<IUIPresenter> OpenViewAsync<TUIConfig>(IUIPresenter parent = null, bool showParent = true) where TUIConfig : IUIConfig, new() {

			IUIGroup parentGroup = FindGroup(parent);

			IUIConfig config = new TUIConfig();
			(IUIView view, IUIPresenter presenter) ui = await config.Open(parentGroup?.View.Instance.transform ?? uiParent);

			UIGroup<IUIConfig, IUIView, IUIPresenter> group = new UIGroup<IUIConfig, IUIView, IUIPresenter>(
				config,
				ui.view,
				ui.presenter,
				parentGroup
			);

			parentGroup?.SetViewActive(showParent);

			if (parent == null) {
				parentGroups.Add(group);
			}

			return ui.presenter;
		}

		public async UniTask<IUIPresenter> SwitchViewsAsync<TUIPresenterToClose, TUIConfigToOpen>(TUIPresenterToClose presenter, bool showParent = true)
			where TUIPresenterToClose : IUIPresenter
			where TUIConfigToOpen : IUIConfig, new()
		{
			IUIGroup closeGroup = FindGroup(presenter);
			if (closeGroup == null) {
				return null;
			}

			IUIPresenter parentPresenter = closeGroup.Parent.Presenter;
			closeGroup.Close();
			return await OpenViewAsync<TUIConfigToOpen>(parentPresenter, showParent);
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
}
