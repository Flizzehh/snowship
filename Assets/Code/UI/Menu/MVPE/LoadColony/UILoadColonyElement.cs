using System;
using Snowship.NUI.Generic;
using UnityEngine;

namespace Snowship.NUI.Menu.LoadColony {

	public class UILoadColonyElement : UIElement<UILoadColonyElementComponent> {

		private readonly PersistenceManager.PersistenceColony colony;

		public event Action<PersistenceManager.PersistenceColony> OnLoadColonyElementClicked;

		public UILoadColonyElement(PersistenceManager.PersistenceColony colony, Transform parent) : base(parent) {
			this.colony = colony;

			component.OnLoadColonyElementComponentButtonClicked += OnLoadColonyElementComponentButtonClicked;

			component.SetColonyNameText(colony.name);
			component.SetLastSaveDateTimeText(colony.lastSaveDateTime);
			if (colony.lastSaveImage != null) {
				component.SetLastSaveImage(colony.lastSaveImage);
			}
		}

		private void OnLoadColonyElementComponentButtonClicked() {
			OnLoadColonyElementClicked?.Invoke(colony);
		}
	}
}
