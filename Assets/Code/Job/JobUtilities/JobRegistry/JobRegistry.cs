using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Snowship.NJob
{
	public class JobRegistry
	{
		private readonly List<JobGroup> jobGroups = new();
		private readonly Dictionary<Type, JobDefinition> jobDefinitionTypeToDefinitionMap = new();
		private readonly Dictionary<string, JobDefinition> jobNameToDefinitionMap = new();

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

				JobDefinition jobDefinition = Activator.CreateInstance(jobType, jobGroup, jobSubGroup, jobName, LoadIcon(jobName)) as JobDefinition
					?? throw new InvalidOperationException();

				jobSubGroup.Children.Add(jobDefinition);
				jobDefinitionTypeToDefinitionMap.Add(jobDefinition.GetType(), jobDefinition);
				jobNameToDefinitionMap.Add(jobName, jobDefinition);
			}
		}

		public IJobDefinition GetJobDefinition<TJobDefinition>() where TJobDefinition : IJobDefinition {
			return jobDefinitionTypeToDefinitionMap.GetValueOrDefault(typeof(TJobDefinition));
		}

		public IJobDefinition GetJobDefinition(Type jobDefinitionType) {
			return jobDefinitionTypeToDefinitionMap.GetValueOrDefault(jobDefinitionType);
		}

		public JobDefinition GetJobTypeFromName(string jobName) {
			return jobNameToDefinitionMap[jobName];
		}

		public JobGroup GetJobGroup(string groupToFind) {
			return jobGroups.FirstOrDefault(group => group.Name == groupToFind);
		}

		private Sprite LoadIcon(string jobName) {
			AsyncOperationHandle<Sprite> jobIconOperationHandle = Addressables.LoadAssetAsync<Sprite>($"ui_icon_job_{jobName}");
			jobIconOperationHandle.ReleaseHandleOnCompletion();
			return jobIconOperationHandle.WaitForCompletion();
		}
	}
}