using System.Collections.Generic;
using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;

namespace Snowship.NJob
{
	public class ColonistJob
	{
		public static readonly Dictionary<Colonist, List<ColonistJob>> colonistJobs = new();

		public Colonist colonist;
		public JobInstance JobInstance;

		public List<ResourceAmount> resourcesColonistHas;
		public List<ContainerPickup> containerPickups;

		public float cost;

		public ColonistJob(Colonist colonist, JobInstance jobInstance, List<ResourceAmount> resourcesColonistHas, List<ContainerPickup> containerPickups) {
			this.colonist = colonist;
			JobInstance = jobInstance;
			this.resourcesColonistHas = resourcesColonistHas;
			this.containerPickups = containerPickups;

			CalculateCost();
		}

		public void CalculateCost() {
			cost = JobManager.CalculateJobCost(colonist, JobInstance, containerPickups);
		}

		public void RecalculatePickupResources() {
			KeyValuePair<bool, List<List<ResourceAmount>>> returnKVP = GameManager.Get<JobManager>().CalculateColonistResourcesToPickup(colonist, JobInstance.requiredResources);
			List<ResourceAmount> resourcesToPickup = returnKVP.Value[0];
			resourcesColonistHas = returnKVP.Value[1];
			if (resourcesToPickup != null) { // If there are resources the colonist doesn't have
				containerPickups = GameManager.Get<JobManager>().CalculateColonistPickupContainers(colonist, resourcesToPickup);
			} else {
				containerPickups = null;
			}
		}

		public static void UpdateColonistJobCosts(Colonist colonist) {
			if (colonistJobs.ContainsKey(colonist)) {
				foreach (ColonistJob colonistJob in colonistJobs[colonist]) {
					colonistJob.CalculateCost();
				}
			}
		}

		public static void UpdateAllColonistJobCosts() {
			foreach (Colonist colonist in Colonist.colonists) {
				UpdateColonistJobCosts(colonist);
			}
		}

		public static void UpdateSingleColonistJobs(Colonist colonist) {

			List<JobInstance> sortedJobs = JobManager.GetSortedJobs(colonist);

			List<ColonistJob> validJobs = new();

			foreach (JobInstance job in sortedJobs) {

				if (job.requiredResources.Count > 0) {

					KeyValuePair<bool, List<List<ResourceAmount>>> returnKVP = GameManager.Get<JobManager>().CalculateColonistResourcesToPickup(colonist, job.requiredResources);
					bool colonistHasAllResources = returnKVP.Key;
					List<ResourceAmount> resourcesToPickup = returnKVP.Value[0];
					List<ResourceAmount> resourcesColonistHas = returnKVP.Value[1];

					if (resourcesToPickup != null) { // If there are resources the colonist doesn't have

						List<ContainerPickup> containerPickups = GameManager.Get<JobManager>().CalculateColonistPickupContainers(colonist, resourcesToPickup);

						if (containerPickups != null) { // If all resources were found in containers
							validJobs.Add(new ColonistJob(colonist, job, resourcesColonistHas, containerPickups));
						} else {
							continue;
						}
					} else if (colonistHasAllResources) { // If the colonist has all resources
						validJobs.Add(new ColonistJob(colonist, job, resourcesColonistHas, null));
					} else {
						continue;
					}
				} else {
					validJobs.Add(new ColonistJob(colonist, job, null, null));
				}
			}
			if (validJobs.Count > 0) {

				//validJobs = validJobs.OrderByDescending(job => job.job.priority).ThenBy(job => job.cost).ToList();

				if (colonistJobs.ContainsKey(colonist)) {
					colonistJobs[colonist] = validJobs;
				} else {
					colonistJobs.Add(colonist, validJobs);
				}
			}
		}

		public static void UpdateColonistJobs() {
			colonistJobs.Clear();
			List<Colonist> availableColonists = Colonist.colonists.Where(colonist => colonist.JobInstance == null && colonist.overTile.walkable).ToList();
			foreach (Colonist colonist in availableColonists) {
				UpdateSingleColonistJobs(colonist);
			}
		}

		public static int GetColonistJobsCountForColonist(Colonist colonist) {
			if (colonistJobs.ContainsKey(colonist)) {
				return colonistJobs[colonist].Count;
			}
			return 0;
		}
	}
}