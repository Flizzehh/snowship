using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;

namespace Snowship.NJob
{
	public class JobManager : IManager
	{
		public JobRegistry JobRegistry { get; private set; }
		public HashSet<IJob> Jobs { get; } = new();
		private readonly Dictionary<Type, HashSet<IJob>> jobsByType = new();
		private readonly Dictionary<TileManager.Tile, HashSet<IJob>> jobsByTile = new();

		public event Action<IJob> OnJobAdded;
		public event Action<IJob> OnJobRemoved;

		public void OnCreate() {
			JobRegistry = new JobRegistry();
		}

		public void AddJob(IJob job) {
			Jobs.Add(job);

			if (!jobsByType.TryAdd(job.GetType(), new HashSet<IJob> { job })) {
				jobsByType[job.GetType()].Add(job);
			}

			if (!jobsByTile.TryAdd(job.Tile, new HashSet<IJob> { job })) {
				jobsByTile[job.Tile].Add(job);
			}

			OnJobAdded?.Invoke(job);
		}

		public void TakeJob(IJob job, Human human) {
			job.AssignWorker(human);
		}

		public void RemoveJob(IJob job) {
			Jobs.Remove(job);

			jobsByType[job.GetType()].Remove(job);
			if (jobsByType[job.GetType()].Count == 0) {
				jobsByType.Remove(job.GetType());
			}

			jobsByTile[job.Tile].Remove(job);
			if (jobsByTile[job.Tile].Count == 0) {
				jobsByTile.Remove(job.Tile);
			}

			OnJobRemoved?.Invoke(job);
		}

		public void ReturnJob(IJob job) {
			job?.AssignWorker(null);
		}

		public IJob JobOfTypeExistsAtTile<TJob>(TileManager.Tile tile) where TJob : IJob {
			return jobsByTile.GetValueOrDefault(tile)?.FirstOrDefault(job => job.GetType() == typeof(TJob));
		}

		public IJob JobAtLayerExistsAtTile(TileManager.Tile tile, int layer) {
			return jobsByTile.GetValueOrDefault(tile)?.FirstOrDefault(j => j.Layer == layer);
		}

		public HashSet<IJob> JobsAtTile(TileManager.Tile tile) {
			return jobsByTile.GetValueOrDefault(tile);
		}
	}
}