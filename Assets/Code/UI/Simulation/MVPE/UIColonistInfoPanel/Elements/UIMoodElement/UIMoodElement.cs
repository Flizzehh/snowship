using Snowship.NColonist;

namespace Snowship.NUI
{
	public class UIMoodElement : UIElement<UIMoodElementComponent>
	{
		public MoodModifierInstance Mood { get; }

		public UIMoodElement(MoodModifierInstance mood) {
			Mood = mood;
		}

		protected override void OnCreate() {
			base.OnCreate();

			Mood.OnTimerChanged += OnMoodTimerChanged;

			Component.SetMoodNameText(Mood.Prefab.name);
			Component.SetMoodEffectAmountText(Mood.Prefab.effectAmount);
			OnMoodTimerChanged(Mood.Timer);
		}

		protected override void OnClose() {
			base.OnClose();
			Mood.OnTimerChanged -= OnMoodTimerChanged;
		}

		private void OnMoodTimerChanged(int moodTimer) {
			Component.SetMoodTimerText(moodTimer, Mood.Prefab.effectLengthSeconds, Mood.Prefab.infinite);
		}
	}
}