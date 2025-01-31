using Snowship.NColonist;
using UnityEngine;

namespace Snowship.NUI
{
	public class UINeedElement : UIElement<UINeedElementComponent>
	{
		private readonly NeedInstance need;

		public UINeedElement(Transform parent, NeedInstance need) : base(parent) {
			this.need = need;

			need.OnValueChanged += OnNeedValueChanged;

			Component.SetNeedNameText(need.prefab.name);
			OnNeedValueChanged(need.GetValue(), need.GetRoundedValue());
		}

		protected override void OnClose() {
			base.OnClose();
			need.OnValueChanged -= OnNeedValueChanged;
		}

		private void OnNeedValueChanged(float needValue, int needValueRounded) {
			Component.SetNeedValueText(needValueRounded);
			Component.SetNeedSlider(needValue, need.prefab.clampValue);
		}
	}
}