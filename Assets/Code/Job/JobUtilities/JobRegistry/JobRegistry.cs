using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Snowship.NJob
{
	public class JobRegistry
	{
		private readonly List<JobGroup> jobGroups = new();
		private readonly Dictionary<Type, object> jobDefinitionTypeToDefinitionMap = new();
		private readonly Dictionary<string, object> jobNameToDefinitionMap = new();

		public JobRegistry() {
			List<Type> jobTypes = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.GetCustomAttributes(typeof(RegisterJobAttribute), false).Any())
				.ToList();

			foreach (Type jobType in jobTypes) {

				RegisterJobAttribute attribute = (RegisterJobAttribute)jobType
					.GetCustomAttributes(typeof(RegisterJobAttribute), false)
					.First();

				JobGroup jobGroup = jobGroups.Find(g => g.Name == attribute.Group);
				if (jobGroup == null) {
					jobGroup = new JobGroup(attribute.Group, null);
					jobGroups.Add(jobGroup);
				}

				if (jobGroup.Children.Find(g => g.Name == attribute.SubGroup) is not JobGroup jobSubGroup) {
					jobSubGroup = new JobGroup(attribute.SubGroup, null);
					jobGroup.Children.Add(jobSubGroup);
				}

				string jobName = attribute.JobName;

				CreateJobDefinition(jobType, jobGroup, jobSubGroup, jobName);
			}
		}

		private void CreateJobDefinition(Type jobType, JobGroup jobGroup, JobGroup jobSubGroup, string jobName) {
			IJobDefinition jobDefinition = Activator.CreateInstance(jobType, jobGroup, jobSubGroup, jobName) as IJobDefinition
				?? throw new InvalidOperationException();

			jobSubGroup.Children.Add(jobDefinition);
			jobDefinitionTypeToDefinitionMap.Add(jobDefinition.GetType(), jobDefinition);
			jobNameToDefinitionMap.Add(jobName, jobDefinition);

			jobDefinition.SetIcon(LoadIcon(jobName)).Forget();
		}

		public IJobDefinition GetJobDefinition<TJob, TJobDefinition>() where TJobDefinition : class, IJobDefinition {
			return jobDefinitionTypeToDefinitionMap.GetValueOrDefault(typeof(TJobDefinition)) as IJobDefinition;
		}

		public IJobDefinition GetJobDefinition(Type jobDefinitionType) {
			return jobDefinitionTypeToDefinitionMap.GetValueOrDefault(jobDefinitionType) as IJobDefinition;
		}

		public IJobDefinition GetJobTypeFromName(string jobName) {
			return jobNameToDefinitionMap[jobName] as IJobDefinition;
		}

		public JobGroup GetJobGroup(string groupToFind) {
			return jobGroups.FirstOrDefault(group => group.Name == groupToFind);
		}

		private async UniTask<Sprite> LoadIcon(string jobName) {
			AsyncOperationHandle<Sprite> jobIconOperationHandle = Addressables.LoadAssetAsync<Sprite>($"ui_icon_job_{jobName}");
			jobIconOperationHandle.ReleaseHandleOnCompletion();
			return await jobIconOperationHandle;
		}
	}
}