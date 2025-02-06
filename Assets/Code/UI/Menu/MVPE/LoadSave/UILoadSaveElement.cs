using System;
using Snowship.NPersistence;
using Snowship.NUtilities;

namespace Snowship.NUI
{
	internal class UILoadSaveElement : UIElement<UILoadSaveElementComponent> {

		private readonly PSave.PersistenceSave save;

		public event Action<PSave.PersistenceSave> OnLoadSaveElementClicked;

		public UILoadSaveElement(PSave.PersistenceSave save) {
			this.save = save;
		}

		protected override void OnCreate() {
			base.OnCreate();

			if (save.loadable) {
				Component.OnLoadSaveElementComponentButtonClicked += OnLoadSaveElementComponentButtonClicked;
				Component.SetSaveDateTimeText(save.saveDateTime);
				if (save.image != null) {
					Component.SetSaveImage(save.image);
				}
			} else {
				Component.SetSaveText("Error while reading save.");
				Component.SetSaveDateTimeText(string.Empty);
				Component.SetSaveElementBackgroundColour(ColourUtilities.GetColour(ColourUtilities.EColour.LightRed));
				Component.SetElementButtonInteractable(false);
			}
		}

		private void OnLoadSaveElementComponentButtonClicked() {
			OnLoadSaveElementClicked?.Invoke(save);
		}
	}
}