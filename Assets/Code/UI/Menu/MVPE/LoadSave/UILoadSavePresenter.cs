using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NPersistence;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UILoadSavePresenter : UIPresenter<UILoadSaveView> {

		private PSave.PersistenceSave selectedSave;
		private readonly PSave pSave = new PSave();

		public UILoadSavePresenter(UILoadSaveView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnBackButtonClicked += OnBackButtonClicked;
			View.OnLoadSaveButtonClicked += OnLoadSaveButtonClicked;

			CreateSaveElements();
		}

		public override void OnClose() {
			View.OnBackButtonClicked -= OnBackButtonClicked;
			View.OnLoadSaveButtonClicked -= OnLoadSaveButtonClicked;
		}

		private void CreateSaveElements() {
			foreach (PSave.PersistenceSave save in pSave.GetPersistenceSaves()) {
				UILoadSaveElement loadSaveElement = new(save);
				loadSaveElement.Open(View.GetSaveElementsParentTransform()).Forget();
				loadSaveElement.OnLoadSaveElementClicked += OnSaveElementClicked;
			}
		}

		private void OnBackButtonClicked() {
			GameManager.Get<UIManager>().CloseView(this);
		}

		private async void OnLoadSaveButtonClicked() {
			await GameManager.Get<PersistenceManager>().ApplyLoadedSave(selectedSave);
		}

		private void OnSaveElementClicked(PSave.PersistenceSave save) {
			selectedSave = save;

			bool saveValid = selectedSave != null;
			string loadSaveButtonText = saveValid ? $"Load Save ({selectedSave.saveDateTime})" : "Select a Save to Load";
			View.SetLoadSaveButtonInteractable(saveValid, loadSaveButtonText);
		}
	}
}