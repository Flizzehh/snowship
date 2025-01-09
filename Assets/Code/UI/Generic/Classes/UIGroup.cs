using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUI.Generic {
	public class UIGroup<TView, TPresenter> : IUIGroup
		where TView : IUIView
		where TPresenter : IUIPresenter
	{
		private readonly TView view;
		private readonly TPresenter presenter;

		private readonly IUIGroup parent;
		private readonly List<IUIGroup> children = new List<IUIGroup>();

		public UIGroup() {
			view = default(TView);
			presenter = default(TPresenter);
			parent = null;
		}

		public UIGroup(TView view, TPresenter presenter, IUIGroup parent) {
			this.view = view;
			this.presenter = presenter;

			this.parent = parent;

			OnConstructed();
		}

		private void OnConstructed() {
			parent?.AddChild(this);

			view.OnOpen();
			presenter.OnCreate();
		}

		public IUIView GetView() {
			return view;
		}

		public void SetViewActive(bool active) {
			view.SetActive(active);
		}

		public IUIPresenter GetPresenter() {
			return presenter;
		}

		public IUIGroup GetParent() {
			return parent;
		}

		public void AddChild(IUIGroup child) {
			children.Add(child);
		}

		public void RemoveChild(IUIGroup child) {
			children.Remove(child);
		}

		public void Close() {
			foreach (IUIGroup child in children) {
				child.Close();
				break;
			}

			view.OnClose();

			Object.Destroy(view.Instance);

			parent.RemoveChild(this);
		}

		public IUIGroup FindGroup(IUIPresenter presenterToFind) {

			if (presenterToFind == null) {
				return null;
			}

			if (presenter.Equals(presenterToFind)) {
				return this;
			}

			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup(presenterToFind);
				if (foundGroup.GetPresenter().Equals(presenterToFind)) {
					return foundGroup;
				}
			}

			return null;
		}

		public IUIGroup FindGroup(IUIView viewToFind) {

			if (viewToFind == null) {
				return null;
			}

			if (view.Equals(viewToFind)) {
				return this;
			}

			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup(viewToFind);
				if (foundGroup.GetView().Equals(viewToFind)) {
					return foundGroup;
				}
			}

			return null;
		}
	}
}
