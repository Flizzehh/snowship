using Snowship.Job;
using System.Collections;
using UnityEngine;
using static ColonistManager;

namespace Snowship.Profession {

	public class Profession {
		public readonly ProfessionPrefab prefab;
		public readonly Colonist colonist;

		private int priority;

		public Profession(
			ProfessionPrefab prefab,
			Colonist colonist,
			int priority
		) {
			this.prefab = prefab;
			this.colonist = colonist;
			this.priority = priority;
		}

		public int GetPriority() {
			return priority;
		}

		public void SetPriority(int priority) {
			if (priority > ProfessionPrefab.maxPriority) {
				priority = 0;
			}

			if (priority < 0) {
				priority = ProfessionPrefab.maxPriority;
			}

			this.priority = priority;

			ColonistJob.UpdateSingleColonistJobs(colonist);
		}

		public void IncreasePriority() {
			SetPriority(priority + 1);
		}

		public void DecreasePriority() {
			SetPriority(priority - 1);
		}
	}
}