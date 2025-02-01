using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Snowship.NJob
{
	public class JobRegistry
	{
		private readonly List<JobGroup> jobGroups = new();
		private readonly Dictionary<Type, JobTypeData> jobTypeToDataMap = new();
		private readonly Dictionary<string, JobTypeData> jobNameToDataMap = new();

		public JobRegistry() {
			List<Type> jobTypes = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.GetCustomAttributes(typeof(RegisterJobAttribute), false).Any())
				.ToList();

			foreach (Type jobType in jobTypes) {

				JobTypeData jobTypeData = new(jobType);
				jobTypeToDataMap.Add(jobType, jobTypeData);

				RegisterJobAttribute attribute = (RegisterJobAttribute)jobType
					.GetCustomAttributes(typeof(RegisterJobAttribute), false)
					.First();
				jobTypeData.SetAttributeData(attribute);

				JobGroup jobGroup = jobGroups.Find(g => g.Name == attribute.Group);
				if (jobGroup == null) {
					jobGroup = new JobGroup(attribute.Group, null);
					jobGroups.Add(jobGroup);
				}

				if (jobGroup.Children.Find(g => g.Name == attribute.SubGroup) is not JobGroup jobSubGroup) {
					jobSubGroup = new JobGroup(attribute.SubGroup, null);
					jobGroup.Children.Add(jobSubGroup);
				}

				jobSubGroup.Children.Add(jobTypeData);

				jobTypeData.SetGroups(jobGroup, jobSubGroup);
				jobNameToDataMap.Add(jobTypeData.Name, jobTypeData);
			}
		}

		public JobTypeData GetJobTypeData(Type jobType) {
			return jobTypeToDataMap[jobType];
		}

		public JobTypeData GetJobTypeFromName(string jobName) {
			return jobNameToDataMap[jobName];
		}

		public JobGroup GetJobGroup(string jobGroup) {
			return jobGroups.FirstOrDefault(jg => jg.Name == jobGroup);
		}
	}
}