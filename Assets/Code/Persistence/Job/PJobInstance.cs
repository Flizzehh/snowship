using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NColony;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PJobInstance : JobInstance
	{

		private readonly PInventory pInventory = new PInventory();

		public enum JobProperty {
			Job,
			StoredJob,
			BacklogJob,
			Prefab,
			Type,
			Variation,
			Position,
			RotationIndex,
			Priority,
			Started,
			Progress,
			ColonistBuildTime,
			RequiredResources,
			ResourcesColonistHas,
			ContainerPickups,
			Plant,
			CreateResource,
			ActiveObject,
			TransferResources
		}

		private enum ContainerPickupProperty
		{
			ContainerPickup,
			Position,
			ResourcesToPickup
		}

		public void SaveJobs(string saveDirectoryPath) {

			StreamWriter file = PU.CreateFileAtDirectory(saveDirectoryPath, "jobs.snowship");

			foreach (JobInstance job in jobs) {
				WriteJobLines(file, job, JobProperty.Job, 0);
			}

			file.Close();
		}

		public void WriteJobLines(StreamWriter file, JobInstance jobInstance, JobProperty jobType, int startLevel) {
			file.WriteLine(PU.CreateKeyValueString(jobType, string.Empty, startLevel));

			file.WriteLine(PU.CreateKeyValueString(JobProperty.Prefab, jobInstance.prefab.name, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.Type, jobInstance.objectPrefab.type, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.Variation, jobInstance.variation == null ? "null" : jobInstance.variation.name, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.Position, PU.FormatVector2ToString(jobInstance.tile.obj.transform.position), startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.RotationIndex, jobInstance.rotationIndex, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.Priority, jobInstance.priority, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.Started, jobInstance.started, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.Progress, jobInstance.jobProgress, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(JobProperty.ColonistBuildTime, jobInstance.colonistBuildTime, startLevel + 1));

			if (jobInstance.requiredResources is { Count: > 0 }) {
				file.WriteLine(PU.CreateKeyValueString(JobProperty.RequiredResources, string.Empty, startLevel + 1));
				foreach (ResourceAmount resourceAmount in jobInstance.requiredResources) {
					pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}

			if (jobInstance.resourcesColonistHas is { Count: > 0 }) {
				file.WriteLine(PU.CreateKeyValueString(JobProperty.ResourcesColonistHas, string.Empty, startLevel + 1));
				foreach (ResourceAmount resourceAmount in jobInstance.resourcesColonistHas) {
					pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}

			if (jobInstance.containerPickups is { Count: > 0 }) {
				file.WriteLine(PU.CreateKeyValueString(JobProperty.ContainerPickups, string.Empty, startLevel + 1));
				foreach (ContainerPickup containerPickup in jobInstance.containerPickups) {
					file.WriteLine(PU.CreateKeyValueString(ContainerPickupProperty.ContainerPickup, string.Empty, startLevel + 2));

					file.WriteLine(PU.CreateKeyValueString(ContainerPickupProperty.Position, PU.FormatVector2ToString(containerPickup.container.zeroPointTile.obj.transform.position), startLevel + 3));

					if (containerPickup.resourcesToPickup.Count > 0) {
						file.WriteLine(PU.CreateKeyValueString(ContainerPickupProperty.ResourcesToPickup, string.Empty, startLevel + 3));
						foreach (ResourceAmount resourceAmount in containerPickup.resourcesToPickup) {
							pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 4);
						}
					}
				}
			}

			if (jobInstance.createResource != null) {
				file.WriteLine(PU.CreateKeyValueString(JobProperty.CreateResource, jobInstance.createResource.resource.type, startLevel + 1));
			}

			if (jobInstance.activeObject != null) {
				file.WriteLine(PU.CreateKeyValueString(JobProperty.ActiveObject, string.Empty, startLevel + 1));

				file.WriteLine(PU.CreateKeyValueString(PObject.ObjectProperty.Position, PU.FormatVector2ToString(jobInstance.activeObject.zeroPointTile.obj.transform.position), startLevel + 2));
				file.WriteLine(PU.CreateKeyValueString(PObject.ObjectProperty.Type, jobInstance.activeObject.prefab.type, startLevel + 2));
			}

			if (jobInstance.transferResources is { Count: > 0 }) {
				file.WriteLine(PU.CreateKeyValueString(JobProperty.TransferResources, string.Empty, startLevel + 1));
				foreach (ResourceAmount resourceAmount in jobInstance.transferResources) {
					pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}
		}

		public class PersistenceJob {
			public string prefab;
			public ObjectPrefab.ObjectEnum? type;
			public string variation;
			public Vector2? position;
			public int? rotationIndex;
			public int? priority;
			public bool? started;
			public float? progress;
			public float? colonistBuildTime;
			public List<ResourceAmount> requiredResources;
			public List<ResourceAmount> resourcesColonistHas;
			public List<PersistenceContainerPickup> containerPickups;
			public Resource createResource;
			public PObject.PersistenceObject activeObject;
			public List<ResourceAmount> transferResources;

			public PersistenceJob(
				string prefab,
				ObjectPrefab.ObjectEnum? type,
				string variation,
				Vector2? position,
				int? rotationIndex,
				int? priority,
				bool? started,
				float? progress,
				float? colonistBuildTime,
				List<ResourceAmount> requiredResources,
				List<ResourceAmount> resourcesColonistHas,
				List<PersistenceContainerPickup> containerPickups,
				Resource createResource,
				PObject.PersistenceObject activeObject,
				List<ResourceAmount> transferResources
			) {
				this.prefab = prefab;
				this.type = type;
				this.variation = variation;
				this.position = position;
				this.rotationIndex = rotationIndex;
				this.priority = priority;
				this.started = started;
				this.progress = progress;
				this.colonistBuildTime = colonistBuildTime;
				this.requiredResources = requiredResources;
				this.resourcesColonistHas = resourcesColonistHas;
				this.containerPickups = containerPickups;
				this.createResource = createResource;
				this.activeObject = activeObject;
				this.transferResources = transferResources;
			}
		}

		public class PersistenceContainerPickup {
			public Vector2? containerPickupZeroPointTilePosition;
			public List<ResourceAmount> containerPickupResourceAmounts;

			public PersistenceContainerPickup(
				Vector2? containerPickupZeroPointTilePosition,
				List<ResourceAmount> containerPickupResourceAmounts
			) {
				this.containerPickupZeroPointTilePosition = containerPickupZeroPointTilePosition;
				this.containerPickupResourceAmounts = containerPickupResourceAmounts;
			}
		}

		public List<PersistenceJob> LoadJobs(string path) {
			List<PersistenceJob> persistenceJobs = new List<PersistenceJob>();

			List<KeyValuePair<string, object>> properties = PU.GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((JobProperty)Enum.Parse(typeof(JobProperty), property.Key)) {
					case JobProperty.Job:
						persistenceJobs.Add(LoadPersistenceJob((List<KeyValuePair<string, object>>)property.Value));
						break;
				}
			}

			GameManager.Get<PersistenceManager>().loadingState = PersistenceManager.LoadingState.LoadedJobs;
			return persistenceJobs;
		}

		public PersistenceJob LoadPersistenceJob(List<KeyValuePair<string, object>> properties) {
			string prefab = null;
			ObjectPrefab.ObjectEnum? type = null;
			string variation = null;
			Vector2? position = null;
			int? rotationIndex = null;
			int? priority = null;
			bool? started = null;
			float? progress = null;
			float? colonistBuildTime = null;
			List<ResourceAmount> requiredResources = new();
			List<ResourceAmount> resourcesColonistHas = null;
			List<PersistenceContainerPickup> containerPickups = null;
			Resource createResource = null;
			PObject.PersistenceObject activeObject = null;
			List<ResourceAmount> transferResources = null;

			foreach (KeyValuePair<string, object> jobProperty in properties) {
				JobProperty jobPropertyKey = (JobProperty)Enum.Parse(typeof(JobProperty), jobProperty.Key);
				switch (jobPropertyKey) {
					case JobProperty.Prefab:
						prefab = StringUtilities.RemoveNonAlphanumericChars((string)jobProperty.Value);
						break;
					case JobProperty.Type:
						type = (ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), (string)jobProperty.Value);
						break;
					case JobProperty.Variation:
						variation = StringUtilities.RemoveNonAlphanumericChars((string)jobProperty.Value);
						break;
					case JobProperty.Position:
						position = new Vector2(float.Parse(((string)jobProperty.Value).Split(',')[0]), float.Parse(((string)jobProperty.Value).Split(',')[1]));
						break;
					case JobProperty.RotationIndex:
						rotationIndex = int.Parse((string)jobProperty.Value);
						break;
					case JobProperty.Priority:
						priority = int.Parse((string)jobProperty.Value);
						break;
					case JobProperty.Started:
						started = bool.Parse((string)jobProperty.Value);
						break;
					case JobProperty.Progress:
						progress = float.Parse((string)jobProperty.Value);
						break;
					case JobProperty.ColonistBuildTime:
						colonistBuildTime = float.Parse((string)jobProperty.Value);
						break;
					case JobProperty.RequiredResources:
						foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
							requiredResources.Add(pInventory.LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
						}
						break;
					case JobProperty.ResourcesColonistHas:
						resourcesColonistHas = new List<ResourceAmount>();
						foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
							resourcesColonistHas.Add(pInventory.LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
						}
						if (resourcesColonistHas.Count <= 0) {
							resourcesColonistHas = null;
						}
						break;
					case JobProperty.ContainerPickups:
						containerPickups = new List<PersistenceContainerPickup>();
						foreach (KeyValuePair<string, object> containerPickupProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
							switch ((ContainerPickupProperty)Enum.Parse(typeof(ContainerPickupProperty), containerPickupProperty.Key)) {
								case ContainerPickupProperty.ContainerPickup:

									Vector2? containerPickupZeroPointTilePosition = null;
									List<ResourceAmount> containerPickupResourceAmounts = new();

									foreach (KeyValuePair<string, object> containerPickupSubProperty in (List<KeyValuePair<string, object>>)containerPickupProperty.Value) {
										switch ((ContainerPickupProperty)Enum.Parse(typeof(ContainerPickupProperty), containerPickupSubProperty.Key)) {
											case ContainerPickupProperty.Position:
												containerPickupZeroPointTilePosition = new Vector2(float.Parse(((string)containerPickupSubProperty.Value).Split(',')[0]), float.Parse(((string)containerPickupSubProperty.Value).Split(',')[1]));
												break;
											case ContainerPickupProperty.ResourcesToPickup:
												foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)containerPickupSubProperty.Value) {
													containerPickupResourceAmounts.Add(pInventory.LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
												}
												break;
											default:
												Debug.LogError("Unknown container pickup sub property: " + containerPickupSubProperty.Key + " " + containerPickupSubProperty.Value);
												break;
										}
									}

									containerPickups.Add(new PersistenceContainerPickup(containerPickupZeroPointTilePosition, containerPickupResourceAmounts));
									break;
								default:
									Debug.LogError("Unknown container pickup property: " + containerPickupProperty.Key + " " + containerPickupProperty.Value);
									break;
							}
						}
						if (containerPickups.Count <= 0) {
							containerPickups = null;
						}
						break;
					case JobProperty.CreateResource:
						createResource = Resource.GetResourceByEnum((EResource)Enum.Parse(typeof(EResource), (string)jobProperty.Value));
						break;
					case JobProperty.ActiveObject:
						Vector2? activeObjectZeroPointTilePosition = null;
						ObjectPrefab.ObjectEnum? activeObjectType = null;

						foreach (KeyValuePair<string, object> activeObjectProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
							switch ((PObject.ObjectProperty)Enum.Parse(typeof(PObject.ObjectProperty), activeObjectProperty.Key)) {
								case PObject.ObjectProperty.Position:
									activeObjectZeroPointTilePosition = new Vector2(float.Parse(((string)activeObjectProperty.Value).Split(',')[0]), float.Parse(((string)activeObjectProperty.Value).Split(',')[1]));
									break;
								case PObject.ObjectProperty.Type:
									activeObjectType = (ObjectPrefab.ObjectEnum)Enum.Parse(typeof(ObjectPrefab.ObjectEnum), (string)activeObjectProperty.Value);
									break;
								default:
									Debug.LogError("Unknown active tile object property: " + activeObjectProperty.Key + " " + activeObjectProperty.Value);
									break;
							}
						}

						activeObject = new PObject.PersistenceObject(
							activeObjectType,
							null,
							activeObjectZeroPointTilePosition,
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							null,
							null
						);
						break;
					case JobProperty.TransferResources:
						transferResources = new List<ResourceAmount>();
						foreach (KeyValuePair<string, object> resourceAmountProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
							transferResources.Add(pInventory.LoadResourceAmount((List<KeyValuePair<string, object>>)resourceAmountProperty.Value));
						}
						if (transferResources.Count <= 0) {
							transferResources = null;
						}
						break;
					default:
						Debug.LogError("Unknown job property: " + jobProperty.Key + " " + jobProperty.Value);
						break;
				}
			}

			return new PersistenceJob(
				prefab,
				type,
				variation,
				position,
				rotationIndex,
				priority,
				started,
				progress,
				colonistBuildTime,
				requiredResources,
				resourcesColonistHas,
				containerPickups,
				createResource,
				activeObject,
				transferResources
			);
		}

		public void ApplyLoadedJobs(List<PersistenceJob> persistenceJobs) {
			foreach (PersistenceJob persistenceJob in persistenceJobs) {
				GameManager.Get<JobManager>().CreateJob(LoadJob(persistenceJob));
			}
		}

		public JobInstance LoadJob(PersistenceJob persistenceJob) {
			List<ContainerPickup> containerPickups = null;
			if (persistenceJob.containerPickups != null) {
				containerPickups = new List<ContainerPickup>();
				foreach (PersistenceContainerPickup persistenceContainerPickup in persistenceJob.containerPickups) {
					containerPickups.Add(
						new ContainerPickup(
							Container.containers.Find(c => c.zeroPointTile == GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(persistenceContainerPickup.containerPickupZeroPointTilePosition.Value)),
							persistenceContainerPickup.containerPickupResourceAmounts
						)
					);
				}
			}

			JobInstance jobInstance = new(
				JobPrefab.GetJobPrefabByName(persistenceJob.prefab),
				GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(persistenceJob.position.Value),
				ObjectPrefab.GetObjectPrefabByEnum(persistenceJob.type.Value),
				ObjectPrefab.GetObjectPrefabByEnum(persistenceJob.type.Value).GetVariationFromString(persistenceJob.variation),
				persistenceJob.rotationIndex.Value
			) { started = persistenceJob.started ?? false, jobProgress = persistenceJob.progress ?? 0, colonistBuildTime = persistenceJob.colonistBuildTime ?? 0, requiredResources = persistenceJob.requiredResources, resourcesColonistHas = persistenceJob.resourcesColonistHas, containerPickups = containerPickups, transferResources = persistenceJob.transferResources };
			if (persistenceJob.priority.HasValue) {
				jobInstance.ChangePriority(persistenceJob.priority.Value);
			}

			if (persistenceJob.createResource != null) {
				CraftingObject craftingObject = (CraftingObject)jobInstance.tile.GetAllObjectInstances().Find(o => o.prefab.type == persistenceJob.activeObject.type);
				CraftableResourceInstance craftableResourceInstance = craftingObject.resources.Find(r => r.resource == persistenceJob.createResource);
				jobInstance.SetCreateResourceData(craftableResourceInstance, false);
				craftableResourceInstance.JobInstance = jobInstance;
			}
			return jobInstance;
		}

	}
}