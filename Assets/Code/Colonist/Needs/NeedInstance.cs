using UnityEngine;

namespace Snowship.NColonist {
	public class NeedInstance {

		public Colonist colonist;
		public NeedPrefab prefab;
		private float value = 0;
		private int roundedValue = 0;

		public NeedInstance(Colonist colonist, NeedPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}

		public float GetValue() {
			return value;
		}

		public void SetValue(float newValue) {
			float oldValue = value;
			value = newValue;
			value = Mathf.Clamp(value, 0, prefab.clampValue);
			roundedValue = Mathf.RoundToInt(value / prefab.clampValue * 100);
			if (GameManager.humanM.selectedHuman == colonist && Mathf.RoundToInt(oldValue / prefab.clampValue * 100) != roundedValue) {
				// GameManager.uiMOld.RemakeSelectedColonistNeeds(); // TODO Need Value Updated
			}
		}

		public void ChangeValue(float amount) {
			SetValue(value + amount);
		}

		public int GetRoundedValue() {
			return roundedValue;
		}

	}
}
