using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

namespace Snowship.NJob
{
	public class JobRegistry
	{
		private readonly HashSet<Type> jobTypes;
		private readonly Dictionary<string, Type> jobNameToTypeMap = new();
		private readonly Dictionary<Type, RegisterJobAttribute> jobToAttributesMap = new();

		private readonly Dictionary<string, HashSet<Type>> jobGroupToJobs = new();
		private readonly Dictionary<string, HashSet<string>> jobGroupToSubGroups = new();

		private readonly Dictionary<string, HashSet<Type>> jobSubGroupToTypeMap = new();

		public JobRegistry() {
			jobTypes = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.GetCustomAttributes(typeof(RegisterJobAttribute), false).Any())
				.ToHashSet();

			foreach (Type jobType in jobTypes) {
				RegisterJobAttribute attribute = (RegisterJobAttribute)jobType
					.GetCustomAttributes(typeof(RegisterJobAttribute), false)
					.First();

				if (!jobGroupToJobs.TryAdd(attribute.Group, new HashSet<Type> { jobType })) {
					jobGroupToJobs[attribute.Group].Add(jobType);
				}

				if (!jobGroupToSubGroups.TryAdd(attribute.Group, new HashSet<string> { attribute.SubGroup })) {
					jobGroupToSubGroups[attribute.Group].Add(attribute.SubGroup);
				}

				if (!jobSubGroupToTypeMap.TryAdd(attribute.SubGroup, new HashSet<Type> { jobType })) {
					jobSubGroupToTypeMap[attribute.SubGroup].Add(jobType);
				}

				if (!jobNameToTypeMap.TryAdd(attribute.JobName, jobType)) {
					Debug.LogError($"Duplicate Job Name: {attribute.JobName} (Group: {attribute.Group}/{attribute.SubGroup})");
				}
				if (!jobToAttributesMap.TryAdd(jobType, attribute)) {
					Debug.LogError($"Duplicate Job Name: {jobType} (Group: {attribute.Group}/{attribute.SubGroup})");
				}
			}
		}

		public Type GetJobType(string jobName) {
			return jobNameToTypeMap[jobName];
		}

		public RegisterJobAttribute GetJobAttributes(Type jobType) {
			return jobToAttributesMap[jobType];
		}

		public HashSet<Type> GetJobTypes() {
			return jobTypes.ToHashSet();
		}
	}
}