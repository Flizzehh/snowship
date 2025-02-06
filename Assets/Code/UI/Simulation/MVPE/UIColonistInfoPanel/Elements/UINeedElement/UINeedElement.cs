using Snowship.NColonist;

namespace Snowship.NUI
{
	public class UINeedElement : UIElement<UINeedElementComponent>
	{
		private readonly NeedInstance need;

		public UINeedElement(NeedInstance need) {
			this.need = need;
		}

		protected override void OnCreate() {
			base.OnCreate();

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