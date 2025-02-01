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
	public abstract class Job : IJob, IGroupItem
	{
		// Job Instance Properties
		public TileManager.Tile Tile { get; private set; }
		public EJobState JobState { get; private set; } = EJobState.Ready;
		public float TimeToWork { get; protected set; } = 0;
		public float Progress { get; private set; } = 0;
		public Human Worker { get; private set; }
		public string TargetName { get; protected set; } = string.Empty;
		public string Description { get; protected set; } = string.Empty;
		public List<ResourceAmount> RequiredResources { get; } = new();
		public List<ContainerPickup> ContainerPickups { get; protected set; } = new();
		public bool Returnable { get; protected set; } = true;
		public bool ShouldBeCancelled { get; protected set; } = false;
		public GameObject JobPreviewObject { get; protected set; }
		public int Priority { get; private set; } = 1;
		public int Layer { get; protected set; } = 0;

		// State Shortcuts
		public bool Started => JobState >= EJobState.Started;
		public bool InProgress => JobState >= EJobState.InProgress;
		public bool Finished => JobState >= EJobState.Finished;

		// Attribute Shortcuts
		public JobGroup Group => GameManager.Get<JobManager>().JobRegistry.GetJobTypeData(GetType()).Group as JobGroup;
		public JobGroup SubGroup => GameManager.Get<JobManager>().JobRegistry.GetJobTypeData(GetType()).SubGroup as JobGroup;

		// IGroupItem
		public string Name => GameManager.Get<JobManager>().JobRegistry.GetJobTypeData(GetType()).Name;
		public Sprite Icon { get; } // TODO Load image from a registry of job images?
		public List<IGroupItem> Children => null;

		// Events
		public event Action<Job, Human> OnWorkerAssigned;
		public event Action<Job, EJobState> OnJobStateChanged;
		public event Action<Job, int> OnPriorityChanged;

		// Methods
		protected Job(TileManager.Tile tile) {
			Tile = tile;
			GameManager.Get<TimeManager>().OnTimeChanged += OnTimeChanged;
		}

		protected Job() {
		}

		protected void SetTimeToWork(float timeToWork) {
			TimeToWork = timeToWork;
			Progress = TimeToWork;
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
			if (Returnable) {
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