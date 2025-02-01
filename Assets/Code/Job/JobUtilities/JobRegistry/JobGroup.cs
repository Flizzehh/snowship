using System.Collections.Generic;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	public class JobGroup : IGroupItem
	{
		public string Name { get; }
		public Sprite Icon { get; }
		public List<IGroupItem> Children { get; } = new();

		public JobGroup(string name, Sprite icon) {
			Name = name;
			Icon = icon;
		}
	}
}