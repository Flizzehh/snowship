namespace Snowship.NUI
{
	public abstract class UIPresenter<TView> : IUIPresenter
		where TView : UIView
	{
		protected TView View { get; }

		protected UIPresenter(TView view) {
			View = view;
		}

		public virtual void OnCreate() {
		}

		public virtual void OnPostCreate() {
		}

		public virtual void OnClose() {
			View.OnClose();
		}

		public void Close() => OnClose();
	}

	public abstract class UIPresenter<TView, TParameters> : UIPresenter<TView>
		where TView : UIView
		where TParameters : IUIParameters
	{
		protected TParameters Parameters { get; }

		protected UIPresenter(TView view, TParameters parameters) : base(view) {
			Parameters = parameters;
		}
	}
}
