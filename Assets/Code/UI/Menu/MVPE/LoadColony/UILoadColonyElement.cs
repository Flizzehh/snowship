using System;
using Snowship.NPersistence;

namespace Snowship.NUI
{

	public class UILoadColonyElement : UIElement<UILoadColonyElementComponent> {

		private readonly PersistenceColony colony;

		public event Action<PersistenceColony> OnLoadColonyElementClicked;

		public UILoadColonyElement(PersistenceColony colony) {
			this.colony = colony;
		}

		protected override void OnCreate() {
			base.OnCreate();

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