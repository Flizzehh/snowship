using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUtilities
{
	public interface IGroupItem
	{
		string Name { get; }
		Sprite Icon { get; set; }

		virtual async UniTaskVoid SetIcon(UniTask<Sprite> iconTask) {
			Icon = await iconTask;
		}

		virtual List<IGroupItem> Children => null;
	}
}