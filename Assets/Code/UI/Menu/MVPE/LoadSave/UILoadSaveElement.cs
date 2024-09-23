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
				component.OnLoadSaveElementComponentButtonClicked += OnLoadSaveElementComponentButtonClicked;
				component.SetSaveDateTimeText(save.saveDateTime);
				if (save.image != null) {
					component.SetSaveImage(save.image);
				}
			} else {
				component.SetSaveText("Error while reading save.");
				component.SetSaveDateTimeText(string.Empty);
				component.SetSaveElementBackgroundColour(ColourUtilities.GetColour(ColourUtilities.Colours.LightRed));
				component.SetElementButtonInteractable(false);
			}
		}

		private void OnLoadSaveElementComponentButtonClicked() {
			OnLoadSaveElementClicked?.Invoke(save);
		}
	}
}
