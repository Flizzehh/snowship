using System.Collections.Generic;
using Snowship.NResource;

namespace Snowship.NJob
{
	public interface IJob
	{
		JobPrefab JobPrefab { get; }
		TileManager.Tile Tile { get; }

		public EJobState JobState { get; }
		float Progress { get; }
		HumanManager.Human Worker { get; }
		string Description { get; }
		List<ResourceAmount> RequiredResources { get; }

		void AssignWorker(HumanManager.Human worker);

		void OnJobTaken();
		void OnJobStarted();
		void OnJobInProgress();
		void OnJobFinished();
		void OnJobReturned();

		EJobState ChangeJobState(EJobState newState);
	}
}