using System;
using System.Collections.Generic;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	public class JobTypeData : IGroupItem
	{
		public Type JobType { get; }
		public IGroupItem Group { get; private set; }
		public IGroupItem SubGroup { get; private set; }

		public string Name { get; private set; }
		public Sprite Icon => null; // TODO Set Icon
		public List<IGroupItem> Children => null;

		public JobTypeData(Type jobType) {
			JobType = jobType;
		}

		public void SetAttributeData(RegisterJobAttribute attribute) {
			Name = attribute.JobName;
		}

		public void SetGroups(IGroupItem group, IGroupItem subGroup) {
			Group = group;
			SubGroup = subGroup;
		}
	}
}