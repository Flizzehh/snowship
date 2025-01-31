using Snowship.NColonist;
using UnityEngine;

namespace Snowship.NUI
{
	public class UIMoodElement : UIElement<UIMoodElementComponent>
	{
		public MoodModifierInstance Mood { get; }

		public UIMoodElement(Transform parent, MoodModifierInstance mood) : base(parent) {
			Mood = mood;

			mood.OnTimerChanged += OnMoodTimerChanged;

			Component.SetMoodNameText(mood.Prefab.name);
			Component.SetMoodEffectAmountText(mood.Prefab.effectAmount);
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