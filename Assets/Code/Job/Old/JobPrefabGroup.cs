using System.Collections.Generic;

namespace Snowship.NJob
{
	public class JobPrefabGroup {

		public static readonly Dictionary<string, JobPrefabGroup> jobPrefabGroups = new();

		public readonly string name;

		public readonly List<JobPrefab> jobPrefabs = new();

		public JobPrefabGroup(string name, List<JobPrefab> jobPrefabs) {
			this.name = name;
			this.jobPrefabs = jobPrefabs;

			foreach (JobPrefab jobPrefab in jobPrefabs) {
				jobPrefab.group = this;
			}
		}
	}
}