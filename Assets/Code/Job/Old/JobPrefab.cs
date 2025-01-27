using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUI;
using UnityEngine;

namespace Snowship.NJob {
	public class JobPrefab {

		public static readonly Dictionary<string, JobPrefab> jobPrefabs = new();

		public JobPrefabGroup group;

		public readonly string name;

		public readonly bool returnable;

		public JobPrefab(
			string name,
			bool returnable
		) {
			this.name = name;

			this.returnable = returnable;
		}

		public static JobPrefab GetJobPrefabByName(string name) {
			return jobPrefabs[name];
		}
	}
}