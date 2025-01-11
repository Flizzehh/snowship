using System;
using Snowship.NPersistence;
using Snowship.NUI.Generic;
using UnityEngine;

namespace Snowship.NUI.Menu.LoadColony {

	public class UILoadColonyElement : UIElement<UILoadColonyElementComponent> {

		private readonly PersistenceManager.PersistenceColony colony;

		public event Action<PersistenceManager.PersistenceColony> OnLoadColonyElementClicked;

		public UILoadColonyElement(PersistenceManager.PersistenceColony colony, Transform parent) : base(parent) {
			this.colony = colony;

			Component.OnLoadColonyElementComponentButtonClicked += OnLoadColonyElementComponentButtonClicked;

			Component.SetColonyNameText(colony.name);
			Component.SetLastSaveDateTimeText(colony.lastSaveDateTime);
			if (colony.lastSaveImage != null) {
				Component.SetLastSaveImage(colony.lastSaveImage);
			}
		}

		private void OnLoadColonyElementComponentButtonClicked() {
			OnLoadColonyElementClicked?.Invoke(colony);
		}
	}
}
