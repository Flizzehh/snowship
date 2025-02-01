using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUtilities
{
	public interface IGroupItem
	{
		string Name { get; }
		Sprite Icon { get; }

		List<IGroupItem> Children { get; }
	}
}