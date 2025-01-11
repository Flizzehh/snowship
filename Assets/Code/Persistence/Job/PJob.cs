using System;
using System.Collections.Generic;
using System.IO;
using Snowship.NJob;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PJob : PersistenceHandler {

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

		public enum ContainerPickupProperty {
			ContainerPickup,
			Position,
			ResourcesToPickup
		}

		public void SaveJobs(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "jobs.snowship");

			foreach (Job job in Job.jobs) {
				WriteJobLines(file, job, JobProperty.Job, 0);
			}

			file.Close();
		}

		public void WriteJobLines(StreamWriter file, Job job, JobProperty jobType, int startLevel) {
			file.WriteLine(CreateKeyValueString(jobType, string.Empty, startLevel));

			file.WriteLine(CreateKeyValueString(JobProperty.Prefab, job.prefab.name, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.Type, job.objectPrefab.type, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.Variation, job.variation == null ? "null" : job.variation.name, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.Position, FormatVector2ToString(job.tile.obj.transform.position), startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.RotationIndex, job.rotationIndex, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.Priority, job.priority, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.Started, job.started, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.Progress, job.jobProgress, startLevel + 1));
			file.WriteLine(CreateKeyValueString(JobProperty.ColonistBuildTime, job.colonistBuildTime, startLevel + 1));

			if (job.requiredResources != null && job.requiredResources.Count > 0) {
				file.WriteLine(CreateKeyValueString(JobProperty.RequiredResources, string.Empty, startLevel + 1));
				foreach (ResourceManager.ResourceAmount resourceAmount in job.requiredResources) {
					pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}

			if (job.resourcesColonistHas != null && job.resourcesColonistHas.Count > 0) {
				file.WriteLine(CreateKeyValueString(JobProperty.ResourcesColonistHas, string.Empty, startLevel + 1));
				foreach (ResourceManager.ResourceAmount resourceAmount in job.resourcesColonistHas) {
					pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}

			if (job.containerPickups != null && job.containerPickups.Count > 0) {
				file.WriteLine(CreateKeyValueString(JobProperty.ContainerPickups, string.Empty, startLevel + 1));
				foreach (ContainerPickup containerPickup in job.containerPickups) {
					file.WriteLine(CreateKeyValueString(ContainerPickupProperty.ContainerPickup, string.Empty, startLevel + 2));

					file.WriteLine(CreateKeyValueString(ContainerPickupProperty.Position, FormatVector2ToString(containerPickup.container.zeroPointTile.obj.transform.position), startLevel + 3));

					if (containerPickup.resourcesToPickup.Count > 0) {
						file.WriteLine(CreateKeyValueString(ContainerPickupProperty.ResourcesToPickup, string.Empty, startLevel + 3));
						foreach (ResourceManager.ResourceAmount resourceAmount in containerPickup.resourcesToPickup) {
							pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 4);
						}
					}
				}
			}

			if (job.createResource != null) {
				file.WriteLine(CreateKeyValueString(JobProperty.CreateResource, job.createResource.resource.type, startLevel + 1));
			}

			if (job.activeObject != null) {
				file.WriteLine(CreateKeyValueString(JobProperty.ActiveObject, string.Empty, startLevel + 1));

				file.WriteLine(CreateKeyValueString(PObject.ObjectProperty.Position, FormatVector2ToString(job.activeObject.zeroPointTile.obj.transform.position), startLevel + 2));
				file.WriteLine(CreateKeyValueString(PObject.ObjectProperty.Type, job.activeObject.prefab.type, startLevel + 2));
			}

			if (job.transferResources != null && job.transferResources.Count > 0) {
				file.WriteLine(CreateKeyValueString(JobProperty.TransferResources, string.Empty, startLevel + 1));
				foreach (ResourceManager.ResourceAmount resourceAmount in job.transferResources) {
					pInventory.WriteResourceAmountLines(file, resourceAmount, startLevel + 2);
				}
			}
		}

		public class PersistenceJob {
			public string prefab;
			public ResourceManager.ObjectEnum? type;
			public string variation;
			public Vector2? position;
			public int? rotationIndex;
			public int? priority;
			public bool? started;
			public float? progress;
			public float? colonistBuildTime;
			public List<ResourceManager.ResourceAmount> requiredResources;
			public List<ResourceManager.ResourceAmount> resourcesColonistHas;
			public List<PersistenceContainerPickup> containerPickups;
			public ResourceManager.Resource createResource;
			public PObject.PersistenceObject activeObject;
			public List<ResourceManager.ResourceAmount> transferResources;

			public PersistenceJob(
				string prefab,
				ResourceManager.ObjectEnum? type,
				string variation,
				Vector2? position,
				int? rotationIndex,
				int? priority,
				bool? started,
				float? progress,
				float? colonistBuildTime,
				List<ResourceManager.ResourceAmount> requiredResources,
				List<ResourceManager.ResourceAmount> resourcesColonistHas,
				List<PersistenceContainerPickup> containerPickups,
				ResourceManager.Resource createResource,
				PObject.PersistenceObject activeObject,
				List<ResourceManager.ResourceAmount> transferResources
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
			public List<ResourceManager.ResourceAmount> containerPickupResourceAmounts;

			public PersistenceContainerPickup(
				Vector2? containerPickupZeroPointTilePosition,
				List<ResourceManager.ResourceAmount> containerPickupResourceAmounts
			) {
				this.containerPickupZeroPointTilePosition = containerPickupZeroPointTilePosition;
				this.containerPickupResourceAmounts = containerPickupResourceAmounts;
			}
		}

		public List<PersistenceJob> LoadJobs(string path) {
			List<PersistenceJob> persistenceJobs = new List<PersistenceJob>();

			List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((JobProperty)Enum.Parse(typeof(JobProperty), property.Key)) {
					case JobProperty.Job:
						persistenceJobs.Add(LoadPersistenceJob((List<KeyValuePair<string, object>>)property.Value));
						break;
				}
			}

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedJobs;
			return persistenceJobs;
		}

		public PersistenceJob LoadPersistenceJob(List<KeyValuePair<string, object>> properties) {
			string prefab = null;
			ResourceManager.ObjectEnum? type = null;
			string variation = null;
			Vector2? position = null;
			int? rotationIndex = null;
			int? priority = null;
			bool? started = null;
			float? progress = null;
			float? colonistBuildTime = null;
			List<ResourceManager.ResourceAmount> requiredResources = new List<ResourceManager.ResourceAmount>();
			List<ResourceManager.ResourceAmount> resourcesColonistHas = null;
			List<PersistenceContainerPickup> containerPickups = null;
			ResourceManager.Resource createResource = null;
			PObject.PersistenceObject activeObject = null;
			List<ResourceManager.ResourceAmount> transferResources = null;

			foreach (KeyValuePair<string, object> jobProperty in properties) {
				JobProperty jobPropertyKey = (JobProperty)Enum.Parse(typeof(JobProperty), jobProperty.Key);
				switch (jobPropertyKey) {
					case JobProperty.Prefab:
						prefab = StringUtilities.RemoveNonAlphanumericChars((string)jobProperty.Value);
						break;
					case JobProperty.Type:
						type = (ResourceManager.ObjectEnum)Enum.Parse(typeof(ResourceManager.ObjectEnum), (string)jobProperty.Value);
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
						resourcesColonistHas = new List<ResourceManager.ResourceAmount>();
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
									List<ResourceManager.ResourceAmount> containerPickupResourceAmounts = new List<ResourceManager.ResourceAmount>();

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
						createResource = GameManager.resourceM.GetResourceByEnum((ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)jobProperty.Value));
						break;
					case JobProperty.ActiveObject:
						Vector2? activeObjectZeroPointTilePosition = null;
						ResourceManager.ObjectEnum? activeObjectType = null;

						foreach (KeyValuePair<string, object> activeObjectProperty in (List<KeyValuePair<string, object>>)jobProperty.Value) {
							switch ((PObject.ObjectProperty)Enum.Parse(typeof(PObject.ObjectProperty), activeObjectProperty.Key)) {
								case PObject.ObjectProperty.Position:
									activeObjectZeroPointTilePosition = new Vector2(float.Parse(((string)activeObjectProperty.Value).Split(',')[0]), float.Parse(((string)activeObjectProperty.Value).Split(',')[1]));
									break;
								case PObject.ObjectProperty.Type:
									activeObjectType = (ResourceManager.ObjectEnum)Enum.Parse(typeof(ResourceManager.ObjectEnum), (string)activeObjectProperty.Value);
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
						transferResources = new List<ResourceManager.ResourceAmount>();
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
				GameManager.jobM.CreateJob(LoadJob(persistenceJob));
			}
		}

		public Job LoadJob(PersistenceJob persistenceJob) {
			List<ContainerPickup> containerPickups = null;
			if (persistenceJob.containerPickups != null) {
				containerPickups = new List<ContainerPickup>();
				foreach (PersistenceContainerPickup persistenceContainerPickup in persistenceJob.containerPickups) {
					containerPickups.Add(
						new ContainerPickup(
							GameManager.resourceM.containers.Find(c => c.zeroPointTile == GameManager.colonyM.colony.map.GetTileFromPosition(persistenceContainerPickup.containerPickupZeroPointTilePosition.Value)),
							persistenceContainerPickup.containerPickupResourceAmounts
						));
				}
			}

			Job job = new(
				JobPrefab.GetJobPrefabByName(persistenceJob.prefab),
				GameManager.colonyM.colony.map.GetTileFromPosition(persistenceJob.position.Value),
				GameManager.resourceM.GetObjectPrefabByEnum(persistenceJob.type.Value),
				GameManager.resourceM.GetObjectPrefabByEnum(persistenceJob.type.Value).GetVariationFromString(persistenceJob.variation),
				persistenceJob.rotationIndex.Value
			) { started = persistenceJob.started ?? false, jobProgress = persistenceJob.progress ?? 0, colonistBuildTime = persistenceJob.colonistBuildTime ?? 0, requiredResources = persistenceJob.requiredResources, resourcesColonistHas = persistenceJob.resourcesColonistHas, containerPickups = containerPickups, transferResources = persistenceJob.transferResources };
			if (persistenceJob.priority.HasValue) {
				job.ChangePriority(persistenceJob.priority.Value);
			}

			if (persistenceJob.createResource != null) {
				ResourceManager.CraftingObject craftingObject = (ResourceManager.CraftingObject)job.tile.GetAllObjectInstances().Find(o => o.prefab.type == persistenceJob.activeObject.type);
				ResourceManager.CraftableResourceInstance craftableResourceInstance = craftingObject.resources.Find(r => r.resource == persistenceJob.createResource);
				job.SetCreateResourceData(craftableResourceInstance, false);
				craftableResourceInstance.job = job;
			}
			return job;
		}

	}
}
