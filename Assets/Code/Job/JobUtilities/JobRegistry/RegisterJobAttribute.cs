using System;

namespace Snowship.NJob
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class RegisterJobAttribute : Attribute
	{
		public string Group { get; }
		public string SubGroup { get; }
		public string JobName { get; }

		public RegisterJobAttribute(string group, string subGroup, string jobName) {
			Group = group;
			SubGroup = subGroup;
			JobName = jobName;
		}
	}
}