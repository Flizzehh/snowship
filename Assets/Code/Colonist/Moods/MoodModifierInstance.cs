using System;
using Snowship.NTime;

namespace Snowship.NColonist {
	public class MoodModifierInstance : IDisposable
	{
		public readonly MoodModifierPrefab Prefab;

		public int Timer = 0;

		public event Action<int> OnTimerChanged;
		public event Action<MoodModifierInstance> OnTimerAtZero;

		public MoodModifierInstance(MoodModifierPrefab prefab) {
			Prefab = prefab;

			Timer = prefab.effectLengthSeconds;

			if (!prefab.infinite) {
				GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
			}
		}

		private void OnTimeChanged(SimulationDateTime time) {
			Timer -= 1;
			OnTimerChanged?.Invoke(Timer);
			if (Timer <= 0) {
				OnTimerAtZero?.Invoke(this);
			}
		}

		public void Dispose() {
			if (!Prefab.infinite) {
				GameManager.Get<TimeManager>().OnTimeChanged -= OnTimeChanged;
			}
		}
	}
}