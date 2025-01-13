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

			IUIConfig config = new TUIConfig();
			(IUIView view, IUIPresenter presenter) ui = await config.Open(uiParent);

			IUIGroup parentGroup = FindGroup(parent);

			UIGroup<IUIView, IUIPresenter> group = new UIGroup<IUIView, IUIPresenter>(
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

			IUIPresenter parentPresenter = closeGroup.GetParent().GetPresenter();
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
				if (foundGroup.GetPresenter().Equals(presenter)) {
					return foundGroup;
				}
			}

			return null;
		}

		public void CloseView<TP>(TP presenter) where TP : IUIPresenter {
			IUIGroup group = FindGroup(presenter);
			if (group.GetParent() == null) {
				parentGroups.Remove(group);
			}
			group.Close();
		}

		public void GoBack<TP>(TP presenter) where TP : IUIPresenter {
			IUIGroup group = FindGroup(presenter);
			IUIGroup parent = group.GetParent();
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
