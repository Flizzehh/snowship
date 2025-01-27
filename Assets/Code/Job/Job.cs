using System;
using System.Collections.Generic;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	public class Job : IJob
	{
		public JobPrefab JobPrefab { get; }
		public TileManager.Tile Tile { get; }
		public EJobState JobState { get; private set; }
		public float Progress { get; }
		public HumanManager.Human Worker { get; private set; }
		public string TargetName { get; protected set; }
		public string Description { get; protected set; }
		public List<ResourceAmount> RequiredResources { get; protected set; }
		public bool Returnable = true;
		public bool ShouldBeCancelled { get; protected set; }

		protected Job(JobPrefab jobPrefab, TileManager.Tile tile) {
			JobPrefab = jobPrefab;
			Tile = tile;
			// TODO Set Progress Value?

			JobState = EJobState.Ready;

			TargetName = jobPrefab.name;
			Description = jobPrefab.name;
		}

		public void AssignWorker(HumanManager.Human worker) {
			Worker = worker;
		}

		public virtual void OnJobTaken() {
		}

		public virtual void OnJobStarted() {
		}

		public virtual void OnJobInProgress() {
		}

		public virtual void OnJobFinished() {
			foreach (ResourceAmount resourceAmount in RequiredResources) {
				Worker.GetInventory().ChangeResourceAmount(resourceAmount.Resource, -resourceAmount.Amount, false);
			}
		}

		public virtual void OnJobReturned() {
		}

		public EJobState ChangeJobState(EJobState newState) {
			Action stateChangedMethod = null;
			switch (newState) {
				case EJobState.Ready:
					if (JobState != EJobState.Returned) {
						Debug.LogError("Must progress from Returned -> Ready (or set at init).");
						return JobState;
					}
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
					if (JobState == EJobState.Ready) {
						Debug.LogError("Job shouldn't go from Ready -> Returned.");
						return JobState;
					}
					stateChangedMethod = OnJobReturned;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}

			JobState = newState;
			stateChangedMethod?.Invoke();
			return JobState;
		}
	}
}