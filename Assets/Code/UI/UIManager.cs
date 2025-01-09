using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NUI.Generic;
using Snowship.NUI.Menu.MainMenu;
using UnityEngine;

namespace Snowship.NUI {
	public class UIManager : BaseManager {
		private Transform uiParent;

		private List<IUIGroup> parentGroups = new List<IUIGroup>();

		public void Initialize(Transform uiParent) {
			this.uiParent = uiParent;

			_ = OpenViewAsync<UIMainMenu>();
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

		public void GoBack<TP>(TP presenter) where TP : IUIPresenter {
			IUIGroup group = FindGroup(presenter);
			IUIGroup parent = group.GetParent();
			parent?.SetViewActive(true);
			CloseView(presenter);
		}
	}
}
