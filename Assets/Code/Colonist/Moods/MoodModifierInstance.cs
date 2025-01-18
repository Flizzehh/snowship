namespace Snowship.NColonist {
	public class MoodModifierInstance {
		public Colonist colonist;
		public MoodModifierPrefab prefab;

		public float timer = 0;

		public MoodModifierInstance(Colonist colonist, MoodModifierPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;

			timer = prefab.effectLengthSeconds;
		}

		public void Update() {
			timer -= 1 * GameManager.timeM.Time.DeltaTime;
		}
	}
}