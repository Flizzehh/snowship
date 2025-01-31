using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowship.NJob
{
	public class JobManager : IManager
	{
		public JobRegistry JobRegistry { get; } = new();
		public HashSet<Job> Jobs { get; } = new();
		public Dictionary<Type, HashSet<Job>> JobsByType { get; } = new();

		public event Action<Job> OnJobAdded;
		public event Action<Job> OnJobRemoved;

		public void OnCreate() {
		}

		public void OnGameSetupComplete() {
		}

		public void OnClose() {
		}

		public void AddJob(Job job) {
			Jobs.Add(job);
			OnJobAdded?.Invoke(job);
		}

		public void RemoveJob(Job job) {
			Jobs.Remove(job);
			OnJobRemoved?.Invoke(job);
		}

		public void ReturnJob(Job job) {
			if (job == null) {
				return;
			}

			job.AssignWorker(null);

			if (Jobs.Contains(job)) {
				return;
			}

			AddJob(job);
		}

		public Job JobOfTypeExistsAtTile<TJob>(TileManager.Tile tile) where TJob : IJob {
			JobsByType.TryGetValue(typeof(TJob), out HashSet<Job> jobs);
			return jobs?.First(j => j.Tile == tile);
		}
	}
}