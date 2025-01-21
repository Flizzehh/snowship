using Snowship.NColonist;
using UnityEngine;

namespace Snowship.NUI.Simulation
{
	public class UIMoodElement : UIElement<UIMoodElementComponent>
	{
		public MoodModifierInstance Mood { get; }

		public UIMoodElement(Transform parent, MoodModifierInstance mood) : base(parent) {
			Mood = mood;

			mood.OnTimerChanged += OnMoodTimerChanged;

			Component.SetMoodNameText(mood.prefab.name);
			Component.SetMoodEffectAmountText(mood.prefab.effectAmount);
			OnMoodTimerChanged(Mood.timer);
		}

		protected override void OnClose() {
			base.OnClose();
			Mood.OnTimerChanged -= OnMoodTimerChanged;
		}

		private void OnMoodTimerChanged(int moodTimer) {
			Component.SetMoodTimerText(moodTimer, Mood.prefab.effectLengthSeconds, Mood.prefab.infinite);
		}
	}
}
