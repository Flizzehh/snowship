using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Snowship.NJob
{
	public class JobRegistry
	{
		private readonly Dictionary<string, Type> jobRegistry = new();

		public JobRegistry() {
			IEnumerable<Type> jobTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.GetCustomAttributes(typeof(RegisterJobAttribute), false).Any());

			foreach (Type jobType in jobTypes) {
				RegisterJobAttribute attribute = (RegisterJobAttribute)jobType
					.GetCustomAttributes(typeof(RegisterJobAttribute), false)
					.First();

				string key = $"{attribute.Group}:{attribute.JobName}";
				if (jobRegistry.TryAdd(key, jobType)) {
					Console.WriteLine($"Registered Job: {key} -> {jobType.Name}");
				}
			}
		}

		public Type GetJobType(string group, string jobName) {
			string key = $"{group}:{jobName}";
			return jobRegistry.GetValueOrDefault(key);
		}
	}
}