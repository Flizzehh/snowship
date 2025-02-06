using System;
using System.Collections.Generic;
using Snowship.NHuman;
using Snowship.NResource;
using Snowship.NTime;
using Snowship.NUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Snowship.NJob
{
	public abstract class Job<TJobDefinition> : IJob where TJobDefinition : class, IJobDefinition
	{
		// Job Definition Properties
		public TJobDefinition Definition { get; }
		public int Layer => Definition.Layer;
		public string Name => Definition.Name;
		public IGroupItem Group => Definition.Group;
		public IGroupItem SubGroup => Definition.SubGroup;

		// Job Instance Properties
		public TileManager.Tile Tile { get; private set; }
		public EJobState JobState { get; private set; } = EJobState.Ready;
		public float Progress { get; private set; } = 0;
		public Human Worker { get; private set; }
		public string TargetName { get; protected set; } = string.Empty;
		public string Description { get; protected set; } = string.Empty;
		public List<ResourceAmount> RequiredResources { get; } = new();
		public List<ContainerPickup> ContainerPickups { get; protected set; } = new();
		public bool ShouldBeCancelled { get; protected set; } = false;
		public GameObject JobPreviewObject { get; protected set; }
		public int Priority { get; private set; } = 1;

		// State Shortcuts
		public bool Started => JobState >= EJobState.Started;
		public bool InProgress => JobState >= EJobState.InProgress;
		public bool Finished => JobState >= EJobState.Finished;

		// Events
		public event Action<Job<TJobDefinition>, Human> OnWorkerAssigned;
		public event Action<Job<TJobDefinition>, EJobState> OnJobStateChanged;
		public event Action<Job<TJobDefinition>, int> OnPriorityChanged;

		protected Job(TileManager.Tile tile) {
			Definition = GameManager.Get<JobManager>().JobRegistry.GetJobDefinition<TJobDefinition>() as TJobDefinition
				?? throw new InvalidOperationException();
			Tile = tile;

			RequiredResources.AddRange(Definition.BaseRequiredResources);

			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;

			if (Definition.HasPreviewObject) {
				JobPreviewObject = Object.Instantiate(
					GameManager.Get<ResourceManager>().tilePrefab, // TODO Create separate Class/Prefab for JobPreviewObject
					Tile.obj.transform,
					false
				);
			}
		}

		protected Job() {
		}

		protected void SetTimeToWork(float timeToWork) {
			Progress = Definition.TimeToWork;
		}

		protected void ChangeTile(TileManager.Tile tile) {
			Tile = tile;
			if (JobPreviewObject) {
				JobPreviewObject.transform.SetParent(Tile.obj.transform, false);
			}
		}

		public void ChangePriority(int amount) {
			Priority += amount;
			OnPriorityChanged?.Invoke(this, Priority);
		}

		public void AssignWorker(Human worker) {
			Worker = worker;
			OnWorkerAssigned?.Invoke(this, Worker);
			ChangeJobState(worker == null ? EJobState.Returned : EJobState.Taken);
			if (worker != null) {
				Debug.Log($"Assigned {worker.Name} to job {Name} at {Tile.position}");
			}
		}

		protected virtual void OnJobTaken() {
		}

		protected virtual void OnJobStarted() {
		}

		private void OnTimeChanged(SimulationDateTime time) {
			OnJobInProgress();
		}

		protected virtual void OnJobInProgress() {
			Progress -= 1;
		}

		protected virtual void OnJobFinished() {
			foreach (ResourceAmount resourceAmount in RequiredResources) {
				Worker.Inventory.ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
			}
		}

		protected virtual void OnJobReturned() {
			if (Definition.Returnable) {
				ChangeJobState(EJobState.Ready);
				return;
			}

			if (JobPreviewObject) {
				Object.Destroy(JobPreviewObject);
			}
		}

		public EJobState ChangeJobState(EJobState newState) {
			Action stateChangedMethod = null;
			switch (newState) {
				case EJobState.Ready:
					break;
				case EJobState.Taken:
					if (JobState != EJobState.Ready) {
						Debug.LogError("Must progress from Ready -> Taken.");
						return JobState;
					}
					if (Worker == null) {
						Debug.LogError("Worker not assigned to job.");
						return JobState;
					}
					stateChangedMethod = OnJobTaken;
					break;
				case EJobState.Started:
					if (JobState != EJobState.Taken) {
						Debug.LogError("Must progress from Taken -> Started.");
						return JobState;
					}
					stateChangedMethod = OnJobStarted;
					break;
				case EJobState.InProgress:
					if (JobState != EJobState.Started) {
						Debug.LogError("Must progress from Started -> InProgress.");
						return JobState;
					}
					stateChangedMethod = OnJobInProgress;
					break;
				case EJobState.Finished:
					if (JobState != EJobState.InProgress) {
						Debug.LogError("Must progress from InProgress -> Finished.");
						return JobState;
					}
					stateChangedMethod = OnJobFinished;
					break;
				case EJobState.Returned:
					stateChangedMethod = OnJobReturned;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}

			JobState = newState;
			stateChangedMethod?.Invoke();
			OnJobStateChanged?.Invoke(this, JobState);
			return JobState;
		}

		public void Close() {
			Object.Destroy(JobPreviewObject.gameObject);
		}
	}
}