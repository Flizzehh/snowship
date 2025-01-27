using System;

namespace Snowship.NJob
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class RegisterJobAttribute : Attribute
	{
		public string Group { get; }
		public string JobName { get; }

		public RegisterJobAttribute(string group, string jobName) {
			Group = group;
			JobName = jobName;
		}
	}
}