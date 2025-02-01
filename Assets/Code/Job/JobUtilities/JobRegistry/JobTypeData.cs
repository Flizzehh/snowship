using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Snowship.NJob
{
	public class JobTypeData : IGroupItem
	{
		public Type JobType { get; }
		public IGroupItem Group { get; private set; }
		public IGroupItem SubGroup { get; private set; }

		public string Name { get; private set; }
		public Sprite Icon { get; private set; }
		public List<IGroupItem> Children => null;

		public JobTypeData(Type jobType) {
			JobType = jobType;
		}

		public void SetAttributeData(RegisterJobAttribute attribute) {
			Name = attribute.JobName;
			LoadIcon().Forget();
		}

		public void SetGroups(IGroupItem group, IGroupItem subGroup) {
			Group = group;
			SubGroup = subGroup;
		}

		private async UniTaskVoid LoadIcon() {
			AsyncOperationHandle<Sprite> jobIconOperationHandle = Addressables.LoadAssetAsync<Sprite>($"ui_icon_job_{Name}");
			jobIconOperationHandle.ReleaseHandleOnCompletion();
			Icon = await jobIconOperationHandle;
		}
	}
}