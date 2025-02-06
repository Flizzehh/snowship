using System;
using System.Collections.Generic;
using Snowship.NResource;
using Snowship.NUtilities;
using Snowship.Selectable;
using UnityEngine;

namespace Snowship.NJob
{
	public abstract class JobDefinition<TJobParams> : JobDefinition where TJobParams : IJobParams, new()
	{
		public Type JobParams { get; }

		protected JobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
			JobParams = typeof(TJobParams);
		}
	}

	public abstract class JobDefinition : IJobDefinition
	{
		// Job Definition Properties
		public virtual Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; }
		public virtual List<ResourceAmount> BaseRequiredResources { get; } = new();
		public float TimeToWork { get; protected set; } = 3;
		public bool Returnable { get; protected set; } = true;
		public int Layer { get; protected set; } = 0;
		public SelectionType SelectionType { get; protected set; } = SelectionType.Full;
		public bool HasPreviewObject { get; } = true;

		// Attribute Shortcuts
		public IGroupItem Group { get; }
		public IGroupItem SubGroup { get; }

		// IGroupItem
		public string Name { get; }
		public Sprite Icon { get; }

		protected JobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) {
			Group = group;
			SubGroup = subGroup;
			Name = name;
			Icon = icon;
		}
	}
}