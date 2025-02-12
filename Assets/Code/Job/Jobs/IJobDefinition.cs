using System;
using System.Collections.Generic;
using Snowship.NResource;
using Snowship.NUtilities;
using Snowship.Selectable;

namespace Snowship.NJob
{
	public interface IJobDefinition : IGroupItem
	{
		public Type JobType { get; }
		public Type JobParamsType { get; }

		// Job Definition Properties
		Func<TileManager.Tile, int, bool>[] SelectionConditions { get; }
		List<ResourceAmount> BaseRequiredResources { get; }
		float TimeToWork { get; }
		bool Returnable { get; }
		int Layer { get; }
		SelectionType SelectionType { get; }
		bool HasPreviewObject { get; }

		// Attribute Shortcuts
		IGroupItem Group { get; }
		IGroupItem SubGroup { get; }
	}
}