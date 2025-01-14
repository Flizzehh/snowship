using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUI.Generic {
	public class UIGroup<TConfig, TView, TPresenter> : IUIGroup
		where TConfig : IUIConfig
		where TView : IUIView
		where TPresenter : IUIPresenter {

		private readonly TConfig config;
		private readonly TView view;
		private readonly TPresenter presenter;

		private readonly IUIGroup parent;
		private readonly List<IUIGroup> children = new List<IUIGroup>();

		public IUIView View => view;
		public IUIPresenter Presenter => presenter;
		public IUIConfig Config => config;
		public IUIGroup Parent => parent;

		public bool IsActive => view.IsActive;

		public UIGroup(TConfig config, TView view, TPresenter presenter, IUIGroup parent) {
			this.config = config;
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

		public void SetViewActive(bool active) {
			view.SetActive(active);
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

			config.OnClose();
			presenter.OnClose();
			view.OnClose();

			Object.Destroy(view.Instance);

			parent?.RemoveChild(this);
		}

		public IUIGroup FindGroup(IUIPresenter presenterToFind) {

			if (presenterToFind == null) {
				return null;
			}

			if (presenter.Equals(presenterToFind)) {
				return this;
			}

			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup(presenterToFind);
				if (foundGroup.Presenter.Equals(presenterToFind)) {
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

			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup(viewToFind);
				if (foundGroup.View.Equals(viewToFind)) {
					return foundGroup;
				}
			}

			return null;
		}
		public IUIGroup FindGroup<TConfigToFind>() where TConfigToFind : IUIConfig {

			if (Config.GetType() == typeof(TConfigToFind)) {
				return this;
			}

			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup<TConfigToFind>();
				if (foundGroup.Config.GetType() == typeof(TConfigToFind)) {
					return foundGroup;
				}
			}

			return null;
		}

	}
}
