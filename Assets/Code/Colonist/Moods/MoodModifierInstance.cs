using System;
using Snowship.NTime;

namespace Snowship.NColonist {
	public class MoodModifierInstance : IDisposable
	{
		public Colonist colonist;
		public MoodModifierPrefab prefab;

		public int timer = 0;

		public event Action<int> OnTimerChanged;
		public event Action<MoodModifierInstance> OnTimerAtZero;

		public MoodModifierInstance(Colonist colonist, MoodModifierPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;

			timer = prefab.effectLengthSeconds;

			GameManager.timeM.OnTimeChanged += OnTimeChanged;
		}

		private void OnTimeChanged(SimulationDateTime time) {
			timer -= 1;
			OnTimerChanged?.Invoke(timer);
			if (timer <= 0) {
				OnTimerAtZero?.Invoke(this);
			}
		}

		public void Dispose() {
			GameManager.timeM.OnTimeChanged -= OnTimeChanged;
		}
	}
}
