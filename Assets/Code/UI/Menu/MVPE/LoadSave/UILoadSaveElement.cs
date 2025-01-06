using System;
using Snowship.NUI.Generic;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI.Menu.LoadSave {
	internal class UILoadSaveElement : UIElement<UILoadSaveElementComponent> {

		private readonly PersistenceManager.PersistenceSave save;

		public event Action<PersistenceManager.PersistenceSave> OnLoadSaveElementClicked;

		public UILoadSaveElement(PersistenceManager.PersistenceSave save, Transform parent) : base(parent) {
			this.save = save;

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
