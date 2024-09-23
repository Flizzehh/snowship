using System.Collections.Generic;
using Snowship.NUI.Generic;
using Snowship.NUI.Menu.MainMenu;
using UnityEngine;

namespace Snowship.NUI {
	public class UIManager : BaseManager {
		private Transform uiParent;

		private List<IUIGroup> parentGroups = new List<IUIGroup>();

		public void Initialize(Transform uiParent) {
			this.uiParent = uiParent;

			OpenViewAsync<UIMainMenu>(null);
		}

		public async void OpenViewAsync<TUIConfig>(IUIPresenter parent) where TUIConfig : IUIConfig, new() {

			IUIConfig config = new TUIConfig();
			(IUIView view, IUIPresenter presenter) ui = await config.Open(uiParent);

			UIGroup<IUIView, IUIPresenter> group = new UIGroup<IUIView, IUIPresenter>(
				ui.view,
				ui.presenter,
				FindGroup(parent)
			);

			if (parent == null) {
				parentGroups.Add(group);
			}
		}

		private IUIGroup FindGroup(IUIPresenter presenter) {
			if (presenter == null) {
				return null;
			}

			foreach (IUIGroup group in parentGroups) {
				IUIGroup foundGroup = group.FindGroup(presenter);
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
	}
}
