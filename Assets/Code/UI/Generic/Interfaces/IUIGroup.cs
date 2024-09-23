namespace Snowship.NUI.Generic {
	public interface IUIGroup {
		public IUIView GetView();
		public IUIPresenter GetPresenter();
		public IUIGroup GetParent();
		public void AddChild(IUIGroup child);
		public void RemoveChild(IUIGroup child);
		public void Close();
		public IUIGroup FindGroup(IUIPresenter presenterToFind);
		public IUIGroup FindGroup(IUIView viewToFind);
	}
}
